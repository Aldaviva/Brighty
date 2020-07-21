#nullable enable

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BrightyUI {

    public class PercentageValueConverter: IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return $"{(uint) value/100.0:P0}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                return uint.Parse(value?.ToString().Replace(",", "").TrimEnd('%') ?? "0");
            } catch (FormatException e) {
                return new ValidationResult(false, e.Message);
            }
        }

    }

}