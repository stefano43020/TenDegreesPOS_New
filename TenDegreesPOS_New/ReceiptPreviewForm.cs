using System;
using System.Drawing;
using System.Windows.Forms;
using TenDegreesPOS_New.Models;
using TenDegreesPOS_New.Services;

namespace TenDegreesPOS_New
{
    public class ReceiptPreviewForm : Form
    {
        private Transaction _transaction;
        private string _cashierName;
        private Action<Transaction> _onPrint;

        // DeepSeek Colors
        private readonly Color DeepSeekBg = Color.FromArgb(25, 27, 31);
        private readonly Color DeepSeekSurface = Color.FromArgb(32, 35, 39);
        private readonly Color DeepSeekText = Color.FromArgb(236, 237, 238);
        private readonly Color DeepSeekTextMuted = Color.FromArgb(148, 155, 164);
        private readonly Color DeepSeekBorder = Color.FromArgb(47, 51, 58);
        private readonly Color DeepSeekSuccess = Color.FromArgb(16, 185, 129);

        public ReceiptPreviewForm(Transaction transaction, string cashierName, Action<Transaction> onPrintAction)
        {
            _transaction = transaction;
            _cashierName = cashierName;
            _onPrint = onPrintAction;

            this.Text = "Receipt Preview";
            this.Size = new Size(420, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = DeepSeekBg;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupUI();
        }

        private void SetupUI()
        {
            // --- Paper Panel ---
            Panel pnlPaper = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(360, 550),
                BackColor = Color.White,
                AutoScroll = true
            };

            int y = 20;
            int leftMargin = 20;
            int rightMargin = 340;

            // Header
            pnlPaper.Controls.Add(CreateLabel("TEN DEGREES CAFE", 16, FontStyle.Bold, Color.Black, new Point(leftMargin, y), true));
            y += 25;
            pnlPaper.Controls.Add(CreateLabel("Commonwealth Avenue, Quezon City", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), true));
            y += 15;
            pnlPaper.Controls.Add(CreateLabel("VAT REG TIN# 123-456-789-00012", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), true));
            y += 20;

            // Divider
            pnlPaper.Controls.Add(CreateDivider(leftMargin, y, 320));
            y += 15;

