using Microsoft.Diagnostics.Runtime;

namespace SafeCrashHandler;

public class SafeCrashHandlerEventArgs : EventArgs
{
    public DataTarget Target;

    internal SafeCrashHandlerEventArgs(DataTarget target)
    {
        Target = target;
    }
}