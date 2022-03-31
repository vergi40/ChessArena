using System.Collections.Generic;
using CommonNetStandard.Interface;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;

namespace vergiBlue.Pieces
{
    public interface IPiece : IPieceMinimal
    {
        /// <summary>
        /// Static strength for piece type. White positive, black negative.
        /// </summary>
        double RelativeStrength { get; }

        /// <summary>
        /// Static strenght for combination of piece type and position.
        /// </summary>
        double PositionStrength { get; }

        /// <summary>
        /// Sign of general direction. Can also be used to classify white as positive and black as negative value.
        /// </summary>
        int Direction { get; }

        /// <summary>
        /// Position strenght that varies depending how far the game is
        /// </summary>
        /// <param name="endGameWeight"></param>
        /// <returns></returns>
        double GetEvaluationStrength(double endGameWeight = 0);

        /// <summary>
        /// Lightweight validation if piece in current position can attack target (king)
        /// </summary>
        bool CanAttackQuick((int column, int row) target, IBoard board);

        /// <summary>
        /// Each move the piece can make in current board setting
        /// </summary>
        /// <returns></returns>
        IEnumerable<SingleMove> Moves(IBoard board);

        /// <summary>
        /// List all capture moves for knowing possible attack squares.
        /// "Pseudo" as the pawn captures are listed even though there is no opponent in target square
        /// </summary>
        IEnumerable<SingleMove> PseudoCaptureMoves(IBoard board);

        bool TryCreateSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack);

        /// <summary>
        /// Slider move to one direction, Rook, bishop, queen
        /// </summary>
        IReadOnlyList<(int column, int row)> MovesValidatedToDirection((int x, int y) direction);
    }
}
