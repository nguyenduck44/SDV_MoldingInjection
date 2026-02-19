using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Process;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class InjectMaintenanceViewModel : MaintenanceViewModel<ESequence>
    {
        public InjectMaintenanceViewModel(NavigationStore navigationStore, Devices devices)
            : base(navigationStore)
        {
            _devices = devices;
        }
        override public void Init()
        {
            RelatedViewModel = new List<string>
            {
                nameof(EProcess.DryPump),
                nameof(EProcess.SPDHead1),
                nameof(EProcess.SPDHead2),
                nameof(EProcess.SPDHead3),
                nameof(EProcess.SPDHead4),
            };
            Motions = new ObservableCollection<IMotion>
            {
                _devices.Motions.XAxis,
                _devices.Motions.StageYAxis,
            };
            Cylinders = new ObservableCollection<ICylinder>
            {
                _devices.Cylinders.BellowCyl,
                _devices.Cylinders.ChamberOpenClose,
                _devices.Cylinders.NozzleClean_H1,
                _devices.Cylinders.NozzleClean_H2,
                _devices.Cylinders.NozzleClean_H3,
                _devices.Cylinders.NozzleClean_H4,
            };
            Inputs = new ObservableCollection<IDInput>
            {
                _devices.Inputs.Jig1Detect,
                _devices.Inputs.Jig2Detect,
                _devices.Inputs.Jig3Detect,
                _devices.Inputs.Jig4Detect,
            };
            Outputs = new ObservableCollection<IDOutput>
            {
                _devices.Outputs.VacChamberOpen,
                _devices.Outputs.VacChamberClose,
            };
        }

        #region Privates
        private readonly Devices _devices;
        #endregion
    }
}