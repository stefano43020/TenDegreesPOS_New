using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TenDegreesPOS_New.Models;
using TenDegreesPOS_New.Services;

namespace TenDegreesPOS_New
{
    public partial class BaristaDashboard : Form
    {
        private TcpClientService _tcpClient = new TcpClientService();

        // Split the queues
        private FlowLayoutPanel flpWaitingOrders;
        private FlowLayoutPanel flpPreparingOrders;

        private Label lblCompletedCount;
        private Label lblOngoingCount;
        private TextBox txtIp;
        private Button btnConnect;

        private Panel pnlContentMain;
        private Panel pnlInventory;

        private int completedCount = 0;
        private int ongoingCount = 0;

        // DeepSeek Theme Colors
        private readonly Color DeepSeekBg = Color.FromArgb(25, 27, 31);
        private readonly Color DeepSeekSurface = Color.FromArgb(32, 35, 39);
        private readonly Color DeepSeekSidebar = Color.FromArgb(18, 20, 23);
        private readonly Color DeepSeekAccent = Color.FromArgb(76, 130, 250);
        private readonly Color DeepSeekText = Color.FromArgb(236, 237, 238);
        private readonly Color DeepSeekTextMuted = Color.FromArgb(148, 155, 164);
        private readonly Color DeepSeekBorder = Color.FromArgb(47, 51, 58);
        private readonly Color DeepSeekSuccess = Color.FromArgb(16, 185, 129);
        private readonly Color DeepSeekWarning = Color.FromArgb(245, 158, 11);
        private readonly Color DeepSeekDanger = Color.FromArgb(239, 68, 68);

        public BaristaDashboard()
        {
            this.Size = new Size(1800, 1000);
            this.Text = "Barista Dashboard";
            this.BackColor = DeepSeekBg;
            this.WindowState = FormWindowState.Maximized;

            SetupLayout();
            _tcpClient.OnOrderReceived += HandleIncomingOrder;
        }

        private void SetupLayout()
        {
            // --- SIDEBAR (Unchanged) ---
            Panel pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = DeepSeekSidebar
            };

            Panel pnlProfile = new Panel { Dock = DockStyle.Top, Height = 100 };
            Label lblName = new Label { Text = "Barista", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(20, 35), AutoSize = true };
            Label lblRole = new Label { Text = "Ten Degrees Management", Font = new Font("Segoe UI", 9), ForeColor = DeepSeekTextMuted, Location = new Point(20, 60), AutoSize = true };
            pnlProfile.Controls.AddRange(new Control[] { lblName, lblRole });

