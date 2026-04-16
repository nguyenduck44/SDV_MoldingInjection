using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System.ComponentModel;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AppMaintenanceViewModel : MaintenanceViewModel<ESemiSequence, RecipeList>
    {
        #region Commands
        public ICommand TeachPositionSaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog($"Do you want to Save Position Teaching") == false) return;

                    _recipeSelector.Save();
                    CaptureTeachingSnapshot();
                });
            }
        }

        public ICommand TeachPositionCancelCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _recipeSelector.Load();
                    CaptureTeachingSnapshot();
                });
            }
        }
        #endregion

        #region Constructor(s)
        public AppMaintenanceViewModel(NavigationStore navigationStore,
           MachineStatus machineStatus, 
           RecipeSelector recipeSelector,
           Devices devices)
           : base(navigationStore, machineStatus)
        {
            _recipeSelector = recipeSelector;
            _devices = devices;
        }
        #endregion

        protected override RecipePositionManagerBase<RecipeList> UpdatePositionManager()
        {
            return new RecipePositionManager(_recipeSelector.CurrentRecipe, _devices.Motions.All);
        }

        protected override bool ConfirmSemiSequence(string message)
        {
            return MessageBoxEx.ShowDialog(message, "Confirm") == true;
        }

        protected override bool BeforeTeachingPositionSelectionChanged(MultiPointPosition? currentSelection, MultiPointPosition nextSelection)
        {
            if (currentSelection == null || currentSelection == nextSelection)
            {
                return true;
            }

            if (HasTeachingChanges() == false)
            {
                return true;
            }

            bool shouldSave = MessageBoxEx.ShowDialog("Position teaching has changed. Do you want to Save?", "Confirm") == true;
            if (shouldSave)
            {
                _recipeSelector.Save();
            }
            else
            {
                _recipeSelector.Load();
            }

            CaptureTeachingSnapshot();
            return true;
        }

        private bool HasTeachingChanges()
        {
            if (GroupedPositions == null)
            {
                return false;
            }

            const double epsilon = 1e-6;
            foreach (var group in GroupedPositions)
            {
                foreach (var point in group.Points)
                {
                    if (_teachingSnapshot.TryGetValue(point, out var snapshot) == false)
                    {
                        return true;
                    }

                    if (Math.Abs(snapshot.FixedPos - point.FixedPos) > epsilon ||
                        Math.Abs(snapshot.OffsetPos - point.OffsetPos) > epsilon)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void CaptureTeachingSnapshot()
        {
            _teachingSnapshot.Clear();
            if (GroupedPositions == null)
            {
                return;
            }

            foreach (var group in GroupedPositions)
            {
                foreach (var point in group.Points)
                {
                    _teachingSnapshot[point] = (point.FixedPos, point.OffsetPos);
                }
            }
        }

        protected override void WireTeachingPointEvents()
        {
            if (GroupedPositions == null)
            {
                return;
            }

            foreach (var group in GroupedPositions)
            {
                foreach (var point in group.Points)
                {
                    if (_hookedTeachingPoints.Add(point))
                    {
                        point.PropertyChanged += OnTeachingPointPropertyChanged;
                    }
                }
            }
        }

        private void OnTeachingPointPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isRestoringTeachingSnapshot || _isHandlingTeachingChange)
            {
                return;
            }

            if (sender is not PositionPoint point)
            {
                return;
            }

            if (e.PropertyName != nameof(PositionPoint.FixedPos) &&
                e.PropertyName != nameof(PositionPoint.OffsetPos))
            {
                return;
            }

            if (_teachingSnapshot.TryGetValue(point, out var oldValue) == false)
            {
                return;
            }

            const double epsilon = 1e-6;
            bool changed = Math.Abs(oldValue.FixedPos - point.FixedPos) > epsilon
                        || Math.Abs(oldValue.OffsetPos - point.OffsetPos) > epsilon;
            if (changed == false)
            {
                return;
            }

            _isHandlingTeachingChange = true;
            try
            {
                bool shouldSave = MessageBoxEx.ShowDialog("Position teaching has changed. Do you want to Save?", "Confirm") == true;
                if (shouldSave)
                {
                    _recipeSelector.Save();
                    CaptureTeachingSnapshot();
                }
                else
                {
                    RestoreTeachingSnapshot();
                }
            }
            finally
            {
                _isHandlingTeachingChange = false;
            }
        }

        private void RestoreTeachingSnapshot()
        {
            _isRestoringTeachingSnapshot = true;
            try
            {
                foreach (var snapshot in _teachingSnapshot)
                {
                    snapshot.Key.FixedPos = snapshot.Value.FixedPos;
                    snapshot.Key.OffsetPos = snapshot.Value.OffsetPos;
                }
            }
            finally
            {
                _isRestoringTeachingSnapshot = false;
            }
        }

        #region Privates
        protected readonly RecipeSelector _recipeSelector;
        protected readonly Devices _devices;
        private readonly Dictionary<PositionPoint, (double FixedPos, double OffsetPos)> _teachingSnapshot = new();
        private readonly HashSet<PositionPoint> _hookedTeachingPoints = new();
        private bool _isRestoringTeachingSnapshot;
        private bool _isHandlingTeachingChange;
        #endregion
    }
}