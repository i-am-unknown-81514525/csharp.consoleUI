using ui.core;

namespace ui
{
    public static class Global
    {
        // ReSharper disable once UnusedMember.Global
        public static readonly RootInputHandler InputHandler = new RootInputHandler();
        public static readonly ConsoleCanva consoleCanva = new ConsoleCanva();
        public static readonly ActiveStatusHandler ActiveStatus = new ActiveStatusHandler();
        
    }
}