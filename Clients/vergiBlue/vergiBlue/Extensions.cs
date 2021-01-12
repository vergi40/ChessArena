using System;
using System.Collections.Generic;
using System.Data;
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

        public static int ToArray(this (int column, int row) position)
        {
            return (position.row * 8) + position.column;
        }

        /// <summary>
        /// Transforms array index to (column, row) format
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static (int column, int row) ToTuple(this int index)
        {
            // 1,0 -> 8
            // 1,1 -> 9

            // 8 -> row = 1
            // 8 -> column = 0
            // 9 -> row = 1
            // 9 -> column = 1

            var row = index / 8;
            var column = index % 8;
            return (column, row);
        }

    }
}
