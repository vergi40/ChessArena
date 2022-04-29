using log4net;

namespace vergiBlueConsole.UciMode;

class UciInput
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(Uci));
    private bool UseStreamAsInput { get; set; }
    private StreamReader InputStream { get; }

    public UciInput(StreamReader? inputStream)
    {
        if (inputStream != null)
        {
            InputStream = inputStream;
            UseStreamAsInput = true;
        }
        else
        {
            InputStream = StreamReader.Null;
        }
    }

    public string ReadLine()
    {
        if (UseStreamAsInput)
        {
            var lineFromStream = InputStream.ReadLine();
            if (lineFromStream != null)
            {
                _logger.Info($"Input  >> {lineFromStream}");
                return lineFromStream;
            }
            // Else stream ended, change to console read
            UseStreamAsInput = false;
        }

        var line = Console.ReadLine();
        if (line == null)
        {
            throw new ArgumentException($"Received end of stream from Console.ReadLine. Exiting in error state.");
        }

        _logger.Info($"Input  >> {line}");
        return line;
    }
}