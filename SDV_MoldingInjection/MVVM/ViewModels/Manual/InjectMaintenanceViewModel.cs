using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.TeachingPosition;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class InjectMaintenanceViewModel : AppMaintenanceViewModel
    {
        #region Properties
        public AutoTeachModeViewModel AutoTeachModeViewModel { get; }
        public InjectMaintenanceTeachingPosition InjectMaintenanceTeachingPosition { get; }
        #endregion

        public InjectMaintenanceViewModel(NavigationStore navigationStore,
            Devices devices, 
            MachineStatus machineStatus, 
            RecipeSelector recipeSelector,
            AutoTeachModeViewModel autoTeachModeViewModel,
            InjectMaintenanceTeachingPosition injectMaintenanceTeachingPosition)
            : base(navigationStore, machineStatus, recipeSelector, devices)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            AutoTeachModeViewModel = autoTeachModeViewModel;
            InjectMaintenanceTeachingPosition = injectMaintenanceTeachingPosition;
            if (GroupedPositions != null && GroupedPositions.Count > 0)
            {
                SelectedGroupedPosition = GroupedPositions.FirstOrDefault()!;
            }
        }

        protected override void ActualInit()
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
                _devices.Inputs.BelowsUp,
                _devices.Inputs.BelowsDown,
                _devices.Inputs.Nozzle1Clean,
                _devices.Inputs.Nozzle2Clean,
                _devices.Inputs.Nozzle3Clean,
                _devices.Inputs.Nozzle4Clean,

            };
            Outputs = new ObservableCollection<IDOutput>
            {
                _devices.Outputs.NozzleCleanH1H2,
                _devices.Outputs.NozzleCleanH3H4,
                _devices.Outputs.VacChamberOpen,
                _devices.Outputs.VacChamberClose,
            };
            Sequences = new ObservableCollection<ESemiSequence>
            {
                ESemiSequence.Loading,
                ESemiSequence.Unloading,
                ESemiSequence.IdlePurge,
            };
        }

        protected override RecipePositionManagerBase<RecipeList> UpdatePositionManager()
        {
            return InjectMaintenanceTeachingPosition.PositionManager;
        }

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;

        #endregion
    }
}