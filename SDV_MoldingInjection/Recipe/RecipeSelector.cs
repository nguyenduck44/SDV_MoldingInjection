using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using EQX.UI.Language;
using log4net;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDV_MoldingInjection.MVVM.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using TOPENG_Device;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipeSelector : ObservableObject
    {
        public string GetRecipeFolderPath(string recipeName)
        {
            return Path.Combine(recipeFolder, recipeName);
        }

        #region Privates
        private RecipeSetting recipeSetting;
        private readonly IConfiguration _configuration;

        private readonly IAlertService _alarmService;
        private readonly IAlertService _warningService;
        private readonly ILanguageService _languageService;
        private readonly NavigationStore _navigationStore;
        private readonly ObservableCollection<string> _validRecipes = new ObservableCollection<string>();
        private string recipeFolder => _configuration.GetValue<string>("Folders:RecipeFolder") ?? "";
        #endregion

        #region Properties
        public string[] AllRecipe;
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
            ILanguageService languageService, NavigationStore navigationStore)
        {
            _configuration = configuration;
            recipeSetting = new RecipeSetting();
            _alarmService = alarmService;
            _warningService = warningService;
            _languageService = languageService;
            _navigationStore = navigationStore;
            UpdateValidRecipes();

            CurrentRecipe = new RecipeList();
            CurrentRecipe.CommonRecipe.SelectedLanguageEvent += CommonRecipe_SelectedLanguageEvent;

            CurrentRecipe.CommonRecipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.InjectRecipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.DryPumpRecipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.SPDHead1_Recipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.SPDHead2_Recipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.SPDHead3_Recipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.SPDHead4_Recipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.AdditionalMolding_Recipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.OptionRecipe.RecipeChanged += SingleRecipe_RecipeChanged;
        }

        private void SingleRecipe_RecipeChanged(object oldValue, object newValue, string? propertyName = null)
        {
            if (_navigationStore.CurrentViewModel.GetType() == typeof(InitDeinitViewModel)) return;

            bool result1 = CIMHelpers.TryParseRecipeNumber(RecipeSetting.CurrentRecipe, out int index);
            if (result1 == false)
            {
                throw new Exception("PPID Name format is not match");
            }

            var parameterArea = new ParameterWordArea
            {
                PPIDName = RecipeSetting.CurrentRecipe,
            };
            parameterArea.Parameters[0] = CurrentRecipe.CommonRecipe.LogSaveDay;
            EquipEventHelpers.ParameterChange(parameterArea, index);

            LogManager.GetLogger("Data").Info($"{propertyName} value updated : {oldValue} -> {newValue}");
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
                MessageBoxEx.Show($"{recipeSettingFile} file not found");
                return false;
            }

            string currentRecipeFileContain = File.ReadAllText(recipeSettingFile);

            try
            {
                RecipeSetting = JsonConvert.DeserializeObject<RecipeSetting>(currentRecipeFileContain);
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message);
                return false;
            }

            // 2. Get Current Recipe
            string currentRecipeFolder = Path.Combine(recipeFolder, RecipeSetting.CurrentRecipe);
            string currentRecipeFile = Path.Combine(currentRecipeFolder, "Recipe.json");
            if (Directory.Exists(currentRecipeFolder) == false)
                MessageBoxEx.Show($" Recipe folder \"{currentRecipeFolder}\" not found");

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
                    EQPPPIDArea ppipArea = new EQPPPIDArea
                    {
                        EQPPPID = RecipeSetting.CurrentRecipe,
                    };
                    EquipEventDetail.Create(EquipEvent.EQPPIDUpdate).Write(ppipArea.ToCIMData());
                }
                else
                {
                    MessageBoxEx.Show($"Failed to load recipe file: {currentRecipeFile}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show($"Error loading recipe: {ex.Message}");
                return false;
            }
            return true;
        }

        public void Create(string newRecipe, string cimRecipeNumber = "")
        {
            string createFolder = Path.Combine(recipeFolder, newRecipe);
            if (Directory.Exists(createFolder))
            {
                MessageBoxEx.Show($"Recipe folder \"{createFolder}\" exist.");
                return;
            }

            try
            {
                Directory.CreateDirectory(createFolder);

                // TODO: Update recipe
                foreach (var file in Directory.GetFiles(createFolder))
                {
                    string destFile = Path.Combine(createFolder, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }

                EquipEventHelpers.PPIDCreate(Path.GetFileName(createFolder), cimRecipeNumber);
                
                UpdateValidRecipes(); // Refresh the list to show the new copied recipe

                MessageBoxEx.Show($"Recipe '{createFolder}' copied successfully to '{Path.GetFileName(createFolder)}'.");
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show($"Failed to copy recipe: {ex.Message}");
            }
        }

        public void Delete(string selectedRecipe, string fromCIMRecipeNumber = "")
        {
            if (string.IsNullOrEmpty(fromCIMRecipeNumber))
            {
                bool? confirm = MessageBoxEx.ShowDialog($"DO you realy want to DELETE RECIPE {selectedRecipe}");

                if (confirm != true) return;
            }

            string CopyRecipe = (string)Application.Current.Resources["str_CopyRecipe"];
            if (selectedRecipe == RecipeSetting.CurrentRecipe)
            {
                MessageBoxEx.ShowDialog($"Can not delete CURRENT RECIPE {selectedRecipe}");
                return;
            }
            string folderPath = GetRecipeFolderPath(selectedRecipe);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);

                UpdateValidRecipes();

                EquipEventHelpers.PPIDListSinglePPIDWrite(selectedRecipe, true);
                EquipEventHelpers.PPIDDelete(selectedRecipe, fromCIMRecipeNumber);
                EquipEventHelpers.PPIDListSinglePPIDWrite(selectedRecipe, true);
            }
        }

        // FROM EQUIPMENT ONLY
        public void Copy(string selectedRecipe)
        {
            if (string.IsNullOrEmpty(selectedRecipe)) return;

            string sourceFolder = Path.Combine(recipeFolder, selectedRecipe);
            if (!Directory.Exists(sourceFolder))
            {
                MessageBoxEx.Show($"Recipe folder \"{sourceFolder}\" not found.");
                return;
            }

            string destinationFolder = $"D:\\MoldInjection\\Recipe\\TT_094_MODEL094";
            if (Directory.Exists(destinationFolder))
            {
                MessageBoxEx.Show($"Recipe folder \"{destinationFolder}\" already exists.");
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
                MessageBoxEx.Show($"Recipe '{selectedRecipe}' copied successfully to '{Path.GetFileName(destinationFolder)}'.");
                UpdateValidRecipes(); // Refresh the list to show the new copied recipe

                // TODO: WRITE PARAMETER DATA TO RMS AREA
                EquipEventHelpers.ParameterWordSingleParameterUpdate(1, CurrentRecipe.CommonRecipe.LogSaveDay);

                bool result1 = CIMHelpers.TryParseRecipeNumber(Path.GetFileName(destinationFolder), out int index);
                if (result1 == false)
                {
                    throw new Exception("PPID Name format is not match");
                }

                EquipEventHelpers.PPIDCreate(Path.GetFileName(destinationFolder));
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show($"Failed to copy recipe: {ex.Message}");
            }
        }

        public void Save()
        {
            lock (_locker)
            {
                string currentRecipeFolder = Path.Combine(recipeFolder, RecipeSetting.CurrentRecipe);
                string currentRecipeFile = Path.Combine(currentRecipeFolder, "Recipe.json");
                if (Directory.Exists(currentRecipeFolder) == false)
                    MessageBoxEx.Show($" Recipe folder \"{currentRecipeFolder}\" not found");

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                string serializeStr = JsonConvert.SerializeObject(CurrentRecipe, Formatting.Indented, settings);
                File.WriteAllText(currentRecipeFile, serializeStr);
            }
        }

        public void UpdateValidRecipes()
        {
            Application.Current.Dispatcher.Invoke(() => {
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

                    AllRecipe = _validRecipes.ToArray();
                    OnPropertyChanged(nameof(ValidRecipes));
                }
                catch (Exception ex) { MessageBoxEx.Show($"Failed to update recipe list: {ex.Message}"); }
            });
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

        private readonly object _locker = new object();
    }
}
