using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;

namespace vergiBlue.Analytics
{
    internal class DataFactory
    {
        public IMinimalTurnData CreateMinimal(IBoard board, bool isWhiteTurn)
        {
            return new DescriptiveTurnData()
            {
                IsWhiteTurn = isWhiteTurn
            };
        }

        public IDescriptiveTurnData CreateDescriptive(IBoard board, bool isWhiteTurn, DiagnosticsData diagnosticsData)
        {
            return new DescriptiveTurnData()
            {
                IsWhiteTurn = isWhiteTurn
            };
        }
    }
}
