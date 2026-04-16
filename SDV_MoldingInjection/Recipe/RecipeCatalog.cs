using EQX.Core.Communication.CIM;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipeCatalog
    {
        public const string RecipeInfoFileName = "RecipeInfo.json";

        private readonly IConfiguration _configuration;
        private readonly object _sync = new();
        private List<RecipeInfo> _recipes = new();
        private Dictionary<string, RecipeInfo> _recipesByName = new(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, RecipeInfo> _recipesByDisplay = new(StringComparer.OrdinalIgnoreCase);
        private Dictionary<int, RecipeInfo> _recipesById = new();

        private string RecipeFolder => _configuration.GetValue<string>("Folders:RecipeFolder") ?? string.Empty;

        public RecipeCatalog(IConfiguration configuration)
        {
            _configuration = configuration;
            Refresh();
        }

        public void Refresh()
        {
            List<RecipeInfo> scannedRecipes = new();

            if (Directory.Exists(RecipeFolder))
            {
                scannedRecipes = Directory.GetDirectories(RecipeFolder, "*", SearchOption.TopDirectoryOnly)
                    .Select(folderPath => LoadOrCreateRecipeInfo(folderPath))
                    .Where(recipe => !string.IsNullOrWhiteSpace(recipe.Name))
                    .GroupBy(recipe => recipe.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderBy(recipe => recipe.Id > 0 ? recipe.Id : int.MaxValue)
                    .ThenBy(recipe => recipe.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            lock (_sync)
            {
                _recipes = scannedRecipes;
                _recipesByName = BuildByName(_recipes);
                _recipesByDisplay = BuildByDisplay(_recipes);
                _recipesById = BuildById(_recipes);
            }
        }

        public IReadOnlyList<RecipeInfo> GetAll()
        {
            lock (_sync)
            {
                return _recipes.ToList();
            }
        }

        public string ResolveRecipeName(string recipeNameOrDisplay)
        {
            if (string.IsNullOrWhiteSpace(recipeNameOrDisplay))
            {
                return recipeNameOrDisplay;
            }

            lock (_sync)
            {
                if (_recipesByName.TryGetValue(recipeNameOrDisplay, out RecipeInfo? byName))
                {
                    return byName.Name;
                }

                if (_recipesByDisplay.TryGetValue(recipeNameOrDisplay, out RecipeInfo? byDisplay))
                {
                    return byDisplay.Name;
                }

                int separatorIndex = recipeNameOrDisplay.IndexOf('.');
                if (separatorIndex > 0)
                {
                    string idText = recipeNameOrDisplay[..separatorIndex];
                    if (int.TryParse(idText, out int recipeId) && _recipesById.TryGetValue(recipeId, out RecipeInfo? byId))
                    {
                        return byId.Name;
                    }
                }
            }

            return recipeNameOrDisplay;
        }

        public string ResolveFolderName(string recipeNameOrDisplay)
        {
            if (string.IsNullOrWhiteSpace(recipeNameOrDisplay))
            {
                return recipeNameOrDisplay;
            }

            lock (_sync)
            {
                if (TryGetRecipeInfoUnsafe(recipeNameOrDisplay, out RecipeInfo? recipeInfo))
                {
                    return recipeInfo.FolderName;
                }
            }

            return recipeNameOrDisplay;
        }

        public bool TryGetRecipeInfo(string recipeNameOrDisplay, out RecipeInfo? recipeInfo)
        {
            lock (_sync)
            {
                return TryGetRecipeInfoUnsafe(recipeNameOrDisplay, out recipeInfo);
            }
        }

        public bool ContainsName(string recipeName)
        {
            lock (_sync)
            {
                return _recipesByName.ContainsKey(recipeName);
            }
        }

        public bool ContainsId(int recipeId)
        {
            if (recipeId <= 0)
            {
                return false;
            }

            lock (_sync)
            {
                return _recipesById.ContainsKey(recipeId);
            }
        }

        public RecipeInfo WriteRecipeInfo(string folderName, int recipeId, string recipeName)
        {
            string normalizedName = recipeName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                throw new ArgumentException("Recipe name is empty.", nameof(recipeName));
            }

            string recipeFolderPath = Path.Combine(RecipeFolder, folderName);
            if (Directory.Exists(recipeFolderPath) == false)
            {
                throw new DirectoryNotFoundException($"Recipe folder not found: {recipeFolderPath}");
            }

            RecipeInfo recipeInfo = new RecipeInfo
            {
                Id = recipeId,
                Name = normalizedName,
                FolderName = folderName
            };

            string recipeInfoFilePath = Path.Combine(recipeFolderPath, RecipeInfoFileName);
            File.WriteAllText(recipeInfoFilePath, JsonConvert.SerializeObject(recipeInfo, Formatting.Indented));

            return recipeInfo;
        }

        public string ToDisplayName(string recipeNameOrDisplay)
        {
            string recipeName = ResolveRecipeName(recipeNameOrDisplay);

            lock (_sync)
            {
                return _recipesByName.TryGetValue(recipeName, out RecipeInfo? info)
                    ? info.DisplayName
                    : recipeName;
            }
        }

        private RecipeInfo LoadOrCreateRecipeInfo(string recipeFolderPath)
        {
            string recipeFolderName = Path.GetFileName(recipeFolderPath) ?? string.Empty;
            string recipeInfoFilePath = Path.Combine(recipeFolderPath, RecipeInfoFileName);

            RecipeInfo recipeInfo;
            if (File.Exists(recipeInfoFilePath))
            {
                try
                {
                    recipeInfo = JsonConvert.DeserializeObject<RecipeInfo>(File.ReadAllText(recipeInfoFilePath)) ?? new RecipeInfo();
                }
                catch
                {
                    recipeInfo = new RecipeInfo();
                }
            }
            else
            {
                recipeInfo = new RecipeInfo();
            }

            if (string.IsNullOrWhiteSpace(recipeInfo.Name))
            {
                recipeInfo.Name = recipeFolderName;
            }

            if (recipeInfo.Id <= 0 && CIMHelpers.TryParseRecipeNumber(recipeInfo.Name, out int recipeId))
            {
                recipeInfo.Id = recipeId;
            }

            recipeInfo.FolderName = recipeFolderName;

            if (File.Exists(recipeInfoFilePath) == false)
            {
                File.WriteAllText(recipeInfoFilePath, JsonConvert.SerializeObject(recipeInfo, Formatting.Indented));
            }

            return recipeInfo;
        }

        private static Dictionary<string, RecipeInfo> BuildByName(IEnumerable<RecipeInfo> recipes)
        {
            Dictionary<string, RecipeInfo> byName = new(StringComparer.OrdinalIgnoreCase);
            foreach (RecipeInfo recipe in recipes)
            {
                if (byName.ContainsKey(recipe.Name))
                {
                    continue;
                }

                byName.Add(recipe.Name, recipe);
            }

            return byName;
        }

        private static Dictionary<string, RecipeInfo> BuildByDisplay(IEnumerable<RecipeInfo> recipes)
        {
            Dictionary<string, RecipeInfo> byDisplay = new(StringComparer.OrdinalIgnoreCase);
            foreach (RecipeInfo recipe in recipes)
            {
                if (byDisplay.ContainsKey(recipe.DisplayName))
                {
                    continue;
                }

                byDisplay.Add(recipe.DisplayName, recipe);
            }

            return byDisplay;
        }

        private static Dictionary<int, RecipeInfo> BuildById(IEnumerable<RecipeInfo> recipes)
        {
            Dictionary<int, RecipeInfo> byId = new();
            foreach (RecipeInfo recipe in recipes)
            {
                if (recipe.Id <= 0 || byId.ContainsKey(recipe.Id))
                {
                    continue;
                }

                byId.Add(recipe.Id, recipe);
            }

            return byId;
        }

        private bool TryGetRecipeInfoUnsafe(string recipeNameOrDisplay, out RecipeInfo? recipeInfo)
        {
            recipeInfo = null;

            if (_recipesByName.TryGetValue(recipeNameOrDisplay, out RecipeInfo? byName))
            {
                recipeInfo = byName;
                return true;
            }

            if (_recipesByDisplay.TryGetValue(recipeNameOrDisplay, out RecipeInfo? byDisplay))
            {
                recipeInfo = byDisplay;
                return true;
            }

            int separatorIndex = recipeNameOrDisplay.IndexOf('.');
            if (separatorIndex > 0)
            {
                string idText = recipeNameOrDisplay[..separatorIndex];
                if (int.TryParse(idText, out int recipeId) && _recipesById.TryGetValue(recipeId, out RecipeInfo? byId))
                {
                    recipeInfo = byId;
                    return true;
                }
            }

            return false;
        }
    }
}
