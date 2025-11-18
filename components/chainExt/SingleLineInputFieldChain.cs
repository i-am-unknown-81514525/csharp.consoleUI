using ui.fmt;

namespace ui.components.chainExt
{
    public static class SingleLineInputFieldChain
    {
        public static T WithActive<T>(this T v, (ForegroundColor fore, BackgroundColor back) active) where T : SingleLineInputField
        {
            v.active = active;
            return v;
        }

        public static T WithDeactive<T>(this T v, (ForegroundColor fore, BackgroundColor back) deactive) where T : SingleLineInputField
        {
            v.deactive = deactive;
            return v;
        }

        public static T WithContent<T>(this T v, string content) where T : SingleLineInputField
        {
            v.content = content;
            return v;
        }
    }
}
