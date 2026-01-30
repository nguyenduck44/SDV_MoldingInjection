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

namespace SDV_DotDispenser.Recipe
{
    public class RecipeSelector : ObservableObject
    {
        #region Privates
        private RecipeSetting recipeSetting;
        private readonly IConfiguration _configuration;
        private ObservableCollection<string> validRecipes;

        //[Minh]
        private readonly IAlertService _alarmService;
        private readonly IAlertService _warningService;
        private readonly ILanguageService _languageService;

        //--------------------------------------------
        private string recipeFolder => _configuration.GetValue<string>("Folders:RecipeFolder") ?? "";
        #endregion

        #region Properties
        public ObservableCollection<string> ValidRecipes
        {
            get
            {
                return UpdateValidRecipes();
            }
            set
            {
                validRecipes = value;
                OnPropertyChanged(nameof(ValidRecipes));
            }
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
            RecipeList currentRecipe,
            [FromKeyedServices("AlarmService")] IAlertService alarmService,
            [FromKeyedServices("WarningService")] IAlertService warningService,
            ILanguageService languageService)
        {
            _configuration = configuration;
            CurrentRecipe = currentRecipe;
            recipeSetting = new RecipeSetting();
            _alarmService = alarmService;
            _warningService = warningService;
            _languageService = languageService;

            currentRecipe.CommonRecipe.SelectedLanguageEvent += CommonRecipe_SelectedLanguageEvent; ;
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
                RecipeList backupRecipe = JsonConvert.DeserializeObject<RecipeList>(File.ReadAllText(currentRecipeFile), settings);
                PropertyInfo[] properties = CurrentRecipe.GetType().GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    try
                    {
                        ((IRecipe)property.GetValue(CurrentRecipe, null)).Clone((IRecipe)property.GetValue(backupRecipe, null));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public void Copy(string selectedRecipe)
        {
            string recipeFolder = _configuration.GetValue<string>("Folders:RecipeFolder");

            string currentRecipeFolder = Path.Combine(recipeFolder, selectedRecipe);
            string currentRecipeFile = Path.Combine(currentRecipeFolder, "Recipe.json");
            if (Directory.Exists(currentRecipeFolder) == false)
                MessageBox.Show($" Recipe folder \"{currentRecipeFolder}\" not found");
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            RecipeList backupRecipe = JsonConvert.DeserializeObject<RecipeList>(File.ReadAllText(currentRecipeFile), settings);
            if (Directory.Exists($"{currentRecipeFolder}_Copy") == false)
            {
                Directory.CreateDirectory($"{currentRecipeFolder}_Copy");

                foreach (var file in Directory.GetFiles(currentRecipeFolder))
                {
                    string fileSource = Path.Combine(currentRecipeFolder, Path.GetFileName(file));
                    string fileBackup = Path.Combine($"{currentRecipeFolder}_Copy", Path.GetFileName(file));

                    File.Copy(fileSource, fileBackup, true);
                }
            }
            else
            {
                MessageBox.Show($" Recipe folder \"$\"{currentRecipeFolder}_copy\"\" exists");
            }
        }

        public void Save()
        {
            string recipeFolder = _configuration.GetValue<string>("Folders:RecipeFolder");

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

        public ObservableCollection<string> UpdateValidRecipes()
        {
            ObservableCollection<string> validRecipes = new ObservableCollection<string>();
            List<string> result = Directory.GetDirectories(recipeFolder, "", SearchOption.TopDirectoryOnly)
                .Select(d => new DirectoryInfo(d).Name)
                .ToList();
            foreach (var model in result)
            {
                validRecipes.Add(model);
            }

            return validRecipes;
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
