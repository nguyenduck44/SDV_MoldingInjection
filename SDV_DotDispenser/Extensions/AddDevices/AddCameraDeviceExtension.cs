using EQX.Core.LightController;
using EQX.Core.Vision.Grabber;
using EQX.Vision.Grabber;
using EQX.Vision.Grabber.Hikrobot;
using EQX.Vision.LightController;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace SDV_DotDispenser.Extensions
{
    public static class AddCameraDeviceExtension
    {
        public static IHostBuilder AddCameraDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
#if SIMULATION
                services.AddKeyedScoped<ICamera>("AlignCamera#1", (services, obj) => { return new SimulationCamera(@"D:\DotDispenser\Vision\ImageTest"); });
                services.AddKeyedScoped<ICamera>("AlignCamera#2", (services, obj) => { return new SimulationCamera(@"D:\DotDispenser\Vision\ImageTest"); });
                services.AddKeyedScoped<ICamera>("InspectCamera#1", (services, obj) => { return new SimulationCamera(@"D:\DotDispenser\Vision\ImageTest"); });
                services.AddKeyedScoped<ICamera>("InspectCamera#2", (services, obj) => { return new SimulationCamera(@"D:\DotDispenser\Vision\ImageTest"); });
                services.AddKeyedScoped<ILightController>("AlignLightController#1", (services, obj) => { return new SimulationLightController(1, "Align Light"); });
                services.AddKeyedScoped<ILightController>("InspectLightController#1", (services, obj) => { return new SimulationLightController(1, "Inspect Light"); });
#else
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                string ipAlignCam1 = "";
                string ipAlignCam2 = "";
                string ipInspectCam1 = "";
                string ipInspectCam2 = "";
                try
                {
                    var configProcess = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(configuration["Files:CameraConfigFile"]!)
                        .Build();

                    ipAlignCam1 = configProcess.GetValue<string>("AlignCamera1")!;
                    ipAlignCam2 = configProcess.GetValue<string>("AlignCamera2")!;
                    ipInspectCam1 = configProcess.GetValue<string>("InspectCamera1")!;
                    ipInspectCam2 = configProcess.GetValue<string>("InspectCamera2")!;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                services.AddKeyedScoped<ICamera>("AlignCamera#1", (services, obj) => { return new CameraHikrobotGigEv2(ipAlignCam1, "Align Camera #1"); });
                services.AddKeyedScoped<ICamera>("AlignCamera#2", (services, obj) => { return new CameraHikrobotGigEv2(ipAlignCam2, "Align Camera #2"); });
                services.AddKeyedScoped<ICamera>("InspectCamera#1", (services, obj) => { return new CameraHikrobotGigEv2(ipInspectCam1, "Inspect Camera #1"); });
                services.AddKeyedScoped<ICamera>("InspectCamera#2", (services, obj) => { return new CameraHikrobotGigEv2(ipInspectCam2, "Inspect Camera #2"); });
                services.AddKeyedScoped<ILightController>("AlignLightController", (services, obj) => { return new LightControllerMV(1, "Align Light", "COM2"); });
                services.AddKeyedScoped<ILightController>("AlignLightController", (services, obj) => { return new LightControllerMV(1, "Inspect Light", "COM3"); });
#endif
            });

            return hostBuilder;
        }

    }
}
