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
        private Task FDCReportTask;

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

            CancellationToken token = new CancellationToken();
            FDCReportTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await WriteFDC();
                    await Task.Delay(500);
                }
            }, token);
        }

        #region Private Methods
        private async Task WriteFDC()
        {
            if (_config.IsConnectEBoxMainPower)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_T_PW, EnergyReceived * 1000);
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_I_PW, (int)(MaxActivePowerTotal * 1000));
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_R_VOL, (int)(ABVoltage * 1000));
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_T_VOL, (int)(BCVoltage * 1000));
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_R_CUR, (int)(ACurrent * 1000));
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_T_CUR, (int)(BCurrent * 1000));
                });
            }
        }

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
