using EQX.Core.Motion;
using EQX.Motion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.IO;
using EQX.TemplateProject.Defines;
using EQX.Motion.ByVendor.Inovance;
using EQX.TemplateProject.Defines.Devices;
using EQX.Motion.ByVendor.Ajinextek;

namespace EQX.TemplateProject.Extensions
{
    public static class AddMotionDeviceExtension
    {
        public static IHostBuilder AddMotionDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddKeyedScoped<List<IMotionParameter>>("MotionAjinParameters", (ser, obj) =>
                {
                    var configuration = ser.GetRequiredService<IConfiguration>();

                    var motionParameters = JsonConvert.DeserializeObject<List<MotionAjinParameter>>(
                        File.ReadAllText(configuration["Files:MotionAjinParaConfigFile"] ?? "")
                    );
                    if (motionParameters == null)
                    {
                        throw new FormatException("MotionParaConfigFile format error");
                    }
                    List<IMotionParameter> result = new List<IMotionParameter>();
                    foreach (var parameter in motionParameters)
                    {
                        result.Add(parameter);
                    }
                    return result;
                });

#if SIMULATION
                services.AddKeyedScoped<IMotionMaster, SimulationMotionMaster>("AjinMaster#1");

                services.AddKeyedScoped<IMotionFactory<IMotion>, SimulationMotionFactory>("AjinMotionFactory");
#else
                services.AddKeyedScoped<IMotionFactory<IMotion>>("AjinMotionFactory", (ser, obj) =>
                {
                    return new MotionAjinFactory(ser.GetRequiredKeyedService<IMotionMaster>("AjinMaster#1"));
                });
#endif

                services.AddSingleton<Motions>();
                services.AddSingleton<Devices>();
            });

            return hostBuilder;
        }
    }
}
