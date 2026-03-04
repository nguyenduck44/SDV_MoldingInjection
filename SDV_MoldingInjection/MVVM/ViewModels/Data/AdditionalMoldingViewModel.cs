using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AdditionalMoldingViewModel : ViewModelBase
    {
        private readonly IConfiguration _configuration;
        private readonly RecipeSelector _recipeSelector;
        private string recipeFolder => _configuration.GetValue<string>("Folders:RecipeFolder") ?? "";

        public RecipeList RecipeList { get; }
        public MachineStatus MachineStatus { get; }

        public AdditionalMoldingViewModel(IConfiguration configuration,
            RecipeSelector recipeSelector,
            RecipeList recipeList,
            MachineStatus machineStatus)
        {
            _configuration = configuration;
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
