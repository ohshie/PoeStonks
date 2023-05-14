namespace PoeStonks.Helper;

public class Logger
{
    private static string? _logMessageOutput;
    public static event Action<string>? LogMessageOutputChanged;

    public static string LogMessageOutput
    {
        get { return _logMessageOutput!; }
        set
        {
            if (_logMessageOutput != value)
            {
                _logMessageOutput = value;
                LogMessageOutputChanged?.Invoke(_logMessageOutput);
            }
        }
    }
}