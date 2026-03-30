using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Robot;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.MVVM.Models;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class DevViewModel : ViewModelBase
    {
        private readonly Devices _devices;
        private readonly INavigationService _navigationService;
        private readonly Plotter _plotter;

        #region Properties
        #endregion

        #region Commands
        public ICommand CIMTetstViewNavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<CIMTestViewModel>();
                });
            }
        }

        public ICommand SetTorqueZAxisCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                    AXM.AxmMotSetTorqueLimit(_devices.Motions.Z1Axis.Id, 0.1, 0.1);
                    AXM.AxmMotSetTorqueLimit(_devices.Motions.Z2Axis.Id, 0.1, 0.1);
                    AXM.AxmMotSetTorqueLimit(_devices.Motions.Z3Axis.Id, 0.1, 0.1);
                    AXM.AxmMotSetTorqueLimit(_devices.Motions.Z4Axis.Id, 0.1, 0.1);

                    double plusTorque = -1;
                    double minusTorque = -1;
                    AXM.AxmMotGetTorqueLimit(_devices.Motions.Z3Axis.Id, ref plusTorque, ref minusTorque);
                });
            }
        }

        public ICommand TestButtonCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _plotter.Save();
                });
            }
        }
        #endregion

        public MachineStatus MachineStatus { get; }
        public string Name { get; private set; }

        public DevViewModel(
            Devices devices,
            MachineStatus machineStatus,
            INavigationService navigationService,
            Plotter plotter)
        {
            _devices = devices;
            MachineStatus = machineStatus;
            _navigationService = navigationService;
            _plotter = plotter;

            Log = LogManager.GetLogger("DevVM");
        }

        #region Privates
        #endregion
    }
}