            // Info
            pnlPaper.Controls.Add(CreateLabel($"SI #: {_transaction.TransactionNumber}", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
            pnlPaper.Controls.Add(CreateLabel($"Cashier: {_cashierName}", 9, FontStyle.Regular, Color.Black, new Point(180, y), false));
            y += 15;
            pnlPaper.Controls.Add(CreateLabel($"Date: {_transaction.TransactionDate:MM/dd/yyyy HH:mm}", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
            y += 20;

            // Divider
            pnlPaper.Controls.Add(CreateDivider(leftMargin, y, 320));
            y += 10;

            // Headers
            pnlPaper.Controls.Add(CreateLabel("Item", 9, FontStyle.Bold, Color.Black, new Point(leftMargin, y), false));
            pnlPaper.Controls.Add(CreateLabel("Qty", 9, FontStyle.Bold, Color.Black, new Point(200, y), false));
            pnlPaper.Controls.Add(CreateLabel("Price", 9, FontStyle.Bold, Color.Black, new Point(240, y), false));
            pnlPaper.Controls.Add(CreateLabel("Total", 9, FontStyle.Bold, Color.Black, new Point(290, y), false));
            y += 15;

            pnlPaper.Controls.Add(CreateDivider(leftMargin, y, 320));
            y += 10;

            // Items
            var items = _transaction.TransactionItems;
            if (items == null || items.Count == 0)
            {
                // Fetch items if not included in the transaction object
                var dbService = new DatabaseService();
                items = dbService.GetTransactionById(_transaction.Id)?.TransactionItems;
            }

            if (items != null)
            {
                foreach (var item in items)
                {
                    string itemName = item.Product?.Name ?? "Unknown";
                    if (!string.IsNullOrEmpty(item.Size)) itemName += $" ({item.Size})";

                    pnlPaper.Controls.Add(CreateLabel(itemName, 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
                    pnlPaper.Controls.Add(CreateLabel(item.Quantity.ToString(), 9, FontStyle.Regular, Color.Black, new Point(200, y), false));
                    pnlPaper.Controls.Add(CreateLabel($"₱{item.UnitPrice:F2}", 9, FontStyle.Regular, Color.Black, new Point(240, y), false));
                    pnlPaper.Controls.Add(CreateLabel($"₱{item.TotalPrice:F2}", 9, FontStyle.Regular, Color.Black, new Point(290, y), false));
                    y += 18;
                }
            }

            // Divider
            pnlPaper.Controls.Add(CreateDivider(leftMargin, y, 320));
            y += 15;

            // Totals
            pnlPaper.Controls.Add(CreateLabel("Subtotal:", 9, FontStyle.Regular, Color.Black, new Point(200, y), false));
            pnlPaper.Controls.Add(CreateLabel($"₱{_transaction.SubTotal:F2}", 9, FontStyle.Regular, Color.Black, new Point(290, y), false));
            y += 18;

            if (_transaction.Discount > 0)
            {
                pnlPaper.Controls.Add(CreateLabel("Discount:", 9, FontStyle.Regular, Color.Black, new Point(200, y), false));
                pnlPaper.Controls.Add(CreateLabel($"-₱{_transaction.Discount:F2}", 9, FontStyle.Regular, Color.Black, new Point(290, y), false));
                y += 18;
            }

            pnlPaper.Controls.Add(CreateLabel("VAT (12%):", 9, FontStyle.Regular, Color.Black, new Point(200, y), false));
            pnlPaper.Controls.Add(CreateLabel($"₱{_transaction.Tax:F2}", 9, FontStyle.Regular, Color.Black, new Point(290, y), false));
            y += 18;

            // Total
            pnlPaper.Controls.Add(CreateLabel("TOTAL DUE:", 11, FontStyle.Bold, Color.Black, new Point(200, y), false));
            pnlPaper.Controls.Add(CreateLabel($"₱{_transaction.Total:F2}", 11, FontStyle.Bold, Color.Black, new Point(290, y), false));
            y += 25;

            // Payment Details
            if (_transaction.PaymentMethod == "Hybrid")
            {
                pnlPaper.Controls.Add(CreateLabel("Payment (Hybrid)", 9, FontStyle.Bold, Color.Black, new Point(leftMargin, y), false));
                y += 15;
                pnlPaper.Controls.Add(CreateLabel($"  Cash: ₱{_transaction.CashPortion:F2}", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
                y += 15;
                pnlPaper.Controls.Add(CreateLabel($"  GCash: ₱{_transaction.GcashPortion:F2}", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
                y += 15;
                if (!string.IsNullOrEmpty(_transaction.GcashReferenceNumber))
                {
                    pnlPaper.Controls.Add(CreateLabel($"  Ref #: {_transaction.GcashReferenceNumber}", 8, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
                    y += 15;
                }
            }
            else if (_transaction.PaymentMethod == "GCash")
            {
                pnlPaper.Controls.Add(CreateLabel($"Payment (GCash)", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
                y += 15;
                if (!string.IsNullOrEmpty(_transaction.GcashReferenceNumber))
                {
                    pnlPaper.Controls.Add(CreateLabel($"  Ref #: {_transaction.GcashReferenceNumber}", 8, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
                    y += 15;
                }
            }
            else
            {
                pnlPaper.Controls.Add(CreateLabel($"Payment (Cash): ₱{_transaction.AmountPaid:F2}", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
                y += 15;
                pnlPaper.Controls.Add(CreateLabel($"Change: ₱{_transaction.Change:F2}", 9, FontStyle.Regular, Color.Black, new Point(leftMargin, y), false));
                y += 15;
            }

            y += 20;
            pnlPaper.Controls.Add(CreateDivider(leftMargin, y, 320));
            y += 20;

            // Footer
            pnlPaper.Controls.Add(CreateLabel("Thank you for your purchase!", 9, FontStyle.Bold, Color.Black, new Point(leftMargin, y), true));
            y += 15;
            pnlPaper.Controls.Add(CreateLabel("This is a system-generated receipt.", 8, FontStyle.Italic, Color.Gray, new Point(leftMargin, y), true));

            this.Controls.Add(pnlPaper);

            // --- Buttons ---
            Button btnPrint = new Button
            {
                Text = "🖨️ Save PDF / Print",
                Location = new Point(20, 590),
                Size = new Size(200, 45),
                BackColor = DeepSeekSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            btnPrint.Click += (s, e) =>
            {
                _onPrint?.Invoke(_transaction);
                this.Close();
            };

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(230, 590),
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(239, 68, 68), // Danger red
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnPrint);
            this.Controls.Add(btnClose);
        }

        private Label CreateLabel(string text, int size, FontStyle style, Color color, Point location, bool centered)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Consolas", size, style),
                ForeColor = color,
                Location = location,
                AutoSize = true
            };

            if (centered)
            {
                // Center text manually for receipt look
                lbl.Location = new Point(180 - (text.Length * 4), location.Y);
            }
            return lbl;
        }

        private Panel CreateDivider(int x, int y, int width)
        {
            return new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 1),
                BackColor = Color.LightGray
            };
        }
    }
}