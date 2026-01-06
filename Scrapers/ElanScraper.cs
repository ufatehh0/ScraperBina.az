using Microsoft.Playwright;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BinaScraperApp.Scraper
{
    public class ElanScraper
    {
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

      
        public async Task<int> GetListingCountAsync(string url, bool browserVisible)
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    ExecutablePath = GetEdgePath(),
                    Headless = !browserVisible,
                    Args = new[]
                    {
                        "--disable-gpu",
                        "--disable-software-rasterizer",
                        "--disable-dev-shm-usage",
                        "--disable-background-networking",
                        "--disable-background-timer-throttling",
                        "--disable-backgrounding-occluded-windows",
                        "--disable-renderer-backgrounding",
                        "--disable-extensions",
                        "--disable-sync",
                        "--no-sandbox",
                        "--disable-setuid-sandbox"
                    }
                });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true,
                RecordVideoDir = null,
                RecordVideoSize = null,
            });

            var page = await context.NewPageAsync();

            // Lazımsız resursları bloklama
            await page.RouteAsync("**/*", async route =>
            {
                var r = route.Request;
                var t = r.ResourceType;

                if (t == "image" ||
                    t == "media" ||
                    t == "font" ||
                    t == "stylesheet" ||
                    r.Url.Contains("ads") ||
                    r.Url.Contains("analytics"))
                {
                    await route.AbortAsync();
                }
                else
                {
                    await route.ContinueAsync();
                }
            });

            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 15000
            });

            // Elanların tapılmaması halını yoxla
            const string notFoundText = "Təəssüf ki, axtarışınız əsasında heç nə tapılmadı";

            var content = await page.ContentAsync();
            if (content.Contains(notFoundText))
                return 0;

            // Elan sayını çıxar 
            try
            {
                var titleLocator = page.Locator("[data-cy='search-page-regular-items-title'] span").First;

               
                await titleLocator.WaitForAsync(new LocatorWaitForOptions
                {
                    Timeout = 5000,
                    State = WaitForSelectorState.Visible
                });

                var raw = (await titleLocator.InnerTextAsync()).Trim();
                var digits = new string(raw.Where(char.IsDigit).ToArray());

                if (int.TryParse(digits, out int count))
                    return count;
            }
            catch (TimeoutException)
            {
                // Element tapılmadı və ya yüklənmədi
                return 0;
            }

            return 0;
        }
    }
}