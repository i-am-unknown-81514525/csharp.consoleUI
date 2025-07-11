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
            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern int init();

            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern byte read_stdin();

            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern int reset();

            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool stdin_data_remain();

            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern string read_stdin_end();

            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern void consume(uint amount);

            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern string read_clipboard();

            public static byte readStdin() => read_stdin();

            public static void Setup()
            {
                init();
            }

            public static void Reset()
            {
                reset();
            }

            public static bool StdinDataRemain() => stdin_data_remain();

            public static string ReadStdinToEnd()
            {
                string src = read_stdin_end();
                return src;
            }

            internal static void Consume(uint size)
            {
                consume(size);
            }

            public static string ReadClipboard() => read_clipboard();
        }

        private static class PosixConsoleHandler
        {
            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern int init();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern byte read_stdin();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern int reset();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool stdin_data_remain();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern string read_stdin_end();

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern void consume(uint amount);

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern string read_clipboard();

            public static byte readStdin() => read_stdin();

            private static void addPath(string name)
            {
                string content = Environment.GetEnvironmentVariable(name);
                string path = Environment.CurrentDirectory;
                if (content == null)
                    content = path;
                else
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

            public static bool StdinDataRemain() => stdin_data_remain();

            public static string ReadStdinToEnd()
            {
                string src = read_stdin_end();
                return src;
            }

            internal static void Consume(uint size)
            {
                consume(size);
            }

            public static string ReadClipboard() => read_clipboard();
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

            public static string ToANSI(string content, string control = "[", string special = "\x1b") => special + control + content;

            public static void ANSISetup()
            {
                Console.Write($"{ToANSI("?1049h")}{ToANSI("=19h")}{ToANSI("=7l")}{ToANSI("?25l")}{ToANSI("38;2;128;130;155m")}{ToANSI("0m")}{ToANSI("6n")}{ToANSI("?1004h")}{ToANSI("?9h")}{ToANSI("?1001h")}{ToANSI("?1000h")}{ToANSI("?1003h", "[", "\x1b")}{ToANSI("?25h")}{ToANSI("?40l")}{ToANSI("?3l")}{ToANSI("?1006h")}");
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

            public static bool StdinDataRemain()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return WindowConsoleHandler.StdinDataRemain();
                }
                else
                {
                    return PosixConsoleHandler.StdinDataRemain();
                }
                // Task<bool> task = Task<bool>.Run(
                //     () =>
                //     {
                //         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                //         {
                //             return WindowConsoleHandler.StdinDataRemain();
                //         }
                //         else
                //         {
                //             return PosixConsoleHandler.StdinDataRemain();
                //         }
                //     }
                // );
                // bool result = task.Wait(5);
                // return result && task.Result;
            }

            public static string ReadStdinToEnd()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return WindowConsoleHandler.ReadStdinToEnd();
                }
                else
                {
                    return PosixConsoleHandler.ReadStdinToEnd();
                }
                // Task<string> task = Task<string>.Run(
                //     () =>
                //     {
                //         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                //         {
                //             return WindowConsoleHandler.ReadStdinToEnd();
                //         }
                //         else
                //         {
                //             return PosixConsoleHandler.ReadStdinToEnd();
                //         }
                //     }
                // );
                // bool result = task.Wait(10);
                // if (result)
                // {
                //     if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                //         {
                //             WindowConsoleHandler.Consume((uint)task.Result.Length);
                //         }
                //         else
                //         {
                //             PosixConsoleHandler.Consume((uint)task.Result.Length);
                //         }
                // }
                // return result ? task.Result : "";
            }
            public static string ReadClipboard()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return WindowConsoleHandler.ReadClipboard();
                }
                else
                {
                    return PosixConsoleHandler.ReadClipboard();
                }
            }
        }
    }
}
