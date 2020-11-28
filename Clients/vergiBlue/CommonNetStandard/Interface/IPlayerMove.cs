namespace CommonNetStandard.Interface
{
    public interface IPlayerMove
    {
        IMove Move { get; set; }

        /// <summary>
        /// Any optional additional data about the move.
        /// E.g. search depth, eval count, strategy...
        /// </summary>
        string Diagnostics { get; set; }
    }
}