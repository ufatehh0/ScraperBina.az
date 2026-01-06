namespace BinaScraperApp.Models
{
    public class SearchFilters
    {
        // Filterlər üçün property-lər
        public string City { get; set; } = "baki";
        public string OperationType { get; set; } = "alqi-satqi";
        public List<string> Metros { get; set; } = new();
        public string PropertyType { get; set; } = "menziller";
        public string BuildingType { get; set; } = "";
        public string IsRepair { get; set; } = "";
        public string MinPrice { get; set; } = "0";
        public string MaxPrice { get; set; } = "999999999";
        public string HasBillOfSale { get; set; } = "";
        public string Ipoteka { get; set; } = "";
        public string MinArea { get; set; } = "0";
        public string MaxArea { get; set; } = "999999999";
        public string MinLandArea { get; set; } = "0";
        public string MaxLandArea { get; set; } = "999999999";
        public List<string> Rooms { get; set; } = new();
        public string MinFloor { get; set; } = "0";
        public string MaxFloor { get; set; } = "999999999";
        public string FirstFloor { get; set; } = "";
        public string LastFloor { get; set; } = "";
        public string OnlyResidences { get; set; } = "";
        public string RentType { get; set; } = "";
        public string OfficeType { get; set; } = "";
    }
}
