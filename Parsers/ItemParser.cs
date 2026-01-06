using Microsoft.Playwright;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using BinaScraperApp.Scraper;

namespace BinaScraperApp.Parsers
{
    public class ItemParser
    {
        public async Task<BinaItem> ParseAsync(ILocator card)
        {
            var item = new BinaItem();

            // Link və Id məlumatını çıxar
            var linkEl = card.Locator("a[href]").First;
            var href = await linkEl.GetAttributeAsync("href");
            item.Link = "https://bina.az" + href;
            item.Id = href.Split('/').Last();

            // Qiymət məlumatını çıxar
            item.Price = await SafeText(card, "[class*='price-container'] span");

            // Satış növünü çıxar
            item.Type = await SafeText(card, "[data-cy='item-card-price-container']");
            if (!string.IsNullOrWhiteSpace(item.Type))
            {
                item.Type = item.Type.TrimStart('/').Trim();
                item.Type = item.Type + " Kiraye";
            }
            else
            {
                item.Type = "Alqı Satqı";
            }

            // Şəhər məlumatını çıxar
            var cityWhen = await SafeText(card, "[data-cy='city_when']");
            if (cityWhen.Contains(","))
                item.City = cityWhen.Split(',')[0];

            // Satıcı növünü çıxar
            var seller = await SafeText(card, "[data-cy='product-label-agency']");
            item.Seller = string.IsNullOrWhiteSpace(seller) ? "Sahibi" : "Agentlik";

            return item;
        }


        private async Task<string> SafeText(ILocator parent, string selector)
        {
            try
            {
                var loc = parent.Locator(selector);
                if (await loc.CountAsync() == 0)
                    return "";
                return (await loc.First.InnerTextAsync()).Trim();
            }
            catch { return ""; }
        }
    }
}