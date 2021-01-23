using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Algorithms
{
    /// <summary>
    /// Substitutes basic tuple evaluationscore list with constant min/max value return.
    /// Prioritizes captures.
    /// 
    /// Requirements:
    /// Get min element in constant time
    /// Get max element in constant time
    ///
    /// No need to keep track of middle elements
    /// </summary>
    public class EvaluationResult
    {
        public double Min { get; set; } = 1000000;
        public double Max { get; set; } = -1000000;

        private bool _minIsCapture = false;
        private bool _maxIsCapture = false;

        public SingleMove MinMove { get; set; } = new SingleMove((-1,-1), (-1,-1));
        public SingleMove MaxMove { get; set; } = new SingleMove((-1, -1), (-1, -1));

        //private IList<SingleMove> _moves = new List<SingleMove>();
        public bool Empty { get; set; } = true;

        /// <summary>
        /// Lowest value first, highest last. Others random.
        /// </summary>
        public List<SingleMove> OrderedEndsAscending { get; set; } = new List<SingleMove>();
        
        /// <summary>
        /// Highest value first, lowest last. Others random.
        /// </summary>
        public List<SingleMove> OrderedEndsDescending 
        { 
            get
            {
                var list = new List<SingleMove>(OrderedEndsAscending);
                list.Reverse();
                return list;
            } 
        }
        
        public void Add(double evaluation, SingleMove move)
        {
            if (Empty)
            {
                OrderedEndsAscending.Add(move);
                MinMove = move;
                MaxMove = move;
                Min = evaluation;
                Max = evaluation;
                Empty = false;
            }
            
            if (evaluation < Min)
            {
                Min = evaluation;
                MinMove = move;
                _minIsCapture = move.Capture;
                OrderedEndsAscending.Insert(0, move);
            }
            else if(Math.Abs(evaluation - Min) < Double.Epsilon && !_minIsCapture && move.Capture)
            {
                Min = evaluation;
                MinMove = move;
                _minIsCapture = true;
                OrderedEndsAscending.Insert(0, move);
            }
            else if (evaluation > Max)
            {
                Max = evaluation;
                MaxMove = move;
                _maxIsCapture = move.Capture;
                OrderedEndsAscending.Add(move);
            }
            else if (Math.Abs(evaluation - Max) < Double.Epsilon && _maxIsCapture && move.Capture)
            {
                Max = evaluation;
                MaxMove = move;
                _maxIsCapture = true;
                OrderedEndsAscending.Add(move);
            }
        }

        public void Add(IEnumerable<(double evaluation, SingleMove move)> evaluationList)
        {
            foreach (var (evaluation, move) in evaluationList)
            {
                Add(evaluation, move);
            }
        }

        public SingleMove Best(bool isMaximizing)
        {
            if (isMaximizing) return MaxMove;
            return MinMove;
        }
    }
}
