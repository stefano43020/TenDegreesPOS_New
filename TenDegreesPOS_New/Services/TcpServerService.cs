using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TenDegreesPOS_New.Models;

namespace TenDegreesPOS_New.Services
{
    public class TcpServerService
    {
        private TcpListener? _listener;
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();
        private bool _isRunning;

        public void Start(int port = 8888)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;

            // Start listening in the background
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    try
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        _connectedClients.Add(client);
                        Console.WriteLine("Barista connected!");
                    }
                    catch { break; }
                }
            });
        }

        public void BroadcastOrder(NetworkOrder order)
        {
            if (!_isRunning) return;

            string json = JsonSerializer.Serialize(order);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n"); // Add newline for framing

            // Remove disconnected clients
            _connectedClients.RemoveAll(c => !c.Connected);

            foreach (var client in _connectedClients)
            {
                try
                {
                    var stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                catch { /* Ignore broken connections */ }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            foreach (var client in _connectedClients) client.Close();
        }
    }
}