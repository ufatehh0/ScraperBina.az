using BinaScraperApp.Scraper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinaScraperApp.Services
{
    public class ExportToCsv
    {
        private List<BinaItem> _items;
        private string _filePath;

        public void Init()
        {
            _items = new List<BinaItem>();

           
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string outputDir = Path.Combine(baseDir, "output");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            _filePath = Path.Combine(
                outputDir,
                $"bina_elanlar_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            );
        }

        
        public void AddItem(BinaItem item)
        {
            _items.Add(item);
        }

    
        public void Save()
        {
            using (var writer = new StreamWriter(_filePath, false, Encoding.UTF8))
            {
                
                writer.WriteLine("No,Id,Link,Yenilənmə/Əlavə edilmə vaxtı,Baxış sayı,Əməliyyat Növü,Əmlak Növü,Kateqoriya,Qiymət,Məkan,Şəhər,Otaq,Sahə (m²),Sahə (sot),Mərtəbə,Satıcı,Çıxarış,Ipoteka,Təmir,Küçə,Nişangahlar");

                
                foreach (var item in _items)
                {
                    var line = string.Join(",",
                        EscapeCsv(item.No.ToString()),
                        EscapeCsv(item.Id),          
                        EscapeCsv(item.Link),  
                        EscapeCsv(item.Date),
                        EscapeCsv(item.ViewCount),
                        EscapeCsv(item.Type),              
                        EscapeCsv(item.EmlakType),       
                        EscapeCsv(item.Category),       
                        EscapeCsv(item.Price),            
                        EscapeCsv(item.Address),           
                        EscapeCsv(item.City),           
                        EscapeCsv(item.Room),               
                        EscapeCsv(item.Area),             
                        EscapeCsv(item.HeyetEvArea),       
                        EscapeCsvAsText(item.Floor),      
                        EscapeCsv(item.Seller),          
                        EscapeCsv(item.Cixarish),          
                        EscapeCsv(item.Ipoteka),           
                        EscapeCsv(item.Temir),             
                        EscapeCsv(item.Street),       
                        EscapeCsv(item.LocationHint)        
                    );

                    writer.WriteLine(line);
                }
            }
        }

        
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

          
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                value = value.Replace("\"", "\"\""); 
                return $"\"{value}\"";
            }

            return value;
        }

        
        private string EscapeCsvAsText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            
            value = value.Replace("\"", "\"\"");
            return $"=\"{value}\"";
        }

        public string GetFilePath() => _filePath;
    }
}