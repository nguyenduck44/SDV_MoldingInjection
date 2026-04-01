using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;
using SDV_MoldingInjection.Recipe;
using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.Models
{
    public class Plotter : ObservableObject
    {
        #region Properties
        public WpfPlot ChamberPressurePlot { get; set; }
        #endregion

        #region Constructor(s)
        public Plotter(IConfiguration configuration, RecipeSelector recipeSelector)
        {
            _recipeSelector = recipeSelector;

            Application.Current.Dispatcher.Invoke(() =>
            {
                ChamberPressurePlot = new WpfPlot();
                ToolTipService.SetInitialShowDelay(ChamberPressurePlot, 0);
                ToolTipService.SetBetweenShowDelay(ChamberPressurePlot, 0);
                ChamberPressurePlot.MouseMove += OnChamberPressurePlotMouseMove;
                ChamberPressurePlot.MouseLeave += OnChamberPressurePlotMouseLeave;
            });
            logFolderPath = configuration.GetValue<string>("Folders:PressureLogFolder");

            ClearData();

            FormatAxis();
        }
        #endregion

        #region Public Methods
        public void AddChamberPressureData(double pressure)
        {
            DateTime currentTime = DateTime.Now;
            pressureDateTimes.Add(currentTime);
            if (isLogScale)
            {
                // Trục Y log: ScottPlot nhận tọa độ log10; nhãn trục hiển thị giá trị thực (Pow(10,y))
                double y = pressure > 0 ? Math.Log10(pressure) : Math.Log10(MinPressureForLogScale);
                pressureValues.Add(y);
            }
            else
            {
                pressureValues.Add(pressure);
            }

            Refresh();
        }

        public void ClearData()
        {
            pressureDateTimes.Clear();
            pressureValues.Clear();

            ChamberPressurePlot!.Plot.Remove(typeof(ScottPlot.Plottables.Scatter));
            _chamberPressureScatter = ChamberPressurePlot!.Plot.Add.Scatter(pressureDateTimes!, pressureValues!);

            ChamberPressurePlot!.Plot.Remove(typeof(ScottPlot.Plottables.HorizontalLine));

            double spec, specUnder;
            if (isLogScale)
            {
                // Trục Y log: ScottPlot nhận tọa độ log10; nhãn trục hiển thị giá trị thực (Pow(10,y))
                spec = _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureSpec > 0
                    ? Math.Log10(_recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureSpec)
                    : Math.Log10(MinPressureForLogScale);
                specUnder = _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec > 0
                    ? Math.Log10(_recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec)
                    : Math.Log10(MinPressureForLogScale);
            }
            else
            {
                spec = _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureSpec;
                specUnder = _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec;
            }

            ChamberPressurePlot!.Plot.Add.HorizontalLine(spec);
            ChamberPressurePlot!.Plot.Add.HorizontalLine(specUnder);

            _chamberPressureScatter.MarkerSize = 3;

            Refresh();
        }

        public void Save(string? specificText = null)
        {
            if (string.IsNullOrEmpty(logFolderPath)) return;
            string folder = Path.Combine(logFolderPath, DateTime.Now.ToString("yyyy-MM"), DateTime.Now.ToString("yyyy-MM-dd"));
            string file = Path.Combine(folder, $"ChamberPressure_{DateTime.Now:HH-mm-ss}.svg");
            if (string.IsNullOrEmpty(specificText) == false) file = Path.Combine(folder, $"ChamberPressure_{DateTime.Now:HH-mm-ss}_{specificText}.svg");
            Directory.CreateDirectory(folder);

            ChamberPressurePlot.Plot.SaveSvg(file, 1920, 1080);
        }
        #endregion

        #region Private methods
        private void OnChamberPressurePlotMouseMove(object sender, MouseEventArgs e)
        {
            var plot = ChamberPressurePlot.Plot;
            if (plot.LastRender.Count == 0 || pressureDateTimes.Count == 0)
                return;

            Pixel mousePx = ChamberPressurePlot.GetPlotPixelPosition(e);
            Coordinates mouseCoords = plot.GetCoordinates(mousePx, plot.Axes.Bottom, plot.Axes.Left);

            IDataSource data = _chamberPressureScatter.GetIDataSource();
            DataPoint nearest = DataSourceUtilities.GetNearest(
                data,
                mouseCoords,
                plot.LastRender,
                maxDistance: 28f,
                plot.Axes.Bottom,
                plot.Axes.Left);

            if (!nearest.IsReal || nearest.Index < 0 || nearest.Index >= pressureDateTimes.Count)
            {
                ChamberPressurePlot.ToolTip = null;
                return;
            }

            DateTime t = pressureDateTimes[nearest.Index];
            double pressureLinear = Math.Pow(10, pressureValues[nearest.Index]);
            ChamberPressurePlot.ToolTip =
                $"Thời gian: {t:yyyy/MM/dd HH:mm:ss.fff}\nÁp suất: {pressureLinear:F3}";
        }

        private void OnChamberPressurePlotMouseLeave(object sender, MouseEventArgs e)
        {
            ChamberPressurePlot.ToolTip = null;
        }

        private void Refresh()
        {
            if (DateTime.Now - lastRefresh < TimeSpan.FromMilliseconds(800))
                return; // Giới hạn tần suất refresh để tránh lag
            lastRefresh = DateTime.Now;

            ChamberPressurePlot.Plot.Axes.AutoScale(true);
            ChamberPressurePlot.Plot.Axes.RectifyX();

            ChamberPressurePlot.Refresh();
            OnPropertyChanged(nameof(ChamberPressurePlot));
        }

        DateTime lastRefresh = DateTime.Now;

        private void FormatAxis()
        {
            var dtGen = new DateTimeAutomatic();
            dtGen.LabelFormatter = (DateTime dt) =>
            {
                return dt.ToString("HH:mm:ss");
            };
            ChamberPressurePlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = -45;
            ChamberPressurePlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            ChamberPressurePlot.Plot.Axes.Bottom.TickGenerator = dtGen;
            ChamberPressurePlot.Plot.Axes.Bottom.MinimumSize = 50;

            if (isLogScale)
            {
                // Trục Y thang log (mật độ tick theo bậc 10)
                IMinorTickGenerator minorY = new LogDecadeMinorTickGenerator();
                var tickGenY = new NumericAutomatic
                {
                    MinorTickGenerator = minorY,
                    IntegerTicksOnly = true,
                    LabelFormatter = static y => $"{Math.Pow(10, y):G4}",
                };
                ChamberPressurePlot.Plot.Axes.Left.TickGenerator = tickGenY;
                ChamberPressurePlot.Plot.Grid.MajorLineColor = Colors.Black.WithOpacity(0.15f);
                ChamberPressurePlot.Plot.Grid.MinorLineColor = Colors.Black.WithOpacity(0.05f);
                ChamberPressurePlot.Plot.Grid.MinorLineWidth = 1;
            }

            _chamberPressureScatter.MarkerSize = 3;
        }
        #endregion

        #region Private fields
        private const double MinPressureForLogScale = 1e-2;
        private readonly RecipeSelector _recipeSelector;
        private bool isLogScale = true;

        private string? logFolderPath;
        private Scatter _chamberPressureScatter = null!;
        private List<DateTime> pressureDateTimes = new List<DateTime>();
        private List<double> pressureValues = new List<double>();
        #endregion
    }
}
