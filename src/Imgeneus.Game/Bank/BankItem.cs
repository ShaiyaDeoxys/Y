using System;
using Imgeneus.Database.Entities;

namespace Imgeneus.World.Game.Bank
{
    public class BankItem
    {
        public byte Slot { get; set; }

        public byte Type { get; set; }

        public byte TypeId { get; set; }

        public byte Count { get; set; }

        public DateTime ObtainmentTime { get; set; }

        public DateTime? ClaimTime { get; set; }

        public BankItem(byte slot, byte type, byte typeId, byte count, DateTime obtainmentTime) : this(type, typeId, count, obtainmentTime)
        {
            Slot = slot;
        }

        public BankItem(byte type, byte typeId, byte count, DateTime obtainmentTime)
        {
            Type = type;
            TypeId = typeId;
            Count = count;
            ObtainmentTime = obtainmentTime;
        }

        public BankItem(DbBankItem dbBankItem) : this(dbBankItem.Slot, dbBankItem.Type, dbBankItem.TypeId, dbBankItem.Count, dbBankItem.ObtainmentTime)
        {
        }
    }
}
