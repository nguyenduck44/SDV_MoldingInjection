using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using EQX.UI.Language;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SDV_MoldingInjection.MVVM.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using TOPENG_Device;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipeSelector : ObservableObject
    {
        #region Privates
        private RecipeSetting recipeSetting;
        private readonly IConfiguration _configuration;
        private readonly RecipeCatalog _recipeCatalog;
        private readonly ObservableCollection<string> _validRecipes = new ObservableCollection<string>();
        private readonly ObservableCollection<RecipeInfo> _validRecipeInfos = new ObservableCollection<RecipeInfo>();
        private int _loadingDepth;

        private readonly IAlertService _alarmService;
        private readonly IAlertService _warningService;
        private readonly ILanguageService _languageService;
        private readonly NavigationStore _navigationStore;
        private bool isChangingModel { get; set; } = false;

        private object lockObj = new object();
        private string recipeFolder => _configuration.GetValue<string>("Folders:RecipeFolder") ?? "";
        #endregion

        #region Properties
        public string[] AllRecipe;
        public ObservableCollection<string> ValidRecipes => _validRecipes;
        public ObservableCollection<RecipeInfo> ValidRecipeInfos => _validRecipeInfos;
        public string CurrentRecipeDisplayName => _recipeCatalog.ToDisplayName(RecipeSetting.CurrentRecipe);
        public bool IsLoadingRecipe => _loadingDepth > 0;

        public int CurrentRecipeNumber
        {
            get
            {
                string selectedRecipe = ResolveRecipeName(RecipeSetting.CurrentRecipe);
                if (_recipeCatalog.TryGetRecipeId(selectedRecipe, out int recipeId) == false)
                {
                    throw new Exception("PPID Name format is not match");
                }

                return recipeId;
            }
        }

        public RecipeSetting RecipeSetting
        {
            get { return recipeSetting; }
            set
            {
                if (recipeSetting != null)
                {
                    recipeSetting.PropertyChanged -= RecipeSetting_PropertyChanged;
                }

                recipeSetting = value;
                recipeSetting.PropertyChanged += RecipeSetting_PropertyChanged;
                OnPropertyChanged(nameof(RecipeSetting));
                OnPropertyChanged(nameof(CurrentRecipeDisplayName));
            }
        }

        public RecipeList CurrentRecipe { get; private set; }
        public event Action? CurrentModelChanged;
        public ParameterWordArea ParameterWordArea { get; }
        #endregion

        #region Constructor
        public RecipeSelector(IConfiguration configuration,
            RecipeList currentRecipe,
            RecipeCatalog recipeCatalog,
            [FromKeyedServices("AlarmService")] IAlertService alarmService,
            [FromKeyedServices("WarningService")] IAlertService warningService,
            ILanguageService languageService, NavigationStore navigationStore)
        {
            _configuration = configuration;
            _recipeCatalog = recipeCatalog;
            CurrentRecipe = currentRecipe;
            recipeSetting = new RecipeSetting();
            _alarmService = alarmService;
            _warningService = warningService;
            _languageService = languageService;
            _navigationStore = navigationStore;

            UpdateValidRecipes();

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
            CurrentRecipe.InjectTimeRecipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.IdlePurgeRecipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.CylinderDelayTimeRecipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.CDASettingRecipe.RecipeChanged += SingleRecipe_RecipeChanged;
            CurrentRecipe.MotionSpeedRecipe.RecipeChanged += SingleRecipe_RecipeChanged;

            ParameterWordArea = new ParameterWordArea();
        }

        private bool isUpdateParameter2CIM { get; set; } = false;
        private async void SingleRecipe_RecipeChanged(object? oldValue, object? newValue, string? propertyName = null, int parameterIndex = -1, PropertyInfo? propertyInfo = null)
        {
            if (_navigationStore.CurrentViewModel.GetType() != typeof(InitDeinitViewModel))
            {
                LogManager.GetLogger("Data").Info($"{propertyName} value changed {oldValue} -> {newValue}");
            }

            string selectedRecipe = ResolveRecipeName(RecipeSetting.CurrentRecipe);
            if (_recipeCatalog.TryGetRecipeId(selectedRecipe, out int recipeId) == false)
            {
                throw new Exception("PPID Name format is not match");
            }

            if (propertyInfo == null) return;

            if (parameterIndex > 0)
            {
                await Task.Run(async () =>
                {
                    while(isUpdateParameter2CIM)
                    {
                        await Task.Delay(5);
                    }

                    isUpdateParameter2CIM = true;
                    ParameterWordArea.PPIDName = selectedRecipe;

                    int value;

                    if (newValue == null) return;

                    if (newValue.GetType() == typeof(double) ||
                        newValue.GetType() == typeof(float))
                    {
                        value = (int)((double)newValue * 1000.0);
                    }
                    else if (newValue.GetType() == typeof(int))
                    {
                        value = (int)newValue;
                    }
                    else if ((newValue.GetType() == typeof(bool)))
                    {
                        value = (bool)newValue ? 1 : 0;
                    }
                    else
                    {
                        throw new Exception($"Typeof '{propertyName}' ('{newValue.GetType()}') is not supported.");
                    }

                    if (propertyInfo.GetCustomAttribute(typeof(ParameterDescriptionAttribute)) != null)
                    {
                        lock (lockObj)
                        {
                            ParameterWordArea.Parameters[parameterIndex - 1] = value;
                            EquipEventHelpers.ParameterChange(ParameterWordArea, recipeId);
                        }
                    }

                    else if (propertyInfo.GetCustomAttribute(typeof(ECMParameterDescriptionAttribute)) != null)
                    {
                        EquipEventHelpers.EquipmentConstantParameterChanged(parameterIndex, value);
                    }

                    isUpdateParameter2CIM = false;
                });
            }
        }

        private void CommonRecipe_SelectedLanguageEvent(ILanguageDefinition obj)
        {
            _languageService.SwitchLanguage(obj.Id);
            _alarmService.Update();
            _warningService.Update();
        }
        #endregion

        #region Methods
        public string GetRecipeFolderPath(string recipeName)
        {
            return Path.Combine(recipeFolder, ResolveRecipeFolderName(recipeName));
        }

        public bool Load()
        {
            BeginRecipeLoading();
            try
            {
                string recipeSettingFile = Path.Combine(recipeFolder, "RecipeSetting.json");
                if (File.Exists(recipeSettingFile) == false)
                {
                    MessageBox.Show($"{recipeSettingFile} file not found");
                    return false;
                }

                string currentRecipeFileContain = File.ReadAllText(recipeSettingFile);

                try
                {
                    RecipeSetting? loadedSetting = JsonConvert.DeserializeObject<RecipeSetting>(currentRecipeFileContain);
                    if (loadedSetting == null)
                    {
                        MessageBox.Show("Failed to load RecipeSetting.json");
                        return false;
                    }

                    RecipeSetting = loadedSetting;
                    RecipeSetting.CurrentRecipe = ResolveRecipeName(RecipeSetting.CurrentRecipe);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }

                string currentRecipeFolder = GetRecipeFolderPath(RecipeSetting.CurrentRecipe);
                string currentRecipeFile = Path.Combine(currentRecipeFolder, "Recipe.json");
                if (Directory.Exists(currentRecipeFolder) == false)
                {
                    MessageBox.Show($" Recipe folder \"{currentRecipeFolder}\" not found");
                }

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
                        NormalizeSelectedLanguageReference();
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
            finally
            {
                EndRecipeLoading();
            }
        }

        public void Create(string newRecipe, int recipeNumber, string cimRecipeNumberWithText = "")
        {
            string currentRecipeName = ResolveRecipeName(RecipeSetting.CurrentRecipe);
            string currentRecipeFolder = GetRecipeFolderPath(currentRecipeName);
            string newRecipeName = ResolveRecipeName(newRecipe);
            if (string.IsNullOrWhiteSpace(newRecipeName))
            {
                MessageBoxEx.Show("Recipe name is empty.");
                return;
            }

            if (_recipeCatalog.ContainsName(newRecipeName))
            {
                MessageBoxEx.Show($"Recipe name '{newRecipeName}' already exists in RecipeInfo.");
                return;
            }
            if (recipeNumber < 0)
            {
                MessageBoxEx.Show($"Recipe Id for '{newRecipeName}' is not valid.");
                return;
            }
            if (recipeNumber > 0 && _recipeCatalog.ContainsId(recipeNumber))
            {
                MessageBoxEx.Show($"Recipe Id '{recipeNumber}' already exists in RecipeInfo.");
                return;
            }

            string recipeFolderName = newRecipeName;
            string createRecipeFolder = Path.Combine(recipeFolder, recipeFolderName);
            if (Directory.Exists(createRecipeFolder))
            {
                MessageBox.Show($"Recipe folder \"{createRecipeFolder}\" exist.");
                return;
            }

            try
            {
                Directory.CreateDirectory(createRecipeFolder);

                CopyDirectory(currentRecipeFolder, createRecipeFolder);
                _recipeCatalog.WriteRecipeInfo(recipeFolderName, recipeNumber, newRecipeName);

                EquipEventHelpers.PPIDCreate(newRecipeName, recipeNumber, cimRecipeNumberWithText);

                UpdateValidRecipes(); // Refresh the list to show the new copied recipe

                MessageBoxEx.Show($"Recipe '{currentRecipeName}' copied successfully to '{newRecipeName}'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy recipe: {ex.Message}");
            }
        }

        public void Delete(string selectedRecipe, int? recipeNumber = null, string fromCIMRecipeNumberWithText = "")
        {
            selectedRecipe = ResolveRecipeName(selectedRecipe);
            if (_recipeCatalog.TryGetRecipeInfo(selectedRecipe, out RecipeInfo? recipeInfo) == false || recipeInfo == null)
            {
                MessageBoxEx.Show($"Recipe info for '{selectedRecipe}' not found.");
                return;
            }

            if (string.IsNullOrEmpty(fromCIMRecipeNumberWithText) || recipeNumber == null)
            {
                bool? confirm = MessageBoxEx.ShowDialog($"Do you really want to DELETE RECIPE {selectedRecipe}");

                if (confirm != true) return;
            }

            string CopyRecipe = (string)Application.Current.Resources["str_CopyRecipe"];
            if (selectedRecipe == RecipeSetting.CurrentRecipe)
            {
                MessageBoxEx.ShowDialog($"Can not delete CURRENT RECIPE {selectedRecipe}");
                return;
            }
            string folderPath = Path.Combine(recipeFolder, recipeInfo.FolderName);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);

                UpdateValidRecipes();

                EquipEventHelpers.PPIDListSinglePPIDWrite(selectedRecipe, recipeInfo.Id.ToString(), true);
                EquipEventHelpers.PPIDDelete(selectedRecipe, fromCIMRecipeNumberWithText);
                EquipEventHelpers.PPIDListSinglePPIDWrite(selectedRecipe, recipeInfo.Id.ToString(), true);
            }
        }

        public void Copy(string selectedRecipe, RecipeInfo recipeInfo)
        {
            string newRecipeName = recipeInfo.Name;
            if (string.IsNullOrWhiteSpace(newRecipeName))
            {
                MessageBoxEx.Show("Recipe name is empty.");
                return;
            }

            if (_recipeCatalog.ContainsName(newRecipeName))
            {
                MessageBoxEx.Show($"Recipe name '{newRecipeName}' already exists in RecipeInfo.");
                return;
            }
            //if (recipeInfo.Id <= 90)
            //{
            //    MessageBoxEx.Show($"Recipe Id for '{newRecipeName}' is not valid.");
            //    return;
            //}
            if (_recipeCatalog.ContainsId(recipeInfo.Id))
            {
                MessageBoxEx.Show($"Recipe Id '{recipeInfo.Id}' already exists in RecipeInfo.");
                return;
            }

            if (recipeInfo.Name.StartsWith("TT") == false && recipeInfo.Id > 90)
            {
                MessageBoxEx.Show($"Recipe ID > 90 Name for '{newRecipeName}' is not valid.");
                return;
            }

            selectedRecipe = ResolveRecipeName(selectedRecipe);
            string currentRecipeFolder = GetRecipeFolderPath(selectedRecipe);

            if (Directory.Exists(currentRecipeFolder) == false)
                MessageBox.Show($" Recipe folder \"{currentRecipeFolder}\" not found");

            string destinationFolder = Path.Combine(recipeFolder, recipeInfo.FolderName);

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (Directory.Exists(destinationFolder) == false)
            {
                Directory.CreateDirectory(destinationFolder);

                CopyDirectory(currentRecipeFolder, destinationFolder);

                string RecipeInfoFilePath = Path.Combine(destinationFolder, "RecipeInfo.json");

                File.WriteAllText(RecipeInfoFilePath, JsonConvert.SerializeObject(recipeInfo, Formatting.Indented, settings));

                UpdateValidRecipes(); // Refresh the list to show the new copied recipe

                // TODO: WRITE PARAMETER DATA TO RMS AREA
                EquipEventHelpers.ParameterWordSingleParameterUpdate(1, CurrentRecipe.CommonRecipe.LogSaveDay);

                // TODO : Temp disable
                //EquipEventHelpers.PPIDCreate(Path.GetFileName(destinationFolder));
            }
            else
            {
                MessageBox.Show($" Recipe folder \"$\"{currentRecipeFolder}_copy\"\" exists");
            }
        }

        public void Save()
        {
            string currentRecipeFolder = GetRecipeFolderPath(RecipeSetting.CurrentRecipe);
            string currentRecipeFile = Path.Combine(currentRecipeFolder, "Recipe.json");
            if (Directory.Exists(currentRecipeFolder) == false)
            {
                Directory.CreateDirectory(currentRecipeFolder);
            }

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            string serializeStr = JsonConvert.SerializeObject(CurrentRecipe, Formatting.Indented, settings);
            File.WriteAllText(currentRecipeFile, serializeStr);
        }

        public void UpdateValidRecipes()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _recipeCatalog.Refresh();
                    IReadOnlyList<RecipeInfo> recipes = _recipeCatalog.GetAll();

                    _validRecipes.Clear();
                    _validRecipeInfos.Clear();
                    foreach (RecipeInfo recipe in recipes)
                    {
                        _validRecipes.Add(recipe.Name);
                        _validRecipeInfos.Add(recipe);
                    }

                    AllRecipe = _validRecipes.ToArray();
                    OnPropertyChanged(nameof(ValidRecipes));
                    OnPropertyChanged(nameof(ValidRecipeInfos));
                }
                catch (Exception ex) { MessageBoxEx.Show($"Failed to update recipe list: {ex.Message}"); }
            });
        }

        public void SetCurrentModel(string selectedRecipe)
        {
            isChangingModel = true;

            selectedRecipe = ResolveRecipeName(selectedRecipe);
            string selectedRecipeFolder = GetRecipeFolderPath(selectedRecipe);
            if (Directory.Exists(selectedRecipeFolder) == false)
            {
                isChangingModel = false;
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
                Load();
                isChangingModel = false;
                return;
            }

            CurrentModelChanged?.Invoke();
            OnPropertyChanged(nameof(CurrentRecipeDisplayName));
            isChangingModel = false;
        }

        public void UpdateRecipeFromCIM(int[] parameters)
        {
            var recipeObjects = CurrentRecipe.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(RecipeBase).IsAssignableFrom(p.PropertyType))
                .Select(p => p.GetValue(CurrentRecipe))
                .OfType<RecipeBase>()
                .ToList();

            foreach (var recipeObject in recipeObjects)
            {
                PropertyInfo[] properties = recipeObject.GetType().GetProperties();

                foreach (var property in properties)
                {
                    foreach (var att in property.GetCustomAttributes())
                    {
                        if (att is ParameterDescriptionAttribute parameterDescriptionAttribute)
                        {
                            int parameterIndex = parameterDescriptionAttribute.Index - 1;
                            if (recipeObject is SPDHeadRecipe sPDHeadRecipe)
                            {
                                if (sPDHeadRecipe.Name == "SPDHead2_Recipe")
                                {
                                    parameterIndex += 40;
                                }
                                else if (sPDHeadRecipe.Name == "SPDHead3_Recipe")
                                {
                                    parameterIndex += 80;
                                }
                                else if (sPDHeadRecipe.Name == "SPDHead4_Recipe")
                                {
                                    parameterIndex += 120;
                                }
                            }

                            object newValue = parameters[parameterIndex];

                            if (property.PropertyType == typeof(double) ||
                                property.PropertyType == typeof(float))
                            {
                                property.SetValue(recipeObject, (Convert.ToDouble(newValue) / 1000.0));
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                property.SetValue(recipeObject, (int)newValue);
                            }
                            else if ((property.PropertyType == typeof(bool)))
                            {
                                property.SetValue(recipeObject, (int)newValue == 1);
                            }
                            else
                            {
                                throw new Exception($"Typeof '{property}' ('{property.PropertyType}') is not supported.");
                            }
                        }
                    }
                }
            }
        }

        public int[] GetParameters(string ppidName)
        {
            int[] data = new int[ParameterWordArea.PARAM_MAX_INT_COUNT];

            string currentRecipeFolder = GetRecipeFolderPath(ppidName);
            string currentRecipeFile = Path.Combine(currentRecipeFolder, "Recipe.json");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            try
            {
                RecipeList? backupRecipe = JsonConvert.DeserializeObject<RecipeList>(File.ReadAllText(currentRecipeFile), settings);

                if (backupRecipe == null) return data;

                var recipeObjects = backupRecipe.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => typeof(RecipeBase).IsAssignableFrom(p.PropertyType))
                    .Select(p => p.GetValue(CurrentRecipe))
                    .OfType<RecipeBase>()
                    .ToList();

                foreach (var recipeObject in recipeObjects)
                {
                    PropertyInfo[] properties = recipeObject.GetType().GetProperties();

                    foreach (var property in properties)
                    {
                        foreach (var att in property.GetCustomAttributes())
                        {
                            if (att is ParameterDescriptionAttribute parameterDescriptionAttribute)
                            {
                                int parameterIndex = parameterDescriptionAttribute.Index - 1;
                                if (recipeObject is SPDHeadRecipe sPDHeadRecipe)
                                {
                                    if (sPDHeadRecipe.Name == "SPDHead2_Recipe")
                                    {
                                        parameterIndex += 40;
                                    }
                                    else if (sPDHeadRecipe.Name == "SPDHead3_Recipe")
                                    {
                                        parameterIndex += 80;
                                    }
                                    else if (sPDHeadRecipe.Name == "SPDHead4_Recipe")
                                    {
                                        parameterIndex += 120;
                                    }
                                }

                                object newValue = property.GetValue(recipeObject);

                                if (newValue.GetType() == typeof(double) ||
                                    newValue.GetType() == typeof(float))
                                {
                                    data[parameterIndex - 1] = (int)(Convert.ToDouble(newValue) * 1000);
                                }
                                else if (newValue.GetType() == typeof(int))
                                {
                                    data[parameterIndex - 1] = Convert.ToInt32(newValue);
                                }
                                else if (newValue.GetType() == typeof(bool))
                                {
                                    data[parameterIndex - 1] = Convert.ToBoolean(newValue) ? 1 : 0;
                                }
                                else
                                {
                                    throw new Exception($"Typeof '{property}' ('{property.PropertyType}') is not supported.");
                                }
                            }
                        }
                    }
                }
                return data;
            }
            catch
            {
                return data;
            }
        }

        #endregion

        #region Private Methods
        private void RecipeSetting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RecipeSetting.CurrentRecipe))
            {
                OnPropertyChanged(nameof(CurrentRecipeDisplayName));
            }
        }

        private void BeginRecipeLoading()
        {
            _loadingDepth++;
            if (_loadingDepth == 1)
            {
                OnPropertyChanged(nameof(IsLoadingRecipe));
            }
        }

        private void EndRecipeLoading()
        {
            if (_loadingDepth <= 0)
            {
                return;
            }

            _loadingDepth--;
            if (_loadingDepth == 0)
            {
                OnPropertyChanged(nameof(IsLoadingRecipe));
            }
        }

        private string ResolveRecipeName(string recipeNameOrDisplay)
        {
            return _recipeCatalog.ResolveRecipeName(recipeNameOrDisplay);
        }

        private string ResolveRecipeFolderName(string recipeNameOrDisplay)
        {
            return _recipeCatalog.ResolveFolderName(recipeNameOrDisplay);
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var folder in Directory.GetDirectories(sourceDir))
            {
                string destFolder = Path.Combine(destinationDir, Path.GetFileName(folder));
                CopyDirectory(folder, destFolder);
            }
        }

        private void NormalizeSelectedLanguageReference()
        {
            ILanguageDefinition? selectedLanguage = CurrentRecipe.CommonRecipe.SelectedLanguage;
            if (selectedLanguage == null)
            {
                return;
            }

            ILanguageDefinition? languageDefinition = _languageService.AvailableLanguages
                .FirstOrDefault(language => language.Id == selectedLanguage.Id);

            if (languageDefinition != null && ReferenceEquals(selectedLanguage, languageDefinition) == false)
            {
                CurrentRecipe.CommonRecipe.SelectedLanguage = languageDefinition;
            }
        }
        #endregion
    }
}
