using Lib_K_Relay.GameData.DataStructures;
using Lib_K_Relay.Properties;
using Lib_K_Relay.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Lib_K_Relay.GameData
{
    public class GameDataMap<IDType, DataType> where DataType : IDataStructure<IDType>
    {
        public Dictionary<IDType, DataType> Map
        {
            get;
            private set;
        }

        private GameDataMap()
        {
        }

        public GameDataMap(Dictionary<IDType, DataType> map)
        {
            Map = map;
        }

        public DataType ByID(IDType id)
        {
            return Map[id];
        }

        public DataType ByName(string name)
        {
            return Map.First(e => e.Value.Name == name).Value;
        }

        public DataType Match(Func<DataType, bool> f)
        {
            return Map.First(e => f(e.Value)).Value;
        }
    }

    public static class GameData
    {
        public static string RawObjectsXML
        {
            get;
            private set;
        }

        public static string RawPacketsXML
        {
            get;
            private set;
        }

        public static string RawTilesXML
        {
            get;
            private set;
        }

        public static GameDataMap<ushort, ItemStructure> Items;

        public static GameDataMap<ushort, TileStructure> Tiles;

        public static GameDataMap<ushort, ObjectStructure> Objects;

        public static GameDataMap<byte, PacketStructure> Packets;

        public static GameDataMap<string, ServerStructure> Servers;

        static GameData()
        {
            RawObjectsXML = Resources.Objects;
            RawPacketsXML = Resources.Packets;
            RawTilesXML = Resources.Tiles;
        }

        public static void Load()
        {
            Parallel.Invoke(
            () =>
            {
                try
                {
                    Items = new GameDataMap<ushort, ItemStructure>(ItemStructure.Load(XDocument.Load("Objects.xml")));
                    PluginUtils.Log("GameData", "loaded items from file!");
                }
                catch
                {
                    Items = new GameDataMap<ushort, ItemStructure>(ItemStructure.Load(XDocument.Parse(RawObjectsXML)));
                }
                PluginUtils.Log("GameData", "Mapped {0} items.", Items.Map.Count);
            },
            () =>
            {
                try
                {
                    Tiles = new GameDataMap<ushort, TileStructure>(TileStructure.Load(XDocument.Load("Tiles.xml")));
                    PluginUtils.Log("GameData", "loaded tiles from file!");
                }
                catch
                {
                    Tiles = new GameDataMap<ushort, TileStructure>(TileStructure.Load(XDocument.Parse(RawTilesXML)));
                }
                PluginUtils.Log("GameData", "Mapped {0} tiles.", Tiles.Map.Count);
            },
            () =>
            {
                try
                {
                    Objects = new GameDataMap<ushort, ObjectStructure>(ObjectStructure.Load(XDocument.Load("Objects.xml")));
                    PluginUtils.Log("GameData", "loaded objects from file!");
                }
                catch
                {
                    Objects = new GameDataMap<ushort, ObjectStructure>(ObjectStructure.Load(XDocument.Parse(RawObjectsXML)));
                }
                PluginUtils.Log("GameData", "Mapped {0} objects.", Objects.Map.Count);
            },
            () =>
            {
                try
                {
                    Packets = new GameDataMap<byte, PacketStructure>(PacketStructure.Load(XDocument.Load("Packets.xml")));
                    PluginUtils.Log("GameData", "loaded packets from file!");
                }
                catch
                {
                    Packets = new GameDataMap<byte, PacketStructure>(PacketStructure.Load(XDocument.Parse(RawPacketsXML)));
                }
                PluginUtils.Log("GameData", "Mapped {0} packets.", Packets.Map.Count);
            },
            () =>
            {
                const string CHAR_LIST_FILE = "char_list.xml";

                XDocument charList = null;

                try
                {
                    charList = XDocument.Load("http://realmofthemadgodhrd.appspot.com/char/list");
                }
                catch (Exception)
                {
                }

                if (charList != null && charList.Element("Error") == null)
                {
                    charList.Save(CHAR_LIST_FILE);
                }
                else if (System.IO.File.Exists(CHAR_LIST_FILE))
                {
                    charList = XDocument.Load(CHAR_LIST_FILE);
                }
                else
                {
                    PluginUtils.Log("GameData", "Error! Unable to retrieve server list.");
                    return;
                }

                Servers = new GameDataMap<string, ServerStructure>(ServerStructure.Load(charList));
                PluginUtils.Log("GameData", "Mapped {0} servers.", Servers.Map.Count);
            });

            PluginUtils.Log("GameData", "Successfully loaded game data.");
        }
    }
}
