namespace BinaScraperApp.Scraper
{
    public class BinaItem
    {

        // Export ediləcək məlumatlar üçün property-lər
        public int No { get; set; }
        public string Link { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Price { get; set; }
        public string Address { get; set; }
        public string Room { get; set; } = "-";
        public string Area { get; set; } = "-";
        public string HeyetEvArea { get; set; } = "-";
        public string Floor { get; set; } = "-";
        public string City { get; set; }
        public string Seller { get; set; }
        public string Cixarish { get; set; }
        public string Temir { get; set; }
        public string Ipoteka { get; set; }
        public string Street { get; set; }
        public string LocationHint { get; set; }
        public string EmlakType { get; set; }
    }
}