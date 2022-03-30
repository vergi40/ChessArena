namespace CommonNetStandard.Interface
{
    public interface IPieceMinimal
    {
        bool IsWhite { get; }
        /// <summary>
        /// Upper case K, Q, R, N, B, P
        /// </summary>
        char Identity { get; }
        (int column, int row) CurrentPosition { get; }
    }
}
