using System.Collections.Generic;
using System.Text;

namespace BananaMpq
{
    public static class MiscUtils
    {
        public static unsafe string ConvertToAscii(byte* pointer, int length)
        {
            var bytes = new byte[length];
            var actualLength = 0;
            while (*pointer != 0)
                bytes[actualLength++] = *pointer++;
            return Encoding.ASCII.GetString(bytes, 0, actualLength);
        }
    }
}