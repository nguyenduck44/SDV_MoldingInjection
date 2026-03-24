using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_MoldingInjection.Defines.Productions;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class ProductionInforViewModel : ViewModelBase
    {
        private readonly ProductionService _productionService;
        private DateTime _selectedDate;
        private ProductionDayItem _selectedProduction;

        public ProductionInforViewModel(ProductionService productionService)
        {
            _productionService = productionService;
            _selectedDate = DateTime.Now.Date;
            _selectedProduction = _productionService.GetProductionByDate(_selectedDate) ?? new ProductionDayItem { Date = _selectedDate };
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate.Date == value.Date) return;

                _selectedDate = value.Date;
                LoadSelectedProduction();
                OnPropertyChanged();
            }
        }

        public ProductionDayItem SelectedProduction
        {
            get => _selectedProduction;
            private set
            {
                _selectedProduction = value;
                OnPropertyChanged();
            }
        }

        public ICommand TodayCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedDate = DateTime.Now.Date;
                });
            }
        }

        private void LoadSelectedProduction()
        {
            SelectedProduction = _productionService.GetProductionByDate(SelectedDate) ?? new ProductionDayItem { Date = SelectedDate.Date };
        }
    }
}
