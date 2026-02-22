using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using EQX.UI.Language;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipeSelector : ObservableObject
    {
        #region Privates
        private RecipeSetting recipeSetting;
        private readonly IConfiguration _configuration;

        private readonly IAlertService _alarmService;
        private readonly IAlertService _warningService;
        private readonly ILanguageService _languageService;

        private readonly ObservableCollection<string> _validRecipes = new ObservableCollection<string>();
        private string recipeFolder => _configuration.GetValue<string>("Folders:RecipeFolder") ?? "";
        #endregion

        #region Properties
        public ObservableCollection<string> ValidRecipes
        {
            get => _validRecipes;
        }

        public RecipeSetting RecipeSetting
        {
            get { return recipeSetting; }
            set
            {
                recipeSetting = value;
                OnPropertyChanged(nameof(RecipeSetting));
            }
        }

        public RecipeList CurrentRecipe { get; private set; }
        #endregion

        #region Constructor
        public RecipeSelector(IConfiguration configuration,
            [FromKeyedServices("AlarmService")] IAlertService alarmService,
            [FromKeyedServices("WarningService")] IAlertService warningService,
            ILanguageService languageService)
        {
            _configuration = configuration;
            recipeSetting = new RecipeSetting();
            _alarmService = alarmService;
            _warningService = warningService;
            _languageService = languageService;

            UpdateValidRecipes();

            CurrentRecipe = new RecipeList();
            CurrentRecipe.CommonRecipe.SelectedLanguageEvent += CommonRecipe_SelectedLanguageEvent; ;
        }

        private void CommonRecipe_SelectedLanguageEvent(ILanguageDefinition obj)
        {
            _languageService.SwitchLanguage(obj.Id);
            _alarmService.Update();
            _warningService.Update();
        }
        #endregion

        #region Methods
        public bool Load()
        {
            // 1. Get Current Recipe
            string recipeSettingFile = Path.Combine(recipeFolder, "RecipeSetting.json");
            if (File.Exists(recipeSettingFile) == false)
            {
                MessageBox.Show($"{recipeSettingFile} file not found");
                return false;
            }

            string currentRecipeFileContain = File.ReadAllText(recipeSettingFile);

            try
            {
                RecipeSetting = JsonConvert.DeserializeObject<RecipeSetting>(currentRecipeFileContain);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            // 2. Get Current Recipe
            string currentRecipeFolder = Path.Combine(recipeFolder, RecipeSetting.CurrentRecipe);
            string currentRecipeFile = Path.Combine(currentRecipeFolder, "Recipe.json");
            if (Directory.Exists(currentRecipeFolder) == false)
                MessageBox.Show($" Recipe folder \"{currentRecipeFolder}\" not found");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            try
            {
                RecipeList? backupRecipe = JsonConvert.DeserializeObject<RecipeList>(File.ReadAllText(currentRecipeFile), settings);
                if (backupRecipe != null)
                {
                    CurrentRecipe.CloneFrom(backupRecipe);
                }
                else
                {
                    MessageBox.Show($"Failed to load recipe file: {currentRecipeFile}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipe: {ex.Message}");
                return false;
            }
            return true;
        }

        public void Copy(string selectedRecipe)
        {
            string sourceFolder = Path.Combine(recipeFolder, selectedRecipe);
            if (!Directory.Exists(sourceFolder))
            {
                MessageBox.Show($"Recipe folder \"{sourceFolder}\" not found.");
                return;
            }

            string destinationFolder = $"{sourceFolder}_Copy";
            if (Directory.Exists(destinationFolder))
            {
                MessageBox.Show($"Recipe folder \"{destinationFolder}\" already exists.");
                return;
            }

            try
            {
                Directory.CreateDirectory(destinationFolder);

                foreach (var file in Directory.GetFiles(sourceFolder))
                {
                    string destFile = Path.Combine(destinationFolder, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
                MessageBox.Show($"Recipe '{selectedRecipe}' copied successfully to '{Path.GetFileName(destinationFolder)}'.");
                UpdateValidRecipes(); // Refresh the list to show the new copied recipe
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy recipe: {ex.Message}");
            }
        }

        public void Save()
        {
            string currentRecipeFolder = Path.Combine(recipeFolder, RecipeSetting.CurrentRecipe);
            string currentRecipeFile = Path.Combine(currentRecipeFolder, "Recipe.json");
            if (Directory.Exists(currentRecipeFolder) == false)
                MessageBox.Show($" Recipe folder \"{currentRecipeFolder}\" not found");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            string serializeStr = JsonConvert.SerializeObject(CurrentRecipe, Formatting.Indented, settings);
            File.WriteAllText(currentRecipeFile, serializeStr);
        }

        public void UpdateValidRecipes()
        {
            try
            {
                if (!Directory.Exists(recipeFolder))
                {
                    _validRecipes.Clear();
                    return;
                }

                var onDiskRecipes = Directory.GetDirectories(recipeFolder, "*", SearchOption.TopDirectoryOnly)
                                             .Select(Path.GetFileName)
                                             .Where(name => name != null)
                                             .ToList();

                _validRecipes.Clear();
                foreach (var recipe in onDiskRecipes.OrderBy(r => r))
                {
                    _validRecipes.Add(recipe!);
                }

                OnPropertyChanged(nameof(ValidRecipes));
            }
            catch (Exception ex) { MessageBox.Show($"Failed to update recipe list: {ex.Message}"); }
        }
        public void SetCurrentModel(string selectedRecipe)
        {
            string selectedRecipeFolder = Path.Combine(recipeFolder, selectedRecipe);
            if (Directory.Exists(selectedRecipeFolder) == false)
            {
                MessageBoxEx.ShowDialog($"{selectedRecipeFolder} folder not exits");
                return;
            }
            string tempRecipe = RecipeSetting.CurrentRecipe;
            RecipeSetting.CurrentRecipe = selectedRecipe;
            File.WriteAllText((Path.Combine(recipeFolder, "RecipeSetting.json")), JsonConvert.SerializeObject(RecipeSetting));
            if (Load() == false)
            {
                RecipeSetting.CurrentRecipe = tempRecipe;
                File.WriteAllText((Path.Combine(recipeFolder, "RecipeSetting.json")), JsonConvert.SerializeObject(RecipeSetting));
            }
        }
        #endregion
    }
}
