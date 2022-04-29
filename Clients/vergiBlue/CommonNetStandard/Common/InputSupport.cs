using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonNetStandard.Common
{
    public static class InputSupport
    {
        /// <summary>
        /// Read UCI position definition, such as
        /// <code>position startpos moves e2e4</code>
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// (startpos or fenstring, list of moves as algebraic notation)
        /// </returns>
        public static (string startPosOrFenBoard, List<string> moves) ReadUciPosition(string input)
        {
            // Either
            //   position startpos moves e2e4
            // Or
            //   position fen r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1 moves ___
            string startPosOrFenBoard;
            var moves = new List<string>();
            var line = input.Replace("position ", "");
            if (line.Contains("fen"))
            {
                line = line.Replace("fen ", "");
                if (line.Contains("moves"))
                {
                    if(line.Contains("moves "))
                    {
                        var splitted = line.Split(" moves ");
                        startPosOrFenBoard = splitted[0];
                        moves = splitted[1].Split(" ").ToList();
                    }
                    else
                    {
                        // no moves listed
                        var splitted = line.Split(" moves");
                        startPosOrFenBoard = splitted[0];
                    }
                }
                else
                {
                    startPosOrFenBoard = line;
                }
            }
            else
            {
                startPosOrFenBoard = "startpos";
                if (line.Contains("moves "))
                {
                    var splitted = line.Split(" moves ");
                    moves = splitted[1].Split(" ").ToList();
                }
            }

            return (startPosOrFenBoard, moves);
        }

        public static UciGoParameters ReadGoParameters(string input)
        {
            // go wtime 122000 btime 120000 winc 2000 binc 2000
            // go infinite
            // go infinite searchmoves e2e4 d2d4
            // go movetime 4000 depth 5 nodes 500000 mate 4
            var s = input.Split(' ');
            if (s[0] != "go")
            {
                throw new ArgumentException("Missing go command");
            }
            if (s.Length < 2)
            {
                throw new ArgumentException("Missing go parameters");
            }

            var parameters = new UciGoParameters();
            if (input.Contains("infinite"))
            {
                parameters.Infinite = true;

                if (s.Length > 3 && s[2] == "searchmoves")
                {
                    for (int i = 3; i < s.Length; i++)
                    {
                        parameters.SearchMoves.Add(s[i]);
                    }
                }
                return parameters;
            }

            for (int i = 1; i < s.Length - 1; i += 2)
            {
                var key = s[i];
                var value = s[i + 1];
                if (key == "movetime")
                {
                    parameters.SearchLimits.Time = int.Parse(value);
                }
                else if (key == "depth")
                {
                    parameters.SearchLimits.Depth = int.Parse(value);
                }
                else if (key == "nodes")
                {
                    parameters.SearchLimits.Nodes = int.Parse(value);
                }
                else if (key == "mate")
                {
                    parameters.SearchLimits.Mate = int.Parse(value);
                }
                else if (key == "wtime")
                {
                    parameters.WhiteTimeLeft = int.Parse(value);
                }
                else if (key == "btime")
                {
                    parameters.BlackTimeLeft = int.Parse(value);
                }
                else if (key == "winc")
                {
                    parameters.WhiteIncrementPerMove = int.Parse(value);
                }
                else if (key == "binc")
                {
                    parameters.BlackIncrementPerMove = int.Parse(value);
                }

            }

            return parameters;
        }
    }

    /// <summary>
    /// All time values in milliseconds
    /// </summary>
    public class UciGoParameters
    {
        public List<string> SearchMoves { get; set; } = new List<string>();
        public bool Ponder { get; set; }
        public int WhiteTimeLeft { get; set; }
        public int BlackTimeLeft { get; set; }
        public int WhiteIncrementPerMove { get; set; }
        public int BlackIncrementPerMove { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public int MovesToGo { get; set; }

        public UciSearchLimits SearchLimits { get; set; } = new UciSearchLimits();

        /// <summary>
        /// Search until "stop" command given
        /// </summary>
        public bool Infinite { get; set; }

    }

    public class UciSearchLimits
    {
        public int Depth { get; set; }
        public int Nodes { get; set; }

        /// <summary>
        /// Search for a mate in x moves
        /// </summary>
        public int Mate { get; set; }

        /// <summary>
        /// Search exactly x milliseconds
        /// </summary>
        public int Time { get; set; }

    }
}
