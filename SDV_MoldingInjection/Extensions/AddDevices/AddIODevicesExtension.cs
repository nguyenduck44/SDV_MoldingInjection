using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.InOut;
using EQX.InOut.InOut.Analog;
using EQX.Motion;
using EQX.Motion.ByVendor.Inovance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.Extensions
{
    public enum EInputDevice
    {
         MachineInput = 0,
         Head1Input = 1,
         Head2Input = 2,
         Head3Input = 3,
         Head4Input = 4,
    }

    public enum EOutputDevice
    {
        MachineOutput = 0,
        Head1Output = 1,
        Head2Output = 2,
        Head3Output = 3,
        Head4Output = 4,
    }

    public static class AddIODevicesExtension
    {
        public static IHostBuilder AddIODevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
#if SIMULATION
                services.AddSingleton<IDInputDevice>((services) =>
                {
                    return new SimulationInputDevice_ClientMMF<EMachineInput>()
                    {
                        Id = (int)EInputDevice.MachineInput,
                        Name = EInputDevice.MachineInput.ToString(),
                        MaxPin = 100,
                        SimulationOffset = 0,
                    };
                });
                
                for (int i = 1; i <= 4; i++)
                {
                    var index = i;

                    services.AddSingleton<IDInputDevice>((services) =>
                    {
                        return new SimulationInputDevice_ClientMMF<EHeadInput>()
                        {
                            Id = index - 1,
                            Name = ((EInputDevice)index).ToString(),
                            MaxPin = 16,
                            SimulationOffset = 100 + 16 * (index - 1),
                        };
                    });
                }
#else
                services.AddSingleton<IDInputDevice>((services) =>
                {
                    return new AjinInputDevice<EMachineInput>()
                    {
                        Id = (int)EInputDevice.MachineInput,
                        Name = EInputDevice.MachineInput.ToString(),
                        MaxPin = 100,
                    };
                });
                
                for (int i = 1; i <= 4; i++)
                {
                    var index = i;

                    services.AddSingleton<IDInputDevice>((ser) =>
                    {
                        return new PlusRInputDevice<EHeadInput>()
                        {
                            Id = index - 1,
                            Name = ((EInputDevice)index).ToString(),
                            MaxPin = 100,
                            MotionMaster = (MotionMasterEziPlusR)ser.GetRequiredKeyedService<IMotionMaster>("FastechPlusRMaster#1")

                        };
                    });
                }
#endif

#if SIMULATION
                services.AddSingleton<IDOutputDevice>((services) =>
                {
                    return new SimulationOutputDevice<EMachineOutput>()
                    {
                        Id = (int)EOutputDevice.MachineOutput,
                        Name = EOutputDevice.MachineOutput.ToString(),
                        MaxPin = 100,
                    };
                });
                for (int i = 1; i <= 4; i++)
                {
                    var index = i;

                    services.AddSingleton<IDOutputDevice>((services) =>
                    {
                        return new SimulationOutputDevice<EHeadOutput>()
                        {
                            Id = index - 1,
                            Name = ((EOutputDevice)index).ToString(),
                            MaxPin = 100,
                        };
                    });
                }
#else
                services.AddSingleton<IDOutputDevice>((services) =>
                {
                    return new AjinOutputDevice<EMachineOutput>()
                    {
                        Id = (int)EOutputDevice.MachineOutput,
                        Name = EOutputDevice.MachineOutput.ToString(),
                        MaxPin = 100,
                    };
                });

                for (int i = 1; i <= 4; i++)
                {
                    var index = i;

                    services.AddSingleton<IDOutputDevice>((ser) =>
                    {
                        return new PlusROutputDevice<EHeadOutput>()
                        {
                            Id = index - 1,
                            Name = ((EOutputDevice)index).ToString(),
                            MaxPin = 100,
                            MotionMaster = (MotionMasterEziPlusR)ser.GetRequiredKeyedService<IMotionMaster>("FastechPlusRMaster#1")
                        };
                    });
                }
#endif
#if SIMULATION
                services.AddKeyedSingleton<IAInputDevice>("AnalogInputDevice#1", (services, obj) => { return new SimulationAnalogInputDevice<EAnalogInput>(); });
#else
                services.AddKeyedSingleton<IAInputDevice>("AnalogInputDevice#1", (ser, obj) =>
                {
                    return new AjinAnalogInputDevice<EAnalogInput>()
                    {
                        Id = 28,
                        Name = "AnalogInputDevice",
                    };
                });
#endif

                services.AddSingleton<Inputs>();
                services.AddSingleton<Outputs>();

                services.AddSingleton<AnalogInputs>();
            });

            return hostBuilder;
        }
    }
}
