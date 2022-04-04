using System;
using System.Collections.Generic;
using CommonNetStandard.Interface;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.Subsystems
{
    /// <summary>
    /// Alpha-beta tree node types.
    /// https://www.chessprogramming.org/Node_Types#CUT
    /// </summary>
    public enum NodeType
    {
        Exact,

        /// <summary>
        /// Eval is at most alpha.
        /// All-nodes. Cut-node occured with upper bound beta. Every move from all-node needs to be searched. Node score >= score (at least equal to score). E.g. evaluation 5, lowerbound can be [5, 6, 7, 8, 9].
        /// </summary>
        UpperBound,
        
        /// <summary>
        /// Eval is at least beta.
        /// Cut-nodes. Beta cutoff occured. A minimum of 1 node at a cut-node needs to be searched. Node score at most equal to eval score. E.g. evaluation 5, lowerbound can be [1, 2, 3, 4, 5]
        /// </summary>
        LowerBound
    }
    
    /// <summary>
    /// Store information of one board during certain depth
    /// https://www.chessprogramming.org/Transposition_Table
    /// </summary>
    public class Transposition
    {
        /// <summary>
        /// One-direction hash value for each possibly board situation. If two hashes are same, they have
        /// * Identical piece setup
        /// * Same player turn
        /// * Same castling rights
        /// * Same en passant situation
        /// </summary>
        public ulong Hash { get; set; }
        public int Depth { get; set; }
        public double Evaluation { get; set; }
        
        /// <summary>
        /// Is transposition evaluation from exact result, of some approximation.
        /// </summary>
        public NodeType Type { get; set; }
        
        /// <summary>
        /// Turn count in main board when transposition was saved.
        /// Used to delete old entries.
        /// </summary>
        public int GameTurnCount { get; set; }

        public Transposition(ulong hash, int depth, double evaluation, NodeType nodetype, int gameTurnCount)
        {
            Hash = hash;
            Depth = depth;
            Evaluation = evaluation;
            Type = nodetype;
            GameTurnCount = gameTurnCount;
        }

        public override string ToString()
        {
            return $"Eval: {Evaluation} - {Type.ToString()}";
        }
    }

    public class TranspositionTables
    {
        public Dictionary<ulong, Transposition> Tables { get; set; } = new ();
        
        private bool _isInitialized { get; set; }

        /// <summary>
        /// Stores 12 different random numbers for each board square.
        /// Indexes 0-5 are used for each white piece
        /// Indexes 6-11 are used for each black piece
        /// </summary>
        private ulong[,] _hashTable { get; } = new ulong[64, 12];
        
        /// <summary>
        /// Clear old transpositions and initialize hash table
        /// </summary>
        public void Initialize()
        {
            Tables = new Dictionary<ulong, Transposition>();
            if (!_isInitialized)
            {
                InitializeZobrist();
            }
        }
        private void InitializeZobrist()
        {
            // https://en.wikipedia.org/wiki/Zobrist_hashing
            // https://www.chessprogramming.org/Zobrist_Hashing
            var random = new Random(666);
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    _hashTable[i, j] = GetRandom(random);
                }
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Changes which side turn it is (applied in <see cref="GetNewBoardHash"/>)
        /// Use externally only in tests, if moving same color multiple times in sequence
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public ulong ChangeSideToMove(ulong hash)
        {
            // Pawns are never aligned in 1. or 8. row
            var changeSideHash = _hashTable[(7, 7).To1DimensionArray(),0];
            return hash ^ changeSideHash;
        }

        /// <summary>
        /// Create startup hash based on all current pieces
        /// </summary>
        public ulong CreateBoardHash(IBoard board)
        {
            // TODO castling rights
            ulong hash = 0;
            for (int i = 0; i < board.PieceList.Count; i++)
            {
                var piece = board.PieceList[i];
                hash = hash ^ GetPieceHash(piece, piece.CurrentPosition);
            }

            return hash;
        }

        /// <summary>
        /// Get board hash for given move (with pre-move board reference and it's hash)
        /// </summary>
        public ulong GetNewBoardHash(in ISingleMove move, IBoard oldBoard, ulong hash)
        {
            // Change side to move
            hash = ChangeSideToMove(hash);

            if (move.Castling)
            {
                var king = oldBoard.ValueAtDefinitely(move.PrevPos);
                var row = 0;
                if (!king.IsWhite) row = 7;

                if (move.NewPos == (2, row))
                {
                    // TODO update castling rights
                    var rook = oldBoard.ValueAtDefinitely((0, row));
                    hash = ExecuteMoveHash(hash, rook, (0, row), (3, row));
                    hash = ExecuteMoveHash(hash, king, (4, row), (2, row));
                }
                else if (move.NewPos == (6, row))
                {
                    // TODO update castling rights
                    var rook = oldBoard.ValueAtDefinitely((7, row));
                    hash = ExecuteMoveHash(hash, rook, (7, row), (5, row));
                    hash = ExecuteMoveHash(hash, king, (4, row), (6, row));
                }
                else
                {
                    throw new ArgumentException(
                        $"Castling logical error: invalid king new position {move.NewPos.ToAlgebraic()}");
                }

                return hash;
            }

            if (move.Capture)
            {
                IPiece captured;
                (int column, int row) targetPosition;
                if (move.EnPassant)
                {
                    captured = oldBoard.ValueAtDefinitely(move.EnPassantOpponentPosition);
                    targetPosition = move.EnPassantOpponentPosition;
                }
                else
                {
                    captured = oldBoard.ValueAtDefinitely(move.NewPos);
                    targetPosition = move.NewPos;
                }

                // Erase capture
                hash = hash ^ GetPieceHash(captured, targetPosition);
            }
            
            // Updating player position
            var piece = oldBoard.ValueAtDefinitely(move.PrevPos);
            
            // Remove old position
            hash = hash ^ GetPieceHash(piece, move.PrevPos);

            // Add new position
            if (move.PromotionType != PromotionPieceType.NoPromotion)
            {
                var identity = move.PromotionType switch
                {
                    PromotionPieceType.Queen => 'Q',
                    PromotionPieceType.Rook => 'R',
                    PromotionPieceType.Knight => 'N',
                    PromotionPieceType.Bishop => 'B',
                    _ => throw new ArgumentException($"Unknown promotion: {move.PromotionType}")
                };
                hash = hash ^ GetPieceHash(piece.IsWhite, identity, move.NewPos);
            }
            else
            {
                hash = hash ^ GetPieceHash(piece, move.NewPos);
            }
            return hash;
        }

        /// <summary>
        /// Hash that results from simple piece move
        /// </summary>
        private ulong ExecuteMoveHash(ulong hash, IPiece piece, (int column, int row) prev, (int column, int row) next)
        {
            // Remove old
            hash = hash ^ GetPieceHash(piece, prev);

            // Add new
            hash = hash ^ GetPieceHash(piece, next);
            return hash;
        }

        private ulong GetPieceHash(IPiece piece, (int column, int row) position)
        {
            var pieceIndex = GetPieceCustomIndex(piece.IsWhite, piece.Identity);
            return _hashTable[position.To1DimensionArray(), pieceIndex];
        }

        private ulong GetPieceHash(bool isWhite, char identity, (int column, int row) position)
        {
            var pieceIndex = GetPieceCustomIndex(isWhite, identity);
            return _hashTable[position.To1DimensionArray(), pieceIndex];
        }

        private int GetPieceCustomIndex(bool isWhite, char identity)
        {
            var color = 0;
            if (!isWhite) color = 6;

            return identity switch
            {
                'P' => 0 + color,
                'B' => 1 + color,
                'N' => 2 + color,
                'R' => 3 + color,
                'Q' => 4 + color,
                'K' => 5 + color,
                _ => throw new ArgumentException($"Unknown identity {identity}")
            };
        }

        private ulong GetRandom(Random random)
        {
            // https://www.dotnetperls.com/xor
            // https://stackoverflow.com/questions/6651554/random-number-in-long-range-is-this-the-way

            // bytestring of 8 bytes = 64 bits
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            
            // Convert to ulong
            return BitConverter.ToUInt64(buf, 0);
        }
        
        
        private readonly object _tableLock = new object();

        /// <summary>
        /// Add new boardhash transposition to tables data.
        /// If already contained, only update. 
        /// </summary>
        public void Add(ulong boardHash, int depth, double evaluation, NodeType nodeType, int gameTurnCount)
        {
            if (boardHash == 0) throw new ArgumentException("Board hash was empty.");
            if(Tables.TryGetValue(boardHash, out var transposition))
            {
                // Replacement scheme: always replace
                if (depth >= transposition.Depth)
                {
                    evaluation = Evaluator.CheckMateScoreAdjustToEven(evaluation);
                    transposition.Depth = depth;
                    transposition.Evaluation = evaluation;
                    transposition.Type = nodeType;
                    transposition.GameTurnCount = gameTurnCount;
                }
            }
            else
            {
                lock (_tableLock)
                {
                    // New hash
                    Tables.Add(boardHash, new Transposition(boardHash, depth, evaluation, nodeType, gameTurnCount));
                }
            }
        }

        /// <summary>
        /// Update transposition
        /// </summary>
        public void Update(ulong hash, int depth, double evaluation, NodeType nodeType, int gameTurnCount)
        {
            if (hash == 0) throw new ArgumentException("Board hash was empty.");
            
            // Replacement scheme: always replace
            var transposition = Tables[hash];
            if (depth >= transposition.Depth)
            {
                evaluation = Evaluator.CheckMateScoreAdjustToEven(evaluation);
                transposition.Depth = depth;
                transposition.Evaluation = evaluation;
                transposition.Type = nodeType;
                transposition.GameTurnCount = gameTurnCount;
            }
        }

        /// <summary>
        /// Check if the given move is already calculated (with pre-move board reference).
        /// Fast (could be even faster with tables indexing).
        /// </summary>
        public bool ContainsMove(IBoard oldBoard, SingleMove newMove)
        {
            var oldHash = oldBoard.BoardHash;
            var newHash = GetNewBoardHash(newMove, oldBoard, oldHash);

            if (Tables.ContainsKey(newHash)) return true;
            return false;
        }

        /// <summary>
        /// Get value of the already calculated move (with pre-move board reference)
        /// </summary>
        /// <returns>Null if no transposition exists</returns>
        public Transposition? GetTranspositionForMove(IBoard oldBoard, SingleMove newMove)
        {
            var oldHash = oldBoard.BoardHash;
            var newHash = GetNewBoardHash(newMove, oldBoard, oldHash);

            if (Tables.TryGetValue(newHash, out var transposition))
            {
                return transposition;
            }
            return null;
        }

        public Transposition? GetTranspositionForBoard(ulong boardHash)
        {
            if (boardHash == 0) throw new ArgumentException("Board hash was empty.");
            if (Tables.TryGetValue(boardHash, out var transposition))
            {
                return transposition;
            }
            
            return null;
        }

        public void AddOrUpdate(Transposition transposition)
        {
            if (transposition.Hash == 0) throw new ArgumentException("Board hash was empty.");
            if (Tables.TryGetValue(transposition.Hash, out var oldTransposition))
            {
                // Replacement scheme: always replace
                if (transposition.Depth >= oldTransposition.Depth)
                {
                    oldTransposition.Depth = transposition.Depth;
                    oldTransposition.Evaluation = Evaluator.CheckMateScoreAdjustToEven(transposition.Evaluation);
                    oldTransposition.Type = transposition.Type;
                    oldTransposition.GameTurnCount = transposition.GameTurnCount;
                }
            }
            else
            {
                lock (_tableLock)
                {
                    // New hash
                    transposition.Evaluation = Evaluator.CheckMateScoreAdjustToEven(transposition.Evaluation);
                    Tables.Add(transposition.Hash, transposition);
                }
            }
        }
    }
}
