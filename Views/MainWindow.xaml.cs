using BinaScraperApp.Models;
using BinaScraperApp.Scraper;
using BinaScraperApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BinaScraperApp
{
    public partial class MainWindow : Window
    {
        private SearchFilters _filters = new SearchFilters();
        public int countElan;


        public MainWindow()
        {
            InitializeComponent();
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MetroPanel.Visibility = Visibility.Visible;
        }


        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlBuilder.Build(_filters);


            
            // Elan sayını tapan scraperı işə sal
            var countScraper = new ElanScraper();

            Yazdir_Button.Visibility = Visibility.Collapsed;
            ExportFormatPanel.Visibility = Visibility.Collapsed;
            GirisStackPanel.Visibility = Visibility.Collapsed;
            ScrapeProgressBar.Visibility = Visibility.Collapsed;
            EtaText.Visibility = Visibility.Collapsed;

            MelumatText.Text = "Elanlar sayılır, zəhmət olmasa gözləyin...";

            countElan = await countScraper.GetListingCountAsync(url, BrowserVisibleCheckBox.IsChecked == true);

            MelumatText.Text = $"Toplam elan sayı: {countElan}";
            MelumatText.Visibility = Visibility.Visible;

            if (countElan == 0)
            {
                ExportFormatPanel.Visibility = Visibility.Collapsed;
                ExportRadioPanel.IsEnabled = false;
                Yazdir_Button.Visibility = Visibility.Collapsed;
                GirisStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ExportFormatPanel.Visibility = Visibility.Visible;
                ExportRadioPanel.IsEnabled = true;
                Yazdir_Button.Visibility = Visibility.Visible;
                GirisStackPanel.Visibility = Visibility.Visible;
                

            }
        }

        // Elanları Scrape edib Excel-ə yazdırma
        private async void ElanYazdir_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlBuilder.Build(_filters);

            bool saveAsExcel = ExcelFormatRadio.IsChecked == true;
            bool saveAsCsv = CsvFormatRadio.IsChecked == true;
            bool saveAsBoth = BothFormatRadio?.IsChecked == true;

            

            if (!int.TryParse(SayTextBox.Text, out int maxCount) || maxCount <= 0)
            {
                MessageBox.Show("Elan sayı düzgün deyil");
                return;
            }

            if (maxCount > countElan)
            {
                
                MessageBox.Show(
                    $"Daxil etdiyiniz elan sayı tapılan elandan çoxdur.\n" +
                    $"Tapılan: {countElan}",
                    "Xəta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            ExportRadioPanel.IsEnabled = false;


            var scraper = new BinaScraper();

            
            ScrapeProgressBar.Value = 0;
            EtaText.Text = "";
            ScrapeProgressBar.Visibility = Visibility.Visible;
            EtaText.Visibility = Visibility.Visible;

            DateTime startTime = DateTime.Now;

            ExportToExcel excelExporter = null;
            ExportToCsv csvExporter = null;

            if (saveAsExcel || saveAsBoth)
            {
                excelExporter = new ExportToExcel();
                excelExporter.Init();
            }

            if (saveAsCsv || saveAsBoth)
            {
                csvExporter = new ExportToCsv();
                csvExporter.Init();
            }

            int writtenCount = 0;
            DateTime lastItemTime = DateTime.Now;
            Queue<TimeSpan> lastDurations = new();

            var items = await scraper.GetItemsLazyAsync(
                url,
                maxCount,
                BrowserVisibleCheckBox.IsChecked == true,

                
                (current, total) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (current == -1)
                        {
                            
                            MelumatText.Text = $"Elanlar Linkləri Yığılır... Yığılan Link Sayı: {total}";
                            ScrapeProgressBar.IsIndeterminate = true;
                            return;
                        }

                        if (current == -2)
                        {
                            MelumatText.Text = "Elan Məlumatları Yaddaşa Yazılır...";
                            ScrapeProgressBar.IsIndeterminate = false;
                            ScrapeProgressBar.Value = 0;
                            return;
                        }

                        
                        if (current % 5 == 0 || current == total)
                        {
                            int percent = (int)((current / (double)total) * 100);
                            ScrapeProgressBar.Value = percent;

                            if (lastDurations.Count > 0)
                            {
                                double avgMs = lastDurations.Average(d => d.TotalMilliseconds);
                                var eta = TimeSpan.FromMilliseconds(avgMs * (total - current));
                                EtaText.Text = $"{current}/{total}";
                            }
                        }
                    });
                },

                
                item =>
                {
                   
                    excelExporter?.AddItem(item);
                    csvExporter?.AddItem(item);

                    writtenCount++;

                    var now = DateTime.Now;
                    var duration = now - lastItemTime;
                    lastItemTime = now;

                    lastDurations.Enqueue(duration);
                    if (lastDurations.Count > 5)
                        lastDurations.Dequeue();

                    if (writtenCount % 5 == 0)
                    {
                        excelExporter?.Save();
                        csvExporter?.Save();
                    }
                }
            );


            excelExporter?.Save();
            csvExporter?.Save();

            Dispatcher.Invoke(() =>
            {
                ExportRadioPanel.IsEnabled = true;
                ScrapeProgressBar.Value = 100;
                MelumatText.Text =
                    $"Bütün elanlar yazıldı! Ümumi vaxt: {(DateTime.Now - startTime):hh\\:mm\\:ss}";
                EtaText.Text = $"{maxCount}/{maxCount} | Tamamlandı ✔";
            });
        }

        // Sadəcə rəqəm daxil etməyə icazə ver
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void NumberOnly_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = false;
                return;
            }
        }

        // Əmlak növü filteri
        private void PropertyType_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return; 

            var rb = sender as RadioButton;
            if (rb == null) return;

            string text = rb.Content.ToString();

            if (Maps.PropertyMap.TryGetValue(text, out string propertyUrl))
            {
                _filters.PropertyType = propertyUrl;
            }

            switch (rb.Content.ToString())
            {
                case "Mənzil":
                    YeniKohnePanel.Visibility = Visibility.Visible;
                    OfisTikiliPanel.Visibility = Visibility.Collapsed;
                    LandAreaPanel.Visibility = Visibility.Collapsed;

                    if (_filters.OperationType == "alqi-satqi")
                        OnlyResidences.Visibility = Visibility.Visible;

                    MertebePanel.Visibility = Visibility.Visible;
                    TemirPanel.Visibility = Visibility.Visible;
                    OtaqPanel.Visibility = Visibility.Visible;
                    AreaPanel.Visibility = Visibility.Visible;
                    break;

                case "Həyət evi/Bağ evi":
                    YeniKohnePanel.Visibility = Visibility.Collapsed;
                    OfisTikiliPanel.Visibility = Visibility.Collapsed;
                    LandAreaPanel.Visibility = Visibility.Visible;
                    OnlyResidences.Visibility = Visibility.Collapsed;
                    MertebePanel.Visibility = Visibility.Collapsed;
                    TemirPanel.Visibility = Visibility.Visible;
                    AreaPanel.Visibility = Visibility.Visible;
                    OtaqPanel.Visibility = Visibility.Visible;
                    break;

                case "Ofis":
                    YeniKohnePanel.Visibility = Visibility.Collapsed;
                    OfisTikiliPanel.Visibility = Visibility.Visible;
                    AreaPanel.Visibility = Visibility.Visible;

                    if (_filters.OperationType == "alqi-satqi")
                        OnlyResidences.Visibility = Visibility.Visible;

                    MertebePanel.Visibility = Visibility.Collapsed;
                    TemirPanel.Visibility = Visibility.Visible;
                    OtaqPanel.Visibility = Visibility.Visible;
                    break;

                case "Qaraj":
                    YeniKohnePanel.Visibility = Visibility.Collapsed;
                    OfisTikiliPanel.Visibility = Visibility.Collapsed;

                    if (_filters.OperationType == "alqi-satqi")
                        OnlyResidences.Visibility = Visibility.Visible;

                    MertebePanel.Visibility = Visibility.Collapsed;
                    AreaPanel.Visibility = Visibility.Visible;
                    LandAreaPanel.Visibility = Visibility.Collapsed;
                    TemirPanel.Visibility = Visibility.Collapsed;
                    OtaqPanel.Visibility = Visibility.Collapsed;
                    break;

                case "Torpaq":
                    YeniKohnePanel.Visibility = Visibility.Collapsed;
                    OfisTikiliPanel.Visibility = Visibility.Collapsed;
                    LandAreaPanel.Visibility = Visibility.Visible;
                    OnlyResidences.Visibility = Visibility.Collapsed;
                    MertebePanel.Visibility = Visibility.Collapsed;
                    TemirPanel.Visibility = Visibility.Collapsed;
                    AreaPanel.Visibility = Visibility.Collapsed;
                    OtaqPanel.Visibility = Visibility.Collapsed;
                    break;

                case "Obyekt":
                    YeniKohnePanel.Visibility = Visibility.Collapsed;
                    OfisTikiliPanel.Visibility = Visibility.Collapsed;

                    if (_filters.OperationType == "alqi-satqi")
                        OnlyResidences.Visibility = Visibility.Visible;

                    MertebePanel.Visibility = Visibility.Collapsed;
                    TemirPanel.Visibility = Visibility.Visible;
                    OtaqPanel.Visibility = Visibility.Collapsed;
                    LandAreaPanel.Visibility = Visibility.Collapsed;
                    AreaPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        // Mərtəbə məhdudiyyəti filteri
        private bool _ignoreLastFloorEvents = false;

        private void OnlyTopCheck_Checked(object sender, RoutedEventArgs e)
        {
            _ignoreLastFloorEvents = true;

            NotTopCheck.IsChecked = false;
            NotTopCheck.IsEnabled = false;
            NotFirstCheck.IsEnabled = false;

            _ignoreLastFloorEvents = false;
            UpdateLastFloorFilter();
        }

        private void OnlyTopCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_ignoreLastFloorEvents) return;

            NotTopCheck.IsEnabled = true;
            NotFirstCheck.IsEnabled = true;

            UpdateLastFloorFilter();
        }

        private void NotTopCheck_Checked(object sender, RoutedEventArgs e)
        {
            _ignoreLastFloorEvents = true;

            OnlyTopCheck.IsChecked = false;
            OnlyTopCheck.IsEnabled = false;

            _ignoreLastFloorEvents = false;
            UpdateLastFloorFilter();
        }

        private void NotTopCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_ignoreLastFloorEvents) return;

            OnlyTopCheck.IsEnabled = true;
            UpdateLastFloorFilter();
        }

        private void UpdateLastFloorFilter()
        {
            if (OnlyTopCheck.IsChecked == true)
                _filters.LastFloor = "&floor_last=true";
            else if (NotTopCheck.IsChecked == true)
                _filters.LastFloor = "&floor_last=false";
            else
                _filters.LastFloor = "";
        }

       
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            string placeholder = tb.Tag?.ToString() ?? "";

            if (tb.Text == placeholder)
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            string placeholder = tb.Tag?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = placeholder;
                tb.Foreground = Brushes.Gray;
            }
        }

        
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;

            
            Process.Start(exePath);

           
            Application.Current.Shutdown();
        }

        private void MetroButton_Click(object sender, RoutedEventArgs e)
        {
            bool isVisible = MetroPanel.Visibility == Visibility.Visible;

            MetroPanel.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;

            MetroButton.Content = isVisible
                ? "Metro Panelini Göstər"
                : "Metro Panelini Gizlə";
        }

        private void RayonButton_Click(object sender, RoutedEventArgs e)
        {
            bool isVisible = RayonPanel.Visibility == Visibility.Visible;

            RayonPanel.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;

            RayonButton.Content = isVisible
                ? "Rayon Panelini Göstər"
                : "Rayon Panelini Gizlə";
        }

        private void NisangahButton_Click(object sender, RoutedEventArgs e)
        {
            bool isVisible = NişangahPanel.Visibility == Visibility.Visible;

            NişangahPanel.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;

            NisangahButton.Content = isVisible
                ? "Nişangah Panelini Göstər"
                : "Nişangah Panelini Gizlə";
        }

        // Əməliyyat növü filteri
        private void Operation_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;

            var rb = sender as RadioButton;
            string key = rb.Tag.ToString();

            if (key == "alqi-satqi")
            {
                if (CixarisPanel.Visibility == Visibility.Collapsed)
                {
                    if (_filters.PropertyType == "mənzil" || _filters.PropertyType == "ofis" ||
                        _filters.PropertyType == "qaraj" || _filters.PropertyType == "obyekt")
                        OnlyResidences.Visibility = Visibility.Visible;
                    CixarisPanel.Visibility = Visibility.Visible;
                }

                _filters.RentType = "";
                _filters.OperationType = "alqi-satqi";
            }

            if (key == "kirayeay")
            {
                _filters.RentType = "&paid_daily=false";
                _filters.OperationType = "kiraye";
                CixarisPanel.Visibility = Visibility.Collapsed;
                OnlyResidences.Visibility = Visibility.Collapsed;
            }
            if (key == "kirayegun")
            {
                _filters.RentType = "&paid_daily=true";
                _filters.OperationType = "kiraye";
                CixarisPanel.Visibility = Visibility.Collapsed;
                OnlyResidences.Visibility = Visibility.Collapsed;
            }
            if (key == "kirayeumumi")
            {
                _filters.RentType = "";
                _filters.OperationType = "kiraye";
                CixarisPanel.Visibility = Visibility.Collapsed;
                OnlyResidences.Visibility = Visibility.Collapsed;
            }
        }

        // Şəhər filteri
        private void City_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return; 

            var rb = sender as RadioButton;
            if (rb == null) return;

            string text = rb.Content.ToString();
            bool isBaku = text == "Bakı";

            
            MetroPanel.Visibility = Visibility.Collapsed;
            NişangahPanel.Visibility = Visibility.Collapsed;
            RayonPanel.Visibility = Visibility.Collapsed;

           
            MetroButton.Visibility = isBaku ? Visibility.Visible : Visibility.Collapsed;
            NisangahButton.Visibility = isBaku ? Visibility.Visible : Visibility.Collapsed;
            RayonButton.Visibility = isBaku ? Visibility.Visible : Visibility.Collapsed;

            
            MetroButton.Content = "Metro Panelini Göstər";
            NisangahButton.Content = "Nişangah Panelini Göstər";
            RayonButton.Content = "Rayon Panelini Göstər";

            
            if (Maps.CityMap.TryGetValue(text, out string cityUrl))
                _filters.City = cityUrl;

            
            _filters.City = Maps.CityMap[text];
        }

        // Metro filteri
        private void Metro_Checked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            string metroName = cb.Content.ToString();

            if (Maps.LocationMap.TryGetValue(metroName, out string metroId))
            {
                if (!_filters.Metros.Contains(metroId))
                    _filters.Metros.Add(metroId);
            }
        }

        private void Metro_Unchecked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            string metroName = cb.Content.ToString();

            if (Maps.LocationMap.TryGetValue(metroName, out string metroId))
            {
                _filters.Metros.Remove(metroId);
            }
        }

        // Tikili növü filteri
        private void BuildingType_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            string text = rb.Content.ToString();

            if (Maps.BuildingMap.TryGetValue(text, out string buildingurl))
            {
                _filters.BuildingType = buildingurl;
            }
        }

        // Təmir filteri
        private void Temir_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            string text = rb.Content.ToString();
            if (Maps.RepairMap.TryGetValue(text, out string repairurl))
            {
                _filters.IsRepair = repairurl;
            }
        }

        // Qiymət filterləri
        private void PriceMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filters == null) return;
            _filters.MinPrice = PriceMin.Text;
        }

        private void PriceMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filters == null) return;
            _filters.MaxPrice = PriceMax.Text;
        }

        // Çıxarış filteri
        private void CixarishCheck_Checked(object sender, RoutedEventArgs e)
        {
            _filters.HasBillOfSale = "&has_bill_of_sale=true";
        }

        private void CixarishCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _filters.HasBillOfSale = "";
        }

        // İpoteka filteri
        private void IpotekaCheck_Checked(object sender, RoutedEventArgs e)
        {
            _filters.Ipoteka = "&has_mortgage=true";
        }

        private void IpotekaCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _filters.Ipoteka = "";
        }

        // Sahə filterləri
        private void AreaMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filters == null) return;
            _filters.MinArea = AreaMin.Text;
        }

        private void AreaMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filters == null) return;
            _filters.MaxArea = AreaMax.Text;
        }

        // Sahə (sot) filterləri
        private void LandAreaMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filters == null) return;
            _filters.MinLandArea = LandAreaMin.Text;
        }

        private void LandAreaMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filters == null) return;
            _filters.MaxLandArea = LandAreaMax.Text;
        }

        // Otaq sayı filteri
        private void Room_Checked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            string roomName = cb.Content.ToString();

            if (Maps.RoomMap.TryGetValue(roomName, out string roomId))
            {
                if (!_filters.Rooms.Contains(roomId))
                    _filters.Rooms.Add(roomId);
            }
        }

        private void Room_Unchecked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            string roomName = cb.Content.ToString();

            if (Maps.RoomMap.TryGetValue(roomName, out string roomId))
            {
                _filters.Rooms.Remove(roomId);
            }
        }

        // Mərtəbə filterləri
        private void FloorMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filters == null) return;
            _filters.MinFloor = FloorMin.Text;
        }

        private void FloorMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filters == null) return;
            _filters.MaxFloor = FloorMax.Text;
        }

        // İlk mərtəbə filteri
        public void NotFirstCheck_Checked(object sender, RoutedEventArgs e)
        {
            _filters.FirstFloor = "&floor_first=false";
        }

        public void NotFirstCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _filters.FirstFloor = "";
        }

        // Yalnız Tikiti şirkətləri filteri
        public void OnlyResidencesCheck_Checked(object sender, RoutedEventArgs e)
        {
            _filters.OnlyResidences = "true";
        }

        public void OnlyResidencesCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _filters.OnlyResidences = "";
        }

        // Ofis növü filterləri
        public void OfficeType_BiznesChecked(object sender, RoutedEventArgs e)
        {
            _filters.OfficeType += "&building_type[]=business_center";
        }

        public void OfficeType_BiznesUnchecked(object sender, RoutedEventArgs e)
        {
            _filters.OfficeType = _filters.OfficeType.Replace("&building_type[]=business_center", "");
        }

        public void OfficeType_EvChecked(object sender, RoutedEventArgs e)
        {
            _filters.OfficeType += "&building_type[]=apartment";
        }

        public void OfficeType_EvUnchecked(object sender, RoutedEventArgs e)
        {
            _filters.OfficeType = _filters.OfficeType.Replace("&building_type[]=apartment", "");
        }

        public void OfficeType_VillaChecked(object sender, RoutedEventArgs e)
        {
            _filters.OfficeType += "&building_type[]=villa";
        }

        public void OfficeType_VillaUnchecked(object sender, RoutedEventArgs e)
        {
            _filters.OfficeType = _filters.OfficeType.Replace("&building_type[]=villa", "");
        }

        private void SeherButton_Click(object sender, RoutedEventArgs e)
        {
            bool isVisible = SeherPanel.Visibility == Visibility.Visible;

            SeherPanel.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;

            SeherButton.Content = isVisible
                ? "Şəhər Panelini Göstər"
                : "Şəhər Panelini Gizlə";
        }
    }
}