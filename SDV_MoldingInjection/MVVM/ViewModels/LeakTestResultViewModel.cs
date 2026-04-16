using EQX.Core.Common;
using ScottPlot;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;
using System.Windows;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public interface ILeakTestResultViewModelFactory
    {
        LeakTestResultViewModel Create(
            IReadOnlyList<(double TimeSec, double PressureTorr)> samples,
            double? pressureSpecReachedSec,
            double? testEndSec,
            double vacuumPressureSpec,
            double vacuumPressureHoldUnderSpec);
    }

    public class LeakTestResultViewModelFactory : ILeakTestResultViewModelFactory
    {
        public LeakTestResultViewModel Create(
            IReadOnlyList<(double TimeSec, double PressureTorr)> samples,
            double? pressureSpecReachedSec,
            double? testEndSec,
            double vacuumPressureSpec,
            double vacuumPressureHoldUnderSpec)
        {
            double startToTargetSec = pressureSpecReachedSec ?? 0;
            double totalSec = testEndSec ?? samples.LastOrDefault().TimeSec;
            double? targetToSpecSec = pressureSpecReachedSec is { } tSpec && testEndSec is { } tEnd
                ? Math.Max(0, tEnd - tSpec)
                : null;

            return new LeakTestResultViewModel(
                samples,
                pressureSpecReachedSec,
                testEndSec,
                vacuumPressureSpec,
                vacuumPressureHoldUnderSpec,
                startToTargetSec,
                targetToSpecSec,
                totalSec);
        }
    }

    public class LeakTestResultViewModel : ViewModelBase
    {
        private const double MinPressureForLogScale = 1e-2;

        public LeakTestResultViewModel(
            IReadOnlyList<(double TimeSec, double PressureTorr)> samples,
            double? pressureSpecReachedSec,
            double? testEndSec,
            double vacuumPressureSpec,
            double vacuumPressureHoldUnderSpec,
            double startToTargetSec,
            double? targetToSpecSec,
            double totalSec)
        {
            StartToTargetLabelText = $"Start -> {vacuumPressureSpec:G4} torr:";
            TargetToSpecLabelText = $"{vacuumPressureSpec:G4} torr -> {vacuumPressureHoldUnderSpec:G4} torr:";
            TotalLabelText = $"Total:";

            StartToTargetTimeText = pressureSpecReachedSec is null
                ? "Do not have target"
                : $"{startToTargetSec:F3} s";
            TargetToSpecTimeText = targetToSpecSec is null
                ? "Do not have end milestone"
                : $"{targetToSpecSec.Value:F3} s";
            TotalTimeText = $"{totalSec:F3} s";

            ChamberPressurePlot = new WpfPlot
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
            };
            BuildPlot(ChamberPressurePlot, samples, pressureSpecReachedSec, testEndSec, vacuumPressureSpec, vacuumPressureHoldUnderSpec);
            ChamberPressurePlot.Loaded += OnChamberPressurePlotLoaded;
        }

        private void OnChamberPressurePlotLoaded(object sender, RoutedEventArgs e)
        {
            ChamberPressurePlot.Loaded -= OnChamberPressurePlotLoaded;
            ChamberPressurePlot.Refresh();
        }

        public string StartToTargetLabelText { get; }
        public string TargetToSpecLabelText { get; }
        public string TotalLabelText { get; }
        public string StartToTargetTimeText { get; }
        public string TargetToSpecTimeText { get; }
        public string TotalTimeText { get; }
        public string MilestoneSpecText { get; }

        /// <summary>Control ScottPlot để bind trong XAML (ContentControl).</summary>
        public WpfPlot ChamberPressurePlot { get; }

        private static void BuildPlot(
            WpfPlot plotControl,
            IReadOnlyList<(double TimeSec, double PressureTorr)> samples,
            double? pressureSpecReachedSec,
            double? testEndSec,
            double vacuumPressureSpec,
            double vacuumPressureHoldUnderSpec)
        {
            var plot = plotControl.Plot;
            plot.Clear();

            if (samples.Count == 0)
            {
                plotControl.Refresh();
                return;
            }

            double[] xs = samples.Select(s => s.TimeSec).ToArray();
            double[] ys = samples.Select(s =>
                s.PressureTorr > 0 ? Math.Log10(s.PressureTorr) : Math.Log10(MinPressureForLogScale)).ToArray();

            var scatter = plot.Add.Scatter(xs, ys);
            scatter.MarkerSize = 3;
            scatter.Color = new Color(30, 120, 200);

            double specY = vacuumPressureSpec > 0
                ? Math.Log10(vacuumPressureSpec)
                : Math.Log10(MinPressureForLogScale);
            double holdY = vacuumPressureHoldUnderSpec > 0
                ? Math.Log10(vacuumPressureHoldUnderSpec)
                : Math.Log10(MinPressureForLogScale);

            var lineSpec = plot.Add.HorizontalLine(specY);
            lineSpec.LineStyle.Color = new Color(40, 160, 70);
            lineSpec.LineStyle.Width = 1;
            var lineHold = plot.Add.HorizontalLine(holdY);
            lineHold.LineStyle.Color = new Color(200, 60, 60);
            lineHold.LineStyle.Width = 1;

            void AddVerticalLine(double x, Color color)
            {
                var vl = plot.Add.VerticalLine(x);
                vl.LineStyle.Color = color;
                vl.LineStyle.Width = 2;
            }

            AddVerticalLine(0, new Color(100, 100, 100));
            if (pressureSpecReachedSec is { } ts)
                AddVerticalLine(ts, new Color(40, 160, 70));
            if (testEndSec is { } te)
                AddVerticalLine(te, new Color(200, 60, 60));

            IMinorTickGenerator minorY = new LogDecadeMinorTickGenerator();
            var tickGenY = new NumericAutomatic
            {
                MinorTickGenerator = minorY,
                IntegerTicksOnly = true,
                LabelFormatter = static y => $"{Math.Pow(10, y):G4}",
            };
            plot.Axes.Left.TickGenerator = tickGenY;
            plot.Grid.MajorLineColor = Colors.Black.WithOpacity(0.15f);
            plot.Grid.MinorLineColor = Colors.Black.WithOpacity(0.05f);
            plot.Grid.MinorLineWidth = 1;

            plot.Axes.Bottom.Label.Text = "Time (s) from start";
            plot.Axes.Left.Label.Text = "Pressure (Torr)";

            plot.Axes.AutoScale();
            plotControl.Refresh();
        }
    }
}
