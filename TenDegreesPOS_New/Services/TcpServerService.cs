using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        // ▼▼▼ ADD THIS ▼▼▼
        private DatabaseService _dbService = new DatabaseService();
        // ▲▲▲ END OF ADDITION ▲▲▲

        public event Action<string>? OnMessageReceived;

        public void Start(int port = 8888)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;

            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    try
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        _connectedClients.Add(client);

                        // ▼▼▼ SEND TODAY'S DATA IMMEDIATELY UPON CONNECT ▼▼▼
                        _ = Task.Run(() => SendTodaySyncAsync(client));
                        // ▲▲▲ END OF ADDITION ▲▲▲

                        _ = Task.Run(() => ListenToClientAsync(client));
                    }
                    catch { break; }
                }
            });
        }

        // ▼▼▼ ADD THIS METHOD ▼▼▼
        private async Task SendTodaySyncAsync(TcpClient client)
        {
            try
            {
                // Only send transactions that are "Completed" (paid) but NOT yet "Fulfilled" (made by Barista)
                var todayTx = _dbService.GetAllTransactions()
                    .Where(t => t.TransactionDate.Date == DateTime.Today && t.Status == "Completed")
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();

                foreach (var tx in todayTx)
                {
                    var netOrder = new NetworkOrder
                    {
                        TransactionNumber = tx.TransactionNumber,
                        MessageType = "NEW_ORDER",
                        Items = tx.TransactionItems.Select(ti => new NetworkOrderItem
                        {
                            ProductName = ti.Product?.Name ?? "Unknown",
                            Size = ti.Size ?? "",
                            Quantity = ti.Quantity
                        }).ToList()
                    };

                    string json = JsonSerializer.Serialize(netOrder);
                    byte[] data = Encoding.UTF8.GetBytes(json + "\n");

                    var stream = client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                    await Task.Delay(50);
                }
            }
            catch { }
        }
        // ▲▲▲ END OF ADDITION ▲▲▲

        private async Task ListenToClientAsync(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);
                while (client.Connected)
                {
                    string? line = await reader.ReadLineAsync();
                    if (line == null) break;
                    OnMessageReceived?.Invoke(line);
                }
            }
            catch { }
        }

        public void BroadcastOrder(NetworkOrder order)
        {
            if (!_isRunning) return;

            string json = JsonSerializer.Serialize(order);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");

            _connectedClients.RemoveAll(c => !c.Connected);

            foreach (var client in _connectedClients)
            {
                try
                {
                    var stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                catch { }
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