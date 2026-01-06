using BinaScraperApp.Scraper;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinaScraperApp.Parsers
{
    public static class DetailParser
    {
        public static async Task ParseAsync(IPage page, BinaItem item)
        {
            var pageText = await page.InnerTextAsync("body");

            // Məkan (ünvan) məlumatını çıxar
            var addressEl = page.Locator("h1.product-title");

            if (await addressEl.CountAsync() > 0)
            {
                var text = (await addressEl.First.InnerTextAsync()).Trim();
                // Sonuncu vergüldən sonrakı hissəni ünvan kimi götür
                item.Address = text.Split(',').Last().Trim();
            }

            // Default dəyərləri təyin et
            item.Cixarish = "Çıxarış Yoxdur";
            item.Ipoteka = "İpotekaya Yoxdur";
            item.Temir = "Təmirsiz";

            var rows = await page.Locator(".product-properties__i").AllAsync();

            // Əgər varsa, çıxarış, ipoteka və təmir məlumatlarını çıxar
            foreach (var row in rows)
            {
                var nameEl = row.Locator(".product-properties__i-name");
                var valueEl = row.Locator(".product-properties__i-value");

                if (await nameEl.CountAsync() > 0 && await valueEl.CountAsync() > 0)
                {
                    string name = (await nameEl.InnerTextAsync()).Trim().ToLower();
                    string value = (await valueEl.InnerTextAsync()).Trim().ToLower();

                    if (name.Contains("çıxarış") && value.Contains("var"))
                        item.Cixarish = "Çıxarış Var";

                    else if (name.Contains("ipoteka") && value.Contains("var"))
                        item.Ipoteka = "İpotekaya Var";

                    else if (name.Contains("təmir") && value.Contains("var"))
                        item.Temir = "Təmirli";
                }
            }

            // Otaq sayı, mərtəbə və kateqoriya məlumatlarını çıxar
            var rowsm = await page.Locator(".product-properties__i").AllAsync();

            foreach (var row in rowsm)
            {
                var nameEl = row.Locator(".product-properties__i-name");
                var valueEl = row.Locator(".product-properties__i-value");

                if (await nameEl.CountAsync() > 0 && await valueEl.CountAsync() > 0)
                {
                    string desc = (await nameEl.InnerTextAsync()).Trim().ToLower();
                    string value = (await valueEl.InnerTextAsync()).Trim();

                    if (desc.Contains("otaq"))
                    {
                        item.Room = value;
                    }
                    else if (desc.Contains("mərtəbə"))
                    {
                        item.Floor = value;
                    }
                    else if (desc.Contains("kateqoriya"))
                    {
                        item.Category = value;
                    }
                }
            }

            // Küçə adını çıxar
            var streetEl = page.Locator("div.product-map__left__address");
            if (await streetEl.CountAsync() > 0)
                item.Street = (await streetEl.First.InnerTextAsync()).Trim();

            // Nişangahları çıxar
            var hints = page.Locator("a[data-stat='product-locations']");
            var allTexts = await hints.AllInnerTextsAsync();

            item.LocationHint = string.Join(" ", allTexts
                .Select(txt => txt.Trim())
                .Where(txt => !string.IsNullOrWhiteSpace(txt)));

            // Əmlak növünü çıxar
            var emlakTypeEls = page.Locator(".product-breadcrumbs__i-link");

            int bcCount = await emlakTypeEls.CountAsync();
            if (bcCount > 0)
            {
                item.EmlakType = (await emlakTypeEls.Last.InnerTextAsync()).Trim();
            }

            // Sahə və həyət evi sahəsi məlumatlarını çıxar
            string pageContent = await page.InnerTextAsync("body");

            var matchM2 = System.Text.RegularExpressions.Regex.Match(
                pageContent,
                @"(\d+(?:[.,]\d+)?)\s*m²",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            var matchSot = System.Text.RegularExpressions.Regex.Match(
                pageContent,
                @"(\d+(?:[.,]\d+)?)\s*sot",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            if (matchM2.Success)
            {
                item.Area = matchM2.Groups[1].Value.Replace(',', '.') + " m²";
            }

            if (matchSot.Success)
            {
                item.HeyetEvArea = matchSot.Groups[1].Value.Replace(',', '.') + " sot";
            }

            // Həyət evi və torpaq olmayan əmlak növləri üçün həyət evi sahəsini "-" olaraq təyin et
            if (!string.IsNullOrWhiteSpace(item.EmlakType) &&
                !item.EmlakType.Contains("həyət evi", StringComparison.OrdinalIgnoreCase) &&
                !item.EmlakType.Contains("torpaq", StringComparison.OrdinalIgnoreCase))
            {
                item.HeyetEvArea = "-";
            }
        }
    }
}