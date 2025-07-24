

namespace ui.utils
{
    public static class Array2DHandler
    {
        public static T[,] CopyTo<T>(T[,] src, T[,] dest, (uint minLeft, uint minTop) location)
        {
            for (int x = (int)location.minLeft; x - (int)location.minLeft < src.GetLength(0) && x < dest.GetLength(0); x++)
            {
                for (int y = (int)location.minTop; y - (int)location.minTop < src.GetLength(1) && y < dest.GetLength(1); y++)
                {
                    dest[x, y] = src[x - (int)location.minLeft, y - (int)location.minTop];
                }
            }
            return dest;
        }
        public static T[,] changeSize<T>(T[,] src, (uint x, uint y) loc, T defaultValue)
        {
            T[,] newArr = new T[loc.x, loc.y];
            if (loc.x == src.GetLength(0) && loc.y == src.GetLength(1))
                return (T[,])src.Clone();
            for (int x = 0; x < loc.x && x < src.GetLength(0); x++)
            {
                for (int y = 0; y < loc.y && x < src.GetLength(1); y++)
                {
                    newArr[x, y] = src[x, y];
                }
                for (int y = src.GetLength(1); y < loc.y; y++)
                {
                    newArr[x, y] = defaultValue;
                }
            }
            for (int x = src.GetLength(0); x < loc.x; x++)
            {
                for (int y = 0; y < loc.y; y++)
                {
                    newArr[x, y] = defaultValue;
                }
            }
            return newArr;
        }
    }
}