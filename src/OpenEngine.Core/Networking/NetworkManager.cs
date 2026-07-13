// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OpenEngine.Core.Networking
{
    public enum NetworkRole { Host, Client, Server }
    public enum ConnectionState { Disconnected, Connecting, Connected }

    public class NetworkManager
    {
        public NetworkRole Role { get; set; }
        public ConnectionState State { get; set; }
        public int MaxConnections { get; set; }
        public int Port { get; set; }
        public string HostAddress { get; set; }
        public float NetworkTickRate { get; set; }
        public bool UseLobbyService { get; set; }

        private UdpClient _udpClient;
        private TcpListener _tcpListener;
        private Dictionary<int, NetworkConnection> _connections;
        private int _nextConnectionId;

        public event Action<int> OnConnected;
        public event Action<int> OnDisconnected;
        public event Action<NetworkMessage> OnMessageReceived;

        public NetworkManager()
        {
            Role = NetworkRole.Host;
            State = ConnectionState.Disconnected;
            MaxConnections = 8;
            Port = 7777;
            HostAddress = "127.0.0.1";
            NetworkTickRate = 30f;
            UseLobbyService = false;
            _connections = new Dictionary<int, NetworkConnection>();
            _nextConnectionId = 1;
        }

        public async Task<bool> StartHost()
        {
            Role = NetworkRole.Host;
            return await StartServer();
        }

        public async Task<bool> StartServer()
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, Port);
                _tcpListener.Start();
                State = ConnectionState.Connected;

                _ = AcceptConnectionsAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start server: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ConnectToHost(string address, int port)
        {
            Role = NetworkRole.Client;
            HostAddress = address;
            Port = port;
            State = ConnectionState.Connecting;

            try
            {
                _udpClient = new UdpClient();
                _udpClient.Connect(address, port);
                State = ConnectionState.Connected;

                var connectionId = _nextConnectionId++;
                _connections[connectionId] = new NetworkConnection(connectionId, address, port);
                OnConnected?.Invoke(connectionId);

                _ = ReceiveMessagesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
                State = ConnectionState.Disconnected;
                return false;
            }
        }

        private async Task AcceptConnectionsAsync()
        {
            while (State == ConnectionState.Connected)
            {
                var client = await _tcpListener.AcceptTcpClientAsync();
                var connectionId = _nextConnectionId++;
                var endPoint = client.Client.RemoteEndPoint as IPEndPoint;
                
                _connections[connectionId] = new NetworkConnection(connectionId, endPoint.Address.ToString(), endPoint.Port);
                OnConnected?.Invoke(connectionId);

                _ = HandleClientAsync(client, connectionId);
            }
        }

        private async Task HandleClientAsync(TcpClient client, int connectionId)
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];

            while (client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Disconnect(connectionId);
                    break;
                }

                var message = NetworkMessage.Deserialize(buffer, bytesRead);
                OnMessageReceived?.Invoke(message);
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            while (State == ConnectionState.Connected)
            {
                var result = await _udpClient.ReceiveAsync();
                var message = NetworkMessage.Deserialize(result.Buffer, result.Buffer.Length);
                OnMessageReceived?.Invoke(message);
            }
        }

        public void SendToAll(NetworkMessage message)
        {
            var data = message.Serialize();
            foreach (var connection in _connections.Values)
            {
                SendMessage(connection.ConnectionId, data);
            }
        }

        public void SendToClient(int connectionId, NetworkMessage message)
        {
            var data = message.Serialize();
            SendMessage(connectionId, data);
        }

        private void SendMessage(int connectionId, byte[] data)
        {
            if (_connections.ContainsKey(connectionId))
            {
                // Send via appropriate transport
            }
        }

        public void Disconnect(int connectionId)
        {
            if (_connections.ContainsKey(connectionId))
            {
                _connections.Remove(connectionId);
                OnDisconnected?.Invoke(connectionId);
            }
        }

        public void DisconnectAll()
        {
            foreach (var connectionId in new List<int>(_connections.Keys))
            {
                Disconnect(connectionId);
            }
            Stop();
        }

        public void Stop()
        {
            State = ConnectionState.Disconnected;
            _tcpListener?.Stop();
            _udpClient?.Close();
            _connections.Clear();
        }

        public void Update(float deltaTime)
        {
            // Process network messages
        }
    }

    public class NetworkConnection
    {
        public int ConnectionId { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public float Latency { get; set; }
        public DateTime LastMessageTime { get; set; }

        public NetworkConnection(int id, string address, int port)
        {
            ConnectionId = id;
            Address = address;
            Port = port;
            Latency = 0f;
            LastMessageTime = DateTime.UtcNow;
        }
    }

    public class NetworkMessage
    {
        public int MessageType { get; set; }
        public byte[] Data { get; set; }
        public int SenderId { get; set; }

        public NetworkMessage(int messageType, byte[] data, int senderId = 0)
        {
            MessageType = messageType;
            Data = data;
            SenderId = senderId;
        }

        public byte[] Serialize()
        {
            var result = new byte[12 + Data.Length];
            BitConverter.GetBytes(MessageType).CopyTo(result, 0);
            BitConverter.GetBytes(Data.Length).CopyTo(result, 4);
            BitConverter.GetBytes(SenderId).CopyTo(result, 8);
            Data.CopyTo(result, 12);
            return result;
        }

        public static NetworkMessage Deserialize(byte[] data, int length)
        {
            var messageType = BitConverter.ToInt32(data, 0);
            var dataLength = BitConverter.ToInt32(data, 4);
            var senderId = BitConverter.ToInt32(data, 8);
            var payload = new byte[dataLength];
            Array.Copy(data, 12, payload, 0, dataLength);
            return new NetworkMessage(messageType, payload, senderId);
        }
    }
}
