using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_DotDispenser.MVVM.ViewModels.Teaching;
using SDV_DotDispenser.Process;
using SDV_DotDispenser.Services.Factories;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SDV_DotDispenser.MVVM.ViewModels
{
    public class TeachViewModel : ViewModelBase
    {
        #region Properties
        public MachineStatus MachineStatus { get; }

        public ObservableCollection<UnitTeachingViewModel> TeachingUnits { get; }

        public UnitTeachingViewModel SelectedTeachingUnit
        {
            get 
            {
                return selectedTeachingUnit; 
            }
            set 
            {
                selectedTeachingUnit = value;
                OnPropertyChanged(nameof(SelectedTeachingUnit));
            }
        }
        #endregion

        public void SelectedUnitTeachingOnChanged()
        {
            OnPropertyChanged(nameof(SelectedTeachingUnit));
        }

        public ICommand GetTeachingUnitViewModel
        {
            get
            {
                return new RelayCommand<object>((teachingUnit) =>
                {
                    if(teachingUnit is UnitTeachingViewModel unitTeachingViewModel)
                    {
                        if (SelectedTeachingUnit.Name == unitTeachingViewModel.Name) return;

                        SelectedTeachingUnit.IsSelected = false;
                        SelectedTeachingUnit = unitTeachingViewModel;
                        SelectedTeachingUnit.IsSelected = true;
                        SelectedUnitTeachingOnChanged();
                    }
                });
            }
        }

        public TeachViewModel(MachineStatus machineStatus,
            NavigationStore navigationStore,
            TeachingViewModelFactory factory)
        {
            MachineStatus = machineStatus;
            _navigationStore = navigationStore;

            TeachingUnits = new ObservableCollection<UnitTeachingViewModel>()
            {
                factory.Create("CST Load"),
                factory.Create("CST Unload"),
            };

            SelectedTeachingUnit = TeachingUnits.First();
            SelectedTeachingUnit.IsSelected = true;

            _inoutUpdateTimer = new System.Timers.Timer(100);
            _inoutUpdateTimer.Elapsed += _inoutUpdateTimer_Elapsed;
            _inoutUpdateTimer.Start();
        }

        private void _inoutUpdateTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel.GetType() != typeof(TeachViewModel)) return;

            if (SelectedTeachingUnit == null) return;
            if (SelectedTeachingUnit.Inputs == null) return;
            if (SelectedTeachingUnit.Inputs.Count <= 0) return;

            foreach (var input in SelectedTeachingUnit.Inputs)
            {
                input.RaiseValueUpdated();
            }
        }

        #region Privates
        private readonly NavigationStore _navigationStore;
        private System.Timers.Timer _inoutUpdateTimer;
        private UnitTeachingViewModel selectedTeachingUnit;
        #endregion
    }
}
