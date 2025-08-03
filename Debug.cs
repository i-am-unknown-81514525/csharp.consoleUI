using System;
using System.Text;

namespace ui
{
    public static class DEBUG
    {
        public const bool InputHandler_IgnoreHanlderValidateException = false;
        public const bool InputHandler_IgnoreHandlerHandleException = false;
        public const bool FracConverter_AlternativeLowerScopeCheck = false;
        public static readonly StringBuilder DebugStore = new StringBuilder();
    }
}
