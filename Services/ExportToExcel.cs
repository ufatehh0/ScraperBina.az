using BinaScraperApp.Scraper;
using ClosedXML.Excel;
using System;
using System.IO;

namespace BinaScraperApp
{
    public class ExportToExcel
    {
        private XLWorkbook _wb;
        private IXLWorksheet _ws;
        private int _row = 2;
        private string _filePath;

        
        public void Init()
        {
            _wb = new XLWorkbook();
            _ws = _wb.Worksheets.Add("Elanlar");

            // Sütun başlıqları
            _ws.Cell(1, 1).Value = "No";
            _ws.Cell(1, 2).Value = "Id";
            _ws.Cell(1, 3).Value = "Link";
            _ws.Cell(1, 4).Value = "Yenilənmə/Əlavə edilmə vaxtı";
            _ws.Cell(1, 5).Value = "Baxış sayı";
            _ws.Cell(1, 6).Value = "Əməliyyat Növü";
            _ws.Cell(1, 7).Value = "Əmlak Növü";
            _ws.Cell(1, 8).Value = "Kateqoriya";
            _ws.Cell(1, 9).Value = "Qiymət";
            _ws.Cell(1, 10).Value = "Məkan";
            _ws.Cell(1, 11).Value = "Şəhər";
            _ws.Cell(1, 12).Value = "Otaq";
            _ws.Cell(1, 13).Value = "Sahə (m²)";
            _ws.Cell(1, 14).Value = "Sahə (sot)";
            _ws.Cell(1, 15).Value = "Mərtəbə";
            _ws.Cell(1, 16).Value = "Satıcı";
            _ws.Cell(1, 17).Value = "Çıxarış";
            _ws.Cell(1, 18).Value = "Ipoteka";
            _ws.Cell(1, 19).Value = "Təmir";
            _ws.Cell(1, 20).Value = "Küçə";
            _ws.Cell(1, 21).Value = "Nişangahlar";


            // output qovluğunu təyin et və yaradılmasını təmin et
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string outputDir = Path.Combine(baseDir, "output");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            _filePath = Path.Combine(
                outputDir,
                $"bina_elanlar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            );
        }

        // Məlumatları əlavə et
        public void AddItem(BinaItem item)
        {
            _ws.Cell(_row, 1).Value = item.No;
            _ws.Cell(_row, 2).Value = item.Id;
            _ws.Cell(_row, 3).Value = item.Link;
            _ws.Cell(_row, 4).Value = item.Date;        
            _ws.Cell(_row, 5).Value = item.ViewCount;  
            _ws.Cell(_row, 6).Value = item.Type;        
            _ws.Cell(_row, 7).Value = item.EmlakType;   
            _ws.Cell(_row, 8).Value = item.Category;
            _ws.Cell(_row, 9).Value = item.Price;
            _ws.Cell(_row, 10).Value = item.Address;     
            _ws.Cell(_row, 11).Value = item.City;
            _ws.Cell(_row, 12).Value = item.Room;
            _ws.Cell(_row, 13).Value = item.Area;        
            _ws.Cell(_row, 14).Value = item.HeyetEvArea; 
            _ws.Cell(_row, 15).Value = item.Floor;
            _ws.Cell(_row, 16).Value = item.Seller;
            _ws.Cell(_row, 17).Value = item.Cixarish;
            _ws.Cell(_row, 18).Value = item.Ipoteka;
            _ws.Cell(_row, 19).Value = item.Temir;
            _ws.Cell(_row, 20).Value = item.Street;
            _ws.Cell(_row, 21).Value = item.LocationHint;


            _row++;
        }

        // Diskə yadda saxla
        public void Save()
        {
            _ws.Columns().AdjustToContents();
            _wb.SaveAs(_filePath);
        }
    }
}
