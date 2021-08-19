using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Lomont.Utility;

namespace Lomont.WPF.Converters
{
    /// <summary>
    /// Convert colored text to WPF elements
    /// </summary>
    public class ColoredTextConverter :IValueConverter
    {
        /// <summary>
        /// Convert forward
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sp = new StackPanel {Orientation = Orientation.Horizontal};
            if (value is string str)
            {
                // todo - these not quite right - color settings don't persist between text pieces
                foreach (var token in ColoredText.Colorize(str))
                {
                    var tb = new TextBlock {Text = token.Text};
                    if (token.SetForeground)
                        tb.Foreground = new SolidColorBrush(MakeColor(token.Foreground));

                    if (token.SetBackground)
                        tb.Background = new SolidColorBrush(MakeColor(token.Background));

                    sp.Children.Add(tb);
                }

                return sp;

                Color MakeColor(ColoredText.Color color) =>
                    Color.FromRgb((byte) color.Red, (byte) color.Green, (byte) color.Blue);

            }

            return $"<unknown item: {value}>";
        }

        /// <summary>
        /// Convert back, not implemented
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
