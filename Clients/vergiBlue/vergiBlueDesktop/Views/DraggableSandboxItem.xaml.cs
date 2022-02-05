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
    /// Interaction logic for DraggableSandboxItem.xaml
    /// https://stackoverflow.com/questions/1495408/how-to-drag-a-usercontrol-inside-a-canvas
    /// </summary>
    partial class DraggableSandboxItem : UserControl, IViewObject, INotifyPropertyChanged
    {
        private static int BlockSize = GraphicConstants.BlockSize;

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

        public DraggableSandboxItem() { }
        public DraggableSandboxItem(IPieceWithUiControl model, int column, int row)
        {
            Model = model;
            
            Row = 0;
            Column = 0;
            UpdateInternalLocation(column, row);
            IsAtStartPosition = true;

            InitializeComponent();
            UpdateImageLocation(column, row, true);

            this.MouseLeftButtonDown += new MouseButtonEventHandler(UserControl_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(UserControl_MouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(UserControl_MouseMove);
        }

        /// <summary>
        /// Called every time piece image control needs to be moved.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="updatePreviousPixelLocation"></param>
        public void UpdateImageLocation(int column, int row, bool updatePreviousPixelLocation)
        {
            var x = column * BlockSize;
            // UserControl transform system has mirrored y-axis
            var y = (7 - row) * BlockSize;

            if (updatePreviousPixelLocation)
            {
                prevX = x;
                prevY = y;
            }

            var transform = RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                RenderTransform = transform;
            }
            transform.X = x;
            transform.Y = y;
        }

        /// <summary>
        /// Update row & column after successful move
        /// </summary>
        /// <param name="xTransform"></param>
        /// <param name="yTransform"></param>
        private void UpdateInternalLocation(double xTransform, double yTransform)
        {
            var x = xTransform;
            var y = 420 - yTransform;

            var column = (int)Math.Round(x / BlockSize);
            var row = (int)Math.Round(y / BlockSize);

            UpdateInternalLocation(column, row);
        }

        /// <summary>
        /// Update row & column after successful move
        /// </summary>
        public void UpdateInternalLocation(int column, int row)
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
            // Remove piece
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.Delete))
            {
                Model.Main.RemovePiece(this);
                return;
            }

            isDragging = true;
            var draggableControl = sender as UserControl;
            mousePosition = e.GetPosition(Parent as UIElement);
            draggableControl.CaptureMouse();

            // Might throw if bad position
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
                    // Return
                }
                else
                {
                    prevX = transform.X;
                    prevY = transform.Y;

                    UpdateInternalLocation(prevX, prevY);
                    turnFinished = true;
                }
            }
            draggable.ReleaseMouseCapture();
            Model.ClearPossibleTiles();

            if (turnFinished) Model.SandboxTurnFinished(PreviousPosition, CurrentPosition);
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
                x = Math.Round(x / BlockSize) * BlockSize;
                y = Math.Round(y / BlockSize) * BlockSize;

                // Set position with diff and previous position
                transform.X = x + prevX;
                transform.Y = y + prevY;
            }
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
}
