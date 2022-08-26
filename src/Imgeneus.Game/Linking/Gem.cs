using Imgeneus.Database.Constants;
using Imgeneus.Database.Preload;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Inventory;

namespace Imgeneus.World.Game.Linking
{
    public class Gem
    {
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly DbItem _item;

        public int TypeId { get; private set; }

        public Gem(IGameDefinitionsPreloder definitionsPreloader, int typeId, byte position)
        {
            _definitionsPreloader = definitionsPreloader;
            TypeId = typeId;
            Position = position;

            _item = _definitionsPreloader.Items[(Item.GEM_ITEM_TYPE, (byte)TypeId)];
        }

        public byte Position { get; private set; }

        public ushort Str => _item.ConstStr;

        public ushort Dex => _item.ConstDex;

        public ushort Rec => _item.ConstRec;

        public ushort Int => _item.ConstInt;

        public ushort Luc => _item.ConstLuc;

        public ushort Wis => _item.ConstWis;

        public ushort HP => _item.ConstHP;

        public ushort MP => _item.ConstMP;

        public ushort SP => _item.ConstSP;

        public byte AttackSpeed => _item.AttackTime;

        public byte MoveSpeed => _item.Speed;

        public ushort Defense => _item.Defense;

        public ushort Resistance => _item.Resistance;

        public ushort MinAttack => _item.MinAttack;

        public ushort PlusAttack => _item.PlusAttack;

        public Element Element => _item.Element;

        public byte ReqIg => _item.ReqIg;

        public ushort LinkingRate => _item.ReqVg;

        public byte Absorb => _item.Exp;
    }
}
