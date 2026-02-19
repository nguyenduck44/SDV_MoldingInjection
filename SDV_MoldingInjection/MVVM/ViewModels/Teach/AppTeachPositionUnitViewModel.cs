using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Helpers;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using EQX.UI.MVVM.ViewModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AppTeachPositionUnitViewModel : TeachPositionUnitViewModel<ESequence>
    {
        private readonly RecipeSelector _recipeSelector;
        private readonly IConfiguration _configuration;
        
        private string recipeFolder => _configuration.GetValue<string>("Folders:RecipeFolder") ?? "";

        public AppTeachPositionUnitViewModel(RecipeSelector recipeSelector,
            IConfiguration configuration)
        {
            _recipeSelector = recipeSelector;
            _configuration = configuration;
        }

        public ICommand SaveRecipeCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog("Do you want to Save?", (string)Application.Current.Resources["str_Confirm"]) == false)
                    {
                        return;
                    }
                    string currentRecipeFolder = Path.Combine(recipeFolder, _recipeSelector.RecipeSetting.CurrentRecipe);
                    if (Directory.Exists(currentRecipeFolder) == false)
                    {
                        Directory.CreateDirectory(currentRecipeFolder);
                    }

                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    };

                    PropertyInfo[] properties = _recipeSelector.CurrentRecipe.GetType().GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        var value = property.GetValue(_recipeSelector.CurrentRecipe, null);

                        if (value is IRecipe recipe)
                        {
                            foreach (IRecipe currentRecipe in Recipes)
                            {
                                if (currentRecipe.Name == recipe.Name)
                                {
                                    string propertyFile = Path.Combine(currentRecipeFolder, $"{property.Name}.json");
                                    string serializeStr = JsonConvert.SerializeObject(currentRecipe, Formatting.Indented, settings);
                                    File.WriteAllText(propertyFile, serializeStr);
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
