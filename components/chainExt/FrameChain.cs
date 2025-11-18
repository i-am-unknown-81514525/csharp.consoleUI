namespace ui.components.chainExt
{
    public static class FrameChain
    {
        public static T WithTitle<T>(this T v, GroupComponentConfig title) where T : Frame
        {
            v.title = title;
            return v;
        }

        public static T WithInner<T>(this T v, IComponent inner) where T : Frame
        {
            v.inner = inner;
            return v;
        }
    }
}