            Button btnDashboard = new Button
            {
                Text = "   Dashboard",
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekText,
                BackColor = DeepSeekSidebar,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            btnDashboard.Click += (s, e) => { pnlContentMain.Visible = true; pnlInventory.Visible = false; };

            Button btnInventory = new Button
            {
                Text = "   Inventory",
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekText,
                BackColor = DeepSeekSidebar,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            btnInventory.Click += (s, e) => { pnlContentMain.Visible = false; pnlInventory.Visible = true; };

            Panel pnlConnection = new Panel { Dock = DockStyle.Bottom, Height = 120, Padding = new Padding(20) };
            txtIp = new TextBox { Text = "127.0.0.1", Location = new Point(20, 20), Width = 200, BackColor = DeepSeekSurface, ForeColor = DeepSeekText, BorderStyle = BorderStyle.FixedSingle };
            btnConnect = new Button
            {
                Text = "Connect to Cashier",
                Location = new Point(20, 55),
                Width = 200,
                Height = 35,
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            btnConnect.Click += async (s, e) =>
            {
                btnConnect.Text = "Connecting...";
                btnConnect.Enabled = false;
                bool connected = await _tcpClient.ConnectAsync(txtIp.Text, 8888);
                if (connected) { btnConnect.Text = "Connected!"; btnConnect.BackColor = DeepSeekSuccess; }
                else
                {
                    MessageBox.Show("Connection failed. Check IP and Firewall.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnConnect.Text = "Connect to Cashier"; btnConnect.BackColor = DeepSeekAccent; btnConnect.Enabled = true;
                }
            };
            pnlConnection.Controls.AddRange(new Control[] { txtIp, btnConnect });

            pnlSidebar.Controls.Add(pnlConnection);
            pnlSidebar.Controls.Add(btnInventory);
            pnlSidebar.Controls.Add(btnDashboard);
            pnlSidebar.Controls.Add(pnlProfile);

            // --- MAIN CONTENT ---
            Panel pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = DeepSeekBg, Padding = new Padding(40) };

            Label lblTitle = new Label { Text = "Live Orders", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(40, 20), AutoSize = true };

            // Stats Cards
            Panel pnlStats = new Panel
            {
                Location = new Point(40, 80),
                Size = new Size(1000, 80),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlStats.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = RoundedRect(pnlStats.ClientRectangle, 8))
                using (var pen = new Pen(DeepSeekBorder, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };
            lblCompletedCount = new Label { Text = "Completed: 0", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekSuccess, Location = new Point(30, 25), AutoSize = true };
            lblOngoingCount = new Label { Text = "In Queue: 0", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekWarning, Location = new Point(350, 25), AutoSize = true };
            pnlStats.Controls.AddRange(new Control[] { lblCompletedCount, lblOngoingCount });

            // Left/Right Split Layout
            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Location = new Point(40, 180),
                Size = new Size(1000, 700),
                ColumnCount = 2,
                RowCount = 1,
                BackColor = DeepSeekBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F)); // Left area (Active Orders)
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Right area (Completed)

            // ===== LEFT COLUMN: SPLIT WAITING & PREPARING =====
            TableLayoutPanel tlpLeft = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = DeepSeekBg,
                Margin = new Padding(0, 0, 15, 0)
            };
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // WAITING ZONE
            Panel pnlWaitingContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(34, 30, 24), // Dark Amber
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 0, 8)
            };
            pnlWaitingContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = RoundedRect(pnlWaitingContainer.ClientRectangle, 10))
                using (var pen = new Pen(Color.FromArgb(80, DeepSeekWarning), 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };

            Label lblWaitingTitle = new Label { Text = "⏳ Waiting to be Prepared", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = DeepSeekWarning, Dock = DockStyle.Top, Height = 40, Padding = new Padding(15, 12, 0, 0) };
            flpWaitingOrders = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Padding = new Padding(10) };

            pnlWaitingContainer.Controls.Add(flpWaitingOrders);
            pnlWaitingContainer.Controls.Add(lblWaitingTitle);

            // PREPARING ZONE
            Panel pnlPreparingContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 30, 42), // Dark Blue
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 8, 0, 0)
            };
            pnlPreparingContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = RoundedRect(pnlPreparingContainer.ClientRectangle, 10))
                using (var pen = new Pen(Color.FromArgb(80, DeepSeekAccent), 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };

            Label lblPreparingTitle = new Label { Text = "🔥 Currently Preparing", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = DeepSeekAccent, Dock = DockStyle.Top, Height = 40, Padding = new Padding(15, 12, 0, 0) };
            flpPreparingOrders = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Padding = new Padding(10) };

            pnlPreparingContainer.Controls.Add(flpPreparingOrders);
            pnlPreparingContainer.Controls.Add(lblPreparingTitle);

            tlpLeft.Controls.Add(pnlWaitingContainer, 0, 0);
            tlpLeft.Controls.Add(pnlPreparingContainer, 0, 1);

            // ===== RIGHT COLUMN: COMPLETED =====
            Panel pnlCompletedContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0)
            };
            pnlCompletedContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = RoundedRect(pnlCompletedContainer.ClientRectangle, 10))
                using (var pen = new Pen(DeepSeekBorder, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };

            Label lblCompletedTitle = new Label { Text = "✅ Completed Orders", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = DeepSeekTextMuted, Dock = DockStyle.Top, Height = 40, Padding = new Padding(15, 12, 0, 0) };
            FlowLayoutPanel flpCompletedOrders = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent, FlowDirection = FlowDirection.TopDown, WrapContents = false, Name = "flpCompletedOrders", Padding = new Padding(10) };

            pnlCompletedContainer.Controls.Add(flpCompletedOrders);
            pnlCompletedContainer.Controls.Add(lblCompletedTitle);

            tlpMain.Controls.Add(tlpLeft, 0, 0);
            tlpMain.Controls.Add(pnlCompletedContainer, 1, 0);

            pnlContent.Controls.AddRange(new Control[] { lblTitle, pnlStats, tlpMain });

            pnlContent.Resize += (s, e) =>
            {
                pnlStats.Width = pnlContent.Width - 80;
                tlpMain.Width = pnlContent.Width - 80;
                tlpMain.Height = pnlContent.Height - 220;
            };

            // --- INVENTORY PANEL (Unchanged) ---
            pnlContentMain = pnlContent;
            pnlInventory = new Panel { Dock = DockStyle.Fill, BackColor = DeepSeekBg, Visible = false };

            Label lblInvTitle = new Label { Text = "Inventory", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(30, 20), AutoSize = true };
            Label lblSearch = new Label { Text = "Search:", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = DeepSeekTextMuted, Location = new Point(30, 80), AutoSize = true };
            TextBox txtSearchInventory = new TextBox { Location = new Point(100, 77), Size = new Size(300, 35), Font = new Font("Segoe UI", 11), PlaceholderText = "Search ingredients...", BackColor = DeepSeekSurface, ForeColor = DeepSeekText, BorderStyle = BorderStyle.FixedSingle };
            DataGridView dgvInventory = new DataGridView { Location = new Point(30, 130), Size = new Size(1000, 700), BackgroundColor = DeepSeekBg, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, Font = new Font("Segoe UI", 10), GridColor = DeepSeekBorder, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            dgvInventory.EnableHeadersVisualStyles = false;
            dgvInventory.ColumnHeadersDefaultCellStyle.BackColor = DeepSeekSidebar;
            dgvInventory.ColumnHeadersDefaultCellStyle.ForeColor = DeepSeekText;
            dgvInventory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvInventory.ColumnHeadersHeight = 40;
            dgvInventory.RowsDefaultCellStyle.BackColor = DeepSeekSurface;
            dgvInventory.RowsDefaultCellStyle.ForeColor = DeepSeekText;
            dgvInventory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(38, 41, 46);
            dgvInventory.RowsDefaultCellStyle.SelectionBackColor = DeepSeekAccent;
            dgvInventory.Columns.Add("Item", "Item");
            dgvInventory.Columns.Add("Type", "Type");
            dgvInventory.Columns.Add("Quantity", "Quantity");
            dgvInventory.Columns.Add("Unit", "Unit");
            dgvInventory.Columns.Add("MinStock", "Min Stock");
            dgvInventory.Columns.Add("Status", "Status");

            Action loadInventory = () =>
            {
                dgvInventory.Rows.Clear();
                var invService = new DatabaseService();
                var inventory = invService.GetAllInventory();
                string searchText = txtSearchInventory.Text.ToLower();

                foreach (var item in inventory)
                {
                    if (item.Ingredient == null) continue;
                    string itemName = item.Ingredient.Name;
                    if (!string.IsNullOrEmpty(searchText) && !itemName.ToLower().Contains(searchText)) continue;

                    string status = item.Quantity <= 0 ? "Out of Stock" : item.Quantity < item.Ingredient.MinimumStock ? "Low Stock" : "In Stock";
                    int rIdx = dgvInventory.Rows.Add(itemName, "Ingredient", $"{item.Quantity:F1}", item.Unit, item.Ingredient.MinimumStock, status);

                    if (item.Quantity <= 0) dgvInventory.Rows[rIdx].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                    else if (item.Quantity < item.Ingredient.MinimumStock) dgvInventory.Rows[rIdx].Cells["Status"].Style.ForeColor = DeepSeekWarning;
                    else dgvInventory.Rows[rIdx].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                }
            };
            txtSearchInventory.TextChanged += (s, e) => loadInventory();
            loadInventory();
            pnlInventory.Controls.AddRange(new Control[] { lblInvTitle, lblSearch, txtSearchInventory, dgvInventory });
            pnlInventory.Resize += (s, e) => { dgvInventory.Width = pnlInventory.Width - 60; dgvInventory.Height = pnlInventory.Height - 180; };

            this.Controls.Add(pnlInventory);
            this.Controls.Add(pnlContentMain);
            this.Controls.Add(pnlSidebar);
        }

        // Helper to draw rounded rectangles for the panels
        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            if (radius == 0) { path.AddRectangle(bounds); return path; }

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void HandleIncomingOrder(NetworkOrder order)
        {
            this.Invoke((MethodInvoker)delegate
            {
                // Check if this order is already on the screen to avoid duplicates
                foreach (Control c in flpWaitingOrders.Controls)
                {
                    if (c is Panel p && p.Tag is NetworkOrder existingOrder && existingOrder.TransactionNumber == order.TransactionNumber)
                        return;
                }
                foreach (Control c in flpPreparingOrders.Controls)
                {
                    if (c is Panel p && p.Tag is NetworkOrder existingOrder && existingOrder.TransactionNumber == order.TransactionNumber)
                        return;
                }

                Panel card = CreateOrderCard(order, false);
                flpWaitingOrders.Controls.Add(card);

                SortWaitingOrders(); // Keep FIFO order

                ongoingCount++;
                UpdateStats();

                // Only play sound if it's a real-time order, not an initial sync
                if (flpWaitingOrders.Controls.Count > 1)
                    System.Media.SystemSounds.Asterisk.Play();
            });
        }

        private void SortWaitingOrders()
        {
            // Sort the waiting panel by transaction number (which correlates to time)
            var panels = flpWaitingOrders.Controls.OfType<Panel>().ToList();
            panels.Sort((a, b) => string.Compare(((NetworkOrder)a.Tag).TransactionNumber, ((NetworkOrder)b.Tag).TransactionNumber, StringComparison.Ordinal));

            flpWaitingOrders.Controls.Clear();
            flpWaitingOrders.Controls.AddRange(panels.ToArray());
        }

        private Panel CreateOrderCard(NetworkOrder order, bool isCompleted)
        {
            // Extract the daily number
            string dailyNumber = "0";
            if (order.TransactionNumber.Length >= 4)
            {
                string suffix = order.TransactionNumber.Substring(order.TransactionNumber.Length - 4);
                if (int.TryParse(suffix, out int parsedNum))
                {
                    dailyNumber = parsedNum.ToString();
                }
            }

            Panel card = new Panel
            {
                Size = isCompleted ? new Size(300, 70) : new Size(300, 220),
                BackColor = isCompleted ? Color.FromArgb(28, 30, 34) : DeepSeekSurface,
                Margin = new Padding(10),
                Tag = order
            };

            card.Paint += (s, e) =>
            {
                Color borderColor = isCompleted ? DeepSeekBorder : DeepSeekWarning;
                using (Pen p = new Pen(borderColor, isCompleted ? 1 : 3))
                {
                    if (!isCompleted) e.Graphics.DrawLine(p, 0, 0, card.Width, 0);
                }
                using (Pen p = new Pen(DeepSeekBorder))
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };

            if (isCompleted)
            {
                Label lblOrderNo = new Label { Text = $"#{dailyNumber}", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = DeepSeekTextMuted, Location = new Point(15, 15), AutoSize = true };
                Label lblStatus = new Label { Text = "COMPLETED", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = DeepSeekSuccess, Location = new Point(200, 17), Size = new Size(90, 18), TextAlign = ContentAlignment.MiddleCenter };
                Label lblItems = new Label { Text = string.Join(", ", order.Items.ConvertAll(i => $"{i.Quantity}x {i.ProductName}")), Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 100, 100), Location = new Point(15, 40), Size = new Size(270, 20), AutoEllipsis = true };
                card.Controls.AddRange(new Control[] { lblOrderNo, lblStatus, lblItems });
            }
            else
            {
                Label lblOrderNo = new Label { Text = $"#{dailyNumber}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(15, 15), AutoSize = true };

                Label lblStatus = new Label { Name = "lblStatus", Text = "WAITING", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = DeepSeekWarning, BackColor = Color.FromArgb(60, 45, 20), Location = new Point(180, 18), Size = new Size(100, 22), TextAlign = ContentAlignment.MiddleCenter };
                Label lblItems = new Label { Text = string.Join("\n", order.Items.ConvertAll(i => $"• {i.Quantity}x {i.ProductName} {i.Size}")), Font = new Font("Segoe UI", 11), ForeColor = DeepSeekTextMuted, Location = new Point(15, 55), Size = new Size(270, 100), AutoEllipsis = true };

                Button btnAction = new Button
                {
                    Text = "Start Preparing",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.Black,
                    BackColor = DeepSeekWarning,
                    Location = new Point(15, 165),
                    Size = new Size(270, 40),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    Cursor = Cursors.Hand
                };

                btnAction.Click += async (s, e) =>
                {
                    if (lblStatus.Text == "WAITING")
                    {
                        lblStatus.Text = "PREPARING";
                        lblStatus.ForeColor = DeepSeekAccent;
                        lblStatus.BackColor = Color.FromArgb(20, 35, 70);
                        btnAction.Text = "Mark as Complete";
                        btnAction.BackColor = DeepSeekSuccess;
                        card.Invalidate();

                        // Move to preparing panel
                        flpWaitingOrders.Controls.Remove(card);
                        flpPreparingOrders.Controls.Add(card);

                        // Send message to Cashier
                        var prepMsg = new NetworkOrder
                        {
                            TransactionNumber = order.TransactionNumber,
                            MessageType = "ORDER_PREPARING"
                        };
                        string json = System.Text.Json.JsonSerializer.Serialize(prepMsg);
                        await _tcpClient.SendMessageAsync(json);
                    }
                    else if (lblStatus.Text == "PREPARING")
                    {
                        // 1. Move to the completed column visually
                        MoveToCompleted(card, order);

                        // 2. Send the completion signal to the Cashier (Server)
                        var completionMsg = new NetworkOrder
                        {
                            TransactionNumber = order.TransactionNumber,
                            MessageType = "ORDER_COMPLETED"
                        };
                        string json = System.Text.Json.JsonSerializer.Serialize(completionMsg);
                        await _tcpClient.SendMessageAsync(json);
                    }
                };

                card.Controls.AddRange(new Control[] { lblOrderNo, lblStatus, lblItems, btnAction });
            }

            return card;
        }

        private void MoveToCompleted(Panel activeCard, NetworkOrder order)
        {
            // Remove from the Preparing panel
            flpPreparingOrders.Controls.Remove(activeCard);

            Panel completedCard = CreateOrderCard(order, true);

            // Find the completed panel dynamically since it's created inside SetupLayout
            var flpCompleted = this.Controls.Find("flpCompletedOrders", true).FirstOrDefault() as FlowLayoutPanel;
            if (flpCompleted != null)
            {
                flpCompleted.Controls.Add(completedCard);
                flpCompleted.ScrollControlIntoView(completedCard);
            }

            completedCount++;
            ongoingCount--;
            UpdateStats();
        }

        private void UpdateStats()
        {
            lblCompletedCount.Text = $"Completed: {completedCount}";
            lblOngoingCount.Text = $"In Queue: {ongoingCount}";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _tcpClient.Disconnect();
            base.OnFormClosing(e);
        }
    }
}