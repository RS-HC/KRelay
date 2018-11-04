using System.Collections.Generic;
using System.Xml.Linq;

namespace Lib_K_Relay.GameData.DataStructures
{
    public class ProjectileStructure : IDataStructure<byte>
    {
        public byte ID
        {
            get;
            private set;
        }

        public int Damage;

        public float Speed;

        public int Size;

        public float Lifetime;

        public int MaxDamage;
        public int MinDamage;

        public float Magnitude;
        public float Amplitude;
        public float Frequency;

        public bool Wavy;
        public bool Parametric;
        public bool Boomerang;
        public bool ArmorPiercing;
        public bool MultiHit;
        public bool PassesCover;

        public Dictionary<string, float> StatusEffects;

        public string Name
        {
            get;
            private set;
        }

        public ProjectileStructure(XElement projectile)
        {
            ID = (byte)projectile.AttrDefault("id", "0").ParseInt();
            Damage = projectile.ElemDefault("Damage", "0").ParseInt();
            Speed = projectile.ElemDefault("Speed", "0").ParseFloat() / 10000f;
            Size = projectile.ElemDefault("Size", "0").ParseInt();
            Lifetime = projectile.ElemDefault("LifetimeMS", "0").ParseFloat();

            MaxDamage = projectile.ElemDefault("MaxDamage", "0").ParseInt();
            MinDamage = projectile.ElemDefault("MinDamage", "0").ParseInt();

            Magnitude = projectile.ElemDefault("Magnitude", "0").ParseFloat();
            Amplitude = projectile.ElemDefault("Amplitude", "0").ParseFloat();
            Frequency = projectile.ElemDefault("Frequency", "0").ParseFloat();

            Wavy = projectile.HasElement("Wavy");
            Parametric = projectile.HasElement("Parametric");
            Boomerang = projectile.HasElement("Boomerang");
            ArmorPiercing = projectile.HasElement("ArmorPiercing");
            MultiHit = projectile.HasElement("MultiHit");
            PassesCover = projectile.HasElement("PassesCover");

            var effects = new Dictionary<string, float>();
            projectile.Elements("ConditionEffect")
                .ForEach(effect => effects[effect.Value] = effect.AttrDefault("duration", "0").ParseFloat());

            StatusEffects = effects;
            Name = projectile.ElemDefault("ObjectId", "");
        }

        public override string ToString()
        {
            return string.Format("Projectile: {0} (0x{1:X})", Name, ID);
        }
    }
}
