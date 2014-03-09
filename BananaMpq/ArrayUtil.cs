using System;

namespace BananaMpq
{
    public static class ArrayUtil
    {
        public static T[,] MakeTwoDimensionalArray<T>(T val, int rows, int columns)
        {
            return MakeTwoDimensionalArray(() => val, rows, columns);
        }

        public static T[,] MakeTwoDimensionalArray<T>(Func<T> valueFactory, int rows, int columns)
        {
            var ret = new T[rows, columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    ret[r, c] = valueFactory();
                }
            }
            return ret;
        }
    }
}