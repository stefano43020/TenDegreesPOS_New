using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TenDegreesPOS_New.Services;

namespace TenDegreesPOS_New
{
    public partial class Form1 : Form
    {
        private TextBox? txtUsername;
        private TextBox? txtPassword;
        private Button? btnLogin;
        private Label? lblError;
        private DatabaseService? dbService;
        private Panel? pnlContainer;
        private PictureBox? picLogo;
        private Label? lblTitle;

        public Form1()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            dbService = new DatabaseService();
            SetupLoginForm();
        }

        private void SetupLoginForm()
        {
            // Form properties
            this.Size = new Size(450, 550);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(25, 27, 31);
            this.Text = "Ten Degrees POS - Login";

            // Main container panel with rounded corners
            pnlContainer = new Panel
            {
                Size = new Size(380, 480),
                Location = new Point((this.Width - 380) / 2, (this.Height - 480) / 2),
                BackColor = Color.FromArgb(32, 35, 39),
                Anchor = AnchorStyles.None
            };
            pnlContainer.Paint += (s, e) => DrawRoundedPanel(s, e, pnlContainer);

            // Try to load logo from multiple possible locations
            Image? logoImage = null;
            string[] possiblePaths = {
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.jpg"),
        Path.Combine(Application.StartupPath, "Resources", "logo.png"),
        Path.Combine(Application.StartupPath, "logo.png"),
    };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        logoImage = Image.FromFile(path);
                        break;
                    }
                    catch { /* Continue to next path */ }
                }
            }

            if (logoImage != null)
            {
                // Create a new Bitmap to ensure proper transparency handling
                Bitmap bmp = new Bitmap(logoImage);

                // Make the image background transparent if it has a white/black background
                // Check the color of the top-left pixel
                Color bgColor = bmp.GetPixel(0, 0);

                // If the background is white or black, make it transparent
                if ((bgColor.R == 255 && bgColor.G == 255 && bgColor.B == 255) ||
                    (bgColor.R == 0 && bgColor.G == 0 && bgColor.B == 0))
                {
                    bmp.MakeTransparent(bgColor);
                }

                picLogo = new PictureBox
                {
                    Image = bmp,
                    Size = new Size(80, 80),
                    Location = new Point((pnlContainer.Width - 80) / 2, 30),
                    BackColor = Color.Transparent,
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                // Remove this line - it's causing the error
                // picLogo.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

                pnlContainer.Controls.Add(picLogo);

                // Bring the logo to front
                picLogo.BringToFront();
            }
            else
            {
                // Fallback to emoji if no image found
                Label lblFallbackLogo = new Label
                {
                    Text = "☕",
                    Font = new Font("Segoe UI", 52, FontStyle.Regular),
                    ForeColor = Color.FromArgb(76, 130, 250),
                    Location = new Point((pnlContainer.Width - 60) / 2, 25),
                    Size = new Size(60, 70),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                pnlContainer.Controls.Add(lblFallbackLogo);
            }

            // Title label
            lblTitle = new Label
            {
                Text = "Ten Degrees POS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(236, 237, 238),
                Location = new Point((pnlContainer.Width - 200) / 2, 120),
                Size = new Size(200, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Username TextBox
            Label lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 155, 164),
                Location = new Point(40, 180),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtUsername = new TextBox
            {
                Location = new Point(40, 205),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(25, 27, 31),
                ForeColor = Color.FromArgb(236, 237, 238),
                BorderStyle = BorderStyle.FixedSingle,
                Text = ""
            };
            txtUsername.TextChanged += (s, e) => ClearError();

            // Password TextBox
            Label lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 155, 164),
                Location = new Point(40, 260),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtPassword = new TextBox
            {
                Location = new Point(40, 285),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(25, 27, 31),
                ForeColor = Color.FromArgb(236, 237, 238),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true,
                Text = ""
            };
            txtPassword.TextChanged += (s, e) => ClearError();
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) AttemptLogin(); };

            // Login Button
            btnLogin = new Button
            {
                Text = "LOGIN",
                Location = new Point(40, 345),
                Size = new Size(300, 45),
                BackColor = Color.FromArgb(76, 130, 250),
                ForeColor = Color.FromArgb(236, 237, 238),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => AttemptLogin();
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(96, 150, 255);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(76, 130, 250);

            // Error Label
            lblError = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 400),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Close button
            Button btnClose = new Button
            {
                Text = "×",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(236, 237, 238),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(35, 35),
                Location = new Point(this.Width - 45, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(239, 68, 68);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;

            // Minimize button
            Button btnMinimize = new Button
            {
                Text = "—",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(236, 237, 238),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(35, 35),
                Location = new Point(this.Width - 85, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.FromArgb(42, 46, 52);
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;

            // Add controls to form
            this.Controls.Add(btnClose);
            this.Controls.Add(btnMinimize);
            this.Controls.Add(pnlContainer);
            pnlContainer.Controls.Add(lblTitle);
            pnlContainer.Controls.Add(lblUsername);
            pnlContainer.Controls.Add(txtUsername);
            pnlContainer.Controls.Add(lblPassword);
            pnlContainer.Controls.Add(txtPassword);
            pnlContainer.Controls.Add(btnLogin);
            pnlContainer.Controls.Add(lblError);
        }

        private void DrawRoundedPanel(object sender, PaintEventArgs e, Panel panel)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 15;
            Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            panel.Region = new Region(path);

            using (Pen pen = new Pen(Color.FromArgb(47, 51, 58), 1))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        private void ClearError()
        {
            if (lblError != null)
                lblError.Text = "";
        }

        private void AttemptLogin()
        {
            if (string.IsNullOrWhiteSpace(txtUsername?.Text))
            {
                lblError!.Text = "Please enter username";
                txtUsername?.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword?.Text))
            {
                lblError!.Text = "Please enter password";
                txtPassword?.Focus();
                return;
            }

            try
            {
                var user = dbService?.AuthenticateUser(txtUsername.Text.Trim(), txtPassword.Text);

                if (user != null && user.IsActive)
                {
                    // Login successful
                    this.Hide();

                    if (user.Role == "Admin")
                    {
                        AdminDashboard admin = new AdminDashboard(user.Username);
                        admin.Show();
                        this.Hide();
                    }
                    else if (user.Role == "Barista") // <--- ADD THIS BLOCK
                    {
                        BaristaDashboard barista = new BaristaDashboard();
                        barista.Show();
                        this.Hide();
                    }
                    else if (user.Role == "Cashier" || user.Role == "Manager")
                    {
                        CashierDashboard cashier = new CashierDashboard(user.Username);
                        cashier.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Invalid user role.");
                    }
                }
                else
                {
                    lblError!.Text = "Invalid username or password";
                    txtPassword!.Text = "";
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                lblError!.Text = $"Login Error: {ex.Message}";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            dbService?.Dispose();
            base.OnFormClosing(e);
        }
    }
}