namespace LunaNav
{
    public class SpanPool
    {
        public static int SpansPerPool = 2048;

        public SpanPool Next { get; set; }
        public Span[] Items { get; set; }

        public SpanPool()
        {
            Items = new Span[SpansPerPool];
        }
    }
}