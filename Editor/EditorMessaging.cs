using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    /* Editor:
     * 
     * Starts listening for UDP messages (acts as UDP Server)
     * When it gets one it connects to the IP over TCP (acts as TCP Client)
     * 
     * Device:
     * 
     * Starts broadcasting UDP messages (acts as UDP client)
     * Also listeners for TCP connections (acts as TCP server)
     * 
     */
    public static class EditorMessaging
    {
        static Action<byte[]> responseCallback;

        // ip address to display name mappings
        static readonly Dictionary<string, string> knownEndpoints = new Dictionary<string, string>();


        static TcpClient pendingClient;
        private static UdpClient receivingUdpClient;

        static int bytesReadSoFar; // For large responses that take more than one frame's read
        static byte[] responseBuffer = new byte[0]; // Large responses are filled in over multiple frames

        // Return a copy
        public static Dictionary<string, string> KnownEndpoints => new Dictionary<string, string>(knownEndpoints);

        public static string ConnectedIP
        {
            get
            {
                string activeEndpoint = null;
                foreach (var pair in knownEndpoints)
                {
                    activeEndpoint = pair.Key;
                }

                return activeEndpoint;
            }
        }

        public static bool IsConnected => !string.IsNullOrEmpty(ConnectedIP);

        public static bool HasValidConnection
        {
            get
            {
                bool validConnection = (EditorMessaging.KnownEndpoints.Count >= 1);

#if REMOTEACTIONS_DEBUG
                validConnection |= BridgingContext.Instance.container.NetworkSettings.LocalDevMode;
#endif
                return validConnection;
            }
        }

        public static bool Started => receivingUdpClient != null;

        public static void Start()
        {
            StartReceivingBroadcasts();
        }

        public static void RegisterForResponses(Action<byte[]> callback)
        {
            responseCallback -= callback;
            responseCallback += callback;
        }

        private static void StartReceivingBroadcasts()
        {
            // Receive a message and write it to the console.
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PlayerMessaging.BROADCAST_PORT);
            UdpClient udpClient = new UdpClient(endPoint);
            udpClient.Client.SendTimeout = 5000;
            udpClient.Client.ReceiveTimeout = 5000;

#if REMOTEACTIONS_DEBUG
            Debug.Log("Listening for player broadcasts");
#endif
            
            UdpState udpState = new UdpState(udpClient, endPoint);
            udpClient.BeginReceive(ReceivedBroadcastFromPlayer, udpState);
            receivingUdpClient = udpClient;
        }

        /// <summary>
        /// Fired when a player's UDP broadcast reaches us
        /// </summary>
        private static void ReceivedBroadcastFromPlayer(IAsyncResult ar)
        {
            UdpState udpState = (UdpState) ar.AsyncState;
            UdpClient udpClient = udpState.udpClient;
            IPEndPoint endPoint = udpState.endPoint;

            byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);
            string receivedString = Encoding.UTF8.GetString(receivedBytes);

            if (!knownEndpoints.ContainsKey(endPoint.Address.ToString()))
            {
                knownEndpoints.Add(endPoint.Address.ToString(), receivedString);
#if REMOTEACTIONS_DEBUG
                Debug.Log($"New EndPoint: {receivedString} from {endPoint}");
#endif
            }

            udpClient.BeginReceive(ReceivedBroadcastFromPlayer, ((UdpState) (ar.AsyncState)));
        }

        public static void SendRequest(byte[] sendingBuffer)
        {
            SendRequest(ConnectedIP, sendingBuffer);
        }

        private static void SendRequest(string targetIP, byte[] sendingBuffer)
        {
            try
            {
                TcpClient client;
                if (pendingClient != null && pendingClient.Connected)
                {
                    client = pendingClient;
                }
                else
                {
                    client = new TcpClient();

                    client.Client.SendTimeout = 5000;
                    client.Client.ReceiveTimeout = 5000;
#if REMOTEACTIONS_DEBUG
                    Debug.Log("Connecting...");
#endif
                    client.Connect(targetIP, PlayerMessaging.REQUEST_PORT);


#if REMOTEACTIONS_DEBUG
                    Debug.Log("Connected to player");
#endif
                }

                NetworkStream stream = client.GetStream();

#if REMOTEACTIONS_DEBUG
                Debug.Log("Sending request to Player");
#endif

                stream.Write(sendingBuffer, 0, sendingBuffer.Length);

                // Player should respond so wait until it does
                pendingClient = client;
            }
            catch (SocketException)
            {
                pendingClient = null;
                if (knownEndpoints.ContainsKey(targetIP))
                {
                    knownEndpoints.Remove(targetIP);
                }
            }
        }

        public static void Tick()
        {
            if (pendingClient != null && pendingClient.Connected)
            {
                var stream = pendingClient.GetStream();
                if (stream.DataAvailable)
                {
                    while (stream.DataAvailable)
                    {
                        byte[] networkResponseBuffer = new byte[10000000];
                        var count = stream.Read(networkResponseBuffer, 0, networkResponseBuffer.Length);

                        if (responseBuffer.Length == 0 || bytesReadSoFar == responseBuffer.Length)
                        {
                            // New response has started, responses start with their full size so first of all create a staging buffer
                            int totalCount = BitConverter.ToInt32(networkResponseBuffer, 0);
                            responseBuffer = new byte[totalCount];
                            bytesReadSoFar = 0;
                        }

                        // Copy in the bytes we've read this frame
                        Array.Copy(networkResponseBuffer, 0, responseBuffer, bytesReadSoFar, count);

#if REMOTEACTIONS_DEBUG
                        Debug.Log($"Response received in editor, length is {count}");
#endif
                        bytesReadSoFar += count;

                        if (bytesReadSoFar >= responseBuffer.Length) // Have we read all the bytes we expected to?
                        {
                            // Response buffer is full, fire the callback
                            if (responseCallback != null)
                            {
                                responseCallback(responseBuffer);
                            }

                            // Reset the buffer for the next response
                            bytesReadSoFar = 0;
                            responseBuffer = new byte[0];
                        }
                    }
                }
            }
        }
    }
}