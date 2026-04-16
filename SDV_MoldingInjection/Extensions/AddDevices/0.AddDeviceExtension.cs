using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.TeachingPosition;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddDeviceExtension
    {
        public static IHostBuilder AddDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.AddBalanceDevices();
            hostBuilder.AddMotionDevices();
            hostBuilder.AddIODevices();
            hostBuilder.AddCylinderDevices();
            hostBuilder.AddIndicatorDevices();
            hostBuilder.AddInOutHandler();

            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<InterlockService>();
                services.AddTransient<RecipePositionManager>((ser) =>
                {
                    return new RecipePositionManager(
                        ser.GetRequiredService<RecipeSelector>().CurrentRecipe,
                        ser.GetRequiredService<Devices>().Motions.All);
                });
                services.AddSingleton<InjectMaintenanceTeachingPosition>();
                services.AddSingleton<DryPumpMaintenanceTeachingPosition>();
                services.AddSingleton<SPDHeadMaintenanceTeachingPosition>(s =>
                    new SPDHeadMaintenanceTeachingPosition(
                        s.GetRequiredService<RecipePositionManager>(),
                        s.GetRequiredService<Devices>(),
                        ESPDHead.SPDHead1));
                services.AddSingleton<SPDHeadMaintenanceTeachingPosition>(s =>
                    new SPDHeadMaintenanceTeachingPosition(
                        s.GetRequiredService<RecipePositionManager>(),
                        s.GetRequiredService<Devices>(),
                        ESPDHead.SPDHead2));
                services.AddSingleton<SPDHeadMaintenanceTeachingPosition>(s =>
                    new SPDHeadMaintenanceTeachingPosition(
                        s.GetRequiredService<RecipePositionManager>(),
                        s.GetRequiredService<Devices>(),
                        ESPDHead.SPDHead3));
                services.AddSingleton<SPDHeadMaintenanceTeachingPosition>(s =>
                    new SPDHeadMaintenanceTeachingPosition(
                        s.GetRequiredService<RecipePositionManager>(),
                        s.GetRequiredService<Devices>(),
                        ESPDHead.SPDHead4));
            });

            return hostBuilder;
        }

    }
}
