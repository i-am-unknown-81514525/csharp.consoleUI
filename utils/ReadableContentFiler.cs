using System.Linq;

namespace ui.utils
{
    public static class FilterExtension
    {
        public static bool IsReadable(char v)
        {
            if (v > 31 && v < 127)
            {
                return true;
            }
            return false;
        }

        public static string GetReadable(this string ori)
        {
            return ori.Where(x => IsReadable(x)).AsByteBuffer().AsString();
        }
    }
}