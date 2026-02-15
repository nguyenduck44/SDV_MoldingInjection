using EQX.Core.Motion;
using EQX.Motion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.IO;
using SDV_MoldingInjection.Defines;
using EQX.Motion.ByVendor.Inovance;
using SDV_MoldingInjection.Defines.Devices;
using EQX.Motion.ByVendor.Ajinextek;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddMotionDeviceExtension
    {
        public static IHostBuilder AddMotionDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddKeyedSingleton<List<IMotionParameter>>("MotionAjinParameter", (ser, obj) =>
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
                services.AddKeyedSingleton<List<IMotionParameter>>("MotionFastechParameter", (ser, obj) =>
                {
                    var configuration = ser.GetRequiredService<IConfiguration>();

                    var motionParameters = JsonConvert.DeserializeObject<List<MotionParameter>>(
                        File.ReadAllText(configuration["Files:MotionFastechParaConfigFile"] ?? "")
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

                for (int i = 0; i < Enum.GetNames(typeof(EMachineMotion)).Length; i++)
                {
                    var index = i;

                    services.AddSingleton<IMotion>((ser) =>
                    {
                        return new SimulationMotion(
                            index,
                            ((EMachineMotion)index).ToString(),
                            (MotionAjinParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionAjinParameter").First(p => p.Name == ((EMachineMotion)index).ToString())))
                        {
                        };
                    });
                }
                for (int i = 0; i < Enum.GetNames(typeof(EHeadMotion)).Length; i++)
                {
                    var index = i;

                    services.AddSingleton<IMotion>((ser) =>
                    {
                        return new SimulationMotion(
                            index,
                            ((EHeadMotion)index).ToString(),
                            (MotionParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionFastechParameter").First(p => p.Name == ((EHeadMotion)index).ToString())))
                        {
                        };
                    });
                }
#else
                services.AddKeyedScoped<IMotionMaster, MotionMasterAjin>("AjinMaster#1", (ser, obj) =>
                {
                    return new MotionMasterAjin() { NumberOfDevices = Enum.GetNames(typeof(EMachineMotion)).Length };
                });
                for (int i = 0; i < Enum.GetNames(typeof(EMachineMotion)).Length; i++)
                {
                    var index = i;

                    services.AddSingleton<IMotion>((ser) =>
                    {
                        return new MotionAjin(
                            index,
                            ((EMachineMotion)index).ToString(),
                            (MotionAjinParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionAjinParameter").First(p => p.Name == ((EMachineMotion)index).ToString())))
                        {
                            MotionMaster = (MotionMasterAjin)ser.GetRequiredKeyedService<IMotionMaster>("AjinMaster#1")
                        };
                    });
                }
                for (int i = 0; i < Enum.GetNames(typeof(EHeadMotion)).Length; i++)
                {
                    var index = i;

                    services.AddSingleton<IMotion>((ser) =>
                    {
                        return new MotionEziPlusR(
                            index,
                            ((EHeadMotion)index).ToString(),
                            (MotionParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionFastechParameter").First(p => p.Name == ((EHeadMotion)index).ToString())))
                        {
                            Port = 2,
                            Baudrate = 115200,
                        };
                    });
                }
#endif

                services.AddSingleton<Motions>();
                services.AddSingleton<Devices>();
            });

            return hostBuilder;
        }
    }
}
