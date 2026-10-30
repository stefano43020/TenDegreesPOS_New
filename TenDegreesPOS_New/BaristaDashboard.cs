using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TenDegreesPOS_New.Models;
using TenDegreesPOS_New.Services;

namespace TenDegreesPOS_New
{
    public partial class BaristaDashboard : Form
    {
        private TcpClientService _tcpClient = new TcpClientService();
        private DataGridView dgvOrders;
        private TextBox txtIp;
        private Button btnConnect;

        // Replace this with your BaseDashboard or custom UI setup
        public BaristaDashboard()
        {
            SetupUI();

            _tcpClient.OnOrderReceived += HandleIncomingOrder;
        }

        private void SetupUI()
        {
            this.Size = new Size(1000, 700);
            this.Text = "Barista Queue";
            this.BackColor = Color.FromArgb(25, 27, 31);

            Label lblIp = new Label { Text = "Cashier IP:", ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true };
            txtIp = new TextBox { Text = "127.0.0.1", Location = new Point(100, 17), Size = new Size(150, 25) };
            btnConnect = new Button { Text = "Connect", Location = new Point(260, 15), Size = new Size(100, 30), BackColor = Color.Green, ForeColor = Color.White };

            btnConnect.Click += async (s, e) =>
            {
                bool connected = await _tcpClient.ConnectAsync(txtIp.Text, 8888);
                if (connected)
                    MessageBox.Show("Connected to Cashier!");
                else
                    MessageBox.Show("Connection failed. Check IP and Firewall.");
            };

            dgvOrders = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(940, 580),
                BackgroundColor = Color.FromArgb(32, 35, 39),
                ForeColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            dgvOrders.Columns.Add("TxNumber", "Order #");
            dgvOrders.Columns.Add("Item", "Item");
            dgvOrders.Columns.Add("Size", "Size");
            dgvOrders.Columns.Add("Qty", "Qty");

            this.Controls.Add(lblIp);
            this.Controls.Add(txtIp);
            this.Controls.Add(btnConnect);
            this.Controls.Add(dgvOrders);
        }

        // This method triggers when the Cashier sends an order
        private void HandleIncomingOrder(NetworkOrder order)
        {
            // We must use Invoke because this is called from a background thread
            this.Invoke((MethodInvoker)delegate
            {
                foreach (var item in order.Items)
                {
                    dgvOrders.Rows.Add(order.TransactionNumber, item.ProductName, item.Size, item.Quantity);
                }

                // Optional: Play a sound to alert the barista
                System.Media.SystemSounds.Asterisk.Play();
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _tcpClient.Disconnect();
            base.OnFormClosing(e);
        }
    }
}