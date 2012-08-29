namespace BananaMpq.Layer.Chunks
{
    struct ChunkHeader
    {
#pragma warning disable 649
        private unsafe fixed sbyte _magic[4];
        public readonly int Size;
#pragma warning restore 649

        public unsafe string Magic
        {
            get
            {
                fixed (sbyte* p = _magic)
                    return new string(new[] { (char)p[3], (char)p[2], (char)p[1], (char)p[0] });
            }
        }

        public void ValidateMagic(string expected)
        {
            if (!MagicEquals(expected))
            {
                throw new ChunkMagicException(expected, Magic);
            }
        }

        private unsafe bool MagicEquals(string expected)
        {
            fixed (sbyte* p = _magic)
            {
                for (var i = 0; i < 4; i++)
                {
                    if (expected[i] != (char)p[3 - i])
                        return false;
                }
                return true;
            }
        }

        public unsafe byte* NextChunk(byte* current)
        {
            return current + sizeof(ChunkHeader) + Size;
        }

        public static unsafe void* ChunkBegin(ChunkHeader* header)
        {
            return (byte*)header + sizeof(ChunkHeader);
        }
    }
}