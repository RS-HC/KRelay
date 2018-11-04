using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FameBot.Data.Enums;
using FameBot.Data.Events;
using FameBot.Data.Models;
using FameBot.Helpers;
using FameBot.Services;
using FameBot.UserInterface;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace FameBot.Core {
  public class Plugin : IPlugin {
    #region IPlugin
    public string GetAuthor () {
      return "tcrane (crinx)";
    }

    public string[] GetCommands () {
      return new string[] {
        "/bind - binds the bot to the client where the command is used.",
        "/start - starts the bot",
        "/gui - opens the gui"
      };
    }

    public string GetDescription () {
      return "A bot designed to automate the process of collecting fame.";
    }

    public string GetName () {
      return "FameBot";
    }
    #endregion

    #region Client properties.
    private IntPtr flashPtr;
    private bool followTarget;
    private List<Target> targets;
    private List<Portal> portals;
    private Dictionary<int, Target> playerPositions;
    private List<Enemy> enemies;
    private List<Obstacle> obstacles;
    private List<ushort> obstacleIds;
    private Client connectedClient;
    private Location lastLocation = null;
    private Location lastAverage;
    private bool blockNextAck = false;
    private string preferredRealmName = null;
    #endregion

    private Location target_;

    public enum BagsX : short {
      Brown = 0x500,
      BrownBoosted = 0x6ad,
      Pink = 0x506,
      PinkBoosted = 0x6ae,
      Purple = 0x507,
      PurpleBoosted = 0x6ba,
      Egg = 0x508,
      EggBoosted = 0x6bb,
      Gold = 0x050E,
      GoldBoosted = 0x6bc,
      Cyan = 0x509,
      CyanBoosted = 0x6bd,
      Blue = 0x050B,
      BlueBoosted = 0x6be,
      Orange = 0x50F,
      OrangeBoosted = 0x6bf,
      Red = 0x6AC,
      RedBoosted = 0x6c0,
      White = 0x050C,
      WhiteBoosted = 0x0510,
    }
    private readonly ushort WhiteBoosted = Convert.ToUInt16 (BagsX.WhiteBoosted);
    private readonly ushort White = Convert.ToUInt16 (BagsX.White);
    private readonly ushort OrangeBoosted = Convert.ToUInt16 (BagsX.OrangeBoosted);
    private readonly ushort Orange = Convert.ToUInt16 (BagsX.Orange);
    private readonly ushort RedBoosted = Convert.ToUInt16 (BagsX.RedBoosted);
    private readonly ushort Red = Convert.ToUInt16 (BagsX.Red);
    private readonly ushort BlueBoosted = Convert.ToUInt16 (BagsX.BlueBoosted);
    private readonly ushort Blue = Convert.ToUInt16 (BagsX.Blue);
    private readonly ushort GoldBoosted = Convert.ToUInt16 (BagsX.GoldBoosted);
    private readonly ushort Gold = Convert.ToUInt16 (BagsX.Gold);

    #region Config/other properties.
    private int tickCount;
    private Configuration config;
    private FameBotGUI gui;
    private bool gotoRealm;
    private bool enabled;
    private bool isInNexus;
    private string currentMapName;
    #endregion

    #region Events
    public static event HealthEventHandler healthChanged;
    public delegate void HealthEventHandler (object sender, HealthChangedEventArgs args);

    public static event KeyEventHandler keyChanged;
    public delegate void KeyEventHandler (object sender, KeyEventArgs args);

    private static event GuiEventHandler guiEvent;
    private delegate void GuiEventHandler (GuiEvent evt);

    public static event LogEventHandler logEvent;
    public delegate void LogEventHandler (object sender, LogEventArgs args);

    private static event SendMessageEventHandler sendMessage;
    private delegate void SendMessageEventHandler (string message);

    public static event ReceiveMessageEventHandler receiveMesssage;
    public delegate void ReceiveMessageEventHandler (object sender, MessageEventArgs args);

    public static event FameUpdateEventHandler fameUpdate;
    public delegate void FameUpdateEventHandler (object sender, FameUpdateEventArgs args);
    #endregion

    #region Keys
    private bool wPressed;
    private bool aPressed;
    private bool sPressed;
    private bool dPressed;

    private bool W_PRESSED {
      get { return wPressed; }
      set {
        if (wPressed == value)
          return;
        wPressed = value;
        WinApi.SendMessage (flashPtr, value ? (uint) Key.KeyDown : (uint) Key.KeyUp, new IntPtr ((int) Key.W), IntPtr.Zero);
        keyChanged?.Invoke (this, new KeyEventArgs (Key.W, value));
      }
    }
    private bool A_PRESSED {
      get { return aPressed; }
      set {
        if (aPressed == value)
          return;
        aPressed = value;
        WinApi.SendMessage (flashPtr, value ? (uint) Key.KeyDown : (uint) Key.KeyUp, new IntPtr ((int) Key.A), IntPtr.Zero);
        keyChanged?.Invoke (this, new KeyEventArgs (Key.A, value));
      }
    }
    private bool S_PRESSED {
      get { return sPressed; }
      set {
        if (sPressed == value)
          return;
        sPressed = value;
        WinApi.SendMessage (flashPtr, value ? (uint) Key.KeyDown : (uint) Key.KeyUp, new IntPtr ((int) Key.S), IntPtr.Zero);
        keyChanged?.Invoke (this, new KeyEventArgs (Key.S, value));
      }
    }
    private bool D_PRESSED {
      get { return dPressed; }
      set {
        if (dPressed == value)
          return;
        dPressed = value;
        WinApi.SendMessage (flashPtr, value ? (uint) Key.KeyDown : (uint) Key.KeyUp, new IntPtr ((int) Key.D), IntPtr.Zero);
        keyChanged?.Invoke (this, new KeyEventArgs (Key.D, value));
      }
    }
    #endregion

    private string currRealmName = "";
    private bool loopingforrealm = false;
    private int loopingforrealmcount = 0;
    private int Attempts = 0;
    private string _bestName;
    private string bestName;
    private readonly string spampattern = @"Oryx[Jj]ackpot|LIFEPOT. ORG|RPGStash|Realmgold.com|ROTMG,ORG|RealmGold|RealmShop|Wh!te|RealmBags|-------------------|Rea[lI!]mK[i!]ngs|ORYXSH[O0]P|Rea[lI!]mStock|Rea[lI!]mPower|Rea[lL]mstock";

    public void Initialize (Proxy proxy) {
      targets = new List<Target> ();
      playerPositions = new Dictionary<int, Target> ();
      portals = new List<Portal> ();
      enemies = new List<Enemy> ();
      obstacles = new List<Obstacle> ();

      obstacleIds = new List<ushort> ();
      GameData.Objects.Map.ForEach ((kvp) => {
        if (kvp.Value.FullOccupy || kvp.Value.OccupySquare) {
          obstacleIds.Add (kvp.Key);
        }
      });
      PluginUtils.Log ("FameBot", "Found {0} obstacles.", obstacleIds.Count);

      gui = new FameBotGUI ();
      PluginUtils.ShowGUI (gui);

      config = ConfigManager.GetConfiguration ();

      Process[] processes = Process.GetProcessesByName (config.FlashPlayerName);
      if (processes.Length == 1) {
        Log ("Automatically bound to client.");
        flashPtr = processes[0].MainWindowHandle;
        gui?.SetHandle (flashPtr);
        if (config.AutoConnect)
          Start ();
      } else if (processes.Length > 1) {
        Log ("Multiple flash players running. Use the /bind command on the client you want to use.");
      } else {
        Log ("Couldn't find flash player. Use the /bind command in game, then start the bot.");
      }

      #region Proxy Hooks
      proxy.HookCommand ("bind", ReceiveCommand);
      proxy.HookCommand ("start", ReceiveCommand);
      proxy.HookCommand ("gui", ReceiveCommand);
      proxy.HookCommand ("famebot", ReceiveCommand);
      proxy.HookCommand ("stop1", ReceiveCommand);
      proxy.HookCommand ("options", ReceiveCommand);
      proxy.HookPacket (PacketType.RECONNECT, OnReconnect);
      proxy.HookPacket (PacketType.UPDATE, OnUpdate);
      proxy.HookPacket (PacketType.NEWTICK, OnNewTick);
      proxy.HookPacket (PacketType.PLAYERHIT, OnHit);
      proxy.HookPacket (PacketType.MAPINFO, OnMapInfo);
      proxy.HookPacket (PacketType.TEXT, OnText);
      proxy.HookPacket (PacketType.GOTOACK, (client, packet) => {
        if (blockNextAck) {
          packet.Send = false;
          blockNextAck = false;
        }
      });
      #endregion

      proxy.ClientConnected += (client) => {
        connectedClient = client;
        targets.Clear ();
        playerPositions.Clear ();
        enemies.Clear ();
        obstacles.Clear ();
        followTarget = false;
        isInNexus = false;
        ResetAllKeys ();
      };

      proxy.ClientDisconnected += (client) => {
        Log ("Client disconnected. Waiting a few seconds before trying to press play...");
        PressPlay ();
      };

      guiEvent += (evt) => {
        switch (evt) {
          case GuiEvent.StartBot:
            Start ();
            break;
          case GuiEvent.StopBot:
            Stop ();
            break;
          case GuiEvent.SettingsChanged:
            Log ("Updated config");
            config = ConfigManager.GetConfiguration ();
            break;
        }
      };

      sendMessage += (message) => {
        if (!(connectedClient?.Connected ?? false))
          return;
        PlayerTextPacket packet = (PlayerTextPacket) Packet.Create (PacketType.PLAYERTEXT);
        packet.Text = message;
        connectedClient.SendToServer (packet);
      };

      proxy.HookPacket (PacketType.FAILURE, (client, packet) => {
        FailurePacket p = packet as FailurePacket;

        if (p.ErrorId != 9) Console.WriteLine ("<FAILURE> ID: " + p.ErrorId + " Message: " + p.ErrorMessage);

        if (p.ErrorMessage == "{\"key\":\"server.realm_full\"}") {
          Attempts++;
          if (Attempts >= 2 && bestName != "") {
            _bestName = bestName;
            ReconnectPacket reconnect = (ReconnectPacket) Packet.Create (PacketType.RECONNECT);
            reconnect.Name = "{\"text\":\"server.realm_of_the_mad_god\"}";
            reconnect.Host = "";
            reconnect.Stats = "";
            reconnect.Port = -1;
            reconnect.GameId = -3;
            reconnect.KeyTime = -1;
            reconnect.IsFromArena = false;
            reconnect.Key = new byte[0];

            connectedClient.SendToClient (reconnect);
            Attempts = 0;
            loopingforrealm = true;
          }
        }
      });

      proxy.HookPacket (PacketType.CREATESUCCESS, (client, packet) => {
        target_ = null;
        if (enabled) {
          PluginUtils.Delay (200, () => Stop ());
          PluginUtils.Delay (500, () => Stop ());
          PluginUtils.Delay (1300, () => Stop ());
          PluginUtils.Delay (2200, () => Start ());
        }
      });
    }

    private void OnReconnect (Client client, Packet packet) {
      ReconnectPacket p = (ReconnectPacket) packet;
      if (p.Name.StartsWith ("NexusPortal")) {
        currRealmName = p.Name.Split ('.') [1];
      } else { }

      if (loopingforrealmcount >= 50) {
        loopingforrealm = false;
      }

      if (currRealmName == _bestName) {
        loopingforrealm = false;
        loopingforrealmcount = 0;
      } else if (loopingforrealm) {
        loopingforrealmcount++;
        Console.WriteLine ("Searching for " + _bestName + ". Found " + currRealmName + " instead.");

        ReconnectPacket reconnect = (ReconnectPacket) Packet.Create (PacketType.RECONNECT);
        reconnect.Name = "{\"text\":\"server.realm_of_the_mad_god\"}";
        reconnect.Host = "";
        reconnect.Stats = "";
        reconnect.Port = -1;
        reconnect.GameId = -3;
        reconnect.KeyTime = -1;
        reconnect.IsFromArena = false;
        reconnect.Key = new byte[0];

        connectedClient.SendToClient (reconnect);
      }
    }
    private void ReceiveCommand (Client client, string cmd, string[] args) {
      switch (cmd) {
        case "bind":
          flashPtr = WinApi.GetForegroundWindow ();

          try {
            var flashProcess = Process.GetProcesses ().Single (p => p.Id != 0 && p.MainWindowHandle == flashPtr);
            if (flashProcess.ProcessName != config.FlashPlayerName) {
              gui?.ShowChangeFlashNameMessage (flashProcess.ProcessName, config.FlashPlayerName, () => {
                config.FlashPlayerName = flashProcess.ProcessName;
                client.Notify ("Updated config!");
                ConfigManager.WriteXML (config);
              });
            }
          } catch {

          }

          gui?.SetHandle (flashPtr);
          client.Notify ("FameBot is now active");
          break;
        case "start":
          Start ();
          client.Notify ("FameBot is starting");
          break;
        case "stop1":
          Stop ();
          client.Notify ("FameBot is stopping");
          break;
        case "gui":
          gui?.Close ();
          gui = new FameBotGUI ();
          gui.Show ();
          break;
        case "options":
          PluginUtils.ShowGenericSettingsGUI (Settings.Default, "Bot Settings");
          break;
        case "famebot":
          if (args.Length >= 1) {
            if (string.Compare ("set", args[0], true) == 0) {
              if (args.Length < 2 || string.IsNullOrEmpty (args[1])) {
                client.Notify ("No argument to set was provided");
                return;
              }
              var setting = args[1].ToLower ();
              switch (setting) {
                case "realmposition":
                case "rp":
                  config.RealmLocation = client.PlayerData.Pos;
                  ConfigManager.WriteXML (config);
                  client.Notify ("Successfully changed realm position!");
                  break;
                case "fountainposition":
                case "fp":
                  config.FountainLocation = client.PlayerData.Pos;
                  ConfigManager.WriteXML (config);
                  client.Notify ("Successfully changed fountain position!");
                  break;
                default:
                  client.Notify ("Unrecognized setting.");
                  break;
              }
            }
            if (string.Compare ("prefer", args[0], true) == 0) {
              if (args.Length < 2 || string.IsNullOrEmpty (args[1])) {
                client.Notify ("No realm name was provided");
                return;
              }
              preferredRealmName = args[1];
              client.Notify ("Set preferred realm to " + args[1]);
            }
          }
          break;
      }
    }

    public static void InvokeGuiEvent (GuiEvent evt) {
      guiEvent?.Invoke (evt);
    }

    public static void InvokeSendMessageEvent (string message) {
      sendMessage?.Invoke (message);
    }

    private void Stop () {
      if (!enabled)
        return;
      Log ("Stopping bot.");
      followTarget = false;
      gotoRealm = false;
      targets.Clear ();
      enabled = false;
      isInNexus = false;
    }

    private void Start () {
      if (enabled)
        return;
      Log ("Starting bot.");
      targets.Clear ();
      enabled = true;
      if (currentMapName == null)
        return;
      if (currentMapName.Equals ("Nexus") && config.AutoConnect) {
        gotoRealm = true;
        followTarget = false;
        if (connectedClient != null)
          MoveToRealms (connectedClient);
      } else {
        gotoRealm = false;
        followTarget = true;
      }
    }

    private void Escape (Client client) {
      Log ("Escaping to nexus.");
      client.SendToServer (Packet.Create (PacketType.ESCAPE));
    }

    private void Log (string message) {
      logEvent?.Invoke (this, new LogEventArgs (message));
    }

    private async void PressPlay () {
      await Task.Delay (TimeSpan.FromSeconds (5));

      if (!config.AutoConnect)
        return;
      if (!enabled)
        return;

      if ((connectedClient?.Connected ?? false)) {
        Log ("Client is connected. No need to press play.");
        return;
      } else
        Log ("Client still not connected. Pressing play button...");

      RECT windowRect = new RECT ();
      WinApi.GetWindowRect (flashPtr, ref windowRect);
      var size = windowRect.GetSize ();

      int playButtonX = size.Width / 2 + windowRect.Left;
      int playButtonY = (int) ((double) size.Height * 0.92) + windowRect.Top;

      POINT relativePoint = new POINT (playButtonX, playButtonY);
      WinApi.ScreenToClient (flashPtr, ref relativePoint);

      WinApi.SendMessage (flashPtr, (uint) MouseButton.LeftButtonDown, new IntPtr (0x1), new IntPtr ((relativePoint.Y << 16) | (relativePoint.X & 0xFFFF)));
      WinApi.SendMessage (flashPtr, (uint) MouseButton.LeftButtonUp, new IntPtr (0x1), new IntPtr ((relativePoint.Y << 16) | (relativePoint.X & 0xFFFF)));

      PressPlay ();
    }

    private void ResetAllKeys () {
      W_PRESSED = false;
      A_PRESSED = false;
      S_PRESSED = false;
      D_PRESSED = false;
    }

    #region PacketHookMethods
    private void OnUpdate (Client client, Packet p) {
      UpdatePacket packet = p as UpdatePacket;

      foreach (Entity obj in packet.NewObjs) {
        if (Enum.IsDefined (typeof (Classes), (short) obj.ObjectType)) {
          PlayerData playerData = new PlayerData (obj.Status.ObjectId);
          playerData.Class = (Classes) obj.ObjectType;
          playerData.Pos = obj.Status.Position;
          foreach (var data in obj.Status.Data) {
            playerData.Parse (data.Id, data.IntValue, data.StringValue);
          }

          if (playerData.Stars < Settings.Default.starsToCluster) continue;

          if (playerPositions.ContainsKey (obj.Status.ObjectId))
            playerPositions.Remove (obj.Status.ObjectId);
          playerPositions.Add (obj.Status.ObjectId, new Target (obj.Status.ObjectId, playerData.Name, playerData.Pos));
        }
        if (obj.ObjectType == 1810) {
          foreach (var data in obj.Status.Data) {
            if (data.StringValue != null) {
              string pattern = @"\.(\w+) \((\d+)";
              var match = Regex.Match (data.StringValue, pattern);

              var portal = new Portal (obj.Status.ObjectId, int.Parse (match.Groups[2].Value), match.Groups[1].Value, obj.Status.Position);
              if (portals.Exists (ptl => ptl.ObjectId == obj.Status.ObjectId))
                portals.RemoveAll (ptl => ptl.ObjectId == obj.Status.ObjectId);
              portals.Add (portal);
            }
          }
        }
        if (currentMapName != "Nexus" /*&& Settings.Default.walkToBags*/) {
          if (
            (obj.ObjectType == Orange || obj.ObjectType == OrangeBoosted ||
              obj.ObjectType == Red || obj.ObjectType == RedBoosted ||
              obj.ObjectType == Blue || obj.ObjectType == BlueBoosted ||
              obj.ObjectType == Gold || obj.ObjectType == GoldBoosted ||
              obj.ObjectType == White || obj.ObjectType == WhiteBoosted)) {
            followTarget = false;
            targets.Clear ();

            target_ = obj.Status.Position;
            gotoRealm = true;
            MoveToRealms (connectedClient);
            PluginUtils.Delay (3000, () => followTarget = true);
            PluginUtils.Delay (3000, () => gotoRealm = false);
            PluginUtils.Delay (3000, () => followTarget = true);
          }
        }
        if (Enum.IsDefined (typeof (EnemyId), (int) obj.ObjectType) && config.EnableEnemyAvoidance) {
          if (enemies.Exists (en => en.ObjectId == obj.Status.ObjectId))
            enemies.RemoveAll (en => en.ObjectId == obj.Status.ObjectId);
          enemies.Add (new Enemy (obj.Status.ObjectId, obj.Status.Position));
        }

        if (obstacleIds.Contains (obj.ObjectType)) {
          if (!obstacles.Exists (obstacle => obstacle.ObjectId == obj.Status.ObjectId))
            obstacles.Add (new Obstacle (obj.Status.ObjectId, obj.Status.Position));
        }
      }

      foreach (int dropId in packet.Drops) {
        if (playerPositions.ContainsKey (dropId)) {
          if (followTarget && targets.Exists (t => t.ObjectId == dropId)) {
            targets.Remove (targets.Find (t => t.ObjectId == dropId));
            Log (string.Format ("Dropping \"{0}\" from targets.", playerPositions[dropId].Name));
            if (targets.Count == 0) {
              Log ("No targets left in target list.");
              if (config.EscapeIfNoTargets)
                Escape (client);
            }
          }
          playerPositions.Remove (dropId);
        }

        if (enemies.Exists (en => en.ObjectId == dropId))
          enemies.RemoveAll (en => en.ObjectId == dropId);

        if (portals.Exists (ptl => ptl.ObjectId == dropId))
          portals.RemoveAll (ptl => ptl.ObjectId == dropId);
      }
    }

    private void OnMapInfo (Client client, Packet p) {
      MapInfoPacket packet = p as MapInfoPacket;
      if (packet == null)
        return;
      portals.Clear ();
      currentMapName = packet.Name;

      if (packet.Name == "Oryx's Castle" && enabled) {
        Log ("Escaping from oryx's castle.");
        Escape (client);
        return;
      }
      if (packet.Name == "Nexus" && config.AutoConnect && enabled) {
        isInNexus = true;
        gotoRealm = true;
        MoveToRealms (client);
      } else {
        gotoRealm = false;
        if (enabled)
          followTarget = true;
      }
    }

    private void OnHit (Client client, Packet p) {
      float healthPercentage = (float) client.PlayerData.Health / (float) client.PlayerData.MaxHealth * 100f;
      if (healthPercentage < config.AutonexusThreshold * 1.25f)
        Log (string.Format ("Health at {0}%", (int) (healthPercentage)));
    }

    private void OnNewTick (Client client, Packet p) {
      NewTickPacket packet = p as NewTickPacket;
      tickCount++;

      float healthPercentage = (float) client.PlayerData.Health / (float) client.PlayerData.MaxHealth * 100f;
      healthChanged?.Invoke (this, new HealthChangedEventArgs (healthPercentage));

      if (healthPercentage < config.AutonexusThreshold && !(currentMapName?.Equals ("Nexus") ?? false) && enabled)
        Escape (client);

      fameUpdate?.Invoke (this, new FameUpdateEventArgs (client.PlayerData?.CharacterFame ?? -1, client.PlayerData?.CharacterFameGoal ?? -1));

      if (tickCount % config.TickCountThreshold == 0) {
        if (followTarget && playerPositions.Count > 0 && !gotoRealm) {
          List<Target> newTargets = D36n4.Invoke (playerPositions.Values.ToList (), config.Epsilon, config.MinPoints, config.FindClustersNearCenter);
          if (newTargets == null) {
            if (targets.Count != 0 && config.EscapeIfNoTargets)
              Escape (client);
            targets.Clear ();
            Log ("No valid clusters found.");
          } else {
            if (targets.Count != newTargets.Count)
              Log (string.Format ("Now targeting {0} players.", newTargets.Count));
            targets = newTargets;
          }
        }
        tickCount = 0;
      }

      foreach (Status status in packet.Statuses) {
        if (playerPositions.ContainsKey (status.ObjectId))
          playerPositions[status.ObjectId].UpdatePosition (status.Position);

        if (enemies.Exists (en => en.ObjectId == status.ObjectId))
          enemies.Find (en => en.ObjectId == status.ObjectId).Location = status.Position;

        if (portals.Exists (ptl => ptl.ObjectId == status.ObjectId) && (isInNexus)) {
          foreach (var data in status.Data) {
            if (data.StringValue != null) {
              var strCount = data.StringValue.Split (' ') [1].Split ('/') [0].Remove (0, 1);
              portals[portals.FindIndex (ptl => ptl.ObjectId == status.ObjectId)].PlayerCount = int.Parse (strCount);
            }
          }
        }

        if (isInNexus && status.ObjectId == client.ObjectId) {
          foreach (var data in status.Data) {
          if (data.Id == StatsType.Speed) {
          if (data.IntValue > 45) {
          List<StatData> list = new List<StatData> (status.Data) {
          new StatData {
          Id = StatsType.Speed, IntValue = 45
          }
                };
                status.Data = list.ToArray ();
              }
            }
          }
        }
      }

      if (enabled) {
        if (lastLocation != null) {
          if (client.PlayerData.Pos.X == lastLocation.X && client.PlayerData.Pos.Y == lastLocation.Y) {
            ResetAllKeys ();
          }
        }
        lastLocation = client.PlayerData.Pos;
      }

      if (!followTarget && !gotoRealm) {
        ResetAllKeys ();
      }

      if (followTarget && targets.Count > 0) {
        var targetPosition = new Location (targets.Average (t => t.Position.X), targets.Average (t => t.Position.Y));
        if (lastAverage != null) {
          var dir = targetPosition.Subtract (lastAverage);
          var faraway = targetPosition.Add (dir.Scale (20));
          var desiredTargets = (int) (targets.Count * (config.TrainTargetPercentage / 100f));
          List<Target> newTargets = new List<Target> ();
          for (int i = 0; i < desiredTargets; i++) {
            var closest = targets.OrderBy ((t) => t.Position.DistanceSquaredTo (faraway)).First ();
            newTargets.Add (closest);
            targets.RemoveAll ((t) => t.Name == closest.Name);
          }
          targets.AddRange (newTargets);
          lastAverage = targetPosition;
          targetPosition = new Location (newTargets.Average (t => t.Position.X), newTargets.Average (t => t.Position.Y));
        } else {
          lastAverage = targetPosition;
        }

        if (client.PlayerData.Pos.DistanceTo (targetPosition) > config.TeleportDistanceThreshold) {
          var name = targets.OrderBy (t => t.Position.DistanceTo (targetPosition)).First ().Name;
          if (name != client.PlayerData.Name) {
            var tpPacket = (PlayerTextPacket) Packet.Create (PacketType.PLAYERTEXT);
            tpPacket.Text = "/teleport " + name;
            client.SendToServer (tpPacket);
          }
        }

        if (config.EnableEnemyAvoidance && enemies.Exists (en => en.Location.DistanceSquaredTo (client.PlayerData.Pos) <= (config.EnemyAvoidanceDistance * config.EnemyAvoidanceDistance))) {
          Location closestEnemy = enemies.OrderBy (en => en.Location.DistanceSquaredTo (client.PlayerData.Pos)).First ().Location;
          double angleDifference = client.PlayerData.Pos.GetAngleDifferenceDegrees (targetPosition, closestEnemy);

          if (Math.Abs (angleDifference) < 70.0) {
            double angle = Math.Atan2 (client.PlayerData.Pos.Y - closestEnemy.Y, client.PlayerData.Pos.X - closestEnemy.X);
            if (angleDifference <= 0)
              angle += (Math.PI / 2);
            if (angleDifference > 0)
              angle -= (Math.PI / 2);

            float newX = closestEnemy.X + config.EnemyAvoidanceDistance * (float) Math.Cos (angle);
            float newY = closestEnemy.Y + config.EnemyAvoidanceDistance * (float) Math.Sin (angle);

            var avoidPos = new Location (newX, newY);
            CalculateMovement (client, avoidPos, config.FollowDistanceThreshold);
            return;
          }
        }

        if (obstacles.Exists (obstacle => obstacle.Location.DistanceSquaredTo (client.PlayerData.Pos) <= 4)) {
          Location closestObstacle = obstacles.OrderBy (obstacle => obstacle.Location.DistanceSquaredTo (client.PlayerData.Pos)).First ().Location;
          double angleDifference = client.PlayerData.Pos.GetAngleDifferenceDegrees (targetPosition, closestObstacle);

          if (Math.Abs (angleDifference) < 70.0) {
            double angle = Math.Atan2 (client.PlayerData.Pos.Y - closestObstacle.Y, client.PlayerData.Pos.X - closestObstacle.X);
            if (angleDifference <= 0)
              angle += (Math.PI / 2);
            if (angleDifference > 0)
              angle -= (Math.PI / 2);

            float newX = closestObstacle.X + 2f * (float) Math.Cos (angle);
            float newY = closestObstacle.Y + 2f * (float) Math.Sin (angle);

            var avoidObstaclePos = new Location (newX, newY);
            CalculateMovement (client, avoidObstaclePos, 0.75f);
            return;
          }
        }

        CalculateMovement (client, targetPosition, config.FollowDistanceThreshold);
      }
    }

    private void OnText (Client client, Packet p) {
      TextPacket packet = p as TextPacket;
      if (packet.Name == client.PlayerData?.Name || packet.NumStars < 1)
        return;
      if (Regex.IsMatch (packet.Text, spampattern, RegexOptions.IgnoreCase) || packet.Text == " ")
        return;
      receiveMesssage?.Invoke (this, new MessageEventArgs (packet.Text, packet.Name, packet.Recipient == client.PlayerData?.Name ? true : false));

      if (Settings.Default.blockChat && packet.NumStars != -1 && packet.Recipient != "*Guild*") {
        if (packet.Name != client.PlayerData.Name && packet.Recipient != client.PlayerData.Name) {
          packet.Send = false;
        }
      }
    }
    #endregion

    private async void MoveToRealms (Client client, bool realmChosen = false) {
      if (client == null) {
        Log ("No client passed to MoveToRealms.");
        return;
      }
      Location target = config.RealmLocation;

      if (client.PlayerData == null) {
        await Task.Delay (5);
        MoveToRealms (client);
        return;
      }

      if (target_ != null) target = target_;
      var healthPercentage = (float) client.PlayerData.Health / (float) client.PlayerData.MaxHealth;
      if (healthPercentage < 0.95f && currentMapName == "Nexus")
        target = config.FountainLocation;

      bestName = "";
      if ((client.PlayerData.Pos.Y <= config.RealmLocation.Y + 1f && client.PlayerData.Pos.Y != 0) || realmChosen) {
        if (portals.Count != 0) {
          bool hasNoPreferredRealm = true;
          if (!string.IsNullOrEmpty (preferredRealmName)) {
            if (portals.Exists (ptl => string.Compare (ptl.Name, preferredRealmName, true) == 0)) {
              hasNoPreferredRealm = false;
              Portal preferred = portals.Single (ptl => string.Compare (ptl.Name, preferredRealmName, true) == 0);
              target = preferred.Location;
              bestName = preferred.Name;
              realmChosen = true;
            } else {
              client.Notify (preferredRealmName + " not found. Choosing new realm");
              Log ("The realm \"" + preferredRealmName + "\" was not found. Choosing the best realm instead...");
              preferredRealmName = null;
            }
          }

          if (hasNoPreferredRealm) {
            int bestCount = 0;
            if (portals.Where (ptl => ptl.PlayerCount == 85).Count () > 1) {
              foreach (Portal ptl in portals.Where (ptl => ptl.PlayerCount == 85)) {
                int count = playerPositions.Values.Where (plr => plr.Position.DistanceSquaredTo (ptl.Location) <= 4).Count ();
                if (count > bestCount) {
                  bestCount = count;
                  bestName = ptl.Name;
                  target = ptl.Location;
                  realmChosen = true;
                }
              }
            } else {
              Portal ptl = portals.OrderByDescending (prtl => prtl.PlayerCount).First ();
              target = ptl.Location;
              bestName = ptl.Name;
              realmChosen = true;
            }
          }
        } else if (target_ != null) target = target_;
        else
          target = config.RealmLocation;
      }

      CalculateMovement (client, target, 0.5f);

      if (client.PlayerData.Pos.DistanceTo (target) < 1f && portals.Count != 0) {
        if (client.PlayerData.Pos.DistanceTo (target) <= client.PlayerData.TilesPerTick () && client.PlayerData.Pos.DistanceTo (target) > 0.01f) {
          if (client.Connected) {
            ResetAllKeys ();
            GotoPacket gotoPacket = Packet.Create (PacketType.GOTO) as GotoPacket;
            gotoPacket.Location = target;
            gotoPacket.ObjectId = client.ObjectId;
            blockNextAck = true;
            client.SendToClient (gotoPacket);
          }
        }
        if (client.State.LastRealm?.Name.Contains (bestName) ?? false) {
          Log ("Last realm is still the best realm. Sending reconnect.");
          if (client.ConnectTo (client.State.LastRealm)) {
            gotoRealm = false;
            return;
          }
        }

        Log ("Attempting connection.");
        gotoRealm = false;
        AttemptConnection (client, portals.OrderBy (ptl => ptl.Location.DistanceSquaredTo (client.PlayerData.Pos)).First ().ObjectId);
      }
      await Task.Delay (5);
      if (gotoRealm) {
        MoveToRealms (client, realmChosen);
      } else {
        Log ("Stopped moving to realm.");
      }
    }

    private async void AttemptConnection (Client client, int portalId) {
      UsePortalPacket packet = (UsePortalPacket) Packet.Create (PacketType.USEPORTAL);
      packet.ObjectId = portalId;

      if (!portals.Exists (ptl => ptl.ObjectId == portalId)) {
        gotoRealm = true;
        MoveToRealms (client);
        return;
      }

      var pCount = portals.Find (p => p.ObjectId == portalId).PlayerCount;
      if (connectedClient.Connected && pCount < 999)
        client.SendToServer (packet);
      await Task.Delay (TimeSpan.FromSeconds (0.2));
      if (client.Connected && enabled)
        AttemptConnection (client, portalId);
      else if (enabled)
        Log ("Connection successful.");
      else
        Log ("Bot disabled, cancelling connection attempt.");
    }

    private void CalculateMovement (Client client, Location targetPosition, float tolerance) {
      if (client.PlayerData.Pos.X < targetPosition.X - tolerance) {
        D_PRESSED = true;
        A_PRESSED = false;
      } else if (client.PlayerData.Pos.X <= targetPosition.X + tolerance) {
        D_PRESSED = false;
      }
      if (client.PlayerData.Pos.X > targetPosition.X + tolerance) {
        A_PRESSED = true;
        D_PRESSED = false;
      } else if (client.PlayerData.Pos.X >= targetPosition.X - tolerance) {
        A_PRESSED = false;
      }

      if (client.PlayerData.Pos.Y < targetPosition.Y - tolerance) {
        S_PRESSED = true;
        W_PRESSED = false;
      } else if (client.PlayerData.Pos.Y <= targetPosition.Y + tolerance) {
        S_PRESSED = false;
      }
      if (client.PlayerData.Pos.Y > targetPosition.Y + tolerance) {
        S_PRESSED = false;
        W_PRESSED = true;
      } else if (client.PlayerData.Pos.Y >= targetPosition.Y - tolerance) {
        W_PRESSED = false;
      }
    }
  }
}