using System;

namespace LunaNav
{
    [Serializable]
    public class Span
    {
        public uint SMin { get; set; }
        public uint SMax { get; set; }
        public uint Area { get; set; }
        public Span Next { get; set; }

        public Span()
        {
        }
    }
}