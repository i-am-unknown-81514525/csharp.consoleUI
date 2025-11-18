using System;
using System.Runtime.InteropServices;

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

            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern void write_clipboard(string ptr);

            [DllImport("libstdin_handler.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern void open_website(string ptr);

            public static byte ReadStdin() => read_stdin();

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

            public static void Consume(uint size)
            {
                consume(size);
            }

            public static string ReadClipboard() => read_clipboard();

            public static void WriteClipboard(string content) => write_clipboard(content);

            public static void OpenWebsite(string content) => open_website(content);
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

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern void write_clipboard(string ptr);

            [DllImport("libstdin_handler", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            private static extern void open_website(string ptr);
            public static byte ReadStdin() => read_stdin();

            private static void AddPath(string name)
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
                AddPath("LD_LIBRARY_PATH"); // linux
                AddPath("DYLD_LIBRARY_PATH"); //macbook
                AddPath("DYLD_FRAMEWORK_PATH");
                AddPath("DYLD_FALLBACK_FRAMEWORK_PATH");
                AddPath("DYLD_FALLBACK_LIBRARY_PATH");
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

            public static void Consume(uint size)
            {
                consume(size);
            }

            public static string ReadClipboard() => read_clipboard();

            public static void WriteClipboard(string content) => write_clipboard(content);

            public static void OpenWebsite(string content) => open_website(content);
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

            public static string ToAnsi(string content, string control = "[", string special = "\x1b") => special + control + content;

            public static void AnsiSetup()
            {
                Console.Write($"{ToAnsi("?1049h")}{ToAnsi("=19h")}{ToAnsi("=7l")}{ToAnsi("?25l")}{ToAnsi("38;2;128;130;155m")}{ToAnsi("0m")}{ToAnsi("6n")}{ToAnsi("?1004h")}{ToAnsi("?9h")}{ToAnsi("?1001h")}{ToAnsi("?1000h")}{ToAnsi("?1003h")}{ToAnsi("?25h")}{ToAnsi("?40l")}{ToAnsi("?3l")}{ToAnsi("?1006h")}");
            }

            public static byte Read()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return WindowConsoleHandler.ReadStdin();
                }

                return PosixConsoleHandler.ReadStdin();
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

                return PosixConsoleHandler.StdinDataRemain();
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

                return PosixConsoleHandler.ReadStdinToEnd();
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

                return PosixConsoleHandler.ReadClipboard();
            }

            public static void WriteClipboard(string content)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    WindowConsoleHandler.WriteClipboard(content);
                }
                else
                {
                    PosixConsoleHandler.WriteClipboard(content);
                }
            }

            public static void OpenWebsite(string content)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    WindowConsoleHandler.OpenWebsite(content);
                }
                else
                {
                    PosixConsoleHandler.OpenWebsite(content);
                }
            }
        }
    }
}
