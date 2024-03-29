using Lib_K_Relay.Crypto;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Utilities;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Lib_K_Relay.Networking
{
    public class Client
    {
        private static string Key0 = "6a39570cc9de4ec71d64821894";
        private static string Key1 = "c79332b197f92ba85ed281a023";
        public int LastUpdate = 0;
        public int PreviousTime = 0;
        private object _serverLock = new object();
        private object _clientLock = new object();
        private RC4Cipher _clientReceiveState = new RC4Cipher(Key0);
        private RC4Cipher _serverReceiveState = new RC4Cipher(Key1);
        private RC4Cipher _clientSendState = new RC4Cipher(Key1);
        private RC4Cipher _serverSendState = new RC4Cipher(Key0);
        private PacketBuffer _clientBuffer = new PacketBuffer();
        private PacketBuffer _serverBuffer = new PacketBuffer();
        private TcpClient _clientConnection;
        private TcpClient _serverConnection;
        private NetworkStream _clientStream;
        private NetworkStream _serverStream;
        private bool _closed = false;
        private Proxy _proxy;

        public int Time
        {
            get
            {
                return PreviousTime + (Environment.TickCount - LastUpdate);
            }
        }

        public int ObjectId
        {
            get
            {
                return PlayerData.OwnerObjectId;
            }
        }

        public PlayerData PlayerData
        {
            get;
            set;
        }

        public State State
        {
            get;
            set;
        }

        public bool Connected
        {
            get
            {
                return !_closed;
            }
        }

        public Client(Proxy proxy, TcpClient client)
        {
            _proxy = proxy;
            _clientConnection = client;
            _clientStream = _clientConnection.GetStream();
            _clientConnection.NoDelay = true;
            BeginRead(0, 4, true);
        }

        public void Connect(HelloPacket state)
        {
            _serverConnection = new TcpClient();
            _serverConnection.NoDelay = true;
            _serverConnection.BeginConnect(State.ConTargetAddress, State.ConTargetPort, ServerConnected, state);
        }

        private void ServerConnected(IAsyncResult ar)
        {
            bool success = PluginUtils.ProtectedInvoke(() =>
            {
                _serverConnection.EndConnect(ar);
                _serverStream = _serverConnection.GetStream();
                SendToServer(ar.AsyncState as Packet);
                BeginRead(0, 4, false);
                _proxy.FireClientConnected(this);
                PluginUtils.Log("Client", "Connected to remote host.");
            }, "ClientServerConnect");

            if (!success)
            {
                State.ConTargetAddress = Proxy.DefaultServer;
                State.ConTargetPort = 2050;
                Dispose();
            }
        }

        public void Dispose()
        {
            if (!_closed)
            {
                _closed = true;
                _proxy.FireClientDisconnected(this);
                _clientStream?.Close();
                _serverStream?.Close();
                _clientConnection?.Close();
                _serverConnection?.Close();
                _clientBuffer.Dispose();
                _serverBuffer.Dispose();
                PluginUtils.Log("Client", "Disconnected.");
            }
        }

        public void SendToClient(Packet packet)
        {
            Send(packet, true);
        }

        public void SendToServer(Packet packet)
        {
            Send(packet, false);
        }

        private void Send(Packet packet, bool client)
        {
            lock (client ? _clientLock : _serverLock)
            {
                bool success = false;
                try
                {
                    MemoryStream ms = new MemoryStream();
                    using (PacketWriter w = new PacketWriter(ms))
                    {
                        w.Write((int)0);
                        w.Write(packet.Id);
                        packet.Write(w);
                    }

                    byte[] data = ms.ToArray();
                    PacketWriter.BlockCopyInt32(data, data.Length);

                    if (client)
                    {
                        _clientSendState.Cipher(data);
                        _clientStream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        _serverSendState.Cipher(data);
                        _serverStream.Write(data, 0, data.Length);
                    }
                    success = true;
                }
                catch { }

                if (!success)
                {
                    Dispose();
                }
            }
        }

        private void BeginRead(int offset, int amount, bool client)
        {
            PacketBuffer buffer = client ? _clientBuffer : _serverBuffer;
            NetworkStream stream = client ? _clientStream : _serverStream;
            try
            {
                stream.BeginRead(buffer.Bytes, offset, amount, RemoteRead,
                    new Tuple<NetworkStream, PacketBuffer>(stream, buffer));
            }
            catch { }
        }

        private void RemoteRead(IAsyncResult ar)
        {
            NetworkStream stream = (ar.AsyncState as Tuple<NetworkStream, PacketBuffer>).Item1;
            PacketBuffer buffer = (ar.AsyncState as Tuple<NetworkStream, PacketBuffer>).Item2;
            bool isClient = stream == _clientStream;
            RC4Cipher cipher = isClient ? _clientReceiveState : _serverReceiveState;

            bool success = PluginUtils.ProtectedInvoke(() =>
            {
                if (!stream.CanRead) return;
                int read;
                try
                {
                    read = stream.EndRead(ar);
                }
                catch { return; }

                buffer.Advance(read);

                if (read == 0)
                {
                    Dispose();
                    return;
                }
                else if (buffer.Index == 4)
                {             
                    buffer.Resize(IPAddress.NetworkToHostOrder(
                        BitConverter.ToInt32(buffer.Bytes, 0)));
                    BeginRead(buffer.Index, buffer.BytesRemaining(), isClient);
                }
                else if (buffer.BytesRemaining() > 0)
                {         
                    BeginRead(buffer.Index, buffer.BytesRemaining(), isClient);
                }
                else
                {        
                    cipher.Cipher(buffer.Bytes);
                    Packet packet = Packet.Create(buffer.Bytes);

                    if (isClient)
                    {
                        _proxy.FireClientPacket(this, packet);
                    }
                    else
                    {
                        _proxy.FireServerPacket(this, packet);
                    }

                    if (packet.Send)
                    {
                        Send(packet, !isClient);
                    }

                    buffer.Reset();
                    BeginRead(0, 4, isClient);
                }
            }, "RemoteRead (isClient = " + isClient + ")", typeof(IOException));

            if (!success)
            {
                Dispose();
            }
        }
    }
}
