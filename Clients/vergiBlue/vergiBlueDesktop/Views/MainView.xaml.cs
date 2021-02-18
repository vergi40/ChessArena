using System.Windows;
using System.Windows.Controls;
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
    
}
