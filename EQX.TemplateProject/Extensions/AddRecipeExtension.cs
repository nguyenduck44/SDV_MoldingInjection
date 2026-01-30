using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EQX.TemplateProject.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQX.TemplateProject.Extensions
{
    public static class AddRecipeExtension
    {
        public static IHostBuilder AddRecipes(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<CommonRecipe>();
                services.AddSingleton<RecipeList>();
                services.AddSingleton<RecipeSelector>();
            });
            return hostBuilder;
        }
    }
}
