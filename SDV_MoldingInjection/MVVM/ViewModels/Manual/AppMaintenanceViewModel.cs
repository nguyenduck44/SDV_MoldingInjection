using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Recipe;
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

        #region Privates
        private readonly RecipeSelector _recipeSelector;
        private readonly Devices _devices;
        #endregion
    }
}