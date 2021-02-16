using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue;
using vergiBlue.Pieces;

namespace vergiBlueDesktop.Views
{
    public class PieceViewModel : IPieceWithUiControl
    {
        public bool IsWhite { get; set; }
        public Uri SourceUri { get; set; }
        public PieceBase PieceModel { get; set; }

        private readonly MainViewModel _main;

        public PieceViewModel(MainViewModel mainViewModel)
        {
            _main = mainViewModel;}
        
        public void VisualizePossibleTiles()
        {
            var moves = PieceModel.Moves(_main.Board);
            foreach (var singleMove in moves)
            {
                _main.VisualizationTiles.Add(new Position(singleMove.NewPos.Item2, singleMove.NewPos.Item1));
            }
        }

        public void ClearPossibleTiles()
        {
            _main.VisualizationTiles.Clear();
        }

        public void TurnFinished(Position previousPosition, Position currentPosition)
        {
            _main.PlayerTurnFinished((previousPosition.Column, previousPosition.Row), (currentPosition.Column, currentPosition.Row));
        }
    }
}
