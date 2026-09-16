using Microsoft.Extensions.Configuration;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class HistoryDataViewModel : LogViewModel
    {
        protected override string LogFolder => _configuration["Folders:HistoryDataFolder"] ?? "";
        public HistoryDataViewModel(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
