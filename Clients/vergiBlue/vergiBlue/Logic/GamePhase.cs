namespace vergiBlue.Logic
{
    public enum GamePhase
    {
        /// <summary>
        /// Openings and initial. Very slow evaluation calculation when all the pieces are out open
        /// </summary>
        Start,
        Middle,

        /// <summary>
        /// King might be in danger
        /// </summary>
        MidEndGame,

        /// <summary>
        /// King might be in danger
        /// </summary>
        EndGame
    }
}