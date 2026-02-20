using EQX.Core.Recipe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddRecipeExtension
    {
        public static IHostBuilder AddRecipes(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IRecipe>(new CommonRecipe { Name = "Common" });
                services.AddSingleton<IRecipe>(new InjectRecipe { Name = "MoldRecipe" });
                services.AddSingleton<IRecipe>(new DryPumpRecipe { Name = "DryPumpRecipe" });
                services.AddSingleton<IRecipe>(new SPDHeadRecipe { Name = "SPDHead1_Recipe" });
                services.AddSingleton<IRecipe>(new SPDHeadRecipe { Name = "SPDHead2_Recipe" });
                services.AddSingleton<IRecipe>(new SPDHeadRecipe { Name = "SPDHead3_Recipe" });
                services.AddSingleton<IRecipe>(new SPDHeadRecipe { Name = "SPDHead4_Recipe" });

                services.AddSingleton<RecipeList>();
                services.AddSingleton<RecipeSelector>();
            });
            return hostBuilder;
        }
    }
}
