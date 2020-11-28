﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using CommonNetStandard.LocalImplementation;

namespace vergiBlue
{
    public class SingleMove : IEquatable<SingleMove>
    {
        public bool Capture { get; set; }
        public bool Promotion { get; set; }
        public bool CheckMate { get; set; }

        public (int, int) PrevPos { get; }
        public (int, int) NewPos { get; }

        public SingleMove((int column, int row) previousPosition, (int column, int row) newPosition, bool capture = false, bool promotion = false)
        {
            PrevPos = previousPosition;
            NewPos = newPosition;
            Capture = capture;
            Promotion = promotion;
        }

        public SingleMove(string previousPosition, string newPosition, bool capture = false, bool promotion = false)
        {
            PrevPos = previousPosition.ToTuple();
            NewPos = newPosition.ToTuple();
            Capture = capture;
            Promotion = promotion;
        }

        /// <summary>
        /// Constructor for interface move data
        /// </summary>
        /// <param name="interfaceMove"></param>
        /// <param name="capture"></param>
        public SingleMove(IMove interfaceMove, bool capture = false)
        {
            PrevPos = interfaceMove.StartPosition.ToTuple();
            NewPos = interfaceMove.EndPosition.ToTuple();
            Capture = capture;
            Promotion = interfaceMove.PromotionResult != PromotionPieceType.NoPromotion;
        }

        public IMove ToInterfaceMove(bool castling, bool check)
        {
            var promotionType = PromotionPieceType.NoPromotion;
            if (Promotion) promotionType = PromotionPieceType.Queen;

            var move = new MoveImplementation()
            {
                StartPosition = PrevPos.ToAlgebraic(),
                EndPosition = NewPos.ToAlgebraic(),
                PromotionResult = promotionType,
                Castling = castling,
                Check = check,
                CheckMate = CheckMate
            };
            return move;
        }

        // TODO should be separate comparer for only coordinates and also captures etc
        /// <summary>
        /// Move is equal if previous and new positions match
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SingleMove? other)
        {
            if(other == null) throw new NullReferenceException();
            if (PrevPos.Equals(other.PrevPos) && NewPos.Equals(other.NewPos))
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            var info = $"{PrevPos.ToAlgebraic()} to ";
            if (Capture) info += "x";
            info += NewPos.ToAlgebraic();
            return info;
        }
    }
}
