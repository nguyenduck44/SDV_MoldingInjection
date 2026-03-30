using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using ScottPlot;
using ScottPlot.WPF;
using System;
using System.Configuration;
using System.IO;
using System.Windows;

namespace SDV_MoldingInjection.MVVM.Models
{
    public class Plotter : ObservableObject
    {
        public WpfPlot ChamberPressurePlot { get; set; }

        public Plotter(IConfiguration configuration)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChamberPressurePlot = new WpfPlot();
            });
            logFolderPath = configuration.GetValue<string>("Files:PressureLogFolder");

            ClearData();

            ChamberPressurePlot!.Plot.Add.Scatter(pressureDateTimes!, pressureValues!);

            FormatAxis();
        }

        public void AddChamberPressureData(double pressure)
        {
            DateTime currentTime = DateTime.Now;
            pressureDateTimes.Add(currentTime);
            pressureValues.Add(pressure);

            Refresh();
        }

        public void ClearData()
        {
            pressureDateTimes = new List<DateTime>();
            pressureValues = new List<double>();

            Refresh();
        }

        public void Save(string? specificText = null)
        {
            if (string.IsNullOrEmpty(logFolderPath)) return;
            string folder = Path.Combine(logFolderPath, DateTime.Now.ToString("yyyy-mm"), DateTime.Now.ToString("yyyy-mm-dd"));
            string file = Path.Combine(folder, $"ChamberPressure_{DateTime.Now:HH-mm-ss}.png");
            if (string.IsNullOrEmpty(specificText) == false) file = Path.Combine(folder, $"ChamberPressure_{DateTime.Now:HH-mm-ss}_{specificText}.png");
            Directory.CreateDirectory(folder);

            ChamberPressurePlot.Plot.SaveSvg(file, 1920, 1080);
        }

        private void Refresh()
        {
            ChamberPressurePlot.Plot.Axes.AutoScale(true);

            ChamberPressurePlot.Refresh();
            OnPropertyChanged(nameof(ChamberPressurePlot));
        }

        private void FormatAxis()
        {
            var dtGen = new ScottPlot.TickGenerators.DateTimeAutomatic();
            dtGen.LabelFormatter = (DateTime dt) =>
            {
                return dt.ToString("HH:ss:mm.f");
            };
            ChamberPressurePlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = -45;
            ChamberPressurePlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            ChamberPressurePlot.Plot.Axes.Bottom.TickGenerator = dtGen;
            ChamberPressurePlot.Plot.Axes.Bottom.MinimumSize = 50;
        }

        private string? logFolderPath;
        private List<DateTime> pressureDateTimes;
        private List<double> pressureValues;
    }
}
