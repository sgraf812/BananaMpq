using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BananaMpq.Layer.Chunks
{
    public class StringReferenceChunk : Chunk
    {
        public IList<StringReference> Strings { get; private set; }

        internal unsafe StringReferenceChunk(ChunkHeader* header) : base(header)
        {
            var begin = (byte*)ChunkHeader.ChunkBegin(header);
            var end = begin + header->Size;
            var curBegin = begin;
            var cur = begin;
            Strings = new List<StringReference>();
            
            while(cur < end)
            {
                if (*cur == 0)
                {
                    var length = (int)(cur - curBegin);
                    if (length > 0)
                    {
                        Strings.Add(new StringReference
                        {
                            String = Marshal.PtrToStringAnsi((IntPtr)curBegin, length),
                            Offset = (int)(curBegin - begin)
                        });
                    }
                    curBegin = cur + 1;
                }
                cur++;
            }
        }

        public override IEnumerable<System.Reflection.PropertyInfo> VisualizableProperties
        {
            get { return base.VisualizableProperties.Concat(new[] { typeof(StringReferenceChunk).GetProperty("Strings") }); }
        }
    }
}