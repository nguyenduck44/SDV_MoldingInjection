using EQX.Core.InOut;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SDV_DotDispenser.Converters
{
    public class InputOutputListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] is not int index)
            {
                return Binding.DoNothing;
            }

            if (values[0] is List<IDInput> inputs)
            {
                var ordered = inputs
                    .GroupBy(i => i.Id)
                    .Select(g => g.FirstOrDefault(i => !i.Name.Contains("SPARE", StringComparison.OrdinalIgnoreCase)) ?? g.First())
                    .OrderBy(i => i.Id)
                    .ToList();

                var subList = ordered.Skip(index * 16).Take(16).ToList();
                IDInput[] newList = new IDInput[subList.Count];
                int half = subList.Count / 2;
                for (int i = 0; i < half; i++)
                {
                    newList[i * 2] = subList[i];
                    if (i + half < subList.Count)
                        newList[i * 2 + 1] = subList[i + half];
                }

                return newList.ToList();
            }
            else if (values[0] is List<IDOutput> outputs)
            {
                var ordered = outputs
                    .GroupBy(o => o.Id)
                    .Select(g => g.FirstOrDefault(o => !o.Name.Contains("SPARE", StringComparison.OrdinalIgnoreCase)) ?? g.First())
                    .OrderBy(o => o.Id)
                    .ToList();

                var subList = ordered.Skip(index * 16).Take(16).ToList();
                IDOutput[] newList = new IDOutput[subList.Count];
                int half = subList.Count / 2;
                for (int i = 0; i < half; i++)
                {
                    newList[i * 2] = subList[i];
                    if (i + half < subList.Count)
                        newList[i * 2 + 1] = subList[i + half];
                }

                return newList.ToList();
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
