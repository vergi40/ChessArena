using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.Pieces;

namespace vergiBlue.Algorithms
{
    /// <summary>
    /// Alpha-beta tree node types.
    /// https://www.chessprogramming.org/Node_Types#CUT
    /// </summary>
    public enum NodeType
    {
        Exact,

        /// <summary>
        /// All-nodes. Cut-node occured with upper bound beta. Every move from all-node needs to be searched. Node score >= score (at least equal to score). E.g. evaluation 5, lowerbound can be [5, 6, 7, 8, 9].
        /// </summary>
        UpperBound,
        
        /// <summary>
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
        public ulong Hash { get; set; }
        public int Depth { get; set; }
        public double Evaluation { get; set; }
        public NodeType Type { get; set; }

        /// <summary>
        /// If useful, set read-only to true -> do not replace with new value.
        /// Read-only if: transposition was used in to skip node children searches.
        /// Read-only if: transposition occured in cutoff.
        /// https://adamberent.com/2019/03/02/transposition-table-and-zobrist-hashing/
        /// </summary>
        public bool ReadOnly { get; set; }

        public Transposition(ulong hash, int depth, double evaluation, NodeType nodetype = NodeType.Exact, bool readOnly = false)
        {
            Hash = hash;
            Depth = depth;
            Evaluation = evaluation;
            Type = nodetype;
            ReadOnly = readOnly;
        }
    }

    public class TranspositionTables
    {
        public Dictionary<ulong, Transposition> Tables { get; set; } = new ();
        
        private bool _isInitialized { get; set; } = false;
        
        private ulong[,] hashTable = new ulong[0, 0];
        
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
            var table = new ulong[64, 12];
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    table[i, j] = GetRandom(random);
                }
            }

            hashTable = table;
            _isInitialized = true;
        }

        public ulong GetHash(Board board)
        {
            ulong hash = 0;
            for (int i = 0; i < board.PieceList.Count; i++)
            {
                var piece = board.PieceList[i];
                var index = piece.CurrentPosition.ToArray();
                var j = GetIndex(piece);
                hash = hash ^ hashTable[index, j];
            }

            return hash;
        }

        /// <summary>
        /// Call before piece move is executed.
        /// </summary>
        public ulong UpdateHash(SingleMove move, Board board, ulong hash)
        {
            // Erase capture
            if (move.Capture)
            {
                var captured = board.ValueAtDefinitely(move.NewPos);
                var boardIndex = move.NewPos.ToArray();
                var capturedPieceIndex = GetIndex(captured);
                hash = hash ^ hashTable[boardIndex, capturedPieceIndex];
            }
            // TODO castling
            
            // Update player position
            var piece = board.ValueAtDefinitely(move.PrevPos);
            var from = move.PrevPos.ToArray();
            var to = move.NewPos.ToArray();
            var pieceIndex = GetIndex(piece);
            
            // Remove old position
            hash = hash ^ hashTable[from, pieceIndex];
            
            // Add new position
            hash = hash ^ hashTable[to, pieceIndex];
            return hash;
        }
        
        
        private int GetIndex(PieceBase piece)
        {
            var color = 0;
            if (!piece.IsWhite) color = 6;

            if (piece.Identity == 'P') return 0 + color;
            if (piece.Identity == 'B') return 1 + color;
            if (piece.Identity == 'N') return 2 + color;
            if (piece.Identity == 'R') return 3 + color;
            if (piece.Identity == 'Q') return 4 + color;
            if (piece.Identity == 'K') return 5 + color;

            throw new ArgumentException();
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
        public void Add(ulong hash, int depth, double evaluation, NodeType nodeType)
        {
            if (Tables.ContainsKey(hash))
            {
                Update(hash, depth, evaluation, nodeType);
            }
            else
            {
                lock (_tableLock)
                {
                    // New hash
                    Tables.Add(hash, new Transposition(hash, depth, evaluation, nodeType));
                }
            }
        }

        /// <summary>
        /// Update transposition if it's current content is not significant (table.readOnly == true).
        /// </summary>
        public void Update(ulong hash, int depth, double evaluation, NodeType nodeType, bool readOnly = false)
        {
            lock (_tableLock)
            {
                if (!Tables[hash].ReadOnly)
                {
                    // We found deeper search, substitute
                    Tables[hash].Depth = depth;
                    Tables[hash].Evaluation = evaluation;
                    Tables[hash].Type = nodeType;
                    Tables[hash].ReadOnly = readOnly;
                }
            }
        }
    }
}
