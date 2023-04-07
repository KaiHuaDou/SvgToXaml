using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace SvgToXaml.Infrastructure;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class InvertableBooleanToVisibilityConverter : IValueConverter
{
    private enum InvertEnum
    {
        // ReSharper disable once UnusedMember.Local
        Normal, Invert
    }

    /// <summary>
    /// converts boolean to Visibility (false = Collapsed, true = Visible)
    /// usage: Visibility="{Binding HasError, Converter={StaticResource InvertableBooleanToVisibilityConverter}, ConverterParameter=Invert}"
    /// </summary>
    /// <param name="value">boolean value to convert</param>
    /// <param name="targetType">not used</param>
    /// <param name="parameter">Inverts the boolean value 
    /// possible values: Invert/Normal, true/false (also as strings)
    /// optional (default false)</param>
    /// <param name="culture">not used</param>
    /// <returns></returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = value == null
            ? false ^ InvertParam(parameter)
            : (bool) value ^ InvertParam(parameter);
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
    {
        return ((value is Visibility visibility) && (visibility == Visibility.Visible)) ^ InvertParam(parameter);
    }

    private bool InvertParam(object param)
    {
        if (param is bool?)
            return ((bool?) param).GetValueOrDefault( );
        switch (param)
        {
            case null: return false;
            case InvertEnum enumParam:
                return enumParam == InvertEnum.Invert;
            case string strParam:
            {
                if (Enum.TryParse(strParam, true, out InvertEnum invert))
                    return invert == InvertEnum.Invert;
                if (bool.TryParse(strParam, out bool aBool))
                    return aBool;
                break;
            }
        }
        throw new InvalidDataException($"{GetType( ).Name}: not able to convert the ConverterParameter to InvertEnum or Boolean [{param}]");
    }
}
