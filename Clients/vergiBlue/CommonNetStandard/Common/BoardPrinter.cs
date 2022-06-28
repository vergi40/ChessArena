using System;
using System.Collections.Generic;
using CommonNetStandard;
using CommonNetStandard.Interface;
using CommonNetStandard.LocalImplementation;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;

namespace CommonNetStandard.Common
{
    public class BoardPrinter
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<BoardPrinter>();
        private ConsoleColors Colors { get; }
        public const string PreviousTileValue = "[ ]";

        public string[,] Tiles { get; set; }

        public BoardPrinter(IEnumerable<IPieceMinimal> pieces, bool isWindows = false)
        {
            Colors = new ConsoleColors(isWindows);
            Tiles = new string[8, 8];
            foreach (var piece in pieces)
            {
                var color = 'w';
                if (!piece.IsWhite) color = 'b';
                Set(piece.CurrentPosition, color.ToString() + piece.Identity.ToString() + " ");
            }
        }

        public string Get((int, int) target)
        {
            return Tiles[target.Item1, target.Item2];
        }

        public void Set((int, int) target, string identity)
        {
            Tiles[target.Item1, target.Item2] = identity;
        }

        public void Print()
        {
            for (int row = 7; row >= 0; row--)
            {
                var columnString = $"{row + 1}| ";
                for (int column = 0; column < 8; column++)
                {
                    columnString += DrawPiece(Get((column, row)));
                    columnString += Colors.BlackBackground + Colors.WhiteForeground;
                }
                Console.WriteLine(columnString);
            }
            Console.WriteLine("    A  B  C  D  E  F  G  H ");
        }

        private string DrawPiece(string value)
        {
            if (string.IsNullOrEmpty(value)) return "   ";
            if (value == PreviousTileValue)
            {
                return value;
            }

            // Console coloring magic
            // https://stackoverflow.com/questions/7937256/custom-text-color-in-c-sharp-console-application
            if (value.Contains("w"))
            {
                value = Colors.WhiteBackground + Colors.BlackForeground + value;
            }
            else
            {
                value = Colors.BlackBackground + Colors.WhiteForeground + value;
            }
            return value;
        }
    }
}
