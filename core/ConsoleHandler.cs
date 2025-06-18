using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ui.core
{

    public static class ConsoleHandler
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct InteropsString
        {
            [MarshalAs(UnmanagedType.LPStr)] public string f1;
        }
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

            public static bool StdinDataRemain() => stdin_data_remain();

            public static string ReadStdinToEnd()
            {
                string src = read_stdin_end();
                return src;
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

            public static string ToANSI(string content, string control = "[", string special = "\x1b") => special + control + content;

            public static void ANSISetup()
            {
                Console.Write($"{ToANSI("?1049h")}{ToANSI("5;5H")}{ToANSI("=19h")}{ToANSI("=7l")}{ToANSI("?25l")}{ToANSI("38;2;128;130;155m")}abc{ToANSI("0m")}{ToANSI("6n")}{ToANSI("?1004h")}{ToANSI("?9h")}{ToANSI("?1001h")}{ToANSI("?1000h")}{ToANSI("?1003h", "[", "\x1b")}{ToANSI("?25h")}{ToANSI("?40l")}{ToANSI("?3l")}{ToANSI("?1006h")}");
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

            // public static bool StdinDataRemain()
            // {
            //     System.Threading.Thread.Sleep(1);
            //     if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //     {
            //         WindowConsoleHandler.StdinDataRemain();
            //     }
            //     else
            //     {
            //         PosixConsoleHandler.StdinDataRemain();
            //     }
            //     return Console.KeyAvailable;
            // }

            // public static string ReadStdinToEnd()
            // {
            //     List<byte> buf = new List<byte>();
            //     while (StdinDataRemain())
            //     {
            //         buf.Add(Read());
            //     }
            //     return buf.AsByteBuffer().AsString();
            //     // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //     // {
            //     //     return WindowConsoleHandler.ReadStdinToEnd();
            //     // }
            //     // else
            //     // {
            //     //     return PosixConsoleHandler.ReadStdinToEnd();
            //     // }
            // }
        }

        public static class ConsoleRawStdinHandler
        {
            private static Queue<byte> buf = new Queue<byte>();
            private static Thread t = new Thread(StdinPollWorker);
            public static void StdinPollWorker()
            {
                while (true)
                {
                    try
                    {
                        buf.Enqueue(ConsoleIntermediateHandler.Read());
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            public static byte Read()
            {
                if (buf.Count == 0)
                {
                    throw new IndexOutOfRangeException("No byte in buffer");
                }
                return buf.Dequeue();
            }

            public static int GetSize() => buf.Count;

            public static bool StdinDataRemain() => GetSize() > 0;

            public static string ReadStdinToEnd()
            {
                List<byte> buf = new List<byte>();
                while (StdinDataRemain())
                {
                    buf.Add(Read());
                }
                return buf.AsByteBuffer().AsString();
            }

            public static void Init()
            {
                t.Start();
            }
        }
    }
}
