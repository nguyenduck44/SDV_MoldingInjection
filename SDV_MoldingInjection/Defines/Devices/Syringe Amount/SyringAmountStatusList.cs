using log4net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.IO;
using System.Xml.Linq;

namespace SDV_MoldingInjection.Defines
{
    public class SyringAmountStatusList : MIProcess
    {
        private readonly IConfiguration _configuration;
        private readonly RecipeSelector _recipeSelector;

        private string BackupFolder => _configuration["Folders:BackupFolder"];

        public SyringAmountStatusList(IConfiguration configuration,
                                    RecipeSelector recipeSelector)
        {
            SyringeAmounts = new List<SyringeAmountStatus>
            {
                new SyringeAmountStatus(){Name = "RESIN HEAD 1" },
                new SyringeAmountStatus(){Name = "RESIN HEAD 2"},
                new SyringeAmountStatus(){Name = "RESIN HEAD 3"},
                new SyringeAmountStatus(){Name = "RESIN HEAD 4"}
            };
            _configuration = configuration;
            _recipeSelector = recipeSelector;
        }

        public List<SyringeAmountStatus> SyringeAmounts { get; private set; }
        public double Rho_Resin => _recipeSelector.CurrentRecipe.SPDHead1_Recipe.Rho_Resin;

        public void ConsumeSyringeAmount(ESPDHead head, double height)
        {
            double weight = Math.Round(Math.Pow(2.5,2) * Math.PI * Rho_Resin * Math.Abs(height), 3); // mm^3 = mg
            int index = (int)head - 1;
            if (index < 0 || index >= SyringeAmounts.Count) return;

            var syringeAmount = SyringeAmounts[index];
            syringeAmount.RemainVolume = Math.Max(0, syringeAmount.RemainVolume - weight / 1000);
            LogManager.GetLogger("SyringeAmount").Info($"{head} consume: {weight}mg");
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
