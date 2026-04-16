namespace SDV_MoldingInjection.Recipe
{
    public class RecipeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
        public string DisplayName => Id > 0 ? $"{Id}.{Name}" : Name;
    }
}
