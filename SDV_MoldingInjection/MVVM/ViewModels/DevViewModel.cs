using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using log4net;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.MVVM.Models;
using System.Windows.Input;
using TOPENG_Device;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class DevViewModel : ViewModelBase
    {
        private readonly Devices _devices;
        private readonly INavigationService _navigationService;
        private readonly InterlockService _interlockService;
        private readonly Plotter _plotter;
        private readonly CIMCollection _cIMCollection;
        private bool isDisableInterlock = false;
        #region Properties
        public bool IsDisableInterLock
        {
            get { return isDisableInterlock; }
            set
            {
                isDisableInterlock = value;
                _interlockService.Config(value);
            }
        }

        public bool IsBitOn => CIMAddressMap.ReadCIMBit("B2107") == 1;
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
                    Random rnd = new Random();
                    _plotter.AddChamberPressureData(rnd.Next(0, 749));
                });
            }
        }

        public ICommand TestOnOffBitCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MCCLinkIE.mddevset(151, 255, 23, 0x2007);

                    MCCLinkIE.mddevrst(151, 255, 23, 0x2006);
                    return;

                    int[] iReciveData = new int[1];
                    int iiReciveDataLength = 1;
                    MCCLinkIE.mdreceiveex(151, 0, 255, 23, 0x2000, ref iiReciveDataLength, ref iReciveData[0]);

                    iReciveData[0] = 0xFE << 7 & iReciveData[0];

                    int res = MCCLinkIE.mdsendex(151, 0, 255, 23, 0x2000, ref iiReciveDataLength, ref iReciveData[0]);
                });
            }
        }

        public ICommand TestMCCWriteJigIDCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _cIMCollection.WriteMCC_CellID(EMCCUnit.IJ01, "31A-005216-BME-PA-DTHB-NZPL505");
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
            Plotter plotter,
            CIMCollection cIMCollection,
            InterlockService interlockService)
        {
            _devices = devices;
            MachineStatus = machineStatus;
            _navigationService = navigationService;
            _plotter = plotter;
            _cIMCollection = cIMCollection;
            _interlockService = interlockService;
            Log = LogManager.GetLogger("DevVM");
            var statusUpdateTimer = new NonOverlappingTimer(10);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }

        #region Private Methods
        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {
            OnPropertyChanged(nameof(IsBitOn));
        }
        #endregion
    }
}
