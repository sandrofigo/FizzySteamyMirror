using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.FizzySteam
{
    [HelpURL("https://github.com/Chykary/FizzySteamyMirror")]
    public class FizzySteamyMirror : Transport
    {
        private Client client;
        private Server server;

        public EP2PSend[] Channels = new EP2PSend[2] { EP2PSend.k_EP2PSendReliable, EP2PSend.k_EP2PSendUnreliable };

        [Tooltip("Timeout for connecting in seconds.")]
        public int Timeout = 25;
        [Tooltip("Message update rate in milliseconds.")]
        public int messageUpdateRate = 35;

        private void Start()
        {
            Debug.Assert(Channels != null && Channels.Length > 0, "No channel configured for FizzySteamMirror.");
        }

        // client
        public override bool ClientConnected() => client != null && client.Active;
        public override void ClientConnect(string address)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("SteamWorks not initialized. Client could not be started.");
                return;
            }

            if (client == null)
            {
                client = Client.CreateClient(this, address);
            }
            else
            {
                Debug.LogError("Client already running!");
            }            
        }
        public override bool ClientSend(int channelId, ArraySegment<byte> segment) => client.Send(segment.Array, channelId);
        public override void ClientDisconnect() => client.Disconnect();


        // server
        public override bool ServerActive() => server != null && server.Active;
        public override void ServerStart()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("SteamWorks not initialized. Server could not be started.");
                return;
            }

            if (server == null)
            {
                server = Server.CreateServer(this, NetworkManager.singleton.maxConnections);
            }
            else
            {
                Debug.LogError("Server already started!");
            }
        }


        public override Uri ServerUri() => throw new NotSupportedException();

        public override bool ServerSend(List<int> connectionIds, int channelId, ArraySegment<byte> segment) => ServerActive()  && server.SendAll(connectionIds, segment.Array, channelId);
        public override bool ServerDisconnect(int connectionId) => ServerActive() && server.Disconnect(connectionId);
        public override string ServerGetClientAddress(int connectionId) => ServerActive() ? server.ServerGetClientAddress(connectionId) : string.Empty;
        public override void ServerStop() => server?.Stop();

        public override void Shutdown()
        {
            client.Disconnect();
            server.Stop();
        }

        public override int GetMaxPacketSize(int channelId)
        {
            channelId = Math.Min(channelId, Channels.Length - 1);

            EP2PSend sendMethod = Channels[channelId];
            switch (sendMethod)
            {
                case EP2PSend.k_EP2PSendUnreliable:
                case EP2PSend.k_EP2PSendUnreliableNoDelay:
                    return 1200;
                case EP2PSend.k_EP2PSendReliable:
                case EP2PSend.k_EP2PSendReliableWithBuffering:
                    return 1048576;
                default:
                    throw new NotSupportedException();
            }
        }

        public override bool Available()
        {
            try
            {
                return SteamManager.Initialized;
            }
            catch
            {
                return false;
            }
        }
    }
}