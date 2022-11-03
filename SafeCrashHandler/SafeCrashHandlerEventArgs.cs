using Microsoft.Diagnostics.Runtime;

namespace SafeCrashHandler;

public class SafeCrashHandlerEventArgs : EventArgs
{
    private DataTarget Target;
        
    internal SafeCrashHandlerEventArgs(DataTarget target)
    {
        Target = target;
    }
}