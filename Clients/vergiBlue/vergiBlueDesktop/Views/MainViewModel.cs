﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using vergiBlue;
using vergiBlue.Logic;
using vergiBlue.Pieces;

namespace vergiBlueDesktop.Views
{
    /// <summary>
    /// Update ideas:
    /// * AI doesn't recognize concept of draw. For endgame this is crucial. This needs fine-tuning in evaluation function (commented out)
    ///
    /// UX/UI
    /// * Better highlighing for previous move
    /// * Do a click sound when moving
    /// * Add ability to change board color and piece icons
    /// * Add timer
    /// * Ask ai to give hint
    /// * Endgame scenarios with thumbnail pictures
    /// * Get all possible moves and compare to made move. Eg king cant be lost on purpose
    /// * Show captured pieces on top and bottom, in place of buttons
    /// * - Also highlight player which turn is going
    ///
    /// General
    /// * Save user settings to local app.config. Add restore defaults button
    ///
    /// Design ideas
    /// * Make this as an example MVVM project
    /// * Substitute GameControllerWrapper with better design
    /// * Proper bindings and decoupling
    /// * Each piece it's own viewmodel
    /// </summary>
    public class MainViewModel : NotifyPropertyBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MainViewModel));
        private bool _gameStarted = false;
        private bool _isBusy;

        private static string IconSet = "kosal";


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


        public bool PlayerIsWhite
        {
            get
            {
                if (_session == null) return true;
                return _session.PlayerIsWhite;
            }
        }

        public bool IsWhiteTurn
        {
            get
            {
                if (_session == null) return true;
                return _session.IsWhiteTurn;
            }
        }


        /// <summary>
        /// Actual graphics binded to view
        /// </summary>
        public ObservableCollection<IViewObject> ViewObjectList { get; } = new ObservableCollection<IViewObject>();

        /// <summary>
        /// Binded
        /// </summary>
        public IList<Position> VisualizationTiles { get; } = new ObservableCollection<Position>();

        // TODO this is really a single object
        /// <summary>
        /// Binded
        /// </summary>
        public IList<Position> PreviousPosition { get; } = new ObservableCollection<Position>();

        /// <summary>
        /// As long as binding is oneway (only view-side can alter), no need to implement notifyproperty
        /// </summary>
        public LogicSettings AiLogicSettings { get; set; } = new LogicSettings();

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

        private GameSession _session { get; set; }

        public MainViewModel()
        {
        }

        public void InitializeViewModel(GameSession session, GameModelProxy modelProxy)
        {
            _session = session;

            History.Clear();
            AiMoveDiagnostics.Clear();
            AiPreviousMoveDiagnostics.Clear();

            ViewObjectList.Clear();
            VisualizationTiles.Clear();
            PreviousPosition.Clear();

            // Add pieces
            foreach (var piece in _session.Board.PieceList)
            {
                AddUiPiece(piece, piece.CurrentPosition.column, piece.CurrentPosition.row, modelProxy);
            }
        }

        /// <summary>
        /// Enable game board
        /// </summary>
        public void ViewUpdateGameStart()
        {
            GameStarted = true;
        }

        /// <summary>
        /// Disable game board
        /// </summary>
        public void ViewUpdateGameEnd()
        {
            GameStarted = false;
        }
        
        /// <summary>
        /// </summary>
        /// <param name="move"></param>
        /// <param name="pieceNotMovedInView">Piece in view not updated yet</param>
        public void UpdateGraphics(SingleMove move, bool pieceNotMovedInView)
        {
            // Update view objects
            if (move.Capture)
            {
                // If user move, there exists 2 viewobjects in same square
                var pieceToDelete = ViewObjectList.First(o =>
                    o.Column == move.NewPos.column 
                    && o.Row == move.NewPos.row 
                    && o.IsWhite != IsWhiteTurn);
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
        /// <summary>
        /// Handle stuff that needs Board.ExecuteMove to be finished first
        /// </summary>
        public void UpdatePostGraphics(SingleMove move, GameModelProxy proxy)
        {
            if (move.Promotion)
            {
                // Model-piece is correct. Image-piece is moved, but wrong picture
                var pieceToDelete = ViewObjectList.First(o => 
                    o.Column == move.NewPos.column 
                    && o.Row == move.NewPos.row 
                    && o.IsWhite == IsWhiteTurn);
                ViewObjectList.Remove(pieceToDelete);

                var promotionPieceBase = proxy.Model.Session.Board.ValueAtDefinitely(move.NewPos);
                AddPromotionPiece(promotionPieceBase, move, proxy);
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


        public void AppendHistory(SingleMove move, int turnCount, bool isCheck)
        {
            var text = turnCount.ToString() + ": " + move.ToString();
            if (isCheck) text += " Check.";
            AppendHistory(text);
        }

        public void AppendHistory(string message)
        {
            _logger.Info($"{nameof(History)}: {message}");
            History.Insert(0, message);
        }
        
        public void UpdateAiDiagnostics(string dataString)
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
        
        private Uri GetUriForPiece(PieceBase piece)
        {
            var name = "";
            if (piece.IsWhite) name += "w";
            else name += "b";

            name += piece.Identity.ToString().ToLower();

            return new Uri($"pack://application:,,,/vergiBlueDesktop;component/Resources/{IconSet}/{name}.svg");
        }
        
        public void AddPromotionPiece(PieceBase piece, SingleMove move, GameModelProxy modelProxy)
        {
            AddUiPiece(piece, move.NewPos.column, move.NewPos.row, modelProxy);
        }

        /// <summary>
        /// Add piece to view
        /// </summary>
        public void AddUiPiece(PieceBase piece, int column, int row, GameModelProxy modelProxy)
        {
            var uiPiece = new DraggableItem(
                new PieceViewModel(this, modelProxy)
                {
                    IsWhite = piece.IsWhite,
                    PieceModel = piece,
                    SourceUri = GetUriForPiece(piece),
                },
                column, row);
            ViewObjectList.Add(uiPiece);
        }
    }
}
