using System;
using System.Text;

namespace ui
{
    public static class Debug
    {
        public const bool InputHandlerIgnoreHanlderValidateException = false;
        public const bool InputHandlerIgnoreHandlerHandleException = false;
        public const bool FracConverterAlternativeLowerScopeCheck = false;
        public static readonly StringBuilder DebugStore = new StringBuilder();
    }
}
