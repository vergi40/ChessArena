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
    public class OpeningLibrary
    {
        private IList<OpeningStrategy> Openings { get; }

        /// <summary>
        /// White has started some common opening but black uses different defensive move to counter
        /// </summary>
        private IList<OpeningStrategy> DefensiveOpenings { get; }
        
        /// <summary>
        /// Some attack patterns that black should not follow
        /// </summary>
        private IList<OpeningStrategy> OnlyWhiteOpenings { get; }// TODO

        private readonly Random _random = new Random();


        public OpeningLibrary()
        {
            // https://www.thesprucecrafts.com/most-common-chess-openings-611517
            Openings = new List<OpeningStrategy>
            {
                new OpeningStrategy("Ruy Lopez", new List<SingleMove>
                {
                    new SingleMove("e2", "e4"),
                    new SingleMove("e7", "e5"),
                    new SingleMove("g1", "f3"),
                    new SingleMove("b8", "c6"),
                    new SingleMove("f1", "b5")
                }),
                new OpeningStrategy("Italian game", new List<SingleMove>
                {
                    new SingleMove("e2", "e4"),
                    new SingleMove("e7", "e5"),
                    new SingleMove("g1", "f3"),
                    new SingleMove("b8", "c6"),
                    new SingleMove("f1", "c4")
                }),
                new OpeningStrategy("Queen's gambit", new List<SingleMove>
                {
                    new SingleMove("d2", "d4"),
                    new SingleMove("d7", "d5"),
                    new SingleMove("c2", "c4")
                }),
                new OpeningStrategy("Center game", new List<SingleMove>
                {
                    new SingleMove("e2", "e4"),
                    new SingleMove("e7", "e5"),
                    new SingleMove("d2", "d4"),
                    new SingleMove("e5", "d4", true),// capture soldier
                    new SingleMove("d1", "d4", true)//capture soldier with queen
                }),
                new OpeningStrategy("Danish gambit", new List<SingleMove>
                {
                    new SingleMove("e2", "e4"),
                    new SingleMove("e7", "e5"),
                    new SingleMove("d2", "d4"),
                    new SingleMove("e5", "d4", true),
                    new SingleMove("c2", "c3")
                })
            };

            DefensiveOpenings = new List<OpeningStrategy>
            {
                new OpeningStrategy("Sicilian defense", new List<SingleMove>
                {
                    new SingleMove("e2", "e4"),
                    new SingleMove("c7", "c5")
                }),
                new OpeningStrategy("French defense", new List<SingleMove>
                {
                    new SingleMove("e2", "e4"),
                    new SingleMove("e7", "e6")
                }),
                new OpeningStrategy("Indian defense", new List<SingleMove>
                {
                    new SingleMove("d2", "d4"),
                    new SingleMove("g8", "f6")
                })
            };

            // TODO need a way to add "any"-moves
            // https://en.wikipedia.org/wiki/Scholar%27s_mate
            OnlyWhiteOpenings = new List<OpeningStrategy>
            {
                // Four-move checkmate
                // Bishop first
                new OpeningStrategy("Scholar's mate 1", new List<SingleMove>
                {
                    new SingleMove("e2", "e4"),
                    new SingleMove("e7", "e5"),
                    new SingleMove("f1", "c4"),
                    new SingleMove("b8", "c6"),
                    new SingleMove("d1", "h5"),
                    new SingleMove("g8", "f6"),
                    new SingleMove("h5", "f7")
                }),
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

            // Include defensive openings
            foreach (var opening in Openings.Concat(DefensiveOpenings))
            {
                if (opening.Moves.Count <= previousMoves.Count) continue;
                if (!ListMembersEqual(previousMoves, opening.Moves)) continue;

                // All matched
                list.Add(opening);
            }

            return list;
        }

        private bool ListMembersEqual<T>(IList<T> list1, IList<T> list2) where T : IEquatable<T>
        {
            for (int i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                {
                    return false;
                }
            }

            return true;
        }



        // C# 9.0 new data structure. Minimum boiler plate
        public record OpeningStrategy
        {
            public string Name { get; } = "";
        public IList<SingleMove> Moves { get; } = new List<SingleMove>();

        public OpeningStrategy(string name, IList<SingleMove> moves) => (Name, Moves) = (name, moves);
    }
    }


}
