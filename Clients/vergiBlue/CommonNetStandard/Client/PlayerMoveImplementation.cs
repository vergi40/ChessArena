using CommonNetStandard.Interface;

namespace CommonNetStandard.Client
{
    public class PlayerMoveImplementation : IPlayerMove
    {
        public IMove Move { get; set; }
        public string Diagnostics { get; set; }

        public PlayerMoveImplementation(IMove move, string diagnostics)
        {
            Move = move;
            Diagnostics = diagnostics;
        }
    }
}
