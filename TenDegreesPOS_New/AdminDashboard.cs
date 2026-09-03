using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IO;
using System.Drawing.Drawing2D;
using TenDegreesPOS_New.Models;
using TenDegreesPOS_New.Services;
using System.Drawing.Drawing2D;

namespace TenDegreesPOS_New
{
    public partial class AdminDashboard : BaseDashboard
    {
        private Button? btnDashboard;
        private Button? btnProduct;
        private Button? btnStaff;
        private Button? btnIngredients;
        private Button? btnSupplier;
        private Button? btnReports;

        private List<Ingredient> allIngredientsList = new List<Ingredient>();


        // Order Cancellation Controls
        private Button? btnCancellations;
        private DataGridView? dgvCompletedOrders;
        private DataGridView? dgvCancellations;
        private DateTimePicker? dtpCancelFrom;
        private DateTimePicker? dtpCancelTo;
        private Button? btnFilterCancellations;
        private TextBox? txtSearchOrder;
        private Button? btnCancelOrderTransaction;  // For order cancellations
        private Button? btnViewArchived;

        // Size-specific ingredient quantity controls (for Product Management)
        private NumericUpDown? nudSmallQty;
        private NumericUpDown? nudMediumQty;
        private NumericUpDown? nudLargeQty;
        private Label? lblSmallCostValue;
        private Label? lblMediumCostValue;
        private Label? lblLargeCostValue;
        private Label? lblBasePriceNote;

        // Product Management Controls
        private TextBox? txtSearchProduct;
        private ComboBox? cmbCategory;
        private FlowLayoutPanel? flpProducts;
        private Panel? pnlProductForm;
        private TextBox? txtProductName;
        private TextBox? txtProductPrice;
        private TextBox? txtProductSmallPrice;
        private TextBox? txtProductMediumPrice;
        private TextBox? txtProductLargePrice;
        private CheckBox? chkHasSizes;
        private Panel? pnlSizePricing;
        private ComboBox? cmbProductCategory;
        private Button? btnSaveProduct;
        private Button? btnCancelProduct;  // For product management (only ONE declaration)
        private Button? btnNewProduct;
        private int? selectedProductId;
        private Product? selectedProduct;

        // Report Export Buttons
        private Button? btnExportSalesPDF;
        private Button? btnExportInventoryPDF;
        private Button? btnDailySalesReport;
        private Button? btnDailyInventoryReport;
        private Button? btnMonthlySalesReport;
        private Button? btnMonthlyInventoryReport;

        // Report Module Panel References
        private Panel? pnlDateRange;
        private Panel? pnlSummary;
        private Panel? pnlTotalSalesCard;
        private Panel? pnlProductsSoldCard;
        private Panel? pnlTransactionsCard;
        private Panel? pnlChartContainer;
        private Panel? pnlPerformanceContainer;
        private Panel? pnlTotalIngredientsCard;
        private Panel? pnlLowStockCard;
        private Panel? pnlOutOfStockCard;
        private Panel? pnlInventoryContainer;

        // Product Image Controls
        private PictureBox? picProductImage;
        private Button? btnSelectImage;
        private string? selectedImagePath;

        // Ingredient Management Controls (for Product tab)
        private DataGridView? dgvProductIngredients;
        private ComboBox? cmbIngredient;
        private NumericUpDown? nudIngredientQuantity;
        private ComboBox? cmbProductIngredientUnit;
        private Button? btnSetIngredient;
        private Button? btnRemoveIngredient;
        private Button? btnClearIngredients;
        private Button? btnSaveIngredients;
        private Button? btnCancelIngredients;
        private List<ProductIngredient>? currentProductIngredients;
        private Panel? pnlIngredientPanel;

        // Cost Summary Controls
        private Label? lblRawMaterialCost;
        private Label? lblTotalCost;

        // Staff Management Controls
        private TextBox? txtSearchStaff;
        private DataGridView? dgvStaff;
        private Panel? pnlStaffForm;
        private TextBox? txtStaffName;
        private TextBox? txtStaffEmail;
        private TextBox? txtStaffContact;
        private ComboBox? cmbStaffRole;
        private TextBox? txtStaffPassword;
        private Button? btnSaveStaff;
        private Button? btnCancelStaff;
        private int? selectedStaffId;

        // Ingredient Library Controls
        private TextBox? txtSearchIngredient;
        private DataGridView? dgvIngredientLibrary;
        private Panel? pnlIngredientForm;
        private TextBox? txtIngredientName;
        private ComboBox? cmbIngredientUnit;
        private NumericUpDown? nudMinimumStock;
        private CheckBox? chkIngredientActive;
        private Button? btnSaveIngredient;
        private Button? btnCancelIngredient;
        private Button? btnDeleteIngredient;
        private NumericUpDown? nudAdjustQuantity;
        private ComboBox? cmbAdjustOperation;
        private int? selectedIngredientId;
        private Panel? pnlStockAdjustment;
        private Label? lblSelectedIngredient;

        // Supplier Management Controls
        private TextBox? txtSearchSupplier;
        private DataGridView? dgvSuppliers;
        private Panel? pnlSupplierForm;
        private TextBox? txtSupplierName;
        private TextBox? txtSupplierContact;
        private TextBox? txtSupplierEmail;
        private TextBox? txtSupplierAddress;
        private ComboBox? cmbItemCategory;
        private Button? btnSaveSupplier;
        private Button? btnCancelSupplier;
        private int? selectedSupplierId;

        // Supplier Ingredient Assignment Controls
        private TabControl? tcSupplier;
        private CheckedListBox? clbSupplierIngredients;
        private Button? btnSaveSupplierIngredients;
        private Dictionary<int, List<int>> supplierIngredientsCache = new Dictionary<int, List<int>>();

        // Purchase Order Controls
        private ComboBox? cmbSupplierSelect;
        private DataGridView? dgvIngredientsForOrder;
        private NumericUpDown? nudOrderQuantity;
        private Button? btnAddToOrder;
        private DataGridView? dgvOrderCart;
        private Label? lblOrderTotal;
        private Button? btnSubmitOrder;
        private Button? btnCancelOrder;  // For purchase order clear cart
        private DataGridView? dgvPurchaseOrders;
        private List<PurchaseOrderCartItem> orderCart = new List<PurchaseOrderCartItem>();

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
        private readonly Color DeepSeekCard = Color.FromArgb(32, 35, 39);
        private readonly Color DeepSeekTableHeader = Color.FromArgb(18, 20, 23);
        private readonly Color DeepSeekTableRow = Color.FromArgb(32, 35, 39);
        private readonly Color DeepSeekTableRowAlt = Color.FromArgb(38, 41, 46);
        private readonly Color DeepSeekTableSelected = Color.FromArgb(56, 60, 70);

        public AdminDashboard(string username)
        {
            this.Size = new Size(1800, 1000);
            InitializeComponent();
            SetUserInfo(username, "Admin");
            SetupAdminControls();
            ShowDashboard();  // Dashboard is now the default view
        }

        private void InitializeComponent()
        {
            this.Text = "Admin Dashboard";
        }

        private void SetupAdminControls()
        {


            // Add this in SetupAdminControls() with the other buttons
            btnCancellations = CreateSidebarButton("🚫 Order Cancellations", 310, BtnCancellations_Click!);

            if (pnlSidebar != null)
            {
                pnlSidebar.Controls.AddRange(new Control[] {
        btnDashboard, btnProduct, btnIngredients, btnStaff, btnSupplier, btnReports, btnCancellations
    });
            }
            // Dashboard button at the TOP
            btnDashboard = CreateSidebarButton("📊 Dashboard", 10, BtnDashboard_Click!);
            btnProduct = CreateSidebarButton("📦 Product Management", 60, BtnProduct_Click!);
            btnIngredients = CreateSidebarButton("🥗 Ingredients", 110, BtnIngredients_Click!);
            btnStaff = CreateSidebarButton("👥 User Management", 160, BtnStaff_Click!);
            btnSupplier = CreateSidebarButton("🏭 Supplier", 210, BtnSupplier_Click!);
            btnReports = CreateSidebarButton("📊 Report Module", 260, BtnReports_Click!);

            if (pnlSidebar != null)
            {
                pnlSidebar.Controls.AddRange(new Control[] { btnDashboard, btnProduct, btnIngredients, btnStaff, btnSupplier, btnReports });
            }
        }

        #region Dashboard

        private void ShowDashboard()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true
            };

            // Use the full content area without a separate scroll panel
            pnlContent.AutoScroll = true;

            int currentY = 70;

            // ========== SECTION 1: Sales Overview - Last 7 Days (Full Width) ==========
            Panel pnlSalesOverview = CreateSalesOverviewPanel(currentY);
            pnlContent.Controls.Add(pnlSalesOverview);
            currentY += pnlSalesOverview.Height + 20;

            // ========== SECTION 2: Stock Overview and Alerts Side by Side ==========
            // Create a horizontal split container for Stock and Alerts
            SplitContainer splitContainer = new SplitContainer
            {
                Location = new Point(30, currentY),
                Size = new Size(pnlContent.Width - 60, 500),
                SplitterDistance = (pnlContent.Width - 60) * 2 / 3,
                SplitterWidth = 20,
                BackColor = DeepSeekBg,
                BorderStyle = BorderStyle.None
            };
            splitContainer.Paint += (s, e) => { };

            // Stock Overview Panel (Left side - 2/3 width)
            Panel pnlStockOverview = CreateStockOverviewPanelForSplit();
            splitContainer.Panel1.Controls.Add(pnlStockOverview);

            // Alerts Panel (Right side - 1/3 width)
            Panel pnlAlerts = CreateAlertsPanelForSplit();
            splitContainer.Panel2.Controls.Add(pnlAlerts);

            pnlContent.Controls.Add(splitContainer);
            currentY += splitContainer.Height + 20;

