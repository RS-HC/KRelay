using System.Collections.Generic;
using System.Xml.Linq;

namespace Lib_K_Relay.GameData.DataStructures
{
    public struct TileStructure : IDataStructure<ushort>
    {
        internal static Dictionary<ushort, TileStructure> Load(XDocument doc)
        {
            Dictionary<ushort, TileStructure> map = new Dictionary<ushort, TileStructure>();

            doc.Element("GroundTypes")
                .Elements("Ground")
                .ForEach(tile =>
                {
                    TileStructure t = new TileStructure(tile);
                    map[t.ID] = t;
                });

            return map;
        }

        public ushort ID
        {
            get;
            private set;
        }

        public bool NoWalk;

        public float Speed;

        public bool Sink;

        public ushort MinDamage;

        public ushort MaxDamage;

        public string Name
        {
            get;
            private set;
        }

        public TileStructure(XElement tile)
        {
            ID = (ushort)tile.AttrDefault("type", "0x0").ParseHex();
            NoWalk = tile.HasElement("NoWalk");
            Speed = tile.ElemDefault("Speed", "1").ParseFloat();
            Sink = tile.HasElement("Sink");
            MinDamage = (ushort)tile.ElemDefault("MinDamage", "0").ParseInt();
            MaxDamage = (ushort)tile.ElemDefault("MaxDamage", "0").ParseInt();

            Name = tile.AttrDefault("id", "");
        }

        public override string ToString()
        {
            return string.Format("Tile: {0} (0x{1:X})", Name, ID);
        }
    }
}
