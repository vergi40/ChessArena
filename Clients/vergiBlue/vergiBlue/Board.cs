﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Common;
using vergiBlue.Pieces;

namespace vergiBlue
{
    public class Board
    {
        /// <summary>
        /// Pieces are storaged with (column,row) pair. On algebraic notation [0,0] corresponds to the "a1" notations.
        /// Indexes start from 0
        /// </summary>
        public Dictionary<(int column, int row), PieceBase> Pieces { get; set; } = new Dictionary<(int, int), PieceBase>();

        // Reference
        public Dictionary<(int, int), PieceBase>.ValueCollection PieceList => Pieces.Values;
        public Dictionary<(int, int), PieceBase>.KeyCollection OccupiedCoordinates => Pieces.Keys;

        /// <summary>
        /// Start game initialization
        /// </summary>
        public Board(){}

        /// <summary>
        /// Create board setup after move
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="move"></param>
        public Board(Board previous, SingleMove move)
        {
            InitializeFromReference(previous);
            ExecuteMove(move);
        }

        /// <summary>
        /// Apply single move to board.
        /// </summary>
        /// <param name="move"></param>
        public void ExecuteMove(SingleMove move)
        {
            if (move.Capture)
            {
                Pieces.Remove(move.NewPos);
            }

            var piece = Pieces[move.PrevPos];
            Pieces.Remove(move.PrevPos);
            Pieces.Add(move.NewPos, piece);
            piece.CurrentPosition = move.NewPos;
        }

        private void InitializeFromReference(Board previous)
        {
            foreach (var oldPiece in previous.PieceList)
            {
                var newPiece = oldPiece.CreateCopy(this);
                AddNew(newPiece);
            }
        }

        /// <summary>
        /// Return piece at coordinates, null if empty.
        /// </summary>
        /// <returns>Can be null</returns>
        public PieceBase ValueAt((int, int) target)
        {
            if (Pieces.ContainsKey(target)) return Pieces[target];
            return null;
        }

        public void AddNew(PieceBase piece)
        {
            Pieces.Add((piece.CurrentPosition), piece);
        }

        public double Evaluate()
        {
            // TODO
            Diagnostics.IncrementEvalCount();
            return PieceList.Sum(p => p.RelativeStrength);
        }

        public IEnumerable<SingleMove> Moves(bool forWhite)
        {
            foreach (var piece in PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves())
                {
                    yield return singleMove;
                }
            }
        }

        public void InitializeEmptyBoard()
        {
            // Pawns
            for (int i = 0; i < 8; i++)
            {
                var whitePawn = new Pawn(true, this);
                whitePawn.CurrentPosition = (i, 1);
                AddNew(whitePawn);

                var blackPawn = new Pawn(false, this);
                blackPawn.CurrentPosition = (i, 6);
                AddNew(blackPawn);
            }

            // Rooks
            var rook = new Rook(true, this);
            rook.CurrentPosition = (0,0);
            AddNew(rook);

            rook = new Rook(true, this);
            rook.CurrentPosition = (7, 0);
            AddNew(rook);

            rook = new Rook(false, this);
            rook.CurrentPosition = (0, 7);
            AddNew(rook);

            rook = new Rook(false, this);
            rook.CurrentPosition = (7, 7);
            AddNew(rook);

            Logger.Log("Board initialized.");
        }
    }
}