using EQX.Core.Common;
using EQX.Device.Measurement;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using System.Windows;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class EBoxMainPowerMonitoringViewModel : ViewModelBase
    {
        private readonly Accura2550CMZ3PModule _accura2550;
        private readonly MachineStatus _machineStatus;
        private readonly Config _config;
        private readonly NonOverlappingTimer statusUpdateTimer;

        #region Properties
        public int EnergyReceived => _accura2550.EnergyReceived;
        public double MaxActivePowerTotal => _accura2550.MaxActivePowerTotal;
        public double ABVoltage => _accura2550.ABVoltage;
        public double BCVoltage => _accura2550.BCVoltage;
        public double ACurrent => _accura2550.ACurrent;
        public double BCurrent => _accura2550.BCurrent;
        #endregion

        public EBoxMainPowerMonitoringViewModel([FromKeyedServices("Accura2550")] Accura2550CMZ3PModule accura2550,
            MachineStatus machineStatus,
            Config config)
        {
            _machineStatus = machineStatus;
            _accura2550 = accura2550;
            _config = config;
            statusUpdateTimer = new NonOverlappingTimer(1000);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }

        #region Private Methods
        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_config.IsConnectEBoxMainPower)
            {
                OnPropertyChanged(nameof(EnergyReceived));
                OnPropertyChanged(nameof(MaxActivePowerTotal));
                OnPropertyChanged(nameof(ABVoltage));
                OnPropertyChanged(nameof(BCVoltage));
                OnPropertyChanged(nameof(ACurrent));
                OnPropertyChanged(nameof(BCurrent));
            }
        }
        #endregion

    }
}
