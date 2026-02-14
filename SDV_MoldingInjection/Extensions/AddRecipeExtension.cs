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
                services.AddSingleton<IRecipe>(new DispensingRecipe { Name = "Dispensing" });
                services.AddSingleton<IRecipe>(new StageRecipe { Name = "StageLeft" });
                services.AddSingleton<IRecipe>(new StageRecipe { Name = "StageRight" });
                services.AddSingleton<IRecipe>(new PlasmaRecipe { Name = "Plasma" });
                services.AddSingleton<IRecipe>(new CleanRecipe { Name = "Clean" });
                services.AddSingleton<IRecipe>(new FinalInspectRecipe { Name = "FinalInspect" });
                services.AddSingleton<IRecipe>(new UVCureRecipe { Name = "UVCure" });
                services.AddSingleton<IRecipe>(new TransferRecipe { Name = "Transfer" });

                services.AddSingleton<RecipeList>();
                services.AddSingleton<RecipeSelector>();
            });
            return hostBuilder;
        }
    }
}
