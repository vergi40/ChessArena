using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue
{
    public static class Extensions
    {
        private const int _intToAlphabet = 65;

        /// <summary>
        /// Transforms (column, row) format to e.g. 'a1'
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string ToAlgebraic(this (int column, int row) position)
        {
            var move = $"{((char)(position.column + _intToAlphabet)).ToString().ToLower()}{position.row + 1}";
            return move;
        }

        /// <summary>
        /// Transforms algebraic format e.g. 'a1' to (column, row) format
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static (int column, int row) ToTuple(this string position)
        {
            char columnChar = char.ToUpper(position[0]);
            var column = columnChar - _intToAlphabet;
            var row = int.Parse(position[1].ToString());
            return (column, row - 1);
        }

    }
}
