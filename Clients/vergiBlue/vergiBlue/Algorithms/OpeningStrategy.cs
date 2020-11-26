// ReSharper disable All
// CTRL + SHIFT + ALT + 8

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Algorithms
{
    public record OpeningStrategy
    {
        public string Name { get; } = "";
        public IList<SingleMove> Moves {get;} = new List<SingleMove>();

        public OpeningStrategy(string name, IList<SingleMove> moves) => (Name, Moves) = (name, moves);
    }

    public class OpeningLibrary
    {
        private IList<OpeningStrategy> Openings { get; }
        private readonly Random _random = new Random();


        public OpeningLibrary()
        {
            // https://www.thesprucecrafts.com/most-common-chess-openings-611517
            Openings = new List<OpeningStrategy>
            {
                new OpeningStrategy("RuyLopez", new List<SingleMove>
                {
                    new SingleMove("e2", "e4"),
                    new SingleMove("e7", "e5"),
                    new SingleMove("g1", "f3"),
                    new SingleMove("b8", "c6"),
                    new SingleMove("f1", "b5")
                })
            };
        }

        /// <summary>
        /// Null if moves are not part any known opening
        /// </summary>
        /// <param name="previousMoves"></param>
        /// <returns></returns>
        public SingleMove? NextMove(IList<SingleMove> previousMoves)
        {
            if (!previousMoves.Any())
            {
                // Pick random opening strategy, white move
                var index = _random.Next(Openings.Count);
                var strategy = Openings[index];

                Diagnostics.AddMessage($"Chosen opening strategy: {strategy.Name}.");
                return strategy.Moves.First();
            }
            else
            {
                // Check if there is one or many possible opening strategies and pick random
                var similarOpenings = OpeningsWithSameMoves(previousMoves);
                if (similarOpenings.Count == 0) return null;

                // Pick random continuation
                var index = _random.Next(similarOpenings.Count);
                var strategy = similarOpenings[index];

                Diagnostics.AddMessage($"Using opening strategy {strategy.Name}.");
                return strategy.Moves[previousMoves.Count];
            }
        }

        private IList<OpeningStrategy> OpeningsWithSameMoves(IList<SingleMove> previousMoves)
        {
            var list = new List<OpeningStrategy>();

            foreach (var opening in Openings)
            {
                if (opening.Moves.Count <= previousMoves.Count) continue;

                for (int i = 0; i < previousMoves.Count; i++)
                {
                    if (!previousMoves[i].Equals(opening.Moves[i]))
                    {
                        continue;
                    }
                }

                // All matched
                list.Add(opening);
            }

            return list;
        }
    }


}
