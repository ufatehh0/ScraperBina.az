using BinaScraperApp.Parsers;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BinaScraperApp.Scraper
{
    public class BinaScraper
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _detailPage;

        // Microsoft Edge brauzerinin quraşdırıldığı yolu tap
        private string GetEdgePath()
        {
            string[] possiblePaths =
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
            };

            foreach (var path in possiblePaths)
                if (File.Exists(path))
                    return path;

            throw new FileNotFoundException("Microsoft Edge not found.");
        }

        // Konteksti təzələmə metodu
        private async Task RefreshDetailContextAsync()
        {
            if (_detailPage != null) await _detailPage.CloseAsync();
            if (_context != null) await _context.CloseAsync();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                IgnoreHTTPSErrors = true
            });

            _detailPage = await _context.NewPageAsync();
            await BlockResources(_detailPage);
        }

        /// Brauzer başlatma
        public async Task InitAsync(bool browserVisible)
        {
            _playwright = await Playwright.CreateAsync();

            _browser = await _playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    ExecutablePath = GetEdgePath(),
                    Headless = !browserVisible,
                    Args = new[]
                    {
                        "--disable-dev-shm-usage",
                        "--no-sandbox",
                        "--blink-settings=imagesEnabled=false",
                        "--disable-gpu",
                        "--disable-notifications",
                        "--disable-extensions"
                    }
                });
        }

        // Lazımsız resursları bloklama
        private async Task BlockResources(IPage page)
        {
            await page.RouteAsync("**/*", async route =>
            {
                var r = route.Request;
                var type = r.ResourceType;

                if (type == "image" || type == "font" || type == "stylesheet" || type == "media" ||
                    r.Url.Contains("google-analytics") || r.Url.Contains("facebook") ||
                    r.Url.Contains("ads") || r.Url.Contains("doubleclick") ||
                    r.Url.Contains("yandex"))
                {
                    await route.AbortAsync();
                }
                else
                {
                    await route.ContinueAsync();
                }
            });
        }

        public async Task CloseAsync()
        {
            if (_context != null) await _context.CloseAsync();
            if (_browser != null) await _browser.CloseAsync();
            if (_playwright != null) _playwright.Dispose();
        }

        // Elanları yığma metodu
        public async Task<List<BinaItem>> GetItemsLazyAsync(
            string url,
            int maxCount,
            bool browserVisible,
            Action<int, int>? onProgress = null,
            Action<BinaItem>? onItemParsed = null
        )
        {
            var itemsToScrape = new List<BinaItem>();
            var parser = new ItemParser();

            await InitAsync(browserVisible);

            try
            {
                onProgress?.Invoke(-1, maxCount);

                
                var listContext = await _browser.NewContextAsync();
                var listPage = await listContext.NewPageAsync();
                await BlockResources(listPage);

                await listPage.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });
                await listPage.WaitForSelectorAsync("[data-cy='item-card']");

                int lastCount = 0;
                int sameRepeat = 0;
                

                while (itemsToScrape.Count < maxCount && sameRepeat < 10)
                {
                    var cards = listPage.Locator("[data-cy='item-card']");
                    int currentCount = await cards.CountAsync();

                    if (currentCount > lastCount)
                    {
                        for (int i = lastCount; i < currentCount && itemsToScrape.Count < maxCount; i++)
                        {
                            var item = await parser.ParseAsync(cards.Nth(i));
                            itemsToScrape.Add(item);
                            onProgress?.Invoke(-1, itemsToScrape.Count);
                        }
                        lastCount = currentCount;
                        sameRepeat = 0;
                    }
                    else
                    {
                        sameRepeat++;
                    }

                   
                    await listPage.EvaluateAsync(@"
                        window.scrollTo(0, document.body.scrollHeight);
                        window.scrollBy(0, -500);
                        window.scrollTo(0, document.body.scrollHeight);
                    ");

                    await Task.Delay(100);
                }

                await listPage.CloseAsync();
                await listContext.CloseAsync();

                // Yığılmış elanların detalları üzrə məlumatları çıxarmağa başla
                onProgress?.Invoke(-2, maxCount);
                await RefreshDetailContextAsync();

                var finalItems = new List<BinaItem>();

                for (int i = 0; i < itemsToScrape.Count; i++)
                {
                    
                    if (i > 0 && i % 5 == 0)
                    {
                        await RefreshDetailContextAsync();
                    }

                    var item = itemsToScrape[i];
                    item.No = i + 1;

                    try
                    {
                        await _detailPage.GotoAsync(item.Link, new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.DOMContentLoaded,
                            Timeout = 20000
                        });

                        await DetailParser.ParseAsync(_detailPage, item);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading page: {item.Link}. {ex.Message}");
                    }

                    finalItems.Add(item);
                    onItemParsed?.Invoke(item);
                    onProgress?.Invoke(finalItems.Count, itemsToScrape.Count);
                }

                return finalItems;
            }
            finally
            {
                await CloseAsync();
            }
        }
    }
}