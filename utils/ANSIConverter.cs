using ui.core;

namespace ui.utils {
    public static class AnsiConverter {
        public static ConsoleLocation ToConsoleLocation(int row, int col)
        {
            return new ConsoleLocation(col - 1, row - 1);
        }
    }
}