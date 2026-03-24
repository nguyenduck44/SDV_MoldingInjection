using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace SDV_MoldingInjection.Defines.Productions
{
    public enum EProductionWriteType
    {
        Input,
        Output
    }

    public class ProductionService
    {
        private readonly object lockObj = new object();
        private readonly IConfiguration _configuration;
        private string CountDataFolder => _configuration.GetValue<string>("Folders:CountDataFolder") ?? "";

        public ObservableCollection<ProductionDayItem> ProductionDatas { get; set; }

        public ProductionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private void EnsureCountDataFolderExists()
        {
            if (Directory.Exists(CountDataFolder) == false)
            {
                Directory.CreateDirectory(CountDataFolder);
            }
        }

        private ProductionDayItem? TryLoadProductionByDate(DateTime date)
        {
            string file = Path.Combine(CountDataFolder, $"{date.Date:yyyy_MM_dd}.json");
            if (File.Exists(file) == false)
            {
                return null;
            }

            ProductionDayItem? data = JsonConvert.DeserializeObject<ProductionDayItem>(File.ReadAllText(file));
            if (data == null)
            {
                return null;
            }

            data.SubscribeCountChanged();
            return data;
        }

        private ProductionDayItem GetOrCreateProductionByDate(DateTime date)
        {
            EnsureCountDataFolderExists();

            DateTime targetDate = date.Date;
            ProductionDayItem? cacheData = ProductionDatas.FirstOrDefault(pd => pd.Date.Date == targetDate, null);
            if (cacheData != null)
            {
                return cacheData;
            }

            ProductionDayItem? fileData = TryLoadProductionByDate(targetDate);
            if (fileData != null)
            {
                ProductionDatas.Add(fileData);
                return fileData;
            }

            ProductionDayItem newData = new ProductionDayItem
            {
                Date = targetDate
            };
            ProductionDatas.Add(newData);
            return newData;
        }

        public void WriteData(EProductionWriteType writeType, EPort side, int qty = 1)
        {
            lock (lockObj)
            {
                if (ProductionDatas == null)
                {
                    ProductionDatas = new ObservableCollection<ProductionDayItem>();
                }

                DateTime now = DateTime.Now;
                DateTime currentProductionDate = now.Hour < 8 ? now.Date.AddDays(-1) : now.Date;
                DateTime todayDate = now.Date;

                ProductionDayItem currentProductionDay = GetOrCreateProductionByDate(currentProductionDate);

                if (now.Hour >= 8)
                {
                    ProductionDayItem? yesterdayProduction = ProductionDatas.FirstOrDefault(
                        pd => pd.Date.Date == todayDate.AddDays(-1),
                        null);
                    if (yesterdayProduction != null && currentProductionDate != yesterdayProduction.Date.Date)
                    {
                        Save(yesterdayProduction);
                    }
                }

                var productionHours = side == EPort.Left
                    ? currentProductionDay.ProductionHoursLeft
                    : currentProductionDay.ProductionHoursRight;

                ProductionHourItem? currentProductionHour = productionHours.FirstOrDefault(ph => ph.Hour == now.Hour);
                if (currentProductionHour == null) return;

                switch (writeType)
                {
                    case EProductionWriteType.Input:
                        currentProductionHour.InputCount += qty;
                        break;
                    case EProductionWriteType.Output:
                        currentProductionHour.OutputCount += qty;
                        break;
                }
            }
        }

        public void Load()
        {
            lock (lockObj)
            {
                ProductionDatas = new ObservableCollection<ProductionDayItem>();
                EnsureCountDataFolderExists();

                DateTime currentProductionDate = DateTime.Now.Hour < 8
                    ? DateTime.Now.Date.AddDays(-1)
                    : DateTime.Now.Date;

                string productionFile = Path.Combine(CountDataFolder, $"{currentProductionDate:yyyy_MM_dd}.json");
                if (File.Exists(productionFile))
                {
                    ProductionDayItem? pd = JsonConvert.DeserializeObject<ProductionDayItem>(File.ReadAllText(productionFile));

                    if (pd != null)
                    {
                        pd.SubscribeCountChanged();
                        ProductionDatas.Add(pd);
                    }
                }

                if (ProductionDatas.FirstOrDefault(pd => pd.Date.Date == currentProductionDate) == null)
                {
                    ProductionDayItem todayProduction = new ProductionDayItem
                    {
                        Date = currentProductionDate
                    };
                    ProductionDatas.Add(todayProduction);
                }
            }
        }

        public ProductionDayItem? GetProductionByDate(DateTime date)
        {
            var targetDate = date.Date;
            lock (lockObj)
            {
                EnsureCountDataFolderExists();

                if (ProductionDatas == null)
                {
                    ProductionDatas = new ObservableCollection<ProductionDayItem>();
                }
                else
                {
                    ProductionDayItem? cacheData = ProductionDatas.FirstOrDefault(pd => pd.Date.Date == targetDate, null);
                    if (cacheData != null)
                    {
                        return cacheData;
                    }
                }

                ProductionDayItem? data = TryLoadProductionByDate(targetDate);
                if (data == null)
                {
                    return null;
                }

                ProductionDatas.Add(data);
                return data;
            }
        }

        public void Save(ProductionDayItem pd)
        {
            string file = Path.Combine(CountDataFolder, $"{pd.Date.ToString("yyyy_MM_dd")}.json");

            File.WriteAllText(file, JsonConvert.SerializeObject(pd, Formatting.Indented));
        }

        public void Save()
        {
            ProductionDayItem? todayProduction = ProductionDatas.FirstOrDefault(pd => pd.Date.ToString("dd:MM:yyyy") == DateTime.Now.ToString("dd:MM:yyyy"));
            ProductionDayItem? yesterdayProduction = ProductionDatas.FirstOrDefault(pd => pd.Date.ToString("dd:MM:yyyy") == DateTime.Now.Date.AddDays(-1).ToString("dd:MM:yyyy"));
            if (todayProduction != null)
            {
                Save(todayProduction);
            }

            if (yesterdayProduction != null)
            {
                Save(yesterdayProduction);
            }
        }
    }
}
