namespace Imgeneus.Logs.Entities
{
    public record ItemRecord : BaseTableEntity
    {
        public ItemRecord(uint senderId, byte type, byte typeId, byte count, int gem1, int gem2, int gem3, int gem4, int gem5, int gem6, bool isEnabledDye, byte r, byte g, byte b, string craftname, string source) : base(senderId)
        {
            Type = type;
            TypeId = typeId;
            Count = count;
            Gem1 = gem1;
            Gem2 = gem2;
            Gem3 = gem3;
            Gem4 = gem4;
            Gem5 = gem5;
            Gem6 = gem6;
            IsEnabledDye = isEnabledDye;
            R = r;
            G = g;
            B = b;
            Craftname = craftname;
            Source = source;
        }

        public int Type { get; }
        public int TypeId { get; }
        public int Count { get; }
        public int Gem1 { get; }
        public int Gem2 { get; }
        public int Gem3 { get; }
        public int Gem4 { get; }
        public int Gem5 { get; }
        public int Gem6 { get; }
        public bool IsEnabledDye { get; }
        public int R { get; }
        public int G { get; }
        public int B { get; }
        public string Craftname { get; }

        public string Source { get; }
    }
}
