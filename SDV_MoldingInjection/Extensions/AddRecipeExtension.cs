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
                services.AddSingleton<RecipeSelector>();
                services.AddSingleton<RecipeList>();

            });
            return hostBuilder;
        }
    }
}
