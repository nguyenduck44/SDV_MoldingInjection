using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using EQX.UI.Language;
using log4net;
using Microsoft.Extensions.Configuration;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class RecipeViewModel : ViewModelBase
    {
        private string selectedModel;
        private RecipeBase _selectedRecipe;
        private readonly Motions _motions;
        private readonly ILanguageService _languageService;
        private readonly IConfiguration _configuration;
        public RecipeViewModel(RecipeSelector recipeSelector,
            Motions motions,
            ILanguageService languageService,
            IConfiguration configuration,
            MachineStatus machineStatus)
        {
            RecipeSelector = recipeSelector;
            _motions = motions;
            _languageService = languageService;
            _configuration = configuration;
            MachineStatus = machineStatus;
            Log = LogManager.GetLogger("Data");
        }

        public ObservableCollection<ILanguageDefinition> Cultures => new ObservableCollection<ILanguageDefinition>(_languageService.AvailableLanguages);

        public ObservableCollection<IMotion> AllMotions
        {
            get
            {
                List<IMotion> motions = new List<IMotion>();
                motions.AddRange(_motions.All);

                return new ObservableCollection<IMotion>(motions);
            }
        }

        public RecipeSelector RecipeSelector { get; }

        public string SelectedModel
        {
            get { return selectedModel; }
            set
            {
                selectedModel = value;
                OnPropertyChanged();
            }
        }

        public event Action LoadRecipeEvent;

        public ObservableCollection<RecipeBase> Recipes
        {
            get
            {
                var recipeObjects = RecipeSelector.CurrentRecipe.GetType()
                                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => typeof(RecipeBase).IsAssignableFrom(p.PropertyType))
                                    .Select(p => p.GetValue(RecipeSelector.CurrentRecipe))
                                    .OfType<RecipeBase>()
                                    .Where(r => r is not AdditionalMoldingRecipe && r is not OptionRecipe)
                                    .ToList();

                return new ObservableCollection<RecipeBase>(recipeObjects);
            }
        }

        public RecipeBase SelectedRecipe
        {
            get
            {
                return _selectedRecipe;
            }
            set
            {
                _selectedRecipe = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnableSelectLanguage));
            }
        }

        public bool IsEnableSelectLanguage => SelectedRecipe is CommonRecipe;
        public MachineStatus MachineStatus { get; }

        public RecipeBase CurrentRecipe
        {
            get
            {
                var recipeProps = RecipeSelector.CurrentRecipe.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(RecipeBase).IsAssignableFrom(p.PropertyType))
                .ToList();

                return recipeProps
                .Select(p => p.GetValue(RecipeSelector.CurrentRecipe) as RecipeBase)
                .ToList().First(r => r.Name == SelectedRecipe.Name);
            }
        }

        public ICommand SaveRecipeCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_SaveAllData"]) == true)
                    {
                        RecipeSelector.Save();
                        //CassetteList.RecipeUpdateHandle();
                        OnPropertyChanged(nameof(Recipes));
                    }
                });
            }
        }
        public ICommand RefreshRecipeCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeSelector.UpdateValidRecipes();
                    LoadRecipeEvent?.Invoke();
                });
            }
        }
        public ICommand CopyRecipeCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    string CopyRecipe = (string)Application.Current.Resources["str_CopyRecipe"];
                    if (MessageBoxEx.ShowDialog($"{CopyRecipe} {SelectedModel} ? ") == true)
                    {
                        RecipeSelector.Copy(SelectedModel);
                        RecipeSelector.UpdateValidRecipes();
                        LoadRecipeEvent?.Invoke();
                    }
                });
            }
        }
        public ICommand DeleteRecipeCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeSelector.Delete(SelectedModel);
                });
            }
        }
    }
}
