namespace SafeCrashHandler.Example
{
    static class Program
    {
        static void Main(string[] args)
        {
            SafeCrashHandler.OnUnhandledException += SafeCrashHandlerOnOnUnhandledException;
            SafeCrashHandler.OnCrash += SafeCrashHandlerOnOnCrash;
            SafeCrashHandler.Start();
            Console.WriteLine("work lol");
            Thread.Sleep(200);
            Console.WriteLine("man this work was so hard");
            Console.ReadLine();
            throw new ApplicationException("oops");
        }
        
        private static void SafeCrashHandlerOnOnCrash(object? sender, SafeCrashHandlerEventArgs e)
        {
            Console.WriteLine("The app is crashing");
        }

        private static void SafeCrashHandlerOnOnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Uh oh... an unhandled exception...");
        }
    }
}