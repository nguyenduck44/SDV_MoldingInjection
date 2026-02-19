using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Process;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class SPDHeadMaintenanceViewModel : MaintenanceViewModel<ESequence>
    {
        public SPDHeadMaintenanceViewModel(Devices devices, NavigationStore navigationStore)
            : base(navigationStore)
        {
            _devices = devices;

            Motions = new ObservableCollection<IMotion>();
        }

        override public void Init()
        {
            RelatedViewModel = new List<string>
            {
                nameof(EProcess.Inject),
            };
            Motions = new ObservableCollection<IMotion>
            {
                ZAxis,
                PAxis,
                GAxis,
            };
            Cylinders = new ObservableCollection<ICylinder>
            {
                PistonCyl
            };
            Sequences = new ObservableCollection<ESequence>
            {
                ESequence.ResinInject,
                ESequence.DummyShot,
                ESequence.NeedleCleaning,
                ESequence.BubbleRemove,
                ESequence.DotWeighting,
                ESequence.HeadAssemble,
                ESequence.HeadDisassemble,
            };
            if (Name == EProcess.SPDHead1.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H1_GateOpen,
                    _devices.Inputs.H1_GateClose,
                    _devices.Inputs.H1_AssembleCheck,
                    _devices.Inputs.H1_SyringeCheck,
                    _devices.Inputs.H1_SyringeAir,
                };
                Outputs = new ObservableCollection<IDOutput>
                {
                    _devices.Outputs.H1_CylUp,
                    _devices.Outputs.H1_CylDown,
                };
            }
            if (Name == EProcess.SPDHead2.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H2_GateOpen,
                    _devices.Inputs.H2_GateClose,
                    _devices.Inputs.H2_AssembleCheck,
                    _devices.Inputs.H2_SyringeCheck,
                    _devices.Inputs.H2_SyringeAir,
                };
                Outputs = new ObservableCollection<IDOutput>
                {
                    _devices.Outputs.H2_CylUp,
                    _devices.Outputs.H2_CylDown,
                };
            }

            if (Name == EProcess.SPDHead3.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H3_GateOpen,
                    _devices.Inputs.H3_GateClose,
                    _devices.Inputs.H3_AssembleCheck,
                    _devices.Inputs.H3_SyringeCheck,
                    _devices.Inputs.H3_SyringeAir,
                };
                Outputs = new ObservableCollection<IDOutput>
                {
                    _devices.Outputs.H3_CylUp,
                    _devices.Outputs.H3_CylDown,
                };
            }

            if (Name == EProcess.SPDHead4.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H4_GateOpen,
                    _devices.Inputs.H4_GateClose,
                    _devices.Inputs.H4_AssembleCheck,
                    _devices.Inputs.H4_SyringeCheck,
                    _devices.Inputs.H4_SyringeAir,
                };
                Outputs = new ObservableCollection<IDOutput>
                {
                    _devices.Outputs.H4_CylUp,
                    _devices.Outputs.H4_CylDown,
                };
            }
        }

        #region Privates
        private readonly Devices _devices;

        private IMotion ZAxis => Name switch
        {
            "SPDHead1" => _devices.Motions.Z1Axis,
            "SPDHead2" => _devices.Motions.Z2Axis,
            "SPDHead3" => _devices.Motions.Z3Axis,
            "SPDHead4" => _devices.Motions.Z4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IMotion PAxis => Name switch
        {
            "SPDHead1" => _devices.Motions.P1Axis,
            "SPDHead2" => _devices.Motions.P2Axis,
            "SPDHead3" => _devices.Motions.P3Axis,
            "SPDHead4" => _devices.Motions.P4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IMotion GAxis => Name switch
        {
            "SPDHead1" => _devices.Motions.G1Axis,
            "SPDHead2" => _devices.Motions.G2Axis,
            "SPDHead3" => _devices.Motions.G3Axis,
            "SPDHead4" => _devices.Motions.G4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private ICylinder PistonCyl => Name switch
        {
            "SPDHead1" => _devices.Cylinders.PistonCyl_H1,
            "SPDHead2" => _devices.Cylinders.PistonCyl_H2,
            "SPDHead3" => _devices.Cylinders.PistonCyl_H3,
            "SPDHead4" => _devices.Cylinders.PistonCyl_H4,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        #endregion
    }
}