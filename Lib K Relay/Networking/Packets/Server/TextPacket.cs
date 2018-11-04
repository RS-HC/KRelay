using System;

namespace Lib_K_Relay.Networking.Packets.Server
{

    public class TextPacket : Packet
    {
        public string Name;

        public int ObjectId;

        public int NumStars;

        public byte BubbleTime;

        public string Recipient;

        public string Text;

        public string CleanText;

        public bool isSupporter;

        public override PacketType Type
        {
            get
            {
                return PacketType.TEXT;
            }
        }

        public override void Read(PacketReader r)
        {
            this.Name = r.ReadString();
            this.ObjectId = r.ReadInt32();
            this.NumStars = r.ReadInt32();
            this.BubbleTime = r.ReadByte();
            this.Recipient = r.ReadString();
            this.Text = r.ReadString();
            this.CleanText = r.ReadString();
            this.isSupporter = r.ReadBoolean();
        }
        
        public override void Write(PacketWriter w)
        {
            w.Write(this.Name);
            w.Write(this.ObjectId);
            w.Write(this.NumStars);
            w.Write(this.BubbleTime);
            w.Write(this.Recipient);
            w.Write(this.Text);
            w.Write(this.CleanText);
            w.Write(this.isSupporter);
        }
    }
}
