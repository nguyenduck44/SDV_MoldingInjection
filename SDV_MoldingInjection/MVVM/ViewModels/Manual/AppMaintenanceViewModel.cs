using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
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
                    _recipeSelector.Save();
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
                });
            }
        }
        #endregion

        #region Constructor(s)
        public AppMaintenanceViewModel(NavigationStore navigationStore,
           MachineStatus machineStatus, RecipeSelector recipeSelector)
           : base(navigationStore, machineStatus)
        {
            _recipeSelector = recipeSelector;
        }
        #endregion

        protected override bool ConfirmSemiSequence(string message)
        {
            return MessageBoxEx.ShowDialog(message, "Confirm") == true;
        }

        #region Privates
        private readonly RecipeSelector _recipeSelector;
        #endregion
    }
}