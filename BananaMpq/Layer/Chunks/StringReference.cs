namespace BananaMpq.Layer.Chunks
{
    public class StringReference
    {
        public string String { get; set; }
        public int Offset { get; set; }

        public override string ToString()
        {
            return string.Format("Offset: {0:X}, {1}", Offset, String);
        }
    }
}