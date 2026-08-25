using System;
using System.Drawing;
using System.Windows.Forms;
using TenDegreesPOS_New.Services;
using TenDegreesPOS_New.Models;

namespace TenDegreesPOS_New
{
    public partial class BaseDashboard : Form
    {
        protected string userName = string.Empty;
        protected string userRole = string.Empty;
        protected int userId;

        protected DatabaseService dbService;

        protected Panel? pnlSidebar;
        protected Panel? pnlTopBar;
        protected Panel? pnlContent;
        protected Label? lblWelcome;
        protected Label? lblRole;
        protected Button? btnLogout;
        protected Button? btnMinimize;
        protected Button? btnMaximize;
        protected Button? btnClose;

        // Dark theme colors
        private readonly Color DarkPrimary = Color.FromArgb(32, 33, 36);
        private readonly Color DarkSecondary = Color.FromArgb(41, 42, 45);
        private readonly Color DarkAccent = Color.FromArgb(114, 137, 218);
        private readonly Color DarkText = Color.FromArgb(225, 225, 225);
        private readonly Color DarkTextSecondary = Color.FromArgb(150, 150, 150);
        private readonly Color DarkBorder = Color.FromArgb(60, 60, 60);
        private readonly Color DarkHover = Color.FromArgb(60, 60, 70);
        private readonly Color DarkSuccess = Color.FromArgb(66, 184, 131);
        private readonly Color DarkWarning = Color.FromArgb(240, 173, 78);
        private readonly Color DarkDanger = Color.FromArgb(217, 83, 79);
        private readonly Color DarkInfo = Color.FromArgb(91, 192, 222);

        // DeepSeek theme colors
        private readonly Color DeepSeekBg = Color.FromArgb(25, 27, 31);
        private readonly Color DeepSeekSurface = Color.FromArgb(32, 35, 39);
        private readonly Color DeepSeekSidebar = Color.FromArgb(18, 20, 23);
        private readonly Color DeepSeekAccent = Color.FromArgb(76, 130, 250);
        private readonly Color DeepSeekAccentHover = Color.FromArgb(96, 150, 255);
        private readonly Color DeepSeekText = Color.FromArgb(236, 237, 238);
        private readonly Color DeepSeekTextMuted = Color.FromArgb(148, 155, 164);
        private readonly Color DeepSeekBorder = Color.FromArgb(47, 51, 58);
        private readonly Color DeepSeekHover = Color.FromArgb(42, 46, 52);
        private readonly Color DeepSeekSuccess = Color.FromArgb(16, 185, 129);
        private readonly Color DeepSeekWarning = Color.FromArgb(245, 158, 11);
        private readonly Color DeepSeekDanger = Color.FromArgb(239, 68, 68);
        private readonly Color DeepSeekInfo = Color.FromArgb(59, 130, 246);

        public BaseDashboard()
        {
            dbService = new DatabaseService();
            InitializeComponent();
            SetupBaseForm();

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void InitializeComponent()
        {
            // Top Bar - Keep height the same
            pnlTopBar = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = DeepSeekSurface
            };

            // Add subtle bottom border to top bar
            pnlTopBar.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawLine(pen, 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);
                }
            };

            // Sidebar - REDUCED WIDTH from 250 to 200 (20% thinner)
            pnlSidebar = new Panel
            {
                Width = 200,  // Changed from 250 to 200
                Dock = DockStyle.Left,
                BackColor = DeepSeekSidebar,
                BorderStyle = BorderStyle.None
            };

