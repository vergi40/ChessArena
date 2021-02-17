using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Formats.Asn1;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using vergiBlue;
using vergiBlue.Pieces;

namespace vergiBlueDesktop.Views
{
    public class MainViewModel : NotifyPropertyBase
    {
        private static string IconSet = "kosal";
        
        private bool PlayerIsWhite { get; set; }
        private bool IsWhiteTurn { get; set; }
        
        private Logic AiLogic { get; set; }
        
        /// <summary>
        /// Actual graphics binded to view
        /// </summary>
        public ObservableCollection<IViewObject> ViewObjectList { get; } = new ObservableCollection<IViewObject>();

        public IList<Position> VisualizationTiles { get; } = new ObservableCollection<Position>();

        public ICommand StartWhiteCommand { get; set; }
        public ICommand StartBlackCommand { get; set; }
        public ICommand InitializeCase2Command { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand TestMoveCommand { get; set; }
        
        public Board Board { get; set; }

        public MainViewModel()
        {
            StartWhiteCommand = new RelayCommand<object>(StartWhite);
            StartBlackCommand = new RelayCommand<object>(StartBlack);
            InitializeCase2Command = new RelayCommand<object>(Initialize2);
            StartCommand = new RelayCommand<object>(Start);
            TestMoveCommand = new RelayCommand<object>(TestMove);

        }

        private void StartWhite(object parameter)
        {
            InitializeBoard();
            PlayerIsWhite = true;
            IsWhiteTurn = true;

            AiLogic = new Logic(!PlayerIsWhite, Board);
        }

        private void StartBlack(object parameter)
        {
            InitializeBoard();
            PlayerIsWhite = false;
            IsWhiteTurn = false;
            
            AiLogic = new Logic(!PlayerIsWhite, Board);
            
            var interfaceMoveData = AiLogic.CreateMove();
            var move = new SingleMove(interfaceMoveData.Move);
            
            TurnFinished(move, false);
        }

        private void InitializeBoard()
        {
            Board = new Board();
            Board.InitializeEmptyBoard();

            ViewObjectList.Clear();
            VisualizationTiles.Clear();
            foreach (var piece in Board.PieceList)
            {
                var viewModel = new PieceViewModel(this)
                {
                    IsWhite = piece.IsWhite,
                    PieceModel = piece,
                    SourceUri = GetUriForPiece(piece),
                };

                ViewObjectList.Add(new DraggableItem(viewModel, piece.CurrentPosition.column, piece.CurrentPosition.row));
            }
        }

        private void UpdateGraphics(SingleMove move, bool pieceNotMovedInView)
        {
            // Update view
            if (move.Capture)
            {
                var pieceToDelete = ViewObjectList.First(o => o.Column == move.NewPos.column && o.Row == move.NewPos.row);
                ViewObjectList.Remove(pieceToDelete);
            }

            if (move.Promotion)
            {

            }

            if(pieceNotMovedInView)
            {
                var viewObject =
                    ViewObjectList.First(o => o.Column == move.PrevPos.column && o.Row == move.PrevPos.row);
                viewObject.UpdatePhysicalLocation(move.NewPos.column, move.NewPos.row, false);
                viewObject.UpdateAbstractLocation(move.NewPos.column, move.NewPos.row);
            }
        }

        public void TurnFinished(SingleMove move, bool pieceNotMovedInView)
        {
            ValidateMove(move);
            UpdateGraphics(move, pieceNotMovedInView);

            Board.ExecuteMove(move);
            IsWhiteTurn = !IsWhiteTurn;
            
            if (IsWhiteTurn != PlayerIsWhite)
            {
                // Ai turn
                AiLogic.ReceiveMove(move.ToInterfaceMove(false,false));
                var interfaceMoveData = AiLogic.CreateMove();
                var nextMove = new SingleMove(interfaceMoveData.Move);

                TurnFinished(nextMove, true);
            }
        }

        /// <summary>
        /// Do before value is updated to Board and view
        /// </summary>
        /// <param name="move"></param>
        private void ValidateMove(SingleMove move)
        {
            // TODO actual validation
            // TODO do in vergiBlue project
            
            // Now just check there is info missing
            var targetPiece = Board.ValueAtDefinitely(move.PrevPos);
            var isWhite = targetPiece.IsWhite;

            // Capture
            var opponentPiece = Board.ValueAt(move.NewPos);
            if (opponentPiece != null)
            {
                if (opponentPiece.IsWhite != isWhite)
                {
                    move.Capture = true;
                }
                else throw new ArgumentException($"Player with white={isWhite} tried to capture own piece.");
            }

            // Promotion
            if (isWhite && targetPiece.Identity != 'Q' && move.NewPos.column == 7)
            {
                move.Promotion = true;
            }
            else if (!isWhite && targetPiece.Identity != 'Q' && move.NewPos.column == 0)
            {
                move.Promotion = true;
            }
            
            // Castling
            
            // 
        }

        private void Initialize2(object parameter)
        {
        }

        private void Start(object parameter)
        {
        }

        private void TestMove(object parameter)
        {
        }
        
        private Uri GetUriForPiece(PieceBase piece)
        {
            var name = "";
            if (piece.IsWhite) name += "w";
            else name += "b";

            name += piece.Identity.ToString().ToLower();

            return new Uri($"pack://application:,,,/vergiBlueDesktop;component/Resources/{IconSet}/{name}.svg");
        }
    }
    
}
