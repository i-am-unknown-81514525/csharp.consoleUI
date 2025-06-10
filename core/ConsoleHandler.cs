using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ui.core
{
    public static class ConsoleHandler
    {
        private static class WindowConsoleHandler
        {
            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern int init();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern byte read_stdin();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention =CallingConvention.Cdecl)]
            private static extern int reset();

            public static byte readStdin() => read_stdin();

            public static void Setup()
            {
                init();
            }

            public static void Reset()
            {
                reset();
            }
        }

        private static class PosixConsoleHandler
        {
            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern int init();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern byte read_stdin();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern int reset();

            public static byte readStdin() => read_stdin();

            private static void addPath(string name)
            {
                string content = Environment.GetEnvironmentVariable(name);
                if (content == null) content = "";
                string path = Environment.CurrentDirectory;
                content += $":{path}";
                Environment.SetEnvironmentVariable(name, content);
            }

            public static void Setup()
            {
                addPath("LD_LIBRARY_PATH"); // linux
                addPath("DYLD_LIBRARY_PATH"); //macbook
                addPath("DYLD_FRAMEWORK_PATH");
                addPath("DYLD_FALLBACK_FRAMEWORK_PATH");
                addPath("DYLD_FALLBACK_LIBRARY_PATH");
                init();
            }

            public static void Reset()
            {
                reset();
            }
        }

        public static class ConsoleIntermediateHandler
        {
            public static void Setup()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    WindowConsoleHandler.Setup();
                }
                else
                {
                    PosixConsoleHandler.Setup();
                }
            }

            public static byte Read()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return WindowConsoleHandler.readStdin();
                }
                else
                {
                    return PosixConsoleHandler.readStdin();
                }
            }

            public static void Reset()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    WindowConsoleHandler.Reset();
                }
                else
                {
                    PosixConsoleHandler.Reset();
                }
            }
        }
    }
}
