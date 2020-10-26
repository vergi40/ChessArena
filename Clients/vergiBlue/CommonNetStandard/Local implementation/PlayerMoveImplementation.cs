using System;
using System.Collections.Generic;
using System.Text;
using CommonNetStandard.Interface;

namespace CommonNetStandard.Local_implementation
{
    public class PlayerMoveImplementation : IPlayerMove
    {
        public IPlayerMove Clone()
        {
            throw new NotImplementedException();
        }

        public IMove Move { get; set; }
        public string Diagnostics { get; set; }
    }
}
