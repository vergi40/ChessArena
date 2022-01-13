using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using vergiBlue;
using vergiBlue.Pieces;

namespace vergiBlueDesktop.Views
{
    public class PieceViewModel : IPieceWithUiControl
    {
        public bool IsWhite { get; set; }
        public Uri SourceUri { get; set; }
        public PieceBase PieceModel { get; set; }

        public MainViewModel Main { get; }

        /// <summary>
        /// temp
        /// </summary>
        private GameModelProxy _gameModel { get; }

        public PieceViewModel(MainViewModel mainViewModel, GameModelProxy model)
        {
            Main = mainViewModel;
            _gameModel = model;
        }
        
        public void VisualizePossibleTiles()
        {
            var moves = PieceModel.Moves(_gameModel.Model.Session.Board);
            moves = _gameModel.Model.Session.Board.FilterOutIllegalMoves(moves, IsWhite);// TODO how to fix?

            var borderColor = Brushes.Chartreuse;
            if (IsWhite != Main.PlayerIsWhite) borderColor = Brushes.Coral;
            foreach (var singleMove in moves)
            {
                Main.VisualizationTiles.Add(new Position(singleMove.NewPos.row, singleMove.NewPos.column, borderColor));
            }
        }

        public void ClearPossibleTiles()
        {
            Main.VisualizationTiles.Clear();
        }

        public void TurnFinished(Position previousPosition, Position currentPosition)
        {
            var move = new SingleMove((previousPosition.Column, previousPosition.Row),
                (currentPosition.Column, currentPosition.Row));
            _gameModel.Model.TurnFinished(move, false);
        }
    }
}
