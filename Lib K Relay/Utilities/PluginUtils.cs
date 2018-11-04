using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lib_K_Relay.Utilities
{
    public static class PluginUtils
    {
        public static bool ProtectedInvoke(Action action, string errorProvider)
        {
            return ProtectedInvoke(action, errorProvider, null);
        }

        public static bool ProtectedInvoke(Action action, string errorProvider, Type filteredException)
        {
#if DEBUG
            action();
            return true;
#else
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                if (e.GetType() != filteredException)
                {
                    LogPluginException(e, errorProvider);
                }
                return false;
            }
#endif
        }

        public static void LogPluginException(Exception e, string caller)
        {
            MethodBase site = e.TargetSite;
            string methodName = site == null ? "<null method reference>" : site.Name;
            string className = site == null ? "" : site.ReflectedType.Name;

            Log("Error", "An exception was thrown\nwithin {0} \nat {1}\n\n{2}",
                caller, className + "." + methodName,
#if DEBUG
                e);
#else
                e.Message);
#endif
        }

        public static void Log(string sender, string message)
        {
            if (sender.Length > 13)
            {
                sender = sender.Substring(0, 13);
            }
            sender += "]";
            Console.WriteLine(string.Format("[{0,-15} {1}", sender, message));
        }

        public static void Log(string sender, string message, params object[] list)
        {
            string formatted = string.Format(message, list);
            Log(sender, formatted);
        }

        public static void ShowGUI(Form gui)
        {
            gui.Shown += (s, e) =>
            {
                gui.WindowState = FormWindowState.Minimized;
                gui.Show();
                gui.WindowState = FormWindowState.Normal;
            };

            Task.Run(() => gui.ShowDialog());
        }

        public static void ShowGenericSettingsGUI(dynamic settingsObject, string title)
        {
            ShowGUI(new FrmGenericSettings(settingsObject, title));
        }

        public static void Delay(int ms, Action callback)
        {
            Task.Run(() =>
            {
                Thread.Sleep(ms);
                callback();
            });
        }

        public static NotificationPacket CreateNotification(int objectId, string message)
        {
            return CreateNotification(objectId, 0x00FFFF, message);
        }

        public static NotificationPacket CreateNotification(int objectId, int color, string message)
        {
            NotificationPacket notif = (NotificationPacket)Packet.Create(PacketType.NOTIFICATION);
            notif.ObjectId = StealthConfig.Default.StealthEnabled ? 0 : objectId;
            notif.Message = "{\"key\":\"blank\",\"tokens\":{\"data\":\"" + message + "\"}}";
            notif.Color = color;
            return notif;
        }

        public static TextPacket CreateOryxNotification(string sender, string message)
        {
            TextPacket tpacket = (TextPacket)Packet.Create(PacketType.TEXT);
            tpacket.BubbleTime = 0;
            tpacket.CleanText = message;
            tpacket.Name = "";
            tpacket.NumStars = -1;
            tpacket.ObjectId = -1;
            tpacket.Recipient = "";
            tpacket.Text = "<KRelay> " + message;
            return tpacket;
        }
    }
}
