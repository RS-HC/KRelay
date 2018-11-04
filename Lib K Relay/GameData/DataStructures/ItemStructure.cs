using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Lib_K_Relay.GameData.DataStructures
{
    public class ItemStructure : IDataStructure<ushort>
    {
        internal static Dictionary<ushort, ItemStructure> Load(XDocument doc)
        {
            Dictionary<ushort, ItemStructure> map = new Dictionary<ushort, ItemStructure>();

            doc.Element("Objects")
                .Elements("Object")
                .Where(elem => elem.HasElement("Item"))
                .ForEach(item =>
                {
                    ItemStructure i = new ItemStructure(item);
                    map[i.ID] = i;
                });

            return map;
        }

        public enum Tiers : byte
        {
            T0 = 0,
            T1,
            T2,
            T3,
            T4,
            T5,
            T6,
            T7,
            T8,
            T9,
            T10,
            T11,
            T12,
            T13,

            T14,

            T15,

            UT = 255
        }

        public ushort ID
        {
            get;
            private set;
        }

        public ProjectileStructure Projectile;

        public int NumProjectiles;

        public Tiers Tier;

        public byte SlotType;     

        public float RateOfFire;

        public uint FeedPower;

        public byte BagType;

        public byte MPCost;

        public byte FameBonus;

        public bool Soulbound;

        public bool Usable;

        public bool Consumable;

        public string Name
        {
            get;
            private set;
        }

        public ItemStructure(XElement item)
        {
            ID = (ushort)item.AttrDefault("type", "0x0").ParseHex();
            Tier = item.HasElement("Tier") ? (Tiers)item.Element("Tier").Value.ParseInt() : Tiers.UT;
            SlotType = (byte)item.ElemDefault("SlotType", "0").ParseInt();
            RateOfFire = item.ElemDefault("RateOfFire", "1").ParseFloat();
            FeedPower = (uint)item.ElemDefault("feedPower", "0").ParseInt();
            BagType = (byte)item.ElemDefault("BagType", "0").ParseInt();
            MPCost = (byte)item.ElemDefault("MpCost", "0").ParseInt();
            FameBonus = (byte)item.ElemDefault("FameBonus", "0").ParseInt();

            Soulbound = item.HasElement("Soulbound");
            Usable = item.HasElement("Usable");
            Consumable = item.HasElement("Consumable");

            Name = item.AttrDefault("id", "");

            NumProjectiles = item.ElemDefault("NumProjectiles", "0").ParseInt();
            if (item.HasElement("Projectile"))
            {
                Projectile = new ProjectileStructure(item.Element("Projectile"));
            }
        }

        public override string ToString()
        {
            return string.Format("Item: {0} (0x{1:X})", Name, ID);
        }
    }
}
