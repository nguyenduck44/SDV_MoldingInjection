using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using System.ComponentModel;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class DataViewModel : ViewModelBase
    {
        public string RunModeButtonContent =>
            _machineStatus.MachineRunMode == EMachineRunMode.Auto ? "Dry Run Mode" : "Auto Run Mode";

        #region Commands
        public ICommand RecipeDataNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<RecipeViewModel>();
        });

        public ICommand AdditionalMoldingDataNavigateCommand => new RelayCommand(() =>
        {
           _navigationService.NavigateTo<AdditionalMoldingViewModel>();
        });

        public ICommand OptionNavigateCommand => new RelayCommand(() =>
        {
           _navigationService.NavigateTo<OptionViewModel>();
        });

        public ICommand MaterialPortsViewNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<MaterialPortsViewModel>();
        });

        public ICommand RunModeChangeCommand => new RelayCommand(() =>
        {
            EMachineRunMode currentMode = _machineStatus.MachineRunMode;
            EMachineRunMode newMode = currentMode == EMachineRunMode.Auto ? EMachineRunMode.DryRun : EMachineRunMode.Auto;

            bool? result = MessageBoxEx.ShowDialog($"Do you want to change RunMode {currentMode} -> {newMode}");

            if (result == true)
            {
                _machineStatus.MachineRunMode = newMode;
                OnPropertyChanged(nameof(RunModeButtonContent));
            }
        });
        public ICommand MotionConfigNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<MotionsConfigViewModel>();
                });
            }
        }

        public ICommand SecurityControlNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<SecurityControlViewModel>();
                });
            }
        }
        #endregion

        #region Constructor(s)
        public DataViewModel(INavigationService navigationService, MachineStatus machineStatus)
        {
            _navigationService = navigationService;
            _machineStatus = machineStatus;
        }
        #endregion

        #region Privates
        private readonly INavigationService _navigationService;
        private readonly MachineStatus _machineStatus;
        #endregion
    }
}
