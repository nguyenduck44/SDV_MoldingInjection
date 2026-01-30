using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_BIM_Line_DotDispenser.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_BIM_Line_DotDispenser.Extensions
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
