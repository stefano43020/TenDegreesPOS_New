using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TenDegreesPOS_New.Services;

namespace TenDegreesPOS_New
{
    public class ReportPreviewForm : Form
    {
        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _totalSales;
        private int _totalItems;
        private int _totalTransactions;
        private List<SalesProductPerformance> _performance;
        private List<SalesChartDataPoint> _chartData;
        private Action _onExport;

        private readonly Color DeepSeekBg = Color.FromArgb(25, 27, 31);
        private readonly Color DeepSeekSurface = Color.FromArgb(32, 35, 39);
        private readonly Color DeepSeekText = Color.FromArgb(236, 237, 238);
        private readonly Color DeepSeekSuccess = Color.FromArgb(16, 185, 129);
        private readonly Color DeepSeekDanger = Color.FromArgb(239, 68, 68);

        public ReportPreviewForm(DateTime start, DateTime end, decimal sales, int items, int txCount, List<SalesProductPerformance> perf, List<SalesChartDataPoint> chart, Action onExport)
        {
            _startDate = start;
            _endDate = end;
            _totalSales = sales;
            _totalItems = items;
            _totalTransactions = txCount;
            _performance = perf;
            _chartData = chart;
            _onExport = onExport;

            this.Text = "Report Preview";
            this.Size = new Size(700, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = DeepSeekBg;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            SetupUI();
        }

        private void SetupUI()
        {
            // The "Paper" panel
            Panel pnlPaper = new Panel
            {
                Location = new Point(30, 20),
                Size = new Size(620, 650),
                BackColor = Color.White,
                AutoScroll = true
            };

            int y = 20;
            int left = 20;

            // Header
            pnlPaper.Controls.Add(CreateLabel("TEN DEGREES POS", 18, FontStyle.Bold, Color.Black, new Point(left, y))); y += 30;
            pnlPaper.Controls.Add(CreateLabel("Sales Analytics Report", 14, FontStyle.Bold, Color.Blue, new Point(left, y))); y += 25;
            pnlPaper.Controls.Add(CreateLabel($"Period: {_startDate:MMM dd, yyyy} - {_endDate:MMM dd, yyyy}", 10, FontStyle.Regular, Color.Gray, new Point(left, y))); y += 25;

            pnlPaper.Controls.Add(CreateDivider(left, y, 580)); y += 15;

            // Summary
            pnlPaper.Controls.Add(CreateLabel($"Total Sales: ₱{_totalSales:N2}", 12, FontStyle.Bold, Color.Green, new Point(left, y))); y += 25;
            pnlPaper.Controls.Add(CreateLabel($"Items Sold: {_totalItems}", 12, FontStyle.Bold, Color.Black, new Point(left, y))); y += 25;
            pnlPaper.Controls.Add(CreateLabel($"Transactions: {_totalTransactions}", 12, FontStyle.Bold, Color.Black, new Point(left, y))); y += 25;

            pnlPaper.Controls.Add(CreateDivider(left, y, 580)); y += 15;

            // Top Products (Preview of the grid)
            pnlPaper.Controls.Add(CreateLabel("Product Performance", 12, FontStyle.Bold, Color.Black, new Point(left, y))); y += 25;

            foreach (var p in _performance.Take(10)) // Show top 10 in preview
            {
                string line = $"{p.ProductName,-30} {p.UnitsSold,5} units   ₱{p.Revenue,8:N2}";
                pnlPaper.Controls.Add(CreateLabel(line, 10, FontStyle.Regular, Color.Black, new Point(left, y)));
                y += 20;
            }

            if (_performance.Count > 10)
            {
                pnlPaper.Controls.Add(CreateLabel($"...and {_performance.Count - 10} more items.", 10, FontStyle.Italic, Color.Gray, new Point(left, y)));
                y += 20;
            }

            y += 20;
            pnlPaper.Controls.Add(CreateLabel("This is a preview. Click Export below to save the full PDF.", 9, FontStyle.Italic, Color.Gray, new Point(left, y)));

            this.Controls.Add(pnlPaper);

            // Buttons
            Button btnExport = new Button
            {
                Text = "📄 Export PDF",
                Location = new Point(30, 690),
                Size = new Size(180, 45),
                BackColor = DeepSeekSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnExport.Click += (s, e) =>
            {
                _onExport?.Invoke();
                this.Close();
            };

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(220, 690),
                Size = new Size(120, 45),
                BackColor = DeepSeekDanger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnExport);
            this.Controls.Add(btnClose);
        }

        private Label CreateLabel(string text, int size, FontStyle style, Color color, Point location)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Consolas", size, style), // Monospace font for alignment
                ForeColor = color,
                Location = location,
                AutoSize = true
            };
        }

        private Panel CreateDivider(int x, int y, int width)
        {
            return new Panel { Location = new Point(x, y), Size = new Size(width, 1), BackColor = Color.LightGray };
        }
    }
}