            pnlContent.Controls.Add(lblTitle);
        }

        private Panel CreateSalesOverviewPanel(int yPosition)
        {
            Panel pnlSales = new Panel
            {
                Location = new Point(30, yPosition),
                Size = new Size(pnlContent!.Width - 60, 300),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlSales.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSales.Width - 1, pnlSales.Height - 1);
                }
            };

            Label lblSalesTitle = new Label
            {
                Text = "Sales Overview - Last 7 Days",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlSales.Controls.Add(lblSalesTitle);

            // Get last 7 days sales data
            var last7DaysSales = GetLast7DaysSales();
            decimal maxSales = last7DaysSales.Max() > 0 ? last7DaysSales.Max() : 1;
            string peakDay = GetPeakDay(last7DaysSales);

            // Peak day info
            Label lblPeakDay = new Label
            {
                Text = $"Peak day: {peakDay} - ₱{maxSales:F2}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekAccent,
                Location = new Point(15, 50),
                AutoSize = true
            };
            pnlSales.Controls.Add(lblPeakDay);

            // Store data in panel tag for resize
            pnlSales.Tag = new SalesChartData { Sales = last7DaysSales.ToList(), MaxSales = maxSales };

            // Draw bars directly using Paint event instead of creating controls
            pnlSales.Paint += (sender, e) =>
            {
                DrawSalesChart(sender, e, last7DaysSales, maxSales);
            };

            return pnlSales;
        }

        private void DrawSalesChart(object sender, PaintEventArgs e, List<decimal> sales, decimal maxSales)
        {
            Panel pnl = sender as Panel;
            if (pnl == null) return;

            int chartTop = 100;
            int chartHeight = 150;
            int baseY = chartTop + chartHeight;
            int startX = 50;
            int availableWidth = pnl.Width - 100;
            int barWidth = Math.Max(40, availableWidth / 14);
            int spacing = Math.Max(15, (availableWidth - (barWidth * 7)) / 8);

            string[] dayNames = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            int maxBarHeight = 130;

            // Draw grid lines
            using (Pen gridPen = new Pen(Color.FromArgb(60, 60, 70), 1))
            using (SolidBrush barBrush = new SolidBrush(DeepSeekAccent))
            using (SolidBrush textBrush = new SolidBrush(DeepSeekText))
            using (SolidBrush mutedBrush = new SolidBrush(DeepSeekTextMuted))
            using (Font dayFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (Font amountFont = new Font("Segoe UI", 9))
            using (Pen accentPen = new Pen(DeepSeekAccent, 2))
            {
                // Draw horizontal grid lines
                for (int i = 0; i <= 4; i++)
                {
                    int yPos = baseY - (i * maxBarHeight / 4);
                    e.Graphics.DrawLine(gridPen, startX - 10, yPos, pnl.Width - 30, yPos);

                    // Draw value labels
                    string valueLabel = $"₱{(maxSales * i / 4):F0}";
                    SizeF labelSize = e.Graphics.MeasureString(valueLabel, amountFont);
                    e.Graphics.DrawString(valueLabel, amountFont, mutedBrush, startX - 35 - labelSize.Width, yPos - 8);
                }

                for (int i = 0; i < 7; i++)
                {
                    int barHeight = (int)((sales[i] / maxSales) * maxBarHeight);
                    if (barHeight < 3 && sales[i] > 0) barHeight = 3;

                    int xPos = startX + spacing + i * (barWidth + spacing);
                    int yPos = baseY - barHeight;

                    // Draw bar
                    e.Graphics.FillRectangle(barBrush, xPos, yPos, barWidth, barHeight);

                    // Draw bar outline
                    e.Graphics.DrawRectangle(accentPen, xPos, yPos, barWidth, barHeight);

                    // Draw day label
                    SizeF daySize = e.Graphics.MeasureString(dayNames[i], dayFont);
                    e.Graphics.DrawString(dayNames[i], dayFont, textBrush,
                        xPos + (barWidth - daySize.Width) / 2, baseY + 8);

                    // Draw amount label above bar
                    string amountText = $"₱{sales[i]:F2}";
                    SizeF amountSize = e.Graphics.MeasureString(amountText, amountFont);

                    // Add background for amount label
                    RectangleF amountRect = new RectangleF(
                        xPos + (barWidth - amountSize.Width) / 2 - 2,
                        yPos - 18,
                        amountSize.Width + 4,
                        amountSize.Height + 2
                    );
                    e.Graphics.FillRectangle(new SolidBrush(DeepSeekSurface), amountRect);

                    // Use separate brushes for different colors
                    if (sales[i] > 0)
                    {
                        e.Graphics.DrawString(amountText, amountFont, new SolidBrush(DeepSeekSuccess),
                            xPos + (barWidth - amountSize.Width) / 2, yPos - 18);
                    }
                    else
                    {
                        e.Graphics.DrawString(amountText, amountFont, mutedBrush,
                            xPos + (barWidth - amountSize.Width) / 2, yPos - 18);
                    }
                }

                // Draw baseline
                e.Graphics.DrawLine(new Pen(DeepSeekTextMuted, 1), startX - 10, baseY, pnl.Width - 30, baseY);
            }
        }

        // Helper class for storing chart data
        private class SalesChartData
        {
            public List<decimal> Sales { get; set; } = new List<decimal>();
            public decimal MaxSales { get; set; }
        }

        private Panel CreateStockOverviewPanelForSplit()
        {
            Panel pnlStock = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };
            pnlStock.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlStock.Width - 1, pnlStock.Height - 1);
                }
            };

            Label lblStockTitle = new Label
            {
                Text = "Stock Overview",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlStock.Controls.Add(lblStockTitle);

            DataGridView dgvStock = CreateDataGridView(new Point(15, 55), new Size(pnlStock.Width - 30, pnlStock.Height - 110));
            dgvStock.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvStock.Columns.Add("Name", "Ingredient Name");
            dgvStock.Columns.Add("CurrentStock", "Current Stock");
            dgvStock.Columns.Add("MinStock", "Min Stock");
            dgvStock.Columns.Add("Status", "Status");

            dgvStock.Columns["Name"].Width = (dgvStock.Width - 30) * 40 / 100;
            dgvStock.Columns["CurrentStock"].Width = (dgvStock.Width - 30) * 20 / 100;
            dgvStock.Columns["MinStock"].Width = (dgvStock.Width - 30) * 20 / 100;
            dgvStock.Columns["Status"].Width = (dgvStock.Width - 30) * 15 / 100;

            var ingredients = dbService.GetAllIngredients().Where(i => i.IsActive).ToList();
            var inventory = dbService.GetAllInventory();

            foreach (var ingredient in ingredients)
            {
                var stock = inventory.FirstOrDefault(i => i.IngredientId == ingredient.Id);
                decimal currentStock = stock?.Quantity ?? 0;

                // Low Stock = 50% of Minimum Stock
                decimal lowStockThreshold = ingredient.MinimumStock * 0.5m;
                string status;
                Color statusColor;

                if (currentStock <= 0)
                {
                    status = "Out of Stock";
                    statusColor = DeepSeekDanger;
                }
                else if (currentStock <= lowStockThreshold)
                {
                    status = "Low Stock";
                    statusColor = DeepSeekWarning;
                }
                else
                {
                    status = "In Stock";
                    statusColor = DeepSeekSuccess;
                }

                int rowIndex = dgvStock.Rows.Add(
                    ingredient.Name,
                    $"{currentStock:F1} {ingredient.Unit ?? "kg"}",
                    $"{ingredient.MinimumStock} {ingredient.Unit ?? "kg"}",
                    status
                );
                dgvStock.Rows[rowIndex].Cells["Status"].Style.ForeColor = statusColor;
                dgvStock.Rows[rowIndex].Cells["Status"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }

            pnlStock.Controls.Add(dgvStock);

            Button btnManageAll = new Button
            {
                Text = "Manage All →",
                Location = new Point(pnlStock.Width - 145, pnlStock.Height - 45),
                Size = new Size(130, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnManageAll.Click += (s, e) => ShowIngredientLibrary();
            pnlStock.Controls.Add(btnManageAll);

            return pnlStock;
        }

        private Panel CreateAlertsPanelForSplit()
        {
            Panel pnlAlerts = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };
            pnlAlerts.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlAlerts.Width - 1, pnlAlerts.Height - 1);
                }
            };

            Label lblAlertsTitle = new Label
            {
                Text = "Alerts & Notifications",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlAlerts.Controls.Add(lblAlertsTitle);

            var ingredients = dbService.GetAllIngredients().Where(i => i.IsActive).ToList();
            var inventory = dbService.GetAllInventory();
            var lowStockItems = new List<(string name, decimal stock, string unit, string status)>();

            foreach (var ingredient in ingredients)
            {
                var stock = inventory.FirstOrDefault(i => i.IngredientId == ingredient.Id);
                decimal currentStock = stock?.Quantity ?? 0;
                decimal lowStockThreshold = ingredient.MinimumStock * 0.5m;

                if (currentStock <= 0)
                {
                    lowStockItems.Add((ingredient.Name, currentStock, ingredient.Unit ?? "kg", "Critical"));
                }
                else if (currentStock <= lowStockThreshold)
                {
                    lowStockItems.Add((ingredient.Name, currentStock, ingredient.Unit ?? "kg", "Warning"));
                }
            }

            Panel pnlAlertList = new Panel
            {
                Location = new Point(15, 55),
                Size = new Size(pnlAlerts.Width - 30, pnlAlerts.Height - 80),
                BackColor = Color.Transparent,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            int alertY = 5;
            int alertCount = 0;

            foreach (var item in lowStockItems)
            {
                Panel pnlAlert = new Panel
                {
                    Location = new Point(5, alertY),
                    Size = new Size(pnlAlertList.Width - 25, 50),
                    BackColor = Color.Transparent,
                    BorderStyle = BorderStyle.None,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                pnlAlert.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(DeepSeekBorder, 1))
                    {
                        e.Graphics.DrawLine(pen, 0, pnlAlert.Height - 1, pnlAlert.Width, pnlAlert.Height - 1);
                    }
                };

                Label lblAlertName = new Label
                {
                    Text = item.name,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = DeepSeekText,
                    Location = new Point(10, 15),
                    AutoSize = true
                };
                pnlAlert.Controls.Add(lblAlertName);

                Label lblAlertStock = new Label
                {
                    Text = $"{item.stock:F1}{item.unit} left",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = DeepSeekTextMuted,
                    Location = new Point(10, 32),
                    AutoSize = true
                };
                pnlAlert.Controls.Add(lblAlertStock);

                Label lblAlertStatus = new Label
                {
                    Text = item.status,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = item.status == "Critical" ? DeepSeekDanger : DeepSeekWarning,
                    Location = new Point(pnlAlert.Width - 80, 15),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    AutoSize = true
                };
                pnlAlert.Controls.Add(lblAlertStatus);

                pnlAlertList.Controls.Add(pnlAlert);
                alertY += 55;
                alertCount++;
            }

            if (alertCount == 0)
            {
                Label lblNoAlerts = new Label
                {
                    Text = "No alerts at this time.\nAll ingredients have sufficient stock.",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = DeepSeekSuccess,
                    Location = new Point(15, 55),
                    AutoSize = true
                };
                pnlAlertList.Controls.Add(lblNoAlerts);
            }

            pnlAlerts.Controls.Add(pnlAlertList);

            return pnlAlerts;
        }

        private List<decimal> GetLast7DaysSales()
        {
            var sales = new List<decimal>();
            var today = DateTime.Today;

            // Get start of week (Monday)
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-diff);

            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                // Get daily sales - these are already the total amounts (tax included)
                decimal dailySales = dbService.GetDailySales(date);
                sales.Add(dailySales);
            }

            return sales;
        }

        private string GetPeakDay(List<decimal> sales)
        {
            string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            int maxIndex = 0;
            for (int i = 1; i < sales.Count; i++)
            {
                if (sales[i] > sales[maxIndex])
                    maxIndex = i;
            }
            return dayNames[maxIndex];
        }

        #endregion

        #region Product Management Helper Methods

        private void LoadCategories()
        {
            if (cmbCategory == null) return;
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("All Categories");
            var categories = dbService.GetAllCategories();
            foreach (var category in categories)
            {
                cmbCategory.Items.Add(category.Name);
            }
        }

        private void LoadCategoryCombo()
        {
            if (cmbProductCategory == null) return;
            cmbProductCategory.Items.Clear();
            var categories = dbService.GetAllCategories();
            foreach (var category in categories)
            {
                cmbProductCategory.Items.Add(category.Name);
            }
            if (cmbProductCategory.Items.Count > 0)
                cmbProductCategory.SelectedIndex = 0;
        }

        private void LoadIngredientCombo()
        {
            if (cmbIngredient == null) return;

            allIngredientsList = dbService.GetAllIngredients().Where(i => i.IsActive).ToList();

            cmbIngredient.BeginUpdate();
            cmbIngredient.Items.Clear();
            foreach (var ingredient in allIngredientsList)
            {
                cmbIngredient.Items.Add(ingredient);
            }
            cmbIngredient.DisplayMember = "Name";
            cmbIngredient.ValueMember = "Id";
            cmbIngredient.Text = "";
            cmbIngredient.SelectedIndex = -1;  // Important: no selection
            cmbIngredient.EndUpdate();
        }

        private void LoadProductCards()
        {
            if (flpProducts == null) return;
            flpProducts.Controls.Clear();
            var products = dbService.GetAllProducts();

            string searchText = txtSearchProduct?.Text?.ToLower() ?? "";
            string? categoryFilter = cmbCategory?.SelectedItem?.ToString();

            foreach (var product in products.Where(p => p.IsActive))
            {
                if (!string.IsNullOrEmpty(searchText) && !product.Name.ToLower().Contains(searchText))
                    continue;

                if (categoryFilter != "All Categories" && categoryFilter != null && product.Category?.Name != categoryFilter)
                    continue;

                Panel card = new Panel
                {
                    Size = new Size(630, 150),
                    BackColor = DeepSeekSurface,
                    Margin = new Padding(0, 0, 0, 10),
                    Tag = product,
                    Padding = new Padding(10)
                };

                card.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(DeepSeekBorder, 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                    }
                    using (Pen accentPen = new Pen(DeepSeekAccent, 2))
                    {
                        e.Graphics.DrawLine(accentPen, 0, 0, card.Width, 0);
                    }
                };

                Label lblName = new Label
                {
                    Text = product.Name,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = DeepSeekText,
                    Location = new Point(12, 12),
                    AutoSize = true
                };

                Label lblCategory = new Label
                {
                    Text = $"Category: {product.Category?.Name ?? "Uncategorized"}",
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = DeepSeekTextMuted,
                    Location = new Point(12, 37),
                    AutoSize = true
                };

                int yOffset = 0;
                if (product.HasSizes)
                {
                    Label lblSizes = new Label
                    {
                        Text = $"S: ₱{product.PriceSmall?.ToString("F2") ?? "0.00"}   M: ₱{product.PriceMedium?.ToString("F2") ?? "0.00"}   L: ₱{product.PriceLarge?.ToString("F2") ?? "0.00"}",
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = DeepSeekAccent,
                        Location = new Point(12, 57),
                        AutoSize = true
                    };
                    card.Controls.Add(lblSizes);
                    yOffset = 20;
                }
                else
                {
                    Label lblPrice = new Label
                    {
                        Text = $"₱{product.Price:F2}",
                        Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        ForeColor = DeepSeekSuccess,
                        Location = new Point(450, 15),
                        AutoSize = true
                    };
                    card.Controls.Add(lblPrice);
                }

                Button btnEdit = new Button
                {
                    Text = "Edit",
                    Size = new Size(80, 32),
                    Location = new Point(12, 80 + yOffset),
                    BackColor = DeepSeekAccent,
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Tag = product,
                    FlatAppearance = { BorderSize = 0 }
                };
                btnEdit.Click += BtnEditProduct_Click!;

                Button btnViewIngredients = new Button
                {
                    Text = "Ingredients",
                    Size = new Size(100, 32),
                    Location = new Point(100, 80 + yOffset),
                    BackColor = DeepSeekInfo,
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Tag = product,
                    FlatAppearance = { BorderSize = 0 }
                };
                btnViewIngredients.Click += BtnViewIngredients_Click!;

                Button btnDelete = new Button
                {
                    Text = "Delete",
                    Size = new Size(80, 32),
                    Location = new Point(210, 80 + yOffset),
                    BackColor = DeepSeekDanger,
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Tag = product,
                    FlatAppearance = { BorderSize = 0 }
                };
                btnDelete.Click += BtnDeleteProduct_Click!;

                card.Controls.AddRange(new Control[] { lblName, lblCategory, btnEdit, btnViewIngredients, btnDelete });
                flpProducts.Controls.Add(card);
            }
        }

        private void TxtSearchProduct_TextChanged(object sender, EventArgs e) => LoadProductCards();

        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e) => LoadProductCards();

        private void BtnNewProduct_Click(object sender, EventArgs e)
        {
            if (pnlIngredientPanel != null) pnlIngredientPanel.Visible = false;
            selectedProductId = null;
            selectedProduct = null;
            ClearProductForm();
            if (pnlProductForm != null)
            {
                pnlProductForm.Visible = true;
                pnlProductForm.BringToFront();
                txtProductName?.Focus();
            }
        }

        private void BtnEditProduct_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product product)
            {
                if (pnlIngredientPanel != null) pnlIngredientPanel.Visible = false;
                selectedProductId = product.Id;
                selectedProduct = product;

                if (txtProductName != null) txtProductName.Text = product.Name;
                if (txtProductPrice != null) txtProductPrice.Text = product.Price.ToString("F2");
                if (cmbProductCategory != null && product.Category != null)
                    cmbProductCategory.SelectedItem = product.Category.Name;

                if (chkHasSizes != null)
                {
                    chkHasSizes.Checked = product.HasSizes;
                    if (pnlSizePricing != null) pnlSizePricing.Visible = product.HasSizes;
                }

                if (txtProductSmallPrice != null) txtProductSmallPrice.Text = product.PriceSmall?.ToString("F2") ?? "0.00";
                if (txtProductMediumPrice != null) txtProductMediumPrice.Text = product.PriceMedium?.ToString("F2") ?? "0.00";
                if (txtProductLargePrice != null) txtProductLargePrice.Text = product.PriceLarge?.ToString("F2") ?? "0.00";

                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string imagePath = Path.Combine(Application.StartupPath, product.ImagePath);
                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            picProductImage!.Image = Image.FromFile(imagePath);
                            selectedImagePath = product.ImagePath;
                        }
                        catch
                        {
                            picProductImage!.Image = null;
                            selectedImagePath = null;
                        }
                    }
                    else
                    {
                        picProductImage!.Image = null;
                        selectedImagePath = null;
                    }
                }
                else
                {
                    picProductImage!.Image = null;
                    selectedImagePath = null;
                }

                if (pnlProductForm != null)
                {
                    pnlProductForm.Visible = true;
                    pnlProductForm.BringToFront();
                }
            }
        }

        private void BtnViewIngredients_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product product)
            {
                if (pnlProductForm != null) pnlProductForm.Visible = false;
                selectedProductId = product.Id;
                selectedProduct = product;

                if (pnlIngredientPanel != null)
                {
                    var lblProductNameControl = pnlIngredientPanel.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblProductName");
                    if (lblProductNameControl != null) lblProductNameControl.Text = $"Product: {product.Name}";
                }

                var fullProduct = dbService.GetProductById(product.Id);
                if (fullProduct != null && fullProduct.ProductIngredients != null)
                {
                    currentProductIngredients = new List<ProductIngredient>();
                    foreach (var pi in fullProduct.ProductIngredients)
                    {
                        if (pi.Ingredient == null && pi.IngredientId > 0)
                            pi.Ingredient = dbService.GetIngredientById(pi.IngredientId);
                        currentProductIngredients.Add(new ProductIngredient
                        {
                            Id = pi.Id,
                            ProductId = pi.ProductId,
                            IngredientId = pi.IngredientId,
                            Quantity = pi.Quantity,
                            QuantitySmall = pi.QuantitySmall,
                            QuantityMedium = pi.QuantityMedium,
                            QuantityLarge = pi.QuantityLarge,
                            Ingredient = pi.Ingredient
                        });
                    }
                }
                else
                {
                    currentProductIngredients = new List<ProductIngredient>();
                }

                RefreshIngredientsGrid();
                UpdateCostSummary();

                if (pnlIngredientPanel != null)
                {
                    pnlIngredientPanel.Visible = true;
                    pnlIngredientPanel.BringToFront();
                }
            }
        }

        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product product)
            {
                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete '{product.Name}'?\n\nThis product will be marked as inactive.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        dbService.DeleteProduct(product.Id);
                        MessageBox.Show($"Product '{product.Name}' has been deactivated.", "Product Deactivated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadProductCards();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ChkHasSizes_CheckedChanged(object sender, EventArgs e)
        {
            bool hasSizes = chkHasSizes?.Checked ?? false;

            if (pnlSizePricing != null) pnlSizePricing.Visible = hasSizes;

            // Enable/disable base price textbox
            if (txtProductPrice != null)
            {
                txtProductPrice.Enabled = !hasSizes;
                txtProductPrice.BackColor = hasSizes ? DeepSeekTextMuted : DeepSeekBg;
                txtProductPrice.ForeColor = hasSizes ? DeepSeekTextMuted : DeepSeekText;
                if (hasSizes)
                {
                    txtProductPrice.Text = "0.00";
                    txtProductPrice.PlaceholderText = "Not required with sizes";
                }
                else
                {
                    txtProductPrice.Text = "0.00";
                    txtProductPrice.PlaceholderText = "";
                }
            }
        }

        private void BtnSaveProduct_Click(object sender, EventArgs e)
        {
            if (txtProductName == null || cmbProductCategory == null) return;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Product name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedCategoryName = cmbProductCategory.SelectedItem?.ToString();
            var category = dbService.GetAllCategories().FirstOrDefault(c => c.Name == selectedCategoryName);

            if (category == null)
            {
                MessageBox.Show("Please select a valid category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool hasSizes = chkHasSizes?.Checked ?? false;
            decimal price = 0;

            // Only validate base price if product does NOT have sizes
            if (!hasSizes)
            {
                if (!decimal.TryParse(txtProductPrice?.Text, out price) || price <= 0)
                {
                    MessageBox.Show("Please enter a valid base price greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                // For products with sizes, use the Small price as the base price
                decimal smallPrice = 0;
                if (txtProductSmallPrice != null && decimal.TryParse(txtProductSmallPrice.Text, out smallPrice) && smallPrice > 0)
                {
                    price = smallPrice;
                }
                else
                {
                    // If Small price is not set, use Medium or Large
                    decimal mediumPrice = 0;
                    decimal largePrice = 0;
                    if (txtProductMediumPrice != null && decimal.TryParse(txtProductMediumPrice.Text, out mediumPrice) && mediumPrice > 0)
                    {
                        price = mediumPrice;
                    }
                    else if (txtProductLargePrice != null && decimal.TryParse(txtProductLargePrice.Text, out largePrice) && largePrice > 0)
                    {
                        price = largePrice;
                    }
                    else
                    {
                        MessageBox.Show("Please enter at least one valid size price (Small, Medium, or Large).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            try
            {
                // Categories that should have Cup sizes as default ingredients
                string[] cupSizeCategories = new string[] { "Classics", "Signature Coffee", "Frappe Series", "Matcha Series", "Non-Coffee" };
                bool shouldAddCupSizes = cupSizeCategories.Contains(selectedCategoryName);

                if (selectedProductId.HasValue)
                {
                    var product = dbService.GetProductById(selectedProductId.Value);
                    if (product != null)
                    {
                        product.Name = txtProductName.Text;
                        product.Price = price;
                        product.CategoryId = category.Id;
                        product.HasSizes = hasSizes;

                        if (!string.IsNullOrEmpty(selectedImagePath))
                            product.ImagePath = selectedImagePath;

                        if (hasSizes)
                        {
                            if (txtProductSmallPrice != null && decimal.TryParse(txtProductSmallPrice.Text, out decimal small))
                                product.PriceSmall = small;
                            if (txtProductMediumPrice != null && decimal.TryParse(txtProductMediumPrice.Text, out decimal medium))
                                product.PriceMedium = medium;
                            if (txtProductLargePrice != null && decimal.TryParse(txtProductLargePrice.Text, out decimal large))
                                product.PriceLarge = large;
                        }
                        else
                        {
                            product.PriceSmall = null;
                            product.PriceMedium = null;
                            product.PriceLarge = null;
                        }

                        dbService.UpdateProduct(product);
                        MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    var product = new Product
                    {
                        Name = txtProductName.Text,
                        Price = price,
                        CategoryId = category.Id,
                        IsActive = true,
                        HasSizes = hasSizes,
                        ImagePath = selectedImagePath
                    };

                    if (hasSizes)
                    {
                        if (txtProductSmallPrice != null && decimal.TryParse(txtProductSmallPrice.Text, out decimal small))
                            product.PriceSmall = small;
                        if (txtProductMediumPrice != null && decimal.TryParse(txtProductMediumPrice.Text, out decimal medium))
                            product.PriceMedium = medium;
                        if (txtProductLargePrice != null && decimal.TryParse(txtProductLargePrice.Text, out decimal large))
                            product.PriceLarge = large;
                    }

                    dbService.AddProduct(product);

                    // If product is in a category that needs Cup sizes, add them as default ingredients
                    if (shouldAddCupSizes)
                    {
                        AddDefaultCupSizes(product.Id);
                    }

                    MessageBox.Show("Product added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadProductCards();
                pnlProductForm!.Visible = false;
                ClearProductForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSelectImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Product Image";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    string imagesFolder = Path.Combine(Application.StartupPath, "Images", "Products");

                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(imagesFolder);

                    string targetPath = Path.Combine(imagesFolder, fileName);

                    // Handle duplicate filenames
                    int counter = 1;
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);

                    while (File.Exists(targetPath))
                    {
                        targetPath = Path.Combine(imagesFolder, $"{fileNameWithoutExt}_{counter}{extension}");
                        counter++;
                    }

                    File.Copy(openFileDialog.FileName, targetPath, false);
                    selectedImagePath = $"Images/Products/{Path.GetFileName(targetPath)}";

                    try
                    {
                        picProductImage!.Image = Image.FromFile(targetPath);
                        picProductImage.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch
                    {
                        picProductImage!.Image = null;
                        MessageBox.Show("Failed to load image. The file may be corrupted.", "Image Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
        private void AddDefaultCupSizes(int productId)
        {
            try
            {
                // Get the Cup size ingredients (create them if they don't exist)
                var smallCup = dbService.GetIngredientByName("Small Cup");
                var mediumCup = dbService.GetIngredientByName("Medium Cup");
                var largeCup = dbService.GetIngredientByName("Large Cup");

                // Create them if they don't exist
                if (smallCup == null)
                {
                    smallCup = new Ingredient
                    {
                        Name = "Small Cup",
                        Unit = "pcs",
                        MinimumStock = 100,
                        IsActive = true
                    };
                    dbService.AddIngredient(smallCup);
                }
                if (mediumCup == null)
                {
                    mediumCup = new Ingredient
                    {
                        Name = "Medium Cup",
                        Unit = "pcs",
                        MinimumStock = 100,
                        IsActive = true
                    };
                    dbService.AddIngredient(mediumCup);
                }
                if (largeCup == null)
                {
                    largeCup = new Ingredient
                    {
                        Name = "Large Cup",
                        Unit = "pcs",
                        MinimumStock = 100,
                        IsActive = true
                    };
                    dbService.AddIngredient(largeCup);
                }

                // Add them to the product with quantity 0
                var productIngredients = new List<ProductIngredient>();

                // Small Cup - only if product has sizes
                productIngredients.Add(new ProductIngredient
                {
                    ProductId = productId,
                    IngredientId = smallCup.Id,
                    Quantity = 0,
                    QuantitySmall = 1,
                    QuantityMedium = 0,
                    QuantityLarge = 0
                });

                // Medium Cup
                productIngredients.Add(new ProductIngredient
                {
                    ProductId = productId,
                    IngredientId = mediumCup.Id,
                    Quantity = 0,
                    QuantitySmall = 0,
                    QuantityMedium = 1,
                    QuantityLarge = 0
                });

                // Large Cup
                productIngredients.Add(new ProductIngredient
                {
                    ProductId = productId,
                    IngredientId = largeCup.Id,
                    Quantity = 0,
                    QuantitySmall = 0,
                    QuantityMedium = 0,
                    QuantityLarge = 1
                });

                dbService.SaveProductIngredients(productId, productIngredients);
            }
            catch (Exception ex)
            {
                // Log error but don't interrupt the flow
                System.Diagnostics.Debug.WriteLine($"Error adding default cup sizes: {ex.Message}");
            }
        }

        private void ClearProductForm()
        {
            if (txtProductName != null) txtProductName.Text = "";
            if (txtProductPrice != null) txtProductPrice.Text = "";
            if (txtProductSmallPrice != null) txtProductSmallPrice.Text = "0.00";
            if (txtProductMediumPrice != null) txtProductMediumPrice.Text = "0.00";
            if (txtProductLargePrice != null) txtProductLargePrice.Text = "0.00";
            if (chkHasSizes != null) chkHasSizes.Checked = false;
            if (pnlSizePricing != null) pnlSizePricing.Visible = false;
            if (picProductImage != null) picProductImage.Image = null;
            if (cmbProductCategory != null && cmbProductCategory.Items.Count > 0)
                cmbProductCategory.SelectedIndex = 0;
            selectedProductId = null;
            selectedProduct = null;
            selectedImagePath = null;
        }

        #endregion

        #region Product Management

        private void CmbIngredient_TextChanged(object sender, EventArgs e)
        {
            if (cmbIngredient == null) return;

            // Store the current text before clearing
            string searchText = cmbIngredient.Text;

            // Temporarily remove the event handler to avoid recursion
            cmbIngredient.TextChanged -= CmbIngredient_TextChanged;

            // Clear and refill items based on search
            cmbIngredient.BeginUpdate();
            cmbIngredient.Items.Clear();

            var filteredIngredients = allIngredientsList
                .Where(i => string.IsNullOrEmpty(searchText) || i.Name.ToLower().Contains(searchText.ToLower()))
                .ToList();

            foreach (var ingredient in filteredIngredients)
            {
                cmbIngredient.Items.Add(ingredient);
            }

            cmbIngredient.EndUpdate();

            // Restore the text without triggering the event again
            cmbIngredient.Text = searchText;

            // Re-attach the event handler
            cmbIngredient.TextChanged += CmbIngredient_TextChanged;

            // Set cursor position to the end
            cmbIngredient.SelectionStart = cmbIngredient.Text.Length;

            // Show dropdown if there are matches
            if (filteredIngredients.Any() && cmbIngredient.Text.Length > 0)
            {
                cmbIngredient.DroppedDown = true;
            }
        }

        private void CmbIngredient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIngredient?.SelectedItem is Ingredient selected)
            {
                // Find the unit display label in the panel
                var lblSelectedUnit = pnlIngredientPanel?.Controls
                    .OfType<Label>()
                    .FirstOrDefault(l => l.Name == "lblSelectedUnit");

                if (lblSelectedUnit != null)
                {
                    lblSelectedUnit.Text = selected.Unit ?? "g";
                }
            }
        }
        private void ShowProductManagement()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();
            currentProductIngredients = new List<ProductIngredient>();

            Label lblTitle = new Label
            {
                Text = "Menu Product",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true
            };

            SplitContainer splitContainer = new SplitContainer
            {
                Location = new Point(30, 70),
                Size = new Size(pnlContent.Width - 60, pnlContent.Height - 100),
                SplitterDistance = (pnlContent.Width - 60) * 50 / 100,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DeepSeekBorder,
                SplitterWidth = 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // ========== LEFT PANEL - PRODUCTS LIST (50%) ==========
            Panel pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekBg,
                Padding = new Padding(15)
            };

            Label lblSearch = new Label
            {
                Text = "Search",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtSearchProduct = new TextBox
            {
                Location = new Point(15, 45),
                Size = new Size(250, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search products...",
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchProduct.TextChanged += TxtSearchProduct_TextChanged!;

            Label lblFilter = new Label
            {
                Text = "Filter by Category",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(280, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            cmbCategory = new ComboBox
            {
                Location = new Point(280, 45),
                Size = new Size(180, 35),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat
            };
            cmbCategory.Items.Add("All Categories");
            LoadCategories();
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged!;

            btnNewProduct = new Button
            {
                Text = "+ Add New Product",
                Location = new Point(480, 45),
                Size = new Size(180, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnNewProduct.Click += BtnNewProduct_Click!;

            flpProducts = new FlowLayoutPanel
            {
                Location = new Point(15, 100),
                Size = new Size(splitContainer.Panel1.Width - 30, splitContainer.Panel1.Height - 120),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = DeepSeekBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            LoadProductCards();

            pnlLeft.Controls.AddRange(new Control[] {
        lblSearch, txtSearchProduct, lblFilter, cmbCategory, btnNewProduct, flpProducts
    });

            // ========== RIGHT PANEL - INGREDIENTS MANAGEMENT (50%) ==========
            Panel pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekBg,
                Padding = new Padding(15)
            };

            // ========== INGREDIENT PANEL ==========
            if (pnlIngredientPanel == null)
            {
                pnlIngredientPanel = new Panel();
            }
            else
            {
                pnlIngredientPanel.Controls.Clear();
            }

            pnlIngredientPanel.Location = new Point(15, 15);
            pnlIngredientPanel.Size = new Size(splitContainer.Panel2.Width - 30, splitContainer.Panel2.Height - 30);
            pnlIngredientPanel.BackColor = DeepSeekSurface;
            pnlIngredientPanel.BorderStyle = BorderStyle.FixedSingle;
            pnlIngredientPanel.Visible = false;
            pnlIngredientPanel.AutoScroll = true;
            pnlIngredientPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // Title
            Label lblIngredientTitle = new Label
            {
                Text = "Manage Ingredients",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };

            // Product name display
            Label lblProductNameLabel = new Label
            {
                Text = "Product: ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekAccent,
                Location = new Point(15, 50),
                AutoSize = true,
                Name = "lblProductName"
            };

            dgvProductIngredients = new DataGridView
            {
                Location = new Point(15, 90),
                Width = 800,  // Fixed width instead of auto-stretching
                Height = 300,
                BackgroundColor = DeepSeekBg,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = DeepSeekBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            dgvProductIngredients.EnableHeadersVisualStyles = false;
            dgvProductIngredients.ColumnHeadersDefaultCellStyle.BackColor = DeepSeekSidebar;
            dgvProductIngredients.ColumnHeadersDefaultCellStyle.ForeColor = DeepSeekText;
            dgvProductIngredients.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProductIngredients.ColumnHeadersHeight = 40;
            dgvProductIngredients.RowsDefaultCellStyle.BackColor = DeepSeekSurface;
            dgvProductIngredients.RowsDefaultCellStyle.ForeColor = DeepSeekText;
            dgvProductIngredients.RowsDefaultCellStyle.SelectionBackColor = DeepSeekHover;
            dgvProductIngredients.AlternatingRowsDefaultCellStyle.BackColor = DeepSeekBg;

            // Add columns
            dgvProductIngredients.Columns.Clear();
            dgvProductIngredients.Columns.Add("Ingredient", "Ingredient");
            dgvProductIngredients.Columns.Add("Unit", "Unit");
            dgvProductIngredients.Columns.Add("QuantitySmall", "Small Qty");
            dgvProductIngredients.Columns.Add("QuantityMedium", "Medium Qty");
            dgvProductIngredients.Columns.Add("QuantityLarge", "Large Qty");

            // SET EXPLICIT WIDTHS IN PIXELS
            dgvProductIngredients.Columns["Ingredient"].Width = 250;    // Make Ingredient column wider
            dgvProductIngredients.Columns["Unit"].Width = 100;
            dgvProductIngredients.Columns["QuantitySmall"].Width = 120;
            dgvProductIngredients.Columns["QuantityMedium"].Width = 120;
            dgvProductIngredients.Columns["QuantityLarge"].Width = 120;
            dgvProductIngredients.CellClick += DgvProductIngredients_CellClick!;
            // GroupBox for adding ingredients
            GroupBox grpSetIngredient = new GroupBox
            {
                Text = "Set Ingredient Quantities by Size",
                Location = new Point(15, 410),
                Width = 550,
                Height = 150,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekText,
                BackColor = Color.Transparent
            };

            // Row 1: Ingredient selection
            Label lblIngredient = new Label
            {
                Text = "Ingredient:",
                Location = new Point(15, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = DeepSeekTextMuted
            };
            cmbIngredient = new ComboBox
            {
                Location = new Point(100, 27),
                Width = 220,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 10),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            // ADD EVENT HANDLERS HERE


            cmbIngredient.TextChanged += CmbIngredient_TextChanged;
            cmbIngredient.SelectedIndexChanged += CmbIngredient_SelectedIndexChanged;

            LoadIngredientCombo();


            Label lblUnitDisplay = new Label
            {
                Text = "Unit:",
                Location = new Point(340, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = DeepSeekTextMuted
            };
            Label lblSelectedUnit = new Label
            {
                Text = "g",
                Location = new Point(380, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DeepSeekAccent
            };

            cmbIngredient.SelectedIndexChanged += (s, ev) =>
            {
                if (cmbIngredient.SelectedItem is Ingredient ing)
                {
                    lblSelectedUnit.Text = ing.Unit ?? "g";
                }
            };

            // Row 2: Quantities
            Label lblSmallQty = new Label
            {
                Text = "Small Qty:",
                Location = new Point(15, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = DeepSeekTextMuted
            };
            nudSmallQty = new NumericUpDown
            {
                Location = new Point(100, 67),
                Width = 100,
                Minimum = 0,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = 0,
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            Label lblMediumQty = new Label
            {
                Text = "Medium Qty:",
                Location = new Point(220, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = DeepSeekTextMuted
            };
            nudMediumQty = new NumericUpDown
            {
                Location = new Point(310, 67),
                Width = 100,
                Minimum = 0,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = 0,
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            Label lblLargeQty = new Label
            {
                Text = "Large Qty:",
                Location = new Point(430, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = DeepSeekTextMuted
            };
            nudLargeQty = new NumericUpDown
            {
                Location = new Point(520, 67),
                Width = 100,
                Minimum = 0,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = 0,
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            // Row 3: Buttons
            btnSetIngredient = new Button
            {
                Text = "Set",
                Location = new Point(15, 110),
                Width = 80,
                Height = 30,
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnSetIngredient.Click += BtnSetIngredient_Click!;

            btnRemoveIngredient = new Button
            {
                Text = "Remove",
                Location = new Point(105, 110),
                Width = 80,
                Height = 30,
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnRemoveIngredient.Click += BtnRemoveIngredient_Click!;

            btnClearIngredients = new Button
            {
                Text = "Clear All",
                Location = new Point(195, 110),
                Width = 80,
                Height = 30,
                BackColor = DeepSeekWarning,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnClearIngredients.Click += BtnClearIngredients_Click!;

            grpSetIngredient.Controls.AddRange(new Control[] {
        lblIngredient, cmbIngredient, lblUnitDisplay, lblSelectedUnit,
        lblSmallQty, nudSmallQty,
        lblMediumQty, nudMediumQty,
        lblLargeQty, nudLargeQty,
        btnSetIngredient, btnRemoveIngredient, btnClearIngredients
    });

            // Save and Cancel buttons
            btnSaveIngredients = new Button
            {
                Text = "Save Ingredients",
                Location = new Point(15, 580),
                Width = 140,
                Height = 40,
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnSaveIngredients.Click += BtnSaveIngredients_Click!;

            btnCancelIngredients = new Button
            {
                Text = "Close",
                Location = new Point(170, 580),
                Width = 100,
                Height = 40,
                BackColor = DeepSeekTextMuted,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancelIngredients.Click += (s, e) => { pnlIngredientPanel.Visible = false; ClearIngredientSelection(); };

            // Add all controls to panel
            pnlIngredientPanel.Controls.Add(lblIngredientTitle);
            pnlIngredientPanel.Controls.Add(lblProductNameLabel);
            pnlIngredientPanel.Controls.Add(dgvProductIngredients);
            pnlIngredientPanel.Controls.Add(grpSetIngredient);
            pnlIngredientPanel.Controls.Add(btnSaveIngredients);
            pnlIngredientPanel.Controls.Add(btnCancelIngredients);

            // ========== PRODUCT FORM PANEL ==========
            if (pnlProductForm == null)
            {
                pnlProductForm = new Panel();
            }
            else
            {
                pnlProductForm.Controls.Clear();
            }

            pnlProductForm.Location = new Point(15, 15);
            pnlProductForm.Size = new Size(splitContainer.Panel2.Width - 30, splitContainer.Panel2.Height - 30);
            pnlProductForm.BackColor = DeepSeekSurface;
            pnlProductForm.BorderStyle = BorderStyle.FixedSingle;
            pnlProductForm.Visible = false;
            pnlProductForm.AutoScroll = true;
            pnlProductForm.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            Label lblProductFormTitle = new Label
            {
                Text = "Product Details",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };

            // Product Name
            Label lblProdName = new Label
            {
                Text = "Product Name:",
                Location = new Point(15, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            txtProductName = new TextBox
            {
                Location = new Point(140, 57),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Category
            Label lblProdCategory = new Label
            {
                Text = "Category:",
                Location = new Point(15, 110),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            cmbProductCategory = new ComboBox
            {
                Location = new Point(140, 107),
                Size = new Size(200, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText
            };
            LoadCategoryCombo();

            chkHasSizes = new CheckBox
            {
                Text = "This product has size variants (Small, Medium, Large)",
                Location = new Point(15, 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Checked = false
            };
            chkHasSizes.CheckedChanged += (s, e) =>
            {
                bool hasSizes = chkHasSizes.Checked;

                // Show/hide size pricing panel
                if (pnlSizePricing != null) pnlSizePricing.Visible = hasSizes;

                // Enable/disable base price textbox
                if (txtProductPrice != null)
                {
                    txtProductPrice.Enabled = !hasSizes;
                    txtProductPrice.BackColor = hasSizes ? DeepSeekTextMuted : DeepSeekBg;
                    txtProductPrice.ForeColor = hasSizes ? DeepSeekTextMuted : DeepSeekText;
                    txtProductPrice.Text = hasSizes ? "0.00" : "0.00";
                }

                // Add a visual indicator
                if (hasSizes)
                {
                    // Optional: Add a tooltip or label to inform user
                    // You can also change the label color
                }
            };

            // Size Pricing Panel
            pnlSizePricing = new Panel
            {
                Location = new Point(15, 195),
                Size = new Size(550, 80),
                BackColor = DeepSeekBg,
                Visible = false
            };

            Label lblSmallPrice = new Label { Text = "Small Price:", Location = new Point(15, 15), AutoSize = true, ForeColor = DeepSeekTextMuted };
            txtProductSmallPrice = new TextBox { Location = new Point(110, 12), Size = new Size(100, 30), Text = "0.00", BackColor = DeepSeekSurface, ForeColor = DeepSeekText };

            Label lblMediumPrice = new Label { Text = "Medium Price:", Location = new Point(230, 15), AutoSize = true, ForeColor = DeepSeekTextMuted };
            txtProductMediumPrice = new TextBox { Location = new Point(340, 12), Size = new Size(100, 30), Text = "0.00", BackColor = DeepSeekSurface, ForeColor = DeepSeekText };

            Label lblLargePrice = new Label { Text = "Large Price:", Location = new Point(15, 50), AutoSize = true, ForeColor = DeepSeekTextMuted };
            txtProductLargePrice = new TextBox { Location = new Point(110, 47), Size = new Size(100, 30), Text = "0.00", BackColor = DeepSeekSurface, ForeColor = DeepSeekText };

            lblBasePriceNote = new Label
            {
                Text = "Base price will be used for products without sizes",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(230, 50),
                AutoSize = true
            };

            pnlSizePricing.Controls.AddRange(new Control[] {
        lblSmallPrice, txtProductSmallPrice,
        lblMediumPrice, txtProductMediumPrice,
        lblLargePrice, txtProductLargePrice,
        lblBasePriceNote
    });

            // Base Price
            Label lblProdPrice = new Label
            {
                Text = "Base Price:",
                Location = new Point(15, 290),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            txtProductPrice = new TextBox
            {
                Location = new Point(140, 287),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                Text = "0.00"
            };

            // Product Image
            Label lblImage = new Label
            {
                Text = "Product Image:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 340),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            picProductImage = new PictureBox
            {
                Location = new Point(140, 330),
                Size = new Size(80, 80),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnSelectImage = new Button
            {
                Text = "Browse...",
                Location = new Point(230, 355),
                Size = new Size(80, 30),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnSelectImage.Click += BtnSelectImage_Click!;

            // Buttons
            btnSaveProduct = new Button
            {
                Text = "SAVE PRODUCT",
                Location = new Point(140, 430),
                Size = new Size(150, 45),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnSaveProduct.Click += BtnSaveProduct_Click!;

            btnCancelProduct = new Button
            {
                Text = "Cancel",
                Location = new Point(310, 430),
                Size = new Size(120, 45),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancelProduct.Click += (s, e) =>
            {
                pnlProductForm.Visible = false;
                ClearProductForm();
            };

            pnlProductForm.Controls.AddRange(new Control[] {
        lblProductFormTitle,
        lblProdName, txtProductName,
        lblProdCategory, cmbProductCategory,
        chkHasSizes, pnlSizePricing,
        lblProdPrice, txtProductPrice,
        lblImage, picProductImage, btnSelectImage,
        btnSaveProduct, btnCancelProduct
    });

            pnlRight.Controls.Add(pnlProductForm);
            pnlRight.Controls.Add(pnlIngredientPanel);

            splitContainer.Panel1.Controls.Add(pnlLeft);
            splitContainer.Panel2.Controls.Add(pnlRight);

            pnlContent.Controls.Add(lblTitle);
            pnlContent.Controls.Add(splitContainer);

            pnlContent.Resize += (s, e) =>
            {
                // Ensure we have valid dimensions before setting
                int newWidth = pnlContent.Width - 60;
                int newHeight = pnlContent.Height - 100;

                if (newWidth > 0 && newHeight > 0)
                {
                    splitContainer.Size = new Size(newWidth, newHeight);

                    int splitterDist = newWidth * 50 / 100;
                    if (splitterDist > 0 && splitterDist < splitContainer.Width)
                    {
                        splitContainer.SplitterDistance = splitterDist;
                    }
                }

                if (flpProducts != null && splitContainer.Panel1.Width > 0)
                {
                    flpProducts.Size = new Size(splitContainer.Panel1.Width - 30, splitContainer.Panel1.Height - 120);
                }

                if (pnlIngredientPanel != null && splitContainer.Panel2.Width > 0)
                {
                    pnlIngredientPanel.Size = new Size(splitContainer.Panel2.Width - 30, splitContainer.Panel2.Height - 30);

                    if (dgvProductIngredients != null && pnlIngredientPanel.Width > 0)
                    {
                        dgvProductIngredients.Width = pnlIngredientPanel.Width - 30;
                    }

                    if (grpSetIngredient != null && pnlIngredientPanel.Width > 0)
                    {
                        grpSetIngredient.Width = pnlIngredientPanel.Width - 30;
                    }
                }

                if (pnlProductForm != null && splitContainer.Panel2.Width > 0)
                {
                    pnlProductForm.Size = new Size(splitContainer.Panel2.Width - 30, splitContainer.Panel2.Height - 30);
                }
            };
        }

        #endregion

            #region PDF Export

        private void SetupPDFExportButtons()
        {
            // Add PDF export buttons to Sales Report tab
            if (pnlDateRange != null && btnGenerateReport != null)
            {
                btnExportSalesPDF = new Button
                {
                    Text = "📄 Export PDF",
                    Location = new Point(btnGenerateReport.Location.X + btnGenerateReport.Width + 10, btnGenerateReport.Location.Y),
                    Size = new Size(130, 35),
                    BackColor = DeepSeekDanger,
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatAppearance = { BorderSize = 0 }
                };
                btnExportSalesPDF.Click += BtnExportSalesPDF_Click!;
                pnlDateRange.Controls.Add(btnExportSalesPDF);
            }
        }

        private async void BtnExportSalesPDF_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtpReportStart == null || dtpReportEnd == null)
                {
                    MessageBox.Show("Please select a date range first.", "No Date Range",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;
                btnExportSalesPDF!.Enabled = false;
                btnExportSalesPDF.Text = "Generating...";

                DateTime startDate = dtpReportStart.Value.Date;
                DateTime endDate = dtpReportEnd.Value.Date.AddDays(1).AddSeconds(-1);

                // Gather data
                var allTransactions = dbService.GetAllTransactions();
                var transactions = allTransactions
                    .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Status == "Completed")
                    .ToList();

                decimal totalSales = transactions.Sum(t => t.Total);
                int totalItemsSold = transactions.Sum(t => t.TransactionItems?.Sum(ti => ti.Quantity) ?? 0);
                int totalTransactions = transactions.Count;

                // Build product performance data
                var productPerformanceDict = new Dictionary<int, (string Name, int Quantity, decimal Revenue)>();
                foreach (var transaction in transactions)
                {
                    if (transaction.TransactionItems != null)
                    {
                        foreach (var item in transaction.TransactionItems)
                        {
                            string productName = item.Product?.Name ?? "Unknown Product";
                            if (productPerformanceDict.ContainsKey(item.ProductId))
                            {
                                var existing = productPerformanceDict[item.ProductId];
                                productPerformanceDict[item.ProductId] = (productName, existing.Quantity + item.Quantity, existing.Revenue + (item.UnitPrice * item.Quantity));
                            }
                            else
                            {
                                productPerformanceDict[item.ProductId] = (productName, item.Quantity, item.UnitPrice * item.Quantity);
                            }
                        }
                    }
                }

                decimal totalRevenue = productPerformanceDict.Sum(p => p.Value.Revenue);
                var productPerformanceList = new List<SalesProductPerformance>();
                foreach (var p in productPerformanceDict.OrderByDescending(x => x.Value.Revenue))
                {
                    decimal percentOfSales = totalRevenue > 0 ? (p.Value.Revenue / totalRevenue) * 100 : 0;
                    string performance = percentOfSales >= 20 ? "Best Seller" : percentOfSales >= 10 ? "Good Seller" : "Slow Mover";

                    productPerformanceList.Add(new SalesProductPerformance
                    {
                        ProductName = p.Value.Name,
                        UnitsSold = p.Value.Quantity,
                        Revenue = p.Value.Revenue,
                        PercentOfSales = percentOfSales,
                        PerformanceRating = performance
                    });
                }

                // Build chart data
                var chartData = new List<SalesChartDataPoint>();
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    decimal dailySales = transactions.Where(t => t.TransactionDate.Date == date.Date).Sum(t => t.Total);
                    chartData.Add(new SalesChartDataPoint { Date = date, Sales = dailySales });
                }

                // Generate PDF
                var pdfService = new PDFExportService();
                byte[] pdfBytes = pdfService.GenerateSalesReportPDF(
                    startDate, endDate, totalSales, totalItemsSold, totalTransactions,
                    productPerformanceList, chartData);

                // Save file dialog
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF Files|*.pdf";
                    saveDialog.FileName = $"Sales_Report_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.pdf";
                    saveDialog.Title = "Save Sales Report";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
                        MessageBox.Show($"Sales report saved successfully!\n\nLocation: {saveDialog.FileName}",
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Ask if user wants to open the file
                        DialogResult openResult = MessageBox.Show("Would you like to open the PDF file?", "Open File",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (openResult == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                if (btnExportSalesPDF != null)
                {
                    btnExportSalesPDF.Enabled = true;
                    btnExportSalesPDF.Text = "📄 Export PDF";
                }
            }
        }

        private async void BtnExportInventoryPDF_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnExportInventoryPDF!.Enabled = false;
                btnExportInventoryPDF.Text = "Generating...";

                // Gather inventory data
                var ingredients = dbService.GetAllIngredients().Where(i => i.IsActive).ToList();
                var inventory = dbService.GetAllInventory();

                int totalIngredients = ingredients.Count;
                int lowStockCount = 0;
                int outOfStockCount = 0;

                var inventoryItems = new List<InventoryItemReport>();

                foreach (var ingredient in ingredients)
                {
                    var stock = inventory.FirstOrDefault(i => i.IngredientId == ingredient.Id);
                    decimal currentStock = stock?.Quantity ?? 0;

                    string status = currentStock <= 0 ? "Out of Stock" :
                                   currentStock < ingredient.MinimumStock ? "Low Stock" : "In Stock";

                    if (status == "Low Stock") lowStockCount++;
                    if (status == "Out of Stock") outOfStockCount++;

                    inventoryItems.Add(new InventoryItemReport
                    {
                        IngredientName = ingredient.Name,
                        CurrentStock = currentStock,
                        Unit = ingredient.Unit ?? "g",
                        MinimumStock = ingredient.MinimumStock,
                        Status = status
                    });
                }

                // Generate PDF
                var pdfService = new PDFExportService();
                byte[] pdfBytes = pdfService.GenerateInventoryReportPDF(
                    inventoryItems, totalIngredients, lowStockCount, outOfStockCount);

                // Save file dialog
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF Files|*.pdf";
                    saveDialog.FileName = $"Inventory_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    saveDialog.Title = "Save Inventory Report";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
                        MessageBox.Show($"Inventory report saved successfully!\n\nLocation: {saveDialog.FileName}",
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Ask if user wants to open the file
                        DialogResult openResult = MessageBox.Show("Would you like to open the PDF file?", "Open File",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (openResult == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                if (btnExportInventoryPDF != null)
                {
                    btnExportInventoryPDF.Enabled = true;
                    btnExportInventoryPDF.Text = "📄 Export PDF";
                }
            }
        }

        #endregion

        #region Quick Report Actions

        // Sales Report Quick Actions
        private void BtnDailySalesReport_Click(object sender, EventArgs e)
        {
            if (dtpReportStart == null || dtpReportEnd == null) return;

            // Set to today's date
            dtpReportStart.Value = DateTime.Today;
            dtpReportEnd.Value = DateTime.Today;

            // Generate the report
            LoadSalesReport();

            // Refresh the chart
            if (pnlSalesChart != null)
            {
                pnlSalesChart.Invalidate();
            }

            MessageBox.Show($"Showing sales report for {DateTime.Today:MMMM dd, yyyy}",
                "Daily Sales Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnMonthlySalesReport_Click(object sender, EventArgs e)
        {
            if (dtpReportStart == null || dtpReportEnd == null) return;

            // Show month selection dialog
            using (Form monthDialog = CreateMonthSelectionDialog())
            {
                if (monthDialog.ShowDialog() == DialogResult.OK)
                {
                    DateTime selectedMonth = (DateTime)monthDialog.Tag;
                    DateTime startOfMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
                    DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                    dtpReportStart.Value = startOfMonth;
                    dtpReportEnd.Value = endOfMonth;

                    // Generate the report
                    LoadSalesReport();

                    // Refresh the chart
                    if (pnlSalesChart != null)
                    {
                        pnlSalesChart.Invalidate();
                    }

                    MessageBox.Show($"Showing sales report for {startOfMonth:MMMM yyyy}",
                        "Monthly Sales Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Inventory Report Quick Actions
        private void BtnDailyInventoryReport_Click(object sender, EventArgs e)
        {
            // For inventory, "today's inventory" just shows current inventory
            // with a note about the date
            LoadInventoryReport();

            // Add a timestamp label to indicate this is current inventory
            if (pnlInventoryContainer != null)
            {
                // Find or create timestamp label
                Label? timestampLabel = pnlInventoryContainer.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Name == "lblInventoryTimestamp");

                if (timestampLabel == null)
                {
                    timestampLabel = new Label
                    {
                        Name = "lblInventoryTimestamp",
                        Font = new Font("Segoe UI", 9, FontStyle.Italic),
                        ForeColor = DeepSeekTextMuted,
                        Location = new Point(15, pnlInventoryContainer.Height - 25),
                        AutoSize = true,
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                    };
                    pnlInventoryContainer.Controls.Add(timestampLabel);
                }

                timestampLabel.Text = $"Inventory as of {DateTime.Now:MMMM dd, yyyy HH:mm:ss}";
                timestampLabel.BringToFront();
            }

            MessageBox.Show($"Showing current inventory status as of {DateTime.Now:MMMM dd, yyyy HH:mm:ss}",
                "Daily Inventory Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnMonthlyInventoryReport_Click(object sender, EventArgs e)
        {
            // Show month selection dialog for inventory snapshot
            using (Form monthDialog = CreateMonthSelectionDialog())
            {
                if (monthDialog.ShowDialog() == DialogResult.OK)
                {
                    DateTime selectedMonth = (DateTime)monthDialog.Tag;

                    // For monthly inventory, we show a historical snapshot
                    // You can implement this to show inventory levels at the end of that month
                    // For now, we'll show current inventory with a note
                    LoadInventoryReport();

                    // Add a timestamp label indicating this is a snapshot
                    if (pnlInventoryContainer != null)
                    {
                        Label? timestampLabel = pnlInventoryContainer.Controls.OfType<Label>()
                            .FirstOrDefault(l => l.Name == "lblInventoryTimestamp");

                        if (timestampLabel == null)
                        {
                            timestampLabel = new Label
                            {
                                Name = "lblInventoryTimestamp",
                                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                                ForeColor = DeepSeekTextMuted,
                                Location = new Point(15, pnlInventoryContainer.Height - 25),
                                AutoSize = true,
                                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                            };
                            pnlInventoryContainer.Controls.Add(timestampLabel);
                        }

                        timestampLabel.Text = $"Inventory snapshot for {selectedMonth:MMMM yyyy} (as of current data)";
                        timestampLabel.BringToFront();
                    }

                    MessageBox.Show($"Showing inventory report for {selectedMonth:MMMM yyyy}",
                        "Monthly Inventory Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Helper method to create month selection dialog
        private Form CreateMonthSelectionDialog()
        {
            Form dialog = new Form
            {
                Text = "Select Month",
                Size = new Size(300, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = DeepSeekSurface
            };

            // Month selection using MonthCalendar
            MonthCalendar monthCalendar = new MonthCalendar
            {
                Location = new Point(20, 20),
                Size = new Size(240, 160),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                TitleBackColor = DeepSeekAccent,
                TitleForeColor = DeepSeekText
            };

            // Show only one month at a time and remove week numbers
            monthCalendar.CalendarDimensions = new Size(1, 1);
            monthCalendar.ShowWeekNumbers = false;

            // Set to current month
            monthCalendar.SetDate(DateTime.Now);

            Button btnOK = new Button
            {
                Text = "OK",
                Location = new Point(60, 130),
                Size = new Size(80, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(150, 130),
                Size = new Size(80, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            dialog.Controls.Add(monthCalendar);
            dialog.Controls.Add(btnOK);
            dialog.Controls.Add(btnCancel);

            dialog.Tag = monthCalendar.SelectionStart;

            monthCalendar.DateSelected += (s, ev) =>
            {
                dialog.Tag = ev.Start;
            };

            return dialog;
        }

        // Add this method to handle date range validation for monthly reports
        private void DtpReportStart_ValueChanged(object sender, EventArgs e)
        {
            if (dtpReportStart != null && dtpReportEnd != null)
            {
                // Optional: Add validation to prevent selecting dates beyond end date
                if (dtpReportStart.Value > dtpReportEnd.Value)
                {
                    dtpReportEnd.Value = dtpReportStart.Value;
                }
            }
        }

        private void DtpReportEnd_ValueChanged(object sender, EventArgs e)
        {
            if (dtpReportStart != null && dtpReportEnd != null)
            {
                // Optional: Add validation to prevent selecting dates before start date
                if (dtpReportEnd.Value < dtpReportStart.Value)
                {
                    dtpReportStart.Value = dtpReportEnd.Value;
                }
            }
        }

        #endregion

        #region Month Selection Helper

        private (DateTime startDate, DateTime endDate)? ShowMonthSelector(string title)
        {
            Form dialog = new Form
            {
                Text = title,
                Size = new Size(420, 270),  // Increased width further
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = DeepSeekSurface
            };

            // Year selection
            Label lblYear = new Label
            {
                Text = "Select Year:",
                Location = new Point(25, 25),
                Size = new Size(90, 25),  // Increased width
                ForeColor = DeepSeekText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            NumericUpDown nudYear = new NumericUpDown
            {
                Location = new Point(125, 22),  // Moved right
                Size = new Size(110, 28),
                Minimum = 2020,
                Maximum = DateTime.Now.Year + 5,
                Value = DateTime.Now.Year,
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                Font = new Font("Segoe UI", 11)
            };

            // Month selection - with better spacing
            Label lblMonth = new Label
            {
                Text = "Select Month:",
                Location = new Point(25, 65),
                Size = new Size(90, 25),  // Increased width
                ForeColor = DeepSeekText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            ComboBox cmbMonth = new ComboBox
            {
                Location = new Point(125, 62),  // Moved right to align with year dropdown
                Size = new Size(180, 28),  // Increased width for full month names
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                Font = new Font("Segoe UI", 11)
            };

            string[] months = { "January", "February", "March", "April", "May", "June",
                        "July", "August", "September", "October", "November", "December" };
            cmbMonth.Items.AddRange(months);
            cmbMonth.SelectedIndex = DateTime.Now.Month - 1;

            // Preview panel with date range
            Panel pnlPreview = new Panel
            {
                Location = new Point(25, 110),
                Size = new Size(360, 55),
                BackColor = DeepSeekBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPreviewTitle = new Label
            {
                Text = "Date Range:",
                Location = new Point(10, 8),
                Size = new Size(80, 20),
                ForeColor = DeepSeekTextMuted,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            Label lblPreview = new Label
            {
                Location = new Point(10, 30),
                Size = new Size(340, 20),
                ForeColor = DeepSeekAccent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            pnlPreview.Controls.AddRange(new Control[] { lblPreviewTitle, lblPreview });

            // Update preview when selection changes
            void UpdatePreview()
            {
                int year = (int)nudYear.Value;
                int month = cmbMonth.SelectedIndex + 1;
                DateTime startDate = new DateTime(year, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                lblPreview.Text = $"{startDate:MMMM dd, yyyy} - {endDate:MMMM dd, yyyy}";
            }

            nudYear.ValueChanged += (s, e) => UpdatePreview();
            cmbMonth.SelectedIndexChanged += (s, e) => UpdatePreview();
            UpdatePreview();

            // Buttons panel
            Panel pnlButtons = new Panel
            {
                Location = new Point(25, 180),
                Size = new Size(360, 40),
                BackColor = Color.Transparent
            };

            Button btnOK = new Button
            {
                Text = "Export PDF",
                Location = new Point(80, 5),
                Size = new Size(110, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                DialogResult = DialogResult.OK,
                Cursor = Cursors.Hand
            };
            btnOK.MouseEnter += (s, e) => btnOK.BackColor = Color.FromArgb(36, 205, 149);
            btnOK.MouseLeave += (s, e) => btnOK.BackColor = DeepSeekSuccess;

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(200, 5),
                Size = new Size(110, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = Color.FromArgb(249, 88, 88);
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = DeepSeekDanger;

            pnlButtons.Controls.AddRange(new Control[] { btnOK, btnCancel });

            dialog.Controls.AddRange(new Control[] {
        lblYear, nudYear, lblMonth, cmbMonth,
        pnlPreview, pnlButtons
    });

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int year = (int)nudYear.Value;
                int month = cmbMonth.SelectedIndex + 1;
                DateTime startDate = new DateTime(year, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                return (startDate, endDate);
            }

            return null;
        }

        #endregion

        #region PDF Export - Quick Actions

        // Sales Report PDF Exports
        private async void BtnExportDailySalesPDF_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnDailySalesReport!.Enabled = false;
                btnDailySalesReport.Text = "Generating...";

                DateTime today = DateTime.Today;
                DateTime startDate = today;
                DateTime endDate = today.AddDays(1).AddSeconds(-1);

                await ExportSalesReportToPDF(startDate, endDate, $"Daily_Sales_{today:yyyyMMdd}");

                MessageBox.Show($"Daily sales report for {today:MMMM dd, yyyy} exported successfully!",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting daily sales report: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                if (btnDailySalesReport != null)
                {
                    btnDailySalesReport.Enabled = true;
                    btnDailySalesReport.Text = "📄 Export Today's Sales";
                }
            }
        }

        private async void BtnExportMonthlySalesPDF_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedMonth = ShowMonthSelector("Select Month for Sales Report");
                if (!selectedMonth.HasValue) return;

                Cursor = Cursors.WaitCursor;
                btnMonthlySalesReport!.Enabled = false;
                btnMonthlySalesReport.Text = "Generating...";

                var (startDate, endDate) = selectedMonth.Value;
                string fileName = $"Monthly_Sales_{startDate:yyyy_MM}";

                await ExportSalesReportToPDF(startDate, endDate, fileName);

                MessageBox.Show($"Monthly sales report for {startDate:MMMM yyyy} exported successfully!",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting monthly sales report: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                if (btnMonthlySalesReport != null)
                {
                    btnMonthlySalesReport.Enabled = true;
                    btnMonthlySalesReport.Text = "📄 Export Monthly Sales";
                }
            }
        }

        // Inventory Report PDF Exports
        private async void BtnExportCurrentInventoryPDF_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnDailyInventoryReport!.Enabled = false;
                btnDailyInventoryReport.Text = "Generating...";

                string fileName = $"Current_Inventory_{DateTime.Now:yyyyMMdd_HHmmss}";
                await ExportInventoryReportToPDF(fileName);

                MessageBox.Show($"Current inventory report exported successfully!\n\nAs of: {DateTime.Now:MMMM dd, yyyy HH:mm:ss}",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting inventory report: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                if (btnDailyInventoryReport != null)
                {
                    btnDailyInventoryReport.Enabled = true;
                    btnDailyInventoryReport.Text = "📄 Export Current Inventory";
                }
            }
        }

        private async void BtnExportMonthlyInventoryPDF_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedMonth = ShowMonthSelector("Select Month for Inventory Snapshot");
                if (!selectedMonth.HasValue) return;

                Cursor = Cursors.WaitCursor;
                btnMonthlyInventoryReport!.Enabled = false;
                btnMonthlyInventoryReport.Text = "Generating...";

                var (startDate, endDate) = selectedMonth.Value;
                string fileName = $"Monthly_Inventory_{startDate:yyyy_MM}";

                await ExportInventoryReportToPDF(fileName);

                MessageBox.Show($"Monthly inventory snapshot for {startDate:MMMM yyyy} exported successfully!\n\nNote: Shows current inventory levels.",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting monthly inventory report: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                if (btnMonthlyInventoryReport != null)
                {
                    btnMonthlyInventoryReport.Enabled = true;
                    btnMonthlyInventoryReport.Text = "📄 Export Monthly Snapshot";
                }
            }
        }

        // Core export methods
        private async Task ExportSalesReportToPDF(DateTime startDate, DateTime endDate, string fileName)
        {
            // Get data
            var allTransactions = dbService.GetAllTransactions();
            var transactions = allTransactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Status == "Completed")
                .ToList();

            decimal totalSales = transactions.Sum(t => t.Total);
            int totalItemsSold = transactions.Sum(t => t.TransactionItems?.Sum(ti => ti.Quantity) ?? 0);
            int totalTransactions = transactions.Count;

            // Build product performance data
            var productPerformanceDict = new Dictionary<int, (string Name, int Quantity, decimal Revenue)>();
            foreach (var transaction in transactions)
            {
                if (transaction.TransactionItems != null)
                {
                    foreach (var item in transaction.TransactionItems)
                    {
                        string productName = item.Product?.Name ?? "Unknown Product";
                        if (productPerformanceDict.ContainsKey(item.ProductId))
                        {
                            var existing = productPerformanceDict[item.ProductId];
                            productPerformanceDict[item.ProductId] = (productName, existing.Quantity + item.Quantity, existing.Revenue + (item.UnitPrice * item.Quantity));
                        }
                        else
                        {
                            productPerformanceDict[item.ProductId] = (productName, item.Quantity, item.UnitPrice * item.Quantity);
                        }
                    }
                }
            }

            decimal totalRevenue = productPerformanceDict.Sum(p => p.Value.Revenue);
            var productPerformanceList = new List<SalesProductPerformance>();
            foreach (var p in productPerformanceDict.OrderByDescending(x => x.Value.Revenue))
            {
                decimal percentOfSales = totalRevenue > 0 ? (p.Value.Revenue / totalRevenue) * 100 : 0;
                string performance = percentOfSales >= 20 ? "Best Seller" : percentOfSales >= 10 ? "Good Seller" : "Slow Mover";

                productPerformanceList.Add(new SalesProductPerformance
                {
                    ProductName = p.Value.Name,
                    UnitsSold = p.Value.Quantity,
                    Revenue = p.Value.Revenue,
                    PercentOfSales = percentOfSales,
                    PerformanceRating = performance
                });
            }

            // Build chart data
            var chartData = new List<SalesChartDataPoint>();
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                decimal dailySales = transactions.Where(t => t.TransactionDate.Date == date.Date).Sum(t => t.Total);
                chartData.Add(new SalesChartDataPoint { Date = date, Sales = dailySales });
            }

            // Generate PDF
            var pdfService = new PDFExportService();
            byte[] pdfBytes = pdfService.GenerateSalesReportPDF(
                startDate, endDate, totalSales, totalItemsSold, totalTransactions,
                productPerformanceList, chartData);

            // Save file
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PDF Files|*.pdf";
                saveDialog.FileName = $"{fileName}.pdf";
                saveDialog.Title = "Save Sales Report";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);

                    // Ask if user wants to open the file
                    DialogResult openResult = MessageBox.Show("Would you like to open the PDF file?", "Open File",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (openResult == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
        }

        private async Task ExportInventoryReportToPDF(string fileName)
        {
            // Gather inventory data
            var ingredients = dbService.GetAllIngredients().Where(i => i.IsActive).ToList();
            var inventory = dbService.GetAllInventory();

            int totalIngredients = ingredients.Count;
            int lowStockCount = 0;
            int outOfStockCount = 0;

            var inventoryItems = new List<InventoryItemReport>();

            foreach (var ingredient in ingredients)
            {
                var stock = inventory.FirstOrDefault(i => i.IngredientId == ingredient.Id);
                decimal currentStock = stock?.Quantity ?? 0;

                string status = currentStock <= 0 ? "Out of Stock" :
                               currentStock < ingredient.MinimumStock ? "Low Stock" : "In Stock";

                if (status == "Low Stock") lowStockCount++;
                if (status == "Out of Stock") outOfStockCount++;

                inventoryItems.Add(new InventoryItemReport
                {
                    IngredientName = ingredient.Name,
                    CurrentStock = currentStock,
                    Unit = ingredient.Unit ?? "g",
                    MinimumStock = ingredient.MinimumStock,
                    Status = status
                });
            }

            // Generate PDF
            var pdfService = new PDFExportService();
            byte[] pdfBytes = pdfService.GenerateInventoryReportPDF(
                inventoryItems, totalIngredients, lowStockCount, outOfStockCount);

            // Save file
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PDF Files|*.pdf";
                saveDialog.FileName = $"{fileName}.pdf";
                saveDialog.Title = "Save Inventory Report";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);

                    // Ask if user wants to open the file
                    DialogResult openResult = MessageBox.Show("Would you like to open the PDF file?", "Open File",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (openResult == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
        }

        #endregion

        #region Order Cancellation Module

        private void ShowCancellationModule()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            // Header
            Panel pnlHeader = new Panel
            {
                Location = new Point(30, 20),
                Size = new Size(pnlContent.Width - 60, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblIcon = new Label
            {
                Text = "🚫",
                Font = new Font("Segoe UI", 28, FontStyle.Regular),
                ForeColor = DeepSeekDanger,
                Location = new Point(0, 5),
                AutoSize = true
            };

            Label lblTitle = new Label
            {
                Text = "Order Cancellations",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(60, 15),
                AutoSize = true
            };

            pnlHeader.Controls.AddRange(new Control[] { lblIcon, lblTitle });

            // Create TabControl for Active Orders and Archived/Cancelled
            TabControl tcCancellations = new TabControl
            {
                Location = new Point(30, 90),
                Size = new Size(pnlContent.Width - 60, pnlContent.Height - 120),
                Font = new Font("Segoe UI", 11),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Tab 1: Active Orders (For Cancellation)
            TabPage tpActiveOrders = new TabPage("  📋  Active Orders  ")
            {
                BackColor = DeepSeekBg,
                AutoScroll = true,
                Padding = new Padding(15)
            };
            SetupActiveOrdersPanel(tpActiveOrders);

            // Tab 2: Cancelled/Archived Orders
            TabPage tpCancelledOrders = new TabPage("  🗑️  Cancelled Orders  ")
            {
                BackColor = DeepSeekBg,
                AutoScroll = true,
                Padding = new Padding(15)
            };
            SetupCancelledOrdersPanel(tpCancelledOrders);

            tcCancellations.TabPages.Add(tpActiveOrders);
            tcCancellations.TabPages.Add(tpCancelledOrders);

            pnlContent.Controls.Add(pnlHeader);
            pnlContent.Controls.Add(tcCancellations);

            pnlContent.Resize += (s, e) =>
            {
                pnlHeader.Width = pnlContent.Width - 60;
                tcCancellations.Size = new Size(pnlContent.Width - 60, pnlContent.Height - 120);
            };
        }

        private void SetupActiveOrdersPanel(TabPage tabPage)
        {
            int yOffset = 20;
            int panelWidth = tabPage.Width - 40;

            // Search Panel
            Panel pnlSearch = new Panel
            {
                Location = new Point(20, yOffset),
                Size = new Size(panelWidth, 50),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSearch = new Label
            {
                Text = "Search Order:",
                Location = new Point(0, 12),
                Size = new Size(90, 25),
                ForeColor = DeepSeekTextMuted,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            txtSearchOrder = new TextBox
            {
                Location = new Point(95, 8),
                Size = new Size(250, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Order # or Customer...",
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchOrder.TextChanged += (s, e) => LoadActiveOrders();

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtSearchOrder });

            // Active Orders Grid
            Panel pnlGrid = new Panel
            {
                Location = new Point(20, yOffset + 60),
                Size = new Size(panelWidth, 400),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlGrid.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlGrid.Width - 1, pnlGrid.Height - 1);
                }
            };

            Label lblOrdersTitle = new Label
            {
                Text = "Active Orders (Completed Transactions)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlGrid.Controls.Add(lblOrdersTitle);

            dgvCompletedOrders = CreateDataGridView(new Point(15, 50), new Size(pnlGrid.Width - 30, pnlGrid.Height - 100));
            dgvCompletedOrders.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvCompletedOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCompletedOrders.Columns.Clear();
            dgvCompletedOrders.Columns.Add("Id", "Order #");
            dgvCompletedOrders.Columns.Add("Date", "Date");
            dgvCompletedOrders.Columns.Add("Items", "Items");
            dgvCompletedOrders.Columns.Add("Total", "Total");
            dgvCompletedOrders.Columns.Add("Cashier", "Cashier");

            dgvCompletedOrders.Columns["Id"].FillWeight = 20;
            dgvCompletedOrders.Columns["Date"].FillWeight = 20;
            dgvCompletedOrders.Columns["Items"].FillWeight = 15;
            dgvCompletedOrders.Columns["Total"].FillWeight = 15;
            dgvCompletedOrders.Columns["Cashier"].FillWeight = 30;

            dgvCompletedOrders.SelectionChanged += (s, e) => UpdateCancelButtonState();

            pnlGrid.Controls.Add(dgvCompletedOrders);

            // Cancel Order Panel
Panel pnlCancel = new Panel
{
    Location = new Point(20, yOffset + 470),
    Size = new Size(panelWidth, 220),  // Increased height to accommodate notes
    BackColor = DeepSeekSurface,
    BorderStyle = BorderStyle.None,
    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
};
pnlCancel.Paint += (s, e) =>
{
    using (Pen pen = new Pen(DeepSeekBorder))
    {
        e.Graphics.DrawRectangle(pen, 0, 0, pnlCancel.Width - 1, pnlCancel.Height - 1);
    }
};

Label lblCancelTitle = new Label
{
    Text = "Cancel Selected Order",
    Font = new Font("Segoe UI", 12, FontStyle.Bold),
    ForeColor = DeepSeekDanger,
    Location = new Point(15, 15),
    AutoSize = true
};

// First row - Reason
Label lblReason = new Label
{
    Text = "Reason:",
    Location = new Point(15, 55),
    Size = new Size(60, 25),
    ForeColor = DeepSeekTextMuted,
    Font = new Font("Segoe UI", 10, FontStyle.Bold),
    TextAlign = ContentAlignment.MiddleRight
};

ComboBox cmbReason = new ComboBox
{
    Location = new Point(85, 52),
    Size = new Size(220, 35),
    DropDownStyle = ComboBoxStyle.DropDownList,
    BackColor = DeepSeekBg,
    ForeColor = DeepSeekText,
    Name = "cmbReason"
};
cmbReason.Items.AddRange(new object[] {
    "Customer requested cancellation",
    "Order processing error",
    "Item out of stock",
    "Payment issue",
    "Duplicate order",
    "Other"
});
cmbReason.SelectedIndex = 0;

// Second row - Refund Method
Label lblRefundMethod = new Label
{
    Text = "Refund:",
    Location = new Point(15, 100),
    Size = new Size(60, 25),
    ForeColor = DeepSeekTextMuted,
    Font = new Font("Segoe UI", 10, FontStyle.Bold),
    TextAlign = ContentAlignment.MiddleRight
};

ComboBox cmbRefundMethod = new ComboBox
{
    Location = new Point(85, 97),
    Size = new Size(150, 35),
    DropDownStyle = ComboBoxStyle.DropDownList,
    BackColor = DeepSeekBg,
    ForeColor = DeepSeekText,
    Name = "cmbRefundMethod"
};
cmbRefundMethod.Items.AddRange(new object[] { "Original Payment", "Cash", "GCash" });
cmbRefundMethod.SelectedIndex = 0;

// Third row - Notes (added back)
Label lblNotes = new Label
{
    Text = "Notes:",
    Location = new Point(15, 145),
    Size = new Size(60, 25),
    ForeColor = DeepSeekTextMuted,
    Font = new Font("Segoe UI", 10, FontStyle.Bold),
    TextAlign = ContentAlignment.MiddleRight
};

TextBox txtNotes = new TextBox
{
    Location = new Point(85, 142),
    Size = new Size(300, 35),
    Font = new Font("Segoe UI", 10),
    PlaceholderText = "Additional notes (optional)...",
    BackColor = DeepSeekBg,
    ForeColor = DeepSeekText,
    BorderStyle = BorderStyle.FixedSingle,
    Name = "txtNotes"
};

// Cancel Button
btnCancelOrderTransaction = new Button
{
    Text = "🚫 Cancel Order",
    Location = new Point(85, 190),
    Size = new Size(150, 35),
    BackColor = DeepSeekDanger,
    ForeColor = DeepSeekText,
    FlatStyle = FlatStyle.Flat,
    Font = new Font("Segoe UI", 11, FontStyle.Bold),
    FlatAppearance = { BorderSize = 0 }
};

btnCancelOrderTransaction.Click += (s, e) =>
{
    if (dgvCompletedOrders?.CurrentRow == null)
    {
        MessageBox.Show("Please select an order to cancel.", "No Selection", 
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    string orderNumber = dgvCompletedOrders.CurrentRow.Cells["Id"].Value?.ToString() ?? "";
    decimal total = Convert.ToDecimal(dgvCompletedOrders.CurrentRow.Cells["Total"].Value?.ToString()?.Replace("₱", "") ?? "0");
    string reason = cmbReason.SelectedItem?.ToString() ?? "Other";
    string refundMethod = cmbRefundMethod.SelectedItem?.ToString() ?? "Original Payment";
    string notes = txtNotes.Text;

    DialogResult result = MessageBox.Show(
        $"Are you sure you want to cancel Order #{orderNumber}?\n\n" +
        $"Total Amount: ₱{total:F2}\n" +
        $"Refund Method: {refundMethod}\n" +
        $"Reason: {reason}\n\n" +
        "This action will restore all ingredients to inventory and cannot be undone.",
        "Confirm Cancellation",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning);

    if (result == DialogResult.Yes)
    {
        try
        {
            // Get the transaction ID from the selected row
            var orderNumberValue = dgvCompletedOrders.CurrentRow.Cells["Id"].Value?.ToString();
            var transaction = dbService.GetAllCompletedTransactions()
                .FirstOrDefault(t => t.TransactionNumber == orderNumberValue);
            
            if (transaction != null)
            {
                dbService.CancelTransaction(transaction.Id, userId, reason, refundMethod, notes);
                MessageBox.Show($"Order #{orderNumber} has been cancelled successfully!\n\nRefund amount: ₱{total:F2}", 
                    "Order Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh grids
                LoadActiveOrders();
                LoadCancellations();
                
                // Reset form
                cmbReason.SelectedIndex = 0;
                cmbRefundMethod.SelectedIndex = 0;
                txtNotes.Text = "";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cancelling order: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
};

pnlCancel.Controls.AddRange(new Control[] { 
    lblCancelTitle, 
    lblReason, cmbReason, 
    lblRefundMethod, cmbRefundMethod,
    lblNotes, txtNotes,
    btnCancelOrderTransaction
});

tabPage.Controls.AddRange(new Control[] { pnlSearch, pnlGrid, pnlCancel });

            // Load data
            LoadActiveOrders();
        }

        private void SetupCancelledOrdersPanel(TabPage tabPage)
        {
            int yOffset = 20;
            int panelWidth = tabPage.Width - 40;

            // Filter Panel
            Panel pnlFilter = new Panel
            {
                Location = new Point(20, yOffset),
                Size = new Size(panelWidth, 50),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblFrom = new Label
            {
                Text = "From:",
                Location = new Point(0, 12),
                Size = new Size(40, 25),
                ForeColor = DeepSeekTextMuted,
                Font = new Font("Segoe UI", 10)
            };

            dtpCancelFrom = new DateTimePicker
            {
                Location = new Point(45, 8),
                Size = new Size(140, 35),
                Value = DateTime.Now.AddDays(-30),
                Font = new Font("Segoe UI", 10),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            Label lblTo = new Label
            {
                Text = "To:",
                Location = new Point(195, 12),
                Size = new Size(30, 25),
                ForeColor = DeepSeekTextMuted,
                Font = new Font("Segoe UI", 10)
            };

            dtpCancelTo = new DateTimePicker
            {
                Location = new Point(230, 8),
                Size = new Size(140, 35),
                Value = DateTime.Now,
                Font = new Font("Segoe UI", 10),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            btnFilterCancellations = new Button
            {
                Text = "Filter",
                Location = new Point(380, 8),
                Size = new Size(100, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnFilterCancellations.Click += (s, e) => LoadCancellations();

            pnlFilter.Controls.AddRange(new Control[] { lblFrom, dtpCancelFrom, lblTo, dtpCancelTo, btnFilterCancellations });

            // Cancellations Grid
            Panel pnlGrid = new Panel
            {
                Location = new Point(20, yOffset + 60),
                Size = new Size(panelWidth, 500),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            pnlGrid.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlGrid.Width - 1, pnlGrid.Height - 1);
                }
            };

            Label lblCancelledTitle = new Label
            {
                Text = "Cancelled Orders Archive",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlGrid.Controls.Add(lblCancelledTitle);

            dgvCancellations = CreateDataGridView(new Point(15, 50), new Size(pnlGrid.Width - 30, pnlGrid.Height - 70));
            dgvCancellations.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvCancellations.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCancellations.Columns.Clear();
            dgvCancellations.Columns.Add("OrderNumber", "Order #");
            dgvCancellations.Columns.Add("CancelledDate", "Cancelled Date");
            dgvCancellations.Columns.Add("OriginalAmount", "Original Amount");
            dgvCancellations.Columns.Add("RefundAmount", "Refund Amount");
            dgvCancellations.Columns.Add("Reason", "Reason");
            dgvCancellations.Columns.Add("CancelledBy", "Cancelled By");

            dgvCancellations.Columns["OrderNumber"].FillWeight = 15;
            dgvCancellations.Columns["CancelledDate"].FillWeight = 15;
            dgvCancellations.Columns["OriginalAmount"].FillWeight = 12;
            dgvCancellations.Columns["RefundAmount"].FillWeight = 12;
            dgvCancellations.Columns["Reason"].FillWeight = 25;
            dgvCancellations.Columns["CancelledBy"].FillWeight = 21;

            // Add double-click to view details
            dgvCancellations.CellDoubleClick += (s, e) => ShowCancellationDetails();

            pnlGrid.Controls.Add(dgvCancellations);

            tabPage.Controls.AddRange(new Control[] { pnlFilter, pnlGrid });

            LoadCancellations();
        }

        private void LoadActiveOrders()
        {
            if (dgvCompletedOrders == null) return;
            dgvCompletedOrders.Rows.Clear();

            var orders = dbService.GetAllCompletedTransactions();
            string searchText = txtSearchOrder?.Text?.ToLower() ?? "";

            foreach (var order in orders)
            {
                if (!string.IsNullOrEmpty(searchText) &&
                    !order.TransactionNumber.ToLower().Contains(searchText))
                    continue;

                int itemCount = order.TransactionItems?.Count ?? 0;

                dgvCompletedOrders.Rows.Add(
                    order.TransactionNumber,
                    order.TransactionDate.ToString("yyyy-MM-dd HH:mm"),
                    itemCount,
                    $"₱{order.Total:F2}",
                    order.Cashier?.FullName ?? order.Cashier?.Username ?? "Unknown"
                );
            }
        }

        private void LoadCancellations()
        {
            if (dgvCancellations == null) return;
            dgvCancellations.Rows.Clear();

            DateTime startDate = dtpCancelFrom?.Value.Date ?? DateTime.Now.AddDays(-30);
            DateTime endDate = dtpCancelTo?.Value.Date.AddDays(1).AddSeconds(-1) ?? DateTime.Now;

            var cancellations = dbService.GetCancellationsByDateRange(startDate, endDate);

            foreach (var cancel in cancellations)
            {
                dgvCancellations.Rows.Add(
                    cancel.TransactionNumber,
                    cancel.CancelledDate.ToString("yyyy-MM-dd HH:mm"),
                    $"₱{cancel.OriginalTotal:F2}",
                    $"₱{cancel.RefundAmount:F2}",
                    cancel.Reason ?? "-",
                    cancel.CancelledByUser?.FullName ?? cancel.CancelledByUser?.Username ?? "Unknown"
                );

                // Color code rows
                int rowIndex = dgvCancellations.Rows.Count - 1;
                dgvCancellations.Rows[rowIndex].DefaultCellStyle.ForeColor = DeepSeekTextMuted;
            }
        }

        private void UpdateCancelButtonState()
        {
            if (btnCancelOrder != null)
            {
                btnCancelOrder.Enabled = dgvCompletedOrders?.CurrentRow != null;
            }
        }

        private void ShowCancellationDetails()
        {
            if (dgvCancellations?.CurrentRow == null) return;

            string orderNumber = dgvCancellations.CurrentRow.Cells["OrderNumber"].Value?.ToString() ?? "";
            var cancellation = dbService.GetAllCancellations()
                .FirstOrDefault(c => c.TransactionNumber == orderNumber);

            if (cancellation == null) return;

            // Build details string
            string details = $"Order Cancellation Details\n";
            details += "═══════════════════════════════════\n\n";
            details += $"Order Number: {cancellation.TransactionNumber}\n";
            details += $"Cancelled Date: {cancellation.CancelledDate:yyyy-MM-dd HH:mm:ss}\n";
            details += $"Cancelled By: {cancellation.CancelledByUser?.FullName ?? cancellation.CancelledByUser?.Username ?? "Unknown"}\n";
            details += $"Reason: {cancellation.Reason ?? "Not specified"}\n";
            details += $"Refund Method: {cancellation.RefundMethod}\n";
            details += $"Original Amount: ₱{cancellation.OriginalTotal:F2}\n";
            details += $"Refund Amount: ₱{cancellation.RefundAmount:F2}\n";

            if (!string.IsNullOrEmpty(cancellation.Notes))
            {
                details += $"\nNotes: {cancellation.Notes}\n";
            }

            details += "\n═══════════════════════════════════\n";
            details += "Cancelled Items:\n";

            var cancelledItems = dbService.GetCancellationById(cancellation.Id)?.Transaction?.TransactionItems
                ?? new List<TransactionItem>();

            foreach (var item in cancelledItems)
            {
                string sizeInfo = string.IsNullOrEmpty(item.Size) ? "" : $" ({item.Size})";
                details += $"• {item.Product?.Name ?? "Unknown Product"}{sizeInfo} x{item.Quantity} - ₱{item.TotalPrice:F2}\n";
            }

            MessageBox.Show(details, "Cancellation Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCancellations_Click(object sender, EventArgs e)
        {
            ShowCancellationModule();
        }

        #endregion

        #region Report Module

        private TabControl? tcReports;
        private DateTimePicker? dtpReportStart;
        private DateTimePicker? dtpReportEnd;
        private Button? btnGenerateReport;
        private Label? lblTotalSales;
        private Label? lblTotalProductsSold;
        private Label? lblTotalTransactions;
        private DataGridView? dgvProductPerformance;
        private DataGridView? dgvInventoryStatus;
        private Panel? pnlSalesChart;
        private List<SalesChartDataPoint>? salesChartData;

        

        private void BtnReports_Click(object sender, EventArgs e)
        {
            ShowReportModule();
        }

        private void ShowReportModule()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            // Title
            Label lblTitle = new Label
            {
                Text = "Report Module",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true
            };

            // Tab Control
            tcReports = new TabControl
            {
                Location = new Point(30, 70),
                Size = new Size(pnlContent.Width - 60, pnlContent.Height - 100),
                Font = new Font("Segoe UI", 11),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Tab 1: Sales Report
            TabPage tpSalesReport = new TabPage("Sales Report");
            tpSalesReport.BackColor = DeepSeekBg;
            tpSalesReport.AutoScroll = true;
            SetupSalesReportPanel(tpSalesReport);

            // Tab 2: Inventory Report
            TabPage tpInventoryReport = new TabPage("Inventory Report");
            tpInventoryReport.BackColor = DeepSeekBg;
            tpInventoryReport.AutoScroll = true;
            SetupInventoryReportPanel(tpInventoryReport);

            tcReports.TabPages.Add(tpSalesReport);
            tcReports.TabPages.Add(tpInventoryReport);

            pnlContent.Controls.Add(lblTitle);
            pnlContent.Controls.Add(tcReports);

            // Load initial data
            tpSalesReport.Enter += (s, e) => LoadSalesReport();
            tpInventoryReport.Enter += (s, e) => LoadInventoryReport();
        }

        private void SetupSalesReportPanel(TabPage tabPage)
        {
            // Clear any existing controls
            tabPage.Controls.Clear();
            tabPage.AutoScroll = true;

            int yOffset = 20;
            int panelWidth = tabPage.Width - 40;

            // Quick Action PDF Export Buttons Panel
            Panel pnlQuickActions = new Panel
            {
                Location = new Point(20, yOffset),
                Size = new Size(panelWidth, 45),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnDailySalesReport = new Button
            {
                Text = "📄 Export Today's Sales",
                Location = new Point(0, 5),
                Size = new Size(160, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnDailySalesReport.Click += BtnExportDailySalesPDF_Click!;

            btnMonthlySalesReport = new Button
            {
                Text = "📄 Export Monthly Sales",
                Location = new Point(170, 5),
                Size = new Size(160, 35),
                BackColor = DeepSeekInfo,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnMonthlySalesReport.Click += BtnExportMonthlySalesPDF_Click!;

            pnlQuickActions.Controls.AddRange(new Control[] { btnDailySalesReport, btnMonthlySalesReport });

            // Date Range Selection Panel
            Panel pnlDateRange = new Panel
            {
                Location = new Point(20, yOffset + 55),
                Size = new Size(panelWidth, 60),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlDateRange.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlDateRange.Width - 1, pnlDateRange.Height - 1);
                }
            };

            Label lblFrom = new Label
            {
                Text = "From:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 18),
                AutoSize = true
            };

            dtpReportStart = new DateTimePicker
            {
                Location = new Point(70, 15),
                Size = new Size(200, 35),
                Value = DateTime.Now.AddDays(-30),
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText
            };
            dtpReportStart.ValueChanged += DtpReportStart_ValueChanged!;

            Label lblTo = new Label
            {
                Text = "To:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(290, 18),
                AutoSize = true
            };

            dtpReportEnd = new DateTimePicker
            {
                Location = new Point(330, 15),
                Size = new Size(200, 35),
                Value = DateTime.Now,
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText
            };
            dtpReportEnd.ValueChanged += DtpReportEnd_ValueChanged!;

            btnGenerateReport = new Button
            {
                Text = "Generate Report",
                Location = new Point(550, 15),
                Size = new Size(130, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnGenerateReport.Click += BtnGenerateReport_Click!;

            btnExportSalesPDF = new Button
            {
                Text = "📄 Export PDF",
                Location = new Point(690, 15),
                Size = new Size(130, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnExportSalesPDF.Click += BtnExportSalesPDF_Click!;

            pnlDateRange.Controls.AddRange(new Control[] {
        lblFrom, dtpReportStart, lblTo, dtpReportEnd,
        btnGenerateReport, btnExportSalesPDF
    });

            // Summary Cards Panel
            Panel pnlSummary = new Panel
            {
                Location = new Point(20, yOffset + 135),
                Size = new Size(panelWidth, 120),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlSummary.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSummary.Width - 1, pnlSummary.Height - 1);
                }
            };

            int cardWidth = (pnlSummary.Width - 60) / 3;
            int cardX = 15;

            // Total Sales Card
            Panel pnlTotalSalesCard = CreateSummaryCard("Total Sales Amount", "₱0.00", DeepSeekSuccess, cardX);
            pnlTotalSalesCard.Width = cardWidth;
            lblTotalSales = pnlTotalSalesCard.Controls.OfType<Label>().Last();

            // Total Products Sold Card
            Panel pnlProductsSoldCard = CreateSummaryCard("Products Sold", "0", DeepSeekAccent, cardX + cardWidth + 15);
            pnlProductsSoldCard.Width = cardWidth;
            lblTotalProductsSold = pnlProductsSoldCard.Controls.OfType<Label>().Last();

            // Total Transactions Card
            Panel pnlTransactionsCard = CreateSummaryCard("Number of Transactions", "0", DeepSeekInfo, cardX + (cardWidth + 15) * 2);
            pnlTransactionsCard.Width = cardWidth;
            lblTotalTransactions = pnlTransactionsCard.Controls.OfType<Label>().Last();

            pnlSummary.Controls.AddRange(new Control[] { pnlTotalSalesCard, pnlProductsSoldCard, pnlTransactionsCard });

            // Sales Chart Container
            Panel pnlChartContainer = new Panel
            {
                Location = new Point(20, yOffset + 275),
                Size = new Size(panelWidth, 300),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlChartContainer.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlChartContainer.Width - 1, pnlChartContainer.Height - 1);
                }
            };

            Label lblChartTitle = new Label
            {
                Text = "Sales Trend",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlChartContainer.Controls.Add(lblChartTitle);

            pnlSalesChart = new Panel
            {
                Location = new Point(15, 50),
                Size = new Size(pnlChartContainer.Width - 30, 230),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            pnlSalesChart.Paint += DrawSalesChartReport;
            pnlChartContainer.Controls.Add(pnlSalesChart);

            // Product Performance Grid Container
            Panel pnlPerformanceContainer = new Panel
            {
                Location = new Point(20, yOffset + 595),
                Size = new Size(panelWidth, 350),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            pnlPerformanceContainer.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlPerformanceContainer.Width - 1, pnlPerformanceContainer.Height - 1);
                }
            };

            Label lblPerformanceTitle = new Label
            {
                Text = "Product Performance",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlPerformanceContainer.Controls.Add(lblPerformanceTitle);

            dgvProductPerformance = CreateDataGridView(new Point(15, 50), new Size(pnlPerformanceContainer.Width - 30, pnlPerformanceContainer.Height - 70));
            dgvProductPerformance.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvProductPerformance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Clear existing columns and add new ones
            dgvProductPerformance.Columns.Clear();
            dgvProductPerformance.Columns.Add("Product", "Product");
            dgvProductPerformance.Columns.Add("UnitSold", "Unit Sold");
            dgvProductPerformance.Columns.Add("Revenue", "Revenue");
            dgvProductPerformance.Columns.Add("PercentOfSales", "% of Sales");
            dgvProductPerformance.Columns.Add("Performance", "Performance");

            dgvProductPerformance.Columns["Product"].FillWeight = 35;
            dgvProductPerformance.Columns["UnitSold"].FillWeight = 15;
            dgvProductPerformance.Columns["Revenue"].FillWeight = 20;
            dgvProductPerformance.Columns["PercentOfSales"].FillWeight = 15;
            dgvProductPerformance.Columns["Performance"].FillWeight = 15;

            pnlPerformanceContainer.Controls.Add(dgvProductPerformance);

            tabPage.Controls.AddRange(new Control[] { pnlQuickActions, pnlDateRange, pnlSummary, pnlChartContainer, pnlPerformanceContainer });

            // Handle resize events
            tabPage.Resize += (sender, e) =>
            {
                int newPanelWidth = tabPage.Width - 40;
                pnlQuickActions.Width = newPanelWidth;
                pnlDateRange.Width = newPanelWidth;
                pnlSummary.Width = newPanelWidth;
                pnlChartContainer.Width = newPanelWidth;
                pnlPerformanceContainer.Width = newPanelWidth;
                pnlPerformanceContainer.Height = tabPage.Height - 665;

                if (pnlSalesChart != null)
                {
                    pnlSalesChart.Width = pnlChartContainer.Width - 30;
                }

                if (dgvProductPerformance != null)
                {
                    dgvProductPerformance.Width = pnlPerformanceContainer.Width - 30;
                    dgvProductPerformance.Height = pnlPerformanceContainer.Height - 70;
                }

                // Update card widths on resize
                int newCardWidth = (newPanelWidth - 60) / 3;
                pnlTotalSalesCard.Width = newCardWidth;
                pnlProductsSoldCard.Width = newCardWidth;
                pnlTransactionsCard.Width = newCardWidth;

                pnlProductsSoldCard.Left = 15 + newCardWidth + 15;
                pnlTransactionsCard.Left = 15 + (newCardWidth + 15) * 2;
            };
        }

        private void SetupInventoryReportPanel(TabPage tabPage)
        {
            // Clear any existing controls
            tabPage.Controls.Clear();
            tabPage.AutoScroll = true;

            int yOffset = 20;
            int panelWidth = tabPage.Width - 40;

            // Quick Action PDF Export Buttons Panel
            Panel pnlQuickActions = new Panel
            {
                Location = new Point(20, yOffset),
                Size = new Size(panelWidth, 45),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnDailyInventoryReport = new Button
            {
                Text = "📄 Export Current Inventory",
                Location = new Point(0, 5),
                Size = new Size(180, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnDailyInventoryReport.Click += BtnExportCurrentInventoryPDF_Click!;

            // Remove the monthly button - only keep the daily/current button
            pnlQuickActions.Controls.AddRange(new Control[] { btnDailyInventoryReport });

            // Summary Cards Panel
            Panel pnlSummary = new Panel
            {
                Location = new Point(20, yOffset + 55),
                Size = new Size(panelWidth, 120),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlSummary.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSummary.Width - 1, pnlSummary.Height - 1);
                }
            };

            var ingredients = dbService.GetAllIngredients();
            var inventory = dbService.GetAllInventory();

            int totalIngredients = ingredients.Count(i => i.IsActive);
            int lowStockCount = 0;
            int outOfStockCount = 0;

            foreach (var ingredient in ingredients.Where(i => i.IsActive))
            {
                var stock = inventory.FirstOrDefault(inv => inv.IngredientId == ingredient.Id);
                decimal currentStock = stock?.Quantity ?? 0;
                if (currentStock <= 0) outOfStockCount++;
                else if (currentStock < ingredient.MinimumStock) lowStockCount++;
            }

            int cardWidth = (pnlSummary.Width - 60) / 3;
            int cardX = 15;

            // Total Ingredients Card
            Panel pnlTotalIngredientsCard = CreateSummaryCard("Total Ingredients", totalIngredients.ToString(), DeepSeekSuccess, cardX);
            pnlTotalIngredientsCard.Width = cardWidth;

            // Low Stock Card
            Panel pnlLowStockCard = CreateSummaryCard("Low Stock", lowStockCount.ToString(), DeepSeekWarning, cardX + cardWidth + 15);
            pnlLowStockCard.Width = cardWidth;

            // Out of Stock Card
            Panel pnlOutOfStockCard = CreateSummaryCard("Out of Stock", outOfStockCount.ToString(), DeepSeekDanger, cardX + (cardWidth + 15) * 2);
            pnlOutOfStockCard.Width = cardWidth;

            pnlSummary.Controls.AddRange(new Control[] { pnlTotalIngredientsCard, pnlLowStockCard, pnlOutOfStockCard });

            // Export Button Panel
            Panel pnlExport = new Panel
            {
                Location = new Point(20, yOffset + 185),
                Size = new Size(panelWidth, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnExportInventoryPDF = new Button
            {
                Text = "📄 Export PDF",
                Location = new Point(0, 8),
                Size = new Size(150, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnExportInventoryPDF.Click += BtnExportInventoryPDF_Click!;
            pnlExport.Controls.Add(btnExportInventoryPDF);

            // Inventory Status Grid Container
            Panel pnlInventoryContainer = new Panel
            {
                Location = new Point(20, yOffset + 245),
                Size = new Size(panelWidth, tabPage.Height - 285),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            pnlInventoryContainer.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlInventoryContainer.Width - 1, pnlInventoryContainer.Height - 1);
                }
            };

            Label lblInventoryTitle = new Label
            {
                Text = "Current Inventory Status",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlInventoryContainer.Controls.Add(lblInventoryTitle);

            // Create DataGridView with proper sizing
            dgvInventoryStatus = CreateDataGridView(new Point(15, 55), new Size(pnlInventoryContainer.Width - 30, pnlInventoryContainer.Height - 70));
            dgvInventoryStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvInventoryStatus.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Clear existing columns and add new ones
            dgvInventoryStatus.Columns.Clear();
            dgvInventoryStatus.Columns.Add("Ingredient", "Ingredient");
            dgvInventoryStatus.Columns.Add("CurrentStock", "Current Stock");
            dgvInventoryStatus.Columns.Add("Unit", "Unit");
            dgvInventoryStatus.Columns.Add("MinThreshold", "Min Threshold");
            dgvInventoryStatus.Columns.Add("Status", "Status");

            // Set column fill weights
            dgvInventoryStatus.Columns["Ingredient"].FillWeight = 40;
            dgvInventoryStatus.Columns["CurrentStock"].FillWeight = 20;
            dgvInventoryStatus.Columns["Unit"].FillWeight = 10;
            dgvInventoryStatus.Columns["MinThreshold"].FillWeight = 15;
            dgvInventoryStatus.Columns["Status"].FillWeight = 15;

            pnlInventoryContainer.Controls.Add(dgvInventoryStatus);

            tabPage.Controls.AddRange(new Control[] { pnlQuickActions, pnlSummary, pnlExport, pnlInventoryContainer });

            // Handle resize events
            tabPage.Resize += (sender, e) =>
            {
                int newPanelWidth = tabPage.Width - 40;
                pnlQuickActions.Width = newPanelWidth;
                pnlSummary.Width = newPanelWidth;
                pnlExport.Width = newPanelWidth;
                pnlInventoryContainer.Width = newPanelWidth;
                pnlInventoryContainer.Height = tabPage.Height - 305;

                if (dgvInventoryStatus != null)
                {
                    dgvInventoryStatus.Width = pnlInventoryContainer.Width - 30;
                    dgvInventoryStatus.Height = pnlInventoryContainer.Height - 70;
                }

                // Update card widths on resize
                int newCardWidth = (newPanelWidth - 60) / 3;
                pnlTotalIngredientsCard.Width = newCardWidth;
                pnlLowStockCard.Width = newCardWidth;
                pnlOutOfStockCard.Width = newCardWidth;

                pnlLowStockCard.Left = 15 + newCardWidth + 15;
                pnlOutOfStockCard.Left = 15 + (newCardWidth + 15) * 2;
            };

            // Load data
            LoadInventoryReport();
        }

        private Panel CreateSummaryCard(string title, string value, Color accentColor, int xPosition)
        {
            Panel card = new Panel
            {
                Size = new Size(300, 90), // Default size, will be adjusted
                Location = new Point(xPosition, 15),
                BackColor = DeepSeekBg,
                BorderStyle = BorderStyle.None
            };
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(accentColor, 2))
                {
                    e.Graphics.DrawLine(pen, 0, 0, card.Width, 0);
                }
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 15),
                AutoSize = true
            };

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(15, 45),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { lblTitle, lblValue });
            return card;
        }

        private void BtnGenerateReport_Click(object sender, EventArgs e)
        {
            // Clear existing data
            if (dgvProductPerformance != null)
            {
                dgvProductPerformance.Rows.Clear();
            }

            // Load the sales report data
            LoadSalesReport();

            // Refresh the chart
            if (pnlSalesChart != null)
            {
                pnlSalesChart.Invalidate();
            }
        }

        private void LoadSalesReport()
        {
            if (dtpReportStart == null || dtpReportEnd == null) return;

            DateTime startDate = dtpReportStart.Value.Date;
            DateTime endDate = dtpReportEnd.Value.Date.AddDays(1).AddSeconds(-1);

            // Get all transactions in date range
            var allTransactions = dbService.GetAllTransactions();
            var transactions = allTransactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Status == "Completed")
                .ToList();

            decimal totalSales = transactions.Sum(t => t.Total);
            int totalItemsSold = transactions.Sum(t => t.TransactionItems?.Sum(ti => ti.Quantity) ?? 0);
            int totalTransactions = transactions.Count;

            // Update summary labels
            if (lblTotalSales != null) lblTotalSales.Text = $"₱{totalSales:F2}";
            if (lblTotalProductsSold != null) lblTotalProductsSold.Text = totalItemsSold.ToString();
            if (lblTotalTransactions != null) lblTotalTransactions.Text = totalTransactions.ToString();

            // Get ALL active products
            var allProducts = dbService.GetAllProducts().Where(p => p.IsActive).ToList();

            // Build sales data from transactions
            var salesData = new Dictionary<int, (string Name, int Quantity, decimal Revenue)>();

            foreach (var product in allProducts)
            {
                salesData[product.Id] = (product.Name, 0, 0m);
            }

            // Populate sales data from transactions
            foreach (var transaction in transactions)
            {
                if (transaction.TransactionItems != null)
                {
                    foreach (var item in transaction.TransactionItems)
                    {
                        string productName = item.Product?.Name ?? "Unknown Product";
                        if (productName == "Unknown Product" && item.ProductId > 0)
                        {
                            var product = dbService.GetProductById(item.ProductId);
                            productName = product?.Name ?? $"Product #{item.ProductId}";
                        }

                        if (salesData.ContainsKey(item.ProductId))
                        {
                            var existing = salesData[item.ProductId];
                            salesData[item.ProductId] = (existing.Name, existing.Quantity + item.Quantity, existing.Revenue + (item.UnitPrice * item.Quantity));
                        }
                        else
                        {
                            salesData[item.ProductId] = (productName, item.Quantity, item.UnitPrice * item.Quantity);
                        }
                    }
                }
            }

            // Update product performance grid
            if (dgvProductPerformance != null)
            {
                dgvProductPerformance.Rows.Clear();

                // Sort by revenue (highest first), with zero-sales products at the bottom
                var sortedProducts = salesData.OrderByDescending(p => p.Value.Revenue).ToList();
                decimal totalRevenue = sortedProducts.Sum(p => p.Value.Revenue);

                foreach (var product in sortedProducts)
                {
                    decimal percentOfSales = totalRevenue > 0 ? (product.Value.Revenue / totalRevenue) * 100 : 0;
                    string performance;
                    Color perfColor;

                    if (product.Value.Quantity == 0)
                    {
                        performance = "No Sales";
                        perfColor = DeepSeekTextMuted;
                    }
                    else if (percentOfSales >= 20)
                    {
                        performance = "Best Seller";
                        perfColor = DeepSeekSuccess;
                    }
                    else if (percentOfSales >= 10)
                    {
                        performance = "Good Seller";
                        perfColor = DeepSeekInfo;
                    }
                    else
                    {
                        performance = "Slow Mover";
                        perfColor = DeepSeekWarning;
                    }

                    int rowIndex = dgvProductPerformance.Rows.Add(
                        product.Value.Name,
                        product.Value.Quantity,
                        $"₱{product.Value.Revenue:F2}",
                        $"{percentOfSales:F2}%",
                        performance
                    );

                    if (rowIndex >= 0 && dgvProductPerformance.Rows[rowIndex].Cells["Performance"] != null)
                    {
                        dgvProductPerformance.Rows[rowIndex].Cells["Performance"].Style.ForeColor = perfColor;

                        // Style "No Sales" rows with italic font
                        if (product.Value.Quantity == 0)
                        {
                            dgvProductPerformance.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                            dgvProductPerformance.Rows[rowIndex].DefaultCellStyle.ForeColor = DeepSeekTextMuted;
                        }
                    }
                }
            }

            // Build chart data
            salesChartData = new List<SalesChartDataPoint>();
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                decimal dailySales = transactions.Where(t => t.TransactionDate.Date == date.Date).Sum(t => t.Total);
                salesChartData.Add(new SalesChartDataPoint { Date = date, Sales = dailySales });
            }

            // Refresh chart
            if (pnlSalesChart != null)
            {
                pnlSalesChart.Invalidate();
            }
        }

        private void DrawSalesChartReport(object sender, PaintEventArgs e)
        {
            if (salesChartData == null || salesChartData.Count == 0)
            {
                Panel pnl = sender as Panel;
                if (pnl != null)
                {
                    using (SolidBrush textBrush = new SolidBrush(DeepSeekTextMuted))
                    using (Font font = new Font("Segoe UI", 14))
                    {
                        string message = "No sales data available for selected date range";
                        SizeF textSize = e.Graphics.MeasureString(message, font);
                        float x = (pnl.Width - textSize.Width) / 2;
                        float y = (pnl.Height - textSize.Height) / 2;
                        e.Graphics.DrawString(message, font, textBrush, x, y);
                    }
                }
                return;
            }

            Panel pnlChart = sender as Panel;
            if (pnlChart == null) return;

            int chartTop = 20;
            int chartHeight = pnlChart.Height - 60;
            int baseY = chartTop + chartHeight;
            int startX = 50;

            int barCount = salesChartData.Count;
            decimal maxSales = salesChartData.Max(d => d.Sales);
            if (maxSales == 0) maxSales = 1;

            int availableWidth = pnlChart.Width - 100;
            int barWidth = Math.Max(3, availableWidth / barCount - 2);
            int spacing = Math.Max(1, (availableWidth - (barWidth * barCount)) / (barCount + 1));

            bool useLineChart = barWidth < 8;

            using (Pen gridPen = new Pen(Color.FromArgb(60, 60, 70), 1))
            using (SolidBrush barBrush = new SolidBrush(DeepSeekAccent))
            using (Pen linePen = new Pen(DeepSeekAccent, 2))
            using (SolidBrush textBrush = new SolidBrush(DeepSeekText))
            using (SolidBrush mutedBrush = new SolidBrush(DeepSeekTextMuted))
            using (Font dayFont = new Font("Segoe UI", 7))
            using (Font amountFont = new Font("Segoe UI", 7))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int yPos = baseY - (i * chartHeight / 4);
                    e.Graphics.DrawLine(gridPen, startX - 10, yPos, pnlChart.Width - 20, yPos);

                    string valueLabel = $"₱{(maxSales * i / 4):F0}";
                    SizeF labelSize = e.Graphics.MeasureString(valueLabel, amountFont);
                    e.Graphics.DrawString(valueLabel, amountFont, mutedBrush, startX - 30 - labelSize.Width, yPos - 8);
                }

                if (useLineChart)
                {
                    List<Point> points = new List<Point>();
                    for (int i = 0; i < barCount; i++)
                    {
                        int barHeight = (int)((salesChartData[i].Sales / maxSales) * chartHeight);
                        int xPos = startX + spacing + i * (barWidth + spacing);
                        int yPos = baseY - barHeight;
                        points.Add(new Point(xPos + barWidth / 2, yPos));

                        e.Graphics.FillEllipse(barBrush, xPos + barWidth / 2 - 3, yPos - 3, 6, 6);

                        if (i % 5 == 0 || i == barCount - 1)
                        {
                            string dateLabel = salesChartData[i].Date.ToString("MM/dd");
                            SizeF daySize = e.Graphics.MeasureString(dateLabel, dayFont);
                            e.Graphics.DrawString(dateLabel, dayFont, textBrush, xPos + barWidth / 2 - daySize.Width / 2, baseY + 5);
                        }
                    }

                    if (points.Count > 1)
                    {
                        e.Graphics.DrawLines(linePen, points.ToArray());
                    }
                }
                else
                {
                    for (int i = 0; i < barCount; i++)
                    {
                        int barHeight = (int)((salesChartData[i].Sales / maxSales) * chartHeight);
                        if (barHeight < 2 && salesChartData[i].Sales > 0) barHeight = 2;

                        int xPos = startX + spacing + i * (barWidth + spacing);
                        int yPos = baseY - barHeight;

                        e.Graphics.FillRectangle(barBrush, xPos, yPos, barWidth, barHeight);

                        if (i % 5 == 0 || i == barCount - 1)
                        {
                            string dateLabel = salesChartData[i].Date.ToString("MM/dd");
                            SizeF daySize = e.Graphics.MeasureString(dateLabel, dayFont);
                            e.Graphics.DrawString(dateLabel, dayFont, textBrush, xPos + (barWidth - daySize.Width) / 2, baseY + 5);
                        }

                        if (salesChartData[i].Sales > maxSales * 0.1m)
                        {
                            string amountText = $"₱{Math.Round(salesChartData[i].Sales, 0)}";
                            SizeF amountSize = e.Graphics.MeasureString(amountText, amountFont);
                            e.Graphics.DrawString(amountText, amountFont, textBrush, xPos + (barWidth - amountSize.Width) / 2, yPos - 15);
                        }
                    }
                }
            }
        }

        private void LoadInventoryReport()
        {
            if (dgvInventoryStatus == null) return;

            dgvInventoryStatus.Rows.Clear();
            var ingredients = dbService.GetAllIngredients().Where(i => i.IsActive).ToList();
            var inventory = dbService.GetAllInventory();

            foreach (var ingredient in ingredients)
            {
                var stock = inventory.FirstOrDefault(i => i.IngredientId == ingredient.Id);
                decimal currentStock = stock?.Quantity ?? 0;

                string status = currentStock <= 0 ? "Out of Stock" :
                               currentStock < ingredient.MinimumStock ? "Low Stock" : "In Stock";
                Color statusColor = currentStock <= 0 ? DeepSeekDanger :
                                   currentStock < ingredient.MinimumStock ? DeepSeekWarning : DeepSeekSuccess;

                int rowIndex = dgvInventoryStatus.Rows.Add(
                    ingredient.Name,
                    $"{currentStock:F1}",
                    ingredient.Unit ?? "kg",
                    ingredient.MinimumStock.ToString(),
                    status
                );
                dgvInventoryStatus.Rows[rowIndex].Cells["Status"].Style.ForeColor = statusColor;
                dgvInventoryStatus.Rows[rowIndex].Cells["Status"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        #endregion

        #region Ingredient Library Management

        private void ShowIngredientLibrary()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "Ingredient Library",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true
            };

            Panel pnlStats = new Panel
            {
                Location = new Point(30, 70),
                Size = new Size(pnlContent.Width - 60, 100),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlStats.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlStats.Width - 1, pnlStats.Height - 1);
                }
            };

            var ingredients = dbService.GetAllIngredients();
            var inventory = dbService.GetAllInventory();
            int totalIngredients = ingredients.Count;
            int activeIngredients = ingredients.Count(i => i.IsActive);
            int lowStockCount = 0, outOfStockCount = 0;

            foreach (var ingredient in ingredients.Where(i => i.IsActive))
            {
                var ingredientInventory = inventory.FirstOrDefault(inv => inv.IngredientId == ingredient.Id);
                decimal currentStock = ingredientInventory?.Quantity ?? 0;
                if (currentStock <= 0) outOfStockCount++;
                else if (currentStock < ingredient.MinimumStock) lowStockCount++;
            }

            Label lblTotalIng = new Label { Text = $"Total Ingredients: {totalIngredients}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(30, 35), AutoSize = true };
            Label lblActiveIng = new Label { Text = $"Active: {activeIngredients}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekSuccess, Location = new Point(300, 35), AutoSize = true };
            Label lblLowStock = new Label { Text = $"Low Stock: {lowStockCount}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekWarning, Location = new Point(500, 35), AutoSize = true };
            Label lblOutOfStock = new Label { Text = $"Out of Stock: {outOfStockCount}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekDanger, Location = new Point(700, 35), AutoSize = true };

            pnlStats.Controls.AddRange(new Control[] { lblTotalIng, lblActiveIng, lblLowStock, lblOutOfStock });

            Panel pnlSearch = new Panel
            {
                Location = new Point(30, 180),
                Size = new Size(pnlContent.Width - 60, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtSearchIngredient = new TextBox
            {
                Location = new Point(0, 8),
                Size = new Size(350, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search ingredients...",
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchIngredient.TextChanged += TxtSearchIngredient_TextChanged!;

            Button btnAddIngredient = new Button
            {
                Text = "+ Add New Ingredient",
                Location = new Point(370, 8),
                Size = new Size(180, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnAddIngredient.Click += BtnAddIngredientLib_Click!;

            Button btnEditSelected = new Button
            {
                Text = "Edit Selected",
                Location = new Point(570, 8),
                Size = new Size(120, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnEditSelected.Click += BtnEditSelectedIngredient_Click!;

            Button btnToggleSelected = new Button
            {
                Text = "Toggle Status",
                Location = new Point(710, 8),
                Size = new Size(120, 35),
                BackColor = DeepSeekWarning,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnToggleSelected.Click += BtnToggleSelectedIngredient_Click!;

            Button btnReorder = new Button
            {
                Text = "🔄 Reorder",
                Location = new Point(840, 8),
                Size = new Size(100, 35),
                BackColor = DeepSeekInfo,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnReorder.Click += BtnReorderIngredient_Click!;

            pnlSearch.Controls.AddRange(new Control[] {
        txtSearchIngredient, btnAddIngredient, btnEditSelected, btnToggleSelected, btnReorder
    });

            // ========== DATAGRIDVIEW - LOWERED POSITION ==========
            dgvIngredientLibrary = CreateDataGridView(new Point(30, 250), new Size(pnlContent.Width - 60, 380));
            dgvIngredientLibrary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvIngredientLibrary.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            
            dgvIngredientLibrary.Columns.Add("Name", "Ingredient Name");
            dgvIngredientLibrary.Columns.Add("Unit", "Unit");
            dgvIngredientLibrary.Columns.Add("MinStock", "Min Stock");
            dgvIngredientLibrary.Columns.Add("StockLeft", "Stock Left");
            dgvIngredientLibrary.Columns.Add("Status", "Status");

            
            dgvIngredientLibrary.Columns["Name"].FillWeight = 35;
            dgvIngredientLibrary.Columns["Unit"].FillWeight = 15;
            dgvIngredientLibrary.Columns["MinStock"].FillWeight = 15;
            dgvIngredientLibrary.Columns["StockLeft"].FillWeight = 20;
            dgvIngredientLibrary.Columns["Status"].FillWeight = 15;

            // Stock Adjustment Panel - moved down to match new grid position
            pnlStockAdjustment = new Panel
            {
                Location = new Point(30, 660),
                Size = new Size(pnlContent.Width - 60, 60),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Visible = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlStockAdjustment.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlStockAdjustment.Width - 1, pnlStockAdjustment.Height - 1);
                }
            };

            Label lblAdjustTitle = new Label
            {
                Text = "Adjust Stock:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 20),
                AutoSize = true
            };

            lblSelectedIngredient = new Label
            {
                Text = "None selected",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekAccent,
                Location = new Point(130, 20),
                AutoSize = true,
                MaximumSize = new Size(400, 0)
            };

            cmbAdjustOperation = new ComboBox
            {
                Location = new Point(450, 15),
                Size = new Size(80, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };
            cmbAdjustOperation.Items.AddRange(new object[] { "+ Add", "- Subtract" });
            cmbAdjustOperation.SelectedIndex = 0;

            nudAdjustQuantity = new NumericUpDown
            {
                Location = new Point(540, 15),
                Size = new Size(100, 35),
                Minimum = 0.1m,
                Maximum = 10000,
                DecimalPlaces = 1,
                Value = 1,
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            Button btnApplyAdjustment = new Button
            {
                Text = "Apply",
                Location = new Point(660, 15),
                Size = new Size(100, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnApplyAdjustment.Click += BtnApplyAdjustment_Click!;

            pnlStockAdjustment.Controls.AddRange(new Control[] { lblAdjustTitle, lblSelectedIngredient, cmbAdjustOperation, nudAdjustQuantity, btnApplyAdjustment });

            // Ingredient Form Panel - WITH ACTIVE CHECKBOX MOVED
            pnlIngredientForm = new Panel
            {
                Location = new Point(30, 740),
                Size = new Size(pnlContent.Width - 60, 200),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblFormTitle = new Label { Text = "Ingredient Details", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(15, 15), AutoSize = true };

            // Row 1: Name
            Label lblIngName = new Label { Text = "Name*:", Location = new Point(15, 55), AutoSize = true, ForeColor = DeepSeekTextMuted };
            txtIngredientName = new TextBox { Location = new Point(100, 52), Size = new Size(250, 35), BackColor = DeepSeekBg, ForeColor = DeepSeekText };

            // Row 2: Unit (left) and Min Stock (center) and Active (right) - all on same row
            Label lblUnit = new Label { Text = "Unit*:", Location = new Point(15, 100), AutoSize = true, ForeColor = DeepSeekTextMuted };
            cmbIngredientUnit = new ComboBox { Location = new Point(100, 97), Size = new Size(120, 35), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = DeepSeekBg, ForeColor = DeepSeekText };
            cmbIngredientUnit.Items.AddRange(new object[] { "kg", "g", "L", "ml", "pcs" });
            cmbIngredientUnit.SelectedIndex = 0;

            Label lblMinStock = new Label { Text = "Min Stock:", Location = new Point(250, 100), AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = DeepSeekTextMuted };
            nudMinimumStock = new NumericUpDown { Location = new Point(350, 97), Size = new Size(100, 35), Minimum = 0, Maximum = 100000, Value = 10, BackColor = DeepSeekBg, ForeColor = DeepSeekText };

            // Active checkbox moved to the right side of the same row
            chkIngredientActive = new CheckBox
            {
                Text = "Active",
                Location = new Point(480, 103),
                Checked = true,
                AutoSize = true,
                ForeColor = DeepSeekText
            };

            // Row 3: Buttons (moved down slightly)
            btnSaveIngredient = new Button { Text = "Save Ingredient", Location = new Point(15, 150), Size = new Size(140, 40), BackColor = DeepSeekSuccess, ForeColor = DeepSeekText, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 } };
            btnSaveIngredient.Click += BtnSaveIngredient_Click!;

            btnCancelIngredient = new Button { Text = "Cancel", Location = new Point(170, 150), Size = new Size(120, 40), BackColor = DeepSeekTextMuted, ForeColor = DeepSeekText, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 } };
            btnCancelIngredient.Click += (s, e) => { pnlIngredientForm.Visible = false; ClearIngredientForm(); };

            btnDeleteIngredient = new Button { Text = "Delete", Location = new Point(310, 150), Size = new Size(120, 40), BackColor = DeepSeekDanger, ForeColor = DeepSeekText, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 } };
            btnDeleteIngredient.Click += BtnDeleteIngredient_Click!;

            pnlIngredientForm.Controls.AddRange(new Control[] {
    lblFormTitle,
    lblIngName, txtIngredientName,
    lblUnit, cmbIngredientUnit,
    lblMinStock, nudMinimumStock,
    chkIngredientActive,
    btnSaveIngredient, btnCancelIngredient, btnDeleteIngredient
});

            pnlContent.Controls.AddRange(new Control[] { lblTitle, pnlStats, pnlSearch, dgvIngredientLibrary, pnlStockAdjustment, pnlIngredientForm });

            dgvIngredientLibrary.SelectionChanged += DgvIngredientLibrary_SelectionChanged!;
            LoadIngredientLibrary();

            // Handle resize
            pnlContent.Resize += (s, e) =>
            {
                pnlStats.Width = pnlContent.Width - 60;
                pnlSearch.Width = pnlContent.Width - 60;
                dgvIngredientLibrary.Width = pnlContent.Width - 60;
                dgvIngredientLibrary.Height = pnlContent.Height - 400;  // Adjusted from 380 to 400
                pnlStockAdjustment.Width = pnlContent.Width - 60;
                pnlStockAdjustment.Location = new Point(30, pnlContent.Height - 160);
                pnlIngredientForm.Width = pnlContent.Width - 60;
                pnlIngredientForm.Location = new Point(30, pnlContent.Height - 220);
            };
        }

        private void LoadIngredientLibrary()
        {
            if (dgvIngredientLibrary == null) return;
            dgvIngredientLibrary.Rows.Clear();
            var ingredients = dbService.GetAllIngredients().OrderBy(i => i.Name).ToList(); // Sort alphabetically
            var inventory = dbService.GetAllInventory();
            string searchText = txtSearchIngredient?.Text?.ToLower() ?? "";

            foreach (var ingredient in ingredients)
            {
                if (!string.IsNullOrEmpty(searchText) && !ingredient.Name.ToLower().Contains(searchText)) continue;
                var ingredientInventory = inventory.FirstOrDefault(i => i.IngredientId == ingredient.Id);
                decimal currentStock = ingredientInventory?.Quantity ?? 0;
                string stockDisplay = $"{currentStock:F1} {ingredient.Unit ?? "g"}";

                // Low Stock = 50% of Minimum Stock
                decimal lowStockThreshold = ingredient.MinimumStock * 0.5m;
                string statusText;

                if (!ingredient.IsActive)
                {
                    statusText = "Inactive";
                }
                else if (currentStock <= 0)
                {
                    statusText = "Out of Stock";
                }
                else if (currentStock <= lowStockThreshold)
                {
                    statusText = "Low Stock";
                }
                else
                {
                    statusText = "In Stock";
                }

                // REMOVED: ingredient.Id from the row
                int rowIndex = dgvIngredientLibrary.Rows.Add(
                    ingredient.Name,
                    ingredient.Unit ?? "g",
                    ingredient.MinimumStock,
                    stockDisplay,
                    statusText
                );

                if (rowIndex >= 0)
                {
                    if (currentStock <= 0)
                    {
                        dgvIngredientLibrary.Rows[rowIndex].Cells["StockLeft"].Style.ForeColor = DeepSeekDanger;
                        dgvIngredientLibrary.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                    }
                    else if (currentStock <= lowStockThreshold)
                    {
                        dgvIngredientLibrary.Rows[rowIndex].Cells["StockLeft"].Style.ForeColor = DeepSeekWarning;
                        dgvIngredientLibrary.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekWarning;
                    }
                    else
                    {
                        dgvIngredientLibrary.Rows[rowIndex].Cells["StockLeft"].Style.ForeColor = DeepSeekSuccess;
                        dgvIngredientLibrary.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                    }

                    if (statusText == "Inactive")
                    {
                        dgvIngredientLibrary.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekTextMuted;
                    }
                }
            }
        }

        private void TxtSearchIngredient_TextChanged(object sender, EventArgs e) => LoadIngredientLibrary();

        private void DgvIngredientLibrary_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvIngredientLibrary?.CurrentRow == null) return;

            // Instead of using ID column, get the ingredient by name
            string ingredientName = dgvIngredientLibrary.CurrentRow.Cells["Name"].Value?.ToString();
            if (!string.IsNullOrEmpty(ingredientName))
            {
                var ingredient = dbService.GetAllIngredients().FirstOrDefault(i => i.Name == ingredientName);
                if (ingredient != null && lblSelectedIngredient != null)
                {
                    lblSelectedIngredient.Text = $"{ingredient.Name} ({ingredient.Unit})";
                    selectedIngredientId = ingredient.Id;
                }
            }
        }

        private void BtnApplyAdjustment_Click(object sender, EventArgs e)
        {
            if (!selectedIngredientId.HasValue) { MessageBox.Show("Please select an ingredient first.", "No Ingredient Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            decimal adjustAmount = nudAdjustQuantity?.Value ?? 0;
            bool isAdd = cmbAdjustOperation?.SelectedIndex == 0;
            try
            {
                var inventory = dbService.GetInventoryForIngredient(selectedIngredientId.Value);
                if (inventory != null)
                {
                    if (isAdd) { dbService.AdjustInventory(inventory.Id, adjustAmount); MessageBox.Show($"Added {adjustAmount} to inventory.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                    else
                    {
                        if (inventory.Quantity < adjustAmount)
                        {
                            DialogResult result = MessageBox.Show($"Warning: You're trying to subtract {adjustAmount} but only {inventory.Quantity} is available.\n\nContinue anyway?", "Insufficient Stock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (result == DialogResult.Yes) { dbService.AdjustInventory(inventory.Id, -adjustAmount); MessageBox.Show($"Subtracted {adjustAmount} from inventory.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                        }
                        else { dbService.AdjustInventory(inventory.Id, -adjustAmount); MessageBox.Show($"Subtracted {adjustAmount} from inventory.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                    }
                    LoadIngredientLibrary();
                }
                else { MessageBox.Show("No inventory record found for this ingredient.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show($"Error adjusting inventory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnAddIngredientLib_Click(object sender, EventArgs e) { ClearIngredientForm(); selectedIngredientId = null; btnDeleteIngredient!.Visible = false; pnlIngredientForm!.Visible = true; txtIngredientName!.Focus(); }

        private void BtnEditSelectedIngredient_Click(object sender, EventArgs e)
        {
            if (dgvIngredientLibrary?.CurrentRow == null) return;

            string ingredientName = dgvIngredientLibrary.CurrentRow.Cells["Name"].Value?.ToString();
            if (string.IsNullOrEmpty(ingredientName)) return;

            var ingredient = dbService.GetAllIngredients().FirstOrDefault(i => i.Name == ingredientName);
            if (ingredient != null && pnlIngredientForm != null)
            {
                selectedIngredientId = ingredient.Id;
                txtIngredientName!.Text = ingredient.Name;
                cmbIngredientUnit!.SelectedItem = ingredient.Unit ?? "g";
                nudMinimumStock!.Value = ingredient.MinimumStock;
                chkIngredientActive!.Checked = ingredient.IsActive;

                // REMOVED: Box settings loading code

                btnDeleteIngredient!.Visible = true;
                pnlIngredientForm.Visible = true;
            }
        }

        private void SetBoxControlsEnabled(GroupBox boxSettings, bool enabled)
        {
            if (boxSettings == null) return;

            var lblBoxQty = boxSettings.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Box contains:");
            var nudBoxQuantity = boxSettings.Controls.OfType<NumericUpDown>().FirstOrDefault();
            var lblBoxUnit = boxSettings.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "units per");
            var cmbBoxUnit = boxSettings.Controls.OfType<ComboBox>().FirstOrDefault();
            var lblBoxName = boxSettings.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Called:");
            var txtBoxDisplayName = boxSettings.Controls.OfType<TextBox>().FirstOrDefault();

            if (lblBoxQty != null) lblBoxQty.Enabled = enabled;
            if (nudBoxQuantity != null) nudBoxQuantity.Enabled = enabled;
            if (lblBoxUnit != null) lblBoxUnit.Enabled = enabled;
            if (cmbBoxUnit != null) cmbBoxUnit.Enabled = enabled;
            if (lblBoxName != null) lblBoxName.Enabled = enabled;
            if (txtBoxDisplayName != null) txtBoxDisplayName.Enabled = enabled;
        }

        private void BtnToggleSelectedIngredient_Click(object sender, EventArgs e)
        {
            if (dgvIngredientLibrary?.CurrentRow == null) return;

            string ingredientName = dgvIngredientLibrary.CurrentRow.Cells["Name"].Value?.ToString();
            if (string.IsNullOrEmpty(ingredientName)) return;

            var ingredient = dbService.GetAllIngredients().FirstOrDefault(i => i.Name == ingredientName);
            if (ingredient != null)
            {
                ingredient.IsActive = !ingredient.IsActive;
                dbService.UpdateIngredient(ingredient);
                LoadIngredientLibrary();
            }
        }

        private void BtnDeleteIngredient_Click(object sender, EventArgs e)
        {
            if (!selectedIngredientId.HasValue) return;
            DialogResult result = MessageBox.Show("Are you sure you want to delete this ingredient? This action cannot be undone.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try { dbService.DeleteIngredient(selectedIngredientId.Value); MessageBox.Show("Ingredient deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); pnlIngredientForm!.Visible = false; ClearIngredientForm(); LoadIngredientLibrary(); LoadIngredientCombo(); }
                catch (Exception ex) { MessageBox.Show($"Cannot delete ingredient: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void BtnSaveIngredient_Click(object sender, EventArgs e)
        {
            if (txtIngredientName == null || cmbIngredientUnit == null || nudMinimumStock == null) return;
            if (string.IsNullOrWhiteSpace(txtIngredientName.Text))
            {
                MessageBox.Show("Ingredient name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbIngredientUnit.SelectedItem == null)
            {
                MessageBox.Show("Please select a unit.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedIngredientId.HasValue)
            {
                var ingredient = dbService.GetIngredientById(selectedIngredientId.Value);
                if (ingredient != null)
                {
                    ingredient.Name = txtIngredientName.Text;
                    ingredient.Unit = cmbIngredientUnit.SelectedItem.ToString();
                    ingredient.MinimumStock = (int)nudMinimumStock.Value;
                    ingredient.IsActive = chkIngredientActive?.Checked ?? true;

                    // REMOVED: Box settings saving code

                    dbService.UpdateIngredient(ingredient);
                    MessageBox.Show("Ingredient updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                var ingredient = new Ingredient
                {
                    Name = txtIngredientName.Text,
                    Unit = cmbIngredientUnit.SelectedItem.ToString(),
                    MinimumStock = (int)nudMinimumStock.Value,
                    IsActive = chkIngredientActive?.Checked ?? true
                    // REMOVED: Box settings properties
                };
                dbService.AddIngredient(ingredient);
                MessageBox.Show("Ingredient added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            pnlIngredientForm!.Visible = false;
            ClearIngredientForm();
            LoadIngredientLibrary();
            LoadIngredientCombo();
        }

        private void ClearIngredientForm()
        {
            if (txtIngredientName != null) txtIngredientName.Text = "";
            if (cmbIngredientUnit != null && cmbIngredientUnit.Items.Count > 0) cmbIngredientUnit.SelectedIndex = 0;
            if (nudMinimumStock != null) nudMinimumStock.Value = 10;
            if (chkIngredientActive != null) chkIngredientActive.Checked = true;

            // REMOVED: Box settings clearing code

            selectedIngredientId = null;
            if (lblSelectedIngredient != null)
            {
                lblSelectedIngredient.Text = "None selected";
            }
        }

        private void BtnReorderIngredient_Click(object sender, EventArgs e)
        {
            if (dgvIngredientLibrary?.CurrentRow == null)
            {
                MessageBox.Show("Please select an ingredient to reorder.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int ingredientId = Convert.ToInt32(dgvIngredientLibrary.CurrentRow.Cells["Id"].Value);
            var ingredient = dbService.GetIngredientById(ingredientId);

            if (ingredient == null)
            {
                MessageBox.Show("Ingredient not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Find suppliers that supply this ingredient
            var suppliers = dbService.GetAllSuppliers();
            var suppliersForIngredient = new List<Supplier>();

            foreach (var supplier in suppliers)
            {
                var assignedIngredients = dbService.GetSupplierIngredients(supplier.Id);
                if (assignedIngredients.Contains(ingredientId))
                {
                    suppliersForIngredient.Add(supplier);
                }
            }

            if (suppliersForIngredient.Count == 0)
            {
                MessageBox.Show($"No suppliers found for '{ingredient.Name}'.\n\nPlease assign this ingredient to a supplier first.",
                    "No Suppliers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show supplier selection dialog
            using (Form supplierDialog = new Form())
            {
                supplierDialog.Text = $"Select Supplier for {ingredient.Name}";
                supplierDialog.Size = new Size(400, 320);
                supplierDialog.StartPosition = FormStartPosition.CenterParent;
                supplierDialog.BackColor = DeepSeekSurface;
                supplierDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                supplierDialog.MaximizeBox = false;
                supplierDialog.MinimizeBox = false;

                Label lblInstruction = new Label
                {
                    Text = $"Select a supplier to order {ingredient.Name}:",
                    Location = new Point(20, 20),
                    Size = new Size(360, 30),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = DeepSeekText
                };

                ListBox lstSuppliers = new ListBox
                {
                    Location = new Point(20, 60),
                    Size = new Size(340, 150),
                    BackColor = DeepSeekBg,
                    ForeColor = DeepSeekText,
                    Font = new Font("Segoe UI", 11),
                    BorderStyle = BorderStyle.FixedSingle
                };

                foreach (var supplier in suppliersForIngredient)
                {
                    lstSuppliers.Items.Add(supplier);
                    lstSuppliers.DisplayMember = "Name";
                }
                lstSuppliers.SelectedIndex = 0;

                Label lblCurrentStock = new Label
                {
                    Text = $"Current Stock: {GetCurrentStockDisplay(ingredientId)}",
                    Location = new Point(20, 225),
                    Size = new Size(360, 25),
                    Font = new Font("Segoe UI", 10),
                    ForeColor = DeepSeekTextMuted
                };

                Button btnProceed = new Button
                {
                    Text = "Create Purchase Order",
                    Location = new Point(20, 260),
                    Size = new Size(160, 35),
                    BackColor = DeepSeekSuccess,
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };

                Button btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(195, 260),
                    Size = new Size(100, 35),
                    BackColor = DeepSeekDanger,
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    DialogResult = DialogResult.Cancel
                };

                supplierDialog.Controls.AddRange(new Control[] {
            lblInstruction, lstSuppliers, lblCurrentStock, btnProceed, btnCancel
        });

                if (supplierDialog.ShowDialog() == DialogResult.OK && lstSuppliers.SelectedItem != null)
                {
                    var selectedSupplier = lstSuppliers.SelectedItem as Supplier;
                    if (selectedSupplier != null)
                    {
                        GoToSupplierPurchaseOrder(selectedSupplier.Id, ingredientId);
                    }
                }
            }
        }

        private string GetCurrentStockDisplay(int ingredientId)
        {
            var inventory = dbService.GetInventoryForIngredient(ingredientId);
            var ingredient = dbService.GetIngredientById(ingredientId);
            if (inventory != null && ingredient != null)
            {
                return $"{inventory.Quantity:F1} {ingredient.Unit}";
            }
            return "0";
        }

        private void GoToSupplierPurchaseOrder(int supplierId, int ingredientId)
        {
            // Switch to Supplier Management page
            ShowSupplierManagement();

            // Select the Create Purchase Order tab (index 2)
            if (tcSupplier != null && tcSupplier.TabPages.Count > 2)
            {
                tcSupplier.SelectedIndex = 2;

                // Small delay to allow the tab to render
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 100;
                timer.Tick += (s, e) =>
                {
                    timer.Stop();

                    // Select the supplier in the dropdown
                    if (cmbSupplierSelect != null)
                    {
                        foreach (Supplier supplier in cmbSupplierSelect.Items)
                        {
                            if (supplier.Id == supplierId)
                            {
                                cmbSupplierSelect.SelectedItem = supplier;
                                break;
                            }
                        }
                    }

                    // Find and select the ingredient in the grid
                    if (dgvIngredientsForOrder != null)
                    {
                        foreach (DataGridViewRow row in dgvIngredientsForOrder.Rows)
                        {
                            if (row.Cells["Id"].Value != null && Convert.ToInt32(row.Cells["Id"].Value) == ingredientId)
                            {
                                dgvIngredientsForOrder.ClearSelection();
                                row.Selected = true;
                                dgvIngredientsForOrder.FirstDisplayedScrollingRowIndex = row.Index;
                                break;
                            }
                        }
                    }

                    // Focus on quantity field
                    if (nudOrderQuantity != null)
                    {
                        nudOrderQuantity.Focus();
                        nudOrderQuantity.Select(0, nudOrderQuantity.Text.Length);
                    }

                    MessageBox.Show($"Ready to create purchase order for the selected ingredient.\n\nSet the quantity and click 'Add to Order'.",
                        "Supplier Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                timer.Start();
            }
        }

        #endregion

        private void RefreshIngredientsGrid()
        {
            if (dgvProductIngredients == null || currentProductIngredients == null) return;
            dgvProductIngredients.Rows.Clear();

            foreach (var pi in currentProductIngredients)
            {
                if (pi.Ingredient == null && pi.IngredientId > 0)
                    pi.Ingredient = dbService.GetIngredientById(pi.IngredientId);

                if (pi.Ingredient != null)
                {
                    string smallQty = pi.QuantitySmall.HasValue ? $"{pi.QuantitySmall.Value:F2}" : "0.00";
                    string mediumQty = pi.QuantityMedium.HasValue ? $"{pi.QuantityMedium.Value:F2}" : "0.00";
                    string largeQty = pi.QuantityLarge.HasValue ? $"{pi.QuantityLarge.Value:F2}" : "0.00";

                    int rowIndex = dgvProductIngredients.Rows.Add(
                        pi.Ingredient.Name,
                        pi.Ingredient.Unit ?? "g",
                        smallQty,
                        mediumQty,
                        largeQty
                    );

                    // Store the product ingredient ID in the row tag for reference
                    dgvProductIngredients.Rows[rowIndex].Tag = pi;
                }
            }

            UpdateCostSummary();
        }

        private void UpdateCostSummary()
        {
            return;
        }

        private void BtnSetIngredient_Click(object sender, EventArgs e)
        {
            if (selectedProductId == null)
            {
                MessageBox.Show("Please select a product first.", "No Product Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if an ingredient is selected in the dropdown
            if (cmbIngredient?.SelectedItem == null)
            {
                MessageBox.Show("Please select an ingredient from the dropdown or click an ingredient in the list.",
                    "No Ingredient Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedIngredient = cmbIngredient.SelectedItem as Ingredient;
            if (selectedIngredient == null) return;

            // Get quantities from the numeric up/down controls
            decimal smallQty = nudSmallQty?.Value ?? 0;
            decimal mediumQty = nudMediumQty?.Value ?? 0;
            decimal largeQty = nudLargeQty?.Value ?? 0;

            // ========== VALIDATION FOR CUP INGREDIENTS ==========
            // Check if this is a cup ingredient
            bool isSmallCup = selectedIngredient.Name == "Small Cup";
            bool isMediumCup = selectedIngredient.Name == "Medium Cup";
            bool isLargeCup = selectedIngredient.Name == "Large Cup";

            if (isSmallCup || isMediumCup || isLargeCup)
            {
                // Validate that only ONE size has a value, and it's the correct one
                int nonZeroCount = 0;
                if (smallQty > 0) nonZeroCount++;
                if (mediumQty > 0) nonZeroCount++;
                if (largeQty > 0) nonZeroCount++;

                if (nonZeroCount > 1)
                {
                    MessageBox.Show(
                        $"For '{selectedIngredient.Name}', you can only set ONE size quantity.\n\n" +
                        "Example:\n" +
                        "• Small Cup: Small=1, Medium=0, Large=0\n" +
                        "• Medium Cup: Small=0, Medium=1, Large=0\n" +
                        "• Large Cup: Small=0, Medium=0, Large=1",
                        "Invalid Cup Quantity",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Validate that the quantity is in the correct size column
                if (isSmallCup && mediumQty > 0)
                {
                    MessageBox.Show("Small Cup should only have a value in the 'Small Qty' column.\n\nSet Medium Qty to 0.",
                        "Invalid Cup Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (isMediumCup && smallQty > 0)
                {
                    MessageBox.Show("Medium Cup should only have a value in the 'Medium Qty' column.\n\nSet Small Qty to 0.",
                        "Invalid Cup Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (isMediumCup && largeQty > 0)
                {
                    MessageBox.Show("Medium Cup should only have a value in the 'Medium Qty' column.\n\nSet Large Qty to 0.",
                        "Invalid Cup Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (isLargeCup && mediumQty > 0)
                {
                    MessageBox.Show("Large Cup should only have a value in the 'Large Qty' column.\n\nSet Medium Qty to 0.",
                        "Invalid Cup Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // For cup ingredients, ensure the value is 1 (or 0)
                if (nonZeroCount == 1)
                {
                    // Find which one has the value and force it to 1
                    if (smallQty > 0 && !isSmallCup)
                    {
                        MessageBox.Show($"The value you entered should be in the correct column.\n\n{selectedIngredient.Name} should only have a value in the {selectedIngredient.Name.Replace(" Cup", "")} Qty column.",
                            "Invalid Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (mediumQty > 0 && !isMediumCup)
                    {
                        MessageBox.Show($"The value you entered should be in the correct column.\n\n{selectedIngredient.Name} should only have a value in the {selectedIngredient.Name.Replace(" Cup", "")} Qty column.",
                            "Invalid Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (largeQty > 0 && !isLargeCup)
                    {
                        MessageBox.Show($"The value you entered should be in the correct column.\n\n{selectedIngredient.Name} should only have a value in the {selectedIngredient.Name.Replace(" Cup", "")} Qty column.",
                            "Invalid Assignment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // If it's a cup and has a value, ensure it's exactly 1
                    if (isSmallCup && smallQty > 0 && smallQty != 1)
                    {
                        smallQty = 1;
                        nudSmallQty.Value = 1;
                    }
                    if (isMediumCup && mediumQty > 0 && mediumQty != 1)
                    {
                        mediumQty = 1;
                        nudMediumQty.Value = 1;
                    }
                    if (isLargeCup && largeQty > 0 && largeQty != 1)
                    {
                        largeQty = 1;
                        nudLargeQty.Value = 1;
                    }
                }
            }

            // Find if this ingredient already exists in the current list
            var existingIngredient = currentProductIngredients?.FirstOrDefault(pi => pi.IngredientId == selectedIngredient.Id);

            if (existingIngredient != null)
            {
                // Update existing ingredient with new quantities
                existingIngredient.QuantitySmall = smallQty;
                existingIngredient.QuantityMedium = mediumQty;
                existingIngredient.QuantityLarge = largeQty;
                existingIngredient.Quantity = smallQty > 0 ? smallQty : (mediumQty > 0 ? mediumQty : largeQty);
            }
            else
            {
                // Add new ingredient to the list
                var newIngredient = new ProductIngredient
                {
                    ProductId = selectedProductId.Value,
                    IngredientId = selectedIngredient.Id,
                    QuantitySmall = smallQty,
                    QuantityMedium = mediumQty,
                    QuantityLarge = largeQty,
                    Quantity = smallQty > 0 ? smallQty : (mediumQty > 0 ? mediumQty : largeQty),
                    Ingredient = selectedIngredient
                };

                if (currentProductIngredients == null)
                    currentProductIngredients = new List<ProductIngredient>();

                currentProductIngredients.Add(newIngredient);
            }

            // Refresh the grid to show updated values
            RefreshIngredientsGrid();
            UpdateCostSummary();

            // Reset the input fields
            if (nudSmallQty != null) nudSmallQty.Value = 0;
            if (nudMediumQty != null) nudMediumQty.Value = 0;
            if (nudLargeQty != null) nudLargeQty.Value = 0;
        }



        private void DgvProductIngredients_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore header clicks
            if (e.RowIndex < 0) return;

            // Get the selected row
            var row = dgvProductIngredients?.Rows[e.RowIndex];
            if (row == null) return;

            // Get the ingredient name from the first column
            string ingredientName = row.Cells["Ingredient"].Value?.ToString();
            if (string.IsNullOrEmpty(ingredientName)) return;

            // Find and select the ingredient in the dropdown
            bool found = false;
            foreach (Ingredient item in cmbIngredient.Items)
            {
                if (item.Name == ingredientName)
                {
                    cmbIngredient.SelectedItem = item;
                    found = true;
                    break;
                }
            }

            if (!found) return;

            // Find the corresponding ProductIngredient in the current list
            var selectedIngredient = cmbIngredient.SelectedItem as Ingredient;
            if (selectedIngredient == null) return;

            var pi = currentProductIngredients?.FirstOrDefault(p => p.IngredientId == selectedIngredient.Id);

            if (pi != null)
            {
                // Load the quantities into the numeric up/down controls
                nudSmallQty.Value = pi.QuantitySmall ?? 0;
                nudMediumQty.Value = pi.QuantityMedium ?? 0;
                nudLargeQty.Value = pi.QuantityLarge ?? 0;

                // Update the unit display
                var lblSelectedUnit = pnlIngredientPanel?.Controls
                    .OfType<Label>()
                    .FirstOrDefault(l => l.Name == "lblSelectedUnit");
                if (lblSelectedUnit != null)
                {
                    lblSelectedUnit.Text = selectedIngredient.Unit ?? "g";
                }

                // Highlight the selected row
                dgvProductIngredients.ClearSelection();
                dgvProductIngredients.Rows[e.RowIndex].Selected = true;
            }
            else
            {
                // If not found in current list, reset quantities to 0
                nudSmallQty.Value = 0;
                nudMediumQty.Value = 0;
                nudLargeQty.Value = 0;

                // Update the unit display
                var lblSelectedUnit = pnlIngredientPanel?.Controls
                    .OfType<Label>()
                    .FirstOrDefault(l => l.Name == "lblSelectedUnit");
                if (lblSelectedUnit != null)
                {
                    lblSelectedUnit.Text = selectedIngredient.Unit ?? "g";
                }
            }
        }
        private void BtnRemoveIngredient_Click(object sender, EventArgs e)
        {
            if (dgvProductIngredients?.CurrentRow == null || currentProductIngredients == null) return;
            int index = dgvProductIngredients.CurrentRow.Index;
            if (index >= 0 && index < currentProductIngredients.Count)
            {
                currentProductIngredients.RemoveAt(index);
                RefreshIngredientsGrid();
                UpdateCostSummary();
            }
        }

        private void BtnClearIngredients_Click(object sender, EventArgs e)
        {
            if (currentProductIngredients == null) return;
            DialogResult result = MessageBox.Show("Are you sure you want to clear all ingredients for this product?", "Clear All Ingredients", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                currentProductIngredients.Clear();
                RefreshIngredientsGrid();
                UpdateCostSummary();
            }
        }

        private void BtnSaveIngredients_Click(object sender, EventArgs e)
        {
            if (selectedProductId == null) return;
            try
            {
                int savedCount = dbService.SaveProductIngredients(selectedProductId.Value, currentProductIngredients ?? new List<ProductIngredient>());
                MessageBox.Show($"Saved {savedCount} ingredients for product.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving ingredients: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearIngredientSelection()
        {
            selectedProductId = null;
            selectedProduct = null;
            currentProductIngredients = new List<ProductIngredient>();
            RefreshIngredientsGrid();
            UpdateCostSummary();
        }


        #region Staff Helper Methods

        private string GenerateUsername(string firstName, string lastName)
        {
            // Clean the names (remove special characters, convert to lowercase)
            string cleanFirstName = System.Text.RegularExpressions.Regex.Replace(firstName.Trim(), @"[^a-zA-Z]", "");
            string cleanLastName = System.Text.RegularExpressions.Regex.Replace(lastName.Trim(), @"[^a-zA-Z]", "");

            // Generate base username: firstname.lastname
            string baseUsername = $"{cleanFirstName.ToLower()}.{cleanLastName.ToLower()}";

            // Check if username already exists
            string username = baseUsername;
            int counter = 1;

            while (dbService.GetUserByUsername(username) != null)
            {
                username = $"{baseUsername}{counter}";
                counter++;
            }

            return username;
        }

        private string GeneratePassword(string firstName, string lastName)
        {
            // Clean the names
            string cleanFirstName = System.Text.RegularExpressions.Regex.Replace(firstName.Trim(), @"[^a-zA-Z]", "");
            string cleanLastName = System.Text.RegularExpressions.Regex.Replace(lastName.Trim(), @"[^a-zA-Z]", "");

            // Generate password: First letter of first name + last name + random numbers
            string firstInitial = cleanFirstName.Length > 0 ? cleanFirstName.Substring(0, 1).ToUpper() : "X";
            string lastNameLower = cleanLastName.ToLower();

            // Generate random 4-digit number
            Random random = new Random();
            int randomNum = random.Next(1000, 9999);

            string password = $"{firstInitial}{lastNameLower}{randomNum}";

            // Ensure password meets minimum length (at least 6 characters)
            if (password.Length < 6)
            {
                password = password + randomNum.ToString();
            }

            return password;
        }

        #endregion

        #region Staff Management

        private void ShowStaffManagement()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "User Management",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true
            };

            Panel pnlStats = new Panel
            {
                Location = new Point(30, 70),
                Size = new Size(pnlContent.Width - 60, 100),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlStats.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlStats.Width - 1, pnlStats.Height - 1);
                }
            };

            var staff = dbService.GetAllStaff();
            int totalStaff = staff.Count;
            int activeStaff = staff.Count(s => s.IsActive);
            int inactiveStaff = totalStaff - activeStaff;

            Label lblTotalStaff = new Label { Text = $"Total Staff: {totalStaff}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(30, 35), AutoSize = true };
            Label lblActiveStaff = new Label { Text = $"Active: {activeStaff}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekSuccess, Location = new Point(300, 35), AutoSize = true };
            Label lblInactiveStaff = new Label { Text = $"Inactive: {inactiveStaff}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekDanger, Location = new Point(500, 35), AutoSize = true };

            pnlStats.Controls.AddRange(new Control[] { lblTotalStaff, lblActiveStaff, lblInactiveStaff });

            Panel pnlSearch = new Panel
            {
                Location = new Point(30, 180),
                Size = new Size(pnlContent.Width - 60, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtSearchStaff = new TextBox
            {
                Location = new Point(0, 8),
                Size = new Size(350, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search staff by name, role...",
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchStaff.TextChanged += TxtSearchStaff_TextChanged!;

            Button btnAddStaff = new Button
            {
                Text = "+ Add New Staff",
                Location = new Point(370, 8),
                Size = new Size(160, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnAddStaff.Click += BtnAddStaff_Click!;

            Button btnEditStaff = new Button
            {
                Text = "Edit Selected",
                Location = new Point(550, 8),
                Size = new Size(120, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnEditStaff.Click += BtnEditSelectedStaff_Click!;

            Button btnToggleStaff = new Button
            {
                Text = "Toggle Status",
                Location = new Point(690, 8),
                Size = new Size(120, 35),
                BackColor = DeepSeekWarning,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnToggleStaff.Click += BtnToggleSelectedStaff_Click!;

            Button btnResetStaffPwd = new Button
            {
                Text = "Reset Password",
                Location = new Point(830, 8),
                Size = new Size(130, 35),
                BackColor = DeepSeekInfo,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnResetStaffPwd.Click += BtnResetSelectedStaffPassword_Click!;

            Button btnDeleteStaff = new Button
            {
                Text = "Delete Staff",
                Location = new Point(970, 8),
                Size = new Size(120, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnDeleteStaff.Click += BtnDeleteStaff_Click!;

            pnlSearch.Controls.AddRange(new Control[] {
        txtSearchStaff, btnAddStaff, btnEditStaff, btnToggleStaff, btnResetStaffPwd, btnDeleteStaff
    });

            dgvStaff = CreateDataGridView(new Point(30, 240), new Size(pnlContent.Width - 60, 300));
            dgvStaff.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvStaff.CellClick += DgvStaff_CellClick!;
            dgvStaff.CellDoubleClick += DgvStaff_CellDoubleClick!;

            dgvStaff.Columns.Add("Id", "ID");
            dgvStaff.Columns.Add("Name", "Full Name");
            dgvStaff.Columns.Add("Username", "Username");
            dgvStaff.Columns.Add("Role", "Role");
            dgvStaff.Columns.Add("Contact", "Contact");
            dgvStaff.Columns.Add("Status", "Status");

            // Add context menu for restore option
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = DeepSeekSurface;
            contextMenu.ForeColor = DeepSeekText;

            ToolStripMenuItem restoreItem = new ToolStripMenuItem("Restore Staff");
            restoreItem.ForeColor = DeepSeekSuccess;
            restoreItem.Click += (s, e) =>
            {
                if (dgvStaff?.CurrentRow == null) return;

                int staffId = Convert.ToInt32(dgvStaff.CurrentRow.Cells["Id"].Value);
                string staffName = dgvStaff.CurrentRow.Cells["Name"].Value?.ToString() ?? "Unknown";
                string staffStatus = dgvStaff.CurrentRow.Cells["Status"].Value?.ToString() ?? "";

                if (staffStatus != "Inactive")
                {
                    MessageBox.Show("Only inactive staff members can be restored.", "Cannot Restore",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult result = MessageBox.Show($"Restore {staffName}?", "Confirm Restore",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        dbService.RestoreStaff(staffId);
                        MessageBox.Show($"{staffName} has been restored.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadStaff();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error restoring staff: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            contextMenu.Items.Add(restoreItem);
            dgvStaff.ContextMenuStrip = contextMenu;

            // Staff Form Panel
            pnlStaffForm = new Panel
            {
                Location = new Point(30, 550),
                Size = new Size(pnlContent.Width - 60, 260),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblFormTitle = new Label
            {
                Text = "Staff Details",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };

            // First Name
            Label lblFirstName = new Label
            {
                Text = "First Name*:",
                Location = new Point(15, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            txtStaffName = new TextBox
            {
                Name = "txtStaffName",
                Location = new Point(130, 52),
                Size = new Size(200, 35),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Enter first name"
            };

            // Last Name
            Label lblLastName = new Label
            {
                Text = "Last Name*:",
                Location = new Point(350, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            TextBox txtStaffLastName = new TextBox
            {
                Name = "txtStaffLastName",
                Location = new Point(460, 52),
                Size = new Size(200, 35),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Enter last name",
            };

            // Username
            Label lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(15, 105),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            TextBox txtStaffUsername = new TextBox
            {
                Name = "txtStaffUsername",
                Location = new Point(130, 102),
                Size = new Size(200, 35),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekAccent,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
            };

            // Password
            Label lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(350, 105),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            TextBox txtStaffPasswordDisplay = new TextBox
            {
                Location = new Point(460, 102),
                Size = new Size(200, 35),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekAccent,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Name = "txtStaffPasswordDisplay"
            };

            // Contact Number
            Label lblContact = new Label
            {
                Text = "Contact*:",
                Location = new Point(15, 155),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            txtStaffContact = new TextBox
            {
                Name = "txtStaffContact",
                Location = new Point(130, 152),
                Size = new Size(200, 35),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Contact number"
            };

            // Role
            Label lblRole = new Label
            {
                Text = "Role*:",
                Location = new Point(350, 155),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekTextMuted
            };
            cmbStaffRole = new ComboBox
            {
                Name = "cmbStaffRole",
                Location = new Point(460, 152),
                Size = new Size(150, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat
            };
            cmbStaffRole.Items.AddRange(new object[] { "Cashier", "Manager" });
            cmbStaffRole.SelectedIndex = 0;

            // Generate Username button
            Button btnGenerateUsername = new Button
            {
                Text = "🔄 Generate",
                Location = new Point(340, 102),
                Size = new Size(90, 35),
                BackColor = DeepSeekInfo,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            btnGenerateUsername.Click += (s, e) =>
            {
                string firstName = txtStaffName?.Text ?? "";
                string lastName = txtStaffLastName.Text;
                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    string generatedUsername = GenerateUsername(firstName, lastName);
                    txtStaffUsername.Text = generatedUsername;

                    string generatedPassword = GeneratePassword(firstName, lastName);
                    txtStaffPasswordDisplay.Text = generatedPassword;

                    MessageBox.Show($"Username: {generatedUsername}\nPassword: {generatedPassword}\n\nPlease save this information for the staff member.",
                        "Credentials Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please enter both first name and last name first.", "Missing Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // Generate Password button
            Button btnGeneratePassword = new Button
            {
                Text = "🔄 Generate Pwd",
                Location = new Point(670, 102),
                Size = new Size(100, 35),
                BackColor = DeepSeekInfo,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            btnGeneratePassword.Click += (s, e) =>
            {
                string firstName = txtStaffName?.Text ?? "";
                string lastName = txtStaffLastName.Text;
                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    string generatedPassword = GeneratePassword(firstName, lastName);
                    txtStaffPasswordDisplay.Text = generatedPassword;
                    MessageBox.Show($"New Password: {generatedPassword}",
                        "Password Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please enter both first name and last name first.", "Missing Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // Show/Hide Password button
            Button btnTogglePassword = new Button
            {
                Text = "👁️",
                Location = new Point(775, 102),
                Size = new Size(40, 35),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                FlatAppearance = { BorderSize = 1, BorderColor = DeepSeekBorder },
                Cursor = Cursors.Hand
            };
            btnTogglePassword.Click += (s, e) =>
            {
                txtStaffPasswordDisplay.UseSystemPasswordChar = !txtStaffPasswordDisplay.UseSystemPasswordChar;
                btnTogglePassword.Text = txtStaffPasswordDisplay.UseSystemPasswordChar ? "👁️" : "🔒";
            };

            // Save and Cancel buttons
            btnSaveStaff = new Button
            {
                Text = "Save Staff",
                Location = new Point(15, 200),
                Size = new Size(140, 40),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnSaveStaff.Click += BtnSaveStaff_Click!;

            btnCancelStaff = new Button
            {
                Text = "Cancel",
                Location = new Point(170, 200),
                Size = new Size(120, 40),
                BackColor = DeepSeekTextMuted,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancelStaff.Click += (s, e) => { pnlStaffForm.Visible = false; ClearStaffForm(); };

            pnlStaffForm.Controls.AddRange(new Control[] {
        lblFormTitle,
        lblFirstName, txtStaffName,
        lblLastName, txtStaffLastName,
        lblUsername, txtStaffUsername, btnGenerateUsername,
        lblPassword, txtStaffPasswordDisplay, btnGeneratePassword, btnTogglePassword,
        lblContact, txtStaffContact,
        lblRole, cmbStaffRole,
        btnSaveStaff, btnCancelStaff
    });

            pnlContent.Controls.AddRange(new Control[] { lblTitle, pnlStats, pnlSearch, dgvStaff, pnlStaffForm });
            LoadStaff();

            // Handle resize
            pnlContent.Resize += (s, e) =>
            {
                pnlStats.Width = pnlContent.Width - 60;
                pnlSearch.Width = pnlContent.Width - 60;
                dgvStaff.Width = pnlContent.Width - 60;
                pnlStaffForm.Width = pnlContent.Width - 60;
            };
        }

        #endregion

        #region Staff Management Methods

        private void LoadStaff()
        {
            if (dgvStaff == null) return;
            dgvStaff.Rows.Clear();
            var staff = dbService.GetAllStaff();
            string searchText = txtSearchStaff?.Text?.ToLower() ?? "";

            foreach (var user in staff)
            {
                if (!string.IsNullOrEmpty(searchText) &&
                    !(user.FullName?.ToLower().Contains(searchText) ?? false) &&
                    !user.Username.ToLower().Contains(searchText) &&
                    !user.Role.ToLower().Contains(searchText))
                    continue;

                int rowIndex = dgvStaff.Rows.Add(
                    user.Id,
                    user.FullName ?? "N/A",
                    user.Username,
                    user.Role,
                    user.Contact ?? "N/A",
                    user.IsActive ? "Active" : "Inactive"
                );

                if (user.IsActive)
                {
                    dgvStaff.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                }
                else
                {
                    dgvStaff.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                    // Style inactive rows with strikethrough or different color
                    dgvStaff.Rows[rowIndex].DefaultCellStyle.ForeColor = DeepSeekTextMuted;
                    dgvStaff.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Strikeout);
                }
            }
        }

        private void TxtSearchStaff_TextChanged(object sender, EventArgs e)
        {
            LoadStaff();
        }

        private void BtnAddStaff_Click(object sender, EventArgs e)
        {
            ClearStaffForm();
            selectedStaffId = null;

            // Clear the username and password fields for new staff
            var txtStaffUsername = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffUsername");
            var txtStaffPasswordDisplay = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffPasswordDisplay");

            if (txtStaffUsername != null) txtStaffUsername.Text = "";
            if (txtStaffPasswordDisplay != null)
            {
                txtStaffPasswordDisplay.Text = "";
                txtStaffPasswordDisplay.ReadOnly = false;
                txtStaffPasswordDisplay.UseSystemPasswordChar = false;
            }

            if (pnlStaffForm != null)
            {
                pnlStaffForm.Visible = true;
                txtStaffName?.Focus();
            }
        }

        private void BtnEditSelectedStaff_Click(object sender, EventArgs e)
        {
            if (dgvStaff?.CurrentRow == null)
            {
                MessageBox.Show("Please select a staff member to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int staffId = Convert.ToInt32(dgvStaff.CurrentRow.Cells["Id"].Value);
            var staff = dbService.GetStaffById(staffId);

            if (staff == null)
            {
                MessageBox.Show("Staff member not found.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Store the selected staff ID
            selectedStaffId = staff.Id;

            // Find all the controls in pnlStaffForm
            var txtFirstName = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffName");
            var txtLastName = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffLastName");
            var txtUsername = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffUsername");
            var txtPasswordDisplay = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffPasswordDisplay");
            var txtContact = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffContact");
            var cmbRole = pnlStaffForm?.Controls.OfType<ComboBox>().FirstOrDefault(c => c.Name == "cmbStaffRole");

            // Split full name into first and last name
            string[] nameParts = staff.FullName?.Split(new[] { ' ' }, 2) ?? new[] { "", "" };
            string firstName = nameParts.Length > 0 ? nameParts[0] : "";
            string lastName = nameParts.Length > 1 ? nameParts[1] : "";

            // Populate the form
            if (txtFirstName != null) txtFirstName.Text = firstName;
            if (txtLastName != null) txtLastName.Text = lastName;
            if (txtUsername != null) txtUsername.Text = staff.Username;
            if (txtContact != null) txtContact.Text = staff.Contact ?? "";
            if (cmbRole != null) cmbRole.SelectedItem = staff.Role;

            // For password field - show placeholder and make it read-only
            if (txtPasswordDisplay != null)
            {
                txtPasswordDisplay.Text = "●●●●●●●●";
                txtPasswordDisplay.ReadOnly = true;
                txtPasswordDisplay.UseSystemPasswordChar = true;
            }

            // Show the form
            if (pnlStaffForm != null)
            {
                pnlStaffForm.Visible = true;
                pnlStaffForm.BringToFront();
            }
        }

        private void BtnToggleSelectedStaff_Click(object sender, EventArgs e)
        {
            if (dgvStaff?.CurrentRow == null) return;
            int staffId = Convert.ToInt32(dgvStaff.CurrentRow.Cells["Id"].Value);
            dbService.ToggleStaffStatus(staffId);
            LoadStaff();
        }

        private void BtnResetSelectedStaffPassword_Click(object sender, EventArgs e)
        {
            if (dgvStaff?.CurrentRow == null) return;
            int staffId = Convert.ToInt32(dgvStaff.CurrentRow.Cells["Id"].Value);
            var staff = dbService.GetStaffById(staffId);

            if (staff != null)
            {
                // Generate new password based on staff name
                string[] nameParts = staff.FullName?.Split(new[] { ' ' }, 2) ?? new[] { "", "" };
                string firstName = nameParts.Length > 0 ? nameParts[0] : "";
                string lastName = nameParts.Length > 1 ? nameParts[1] : "";

                string newPassword = GeneratePassword(firstName, lastName);
                dbService.ResetStaffPassword(staffId, newPassword);

                MessageBox.Show($"Password reset for {staff.FullName}!\n\nNew Password: {newPassword}\n\nPlease provide this to the staff member.",
                    "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



        private void BtnDeleteStaff_Click(object sender, EventArgs e)
        {
            if (dgvStaff?.CurrentRow == null)
            {
                MessageBox.Show("Please select a staff member to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int staffId = Convert.ToInt32(dgvStaff.CurrentRow.Cells["Id"].Value);
            string staffName = dgvStaff.CurrentRow.Cells["Name"].Value?.ToString() ?? "Unknown";
            string staffStatus = dgvStaff.CurrentRow.Cells["Status"].Value?.ToString() ?? "Active";

            // Check if trying to delete yourself
            if (staffId == userId)
            {
                MessageBox.Show("You cannot delete your own account.", "Cannot Delete Self",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete {staffName}?\n\n" +
                $"Status: {staffStatus}\n\n" +
                "If this staff member has transaction history, they will be deactivated instead of permanently deleted.\n\n" +
                "This action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    dbService.DeleteStaff(staffId);
                    MessageBox.Show($"Staff member {staffName} has been deleted/deactivated successfully.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStaff(); // Refresh the grid
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Delete Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DgvStaff_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional: Handle cell click if needed
        }

        private void DgvStaff_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            BtnEditSelectedStaff_Click(sender, e);
        }


        private void BtnSaveStaff_Click(object sender, EventArgs e)
        {
            // Find all the controls
            var txtStaffLastName = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffLastName");
            var txtStaffUsername = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffUsername");
            var txtStaffPasswordDisplay = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffPasswordDisplay");

            string firstName = txtStaffName?.Text?.Trim() ?? "";
            string lastName = txtStaffLastName?.Text?.Trim() ?? "";
            string username = txtStaffUsername?.Text?.Trim() ?? "";
            string contact = txtStaffContact?.Text?.Trim() ?? "";
            string role = cmbStaffRole?.SelectedItem?.ToString() ?? "Cashier";

            // Validation
            if (string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("First name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStaffName?.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Last name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStaffLastName?.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(contact))
            {
                MessageBox.Show("Contact number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStaffContact?.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStaffUsername?.Focus();
                return;
            }

            string fullName = $"{firstName} {lastName}";

            // Check if we are in EDIT mode (selectedStaffId has a value)
            if (selectedStaffId.HasValue && selectedStaffId.Value > 0)
            {
                // UPDATE existing staff
                try
                {
                    var staff = dbService.GetStaffById(selectedStaffId.Value);
                    if (staff != null)
                    {
                        staff.FullName = fullName;
                        staff.Username = username;  // ADD THIS - update username
                        staff.Contact = contact;
                        staff.Role = role;
                        staff.UpdatedAt = DateTime.Now;

                        dbService.UpdateStaff(staff);
                        MessageBox.Show($"Staff '{fullName}' updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Close form and refresh grid
                        pnlStaffForm!.Visible = false;
                        ClearStaffForm();
                        LoadStaff();
                    }
                    else
                    {
                        MessageBox.Show("Staff member not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating staff: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // ADD new staff
                try
                {
                    string password = txtStaffPasswordDisplay?.Text ?? GeneratePassword(firstName, lastName);

                    var newStaff = new User
                    {
                        Username = username,
                        FullName = fullName,
                        Contact = contact,
                        Role = role,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    dbService.AddStaff(newStaff, password);
                    MessageBox.Show($"Staff '{fullName}' added successfully!\n\nUsername: {username}\nPassword: {password}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Close form and refresh grid
                    pnlStaffForm!.Visible = false;
                    ClearStaffForm();
                    LoadStaff();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding staff: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void ClearStaffForm()
        {
            // Clear textboxes
            if (txtStaffName != null) txtStaffName.Text = "";

            var txtStaffLastName = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffLastName");
            if (txtStaffLastName != null) txtStaffLastName.Text = "";

            var txtStaffUsername = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffUsername");
            if (txtStaffUsername != null) txtStaffUsername.Text = "";

            var txtStaffPasswordDisplay = pnlStaffForm?.Controls.OfType<TextBox>().FirstOrDefault(c => c.Name == "txtStaffPasswordDisplay");
            if (txtStaffPasswordDisplay != null)
            {
                txtStaffPasswordDisplay.Text = "";
                txtStaffPasswordDisplay.ReadOnly = false;
                txtStaffPasswordDisplay.UseSystemPasswordChar = false;
            }

            if (txtStaffContact != null) txtStaffContact.Text = "";
            if (cmbStaffRole != null && cmbStaffRole.Items.Count > 0) cmbStaffRole.SelectedIndex = 0;

            // CRITICAL: Reset the selectedStaffId so next save knows it's a NEW staff
            selectedStaffId = null;
        }

        #endregion





        #region Supplier Management with Purchase Orders

        private void ShowSupplierManagement()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();
            orderCart.Clear();
            LoadSupplierIngredientsCache();

            Label lblTitle = new Label
            {
                Text = "Supplier Management",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true
            };

            // Create Tab Control
            tcSupplier = new TabControl
            {
                Location = new Point(30, 70),
                Size = new Size(pnlContent.Width - 60, pnlContent.Height - 100),
                Font = new Font("Segoe UI", 11),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Handle resize
            pnlContent.Resize += (s, e) =>
            {
                if (tcSupplier != null)
                {
                    tcSupplier.Size = new Size(pnlContent.Width - 60, pnlContent.Height - 100);
                }
            };

            // ========== TAB 1: SUPPLIERS LIST ==========
            TabPage tpSuppliers = new TabPage("Suppliers List");
            tpSuppliers.BackColor = DeepSeekBg;
            tpSuppliers.AutoScroll = true;
            tpSuppliers.Padding = new Padding(0);

            // Stats Panel
            Panel pnlStats = new Panel
            {
                Location = new Point(15, 15),
                Size = new Size(tpSuppliers.Width - 30, 80),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlStats.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlStats.Width - 1, pnlStats.Height - 1);
                }
            };

            var allSuppliers = dbService.GetAllSuppliers();
            int totalSuppliers = allSuppliers.Count;
            int activeSuppliers = allSuppliers.Count(s => s.IsActive);
            int inactiveSuppliers = totalSuppliers - activeSuppliers;

            Label lblTotalSuppliers = new Label
            {
                Text = $"Total suppliers: {totalSuppliers}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 25),
                AutoSize = true
            };
            Label lblActiveSuppliers = new Label
            {
                Text = $"Active: {activeSuppliers}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekSuccess,
                Location = new Point(250, 25),
                AutoSize = true
            };
            Label lblInactiveSuppliers = new Label
            {
                Text = $"Inactive: {inactiveSuppliers}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekDanger,
                Location = new Point(420, 25),
                AutoSize = true
            };

            pnlStats.Controls.AddRange(new Control[] { lblTotalSuppliers, lblActiveSuppliers, lblInactiveSuppliers });

            // Search Panel
            Panel pnlSearch = new Panel
            {
                Location = new Point(15, 105),
                Size = new Size(tpSuppliers.Width - 30, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtSearchSupplier = new TextBox
            {
                Location = new Point(0, 5),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search suppliers...",
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchSupplier.TextChanged += TxtSearchSupplier_TextChanged!;

            Button btnAddSupplier = new Button
            {
                Text = "+ Add New",
                Location = new Point(310, 5),
                Size = new Size(100, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnAddSupplier.Click += BtnAddSupplier_Click!;

            Button btnEditSupplier = new Button
            {
                Text = "Edit",
                Location = new Point(420, 5),
                Size = new Size(80, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnEditSupplier.Click += BtnEditSelectedSupplier_Click!;

            Button btnToggleSupplier = new Button
            {
                Text = "Toggle",
                Location = new Point(510, 5),
                Size = new Size(80, 35),
                BackColor = DeepSeekWarning,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnToggleSupplier.Click += BtnToggleSupplierStatus_Click!;

            pnlSearch.Controls.AddRange(new Control[] { txtSearchSupplier, btnAddSupplier, btnEditSupplier, btnToggleSupplier });

            // Suppliers Grid - NO ID COLUMN
            dgvSuppliers = CreateDataGridView(new Point(15, 160), new Size(tpSuppliers.Width - 30, 280));
            dgvSuppliers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvSuppliers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Add columns - NO ID column
            dgvSuppliers.Columns.Add("Name", "Name");
            dgvSuppliers.Columns.Add("ContactNumber", "Contact #");
            dgvSuppliers.Columns.Add("Email", "Email");
            dgvSuppliers.Columns.Add("ItemCategory", "Category");
            dgvSuppliers.Columns.Add("Status", "Status");

            // Set column fill weights
            dgvSuppliers.Columns["Name"].FillWeight = 30;
            dgvSuppliers.Columns["ContactNumber"].FillWeight = 15;
            dgvSuppliers.Columns["Email"].FillWeight = 25;
            dgvSuppliers.Columns["ItemCategory"].FillWeight = 15;
            dgvSuppliers.Columns["Status"].FillWeight = 15;

            // Set minimum widths
            dgvSuppliers.Columns["Name"].MinimumWidth = 150;
            dgvSuppliers.Columns["ContactNumber"].MinimumWidth = 100;
            dgvSuppliers.Columns["Email"].MinimumWidth = 150;
            dgvSuppliers.Columns["ItemCategory"].MinimumWidth = 80;
            dgvSuppliers.Columns["Status"].MinimumWidth = 60;

            // Supplier Form Panel - Positioned at bottom
            pnlSupplierForm = new Panel
            {
                Location = new Point(15, 425),
                Size = new Size(tpSuppliers.Width - 30, 260),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblFormTitle = new Label
            {
                Text = "Supplier Details",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 10),
                AutoSize = true
            };

            // Row 1: Name and Contact
            Label lblName = new Label { Text = "Name*:", Location = new Point(15, 45), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = DeepSeekTextMuted };
            txtSupplierName = new TextBox { Location = new Point(80, 42), Size = new Size(200, 30), BackColor = DeepSeekBg, ForeColor = DeepSeekText, BorderStyle = BorderStyle.FixedSingle };

            Label lblContact = new Label { Text = "Contact #:", Location = new Point(300, 45), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = DeepSeekTextMuted };
            txtSupplierContact = new TextBox { Location = new Point(370, 42), Size = new Size(180, 30), BackColor = DeepSeekBg, ForeColor = DeepSeekText, BorderStyle = BorderStyle.FixedSingle };

            // Row 2: Email and Category
            Label lblEmail = new Label { Text = "Email:", Location = new Point(15, 85), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = DeepSeekTextMuted };
            txtSupplierEmail = new TextBox { Location = new Point(80, 82), Size = new Size(200, 30), BackColor = DeepSeekBg, ForeColor = DeepSeekText, BorderStyle = BorderStyle.FixedSingle };

            Label lblCategory = new Label { Text = "Category:", Location = new Point(300, 85), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = DeepSeekTextMuted };
            cmbItemCategory = new ComboBox
            {
                Location = new Point(370, 82),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat
            };
            cmbItemCategory.Items.AddRange(new object[] { "Coffee Beans", "Dairy", "Syrups", "Sweeteners", "Flour", "Cheese", "Fruits", "Meat", "Packaging", "Equipment" });

            // Row 3: Address
            Label lblAddress = new Label { Text = "Address:", Location = new Point(15, 125), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = DeepSeekTextMuted };
            txtSupplierAddress = new TextBox { Location = new Point(80, 122), Size = new Size(300, 30), BackColor = DeepSeekBg, ForeColor = DeepSeekText, BorderStyle = BorderStyle.FixedSingle };

            // Row 4: Buttons
            btnSaveSupplier = new Button
            {
                Text = "Save Supplier",
                Location = new Point(15, 158),
                Size = new Size(120, 28),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnSaveSupplier.Click += BtnSaveSupplier_Click!;

            btnCancelSupplier = new Button
            {
                Text = "Cancel",
                Location = new Point(145, 158),
                Size = new Size(100, 28),
                BackColor = DeepSeekTextMuted,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancelSupplier.Click += (s, e) => { pnlSupplierForm.Visible = false; ClearSupplierForm(); };

            pnlSupplierForm.Controls.AddRange(new Control[] {
        lblFormTitle,
        lblName, txtSupplierName,
        lblContact, txtSupplierContact,
        lblEmail, txtSupplierEmail,
        lblCategory, cmbItemCategory,
        lblAddress, txtSupplierAddress,
        btnSaveSupplier, btnCancelSupplier
    });

            tpSuppliers.Controls.AddRange(new Control[] { pnlStats, pnlSearch, dgvSuppliers, pnlSupplierForm });

            // Handle tab page resize - updated for taller form
            tpSuppliers.Resize += (s, e) =>
            {
                int tabWidth = tpSuppliers.Width - 30;
                int tabHeight = tpSuppliers.Height;

                pnlStats.Width = tabWidth;
                pnlSearch.Width = tabWidth;
                dgvSuppliers.Width = tabWidth;
                dgvSuppliers.Height = Math.Max(150, tabHeight - 480);  // Increased from 380 to 420
                pnlSupplierForm.Width = tabWidth;
                pnlSupplierForm.Location = new Point(15, Math.Max(280, tabHeight - 240));  // Adjusted position
                pnlSupplierForm.Height = Math.Min(220, tabHeight - dgvSuppliers.Height - 290);
            };

            // ========== TAB 2: ASSIGN INGREDIENTS ==========
            TabPage tpAssignIngredients = new TabPage("Assign Ingredients")
            {
                BackColor = DeepSeekBg,
                AutoScroll = true,
                Padding = new Padding(10)  // Add padding
            };
            tpAssignIngredients.Name = "tpAssignIngredients";

            Panel pnlAssign = CreateAssignIngredientsPanel();
            tpAssignIngredients.Controls.Add(pnlAssign);

            // ========== TAB 3: CREATE PURCHASE ORDER ==========
            TabPage tpCreateOrder = new TabPage("Create Purchase Order");
            tpCreateOrder.BackColor = DeepSeekBg;
            tpCreateOrder.AutoScroll = true;

            Panel pnlCreateOrder = CreatePurchaseOrderPanel();
            tpCreateOrder.Controls.Add(pnlCreateOrder);

            // ========== TAB 4: PURCHASE ORDERS LIST ==========
            TabPage tpOrdersList = new TabPage("Purchase Orders List");
            tpOrdersList.BackColor = DeepSeekBg;

            Panel pnlOrdersList = CreatePurchaseOrdersListPanel();
            tpOrdersList.Controls.Add(pnlOrdersList);

            tcSupplier.TabPages.Add(tpSuppliers);
            tcSupplier.TabPages.Add(tpAssignIngredients);
            tcSupplier.TabPages.Add(tpCreateOrder);
            tcSupplier.TabPages.Add(tpOrdersList);

            pnlContent.Controls.AddRange(new Control[] { lblTitle, tcSupplier });

            LoadSuppliers();
            LoadPurchaseOrders();

            pnlContent.Resize += (s, e) =>
            {
                if (tcSupplier != null)
                {
                    tcSupplier.Size = new Size(pnlContent.Width - 60, pnlContent.Height - 100);
                }
            };
        }

        private void RefreshCreatePurchaseOrderIngredients()
        {
            // This method refreshes the ingredients list in the Create Purchase Order tab
            if (cmbSupplierSelect != null && cmbSupplierSelect.SelectedItem != null)
            {
                var currentSupplier = cmbSupplierSelect.SelectedItem as Supplier;
                if (currentSupplier != null)
                {
                    // Reload the ingredients for the current supplier
                    LoadIngredientsForOrder();
                }
            }
        }

        private void RefreshSupplierComboInCreateOrder()
        {
            // Refresh the supplier combo box in the Create Purchase Order tab
            if (cmbSupplierSelect != null)
            {
                var currentSupplier = cmbSupplierSelect.SelectedItem as Supplier;
                int? currentSupplierId = currentSupplier?.Id;

                cmbSupplierSelect.Items.Clear();
                var suppliers = dbService.GetAllSuppliers().Where(s => s.IsActive).ToList();

                foreach (var supplier in suppliers)
                {
                    cmbSupplierSelect.Items.Add(supplier);
                }
                cmbSupplierSelect.DisplayMember = "Name";
                cmbSupplierSelect.ValueMember = "Id";

                if (currentSupplierId.HasValue && suppliers.Any(s => s.Id == currentSupplierId.Value))
                {
                    cmbSupplierSelect.SelectedItem = suppliers.First(s => s.Id == currentSupplierId.Value);
                }
                else if (cmbSupplierSelect.Items.Count > 0)
                {
                    cmbSupplierSelect.SelectedIndex = 0;
                }
            }
        }
        private Panel CreateAssignIngredientsPanel()
        {
            Panel pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekBg,
                AutoScroll = true,
                Padding = new Padding(15)
            };

            // Supplier Selection Panel
            Panel pnlSupplierSelect = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(pnlMain.Width - 30, 80),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlSupplierSelect.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSupplierSelect.Width - 1, pnlSupplierSelect.Height - 1);
                }
            };

            Label lblSelectSupplier = new Label
            {
                Text = "Select Supplier:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 25),
                AutoSize = true
            };

            ComboBox cmbSupplierForIngredients = new ComboBox
            {
                Location = new Point(150, 22),
                Size = new Size(350, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Name = "cmbSupplierForIngredients",
                DropDownWidth = 350,
                IntegralHeight = false,
                MaxDropDownItems = 10
            };

            // Load all suppliers into the combo box
            var allSuppliers = dbService.GetAllSuppliers().Where(s => s.IsActive).ToList();
            foreach (var supplier in allSuppliers)
            {
                cmbSupplierForIngredients.Items.Add(supplier);
            }
            cmbSupplierForIngredients.DisplayMember = "Name";
            cmbSupplierForIngredients.ValueMember = "Id";

            if (cmbSupplierForIngredients.Items.Count > 0)
            {
                cmbSupplierForIngredients.SelectedIndex = 0;
            }

            pnlSupplierSelect.Controls.AddRange(new Control[] {
        lblSelectSupplier, cmbSupplierForIngredients
    });

            // Ingredients List Panel
            Panel pnlIngredients = new Panel
            {
                Location = new Point(0, 90),
                Size = new Size(pnlMain.Width - 30, pnlMain.Height - 100),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            pnlIngredients.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlIngredients.Width - 1, pnlIngredients.Height - 1);
                }
            };

            // Search bar for ingredients
            Panel pnlSearch = new Panel
            {
                Location = new Point(15, 15),
                Size = new Size(pnlIngredients.Width - 30, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSearchIngredients = new Label
            {
                Text = "Search Ingredients:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(0, 10),
                AutoSize = true
            };

            TextBox txtIngredientSearch = new TextBox
            {
                Location = new Point(130, 5),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Type to filter ingredients...",
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            pnlSearch.Controls.AddRange(new Control[] { lblSearchIngredients, txtIngredientSearch });

            // ========== DECLARE clbIngredients FIRST ==========
            CheckedListBox clbIngredients = new CheckedListBox
            {
                Location = new Point(15, 115),
                Size = new Size(pnlIngredients.Width - 30, pnlIngredients.Height - 140),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                CheckOnClick = true,
                Enabled = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Load all ingredients initially
            List<Ingredient> allIngredients = dbService.GetAllIngredients().Where(i => i.IsActive).ToList();
            List<Ingredient> filteredIngredients = new List<Ingredient>(allIngredients);

            // Function to refresh the checked list box based on search text
            void RefreshIngredientList(string searchText)
            {
                clbIngredients.BeginUpdate();
                clbIngredients.Items.Clear();

                filteredIngredients = allIngredients
                    .Where(i => string.IsNullOrEmpty(searchText) || i.Name.ToLower().Contains(searchText.ToLower()))
                    .ToList();

                foreach (var ingredient in filteredIngredients)
                {
                    clbIngredients.Items.Add(ingredient, false);
                }
                clbIngredients.DisplayMember = "Name";
                clbIngredients.ValueMember = "Id";
                clbIngredients.EndUpdate();
            }

            // Initial load
            RefreshIngredientList("");

            // Search text changed event - NOW clbIngredients EXISTS
            txtIngredientSearch.TextChanged += (s, e) =>
            {
                RefreshIngredientList(txtIngredientSearch.Text);

                // If a supplier is selected, re-apply the checked states
                if (cmbSupplierForIngredients.SelectedItem is Supplier supplier)
                {
                    var assignedIds = dbService.GetSupplierIngredients(supplier.Id);
                    for (int i = 0; i < clbIngredients.Items.Count; i++)
                    {
                        var ingredient = clbIngredients.Items[i] as Ingredient;
                        if (ingredient != null && assignedIds.Contains(ingredient.Id))
                        {
                            clbIngredients.SetItemChecked(i, true);
                        }
                    }
                }
            };

            // Load assigned ingredients when supplier changes
            cmbSupplierForIngredients.SelectedIndexChanged += (s, e) =>
            {
                if (cmbSupplierForIngredients.SelectedItem is Supplier supplier)
                {
                    // Clear all checks first
                    for (int i = 0; i < clbIngredients.Items.Count; i++)
                    {
                        clbIngredients.SetItemChecked(i, false);
                    }

                    // Load assigned ingredients from database
                    var assignedIds = dbService.GetSupplierIngredients(supplier.Id);
                    supplierIngredientsCache[supplier.Id] = assignedIds;

                    for (int i = 0; i < clbIngredients.Items.Count; i++)
                    {
                        var ingredient = clbIngredients.Items[i] as Ingredient;
                        if (ingredient != null && assignedIds.Contains(ingredient.Id))
                        {
                            clbIngredients.SetItemChecked(i, true);
                        }
                    }
                }
            };

            // ========== SAVE BUTTON - Now clbIngredients EXISTS ==========
            Button btnSaveAssignments = new Button
            {
                Text = "Save Assignments",
                Location = new Point(15, 65),
                Size = new Size(160, 40),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            btnSaveAssignments.Click += (s, e) =>
            {
                if (cmbSupplierForIngredients.SelectedItem is Supplier supplier)
                {
                    var selectedIngredients = new List<int>();
                    for (int i = 0; i < clbIngredients.CheckedItems.Count; i++)
                    {
                        var ingredient = clbIngredients.CheckedItems[i] as Ingredient;
                        if (ingredient != null)
                        {
                            selectedIngredients.Add(ingredient.Id);
                        }
                    }

                    dbService.SaveSupplierIngredients(supplier.Id, selectedIngredients);
                    supplierIngredientsCache[supplier.Id] = selectedIngredients;

                    // Refresh the create purchase order tab
                    RefreshCreatePurchaseOrderIngredients();

                    // Refresh the supplier combo in create purchase order tab
                    RefreshSupplierComboInCreateOrder();

                    MessageBox.Show($"Ingredients assigned to {supplier.Name} saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            // Add controls in order
            pnlIngredients.Controls.AddRange(new Control[] {
        pnlSearch,
        btnSaveAssignments,
        clbIngredients
    });

            pnlMain.Controls.Add(pnlSupplierSelect);
            pnlMain.Controls.Add(pnlIngredients);

            return pnlMain;
        }

        private Panel CreatePurchaseOrderPanel()
        {
            Panel pnlMain = new Panel
            {
                Location = new Point(15, 15),
                Size = new Size(1350, 750),
                BackColor = DeepSeekBg,
                AutoScroll = true
            };

            // Supplier Selection Section
            Panel pnlSupplierInfo = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1350, 100),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None
            };
            pnlSupplierInfo.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSupplierInfo.Width - 1, pnlSupplierInfo.Height - 1);
                }
            };

            Label lblSelectSupplier = new Label
            {
                Text = "Select Supplier:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 20),
                AutoSize = true
            };

            cmbSupplierSelect = new ComboBox
            {
                Location = new Point(150, 17),
                Size = new Size(250, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat
            };

            // Safely load suppliers
            try
            {
                var suppliers = dbService.GetAllSuppliers().Where(s => s.IsActive).ToList();
                foreach (var supplier in suppliers)
                {
                    cmbSupplierSelect.Items.Add(supplier);
                }
                cmbSupplierSelect.DisplayMember = "Name";
                cmbSupplierSelect.ValueMember = "Id";
                if (cmbSupplierSelect.Items.Count > 0)
                    cmbSupplierSelect.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading suppliers: {ex.Message}");
            }

            Label lblSupplierContact = new Label
            {
                Text = "Contact:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(450, 20),
                AutoSize = true
            };

            Label lblContactValue = new Label
            {
                Text = "-",
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekAccent,
                Location = new Point(530, 20),
                AutoSize = true,
                Name = "lblContactValue"
            };

            Label lblCategoryLabel = new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(450, 55),
                AutoSize = true
            };

            Label lblCategoryValue = new Label
            {
                Text = "-",
                Font = new Font("Segoe UI", 11),
                ForeColor = DeepSeekAccent,
                Location = new Point(530, 55),
                AutoSize = true,
                Name = "lblCategoryValue"
            };

            cmbSupplierSelect.SelectedIndexChanged += (s, e) =>
            {
                if (cmbSupplierSelect.SelectedItem is Supplier supplier)
                {
                    lblContactValue.Text = supplier.ContactNumber ?? "-";
                    lblCategoryValue.Text = supplier.ItemCategory ?? "-";
                    LoadIngredientsForOrder();
                }
            };

            pnlSupplierInfo.Controls.AddRange(new Control[] {
        lblSelectSupplier, cmbSupplierSelect, lblSupplierContact, lblContactValue,
        lblCategoryLabel, lblCategoryValue
    });

            // Split Container for Ingredients and Cart
            SplitContainer splitContainer = new SplitContainer
            {
                Location = new Point(0, 110),
                Size = new Size(1350, 600),
                SplitterDistance = 650,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DeepSeekBorder,
                SplitterWidth = 2
            };

            // LEFT PANEL - Available Ingredients
            Panel pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekBg,
                Padding = new Padding(15)
            };

            Label lblIngredients = new Label
            {
                Text = "Available Ingredients (from selected supplier)",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };

            dgvIngredientsForOrder = CreateDataGridView(new Point(15, 50), new Size(610, 420));
            dgvIngredientsForOrder.Columns.Clear();
            dgvIngredientsForOrder.Columns.Add("Id", "ID");
            dgvIngredientsForOrder.Columns.Add("Name", "Ingredient Name");
            dgvIngredientsForOrder.Columns.Add("Unit", "Unit");
            dgvIngredientsForOrder.Columns.Add("BoxInfo", "Box Info");
            dgvIngredientsForOrder.Columns.Add("CurrentStock", "Current Stock");
            dgvIngredientsForOrder.Columns.Add("MinStock", "Min Stock");
            dgvIngredientsForOrder.Columns.Add("SuggestedQty", "Suggested Qty");

            dgvIngredientsForOrder.Columns["Id"].Width = 40;
            dgvIngredientsForOrder.Columns["Name"].Width = 180;
            dgvIngredientsForOrder.Columns["Unit"].Width = 60;
            dgvIngredientsForOrder.Columns["BoxInfo"].Width = 150;
            dgvIngredientsForOrder.Columns["CurrentStock"].Width = 100;
            dgvIngredientsForOrder.Columns["MinStock"].Width = 80;
            dgvIngredientsForOrder.Columns["SuggestedQty"].Width = 100;

           

            btnAddToOrder = new Button
            {
                Text = "Add to Order",
                Location = new Point(280, 487),
                Size = new Size(130, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnAddToOrder.Click += BtnAddToOrder_Click!;

            pnlLeft.Controls.AddRange(new Control[] {
        lblIngredients, dgvIngredientsForOrder,btnAddToOrder
    });

            // RIGHT PANEL - Order Cart
            Panel pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekBg,
                Padding = new Padding(15)
            };

            Label lblCart = new Label
            {
                Text = "Order Items",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true
            };

            dgvOrderCart = CreateDataGridView(new Point(15, 50), new Size(650, 440));
            dgvOrderCart.Columns.Clear();
            dgvOrderCart.Columns.Add("IngredientId", "ID");
            dgvOrderCart.Columns.Add("Name", "Ingredient Name");
            dgvOrderCart.Columns.Add("Quantity", "Quantity");
            dgvOrderCart.Columns.Add("Unit", "Unit");
            dgvOrderCart.Columns.Add("CurrentStock", "Current Stock");
            dgvOrderCart.Columns.Add("MinStock", "Min Stock");

            dgvOrderCart.Columns["IngredientId"].Visible = false;
            dgvOrderCart.Columns["Name"].Width = 180;
            dgvOrderCart.Columns["Quantity"].Width = 120;
            dgvOrderCart.Columns["Unit"].Width = 60;
            dgvOrderCart.Columns["CurrentStock"].Width = 100;
            dgvOrderCart.Columns["MinStock"].Width = 80;

            Button btnRemoveFromCart = new Button
            {
                Text = "Remove Selected",
                Location = new Point(15, 505),
                Size = new Size(130, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnRemoveFromCart.Click += BtnRemoveFromCart_Click!;

            Label lblTotalLabel = new Label
            {
                Text = "Total Units:",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(450, 510),
                AutoSize = true
            };

            lblOrderTotal = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekSuccess,
                Location = new Point(560, 505),
                AutoSize = true
            };

            btnSubmitOrder = new Button
            {
                Text = "Create PO",
                Location = new Point(350, 560),
                Size = new Size(160, 45),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnSubmitOrder.Click += BtnSubmitOrder_Click!;

            btnCancelOrder = new Button
            {
                Text = "Clear Cart",
                Location = new Point(530, 560),
                Size = new Size(120, 45),
                BackColor = DeepSeekWarning,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancelOrder.Click += (s, e) => { orderCart.Clear(); RefreshOrderCart(); };

            pnlRight.Controls.AddRange(new Control[] {
        lblCart, dgvOrderCart, btnRemoveFromCart,
        lblTotalLabel, lblOrderTotal,
        btnSubmitOrder, btnCancelOrder
    });

            splitContainer.Panel1.Controls.Add(pnlLeft);
            splitContainer.Panel2.Controls.Add(pnlRight);

            pnlMain.Controls.Add(pnlSupplierInfo);
            pnlMain.Controls.Add(splitContainer);

            return pnlMain;
        }

        private Panel CreatePurchaseOrdersListPanel()
        {
            Panel pnlMain = new Panel
            {
                Location = new Point(15, 15),
                Size = new Size(1350, 700),
                BackColor = DeepSeekBg
            };

            Label lblOrdersTitle = new Label
            {
                Text = "Purchase Orders",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(0, 0),
                AutoSize = true
            };

            // Search and Filter Panel
            Panel pnlFilter = new Panel
            {
                Location = new Point(0, 40),
                Size = new Size(1350, 50)
            };

            TextBox txtSearchPO = new TextBox
            {
                Location = new Point(0, 8),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search by PO # or Supplier...",
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchPO.TextChanged += (s, e) => FilterPurchaseOrders(txtSearchPO.Text);

            ComboBox cmbStatusFilter = new ComboBox
            {
                Location = new Point(320, 8),
                Size = new Size(150, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Pending", "Received", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterPurchaseOrdersByStatus(cmbStatusFilter.SelectedItem?.ToString() ?? "All", txtSearchPO.Text);

            pnlFilter.Controls.AddRange(new Control[] { txtSearchPO, cmbStatusFilter });

            // Purchase Orders Grid
            dgvPurchaseOrders = CreateDataGridView(new Point(0, 100), new Size(1350, 550));
            dgvPurchaseOrders.Columns.Add("Id", "PO #");
            dgvPurchaseOrders.Columns.Add("Supplier", "Supplier");
            dgvPurchaseOrders.Columns.Add("Date", "Date");
            dgvPurchaseOrders.Columns.Add("Items", "Items");
            dgvPurchaseOrders.Columns.Add("Total", "Total");
            dgvPurchaseOrders.Columns.Add("Status", "Status");

            dgvPurchaseOrders.Columns["Id"].Width = 120;
            dgvPurchaseOrders.Columns["Supplier"].Width = 200;
            dgvPurchaseOrders.Columns["Date"].Width = 120;
            dgvPurchaseOrders.Columns["Items"].Width = 100;
            dgvPurchaseOrders.Columns["Total"].Width = 120;
            dgvPurchaseOrders.Columns["Status"].Width = 100;

            // In the Purchase Orders Grid, add another button column
            DataGridViewButtonColumn btnPartialReceive = new DataGridViewButtonColumn
            {
                Name = "PartialReceiveBtn",
                HeaderText = "",
                Text = "Receive",
                UseColumnTextForButtonValue = true,
                Width = 70
            };
            dgvPurchaseOrders.Columns.Add(btnPartialReceive);

            DataGridViewButtonColumn btnView = new DataGridViewButtonColumn
            {
                Name = "ViewBtn",
                HeaderText = "",
                Text = "View",
                UseColumnTextForButtonValue = true,
                Width = 60
            };
            dgvPurchaseOrders.Columns.Add(btnView);

            dgvPurchaseOrders.CellClick += DgvPurchaseOrders_CellClick!;

            pnlMain.Controls.AddRange(new Control[] { lblOrdersTitle, pnlFilter, dgvPurchaseOrders });

            return pnlMain;
        }

        private void LoadSupplierIngredientsCache()
        {
            supplierIngredientsCache.Clear();
            try
            {
                var allAssignments = dbService.GetAllSupplierIngredients();
                foreach (var assignment in allAssignments)
                {
                    supplierIngredientsCache[assignment.Key] = assignment.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading supplier ingredients cache: {ex.Message}");
                var suppliers = dbService.GetAllSuppliers();
                foreach (var supplier in suppliers)
                {
                    supplierIngredientsCache[supplier.Id] = new List<int>();
                }
            }
        }

        private void LoadSupplierCombo()
        {
            if (cmbSupplierSelect == null) return;

            // Store current selection if any
            var currentSupplier = cmbSupplierSelect.SelectedItem as Supplier;
            int? currentSupplierId = currentSupplier?.Id;

            cmbSupplierSelect.Items.Clear();
            try
            {
                var suppliers = dbService.GetAllSuppliers().Where(s => s.IsActive);
                foreach (var supplier in suppliers)
                {
                    cmbSupplierSelect.Items.Add(supplier);
                }
                cmbSupplierSelect.DisplayMember = "Name";
                cmbSupplierSelect.ValueMember = "Id";

                // Restore previous selection if possible
                if (currentSupplierId.HasValue && suppliers.Any(s => s.Id == currentSupplierId.Value))
                {
                    cmbSupplierSelect.SelectedItem = suppliers.First(s => s.Id == currentSupplierId.Value);
                }
                else if (cmbSupplierSelect.Items.Count > 0)
                {
                    cmbSupplierSelect.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading suppliers: {ex.Message}");
            }
        }

        private void LoadIngredientsForOrder()
        {
            if (dgvIngredientsForOrder == null) return;
            dgvIngredientsForOrder.Rows.Clear();

            try
            {
                var supplier = cmbSupplierSelect?.SelectedItem as Supplier;
                if (supplier == null)
                {
                    dgvIngredientsForOrder.Rows.Add("", "Please select a supplier first", "", "", "", "");
                    return;
                }

                List<int> supplierIngredientIds;
                if (supplierIngredientsCache.ContainsKey(supplier.Id))
                {
                    supplierIngredientIds = supplierIngredientsCache[supplier.Id];
                }
                else
                {
                    supplierIngredientIds = dbService.GetSupplierIngredients(supplier.Id);
                    supplierIngredientsCache[supplier.Id] = supplierIngredientIds;
                }

                var allIngredients = dbService.GetAllIngredients().Where(i => i.IsActive).ToList();
                var inventory = dbService.GetAllInventory();

                var filteredIngredients = allIngredients.Where(i => supplierIngredientIds.Contains(i.Id)).ToList();

                // Clear and add columns
                dgvIngredientsForOrder.Columns.Clear();
                dgvIngredientsForOrder.Columns.Add("Id", "ID");
                dgvIngredientsForOrder.Columns.Add("Name", "Ingredient Name");
                dgvIngredientsForOrder.Columns.Add("Unit", "Unit");
                dgvIngredientsForOrder.Columns.Add("CurrentStock", "Current Stock");
                dgvIngredientsForOrder.Columns.Add("MinStock", "Min Stock");
                dgvIngredientsForOrder.Columns.Add("Status", "Status");
                dgvIngredientsForOrder.Columns.Add("SuggestedQty", "Suggested Qty");

                dgvIngredientsForOrder.Columns["Id"].Width = 40;
                dgvIngredientsForOrder.Columns["Name"].Width = 180;
                dgvIngredientsForOrder.Columns["Unit"].Width = 50;
                dgvIngredientsForOrder.Columns["CurrentStock"].Width = 100;
                dgvIngredientsForOrder.Columns["MinStock"].Width = 80;
                dgvIngredientsForOrder.Columns["Status"].Width = 80;
                dgvIngredientsForOrder.Columns["SuggestedQty"].Width = 100;

                foreach (var ingredient in filteredIngredients)
                {
                    var stock = inventory.FirstOrDefault(i => i.IngredientId == ingredient.Id);
                    decimal currentStock = stock?.Quantity ?? 0;

                    // Low Stock = 50% of Minimum Stock
                    decimal lowStockThreshold = ingredient.MinimumStock * 0.5m;

                    // Suggested quantity = Minimum Stock - Current Stock (only if current stock < minimum)
                    decimal suggestedQty = currentStock < ingredient.MinimumStock ? ingredient.MinimumStock - currentStock : 0;

                    // Status: Low Stock if current stock <= 50% of minimum, otherwise In Stock
                    string status = currentStock <= lowStockThreshold ? "Low Stock" : "In Stock";

                    int rowIndex = dgvIngredientsForOrder.Rows.Add(
                        ingredient.Id,
                        ingredient.Name,
                        ingredient.Unit ?? "kg",
                        $"{currentStock:F1} {ingredient.Unit ?? "kg"}",
                        ingredient.MinimumStock,
                        status,
                        suggestedQty > 0 ? $"{suggestedQty:F0}" : "0"
                    );

                    // Color code the status
                    if (status == "Low Stock")
                    {
                        dgvIngredientsForOrder.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                        dgvIngredientsForOrder.Rows[rowIndex].Cells["Status"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    }
                    else
                    {
                        dgvIngredientsForOrder.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                    }
                }

                if (filteredIngredients.Count == 0)
                {
                    dgvIngredientsForOrder.Rows.Add("", "No ingredients assigned to this supplier", "", "", "", "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ingredients: {ex.Message}");
                dgvIngredientsForOrder.Rows.Add("", $"Error: {ex.Message}", "", "", "", "");
            }
        }

        // Add this helper method to AdminDashboard.cs
        private decimal ConvertToDisplayQuantity(decimal quantity, string fromUnit, string toUnit)
        {
            if (fromUnit.ToLower() == toUnit.ToLower())
                return quantity;

            switch (fromUnit.ToLower())
            {
                case "kg":
                    if (toUnit.ToLower() == "g")
                        return quantity * 1000m;
                    return quantity;
                case "g":
                    if (toUnit.ToLower() == "kg")
                        return quantity / 1000m;
                    return quantity;
                case "l":
                    if (toUnit.ToLower() == "ml")
                        return quantity * 1000m;
                    return quantity;
                case "ml":
                    if (toUnit.ToLower() == "l")
                        return quantity / 1000m;
                    return quantity;
                default:
                    return quantity;
            }
        }

        private void BtnAddToOrder_Click(object sender, EventArgs e)
        {
            if (dgvIngredientsForOrder?.CurrentRow == null)
            {
                MessageBox.Show("Please select an ingredient first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int ingredientId = Convert.ToInt32(dgvIngredientsForOrder.CurrentRow.Cells["Id"].Value);
            string ingredientName = dgvIngredientsForOrder.CurrentRow.Cells["Name"].Value?.ToString() ?? "";
            string unit = dgvIngredientsForOrder.CurrentRow.Cells["Unit"].Value?.ToString() ?? "kg";

            // Get ingredient details
            var ingredient = dbService.GetIngredientById(ingredientId);
            if (ingredient == null) return;

            // Get current stock
            var inventory = dbService.GetInventoryForIngredient(ingredientId);
            decimal currentStock = inventory?.Quantity ?? 0;
            int minStock = ingredient.MinimumStock;

            // Suggested quantity = Minimum Stock - Current Stock (only if current stock < minimum)
            decimal suggestedQty = currentStock < minStock ? minStock - currentStock : 0;

            // Show dialog with suggested quantity and checkbox for custom quantity
            using (Form qtyDialog = new Form())
            {
                qtyDialog.Text = $"Order Quantity for {ingredientName}";
                qtyDialog.Size = new Size(500, 300);
                qtyDialog.StartPosition = FormStartPosition.CenterParent;
                qtyDialog.BackColor = DeepSeekSurface;
                qtyDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                qtyDialog.MaximizeBox = false;
                qtyDialog.MinimizeBox = false;

                // Suggested quantity label
                Label lblSuggested = new Label
                {
                    Text = suggestedQty > 0 ? $"Suggested Qty: {suggestedQty:F0} {unit} (to reach minimum stock)" : $"Current stock ({currentStock:F1} {unit}) is above minimum ({minStock} {unit})",
                    Location = new Point(20, 20),
                    Size = new Size(440, 35),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = suggestedQty > 0 ? DeepSeekAccent : DeepSeekSuccess,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Current stock info
                decimal lowStockThreshold = minStock * 0.5m;
                string stockStatus = currentStock <= lowStockThreshold ? "⚠️ LOW STOCK" : "✓ In Stock";
                Color statusColor = currentStock <= lowStockThreshold ? DeepSeekDanger : DeepSeekSuccess;

                Label lblStockInfo = new Label
                {
                    Text = $"Current Stock: {currentStock:F1} {unit}  |  Minimum: {minStock} {unit}  |  Status: {stockStatus}",
                    Location = new Point(20, 60),
                    Size = new Size(440, 30),
                    Font = new Font("Segoe UI", 11),
                    ForeColor = statusColor
                };

                // Custom quantity checkbox
                CheckBox chkCustomQty = new CheckBox
                {
                    Text = "Override suggested quantity",
                    Location = new Point(20, 105),
                    AutoSize = true,
                    ForeColor = DeepSeekText,
                    Checked = false,
                    Font = new Font("Segoe UI", 11)
                };

                // Quantity input
                Label lblQtyLabel = new Label
                {
                    Text = "Quantity:",
                    Location = new Point(20, 145),
                    AutoSize = true,
                    ForeColor = DeepSeekTextMuted,
                    Enabled = false,
                    Font = new Font("Segoe UI", 11)
                };

                NumericUpDown nudQty = new NumericUpDown
                {
                    Location = new Point(120, 142),
                    Size = new Size(140, 35),
                    Minimum = 0,
                    Maximum = 100000,
                    DecimalPlaces = 0,
                    Value = suggestedQty,
                    Enabled = false,
                    BackColor = DeepSeekBg,
                    ForeColor = DeepSeekText,
                    Font = new Font("Segoe UI", 12)
                };

                // Unit label next to quantity
                Label lblUnitLabel = new Label
                {
                    Text = unit,
                    Location = new Point(270, 147),
                    AutoSize = true,
                    ForeColor = DeepSeekTextMuted,
                    Enabled = false,
                    Font = new Font("Segoe UI", 12)
                };

                // Enable/disable controls based on checkbox
                chkCustomQty.CheckedChanged += (s, ev) =>
                {
                    bool isChecked = chkCustomQty.Checked;
                    nudQty.Enabled = isChecked;
                    lblQtyLabel.Enabled = isChecked;
                    lblUnitLabel.Enabled = isChecked;
                    if (!isChecked)
                    {
                        nudQty.Value = suggestedQty;
                    }
                };

                // Buttons Panel
                Panel pnlButtons = new Panel
                {
                    Location = new Point(20, 200),
                    Size = new Size(440, 50),
                    BackColor = Color.Transparent
                };

                Button btnAdd = new Button
                {
                    Text = "Add to Order",
                    Location = new Point(0, 8),
                    Size = new Size(160, 40),
                    BackColor = DeepSeekSuccess,
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };

                Button btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(175, 8),
                    Size = new Size(120, 40),
                    BackColor = DeepSeekDanger,
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    DialogResult = DialogResult.Cancel
                };

                pnlButtons.Controls.AddRange(new Control[] { btnAdd, btnCancel });

                qtyDialog.Controls.AddRange(new Control[] {
            lblSuggested, lblStockInfo, chkCustomQty,
            lblQtyLabel, nudQty, lblUnitLabel,
            pnlButtons
        });

                if (qtyDialog.ShowDialog() == DialogResult.OK)
                {
                    decimal orderQty = nudQty.Value;

                    // If custom checkbox is not checked, use suggested quantity
                    if (!chkCustomQty.Checked)
                    {
                        orderQty = suggestedQty;
                    }

                    if (orderQty <= 0)
                    {
                        MessageBox.Show("Please enter a quantity greater than 0.", "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if item already in cart
                    var existingItem = orderCart.FirstOrDefault(item => item.IngredientId == ingredientId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += orderQty;
                        existingItem.SuggestedQuantity = suggestedQty;
                        existingItem.CurrentStock = currentStock;
                        existingItem.MinimumStock = minStock;
                        existingItem.IsCustomQuantity = chkCustomQty.Checked;
                    }
                    else
                    {
                        orderCart.Add(new PurchaseOrderCartItem
                        {
                            IngredientId = ingredientId,
                            IngredientName = ingredientName,
                            Quantity = orderQty,
                            Unit = unit,
                            SuggestedQuantity = suggestedQty,
                            CurrentStock = currentStock,
                            MinimumStock = minStock,
                            IsCustomQuantity = chkCustomQty.Checked
                        });
                    }

                    RefreshOrderCart();
                }
            }
        }

        private void BtnRemoveFromCart_Click(object sender, EventArgs e)
        {
            if (dgvOrderCart?.CurrentRow == null) return;

            int index = dgvOrderCart.CurrentRow.Index;
            if (index >= 0 && index < orderCart.Count)
            {
                orderCart.RemoveAt(index);
                RefreshOrderCart();
            }
        }

        private void RefreshOrderCart()
        {
            if (dgvOrderCart == null) return;

            dgvOrderCart.Rows.Clear();
            decimal totalBoxes = 0;

            foreach (var item in orderCart)
            {
                string qtyInfo = item.IsCustomQuantity ? $"{item.Quantity:F1} (Custom)" : $"{item.Quantity:F1} (Suggested: {item.SuggestedQuantity:F1})";

                dgvOrderCart.Rows.Add(
                    item.IngredientId,
                    item.IngredientName,
                    $"{item.Quantity:F1}",
                    qtyInfo,
                    item.Unit
                );
                totalBoxes += item.Quantity;
            }

            lblOrderTotal!.Text = $"{totalBoxes:F1}";
        }

        private void BtnSubmitOrder_Click(object sender, EventArgs e)
        {
            if (cmbSupplierSelect?.SelectedItem == null)
            {
                MessageBox.Show("Please select a supplier.", "No Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (orderCart.Count == 0)
            {
                MessageBox.Show("No items in the order.", "Empty Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var supplier = cmbSupplierSelect.SelectedItem as Supplier;
            if (supplier == null) return;

            try
            {
                var order = new PurchaseOrder
                {
                    SupplierId = supplier.Id,
                    CreatedBy = userId,
                    Notes = $"Purchase order from {supplier.Name}",
                    Status = "Pending"
                };

                // Convert cart items to the format expected by CreatePurchaseOrder
                // Expected format: (int ingredientId, string ingredientName, decimal boxes, decimal quantity, string unit, string boxDisplayName, decimal boxQuantityPerUnit, decimal unitPrice)
                var items = orderCart.Select(item => (
                    ingredientId: item.IngredientId,
                    ingredientName: item.IngredientName,
                    boxes: item.Quantity,
                    quantity: item.Quantity,
                    unit: item.Unit,
                    boxDisplayName: "",      // No box system - empty string
                    boxQuantityPerUnit: 1m,  // No box system - default 1
                    unitPrice: 0m
                )).ToList();

                var savedOrder = dbService.CreatePurchaseOrder(order, items);

                MessageBox.Show($"Purchase Order #{savedOrder.OrderNumber} created successfully!\nTotal Quantity: {orderCart.Sum(i => i.Quantity):F1} {orderCart.FirstOrDefault()?.Unit ?? "units"}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                orderCart.Clear();
                RefreshOrderCart();
                LoadPurchaseOrders();

                if (tcSupplier != null && tcSupplier.TabPages.Count > 3)
                {
                    tcSupplier.SelectedIndex = 3;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating purchase order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPurchaseOrders()
        {
            if (dgvPurchaseOrders == null) return;
            dgvPurchaseOrders.Rows.Clear();

            try
            {
                var orders = dbService.GetAllPurchaseOrders();
                foreach (var order in orders)
                {
                    int itemCount = order.PurchaseOrderItems?.Count ?? 0;
                    decimal totalQuantity = order.PurchaseOrderItems?.Sum(i => i.QuantityOrdered) ?? 0;

                    int rowIndex = dgvPurchaseOrders.Rows.Add(
                        order.OrderNumber,
                        order.Supplier?.Name ?? "N/A",
                        order.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                        itemCount,
                        $"{totalQuantity:F1} units",
                        order.Status
                    );

                    if (order.Status == "Pending")
                        dgvPurchaseOrders.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekWarning;
                    else if (order.Status == "Received")
                        dgvPurchaseOrders.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                    else if (order.Status == "Cancelled")
                        dgvPurchaseOrders.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading purchase orders: {ex.Message}");
            }
        }

        private void FilterPurchaseOrders(string searchText)
        {
            if (dgvPurchaseOrders == null) return;

            foreach (DataGridViewRow row in dgvPurchaseOrders.Rows)
            {
                if (row.IsNewRow) continue;
                string poNumber = row.Cells["Id"].Value?.ToString() ?? "";
                string supplier = row.Cells["Supplier"].Value?.ToString() ?? "";
                bool matches = string.IsNullOrEmpty(searchText) ||
                               poNumber.ToLower().Contains(searchText.ToLower()) ||
                               supplier.ToLower().Contains(searchText.ToLower());
                row.Visible = matches;
            }
        }

        private void FilterPurchaseOrdersByStatus(string status, string searchText)
        {
            if (dgvPurchaseOrders == null) return;

            foreach (DataGridViewRow row in dgvPurchaseOrders.Rows)
            {
                if (row.IsNewRow) continue;
                string rowStatus = row.Cells["Status"].Value?.ToString() ?? "";
                string poNumber = row.Cells["Id"].Value?.ToString() ?? "";
                string supplier = row.Cells["Supplier"].Value?.ToString() ?? "";

                bool matchesStatus = status == "All" || rowStatus == status;
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                     poNumber.ToLower().Contains(searchText.ToLower()) ||
                                     supplier.ToLower().Contains(searchText.ToLower());

                row.Visible = matchesStatus && matchesSearch;
            }
        }

        private void DgvPurchaseOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string columnName = dgvPurchaseOrders!.Columns[e.ColumnIndex].Name;
            string orderNumber = dgvPurchaseOrders.Rows[e.RowIndex].Cells["Id"].Value?.ToString() ?? "";

            var order = dbService.GetAllPurchaseOrders().FirstOrDefault(o => o.OrderNumber == orderNumber);
            if (order == null) return;

            if (columnName == "ReceiveBtn" && (order.Status == "Pending" || order.Status == "Partially Received"))
            {
                // Full receive
                DialogResult result = MessageBox.Show($"Mark Purchase Order #{orderNumber} as Fully Received?\n\nThis will add all remaining ordered items to inventory.",
                    "Confirm Full Receive", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Receive all remaining boxes
                        var receiveQuantities = new Dictionary<int, decimal>();
                        foreach (var item in order.PurchaseOrderItems)
                        {
                            decimal remainingBoxes = item.QuantityOrdered - item.QuantityReceived;
                            if (remainingBoxes > 0)
                            {
                                receiveQuantities[item.Id] = remainingBoxes;
                            }
                        }
                        dbService.ReceivePurchaseOrderPartially(order.Id, receiveQuantities);
                        MessageBox.Show($"Purchase Order #{orderNumber} has been fully received.\nInventory has been updated.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPurchaseOrders();
                        LoadIngredientsForOrder();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error receiving order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (columnName == "PartialReceiveBtn" && (order.Status == "Pending" || order.Status == "Partially Received"))
            {
                // Partial receive - show dialog to enter quantities
                ShowPartialReceiveDialog(order);
            }
            else if (columnName == "ViewBtn")
            {
                ShowOrderDetails(order);
            }
        }

        private void ShowPartialReceiveDialog(PurchaseOrder order)
        {
            Form dialog = new Form();
            dialog.Text = $"Receive - {order.OrderNumber}";
            dialog.Size = new Size(500, 400);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.BackColor = DeepSeekSurface;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;

            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = order.PurchaseOrderItems.Count + 2,
                Padding = new Padding(10),
                BackColor = DeepSeekSurface
            };

            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // Update headers - change "boxes" to "units"
            tlp.Controls.Add(new Label { Text = "Ingredient", ForeColor = DeepSeekText, Font = new Font("Segoe UI", 10, FontStyle.Bold) }, 0, 0);
            tlp.Controls.Add(new Label { Text = "Ordered (units)", ForeColor = DeepSeekText, Font = new Font("Segoe UI", 10, FontStyle.Bold) }, 1, 0);
            tlp.Controls.Add(new Label { Text = "Received", ForeColor = DeepSeekText, Font = new Font("Segoe UI", 10, FontStyle.Bold) }, 2, 0);
            tlp.Controls.Add(new Label { Text = "To Receive", ForeColor = DeepSeekText, Font = new Font("Segoe UI", 10, FontStyle.Bold) }, 3, 0);

            var receiveInputs = new Dictionary<int, NumericUpDown>();
            int row = 1;

            foreach (var item in order.PurchaseOrderItems)
            {
                decimal remaining = item.QuantityOrdered - item.QuantityReceived;

                tlp.Controls.Add(new Label { Text = item.IngredientName, ForeColor = DeepSeekText, AutoSize = true }, 0, row);
                tlp.Controls.Add(new Label { Text = $"{item.QuantityOrdered:F0} units", ForeColor = DeepSeekTextMuted, AutoSize = true }, 1, row);
                tlp.Controls.Add(new Label { Text = $"{item.QuantityReceived:F0} units", ForeColor = DeepSeekAccent, AutoSize = true }, 2, row);

                NumericUpDown nudReceive = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = remaining,
                    DecimalPlaces = 0,  // Integer values only
                    Value = remaining,
                    Increment = 1,
                    Width = 100,
                    BackColor = DeepSeekBg,
                    ForeColor = DeepSeekText
                };

                receiveInputs[item.Id] = nudReceive;
                tlp.Controls.Add(nudReceive, 3, row);

                row++;
            }

            Button btnConfirm = new Button
            {
                Text = "Confirm Receive",
                Size = new Size(120, 35),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat
            };

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(120, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat
            };

            Panel pnlButtons = new Panel { Height = 50, Dock = DockStyle.Bottom };
            pnlButtons.Controls.Add(btnConfirm);
            pnlButtons.Controls.Add(btnCancel);
            btnConfirm.Location = new Point(150, 10);
            btnCancel.Location = new Point(290, 10);

            dialog.Controls.Add(tlp);
            dialog.Controls.Add(pnlButtons);

            btnConfirm.Click += (s, e) =>
            {
                var receiveQuantities = new Dictionary<int, decimal>();
                foreach (var kvp in receiveInputs)
                {
                    if (kvp.Value.Value > 0)
                    {
                        receiveQuantities[kvp.Key] = kvp.Value.Value;
                    }
                }

                if (receiveQuantities.Count > 0)
                {
                    try
                    {
                        dbService.ReceivePurchaseOrderPartially(order.Id, receiveQuantities);
                        MessageBox.Show($"Receive completed for {order.OrderNumber}!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPurchaseOrders();
                        LoadIngredientsForOrder();
                        dialog.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No quantities entered to receive.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnCancel.Click += (s, e) => dialog.Close();

            dialog.ShowDialog();
        }

        private void ShowOrderDetails(PurchaseOrder order)
        {
            string details = $"Purchase Order: {order.OrderNumber}\n";
            details += $"Supplier: {order.Supplier?.Name ?? "N/A"}\n";
            details += $"Date: {order.OrderDate:yyyy-MM-dd HH:mm}\n";
            details += $"Status: {order.Status}\n";
            details += "─────────────────────\n";
            details += "Items:\n";

            decimal totalQuantity = 0;
            if (order.PurchaseOrderItems != null)
            {
                foreach (var item in order.PurchaseOrderItems)
                {
                    details += $"• {item.IngredientName}\n";
                    details += $"  Ordered: {item.QuantityOrdered:F1} {item.Ingredient?.Unit ?? "units"}";
                    if (item.QuantityReceived > 0 && item.QuantityReceived < item.QuantityOrdered)
                    {
                        details += $" (Received: {item.QuantityReceived:F1})";
                    }
                    details += $"\n";
                    totalQuantity += item.QuantityOrdered;
                }
            }

            details += "─────────────────────\n";
            details += $"Total Quantity: {totalQuantity:F1} units\n";

            MessageBox.Show(details, "Purchase Order Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSuppliers()
        {
            if (dgvSuppliers == null) return;

            dgvSuppliers.Rows.Clear();

            try
            {
                var suppliers = dbService.GetAllSuppliers();
                string searchText = txtSearchSupplier?.Text?.ToLower() ?? "";

                foreach (var supplier in suppliers)
                {
                    if (!string.IsNullOrEmpty(searchText) && !supplier.Name.ToLower().Contains(searchText))
                        continue;

                    int rowIndex = dgvSuppliers.Rows.Add(
                        supplier.Name,
                        supplier.ContactNumber ?? "-",
                        supplier.Email ?? "-",
                        supplier.ItemCategory ?? "-",
                        supplier.IsActive ? "Active" : "Inactive"
                    );

                    // Store the entire supplier object in the row's Tag
                    dgvSuppliers.Rows[rowIndex].Tag = supplier;

                    if (supplier.IsActive)
                    {
                        dgvSuppliers.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                    }
                    else
                    {
                        dgvSuppliers.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading suppliers: {ex.Message}");
            }
        }

        private void TxtSearchSupplier_TextChanged(object sender, EventArgs e) => LoadSuppliers();

        private void BtnAddSupplier_Click(object sender, EventArgs e)
        {
            ClearSupplierForm();
            selectedSupplierId = null;
            pnlSupplierForm!.Visible = true;
            txtSupplierName?.Focus();
        }

        private void DgvSuppliers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSuppliers?.CurrentRow == null)
            {
                selectedSupplierId = null;
                return;
            }

            // Since we removed the ID column, use the row's Tag to store the supplier
            if (dgvSuppliers.CurrentRow.Tag is Supplier supplier)
            {
                selectedSupplierId = supplier.Id;
                System.Diagnostics.Debug.WriteLine($"Selected supplier: {supplier.Name} (ID: {supplier.Id})");
            }
            else
            {
                // Fallback: Try to find the supplier by name
                string supplierName = dgvSuppliers.CurrentRow.Cells["Name"].Value?.ToString();
                if (!string.IsNullOrEmpty(supplierName))
                {
                    var foundSupplier = dbService.GetAllSuppliers().FirstOrDefault(s => s.Name == supplierName);
                    if (foundSupplier != null)
                    {
                        selectedSupplierId = foundSupplier.Id;
                        // Store in Tag for next time
                        dgvSuppliers.CurrentRow.Tag = foundSupplier;
                        System.Diagnostics.Debug.WriteLine($"Found supplier by name: {foundSupplier.Name} (ID: {foundSupplier.Id})");
                    }
                }
            }
        }

        private void BtnEditSelectedSupplier_Click(object sender, EventArgs e)
        {
            // Force selection update
            DgvSuppliers_SelectionChanged(sender, e);

            if (!selectedSupplierId.HasValue)
            {
                MessageBox.Show("Please select a supplier first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var supplier = dbService.GetSupplierById(selectedSupplierId.Value);
                if (supplier != null)
                {
                    txtSupplierName!.Text = supplier.Name;
                    txtSupplierContact!.Text = supplier.ContactNumber ?? "";
                    txtSupplierEmail!.Text = supplier.Email ?? "";
                    txtSupplierAddress!.Text = supplier.Address ?? "";
                    if (supplier.ItemCategory != null)
                        cmbItemCategory!.SelectedItem = supplier.ItemCategory;

                    pnlSupplierForm!.Visible = true;
                    pnlSupplierForm.BringToFront();
                }
                else
                {
                    MessageBox.Show("Supplier not found in database.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    selectedSupplierId = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnToggleSupplierStatus_Click(object sender, EventArgs e)
        {
            // Force selection update
            DgvSuppliers_SelectionChanged(sender, e);

            if (!selectedSupplierId.HasValue)
            {
                MessageBox.Show("Please select a supplier first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                dbService.ToggleSupplierStatus(selectedSupplierId.Value);
                LoadSuppliers();
                LoadSupplierIngredientsCache();

                // Refresh the Assign Ingredients tab if visible
                if (tcSupplier != null && tcSupplier.SelectedTab != null && tcSupplier.SelectedTab.Text == "Assign Ingredients")
                {
                    var assignTab = tcSupplier.TabPages["tpAssignIngredients"];
                    if (assignTab != null)
                    {
                        assignTab.Controls.Clear();
                        Panel pnlAssign = CreateAssignIngredientsPanel();
                        assignTab.Controls.Add(pnlAssign);
                    }
                }

                // Clear selection
                selectedSupplierId = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling status: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveSupplier_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSupplierName?.Text))
            {
                MessageBox.Show("Supplier name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (selectedSupplierId.HasValue)
                {
                    var supplier = dbService.GetSupplierById(selectedSupplierId.Value);
                    if (supplier != null)
                    {
                        supplier.Name = txtSupplierName.Text;
                        supplier.ContactNumber = txtSupplierContact?.Text;
                        supplier.Email = txtSupplierEmail?.Text;
                        supplier.Address = txtSupplierAddress?.Text;
                        supplier.ItemCategory = cmbItemCategory?.SelectedItem?.ToString();
                        dbService.UpdateSupplier(supplier);
                        MessageBox.Show("Supplier updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    var supplier = new Supplier
                    {
                        Name = txtSupplierName.Text,
                        ContactNumber = txtSupplierContact?.Text,
                        Email = txtSupplierEmail?.Text,
                        Address = txtSupplierAddress?.Text,
                        ItemCategory = cmbItemCategory?.SelectedItem?.ToString(),
                        IsActive = true
                    };
                    dbService.AddSupplier(supplier);
                    MessageBox.Show("Supplier added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                pnlSupplierForm!.Visible = false;
                ClearSupplierForm();
                LoadSuppliers();
                LoadSupplierCombo();
                LoadSupplierIngredientsCache();

                // Refresh the Assign Ingredients tab
                RefreshAssignIngredientsTab();

                // Refresh the Create Purchase Order tab
                RefreshCreatePurchaseOrderIngredients();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving supplier: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshAssignIngredientsTab()
        {
            if (tcSupplier == null) return;

            // Find the Assign Ingredients tab
            TabPage? assignTab = null;
            foreach (TabPage tab in tcSupplier.TabPages)
            {
                if (tab.Text == "Assign Ingredients")
                {
                    assignTab = tab;
                    break;
                }
            }

            if (assignTab != null)
            {
                // Clear and recreate the Assign Ingredients panel
                assignTab.Controls.Clear();
                Panel pnlAssign = CreateAssignIngredientsPanel();
                assignTab.Controls.Add(pnlAssign);
            }

            // Also refresh the Create Purchase Order tab ingredients
            RefreshCreatePurchaseOrderIngredients();
        }

        private void ClearSupplierForm()
        {
            if (txtSupplierName != null) txtSupplierName.Text = "";
            if (txtSupplierContact != null) txtSupplierContact.Text = "";
            if (txtSupplierEmail != null) txtSupplierEmail.Text = "";
            if (txtSupplierAddress != null) txtSupplierAddress.Text = "";
            if (cmbItemCategory != null && cmbItemCategory.Items.Count > 0) cmbItemCategory.SelectedIndex = -1;
            selectedSupplierId = null;
        }

        #endregion

        #region Navigation

        private void BtnDashboard_Click(object sender, EventArgs e) => ShowDashboard();
        private void BtnProduct_Click(object sender, EventArgs e) => ShowProductManagement();
        private void BtnIngredients_Click(object sender, EventArgs e) => ShowIngredientLibrary();
        private void BtnStaff_Click(object sender, EventArgs e) => ShowStaffManagement();
        private void BtnSupplier_Click(object sender, EventArgs e) => ShowSupplierManagement();

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            dbService.Dispose();
            base.OnFormClosing(e);
        }

        private class PurchaseOrderCartItem
        {
            public int IngredientId { get; set; }
            public string IngredientName { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public string Unit { get; set; } = string.Empty;
            public decimal SuggestedQuantity { get; set; } = 0;
            public decimal CurrentStock { get; set; } = 0;
            public int MinimumStock { get; set; } = 10;
            public bool IsCustomQuantity { get; set; } = false;
        }
    }
}