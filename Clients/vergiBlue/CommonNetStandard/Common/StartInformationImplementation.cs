using CommonNetStandard.Interface;

namespace CommonNetStandard.Common
{
    public class StartInformationImplementation : IGameStartInformation
    {
        public bool WhitePlayer { get; set; }
        public IMove? OpponentMove { get; set; }
    }
}
