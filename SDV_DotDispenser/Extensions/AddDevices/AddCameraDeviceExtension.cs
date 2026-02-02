using EQX.Core.LightController;
using EQX.Core.Vision.Grabber;
using EQX.Vision.Grabber;
using EQX.Vision.LightController;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SDV_DotDispenser.Extensions
{
    public static class AddCameraDeviceExtension
    {
        public static IHostBuilder AddCameraDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
#if SIMULATION
                services.AddKeyedScoped<ICamera>("AlignCamera", (services, obj) => { return new SimulationCamera(@"D:\DotDispenser\Vision\ImageTest"); });
                services.AddKeyedScoped<ILightController>("AlignLightController", (services, obj) => { return new SimulationLightController(1, "Align Light"); });
#else
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                string ipAddress;
                try
                {
                    var configProcess = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(configuration["Files:CameraConfigFile"])
                        .Build();

                    ipAddress = configProcess.GetValue<string>("IpAddress");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                services.AddKeyedScoped<ICamera>("AlignCamera", (services, obj) => { return new CameraHikrobotGigEv2(ipAddress, "Align Camera"); });
                services.AddKeyedScoped<ILightController>("AlignLightController", (services, obj) => { return new LightControllerMV(1, "Align Light", "COM2"); });
#endif
            });

            return hostBuilder;
        }

    }
}
