using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Communication;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using SDV_MoldingInjection.Defines;
using System.Windows;
using System.Windows.Input;
using TOPENG_Device;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class DataViewModel : ViewModelBase
    {
        public string RunModeButtonContent =>
            MachineStatus.MachineRunMode == EMachineRunMode.Auto ? "Dry Run Mode" : "Auto Run Mode";

        #region Commands
        public ICommand RecipeDataNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<RecipeViewModel>();
        });

        public ICommand AdditionalMoldingDataNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<AdditionalMoldingViewModel>();
        });

        public ICommand CDASettingNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<CDASettingViewModel>();
        });

        public ICommand IdlePurgeDataNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<IdlePurgeViewModel>();
        });

        public ICommand OptionNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<OptionViewModel>();
        });

        public ICommand MaterialPortsViewNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<MaterialPortsViewModel>();
        });

        public ICommand TMPLossChangeCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    EquipEventDetail.Create(EquipEvent.TPMLossReady).SetPLCBitOn();

                    TPMLossWindow tpmLossWindow = new TPMLossWindow();
                    tpmLossWindow.ShowDialog();

                    if (tpmLossWindow.SelectedTPMMode == null) return;

                    await Task.Run(() =>
                    {
                        EquipEventHelpers.TPMLostReport((ETPMLossDesciption)tpmLossWindow.SelectedTPMMode);
                    });
                });
            }
        }

        public ICommand OpenCIMFunctionTestCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var window = new Window
                    {
                        Title = "EQP Function Change",
                        Content = new CIMFunctionView(),
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    window.DataContext = _cIMFunctionViewModel;
                    window.Show();
                });
            }
        }

        public ICommand RunModeChangeCommand => new RelayCommand(() =>
        {
            var result = RunModeDialog.ShowRunModeDialog<EQX.Core.Common.EMachineRunMode>(new List<EQX.Core.Common.EMachineRunMode>()
            {
                EQX.Core.Common.EMachineRunMode.Auto,
                EQX.Core.Common.EMachineRunMode.DryRun,
                EQX.Core.Common.EMachineRunMode.PassRun,
            });

            if (result != null) MachineStatus.MachineRunMode = (EQX.Core.Common.EMachineRunMode)result;
        });

        public ICommand InjectTimeNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<InjectTimeViewModel>();
        });

        public ICommand CylinderDelayTimeNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<CylinderDelayTimeViewModel>();
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

        public MachineStatus MachineStatus { get; }
        #endregion

        #region Constructor(s)
        public DataViewModel(INavigationService navigationService, 
            MachineStatus machineStatus,
            CIMFunctionViewModel cIMFunctionViewModel)
        {
            _navigationService = navigationService;
            _cIMFunctionViewModel = cIMFunctionViewModel;
            MachineStatus = machineStatus;
        }
        #endregion

        #region Privates
        private readonly INavigationService _navigationService;
        private readonly CIMFunctionViewModel _cIMFunctionViewModel;
        #endregion
    }
}
