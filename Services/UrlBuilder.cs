using BinaScraperApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace BinaScraperApp.Services
{
    public static class UrlBuilder
    {
        public static string url = "";
        
        

        public static string Build(SearchFilters f)
        {
            string city = f.City;
            string op = f.OperationType;
            string property = f.PropertyType;
            string buildingType = f.BuildingType;
            string repair = f.IsRepair;
            string minPrice = f.MinPrice;
            string maxPrice = f.MaxPrice;
            string billOfSale = f.HasBillOfSale;
            string ipoteka = f.Ipoteka;
            string minArea = f.MinArea;
            string maxArea = f.MaxArea;
            string minLandArea = f.MinLandArea;
            string maxLandArea = f.MaxLandArea;
            string minFloor = f.MinFloor;
            string maxFloor = f.MaxFloor;
            string firstFloor = f.FirstFloor;
            string lastFloor = f.LastFloor;
            string onlyResidences = f.OnlyResidences;
            string officeType = f.OfficeType;
            string rentType = f.RentType;

            // Kirayə üçün ipoteka və sənəd filtrlərini sil
            if (op == "kiraye" || op == "kiraye_daily")
            {
                ipoteka = "";
                billOfSale = "";
                onlyResidences = "";
            }


            // Hər əmlak növü üçün uyğun URL quruluşunu təyin et
            switch (property)
            {
                
                case "menziller":
                    url = $"https://bina.az/{city}/{op}/{property}/{buildingType}" +
                          $"?price_from={minPrice}&price_to={maxPrice}" +
                          $"&area_from={minArea}&area_to={maxArea}" +
                          $"{rentType}" +
                          $"{billOfSale}" +                          
                          $"{ipoteka}" +
                          $"{onlyResidences}" +
                          $"{repair}" +
                          $"&floor_from={minFloor}&floor_to={maxFloor}" +
                          $"{firstFloor}" +
                          $"{lastFloor}" +
                          $"&location_ids[]=0&";
                    break;

                case "heyet-evleri":
                    url = $"https://bina.az/{city}/{op}/{property}/{buildingType}" +
                          $"?price_from={minPrice}&price_to={maxPrice}" +
                          $"&area_from={minArea}&area_to={maxArea}" +
                          $"&land_area_from={minLandArea}&land_area_to={maxLandArea}" +
                          $"{rentType}" +
                          $"{billOfSale}" +
                          $"{ipoteka}" +
                          $"{onlyResidences}" +
                          $"{repair}" +
                          $"&location_ids[]=0&";
                    break;
                     
                case "ofisler":
                    url = $"https://bina.az/{city}/{op}/{property}" +
                          $"?price_from={minPrice}&price_to={maxPrice}" +
                          $"&area_from={minArea}&area_to={maxArea}" +
                          $"{rentType}" +
                          $"{billOfSale}" +
                          $"{ipoteka}" +
                          $"{onlyResidences}"+
                          $"{repair}" +
                          $"{officeType}" +
                          $"&location_ids[]=0&";
                    break;

                case "qarajlar":
                    url = $"https://bina.az/{city}/{op}/{property}" +
                          $"?price_from={minPrice}&price_to={maxPrice}" +
                          $"&area_from={minArea}&area_to={maxArea}" +
                          $"{rentType}" +
                          $"{billOfSale}" +
                          $"{ipoteka}" +
                          $"{onlyResidences}" +
                          $"&location_ids[]=0&";
                    break;

                case "torpaq":
                    url = $"https://bina.az/{city}/{op}/{property}" +
                          $"?price_from={minPrice}&price_to={maxPrice}" +
                          $"&area_from={minLandArea}&area_to={maxLandArea}" +
                          $"{rentType}" +
                          $"{billOfSale}"  +
                          $"{ipoteka}" +
                          $"&location_ids[]=0&";
                    break;
               
                case "obyektler":
                    url = $"https://bina.az/{city}/{op}/obyektler" +    
                          $"?price_from={minPrice}&price_to={maxPrice}" +
                          $"&area_from={minArea}&area_to={maxArea}" +
                          $"{rentType}" +
                          $"{billOfSale}" +
                          $"{ipoteka}" +
                          $"&location_ids[]=0&" +
                          $"{onlyResidences}" +
                          $"{repair}";
                    break;

                default:
                    url = "";
                    break;
            }

            
            List<string> query = new();

            // Metro filterini əlavə et
            if (city == "baki")
            {
                foreach (var metroId in f.Metros)
                    query.Add($"location_ids[]={metroId}");
            }

            if (property == "menziller" || property == "heyet-evleri" || property == "ofisler")
            {
                // Otaq sayı filterini əlavə et
                foreach (var roomId in f.Rooms)
                    query.Add($"room_ids[]={roomId}");

                // Əgər query siyahısında elementlər varsa, onları URL-ə əlavə et
                if (query.Count > 0)
                    url += string.Join("&", query);
            }

            return url;
        }
    }
}
