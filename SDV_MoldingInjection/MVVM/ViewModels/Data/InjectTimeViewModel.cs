using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class InjectTimeViewModel : ViewModelBase
    {
        private readonly RecipeSelector _recipeSelector;

        public RecipeList RecipeList { get; }
        public MachineStatus MachineStatus { get; }

        public InjectTimeViewModel(
            RecipeSelector recipeSelector,
            RecipeList recipeList,
            MachineStatus machineStatus)
        {
            _recipeSelector = recipeSelector;
            RecipeList = _recipeSelector.CurrentRecipe;
            MachineStatus = machineStatus;
        }

        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_SaveAllData"]) == true)
                    {
                        _recipeSelector.Save();
                        OnPropertyChanged();
                    }
                });
            }
        }
    }
}
