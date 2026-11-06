using EQX.Core.Motion;
using EQX.Motion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.IO;
using SDV_MoldingInjection.Defines;
//using EQX.Motion.ByVendor.Inovance;
using EQX.Motion.ByVendor.ZMotion;
//using EQX.Motion.ByVendor.Ajinextek;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddMotionDeviceExtension
    {
        public static IHostBuilder AddMotionDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                #region Ajin Motion (legacy - commented)
                //services.AddKeyedSingleton<List<IMotionParameter>>("MotionAjinParameter", (ser, obj) =>
                //{
                //    var configuration = ser.GetRequiredService<IConfiguration>();
                //
                //    var motionParameters = JsonConvert.DeserializeObject<List<MotionAjinParameter>>(
                //        File.ReadAllText(configuration["Files:MotionAjinParaConfigFile"] ?? "")
                //    );
                //    if (motionParameters == null)
                //    {
                //        throw new FormatException("MotionParaConfigFile format error");
                //    }
                //    List<IMotionParameter> result = new List<IMotionParameter>();
                //    foreach (var parameter in motionParameters)
                //    {
                //        result.Add(parameter);
                //    }
                //    return result;
                //});
                #endregion
                #region Inovance Motion (legacy - commented)
                //services.AddKeyedSingleton<List<IMotionParameter>>("MotionInovanceParameter", (ser, obj) =>
                //{
                //    var configuration = ser.GetRequiredService<IConfiguration>();
                //
                //    var motionParameters = JsonConvert.DeserializeObject<List<MotionInovanceParameter>>(
                //        File.ReadAllText(configuration["Files:MotionInovanceParaConfigFile"] ?? "")
                //    );
                //    if (motionParameters == null)
                //    {
                //        throw new FormatException("MotionParaConfigFile format error");
                //    }
                //    List<IMotionParameter> result = new List<IMotionParameter>();
                //    foreach (var parameter in motionParameters)
                //    {
                //        result.Add(parameter);
                //    }
                //    return result;
                //});
                #endregion
                services.AddKeyedSingleton<List<IMotionParameter>>("MotionZMotionParameter", (ser, obj) =>
                {
                    var configuration = ser.GetRequiredService<IConfiguration>();

                    var motionParameters = JsonConvert.DeserializeObject<List<MotionZMotionParameter>>(
                        File.ReadAllText(configuration["Files:MotionZMotionParaConfigFile"] ?? "")
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
                //services.AddKeyedScoped<IMotionMaster, SimulationMotionMaster>("AjinMaster#1");
                //services.AddKeyedScoped<IMotionMaster, SimulationMotionMaster>("InovanceMaster#1");
                services.AddKeyedScoped<IMotionMaster, SimulationMotionMaster>("ZMotionMaster#1");
                services.AddKeyedScoped<IMotionMaster, SimulationMotionMaster>("FastechPlusRMaster#1");

                for (int i = 0; i < Enum.GetNames(typeof(EMachineMotion)).Length; i++)
                {
                    var index = i;

                    services.AddSingleton<IMotion>((ser) =>
                    {
                        #region Ajin Motion simulation axis (legacy - commented)
                        //return new SimulationMotion(
                        //    index,
                        //    ((EMachineMotion)index).ToString(),
                        //    (MotionAjinParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionAjinParameter").First(p => p.Name == ((EMachineMotion)index).ToString())))
                        //{
                        //};
                        #endregion
                        #region Inovance Motion simulation axis (legacy - commented)
                        //return new SimulationMotion(
                        //    index,
                        //    ((EMachineMotion)index).ToString(),
                        //    (MotionInovanceParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionInovanceParameter").First(p => p.Name == ((EMachineMotion)index).ToString())))
                        //{
                        //};
                        #endregion
                        return new SimulationMotion(
                            index,
                            ((EMachineMotion)index).ToString(),
                            (MotionZMotionParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionZMotionParameter").First(p => p.Name == ((EMachineMotion)index).ToString())))
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
                #region Ajin Motion runtime (legacy - commented)
                //services.AddKeyedScoped<IMotionMaster, MotionMasterAjin>("AjinMaster#1", (ser, obj) =>
                //{
                //    return new MotionMasterAjin() { NumberOfDevices = Enum.GetNames(typeof(EMachineMotion)).Length };
                //});
                #endregion
                #region Inovance Motion runtime (legacy - commented)
                //services.AddKeyedScoped<IMotionMaster, MotionMasterInovance>("InovanceMaster#1", (ser, obj) =>
                //{
                //    var configuration = ser.GetRequiredService<IConfiguration>();
                //    return new MotionMasterInovance()
                //    {
                //        NumberOfDevices = Enum.GetNames(typeof(EMachineMotion)).Length,
                //        DeviceConfigPath = configuration["Files:InovanceDeviceConfigFile"] ?? "",
                //        SystemConfigPath = configuration["Files:InovanceSystemConfigFile"] ?? "",
                //    };
                //});
                #endregion
                services.AddKeyedScoped<IMotionMaster, MotionMasterZMotion>("ZMotionMaster#1", (ser, obj) =>
                {
                    return new MotionMasterZMotion()
                    {
                        NumberOfDevices = Enum.GetNames(typeof(EMachineMotion)).Length,
                        ConnectType = 5,            // 5 = LOCAL (XPCIE / MotionRT), 2 = ETH
                        ConnectString = "local",    // IP khi dùng ETH, ví dụ "192.168.0.11"
                        InitEtherCAT = true,
                    };
                });
                services.AddKeyedScoped<IMotionMaster, MotionMasterEziPlusR>("FastechPlusRMaster#1", (ser, obj) =>
                {
                    return new MotionMasterEziPlusR(1, 115200)
                    {
                        NumberOfDevices = Enum.GetNames(typeof(EHeadMotion)).Length
                    };
                });
                for (int i = 0; i < Enum.GetNames(typeof(EMachineMotion)).Length; i++)
                {
                    var index = i;

                    services.AddSingleton<IMotion>((ser) =>
                    {
                        #region Ajin Motion runtime axis (legacy - commented)
                        //return new MotionAjin(
                        //    index,
                        //    ((EMachineMotion)index).ToString(),
                        //    (MotionAjinParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionAjinParameter").First(p => p.Name == ((EMachineMotion)index).ToString())))
                        //{
                        //    MotionMaster = (MotionMasterAjin)ser.GetRequiredKeyedService<IMotionMaster>("AjinMaster#1")
                        //};
                        #endregion
                        #region Inovance Motion runtime axis (legacy - commented)
                        //return new MotionInovance(
                        //    index,
                        //    ((EMachineMotion)index).ToString(),
                        //    (MotionInovanceParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionInovanceParameter").First(p => p.Name == ((EMachineMotion)index).ToString())))
                        //{
                        //    MotionMaster = (MotionMasterInovance)ser.GetRequiredKeyedService<IMotionMaster>("InovanceMaster#1")
                        //};
                        #endregion
                        return new MotionZMotion(
                            index,
                            ((EMachineMotion)index).ToString(),
                            (MotionZMotionParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionZMotionParameter").First(p => p.Name == ((EMachineMotion)index).ToString())))
                        {
                            MotionMaster = (MotionMasterZMotion)ser.GetRequiredKeyedService<IMotionMaster>("ZMotionMaster#1")
                        };
                    });
                }
                for (int i = 0; i < Enum.GetNames(typeof(EHeadMotion)).Length; i++)
                {
                    var index = i;

                    services.AddSingleton<IMotion>((ser) =>
                    {
                        if (index < (int)EHeadMotion.G1Axis)
                        {
                            return new MotionEziPlusRServo(
                            index,
                            ((EHeadMotion)index).ToString(),
                            (MotionParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionFastechParameter").First(p => p.Name == ((EHeadMotion)index).ToString())))
                            {
                                MotionMaster = (MotionMasterEziPlusR)ser.GetRequiredKeyedService<IMotionMaster>("FastechPlusRMaster#1")
                            };
                        }
                        else
                        {
                            return new MotionEziPlusRStep(
                            index,
                            ((EHeadMotion)index).ToString(),
                            (MotionParameter)(ser.GetRequiredKeyedService<List<IMotionParameter>>("MotionFastechParameter").First(p => p.Name == ((EHeadMotion)index).ToString())))
                            {
                                MotionMaster = (MotionMasterEziPlusR)ser.GetRequiredKeyedService<IMotionMaster>("FastechPlusRMaster#1")
                            };
                        }
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
