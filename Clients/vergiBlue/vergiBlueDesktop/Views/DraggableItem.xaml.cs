using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SharpVectors.Converters;
using vergiBlue;

namespace vergiBlueDesktop.Views
{
    /// <summary>
    /// Interaction logic for DraggableItem.xaml
    /// https://stackoverflow.com/questions/1495408/how-to-drag-a-usercontrol-inside-a-canvas
    /// </summary>
    partial class DraggableItem : UserControl, IViewObject, INotifyPropertyChanged
    {
        public const int BlockSize = 60;

        
        public Uri SourceUri => Model.SourceUri;
        public bool IsWhite => Model.IsWhite;

        /// <summary>
        /// Override if needed
        /// </summary>
        public bool IsAtStartPosition { get; set; }

        protected bool isDragging;
        private Point mousePosition;
        private double prevX, prevY;

        /// <summary>
        /// Current piece row. Left bottom is 0.
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// Current piece column. Left bottom is 0.
        /// </summary>
        public int Column { get; private set; }

        public Position CurrentPosition => new Position(Row, Column);
        public Position PreviousPosition { get; set; }

        public IPieceWithUiControl Model { get; set; }

        public DraggableItem() { }
        public DraggableItem(IPieceWithUiControl model, int column, int row)
        {
            Model = model;

            Row = 0;
            Column = 0;
            UpdateAbstractLocation(column, row);
            IsAtStartPosition = true;

            InitializeComponent();
            UpdatePhysicalLocation(column, row, true);

            this.MouseLeftButtonDown += new MouseButtonEventHandler(UserControl_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(UserControl_MouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(UserControl_MouseMove);
        }

        /// <summary>
        /// Called every time piece image control needs to be moved.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="initialization"></param>
        public void UpdatePhysicalLocation(int column, int row, bool initialization)
        {
            var x = column * BlockSize;
            // UserControl transform system has mirrored y-axis
            var y = 420 - row * BlockSize;

            if (initialization)
            {
                prevX = x;
                prevY = y;
            }

            var transform = this.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                this.RenderTransform = transform;
            }
            transform.X = x;
            transform.Y = y;
        }

        /// <summary>
        /// Update row & column after successful move
        /// </summary>
        /// <param name="xTransform"></param>
        /// <param name="yTransform"></param>
        private void UpdateAbstractLocation(double xTransform, double yTransform)
        {
            var x = xTransform;
            var y = 420 - yTransform;

            var column = (int)Math.Round(x / BlockSize);
            var row = (int)Math.Round(y / BlockSize);

            UpdateAbstractLocation(column, row);
        }

        /// <summary>
        /// Update row & column after successful move
        /// </summary>
        public void UpdateAbstractLocation(int column, int row)
        {
            // Save previous
            PreviousPosition = new Position(Row, Column);
            Column = column;
            Row = row;

            if (IsAtStartPosition) IsAtStartPosition = false;
        }


        // Usercontrol transform logic

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var draggableControl = (sender as UserControl);
            mousePosition = e.GetPosition(Parent as UIElement);
            draggableControl.CaptureMouse();

            Model.VisualizePossibleTiles();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var turnFinished = false;
            var draggable = (sender as UserControl);
            var transform = (draggable.RenderTransform as TranslateTransform);
            if (transform != null)
            {
                // Piece in the same position - cancel
                if (Math.Abs(prevX - transform.X) < double.Epsilon
                    && Math.Abs(prevY - transform.Y) < double.Epsilon)
                {
                    //return;
                }
                else
                {
                    // Piece in not-allowed position
                    // TODO


                    // Piece in new, allowed position
                    prevX = transform.X;
                    prevY = transform.Y;

                    UpdateAbstractLocation(prevX, prevY);
                    turnFinished = true;
                }
            }
            draggable.ReleaseMouseCapture();
            Model.ClearPossibleTiles();

            if (turnFinished) Model.TurnFinished(PreviousPosition, CurrentPosition);
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            var draggableControl = (sender as UserControl);
            if (isDragging && draggableControl != null)
            {
                var currentPosition = e.GetPosition(Parent as UIElement);
                var transform = (draggableControl.RenderTransform as TranslateTransform);
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    draggableControl.RenderTransform = transform;
                }

                // diff
                var x = (currentPosition.X - mousePosition.X);
                var y = (currentPosition.Y - mousePosition.Y);

                // Only allow movement to tiles
                x = Math.Round(x / 60) * 60;
                y = Math.Round(y / 60) * 60;

                // Set position with diff and previous position
                transform.X = x + prevX;
                transform.Y = y + prevY;
                
                // TODO restrict borders

                //transform.X += prevX;
                //transform.Y += prevY;

                //if (prevX > 0)
                //{
                //    transform.X += prevX;
                //    transform.Y += prevY;
                //}
            }
        }

        public void TestIncrRow()
        {
            UpdatePhysicalLocation(Column, Row + 1, false);
            Row++;
        }


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged(string propertyName)
        {

            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));

        }

        public void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression unaryExpression)
            {
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }
            OnPropertyChanged(memberExpression.Member.Name);
        }

    }

    // https://stackoverflow.com/questions/35036894/how-to-bind-to-an-unbindable-property-without-violating-mvvm
    public class SvgViewboxAttachedProperties : DependencyObject
    {
        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var svgControl = obj as SvgViewbox;
            if (svgControl != null)
            {
                var path = (string)e.NewValue;
                svgControl.Source = string.IsNullOrWhiteSpace(path) ? default(Uri) : new Uri(path);
            }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source",
                typeof(string), typeof(SvgViewboxAttachedProperties),
                // default value: null
                new PropertyMetadata(null, OnSourceChanged));
    }
}
