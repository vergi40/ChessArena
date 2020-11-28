namespace CommonNetStandard.Interface
{
    public interface IGameStartInformation
    {
        /// <summary>
        /// True if client is starting player.
        /// If false, opponent move is also returned.
        /// </summary>
        bool WhitePlayer { get; set; }

        IMove? OpponentMove { get; set; }
    }
}