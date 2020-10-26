using System;
using System.Collections.Generic;
using System.Text;
using CommonNetStandard.Interface;

namespace CommonNetStandard.Local_implementation
{
    public class StartInformationImplementation:  IGameStartInformation
    {
        public IGameStartInformation Clone()
        {
            throw new NotImplementedException();
        }

        public bool WhitePlayer { get; set; }
        public IMove OpponentMove { get; set; }
    }
}
