using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace vergiBlueDesktop.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public const int BlockSize = 60;
        static readonly Brush DarkTile = Brushes.DarkGray;
        static readonly Brush LightTile = Brushes.AntiqueWhite;
        static readonly Brush BorderColor = Brushes.Black;
        
        public MainView()
        {
            InitializeComponent();
            DrawRectangles();
        }

        void DrawRectangles()
        {
            var isWhite = false;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    AddRectangle(i * BlockSize, j * BlockSize, isWhite);
                    isWhite = !isWhite;
                }

                isWhite = !isWhite;
            }
        }

        void AddRectangle(double x, double y, bool isWhite)
        {
            BoardTileBackground.Margin = new Thickness();
            var rec = DrawRectangle(isWhite);
            BoardTileBackground.Children.Add(rec);
            Canvas.SetLeft(rec, x);
            Canvas.SetBottom(rec, y);
        }

        Rectangle DrawRectangle(bool isWhite)
        {
            var rec = new Rectangle();
            rec.Height = BlockSize;
            rec.Width = BlockSize;
            rec.HorizontalAlignment = HorizontalAlignment.Left;
            rec.VerticalAlignment = VerticalAlignment.Bottom;

            if (isWhite)
            {
                rec.Fill = LightTile;
                rec.Stroke = BorderColor;
            }
            else
            {
                rec.Fill = DarkTile;
                rec.Stroke = BorderColor;
            }

            return rec;
        }
    }

    // https://stackoverflow.com/questions/534575/how-do-i-invert-booleantovisibilityconverter
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        enum Parameters
        {
            Normal, NormalHidden, Inverted, InvertedHidden
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var boolValue = (bool?)value;
            if (boolValue == null) return Visibility.Visible;
            
            var direction = Parameters.Normal;
            if(parameter != null) direction = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);

            switch (direction)
            {
                case Parameters.Normal:
                    return boolValue.Value ? Visibility.Visible : Visibility.Collapsed;
                case Parameters.NormalHidden:
                    return boolValue.Value ? Visibility.Visible : Visibility.Hidden;
                case Parameters.Inverted:
                    return !boolValue.Value ? Visibility.Visible : Visibility.Collapsed;
                case Parameters.InvertedHidden:
                    return !boolValue.Value ? Visibility.Visible : Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    // https://stackoverflow.com/questions/1039636/how-to-bind-inverse-boolean-properties-in-wpf
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

}
