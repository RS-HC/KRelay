using System.Collections.Generic;
using System.Xml.Linq;

namespace Lib_K_Relay.GameData.DataStructures
{
    public class ObjectStructure : IDataStructure<ushort>
    {
        internal static Dictionary<ushort, ObjectStructure> Load(XDocument doc)
        {
            Dictionary<ushort, ObjectStructure> map = new Dictionary<ushort, ObjectStructure>();

            doc.Element("Objects")
                .Elements("Object")
                .ForEach(obj =>
                {
                    ObjectStructure o = new ObjectStructure(obj);
                    map[o.ID] = o;
                });

            return map;
        }

        public ushort ID
        {
            get;
            private set;
        }

        public string ObjectClass;

        public ushort MaxHP;

        public float XPMult;

        public bool Static;

        public bool OccupySquare;

        public bool EnemyOccupySquare;

        public bool FullOccupy;

        public bool BlocksSight;

        public bool ProtectFromGroundDamage;

        public bool ProtectFromSink;

        public bool Enemy;

        public bool Player;

        public bool Pet;

        public bool DrawOnGround;

        public ushort Size;

        public ushort ShadowSize;

        public ushort Defense;

        public bool Flying;

        public bool God;

        public bool Quest;

        public bool Item;

        public bool Usable;

        public bool Soulbound;

        public ushort MpCost;

        public ProjectileStructure[] Projectiles;

        public string Name
        {
            get;
            private set;
        }

        public ObjectStructure(XElement obj)
        {
            ID = (ushort)obj.AttrDefault("type", "0x0").ParseHex();

            ObjectClass = obj.ElemDefault("Class", "GameObject");

            MaxHP = (ushort)obj.ElemDefault("MaxHitPoints", "0").ParseHex();
            XPMult = obj.ElemDefault("XpMult", "0").ParseFloat();

            Static = obj.HasElement("Static");
            OccupySquare = obj.HasElement("OccupySquare");
            EnemyOccupySquare = obj.HasElement("EnemyOccupySquare");
            FullOccupy = obj.HasElement("FullOccupy");
            BlocksSight = obj.HasElement("BlocksSight");
            ProtectFromGroundDamage = obj.HasElement("ProtectFromGroundDamage");
            ProtectFromSink = obj.HasElement("ProtectFromSink");
            Enemy = obj.HasElement("Enemy");
            Player = obj.HasElement("Player");
            Pet = obj.HasElement("Pet");
            DrawOnGround = obj.HasElement("DrawOnGround");

            Size = (ushort)obj.ElemDefault("Size", "0").ParseInt();
            ShadowSize = (ushort)obj.ElemDefault("ShadowSize", "0").ParseInt();
            Defense = (ushort)obj.ElemDefault("Defense", "0").ParseInt();
            Flying = obj.HasElement("Flying");
            God = obj.HasElement("God");
            Quest = obj.HasElement("Quest");

            Item = obj.HasElement("Item");
            Usable = obj.HasElement("Usable");
            Soulbound = obj.HasElement("Soulbound");
            MpCost = (ushort)obj.ElemDefault("MpCost", "0").ParseInt();

            List<ProjectileStructure> projs = new List<ProjectileStructure>();
            obj.Elements("Projectile").ForEach(projectile => projs.Add(new ProjectileStructure(projectile)));
            Projectiles = projs.ToArray();

            Name = obj.AttrDefault("id", "");
        }

        public override string ToString()
        {
            return string.Format("Object: {0} (0x{1:X})", Name, ID);
        }
    }
}
