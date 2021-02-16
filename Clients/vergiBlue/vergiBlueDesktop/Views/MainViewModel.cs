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
            
            AiTurnFinished(move);
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

        public void PlayerTurnFinished((int,int) from, (int,int) to)
        {
            var move = new SingleMove(from, to);

            var target = Board.ValueAt(to);
            if (target != null)
            {
                if (target.IsWhite != PlayerIsWhite)
                {
                    move.Capture = true;
                    // TODO promotion

                    var pieceToDelete = ViewObjectList.First(o => o.Column == move.NewPos.Item1 && o.Row == move.NewPos.Item2);
                    ViewObjectList.Remove(pieceToDelete);
                }
                else throw new ArgumentException($"Player tried to capture own piece.");
            }

            Board.ExecuteMove(move);
            TurnFinished(move);
        }
        
        public void AiTurnFinished(SingleMove move)
        {
            // Update view
            if (move.Capture)
            {
                var pieceToDelete = ViewObjectList.First(o => o.Column == move.NewPos.Item1 && o.Row == move.NewPos.Item2);
                ViewObjectList.Remove(pieceToDelete);
            }
            
            var viewObject = ViewObjectList.First(o => o.Column == move.PrevPos.Item1 && o.Row == move.PrevPos.Item2);
            viewObject.UpdatePhysicalLocation(move.NewPos.Item1, move.NewPos.Item2, false);
            viewObject.UpdateAbstractLocation(move.NewPos.Item1, move.NewPos.Item2);

            Board.ExecuteMove(move);
            TurnFinished(move);
        }

        private void UpdateGraphics(SingleMove move)
        {

        }

        public void TurnFinished(SingleMove previousMove)
        {
            IsWhiteTurn = !IsWhiteTurn;
            
            if (IsWhiteTurn != PlayerIsWhite)
            {
                // Ai turn
                AiLogic.ReceiveMove(previousMove.ToInterfaceMove(false,false));
                var interfaceMoveData = AiLogic.CreateMove();
                var nextMove = new SingleMove(interfaceMoveData.Move);

                var target = Board.ValueAt(nextMove.NewPos);
                if (target != null)
                {
                    if (target.IsWhite == PlayerIsWhite)
                    {
                        nextMove.Capture = true;
                        // TODO promotion
                    }
                    else throw new ArgumentException("Ai tried to capture own piece.");
                }

                AiTurnFinished(nextMove);
            }
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
