using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TenDegreesPOS_New.Models;

namespace TenDegreesPOS_New.Services
{
    public class TcpClientService
    {
        private TcpClient? _client;
        private StreamReader? _reader;
        public event Action<NetworkOrder>? OnOrderReceived;


        public async Task<bool> ConnectAsync(string serverIp, int port = 8888)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(serverIp, port);

                var stream = _client.GetStream();
                _reader = new StreamReader(stream, Encoding.UTF8);

                // Start listening in the background
                _ = Task.Run(ListenForOrdersAsync);
                return true;
            }
            catch (Exception ex)
            {
                // Temporarily pop a message box so we can see the real error
                MessageBox.Show($"TCP Error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_client != null && _client.Connected)
            {
                try
                {
                    var stream = _client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    await stream.WriteAsync(data, 0, data.Length);
                }
                catch {  }
            }
        }
        private async Task ListenForOrdersAsync()
        {
            while (_client != null && _client.Connected)
            {
                try
                {
                    string? line = await _reader!.ReadLineAsync();
                    if (line == null) break; // Disconnected

                    var order = JsonSerializer.Deserialize<NetworkOrder>(line);
                    if (order != null)
                    {
                        OnOrderReceived?.Invoke(order);
                    }
                }
                catch { break; }
            }
        }

        public void Disconnect()
        {
            _client?.Close();
        }
    }
}