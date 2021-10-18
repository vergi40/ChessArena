using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using vergiBlue;
using vergiBlue.Pieces;

namespace vergiBlueDesktop.Views
{
    /// <summary>
    /// Update ideas:
    /// * AI doesn't recognize concept of draw. For endgame this is crucial. This needs fine-tuning in evaluation function (commented out)
    ///
    /// * Better highlighing for previous move
    /// * Do a click sound when moving
    /// * Add ability to change board color and piece icons
    /// * Add timer
    /// * Ask ai to give hint
    /// * Endgame scenarios with thumbnail pictures
    /// * Get all possible moves and compare to made move. Eg king cant be lost on purpose
    /// * Show captured pieces on top and bottom, in place of buttons
    /// * - Also highlight player which turn is going
    /// </summary>
    public class MainViewModel : NotifyPropertyBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MainViewModel));
        private bool _gameStarted = false;
        private bool _isBusy;


        private static string IconSet = "kosal";
        private int TurnCount { get; set; } = 0;


        // --------------
        // View binded

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(() => IsBusy);
            }
        }

        public bool GameStarted
        {
            get => _gameStarted;
            set
            {
                _gameStarted = value;
                OnPropertyChanged(() => GameStarted);
            }
        }

        // ---------------


        public bool PlayerIsWhite { get; set; }
        public bool IsWhiteTurn { get; set; }
        
        private Logic AiLogic { get; set; }

        /// <summary>
        /// As long as binding is oneway (only view-side can alter), no need to implement notifyproperty
        /// </summary>
        public LogicSettings AiLogicSettings { get; set; } = new LogicSettings();
        
        /// <summary>
        /// Actual graphics binded to view
        /// </summary>
        public ObservableCollection<IViewObject> ViewObjectList { get; } = new ObservableCollection<IViewObject>();

        public IList<Position> VisualizationTiles { get; } = new ObservableCollection<Position>();
        
        // TODO this is really a single object
        public IList<Position> PreviousPosition { get; } = new ObservableCollection<Position>();

        /// <summary>
        /// Auto-rolling move history.
        /// </summary>
        public IList<string> History { get; } = new ObservableCollection<string>();
        public IList<string> AiMoveDiagnostics { get; } = new ObservableCollection<string>();
        public IList<string> AiPreviousMoveDiagnostics { get; } = new ObservableCollection<string>();

        public ICommand StartWhiteCommand { get; set; }
        public ICommand StartBlackCommand { get; set; }
        public ICommand ForfeitCommand { get; set; }
        public ICommand Test1Command { get; set; }
        public ICommand Test2Command { get; set; }
        public ICommand Test3Command { get; set; }
        
        public Board Board { get; set; }

        public MainViewModel()
        {
            StartWhiteCommand = new RelayCommand<object>(StartWhite);
            StartBlackCommand = new RelayCommand<object>(StartBlack);
            ForfeitCommand = new RelayCommand<object>(Forfeit);
            Test1Command = new RelayCommand<object>(DoubleRookTest);
            Test2Command = new RelayCommand<object>(PromotionTest);
            Test3Command = new RelayCommand<object>(CastlingTest);

        }

        private void StartWhite(object parameter)
        {
            ViewUpdateGameStart();
            InitializeEnvironment();
            PlayerIsWhite = true;
            IsWhiteTurn = true;

            AiLogic = new Logic(!PlayerIsWhite, Board);
            AiLogic.Settings = AiLogicSettings;
        }

        private void StartBlack(object parameter)
        {
            ViewUpdateGameStart();
            InitializeEnvironment();
            PlayerIsWhite = false;
            IsWhiteTurn = true;
            
            AiLogic = new Logic(!PlayerIsWhite, Board);
            AiLogic.Settings = AiLogicSettings;

            var interfaceMoveData = AiLogic.CreateMove();
            UpdateAiDiagnostics(interfaceMoveData.Diagnostics);
            var move = new SingleMove(interfaceMoveData.Move);

            TurnFinished(move, true);
        }

        private void Forfeit(object parameter)
        {
            ViewUpdateGameEnd();
        }

        private void ViewUpdateGameStart()
        {
            GameStarted = true;
        }

        private void ViewUpdateGameEnd()
        {
            GameStarted = false;
        }

        private void InitializeEnvironment(Board initializedBoard = null)
        {
            TurnCount = 0;
            History.Clear();
            AiMoveDiagnostics.Clear();
            AiPreviousMoveDiagnostics.Clear();

            if (initializedBoard == null)
            {
                initializedBoard = new Board();
                initializedBoard.InitializeEmptyBoard();
            }

            Board = initializedBoard;
            
            ViewObjectList.Clear();
            VisualizationTiles.Clear();
            PreviousPosition.Clear();
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
            // Update view objects
            if (move.Capture)
            {
                // If user move, there exists 2 viewobjects in same square
                var pieceToDelete = ViewObjectList.First(o => o.Column == move.NewPos.column && o.Row == move.NewPos.row && o.IsWhite != IsWhiteTurn);
                ViewObjectList.Remove(pieceToDelete);
            }
            
            if(pieceNotMovedInView)
            {
                var viewObject =
                    ViewObjectList.First(o => o.Column == move.PrevPos.column && o.Row == move.PrevPos.row);
                viewObject.UpdateImageLocation(move.NewPos.column, move.NewPos.row, false);
                viewObject.UpdateInternalLocation(move.NewPos.column, move.NewPos.row);
            }
            
            // Update last position
            PreviousPosition.Clear();
            PreviousPosition.Add(new Position(move.PrevPos.row, move.PrevPos.column));
            PreviousPosition.Add(new Position(move.NewPos.row, move.NewPos.column));
        }

        public async void TurnFinished(SingleMove move, bool pieceNotMovedInView)
        {
            move = Board.CollectMoveProperties(move);
            UpdateGraphics(move, pieceNotMovedInView);
            Board.ExecuteMove(move);
            UpdatePostGraphics(move);
            
            // Game ended?
            if (move.CheckMate)
            {
                WriteMoveHistory($"{move.ToString()} - Checkmate.");
                ViewUpdateGameEnd();
                return;
            }
            
            AppendHistory(move);
            IsWhiteTurn = !IsWhiteTurn;
            
            if (IsWhiteTurn != PlayerIsWhite)
            {
                // Ai turn
                IsBusy = true;
                AiLogic.ReceiveMove(move.ToInterfaceMove());
                var interfaceMoveData = await Task.Run(() => AiLogic.CreateMove());
                UpdateAiDiagnostics(interfaceMoveData.Diagnostics);
                IsBusy = false;
                var nextMove = new SingleMove(interfaceMoveData.Move);

                TurnFinished(nextMove, true);
            }
        }

        /// <summary>
        /// Handle stuff that needs Board.ExecuteMove to be finished first
        /// </summary>
        /// <param name="move"></param>
        private void UpdatePostGraphics(SingleMove move)
        {
            if (move.Promotion)
            {
                // Model-piece is correct. Image-piece is moved, but wrong picture
                var pieceToDelete = ViewObjectList.First(o => o.Column == move.NewPos.column && o.Row == move.NewPos.row && o.IsWhite == IsWhiteTurn);
                ViewObjectList.Remove(pieceToDelete);

                var promotionPieceBase = Board.ValueAtDefinitely(move.NewPos);
                var promotionPiece = new DraggableItem(
                    new PieceViewModel(this)
                    {
                        IsWhite = promotionPieceBase.IsWhite,
                        PieceModel = promotionPieceBase,
                        SourceUri = GetUriForPiece(promotionPieceBase),
                    },
                    move.NewPos.column, move.NewPos.row);
                ViewObjectList.Add(promotionPiece);
            }

            if (move.Castling)
            {
                // TODO find rook position and add here
                var row = move.NewPos.row;
                if (move.NewPos.column == 2)
                {
                    // Left
                    var viewObject = ViewObjectList.First(o => o.Column == 0 && o.Row == row);
                    viewObject.UpdateImageLocation(3, row, true);
                    viewObject.UpdateInternalLocation(3, row);
                }
                else if (move.NewPos.column == 6)
                {
                    var viewObject = ViewObjectList.First(o => o.Column == 7 && o.Row == row);
                    viewObject.UpdateImageLocation(5, row, true);
                    viewObject.UpdateInternalLocation(5, row);
                }
            }
        }

        private void AppendHistory(SingleMove move)
        {
            TurnCount++;
            var text = TurnCount.ToString() + ": " + move.ToString();
            if (Board.IsCheck(IsWhiteTurn)) text += " Check.";
            WriteMoveHistory(text);
        }

        private void WriteMoveHistory(string message)
        {
            _logger.Info($"{nameof(History)}: {message}");
            History.Insert(0, message);
        }
        
        private void UpdateAiDiagnostics(string dataString)
        {
            if (AiMoveDiagnostics.Any())
            {
                AiPreviousMoveDiagnostics.Clear();
                foreach (var item in AiMoveDiagnostics)
                {
                    AiPreviousMoveDiagnostics.Add(item);
                }
            }
            
            AiMoveDiagnostics.Clear();

            _logger.Info($"{nameof(AiMoveDiagnostics)}: {dataString}");
            var list = dataString.Split(". ");
            foreach (var item in list)
            {
                AiMoveDiagnostics.Add(item);
            }
        }
        
        private void DoubleRookTest(object parameter)
        {
            // 8   K
            // 7   R
            // 6
            // 5   R
            // 4
            // 3
            // 2    K
            // 1
            //  ABCDEFGH
            var board = new Board();
            var pieces = new List<PieceBase>
            {
                new Rook(false, "d5"),
                new Rook(false, "d7"),
                new King(true, "e2"),
                new King(false, "d8")
            };
            board.AddNew(pieces);
            
            ViewUpdateGameStart();
            InitializeEnvironment(board);
            PlayerIsWhite = true;
            IsWhiteTurn = true;

            AiLogic = new Logic(!PlayerIsWhite, Board);
            AiLogic.Settings = AiLogicSettings;
        }

        private void PromotionTest(object parameter)
        {
            // 8 
            // 7 P
            // 6     K
            // 5   
            // 4
            // 3     K
            // 2  P  
            // 1  
            //  ABCDEFGH
            var board = new Board();
            var pieces = new List<PieceBase>
            {
                new King(true, "f6"),
                new King(false, "f3"),
                new Pawn(true, "b7"),
                new Pawn(false, "c2"),
            };
            board.AddNew(pieces);

            ViewUpdateGameStart();
            InitializeEnvironment(board);
            PlayerIsWhite = true;
            IsWhiteTurn = true;

            AiLogic = new Logic(!PlayerIsWhite, Board);
            AiLogic.Settings = AiLogicSettings;
        }

        private void CastlingTest(object parameter)
        {
            // 8    K  R
            // 7     PPP
            // 6 P
            // 5   
            // 4
            // 3P
            // 2     PPP
            // 1    K  R
            //  ABCDEFGH
            var board = new Board();
            var pieces = new List<PieceBase>
            {
                new Rook(true, "h1"),
                new Rook(false, "h8"),
                new King(true, "e1"),
                new King(false, "e8"),
                new Pawn(true, "f2"),
                new Pawn(true, "g2"),
                new Pawn(true, "h2"),
                new Pawn(true, "a3"),
                new Pawn(false, "f7"),
                new Pawn(false, "g7"),
                new Pawn(false, "h7"),
                new Pawn(false, "b6")

            };
            board.AddNew(pieces);

            ViewUpdateGameStart();
            InitializeEnvironment(board);
            PlayerIsWhite = true;
            IsWhiteTurn = true;

            AiLogic = new Logic(!PlayerIsWhite, Board);
            AiLogic.Settings = AiLogicSettings;
        }

        private void Test2(object parameter)
        {
        }

        private void Test3(object parameter)
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