            // Add subtle right border to sidebar
            pnlSidebar.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawLine(pen, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
                }
            };

            // Content Area
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekBg,
                AutoScroll = true
            };

            // Welcome Label
            lblWelcome = new Label
            {
                Text = "Welcome",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(20, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Role Label
            lblRole = new Label
            {
                Text = "Role",
                Font = new Font("Segoe UI", 10),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(120, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Minimize Button
            btnMinimize = new Button
            {
                Text = "—",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatAppearance = { BorderSize = 0 }
            };
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = DeepSeekHover;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Maximize/Restore Button
            btnMaximize = new Button
            {
                Text = "□",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatAppearance = { BorderSize = 0 }
            };
            btnMaximize.MouseEnter += (s, e) => btnMaximize.BackColor = DeepSeekHover;
            btnMaximize.MouseLeave += (s, e) => btnMaximize.BackColor = Color.Transparent;
            btnMaximize.Click += BtnMaximize_Click!;

            // Close Button
            btnClose = new Button
            {
                Text = "×",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekText,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatAppearance = { BorderSize = 0 }
            };
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = DeepSeekDanger;
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
            btnClose.Click += (s, e) => Application.Exit();

            // Add controls to top bar
            pnlTopBar.Controls.AddRange(new Control[] {
                lblWelcome, lblRole,
                btnMinimize, btnMaximize, btnClose
            });

            this.Controls.AddRange(new Control[] { pnlContent, pnlSidebar, pnlTopBar });
        }

        private void SetupBaseForm()
        {
            // Form properties
            this.Text = "Ten Degrees POS - Dashboard";
            this.Size = new Size(1800, 1000);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = DeepSeekBg;

            // Add logout button after form is sized
            this.Load += BaseDashboard_Load;
            this.Resize += BaseDashboard_Resize;
        }

        private void BaseDashboard_Load(object? sender, EventArgs e)
        {
            // Position the window controls based on actual form width
            PositionWindowControls();

            // Create logout button now that the form is loaded and sized
            if (pnlSidebar != null)
            {
                btnLogout = new Button
                {
                    Text = "  Logout",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = DeepSeekText,
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(180, 45),  // Reduced from 200 to 180 to match thinner sidebar
                    Location = new Point(10, pnlSidebar.Height - 70),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    FlatAppearance = { BorderSize = 0 },
                    ImageAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0)
                };
                btnLogout.MouseEnter += (s, e) => btnLogout.BackColor = DeepSeekHover;
                btnLogout.MouseLeave += (s, e) => btnLogout.BackColor = Color.Transparent;
                btnLogout.Click += BtnLogout_Click!;

                pnlSidebar.Controls.Add(btnLogout);
                btnLogout.BringToFront();
            }
        }

        private void BaseDashboard_Resize(object? sender, EventArgs e)
        {
            // Reposition window controls when form is resized
            PositionWindowControls();

            // Update maximize button text based on window state
            if (btnMaximize != null)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    btnMaximize.Text = "❐";
                    btnMaximize.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                }
                else
                {
                    btnMaximize.Text = "□";
                    btnMaximize.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                }
            }

            // Update logout button position if sidebar exists
            if (pnlSidebar != null && btnLogout != null)
            {
                btnLogout.Location = new Point(10, pnlSidebar.Height - 70);
            }
        }

        private void PositionWindowControls()
        {
            if (btnMinimize != null && btnMaximize != null && btnClose != null)
            {
                // Position buttons from the right edge with proper spacing
                btnClose.Location = new Point(this.ClientSize.Width - 45, 15);
                btnMaximize.Location = new Point(this.ClientSize.Width - 90, 15);
                btnMinimize.Location = new Point(this.ClientSize.Width - 135, 15);

                btnMinimize.BringToFront();
                btnMaximize.BringToFront();
                btnClose.BringToFront();
            }
        }

        private void BtnMaximize_Click(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        public void SetUserInfo(string username, string role)
        {
            userName = username;
            userRole = role;

            var user = dbService.GetUserByUsername(username);
            if (user != null)
                userId = user.Id;

            if (lblWelcome != null)
                lblWelcome.Text = $"Welcome, {char.ToUpper(username[0]) + username[1..].ToLower()}";
            if (lblRole != null)
                lblRole.Text = $"({role.ToUpper()})";
            this.Text = $"Ten Degrees POS - {role} Dashboard";
        }

        protected virtual void BtnLogout_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?",
                "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Hide();
                Form1 loginForm = new Form1();
                loginForm.FormClosed += (s, args) => Application.Exit();
                loginForm.Show();
            }
        }

        protected Button CreateSidebarButton(string text, int yPosition, EventHandler clickHandler)
        {
            Button btn = new Button
            {
                Text = $"  {text}",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = DeepSeekTextMuted,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 42),  // Reduced width from 250 to 200, height from 45 to 42
                Location = new Point(0, yPosition),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };

            // Add hover effect
            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = DeepSeekHover;
                btn.ForeColor = DeepSeekText;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = DeepSeekTextMuted;
            };

            btn.Click += clickHandler;
            return btn;
        }

        protected Panel CreateCard(string title, string value, Color color, Point location)
        {
            Panel card = new Panel
            {
                Size = new Size(240, 120),
                Location = location,
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None
            };

            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(15, 45),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[] { lblTitle, lblValue });
            return card;
        }

        public void SetFullScreen()
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }
        protected DataGridView CreateDataGridView(Point location, Size size)
        {
            DataGridView dgv = new DataGridView
            {
                Location = location,
                Size = size,
                BackgroundColor = DeepSeekBg,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10),
                GridColor = DeepSeekBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = DeepSeekSidebar;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = DeepSeekText;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            dgv.RowsDefaultCellStyle.BackColor = DeepSeekSurface;
            dgv.RowsDefaultCellStyle.ForeColor = DeepSeekText;
            dgv.RowsDefaultCellStyle.SelectionBackColor = DeepSeekAccent;
            dgv.RowsDefaultCellStyle.SelectionForeColor = DeepSeekText;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(38, 41, 46);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = DeepSeekText;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = DeepSeekAccent;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = DeepSeekText;

            return dgv;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            dbService.Dispose();
            base.OnFormClosing(e);
        }
    }
}