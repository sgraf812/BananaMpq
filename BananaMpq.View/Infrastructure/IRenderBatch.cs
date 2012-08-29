namespace BananaMpq.View.Infrastructure
{
    public interface IRenderBatch
    {
        void Record<TIndex, TVertex>(TIndex[] indices, TVertex[] vertices)
            where TIndex : struct
            where TVertex : struct;
        void Render();
    }
}