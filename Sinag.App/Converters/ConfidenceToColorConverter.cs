using System.Globalization;
using Sinag.App.Models;
using Sinag.App.Services;

namespace Sinag.App.Converters;

public class ConfidenceToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ConfidenceLevel level)
        {
            return level switch
            {
                ConfidenceLevel.Low => Color.FromArgb("#f8a010").WithAlpha(0.15f),    // primary-container at 15%
                ConfidenceLevel.Medium => Color.FromArgb("#f8a010").WithAlpha(0.08f), // primary-container at 8%
                _ => Color.FromArgb("#f2eedb")  // surface-container-high (slab default)
            };
        }
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoolToTextConverter : IValueConverter
{
    public string? TrueText { get; set; }
    public string? FalseText { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? TrueText : FalseText;
        return FalseText;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class CurrencyFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return $"₱{d:N2}";
        return "₱0.00";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class LocalizedFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string key && value != null)
        {
            var format = LocalizationService.Instance[key];
            return string.Format(format, value);
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
