using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace SDV_MoldingInjection.Defines.Devices
{
    public class SyringAmountStatusList
    {
        private readonly IConfiguration _configuration;
        private string BackupFolder => _configuration["Folders:BackupFolder"];

        public SyringAmountStatusList(IConfiguration configuration)
        {
            SyringeAmounts = new List<SyringeAmountStatus>
            {
                new SyringeAmountStatus(),
                new SyringeAmountStatus(),
                new SyringeAmountStatus(),
                new SyringeAmountStatus()
            };
            _configuration = configuration;
        }

        public List<SyringeAmountStatus> SyringeAmounts { get; private set; }

        public void ConsumeSyringeAmount(ESPDHead head, double weight)
        {
            int index = (int)head - 1;
            if (index < 0 || index >= SyringeAmounts.Count) return;

            var syringeAmount = SyringeAmounts[index];
            syringeAmount.RemainVolume = Math.Max(0, syringeAmount.RemainVolume - weight / 1000);
        }

        public void Save()
        {
            var options = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var syringAmountContent = JsonConvert.SerializeObject(SyringeAmounts, options);
            var syringAmountFilePath = Path.Combine(BackupFolder, "SyringeAmountStatus.json");
            File.WriteAllText(syringAmountFilePath, syringAmountContent);
        }

        public void Load()
        {
            var syringAmountFilePath = Path.Combine(BackupFolder, "SyringeAmountStatus.json");

            if (!File.Exists(syringAmountFilePath))
            {
                return;
            }
            SyringeAmounts = JsonConvert.DeserializeObject<List<SyringeAmountStatus>>(File.ReadAllText(syringAmountFilePath)) ?? new List<SyringeAmountStatus>();
        }
    }
}
