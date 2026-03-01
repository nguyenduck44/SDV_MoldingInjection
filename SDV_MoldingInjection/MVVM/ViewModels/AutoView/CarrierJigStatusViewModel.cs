using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class CarrierJigStatusViewModel : ViewModelBase
    {
        public CarrierJigStatusViewModel(RecipeSelector recipeSelector, Processes processes)
        {
            var processByName = processes.All.ToDictionary(p => p.Name);
            Heads = new ObservableCollection<CarrierJigHeadItemViewModel>
            {
                new CarrierJigHeadItemViewModel("H1", recipeSelector.CurrentRecipe.SPDHead1_Recipe, recipeSelector.Save, processByName.GetValueOrDefault(EProcess.SPDHead1.ToString())),
                new CarrierJigHeadItemViewModel("H2", recipeSelector.CurrentRecipe.SPDHead2_Recipe, recipeSelector.Save, processByName.GetValueOrDefault(EProcess.SPDHead2.ToString())),
                new CarrierJigHeadItemViewModel("H3", recipeSelector.CurrentRecipe.SPDHead3_Recipe, recipeSelector.Save, processByName.GetValueOrDefault(EProcess.SPDHead3.ToString())),
                new CarrierJigHeadItemViewModel("H4", recipeSelector.CurrentRecipe.SPDHead4_Recipe, recipeSelector.Save, processByName.GetValueOrDefault(EProcess.SPDHead4.ToString()))
            };
        }

        public ObservableCollection<CarrierJigHeadItemViewModel> Heads { get; }

        public void UpdateRuntimeStatus()
        {
            foreach (var head in Heads)
            {
                head.UpdateRuntimeStatus();
            }
        }
    }

    public class CarrierJigHeadItemViewModel : ObservableObject
    {
        public CarrierJigHeadItemViewModel(string name, SPDHeadRecipe recipe, Action saveRecipe, object? headProcess)
        {
            Name = name;
            _recipe = recipe;
            _saveRecipe = saveRecipe;
            _headProcess = headProcess as SPDHeadProcess;
            _recipe.PropertyChanged += Recipe_PropertyChanged;
            ToggleHeadSkipCommand = new RelayCommand(() => HeadSkip = !HeadSkip);
        }

        public string Name { get; }
        public SPDHeadRecipe Recipe => _recipe;
        public double ResinWeight => _recipe.ResinWeight;
        public double ResinTimer => _resinTimer;
        public double PAxisInjectVel => _pAxisInjectVel;
        public bool HeadSkip
        {
            get => _recipe.HeadSkip;
            set
            {
                if (_recipe.HeadSkip == value) return;
                _recipe.HeadSkip = value;
                OnPropertyChanged();
            }
        }
        public IRelayCommand ToggleHeadSkipCommand { get; }
        private void Recipe_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SPDHeadRecipe.ResinWeight))
            {
                OnPropertyChanged(nameof(ResinWeight));
            }
            else if (e.PropertyName == nameof(SPDHeadRecipe.InjectTime))
            {
            }
            else if (e.PropertyName == nameof(SPDHeadRecipe.HeadSkip))
            {
                OnPropertyChanged(nameof(HeadSkip));
                _saveRecipe();
            }
        }

        public void UpdateRuntimeStatus()
        {
            SetResinTimer(_headProcess?.InjectSequenceElapsedSeconds ?? 0);
            SetPAxisInjectVel(_headProcess?.PAxisInjectVelocity ?? 0);
        }

        private void SetResinTimer(double value)
        {
            double rounded = Math.Round(value, 1);
            if (Math.Abs(_resinTimer - rounded) < 0.05) return;
            _resinTimer = rounded;
            OnPropertyChanged(nameof(ResinTimer));
        }

        private void SetPAxisInjectVel(double value)
        {
            double rounded = Math.Round(value, 1);
            if (Math.Abs(_pAxisInjectVel - rounded) < 0.05) return;
            _pAxisInjectVel = rounded;
            OnPropertyChanged(nameof(PAxisInjectVel));
        }

        private readonly SPDHeadRecipe _recipe;
        private readonly Action _saveRecipe;
        private readonly SPDHeadProcess? _headProcess;
        private double _resinTimer;
        private double _pAxisInjectVel;
    }
}
