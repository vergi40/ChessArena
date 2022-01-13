using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using vergiBlue;
using vergiBlue.Pieces;
using vergiBlueDesktop.Views;

namespace vergiBlueDesktop
{
    /// <summary>
    /// Controls flow between viewmodel - user - ai logic.
    /// Controller instantiates AI and transmits moves between human and AI
    /// </summary>
    public class GameModel
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GameModel));
        
        private int TurnCount { get; set; } = 0;
        
        private Logic AiLogic { get; set; }

        private MainViewModel _viewModel { get; }

        /// <summary>
        /// temp
        /// </summary>
        private GameModelProxy _proxy { get; }
        public GameSession Session { get; set; }
        
        public GameModel(MainViewModel viewModel)
        {
            _viewModel = viewModel;
            _proxy = new GameModelProxy(this);
            
            // Bind buttons
            _viewModel.StartWhiteCommand = new RelayCommand<object>(StartWhite);
            _viewModel.StartBlackCommand = new RelayCommand<object>(StartBlack);
            _viewModel.ForfeitCommand = new RelayCommand<object>(Forfeit);
            _viewModel.Test1Command = new RelayCommand<object>(DoubleRookTest);
            _viewModel.Test2Command = new RelayCommand<object>(PromotionTest);
            _viewModel.Test3Command = new RelayCommand<object>(CastlingTest);
        }

        private void StartWhite(object parameter)
        {
            _viewModel.ViewUpdateGameStart();
            InitializeEnvironment(true, true);
        }

        private void StartBlack(object parameter)
        {
            _viewModel.ViewUpdateGameStart();
            InitializeEnvironment(false, true);

            var interfaceMoveData = AiLogic.CreateMove();
            _viewModel.UpdateAiDiagnostics(interfaceMoveData.Diagnostics);
            var move = new SingleMove(interfaceMoveData.Move);

            TurnFinished(move, true);
        }

        public void Forfeit(object parameter)
        {
            _viewModel.ViewUpdateGameEnd();
        }

        /// <summary>
        /// Initialize board and view with pieces
        /// </summary>
        private void InitializeEnvironment(bool playerIsWhite, bool isWhiteTurn, Board initializedBoard = null)
        {
            TurnCount = 0;
            if (initializedBoard == null)
            {
                initializedBoard = new Board();
                initializedBoard.InitializeEmptyBoard();
            }

            Session = new GameSession(initializedBoard, playerIsWhite, isWhiteTurn, _viewModel.AiLogicSettings);

            AiLogic = new Logic(!playerIsWhite, initializedBoard);
            AiLogic.Settings = Session.Settings;

            _viewModel.InitializeViewModel(Session, _proxy);
        }
        
        // TODO this needs some refactoring to smaller 
        public async void TurnFinished(SingleMove move, bool pieceNotMovedInView)
        {
            move = Session.Board.CollectMoveProperties(move);
            _viewModel.UpdateGraphics(move, pieceNotMovedInView);
            Session.Board.ExecuteMove(move);
            _viewModel.UpdatePostGraphics(move, _proxy);

            // Game ended?
            if (move.CheckMate)
            {
                _viewModel.AppendHistory($"{move.ToString()} - Checkmate.");
                _viewModel.ViewUpdateGameEnd();
                return;
            }

            TurnCount++;
            _viewModel.AppendHistory(move, TurnCount, Session.Board.IsCheck(Session.IsWhiteTurn));
            Session.TurnChanged();

            if (Session.IsWhiteTurn != Session.PlayerIsWhite)
            {
                // Ai turn
                // TODO some busy-wrapper
                _viewModel.IsBusy = true;
                // TODO synchronous
                AiLogic.ReceiveMove(move.ToInterfaceMove());
                var interfaceMoveData = await Task.Run(() => AiLogic.CreateMove());
                _viewModel.UpdateAiDiagnostics(interfaceMoveData.Diagnostics);
                _viewModel.IsBusy = false;
                var nextMove = new SingleMove(interfaceMoveData.Move);

                TurnFinished(nextMove, true);
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

            _viewModel.ViewUpdateGameStart();
            InitializeEnvironment(true, true, board);
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

            _viewModel.ViewUpdateGameStart();
            InitializeEnvironment(true, true, board);

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

            _viewModel.ViewUpdateGameStart();
            InitializeEnvironment(true, true, board);
        }

        private void Test2(object parameter)
        {
        }

        private void Test3(object parameter)
        {
        }
    }

    /// <summary>
    /// TODO temp proxy for controller methods and properties.
    /// Bad design
    /// </summary>
    public class GameModelProxy
    {
        public GameModel Model { get; }

        public GameModelProxy(GameModel gameModel)
        {
            Model = gameModel;
        }
    }
}
