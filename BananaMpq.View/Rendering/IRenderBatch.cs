namespace BananaMpq.View.Rendering
{
    public interface IRenderBatch
    {
        void Record<TIndex, TVertex>(TIndex[] indices, TVertex[] vertices)
            where TIndex : struct
            where TVertex : struct;
        void Record<TIndex>(TIndex[] indices, IRenderBatch useVerticesFrom)
            where TIndex : struct;
        void Render();
    }
}