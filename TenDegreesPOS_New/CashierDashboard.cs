using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TenDegreesPOS_New.Models;
using TenDegreesPOS_New.Services;

namespace TenDegreesPOS_New
{
    public partial class CashierDashboard : BaseDashboard
    {
        // Sidebar buttons
        private Button? btnHome;
        private Button? btnPromos;
        private Button? btnHistory;
        private Button? btnInventory;
        private Button? btnSettings;

        private TcpServerService _tcpServer = new TcpServerService();

        // Quantity adjustment controls
        private NumericUpDown? nudEditQuantity;

        // Discount controls
        private Panel? pnlDiscount;
        private CheckBox? chkApplyDiscount;
        private NumericUpDown? nudDiscountPercent;
        private Label? lblDiscountAmount;
        private Button? btnApplyDiscount;
        private decimal currentDiscountAmount = 0;

        // Main content panels
        private Panel? pnlHome;
        private Panel? pnlPromos;
        private Panel? pnlHistory;
        private Panel? pnlInventory;
        private Panel? pnlSettings;

        // Home/Order panel controls
        private FlowLayoutPanel? flpCategories;
        private FlowLayoutPanel? flpProducts;
        private DataGridView? dgvCart;
        private Label? lblSubTotal;
        private Label? lblTax;
        private Label? lblDiscount;
        private Label? lblTotal;
        private TextBox? txtSearch;
        private ComboBox? cmbCategory;
        private Button? btnPrintBill;
        private Button? btnClearCart;
        private NumericUpDown? nudQuantity;
        private ComboBox? cmbSize;
        private Label? lblStockWarning;
        private Product? currentSelectedProduct;

        // Cart items
        private List<CartItem> cartItems = new List<CartItem>();

        // Stock cache
        private Dictionary<int, (Ingredient ingredient, decimal available)> ingredientStockCache = new Dictionary<int, (Ingredient, decimal)>();
        private Dictionary<int, Product> productCache = new Dictionary<int, Product>();

        // Promos panel controls
        private DataGridView? dgvPromos;
        private Button? btnApplyPromo;
        private TextBox? txtPromoCode;

        // History panel controls
        private DataGridView? dgvTransactions;
        private DateTimePicker? dtpFrom;
        private DateTimePicker? dtpTo;
        private Button? btnFilter;

        // Inventory panel controls
        private DataGridView? dgvInventory;
        private TextBox? txtSearchInventory;

        // Settings panel controls
        private Button? btnLogout;

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

        public CashierDashboard(string username)
        {
            GlobalFontSettings.FontResolver = new CustomFontResolver();

            this.Size = new Size(1800, 1000);
            InitializeComponent();
            SetUserInfo(username, "Cashier");
            SetupCashierControls();
            _tcpServer.Start(8888);
            LoadStockCache();
            LoadProductCache();
            ShowHome();
        }

        private void InitializeComponent()
        {
            this.Text = "Cashier Dashboard";
        }

        private void SetupCashierControls()
        {
            btnHome = CreateSidebarButton("🏠 Home", 10, BtnHome_Click!);
            btnPromos = CreateSidebarButton("🎉 Promos", 60, BtnPromos_Click!);
            btnHistory = CreateSidebarButton("📋 History", 110, BtnHistory_Click!);
            btnInventory = CreateSidebarButton("📦 Inventory", 160, BtnInventory_Click!);
            btnSettings = CreateSidebarButton("⚙ Settings", 210, BtnSettings_Click!);

            if (pnlSidebar != null)
            {
                pnlSidebar.Controls.AddRange(new Control[] {
                    btnHome, btnPromos, btnHistory, btnInventory, btnSettings
                });
            }
        }

        private void LoadStockCache()
        {
            ingredientStockCache.Clear();
            var inventory = dbService.GetAllInventory();
            var allIngredients = dbService.GetAllIngredients();

            foreach (var ingredient in allIngredients)
            {
                var stock = inventory.FirstOrDefault(i => i.IngredientId == ingredient.Id);
                decimal currentStock = stock?.Quantity ?? 0;
                ingredientStockCache[ingredient.Id] = (ingredient, currentStock);
            }
        }

        private void LoadProductCache()
        {
            productCache.Clear();
            var products = dbService.GetAllProducts();
            foreach (var product in products)
            {
                if (product != null)
                    productCache[product.Id] = product;
            }
        }

        // ============================================================
        //  QUANTITY / AVAILABILITY HELPERS
        // ============================================================

        private decimal GetRequiredQtyPerUnit(ProductIngredient pi, bool hasSizes, string? size)
        {
            if (!hasSizes || string.IsNullOrWhiteSpace(size))
                return pi.Quantity;

            switch (size.Trim().ToLowerInvariant())
            {
                case "small":
                    return pi.QuantitySmall ?? pi.Quantity;
                case "medium":
                    return pi.QuantityMedium ?? pi.Quantity;
                case "large":
                    return pi.QuantityLarge ?? pi.Quantity;
                default:
                    return pi.Quantity;
            }
        }

        private Dictionary<int, decimal> GetCartIngredientUsage()
        {
            var usage = new Dictionary<int, decimal>();

            foreach (var cartItem in cartItems)
            {
                var product = dbService.GetProductById(cartItem.ProductId);
                if (product?.ProductIngredients == null) continue;

                foreach (var pi in product.ProductIngredients)
                {
                    decimal perUnit = GetRequiredQtyPerUnit(pi, product.HasSizes, cartItem.Size);
                    decimal total = perUnit * cartItem.Quantity;

                    if (usage.ContainsKey(pi.IngredientId))
                        usage[pi.IngredientId] += total;
                    else
                        usage[pi.IngredientId] = total;
                }
            }

            return usage;
        }

        private bool CheckProductAvailability(Product product, int quantity, string? selectedSize, out string? insufficientMessage)
        {
            insufficientMessage = null;

            var fullProduct = dbService.GetProductById(product.Id);

            if (fullProduct?.ProductIngredients == null || !fullProduct.ProductIngredients.Any())
            {
                insufficientMessage = $"Product '{product.Name}' has no ingredients configured.\n\nPlease ask the administrator to set up ingredients for this product.";
                return false;
            }

            var freshInventory = dbService.GetAllInventory()
                .ToDictionary(i => i.IngredientId ?? 0, i => i.Quantity);

            var reserved = GetCartIngredientUsage();

            var insufficientIngredients = new List<string>();

            foreach (var productIngredient in fullProduct.ProductIngredients)
            {
                if (productIngredient.IngredientId <= 0) continue;

                var ingredientData = dbService.GetIngredientById(productIngredient.IngredientId);
                if (ingredientData == null) continue;

                decimal requiredPerUnit = GetRequiredQtyPerUnit(productIngredient, product.HasSizes, selectedSize);

                if (requiredPerUnit <= 0)
                {
                    insufficientIngredients.Add($"• {ingredientData.Name}: No quantity set for {selectedSize ?? "base"} size");
                    continue;
                }

                decimal available = freshInventory.TryGetValue(productIngredient.IngredientId, out var q) ? q : 0;

                if (reserved.TryGetValue(productIngredient.IngredientId, out var reservedQty))
                    available -= reservedQty;

                if (available < 0) available = 0;

                decimal required = requiredPerUnit * quantity;

                if (available < required)
                {
                    insufficientIngredients.Add(
                        $"• {ingredientData.Name}: Need {required:F1}{ingredientData.Unit ?? ""}, Have {available:F1}{ingredientData.Unit ?? ""}");
                }
            }

            if (insufficientIngredients.Any())
            {
                insufficientMessage = "Cannot add product. Insufficient ingredients:\n\n" +
                                     string.Join("\n", insufficientIngredients);
                return false;
            }

            return true;
        }

        private int GetMaxOrderableQuantity(Product product, string? selectedSize = null)
        {
            try
            {
                var fullProduct = dbService.GetProductById(product.Id);
                if (fullProduct?.ProductIngredients == null || !fullProduct.ProductIngredients.Any())
                    return 0;

                var freshInventory = dbService.GetAllInventory()
                    .ToDictionary(i => i.IngredientId ?? 0, i => i.Quantity);

                var reserved = GetCartIngredientUsage();
                int? maxQty = null;

                foreach (var pi in fullProduct.ProductIngredients)
                {
                    if (pi.IngredientId <= 0) continue;

                    decimal perUnit = GetRequiredQtyPerUnit(pi, product.HasSizes, selectedSize);
                    if (perUnit <= 0) return 0;

                    decimal available = freshInventory.TryGetValue(pi.IngredientId, out var q) ? q : 0;
                    if (reserved.TryGetValue(pi.IngredientId, out var r)) available -= r;
                    if (available < 0) available = 0;

                    int canMake = (int)Math.Floor(available / perUnit);
                    maxQty = maxQty.HasValue ? Math.Min(maxQty.Value, canMake) : canMake;

                    if (maxQty.Value <= 0) return 0;
                }

                return maxQty ?? 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMaxOrderableQuantity error: {ex.Message}");
                return 0;
            }
        }

        private void RefreshProductAvailabilityLabels()
        {
            if (flpProducts == null) return;

            foreach (Control c in flpProducts.Controls)
            {
                if (c is not Panel card || card.Tag is not Product product) continue;

                var lblAvailable = card.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Name == "lblAvailable");
                if (lblAvailable == null) continue;

                string? sizeForCalc = null;
                if (product.HasSizes)
                {
                    var cmbCardSize = card.Controls.OfType<ComboBox>()
                        .FirstOrDefault(x => x.Name == "cmbCardSize");
                    sizeForCalc = cmbCardSize?.SelectedItem?.ToString() ?? "Small";
                }

                int maxOrderable = GetMaxOrderableQuantity(product, sizeForCalc);

                lblAvailable.Text = $"Available: {maxOrderable}";
                lblAvailable.ForeColor = maxOrderable > 0 ? DeepSeekSuccess : DeepSeekDanger;

                bool hasRecipe = product.ProductIngredients != null && product.ProductIngredients.Any();
                bool isAvailable = maxOrderable > 0;

                card.BackColor = isAvailable ? DeepSeekCard : Color.FromArgb(40, 30, 30);
                card.Invalidate();

                var btnAdd = card.Controls.OfType<Button>()
                    .FirstOrDefault(b => b.Text == "Add to Cart"
                                      || b.Text == "Out of Stock"
                                      || b.Text == "No Recipe");
                if (btnAdd != null)
                {
                    btnAdd.Enabled = isAvailable;
                    btnAdd.Text = isAvailable ? "Add to Cart"
                                 : (hasRecipe ? "Out of Stock" : "No Recipe");
                    btnAdd.BackColor = isAvailable ? DeepSeekSuccess
                                      : (hasRecipe ? DeepSeekTextMuted : DeepSeekWarning);
                }

                var lblOutOfStock = card.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Name == "lblOutOfStock");
                if (lblOutOfStock != null)
                    lblOutOfStock.Visible = !isAvailable;
            }
        }

        private void UpdateProductAvailabilityDisplay()
        {
            RefreshProductAvailabilityLabels();
        }

        // ============================================================
        //  HOME / ORDER PANEL
        // ============================================================

        private void ShowHome()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();
            currentSelectedProduct = null;

            SplitContainer splitContainer = new SplitContainer
            {
                Location = new Point(20, 20),
                Size = new Size(1750, 900),
                SplitterDistance = 1100,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DeepSeekBorder,
                SplitterWidth = 2
            };

            // LEFT PANEL - PRODUCTS
            Panel pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekBg,
                Padding = new Padding(15)
            };
            pnlHome = pnlLeft;

            Label lblTitle = new Label
            {
                Text = "Order",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblSearch = new Label
            {
                Text = "Search",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 70),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtSearch = new TextBox
            {
                Location = new Point(15, 100),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search products...",
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += TxtSearch_TextChanged!;

            Label lblCategory = new Label
            {
                Text = "Category",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(330, 70),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            cmbCategory = new ComboBox
            {
                Location = new Point(330, 100),
                Size = new Size(200, 35),
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

            flpCategories = new FlowLayoutPanel
            {
                Location = new Point(15, 145),
                Size = new Size(1050, 90),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = DeepSeekBg,
                AutoScroll = false
            };
            AddCategoryButtons();

            Label lblProducts = new Label
            {
                Text = "Products",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 245),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            flpProducts = new FlowLayoutPanel
            {
                Location = new Point(15, 285),
                Size = new Size(1050, 615),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = DeepSeekBg
            };

            LoadProductCards();

            pnlLeft.Controls.AddRange(new Control[] {
                lblTitle, lblSearch, txtSearch, lblCategory, cmbCategory,
                flpCategories, lblProducts, flpProducts
            });

            // Resize hook for layout
            pnlLeft.Resize += (s, e) =>
            {
                if (flpCategories != null)
                    flpCategories.Width = pnlLeft.Width - 30;
                if (flpProducts != null)
                    flpProducts.Size = new Size(pnlLeft.Width - 30, pnlLeft.Height - 285);
            };

            // RIGHT PANEL - CART
            Panel pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DeepSeekBg,
                Padding = new Padding(15)
            };

            Label lblCartTitle = new Label
            {
                Text = "Current Order",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            dgvCart = new DataGridView
            {
                Location = new Point(15, 60),
                Size = new Size(580, 350),
                BackgroundColor = DeepSeekBg,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                GridColor = DeepSeekBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            dgvCart.EnableHeadersVisualStyles = false;
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = DeepSeekTableHeader;
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = DeepSeekText;
            dgvCart.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvCart.ColumnHeadersHeight = 40;
            dgvCart.RowsDefaultCellStyle.BackColor = DeepSeekTableRow;
            dgvCart.RowsDefaultCellStyle.ForeColor = DeepSeekText;
            dgvCart.RowsDefaultCellStyle.SelectionBackColor = DeepSeekTableSelected;
            dgvCart.AlternatingRowsDefaultCellStyle.BackColor = DeepSeekTableRowAlt;
            dgvCart.AlternatingRowsDefaultCellStyle.ForeColor = DeepSeekText;

            dgvCart.Columns.Add("Name", "Name");
            dgvCart.Columns.Add("Size", "Size");
            dgvCart.Columns.Add("Quantity", "Qty");
            dgvCart.Columns.Add("Price", "Price");
            dgvCart.Columns.Add("Total", "Total");

            dgvCart.SelectionChanged += DgvCart_SelectionChanged!;

            // ========== QUANTITY EDIT PANEL ==========
            Panel pnlQtyEdit = new Panel
            {
                Location = new Point(15, 420),
                Size = new Size(580, 45),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None
            };
            pnlQtyEdit.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlQtyEdit.Width - 1, pnlQtyEdit.Height - 1);
                }
            };

            Label lblEditQty = new Label
            {
                Text = "Edit Quantity:",
                Location = new Point(10, 12),
                AutoSize = true,
                ForeColor = DeepSeekText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            nudEditQuantity = new NumericUpDown
            {
                Location = new Point(110, 8),
                Size = new Size(70, 30),
                Minimum = 1,
                Maximum = 100,
                Value = 1,
                Font = new Font("Segoe UI", 10),
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText
            };

            Button btnQtyUp = new Button
            {
                Text = "▲",
                Location = new Point(195, 5),
                Size = new Size(35, 17),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                FlatAppearance = { BorderSize = 1, BorderColor = DeepSeekBorder }
            };
            btnQtyUp.Click += (s, e) =>
            {
                if (nudEditQuantity != null)
                {
                    nudEditQuantity.Value = Math.Min(nudEditQuantity.Value + 1, nudEditQuantity.Maximum);
                    ApplyQuantityUpdate();
                }
            };

            Button btnQtyDown = new Button
            {
                Text = "▼",
                Location = new Point(195, 24),
                Size = new Size(35, 17),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                FlatAppearance = { BorderSize = 1, BorderColor = DeepSeekBorder }
            };
            btnQtyDown.Click += (s, e) =>
            {
                if (nudEditQuantity != null)
                {
                    nudEditQuantity.Value = Math.Max(nudEditQuantity.Value - 1, nudEditQuantity.Minimum);
                    ApplyQuantityUpdate();
                }
            };

            nudEditQuantity.ValueChanged += (s, e) =>
            {
                if (dgvCart?.CurrentRow != null)
                {
                    ApplyQuantityUpdate();
                }
            };

            pnlQtyEdit.Controls.AddRange(new Control[] {
                lblEditQty, nudEditQuantity, btnQtyUp, btnQtyDown
            });

            Button btnRemoveItem = new Button
            {
                Text = "Remove Selected",
                Location = new Point(15, 475),
                Size = new Size(120, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnRemoveItem.Click += BtnRemoveItem_Click!;

            btnClearCart = new Button
            {
                Text = "Clear All",
                Location = new Point(145, 475),
                Size = new Size(100, 35),
                BackColor = DeepSeekWarning,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnClearCart.Click += BtnClearCart_Click!;

            Panel pnlTotals = new Panel
            {
                Location = new Point(15, 520),
                Size = new Size(580, 150),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None
            };
            pnlTotals.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlTotals.Width - 1, pnlTotals.Height - 1);
                }
            };

            Label lblSubTotalLabel = new Label
            {
                Text = "Sub total:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblSubTotal = new Label
            {
                Text = "₱0.00",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(450, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblTaxLabel = new Label
            {
                Text = "Tax (12%):",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 50),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblTax = new Label
            {
                Text = "₱0.00",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(450, 50),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblDiscountLabel = new Label
            {
                Text = "Discount:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(15, 80),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblDiscount = new Label
            {
                Text = "₱0.00",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(450, 80),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblTotalLabel = new Label
            {
                Text = "Total:",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekAccent,
                Location = new Point(15, 110),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblTotal = new Label
            {
                Text = "₱0.00",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekSuccess,
                Location = new Point(450, 110),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            pnlTotals.Controls.AddRange(new Control[] {
                lblSubTotalLabel, lblSubTotal,
                lblTaxLabel, lblTax,
                lblDiscountLabel, lblDiscount,
                lblTotalLabel, lblTotal
            });

            // ===== BUTTONS =====
            btnPrintBill = new Button
            {
                Text = "Print Bill",
                Location = new Point(15, 685),
                Size = new Size(150, 45),
                BackColor = DeepSeekInfo,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnPrintBill.Click += BtnPrintBill_Click!;

            Button btnPayCash = new Button
            {
                Text = "Pay Cash",
                Location = new Point(180, 685),
                Size = new Size(140, 45),
                BackColor = DeepSeekSuccess,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnPayCash.Click += BtnPayCash_Click!;

            Button btnPayGCash = new Button
            {
                Text = "GCash",
                Location = new Point(335, 685),
                Size = new Size(105, 45),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnPayGCash.Click += BtnPayGCash_Click!;

            Button btnPayHybrid = new Button
            {
                Text = "Hybrid",
                Location = new Point(450, 685),
                Size = new Size(145, 45),
                BackColor = Color.FromArgb(139, 92, 246),
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnPayHybrid.Click += BtnPayHybrid_Click!;

            // DISCOUNT PANEL
            pnlDiscount = new Panel
            {
                Location = new Point(15, 740),
                Size = new Size(580, 45),
                BackColor = DeepSeekSurface,
                BorderStyle = BorderStyle.None
            };
            pnlDiscount.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(DeepSeekBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlDiscount.Width - 1, pnlDiscount.Height - 1);
                }
            };

            chkApplyDiscount = new CheckBox
            {
                Text = "Apply Discount",
                Location = new Point(10, 12),
                AutoSize = true,
                ForeColor = DeepSeekText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Checked = false
            };
            chkApplyDiscount.CheckedChanged += ChkApplyDiscount_CheckedChanged!;

            Label lblDiscountPercentLabel = new Label
            {
                Text = "Discount (%):",
                Location = new Point(130, 14),
                AutoSize = true,
                ForeColor = DeepSeekText,
                Font = new Font("Segoe UI", 10)
            };

            nudDiscountPercent = new NumericUpDown
            {
                Location = new Point(230, 10),
                Size = new Size(60, 30),
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 0,
                Value = 0,
                Enabled = false,
                BackColor = DeepSeekBg,
                ForeColor = DeepSeekText
            };
            nudDiscountPercent.ValueChanged += NudDiscountPercent_ValueChanged!;

            Label lblDiscountAmountLabel = new Label
            {
                Text = "Discount Amount:",
                Location = new Point(310, 14),
                AutoSize = true,
                ForeColor = DeepSeekText,
                Font = new Font("Segoe UI", 10)
            };

            lblDiscountAmount = new Label
            {
                Text = "₱0.00",
                Location = new Point(430, 14),
                AutoSize = true,
                ForeColor = DeepSeekSuccess,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };

            btnApplyDiscount = new Button
            {
                Text = "Apply",
                Location = new Point(500, 8),
                Size = new Size(65, 30),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Enabled = false
            };
            btnApplyDiscount.Click += BtnApplyDiscount_Click!;

            pnlDiscount.Controls.AddRange(new Control[] {
                chkApplyDiscount,
                lblDiscountPercentLabel, nudDiscountPercent,
                lblDiscountAmountLabel, lblDiscountAmount,
                btnApplyDiscount
            });

            pnlRight.Controls.AddRange(new Control[] {
                lblCartTitle, dgvCart, pnlQtyEdit,
                btnRemoveItem, btnClearCart,
                pnlTotals, btnPrintBill, btnPayCash, btnPayGCash, btnPayHybrid, pnlDiscount
            });

            splitContainer.Panel1.Controls.Add(pnlLeft);
            splitContainer.Panel2.Controls.Add(pnlRight);

            pnlContent.Controls.Add(splitContainer);
        }

        private void ApplyQuantityUpdate()
        {
            if (dgvCart?.CurrentRow == null || nudEditQuantity == null) return;

            int index = dgvCart.CurrentRow.Index;
            if (index >= 0 && index < cartItems.Count)
            {
                int newQuantity = (int)nudEditQuantity.Value;
                var item = cartItems[index];
                var product = dbService.GetProductById(item.ProductId);

                if (product != null)
                {
                    int oldQty = cartItems[index].Quantity;
                    cartItems[index].Quantity = 0;

                    bool ok = CheckProductAvailability(product, newQuantity, item.Size, out string? message);

                    cartItems[index].Quantity = oldQty;

                    if (ok)
                    {
                        cartItems[index].Quantity = newQuantity;
                        RefreshCart();

                        if (dgvCart.Rows.Count > index)
                        {
                            dgvCart.Rows[index].Selected = true;
                            nudEditQuantity.Value = newQuantity;
                        }
                    }
                    else
                    {
                        MessageBox.Show(message, "Insufficient Stock",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        nudEditQuantity.Value = cartItems[index].Quantity;
                    }
                }
            }
        }

        private void DgvCart_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCart?.CurrentRow == null || nudEditQuantity == null) return;

            int index = dgvCart.CurrentRow.Index;
            if (index >= 0 && index < cartItems.Count)
            {
                nudEditQuantity.Value = cartItems[index].Quantity;
            }
        }

        private void AddCategoryButtons()
        {
            if (flpCategories == null) return;
            flpCategories.Controls.Clear();

            Color[] colors = { DeepSeekAccent, DeepSeekSuccess, DeepSeekInfo, DeepSeekWarning, DeepSeekDanger };
            int colorIndex = 0;

            Button btnAll = new Button
            {
                Text = "All",
                Size = new Size(70, 32),
                Margin = new Padding(3, 2, 3, 2),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Tag = "All Categories",
                FlatAppearance = { BorderSize = 0 }
            };
            btnAll.Click += BtnCategory_Click!;
            flpCategories.Controls.Add(btnAll);
            colorIndex = 1;

            var categories = dbService.GetAllCategories();
            foreach (var cat in categories)
            {
                int w = Math.Min(150, Math.Max(70, cat.Name.Length * 8 + 20));

                Button btnCat = new Button
                {
                    Text = cat.Name,
                    Size = new Size(w, 32),
                    Margin = new Padding(3, 2, 3, 2),
                    BackColor = colors[colorIndex % colors.Length],
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Tag = cat.Name,
                    FlatAppearance = { BorderSize = 0 }
                };
                btnCat.Click += BtnCategory_Click!;
                flpCategories.Controls.Add(btnCat);
                colorIndex++;
            }

            HighlightSelectedCategoryButton(btnAll);
        }

        private void HighlightSelectedCategoryButton(Button selected)
        {
            if (flpCategories == null) return;
            foreach (Control c in flpCategories.Controls)
            {
                if (c is Button b)
                {
                    b.FlatAppearance.BorderSize = (b == selected) ? 2 : 0;
                    b.FlatAppearance.BorderColor = DeepSeekText;
                }
            }
        }

        private void LoadCategories()
        {
            if (cmbCategory == null) return;
            var categories = dbService.GetAllCategories().OrderBy(c => c.Name).ToList();
            foreach (var category in categories)
            {
                cmbCategory.Items.Add(category.Name);
            }
        }

        private void LoadProductCards()
        {
            if (flpProducts == null) return;

            flpProducts.Controls.Clear();
            var products = dbService.GetAllProducts().Where(p => p.IsActive).OrderBy(p => p.Name).ToList();

            string searchText = txtSearch?.Text?.ToLower() ?? "";
            string? categoryFilter = cmbCategory?.SelectedItem?.ToString();

            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(searchText) && !product.Name.ToLower().Contains(searchText))
                    continue;

                if (categoryFilter != "All Categories" && categoryFilter != null && product.Category?.Name != categoryFilter)
                    continue;

                string? cardSizeForAvailability = product.HasSizes ? "Small" : null;

                int maxOrderable = GetMaxOrderableQuantity(product, cardSizeForAvailability);
                bool isAvailable = maxOrderable > 0;

                var fullForRecipeCheck = dbService.GetProductById(product.Id);
                bool hasRecipe = fullForRecipeCheck?.ProductIngredients != null
                                 && fullForRecipeCheck.ProductIngredients.Any();

                Panel card = new Panel
                {
                    Size = new Size(210, 440),
                    BackColor = isAvailable ? DeepSeekCard : Color.FromArgb(40, 30, 30),
                    Margin = new Padding(10),
                    Tag = product
                };

                card.Paint += (s, e) =>
                {
                    Product p = card.Tag as Product;
                    int avail = p != null ? GetMaxOrderableQuantity(p,
                        p.HasSizes
                            ? (card.Controls.OfType<ComboBox>().FirstOrDefault(x => x.Name == "cmbCardSize")?.SelectedItem?.ToString() ?? "Small")
                            : null
                    ) : 0;
                    bool availNow = avail > 0;

                    using (Pen pen = new Pen(DeepSeekBorder, 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                    }
                    using (Pen accentPen = new Pen(availNow ? DeepSeekAccent : DeepSeekDanger, 2))
                    {
                        e.Graphics.DrawLine(accentPen, 0, 0, card.Width, 0);
                    }
                };
                card.Click += (s, e) => ProductCard_Click(product);

                // PRODUCT IMAGE
                PictureBox picImage = new PictureBox
                {
                    Size = new Size(190, 110),
                    Location = new Point(10, 10),
                    BackColor = DeepSeekSurface,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.None
                };

                bool imageLoaded = false;
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string imagePath = Path.Combine(Application.StartupPath, product.ImagePath);
                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            picImage.Image = Image.FromFile(imagePath);
                            imageLoaded = true;
                        }
                        catch { }
                    }
                }

                if (!imageLoaded)
                {
                    picImage.BackColor = DeepSeekSurface;
                    Label lblPlaceholder = new Label
                    {
                        Text = "🍽️",
                        Font = new Font("Segoe UI", 36),
                        ForeColor = DeepSeekTextMuted,
                        Location = new Point(70, 30),
                        AutoSize = true,
                        BackColor = Color.Transparent
                    };
                    picImage.Controls.Add(lblPlaceholder);
                    lblPlaceholder.Click += (s, e) => ProductCard_Click(product);
                }

                picImage.Click += (s, e) => ProductCard_Click(product);

                Label lblName = new Label
                {
                    Text = product.Name.Length > 20 ? product.Name.Substring(0, 17) + "..." : product.Name,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = DeepSeekText,
                    Location = new Point(10, 128),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                lblName.Click += (s, e) => ProductCard_Click(product);

                Label lblCategory = new Label
                {
                    Text = product.Category?.Name ?? "Uncategorized",
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = DeepSeekTextMuted,
                    Location = new Point(10, 150),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                lblCategory.Click += (s, e) => ProductCard_Click(product);

                card.Controls.Add(picImage);
                card.Controls.Add(lblName);
                card.Controls.Add(lblCategory);

                if (product.HasSizes)
                {
                    Label lblSizeLabel = new Label
                    {
                        Text = "Size:",
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = DeepSeekTextMuted,
                        Location = new Point(10, 175),
                        AutoSize = true,
                        BackColor = Color.Transparent
                    };
                    card.Controls.Add(lblSizeLabel);

                    ComboBox cmbCardSize = new ComboBox
                    {
                        Name = "cmbCardSize",
                        Location = new Point(60, 173),
                        Size = new Size(90, 28),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = new Font("Segoe UI", 10),
                        BackColor = DeepSeekSurface,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat
                    };
                    cmbCardSize.Items.AddRange(new object[] { "Small", "Medium", "Large" });
                    cmbCardSize.SelectedIndex = 0;

                    Label lblSmallPrice = new Label
                    {
                        Text = $"S: ₱{product.PriceSmall?.ToString("F2") ?? "0.00"}",
                        Font = new Font("Segoe UI", 8),
                        ForeColor = DeepSeekTextMuted,
                        Location = new Point(10, 205),
                        AutoSize = true,
                        BackColor = Color.Transparent
                    };

                    Label lblMediumPrice = new Label
                    {
                        Text = $"M: ₱{product.PriceMedium?.ToString("F2") ?? "0.00"}",
                        Font = new Font("Segoe UI", 8),
                        ForeColor = DeepSeekTextMuted,
                        Location = new Point(70, 205),
                        AutoSize = true,
                        BackColor = Color.Transparent
                    };

                    Label lblLargePrice = new Label
                    {
                        Text = $"L: ₱{product.PriceLarge?.ToString("F2") ?? "0.00"}",
                        Font = new Font("Segoe UI", 8),
                        ForeColor = DeepSeekTextMuted,
                        Location = new Point(130, 205),
                        AutoSize = true,
                        BackColor = Color.Transparent
                    };

                    card.Controls.Add(lblSmallPrice);
                    card.Controls.Add(lblMediumPrice);
                    card.Controls.Add(lblLargePrice);

                    Label lblSelectedPrice = new Label
                    {
                        Text = $"Price: ₱{product.PriceSmall?.ToString("F2") ?? "0.00"}",
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = DeepSeekSuccess,
                        Location = new Point(10, 228),
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Name = "lblSelectedPrice"
                    };
                    card.Controls.Add(lblSelectedPrice);

                    cmbCardSize.SelectedIndexChanged += (s, ev) =>
                    {
                        var combo = s as ComboBox;
                        if (combo?.SelectedItem != null)
                        {
                            string selectedSize = combo.SelectedItem.ToString();
                            decimal price = selectedSize switch
                            {
                                "Small" => product.PriceSmall ?? product.Price,
                                "Medium" => product.PriceMedium ?? product.Price,
                                "Large" => product.PriceLarge ?? product.Price,
                                _ => product.Price
                            };
                            lblSelectedPrice.Text = $"Price: ₱{price:N2}";

                            var lblAvail = card.Controls.OfType<Label>()
                                .FirstOrDefault(l => l.Name == "lblAvailable");
                            if (lblAvail != null)
                            {
                                int max = GetMaxOrderableQuantity(product, selectedSize);
                                lblAvail.Text = $"Available: {max}";
                                lblAvail.ForeColor = max > 0 ? DeepSeekSuccess : DeepSeekDanger;

                                var btnA = card.Controls.OfType<Button>()
                                    .FirstOrDefault(b => b.Text == "Add to Cart" || b.Text == "Out of Stock" || b.Text == "No Recipe");
                                if (btnA != null)
                                {
                                    bool avail = max > 0;
                                    btnA.Enabled = avail;
                                    btnA.Text = avail ? "Add to Cart" : (hasRecipe ? "Out of Stock" : "No Recipe");
                                    btnA.BackColor = avail ? DeepSeekSuccess
                                                   : (hasRecipe ? DeepSeekTextMuted : DeepSeekWarning);
                                }
                            }
                        }
                    };

                    card.Controls.Add(cmbCardSize);

                    Button btnAdd = new Button
                    {
                        Text = isAvailable ? "Add to Cart" : (hasRecipe ? "Out of Stock" : "No Recipe"),
                        Size = new Size(130, 35),
                        Location = new Point(40, 260),
                        BackColor = isAvailable ? DeepSeekSuccess : (hasRecipe ? DeepSeekTextMuted : DeepSeekWarning),
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        Enabled = isAvailable,
                        FlatAppearance = { BorderSize = 0 }
                    };

                    if (!isAvailable)
                    {
                        string msg = !hasRecipe
                            ? $"Product '{product.Name}' has no ingredients configured."
                            : $"Not enough ingredients to make {product.Name}.";
                        ToolTip tooltip = new ToolTip();
                        tooltip.SetToolTip(btnAdd, msg);
                    }

                    btnAdd.Click += (s, ev) =>
                    {
                        string selectedSize = cmbCardSize.SelectedItem?.ToString() ?? "Small";
                        int qty = (int)(nudQuantity?.Value ?? 1);
                        decimal price = selectedSize switch
                        {
                            "Small" => product.PriceSmall ?? product.Price,
                            "Medium" => product.PriceMedium ?? product.Price,
                            "Large" => product.PriceLarge ?? product.Price,
                            _ => product.Price
                        };

                        if (CheckProductAvailability(product, qty, selectedSize, out string? insufficientMessage))
                        {
                            AddToCart(product, qty, price, selectedSize);
                        }
                        else
                        {
                            MessageBox.Show(insufficientMessage, "Insufficient Stock",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    };
                    card.Controls.Add(btnAdd);

                    if (!isAvailable)
                    {
                        Label lblOutOfStock = new Label
                        {
                            Text = hasRecipe ? "⚠ OUT OF STOCK" : "⚠ NO RECIPE",
                            Font = new Font("Segoe UI", 8, FontStyle.Bold),
                            ForeColor = DeepSeekDanger,
                            Location = new Point(10, 305),
                            AutoSize = true,
                            BackColor = Color.Transparent,
                            Name = "lblOutOfStock"
                        };
                        card.Controls.Add(lblOutOfStock);
                    }
                }
                else
                {
                    Label lblPrice = new Label
                    {
                        Text = $"₱{product.Price:N2}",
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = DeepSeekSuccess,
                        Location = new Point(10, 175),
                        AutoSize = true,
                        BackColor = Color.Transparent
                    };
                    lblPrice.Click += (s, e) => ProductCard_Click(product);
                    card.Controls.Add(lblPrice);

                    Button btnAdd = new Button
                    {
                        Text = isAvailable ? "Add to Cart" : (hasRecipe ? "Out of Stock" : "No Recipe"),
                        Size = new Size(130, 35),
                        Location = new Point(40, 210),
                        BackColor = isAvailable ? DeepSeekSuccess : (hasRecipe ? DeepSeekTextMuted : DeepSeekWarning),
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Enabled = isAvailable,
                        FlatAppearance = { BorderSize = 0 }
                    };

                    if (!isAvailable)
                    {
                        string msg = !hasRecipe
                            ? $"Product '{product.Name}' has no ingredients configured."
                            : $"Not enough ingredients to make {product.Name}.";
                        ToolTip tooltip = new ToolTip();
                        tooltip.SetToolTip(btnAdd, msg);
                    }

                    btnAdd.Click += (s, ev) =>
                    {
                        int qty = (int)(nudQuantity?.Value ?? 1);
                        if (CheckProductAvailability(product, qty, null, out string? insufficientMessage))
                        {
                            AddToCart(product, qty, product.Price, null);
                        }
                        else
                        {
                            MessageBox.Show(insufficientMessage, "Insufficient Stock",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    };
                    card.Controls.Add(btnAdd);

                    if (!isAvailable)
                    {
                        Label lblOutOfStock = new Label
                        {
                            Text = hasRecipe ? "⚠ OUT OF STOCK" : "⚠ NO RECIPE",
                            Font = new Font("Segoe UI", 8, FontStyle.Bold),
                            ForeColor = DeepSeekDanger,
                            Location = new Point(10, 260),
                            AutoSize = true,
                            BackColor = Color.Transparent,
                            Name = "lblOutOfStock"
                        };
                        card.Controls.Add(lblOutOfStock);
                    }
                }

                Label lblAvailable = new Label
                {
                    Text = $"Available: {maxOrderable}",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = maxOrderable > 0 ? DeepSeekSuccess : DeepSeekDanger,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Name = "lblAvailable",
                    Location = new Point(10, 395)
                };
                card.Controls.Add(lblAvailable);

                flpProducts.Controls.Add(card);
            }
        }

        private void ProductCard_Click(Product product)
        {
            currentSelectedProduct = product;

            if (cmbSize != null)
            {
                cmbSize.Enabled = product.HasSizes;
                if (product.HasSizes)
                {
                    cmbSize.BackColor = DeepSeekSurface;
                    cmbSize.SelectedIndex = 0;
                }
                else
                {
                    cmbSize.BackColor = DeepSeekSurface;
                    cmbSize.SelectedIndex = -1;
                }
            }
        }

        private void CmbSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentSelectedProduct != null && currentSelectedProduct.HasSizes && cmbSize != null)
            {
                string selectedSize = cmbSize.SelectedItem?.ToString() ?? "Small";
                decimal price = GetPriceForSize(currentSelectedProduct, selectedSize);
            }
        }

        private decimal GetPriceForSize(Product product, string size)
        {
            return size switch
            {
                "Small" => product.PriceSmall ?? product.Price,
                "Medium" => product.PriceMedium ?? product.Price,
                "Large" => product.PriceLarge ?? product.Price,
                _ => product.Price
            };
        }

        private void NudQuantity_ValueChanged(object sender, EventArgs e)
        {
            RefreshProductAvailabilityLabels();
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product product)
            {
                currentSelectedProduct = product;
                int quantity = (int)(nudQuantity?.Value ?? 1);

                decimal price = product.Price;
                string? selectedSize = null;

                if (product.HasSizes)
                {
                    if (cmbSize == null || cmbSize.SelectedItem == null)
                    {
                        MessageBox.Show("Please select a size for this product.", "Size Required",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    selectedSize = cmbSize.SelectedItem.ToString();
                    price = GetPriceForSize(product, selectedSize);
                }

                if (CheckProductAvailability(product, quantity, selectedSize, out string? insufficientMessage))
                {
                    AddToCart(product, quantity, price, selectedSize);
                }
                else
                {
                    MessageBox.Show(insufficientMessage, "Insufficient Stock",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void AddToCart(Product product, int quantity, decimal price, string? size)
        {
            string? normalizedSize = NormalizeSize(size);

            var existingItem = cartItems.FirstOrDefault(item =>
                item.ProductId == product.Id && item.Size == normalizedSize);

            if (existingItem != null)
            {
                int newQuantity = existingItem.Quantity + quantity;

                int oldQty = existingItem.Quantity;
                existingItem.Quantity = 0;

                bool ok = CheckProductAvailability(product, newQuantity, normalizedSize, out string? message);

                existingItem.Quantity = oldQty;

                if (ok)
                {
                    existingItem.Quantity = newQuantity;
                }
                else
                {
                    MessageBox.Show(message, "Cannot Increase Quantity",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Quantity = quantity,
                    Price = price,
                    Size = normalizedSize
                });
            }

            RefreshCart();
        }

        private static string? NormalizeSize(string? size)
        {
            if (string.IsNullOrWhiteSpace(size)) return null;
            return char.ToUpperInvariant(size[0]) + size.Substring(1).ToLowerInvariant();
        }

        private void RefreshCart()
        {
            if (dgvCart == null) return;

            dgvCart.Rows.Clear();
            decimal displaySubtotal = 0;

            foreach (var item in cartItems)
            {
                decimal total = item.Price * item.Quantity;
                displaySubtotal += total;

                int rowIndex = dgvCart.Rows.Add(
                    item.Name,
                    item.Size ?? "-",
                    item.Quantity,
                    $"₱{item.Price:N2}",
                    $"₱{total:N2}"
                );

                if (!string.IsNullOrEmpty(item.Size))
                {
                    dgvCart.Rows[rowIndex].Cells["Size"].Style.ForeColor = DeepSeekAccent;
                }
            }

            UpdateTotals();

            if (chkApplyDiscount != null)
            {
                chkApplyDiscount.Checked = false;
                currentDiscountAmount = 0;
            }

            RefreshProductAvailabilityLabels();
        }

        private void UpdateTotals()
        {
            if (lblSubTotal == null || lblTax == null || lblDiscount == null || lblTotal == null) return;

            decimal subTotal = cartItems.Sum(item => item.Price * item.Quantity);
            decimal discountAmount = currentDiscountAmount;
            decimal discountedSubTotal = subTotal - discountAmount;
            decimal taxRate = 0.12m;
            decimal tax = discountedSubTotal * taxRate / (1 + taxRate);
            decimal total = discountedSubTotal;

            lblSubTotal.Text = $"₱{subTotal:N2}";
            lblTax.Text = $"₱{tax:N2}";
            lblDiscount.Text = $"₱{discountAmount:N2}";
            lblTotal.Text = $"₱{total:N2}";
        }

        // ============================================================
        //  PAYMENT — CASH + GCASH
        // ============================================================

        private void ProcessPayment(string paymentMethod)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty.", "No Items",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in cartItems)
            {
                var product = dbService.GetProductById(item.ProductId);
                if (product != null)
                {
                    int oldQty = item.Quantity;
                    item.Quantity = 0;
                    bool ok = CheckProductAvailability(product, oldQty, item.Size, out string? message);
                    item.Quantity = oldQty;

                    if (!ok)
                    {
                        MessageBox.Show($"Stock has changed for {product.Name}:\n\n{message}",
                            "Stock Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            try
            {
                decimal subTotal = cartItems.Sum(item => item.Price * item.Quantity);
                decimal discountAmount = currentDiscountAmount;
                decimal discountedSubTotal = subTotal - discountAmount;
                decimal tax = discountedSubTotal * 0.12m / 1.12m;
                decimal total = discountedSubTotal;

                using (Form paymentDialog = new Form())
                {
                    paymentDialog.Text = paymentMethod == "GCash" ? "GCash Payment" : "Cash Payment";
                    paymentDialog.Size = new Size(450, 380);
                    paymentDialog.StartPosition = FormStartPosition.CenterParent;
                    paymentDialog.BackColor = DeepSeekSurface;
                    paymentDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    paymentDialog.MaximizeBox = false;
                    paymentDialog.MinimizeBox = false;

                    Label lblTotal = new Label
                    {
                        Text = $"Total Amount: ₱{total:N2}",
                        Location = new Point(20, 20),
                        Size = new Size(400, 35),
                        Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        ForeColor = DeepSeekText,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    Label lblAmountTendered = new Label
                    {
                        Text = paymentMethod == "GCash" ? "GCash Amount:" : "Amount Tendered:",
                        Location = new Point(20, 70),
                        Size = new Size(130, 25),
                        ForeColor = DeepSeekTextMuted,
                        Font = new Font("Segoe UI", 10)
                    };

                    NumericUpDown nudAmountPaid = new NumericUpDown
                    {
                        Location = new Point(160, 67),
                        Size = new Size(180, 30),
                        Minimum = 0,
                        Maximum = 100000,
                        DecimalPlaces = 2,
                        Value = paymentMethod == "GCash" ? total : 0,
                        BackColor = DeepSeekBg,
                        ForeColor = DeepSeekText,
                        Font = new Font("Segoe UI", 12)
                    };

                    Button btnPayExact = new Button
                    {
                        Text = "Pay Exact",
                        Location = new Point(350, 67),
                        Size = new Size(70, 30),
                        BackColor = DeepSeekInfo,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 }
                    };
                    btnPayExact.Click += (s, ev) => { nudAmountPaid.Value = total; };

                    Label lblRefLabel = new Label
                    {
                        Text = "GCash Ref #:",
                        Location = new Point(20, 115),
                        Size = new Size(130, 25),
                        ForeColor = DeepSeekTextMuted,
                        Font = new Font("Segoe UI", 10),
                        Visible = paymentMethod == "GCash"
                    };

                    TextBox txtRef = new TextBox
                    {
                        Location = new Point(160, 112),
                        Size = new Size(260, 30),
                        Font = new Font("Segoe UI", 11),
                        BackColor = DeepSeekBg,
                        ForeColor = DeepSeekText,
                        BorderStyle = BorderStyle.FixedSingle,
                        MaxLength = 13,
                        PlaceholderText = "13-digit reference number",
                        Visible = paymentMethod == "GCash"
                    };

                    Label lblRefHint = new Label
                    {
                        Text = "Must be exactly 13 numeric digits.",
                        Location = new Point(160, 145),
                        Size = new Size(260, 20),
                        ForeColor = DeepSeekTextMuted,
                        Font = new Font("Segoe UI", 8, FontStyle.Italic),
                        Visible = paymentMethod == "GCash"
                    };

                    txtRef.KeyPress += (s, ev) =>
                    {
                        if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                            ev.Handled = true;
                    };

                    Label lblChange = new Label
                    {
                        Text = "Change: ₱0.00",
                        Location = new Point(20, 175),
                        Size = new Size(400, 30),
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = DeepSeekSuccess,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    nudAmountPaid.ValueChanged += (s, ev) =>
                    {
                        decimal amountPaid = nudAmountPaid.Value;
                        decimal change = amountPaid - total;
                        if (change >= 0)
                        {
                            lblChange.Text = $"Change: ₱{change:N2}";
                            lblChange.ForeColor = DeepSeekSuccess;
                        }
                        else
                        {
                            lblChange.Text = $"Short: ₱{-change:N2}";
                            lblChange.ForeColor = DeepSeekDanger;
                        }
                    };

                    Button btnConfirm = new Button
                    {
                        Text = "Confirm Payment",
                        Location = new Point(150, 235),
                        Size = new Size(150, 40),
                        BackColor = DeepSeekSuccess,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 }
                    };

                    Button btnCancel = new Button
                    {
                        Text = "Cancel",
                        Location = new Point(310, 235),
                        Size = new Size(110, 40),
                        BackColor = DeepSeekDanger,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 },
                        DialogResult = DialogResult.Cancel
                    };

                    Action refreshConfirmState = () =>
                    {
                        if (paymentMethod == "GCash")
                        {
                            string refText = txtRef.Text.Trim();
                            bool refValid = refText.Length == 13 && refText.All(char.IsDigit);
                            btnConfirm.Enabled = refValid;
                            btnConfirm.BackColor = refValid ? DeepSeekSuccess : DeepSeekTextMuted;
                        }
                        else
                        {
                            btnConfirm.Enabled = true;
                            btnConfirm.BackColor = DeepSeekSuccess;
                        }
                    };

                    txtRef.TextChanged += (s, ev) => refreshConfirmState();
                    refreshConfirmState();

                    btnConfirm.Click += (s, ev) =>
                    {
                        decimal amountPaid = nudAmountPaid.Value;
                        decimal change = amountPaid - total;

                        if (change < 0)
                        {
                            MessageBox.Show($"Insufficient payment. Short by ₱{-change:N2}", "Payment Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string? refNum = null;
                        if (paymentMethod == "GCash")
                        {
                            refNum = txtRef.Text.Trim();
                            if (refNum.Length != 13 || !refNum.All(char.IsDigit))
                            {
                                MessageBox.Show("Please enter a valid 13-digit GCash reference number.",
                                    "Invalid Reference", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        var transaction = new Transaction
                        {
                            UserId = userId,
                            PaymentMethod = paymentMethod,
                            Status = "Completed",
                            SubTotal = subTotal,
                            Discount = discountAmount,
                            Tax = tax,
                            Total = total,
                            AmountPaid = amountPaid,
                            Change = change,
                            CashPortion = paymentMethod == "Cash" ? amountPaid : 0,
                            GcashPortion = paymentMethod == "GCash" ? total : 0,
                            GcashReferenceNumber = refNum
                        };

                        var items = cartItems.Select(item => (
                            productId: item.ProductId,
                            quantity: item.Quantity,
                            unitPrice: item.Price,
                            size: item.Size
                        )).ToList();

                        var savedTransaction = dbService.CreateTransaction(transaction, items);

                        // Send the order to the Barista via TCP
                        var networkOrder = new NetworkOrder
                        {
                            TransactionNumber = savedTransaction.TransactionNumber,
                            MessageType = "NEW_ORDER",
                            Items = savedTransaction.TransactionItems.Select(ti => new NetworkOrderItem
                            {
                                ProductName = ti.Product?.Name ?? "Unknown",
                                Size = ti.Size ?? "",
                                Quantity = ti.Quantity
                            }).ToList()
                        };
                        _tcpServer.BroadcastOrder(networkOrder);

                        currentDiscountAmount = 0;
                        if (chkApplyDiscount != null) chkApplyDiscount.Checked = false;
                        if (nudDiscountPercent != null) nudDiscountPercent.Value = 0;

                        LoadStockCache();
                        LoadProductCards();
                        cartItems.Clear();
                        RefreshCart();

                        currentSelectedProduct = null;
                        if (cmbSize != null) cmbSize.Enabled = false;

                        paymentDialog.DialogResult = DialogResult.OK;

                        DialogResult printResult = MessageBox.Show("Print receipt?", "Receipt",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (printResult == DialogResult.Yes)
                        {
                            GeneratePDFReceipt(savedTransaction);
                        }
                    };

                    paymentDialog.Controls.AddRange(new Control[] {
                        lblTotal, lblAmountTendered, nudAmountPaid, btnPayExact,
                        lblRefLabel, txtRef, lblRefHint,
                        lblChange, btnConfirm, btnCancel
                    });

                    paymentDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing payment: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPayCash_Click(object sender, EventArgs e) => ProcessPayment("Cash");
        private void BtnPayGCash_Click(object sender, EventArgs e) => ProcessPayment("GCash");

        // ============================================================
        //  HYBRID PAYMENT
        // ============================================================

        private void BtnPayHybrid_Click(object sender, EventArgs e) => ProcessHybridPayment();

        private void ProcessHybridPayment()
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty.", "No Items",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in cartItems)
            {
                var product = dbService.GetProductById(item.ProductId);
                if (product != null)
                {
                    int oldQty = item.Quantity;
                    item.Quantity = 0;
                    bool ok = CheckProductAvailability(product, oldQty, item.Size, out string? message);
                    item.Quantity = oldQty;

                    if (!ok)
                    {
                        MessageBox.Show($"Stock has changed for {product.Name}:\n\n{message}",
                            "Stock Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            try
            {
                decimal subTotal = cartItems.Sum(item => item.Price * item.Quantity);
                decimal discountAmount = currentDiscountAmount;
                decimal discountedSubTotal = subTotal - discountAmount;
                decimal tax = discountedSubTotal * 0.12m / 1.12m;
                decimal total = discountedSubTotal;

                using (Form dlg = new Form())
                {
                    dlg.Text = "Hybrid Payment (Cash + GCash)";
                    dlg.Size = new Size(500, 520);
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    dlg.BackColor = DeepSeekSurface;
                    dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dlg.MaximizeBox = false;
                    dlg.MinimizeBox = false;

                    Label lblTotal = new Label
                    {
                        Text = $"Total Amount: ₱{total:N2}",
                        Location = new Point(20, 20),
                        Size = new Size(440, 40),
                        Font = new Font("Segoe UI", 18, FontStyle.Bold),
                        ForeColor = DeepSeekText,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    Label lblCashLabel = new Label
                    {
                        Text = "Cash:",
                        Location = new Point(30, 90),
                        Size = new Size(80, 25),
                        ForeColor = DeepSeekTextMuted,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold)
                    };

                    NumericUpDown nudCash = new NumericUpDown
                    {
                        Location = new Point(120, 87),
                        Size = new Size(180, 32),
                        Minimum = 0,
                        Maximum = total,
                        DecimalPlaces = 2,
                        Value = 0,
                        BackColor = DeepSeekBg,
                        ForeColor = DeepSeekText,
                        Font = new Font("Segoe UI", 13)
                    };

                    Label lblGcashLabel = new Label
                    {
                        Text = "GCash:",
                        Location = new Point(30, 140),
                        Size = new Size(80, 25),
                        ForeColor = DeepSeekTextMuted,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold)
                    };

                    NumericUpDown nudGcash = new NumericUpDown
                    {
                        Location = new Point(120, 137),
                        Size = new Size(180, 32),
                        Minimum = 0,
                        Maximum = total,
                        DecimalPlaces = 2,
                        Value = total,
                        BackColor = DeepSeekBg,
                        ForeColor = DeepSeekText,
                        Font = new Font("Segoe UI", 13)
                    };

                    bool syncing = false;

                    nudCash.ValueChanged += (s, ev) =>
                    {
                        if (syncing) return;
                        syncing = true;
                        nudGcash.Value = total - nudCash.Value;
                        syncing = false;
                    };

                    nudGcash.ValueChanged += (s, ev) =>
                    {
                        if (syncing) return;
                        syncing = true;
                        nudCash.Value = total - nudGcash.Value;
                        syncing = false;
                    };

                    Button btnPreset5050 = new Button
                    {
                        Text = "50/50",
                        Location = new Point(320, 87),
                        Size = new Size(70, 32),
                        BackColor = DeepSeekInfo,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 }
                    };
                    btnPreset5050.Click += (s, ev) =>
                    {
                        syncing = true;
                        nudCash.Value = Math.Round(total / 2, 2);
                        nudGcash.Value = total - nudCash.Value;
                        syncing = false;
                    };

                    Button btnAllCash = new Button
                    {
                        Text = "All Cash",
                        Location = new Point(395, 87),
                        Size = new Size(70, 32),
                        BackColor = DeepSeekSuccess,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 }
                    };
                    btnAllCash.Click += (s, ev) =>
                    {
                        syncing = true;
                        nudCash.Value = total;
                        nudGcash.Value = 0;
                        syncing = false;
                    };

                    Button btnAllGcash = new Button
                    {
                        Text = "All GCash",
                        Location = new Point(320, 137),
                        Size = new Size(145, 32),
                        BackColor = DeepSeekAccent,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 }
                    };
                    btnAllGcash.Click += (s, ev) =>
                    {
                        syncing = true;
                        nudCash.Value = 0;
                        nudGcash.Value = total;
                        syncing = false;
                    };

                    Label lblSum = new Label
                    {
                        Text = $"Sum: ₱{total:N2}  ✓",
                        Location = new Point(30, 185),
                        Size = new Size(440, 25),
                        Font = new Font("Segoe UI", 10, FontStyle.Italic),
                        ForeColor = DeepSeekSuccess,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    Action updateSumLabel = () =>
                    {
                        decimal sum = nudCash.Value + nudGcash.Value;
                        if (Math.Abs(sum - total) < 0.01m)
                        {
                            lblSum.Text = $"Sum: ₱{sum:N2}  ✓";
                            lblSum.ForeColor = DeepSeekSuccess;
                        }
                        else
                        {
                            lblSum.Text = $"Sum: ₱{sum:N2}  (should be ₱{total:N2})";
                            lblSum.ForeColor = DeepSeekDanger;
                        }
                    };

                    Label lblRefLabel = new Label
                    {
                        Text = "GCash Ref #:",
                        Location = new Point(30, 230),
                        Size = new Size(120, 25),
                        ForeColor = DeepSeekTextMuted,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold)
                    };

                    TextBox txtRef = new TextBox
                    {
                        Location = new Point(160, 227),
                        Size = new Size(300, 32),
                        Font = new Font("Segoe UI", 12),
                        BackColor = DeepSeekBg,
                        ForeColor = DeepSeekText,
                        BorderStyle = BorderStyle.FixedSingle,
                        MaxLength = 13,
                        PlaceholderText = "13-digit reference number"
                    };

                    Label lblRefHint = new Label
                    {
                        Text = "Required whenever GCash portion is greater than ₱0. Must be exactly 13 numeric digits.",
                        Location = new Point(30, 262),
                        Size = new Size(430, 30),
                        ForeColor = DeepSeekTextMuted,
                        Font = new Font("Segoe UI", 8, FontStyle.Italic)
                    };

                    txtRef.KeyPress += (s, ev) =>
                    {
                        if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                            ev.Handled = true;
                    };

                    Button btnConfirm = new Button
                    {
                        Text = "Confirm Hybrid Payment",
                        Location = new Point(90, 320),
                        Size = new Size(200, 45),
                        BackColor = DeepSeekSuccess,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 }
                    };

                    Button btnCancel = new Button
                    {
                        Text = "Cancel",
                        Location = new Point(300, 320),
                        Size = new Size(120, 45),
                        BackColor = DeepSeekDanger,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 },
                        DialogResult = DialogResult.Cancel
                    };

                    Action refreshConfirmState = () =>
                    {
                        decimal sum = nudCash.Value + nudGcash.Value;
                        bool sumOk = Math.Abs(sum - total) < 0.01m;

                        bool refOk = true;
                        if (nudGcash.Value > 0)
                        {
                            string refText = txtRef.Text.Trim();
                            refOk = refText.Length == 13 && refText.All(char.IsDigit);
                        }

                        bool ok = sumOk && refOk;
                        btnConfirm.Enabled = ok;
                        btnConfirm.BackColor = ok ? DeepSeekSuccess : DeepSeekTextMuted;
                    };

                    txtRef.TextChanged += (s, ev) => refreshConfirmState();
                    nudCash.ValueChanged += (s, ev) => { updateSumLabel(); refreshConfirmState(); };
                    nudGcash.ValueChanged += (s, ev) => { updateSumLabel(); refreshConfirmState(); };
                    refreshConfirmState();

                    btnConfirm.Click += (s, ev) =>
                    {
                        decimal cash = nudCash.Value;
                        decimal gcash = nudGcash.Value;
                        decimal sum = cash + gcash;

                        if (Math.Abs(sum - total) >= 0.01m)
                        {
                            MessageBox.Show($"Payment amounts must sum to ₱{total:N2}. Currently ₱{sum:N2}.",
                                "Invalid Split", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string? refNum = null;
                        if (gcash > 0)
                        {
                            refNum = txtRef.Text.Trim();
                            if (refNum.Length != 13 || !refNum.All(char.IsDigit))
                            {
                                MessageBox.Show("Please enter a valid 13-digit GCash reference number.",
                                    "Invalid Reference", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        var transaction = new Transaction
                        {
                            UserId = userId,
                            PaymentMethod = "Hybrid",
                            Status = "Completed",
                            SubTotal = subTotal,
                            Discount = discountAmount,
                            Tax = tax,
                            Total = total,
                            AmountPaid = total,
                            Change = 0,
                            CashPortion = cash,
                            GcashPortion = gcash,
                            GcashReferenceNumber = refNum
                        };

                        var items = cartItems.Select(item => (
                            productId: item.ProductId,
                            quantity: item.Quantity,
                            unitPrice: item.Price,
                            size: item.Size
                        )).ToList();

                        var savedTransaction = dbService.CreateTransaction(transaction, items);

                        currentDiscountAmount = 0;
                        if (chkApplyDiscount != null) chkApplyDiscount.Checked = false;
                        if (nudDiscountPercent != null) nudDiscountPercent.Value = 0;

                        LoadStockCache();
                        LoadProductCards();
                        cartItems.Clear();
                        RefreshCart();

                        currentSelectedProduct = null;
                        if (cmbSize != null) cmbSize.Enabled = false;

                        dlg.DialogResult = DialogResult.OK;

                        DialogResult printResult = MessageBox.Show("Print receipt?", "Receipt",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (printResult == DialogResult.Yes)
                        {
                            GeneratePDFReceipt(savedTransaction);
                        }
                    };

                    dlg.Controls.AddRange(new Control[] {
                        lblTotal,
                        lblCashLabel, nudCash,
                        lblGcashLabel, nudGcash,
                        btnPreset5050, btnAllCash, btnAllGcash,
                        lblSum,
                        lblRefLabel, txtRef, lblRefHint,
                        btnConfirm, btnCancel
                    });

                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing hybrid payment: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  PDF RECEIPT
        // ============================================================

        private void GeneratePDFReceipt(Transaction transaction)
        {
            try
            {
                var fullTransaction = dbService.GetTransactionById(transaction.Id);
                var items = fullTransaction?.TransactionItems ?? new List<TransactionItem>();

                PdfDocument document = new PdfDocument();
                document.Info.Title = $"Receipt_{transaction.TransactionNumber}";

                PdfPage page = document.AddPage();
                page.Width = 500;
                page.Height = 700;

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                {
                    XFont fontTitle = new XFont("Arial", 20);
                    XFont fontSubTitle = new XFont("Arial", 14);
                    XFont fontHeader = new XFont("Arial", 13);
                    XFont fontNormal = new XFont("Arial", 12);
                    XFont fontBold = new XFont("Arial", 13);
                    XFont fontSmall = new XFont("Arial", 10);

                    XPen penLine = new XPen(XColors.Black, 1);
                    XPen penDoubleLine = new XPen(XColors.Black, 2);
                    XBrush brushBlack = new XSolidBrush(XColors.Black);

                    double yPos = 30 + (page.Height * 0.10);
                    double centerX = page.Width / 2;
                    double leftMargin = 30;
                    double rightCol = page.Width - 30;

                    string storeName = "TEN DEGREES CAFE";
                    double storeNameWidth = gfx.MeasureString(storeName, fontTitle).Width;
                    gfx.DrawString(storeName, fontTitle, brushBlack, new XPoint(centerX - storeNameWidth / 2, yPos));
                    yPos += 32;

                    string address = "Commonwealth Avenue, Quezon City";
                    double addressWidth = gfx.MeasureString(address, fontSubTitle).Width;
                    gfx.DrawString(address, fontSubTitle, brushBlack, new XPoint(centerX - addressWidth / 2, yPos));
                    yPos += 24;

                    string tin = "VAT REG TIN# 123-456-789-00012";
                    double tinWidth = gfx.MeasureString(tin, fontSubTitle).Width;
                    gfx.DrawString(tin, fontSubTitle, brushBlack, new XPoint(centerX - tinWidth / 2, yPos));
                    yPos += 24;

                    gfx.DrawLine(penDoubleLine, 30, yPos, page.Width - 30, yPos);
                    yPos += 18;

                    gfx.DrawString("SALES INVOICE", fontHeader, brushBlack, new XPoint(leftMargin, yPos));
                    gfx.DrawString($"SI # {transaction.TransactionNumber}", fontNormal, brushBlack, new XPoint(350, yPos));
                    yPos += 28;

                    gfx.DrawString($"Date: {transaction.TransactionDate:MMMM dd, yyyy HH:mm}", fontNormal, brushBlack, new XPoint(leftMargin, yPos));
                    gfx.DrawString($"Cashier: {userName}", fontNormal, brushBlack, new XPoint(350, yPos));
                    yPos += 24;

                    gfx.DrawLine(penLine, leftMargin, yPos, page.Width - leftMargin, yPos);
                    yPos += 16;

                    gfx.DrawString("Item", fontBold, brushBlack, new XPoint(leftMargin, yPos));
                    gfx.DrawString("Qty", fontBold, brushBlack, new XPoint(280, yPos));
                    gfx.DrawString("Price", fontBold, brushBlack, new XPoint(350, yPos));
                    gfx.DrawString("Total", fontBold, brushBlack, new XPoint(440, yPos));
                    yPos += 24;

                    gfx.DrawLine(penLine, leftMargin, yPos, page.Width - leftMargin, yPos);
                    yPos += 12;

                    foreach (var item in items)
                    {
                        string itemName = item.Product?.Name ?? "Unknown Product";
                        string sizeInfo = string.IsNullOrEmpty(item.Size) ? "" : $" ({item.Size})";
                        string fullName = $"{itemName}{sizeInfo}";

                        if (fullName.Length > 35)
                            fullName = fullName.Substring(0, 32) + "...";

                        gfx.DrawString(fullName, fontNormal, brushBlack, new XPoint(leftMargin, yPos));
                        gfx.DrawString($"{item.Quantity}", fontNormal, brushBlack, new XPoint(290, yPos));
                        gfx.DrawString($"₱{item.UnitPrice:N2}", fontNormal, brushBlack, new XPoint(350, yPos));
                        gfx.DrawString($"₱{item.TotalPrice:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 24;
                    }

                    gfx.DrawLine(penLine, leftMargin, yPos, page.Width - leftMargin, yPos);
                    yPos += 16;

                    gfx.DrawString("Subtotal:", fontNormal, brushBlack, new XPoint(280, yPos));
                    gfx.DrawString($"₱{transaction.SubTotal:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                    yPos += 24;

                    if (transaction.Discount > 0)
                    {
                        gfx.DrawString("Discount:", fontNormal, brushBlack, new XPoint(280, yPos));
                        gfx.DrawString($"-₱{transaction.Discount:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 24;
                    }

                    gfx.DrawString("VAT (12%):", fontNormal, brushBlack, new XPoint(280, yPos));
                    gfx.DrawString($"₱{transaction.Tax:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                    yPos += 24;

                    gfx.DrawLine(penDoubleLine, leftMargin, yPos, page.Width - leftMargin, yPos);
                    yPos += 16;

                    gfx.DrawString("TOTAL DUE", fontBold, brushBlack, new XPoint(280, yPos));
                    gfx.DrawString($"₱{transaction.Total:N2}", fontBold, brushBlack, new XPoint(440, yPos));
                    yPos += 30;

                    // Payment section — varies by method
                    if (transaction.PaymentMethod == "Hybrid")
                    {
                        gfx.DrawString("Payment (Hybrid)", fontBold, brushBlack, new XPoint(leftMargin, yPos));
                        yPos += 22;

                        gfx.DrawString("  Cash:", fontNormal, brushBlack, new XPoint(280, yPos));
                        gfx.DrawString($"₱{transaction.CashPortion:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 22;

                        gfx.DrawString("  GCash:", fontNormal, brushBlack, new XPoint(280, yPos));
                        gfx.DrawString($"₱{transaction.GcashPortion:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 22;

                        if (!string.IsNullOrEmpty(transaction.GcashReferenceNumber))
                        {
                            gfx.DrawString("    Ref #:", fontSmall, brushBlack, new XPoint(280, yPos));
                            gfx.DrawString(transaction.GcashReferenceNumber, fontSmall, brushBlack, new XPoint(440, yPos));
                            yPos += 22;
                        }

                        gfx.DrawString("Change:", fontNormal, brushBlack, new XPoint(280, yPos));
                        gfx.DrawString($"₱{transaction.Change:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 24;
                    }
                    else if (transaction.PaymentMethod == "GCash")
                    {
                        gfx.DrawString("Payment (GCash):", fontNormal, brushBlack, new XPoint(280, yPos));
                        gfx.DrawString($"₱{transaction.AmountPaid:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 22;

                        if (!string.IsNullOrEmpty(transaction.GcashReferenceNumber))
                        {
                            gfx.DrawString("  Ref #:", fontNormal, brushBlack, new XPoint(280, yPos));
                            gfx.DrawString(transaction.GcashReferenceNumber, fontNormal, brushBlack, new XPoint(440, yPos));
                            yPos += 22;
                        }

                        gfx.DrawString("Change:", fontNormal, brushBlack, new XPoint(280, yPos));
                        gfx.DrawString($"₱{transaction.Change:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 24;
                    }
                    else
                    {
                        gfx.DrawString($"Payment ({transaction.PaymentMethod}):", fontNormal, brushBlack, new XPoint(280, yPos));
                        gfx.DrawString($"₱{transaction.AmountPaid:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 24;

                        gfx.DrawString("Change:", fontNormal, brushBlack, new XPoint(280, yPos));
                        gfx.DrawString($"₱{transaction.Change:N2}", fontNormal, brushBlack, new XPoint(440, yPos));
                        yPos += 24;
                    }

                    gfx.DrawLine(penLine, leftMargin, yPos, page.Width - leftMargin, yPos);
                    yPos += 20;

                    string itemCount = $"{items.Count} Item(s)";
                    double itemCountWidth = gfx.MeasureString(itemCount, fontNormal).Width;
                    gfx.DrawString(itemCount, fontNormal, brushBlack, new XPoint(centerX - itemCountWidth / 2, yPos));
                    yPos += 28;

                    string thankYou = "Thank you for your purchase!";
                    double thankYouWidth = gfx.MeasureString(thankYou, fontBold).Width;
                    gfx.DrawString(thankYou, fontBold, brushBlack, new XPoint(centerX - thankYouWidth / 2, yPos));
                    yPos += 26;

                    string comeAgain = "Come again!";
                    double comeAgainWidth = gfx.MeasureString(comeAgain, fontNormal).Width;
                    gfx.DrawString(comeAgain, fontNormal, brushBlack, new XPoint(centerX - comeAgainWidth / 2, yPos));
                    yPos += 24;

                    gfx.DrawLine(penLine, leftMargin, yPos, page.Width - leftMargin, yPos);
                    yPos += 14;

                    string footer = "This is a system-generated receipt";
                    double footerWidth = gfx.MeasureString(footer, fontSmall).Width;
                    gfx.DrawString(footer, fontSmall, brushBlack, new XPoint(centerX - footerWidth / 2, yPos));
                    yPos += 22;

                    page.Height = yPos + 30;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    document.Save(stream, false);
                    byte[] pdfBytes = stream.ToArray();

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "PDF Files|*.pdf";
                        saveDialog.FileName = $"Receipt_{transaction.TransactionNumber}.pdf";
                        saveDialog.Title = "Save Receipt";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllBytes(saveDialog.FileName, pdfBytes);
                            MessageBox.Show($"Receipt saved successfully!\n\nLocation: {saveDialog.FileName}",
                                "Receipt Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            DialogResult openResult = MessageBox.Show("Would you like to open the PDF receipt?", "Open File",
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF receipt: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshProductAvailability()
        {
            RefreshProductAvailabilityLabels();
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart?.CurrentRow == null) return;

            int index = dgvCart.CurrentRow.Index;
            if (index >= 0 && index < cartItems.Count)
            {
                cartItems.RemoveAt(index);
                RefreshCart();
            }
        }

        private void BtnClearCart_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Clear all items from cart?", "Clear Cart",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                cartItems.Clear();
                RefreshCart();
            }
        }

        private void BtnPrintBill_Click(object sender, EventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty.", "No Items",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tempTransaction = new Transaction
            {
                TransactionNumber = "TEMP-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TransactionDate = DateTime.Now,
                SubTotal = cartItems.Sum(item => item.Price * item.Quantity),
                Tax = cartItems.Sum(item => item.Price * item.Quantity) * 0.12m / 1.12m,
                Total = cartItems.Sum(item => item.Price * item.Quantity)
            };

            GeneratePDFReceipt(tempTransaction);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) => LoadProductCards();
        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProductCards();

            if (flpCategories == null || cmbCategory == null) return;
            string selected = cmbCategory.SelectedItem?.ToString() ?? "All Categories";

            foreach (Control c in flpCategories.Controls)
            {
                if (c is Button b)
                {
                    bool isMatch = (b.Tag?.ToString() == selected);
                    b.FlatAppearance.BorderSize = isMatch ? 2 : 0;
                    b.FlatAppearance.BorderColor = DeepSeekText;
                }
            }
        }

        private void BtnCategory_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            string category = btn.Tag?.ToString() ?? "All Categories";

            if (cmbCategory != null)
            {
                if (!cmbCategory.Items.Contains(category))
                {
                    cmbCategory.Items.Add(category);
                }
                cmbCategory.SelectedItem = category;
            }

            LoadProductCards();
            HighlightSelectedCategoryButton(btn);
        }

        // ============================================================
        //  DISCOUNT
        // ============================================================

        private void ChkApplyDiscount_CheckedChanged(object sender, EventArgs e)
        {
            bool applyDiscount = chkApplyDiscount?.Checked ?? false;

            nudDiscountPercent!.Enabled = applyDiscount;
            lblDiscountAmount!.Enabled = applyDiscount;
            btnApplyDiscount!.Enabled = applyDiscount;

            if (!applyDiscount)
            {
                nudDiscountPercent.Value = 0;
                UpdateTotalsWithDiscount(0);
            }
        }

        private void NudDiscountPercent_ValueChanged(object sender, EventArgs e)
        {
            if (chkApplyDiscount?.Checked == true)
            {
                decimal discountPercent = nudDiscountPercent?.Value ?? 0;
                decimal subTotal = cartItems.Sum(item => item.Price * item.Quantity);
                decimal discountAmount = subTotal * (discountPercent / 100);
                lblDiscountAmount!.Text = $"₱{discountAmount:N2}";
            }
        }

        private void BtnApplyDiscount_Click(object sender, EventArgs e)
        {
            if (chkApplyDiscount?.Checked == true)
            {
                decimal discountPercent = nudDiscountPercent?.Value ?? 0;
                UpdateTotalsWithDiscount(discountPercent);

                MessageBox.Show($"Discount of {discountPercent}% applied successfully!",
                    "Discount Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateTotalsWithDiscount(decimal discountPercent)
        {
            if (lblSubTotal == null || lblTax == null || lblDiscount == null || lblTotal == null) return;

            decimal subTotal = cartItems.Sum(item => item.Price * item.Quantity);
            decimal discountAmount = subTotal * (discountPercent / 100);
            decimal discountedSubTotal = subTotal - discountAmount;
            decimal taxRate = 0.12m;
            decimal tax = discountedSubTotal * taxRate / (1 + taxRate);
            decimal total = discountedSubTotal;

            lblSubTotal.Text = $"₱{subTotal:N2}";
            lblTax.Text = $"₱{tax:N2}";
            lblDiscount.Text = $"₱{discountAmount:N2}";
            lblTotal.Text = $"₱{total:N2}";

            currentDiscountAmount = discountAmount;
        }

        // ============================================================
        //  PROMOS
        // ============================================================

        private void ShowPromos()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "Promotions",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblPromoCode = new Label
            {
                Text = "Enter Promo Code:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(30, 80),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtPromoCode = new TextBox
            {
                Location = new Point(30, 115),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnApplyPromo = new Button
            {
                Text = "Apply",
                Location = new Point(240, 115),
                Size = new Size(100, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnApplyPromo.Click += BtnApplyPromo_Click!;

            Label lblActivePromos = new Label
            {
                Text = "Active Promotions",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 170),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            dgvPromos = CreateDataGridView(new Point(30, 210), new Size(1700, 600));
            dgvPromos.Columns.Add("Code", "Promo Code");
            dgvPromos.Columns.Add("Name", "Promo Name");
            dgvPromos.Columns.Add("Discount", "Discount");
            dgvPromos.Columns.Add("ValidUntil", "Valid Until");
            dgvPromos.Columns.Add("Status", "Status");

            dgvPromos.Rows.Add("WELCOME10", "Welcome 10% Off", "10%", "2024-12-31", "Active");
            dgvPromos.Rows.Add("FREESHIP", "Free Shipping", "₱50", "2024-12-31", "Active");
            dgvPromos.Rows.Add("HOLIDAY20", "Holiday Special", "20%", "2024-12-25", "Active");

            foreach (DataGridViewRow row in dgvPromos.Rows)
            {
                if (row.Cells["Status"].Value?.ToString() == "Active")
                {
                    row.Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                }
            }

            pnlContent.Controls.AddRange(new Control[] { lblTitle, lblPromoCode, txtPromoCode, btnApplyPromo, lblActivePromos, dgvPromos });
        }

        private void BtnApplyPromo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPromoCode?.Text))
            {
                MessageBox.Show("Please enter a promo code.", "No Code",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show($"Promo code '{txtPromoCode.Text}' applied!",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ============================================================
        //  HISTORY
        // ============================================================

        private void ShowHistory()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "Transaction History",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblFrom = new Label
            {
                Text = "From:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(30, 80),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            dtpFrom = new DateTimePicker
            {
                Location = new Point(80, 77),
                Size = new Size(150, 35),
                Value = DateTime.Now.AddDays(-30),
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            Label lblTo = new Label
            {
                Text = "To:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(250, 80),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            dtpTo = new DateTimePicker
            {
                Location = new Point(290, 77),
                Size = new Size(150, 35),
                Value = DateTime.Now,
                Font = new Font("Segoe UI", 11),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText
            };

            btnFilter = new Button
            {
                Text = "Filter",
                Location = new Point(460, 77),
                Size = new Size(100, 35),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnFilter.Click += BtnFilter_Click!;

            dgvTransactions = CreateDataGridView(new Point(30, 130), new Size(1700, 700));
            dgvTransactions.Columns.Add("Id", "Transaction #");
            dgvTransactions.Columns.Add("Date", "Date");
            dgvTransactions.Columns.Add("Items", "Items");
            dgvTransactions.Columns.Add("SubTotal", "Sub Total");
            dgvTransactions.Columns.Add("Tax", "Tax");
            dgvTransactions.Columns.Add("Total", "Total");
            dgvTransactions.Columns.Add("Payment", "Payment");
            dgvTransactions.Columns.Add("PaymentDetail", "Payment Detail");
            dgvTransactions.Columns.Add("Status", "Status");

            LoadTransactions();

            pnlContent.Controls.AddRange(new Control[] { lblTitle, lblFrom, dtpFrom, lblTo, dtpTo, btnFilter, dgvTransactions });
        }

        private string GetPaymentDetail(Transaction t)
        {
            if (t.PaymentMethod == "Hybrid")
            {
                string detail = $"Cash ₱{t.CashPortion:N2} + GCash ₱{t.GcashPortion:N2}";
                if (!string.IsNullOrEmpty(t.GcashReferenceNumber))
                    detail += $" (Ref {t.GcashReferenceNumber})";
                return detail;
            }
            else if (t.PaymentMethod == "GCash" && !string.IsNullOrEmpty(t.GcashReferenceNumber))
            {
                return $"GCash (Ref {t.GcashReferenceNumber})";
            }
            return t.PaymentMethod;
        }

        private void LoadTransactions()
        {
            if (dgvTransactions == null) return;
            dgvTransactions.Rows.Clear();
            var transactions = dbService.GetTransactionsByCashier(userId);

            foreach (var t in transactions)
            {
                int itemCount = t.TransactionItems?.Count ?? 0;
                int rowIndex = dgvTransactions.Rows.Add(
                    t.TransactionNumber,
                    t.TransactionDate.ToString("yyyy-MM-dd HH:mm"),
                    itemCount,
                    $"₱{t.SubTotal:N2}",
                    $"₱{t.Tax:N2}",
                    $"₱{t.Total:N2}",
                    t.PaymentMethod,
                    GetPaymentDetail(t),
                    t.Status
                );

                if (t.Status == "Completed")
                    dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                else if (t.Status == "Refunded")
                    dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekWarning;
                else if (t.Status == "Cancelled")
                    dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
            }
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            if (dtpFrom == null || dtpTo == null || dgvTransactions == null) return;

            dgvTransactions.Rows.Clear();
            var allTransactions = dbService.GetTransactionsByCashier(userId);
            var filtered = allTransactions.Where(t =>
                t.TransactionDate.Date >= dtpFrom.Value.Date &&
                t.TransactionDate.Date <= dtpTo.Value.Date);

            foreach (var t in filtered)
            {
                int itemCount = t.TransactionItems?.Count ?? 0;
                int rowIndex = dgvTransactions.Rows.Add(
                    t.TransactionNumber,
                    t.TransactionDate.ToString("yyyy-MM-dd HH:mm"),
                    itemCount,
                    $"₱{t.SubTotal:N2}",
                    $"₱{t.Tax:N2}",
                    $"₱{t.Total:N2}",
                    t.PaymentMethod,
                    GetPaymentDetail(t),
                    t.Status
                );

                if (t.Status == "Completed")
                    dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                else if (t.Status == "Refunded")
                    dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekWarning;
                else if (t.Status == "Cancelled")
                    dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
            }
        }

        // ============================================================
        //  INVENTORY
        // ============================================================

        private void ShowInventory()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "Inventory",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekTextMuted,
                Location = new Point(30, 80),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtSearchInventory = new TextBox
            {
                Location = new Point(100, 77),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search ingredients...",
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchInventory.TextChanged += TxtSearchInventory_TextChanged!;

            dgvInventory = CreateDataGridView(new Point(30, 130), new Size(1700, 700));
            dgvInventory.Columns.Add("Item", "Item");
            dgvInventory.Columns.Add("Type", "Type");
            dgvInventory.Columns.Add("Quantity", "Quantity");
            dgvInventory.Columns.Add("Unit", "Unit");
            dgvInventory.Columns.Add("MinStock", "Min Stock");
            dgvInventory.Columns.Add("Status", "Status");

            LoadInventory();

            pnlContent.Controls.AddRange(new Control[] { lblTitle, lblSearch, txtSearchInventory, dgvInventory });
        }

        private void LoadInventory()
        {
            if (dgvInventory == null) return;
            dgvInventory.Rows.Clear();
            var inventory = dbService.GetAllInventory();
            string searchText = txtSearchInventory?.Text?.ToLower() ?? "";

            foreach (var item in inventory)
            {
                string itemName = "";
                string type = "";

                if (item.Ingredient != null)
                {
                    itemName = item.Ingredient.Name;
                    type = "Ingredient";

                    if (!string.IsNullOrEmpty(searchText) && !itemName.ToLower().Contains(searchText))
                        continue;

                    string status = item.Quantity <= 0 ? "Out of Stock" :
                                   item.Quantity < item.Ingredient.MinimumStock ? "Low Stock" : "In Stock";

                    int rowIndex = dgvInventory.Rows.Add(
    itemName,
    type,
    $"{item.Quantity:F1}",
    item.Unit,
    item.Ingredient.MinimumStock,
    status
);

                    if (item.Quantity <= 0)
                        dgvInventory.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                    else if (item.Quantity < item.Ingredient.MinimumStock)
                        dgvInventory.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekWarning;
                    else
                        dgvInventory.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                }
                else if (item.Product != null)
                {
                    itemName = item.Product.Name;
                    type = "Product";

                    if (!string.IsNullOrEmpty(searchText) && !itemName.ToLower().Contains(searchText))
                        continue;

                    int rowIndex = dgvInventory.Rows.Add(itemName, type, $"{item.Quantity:F0}", item.Unit, "N/A", item.Quantity <= 0 ? "Out of Stock" : "In Stock");

                    if (item.Quantity <= 0)
                        dgvInventory.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                    else
                        dgvInventory.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                }
            }
        }

        private void TxtSearchInventory_TextChanged(object sender, EventArgs e) => LoadInventory();

        // ============================================================
        //  SETTINGS
        // ============================================================

        private void ShowSettings()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(30, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            btnLogout = new Button
            {
                Text = "Logout",
                Location = new Point(30, 100),
                Size = new Size(200, 50),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnLogout.Click += (s, e) => BtnLogout_Click(s, e);

            pnlContent.Controls.AddRange(new Control[] { lblTitle, btnLogout });
        }

        // ============================================================
        //  NAVIGATION
        // ============================================================

        private void BtnHome_Click(object sender, EventArgs e) => ShowHome();
        private void BtnPromos_Click(object sender, EventArgs e) => ShowPromos();
        private void BtnHistory_Click(object sender, EventArgs e) => ShowHistory();
        private void BtnInventory_Click(object sender, EventArgs e) => ShowInventory();
        private void BtnSettings_Click(object sender, EventArgs e) => ShowSettings();

        // ============================================================
        //  CART ITEM CLASS
        // ============================================================

        private class CartItem
        {
            public int ProductId { get; set; }
            public string? Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public string? Size { get; set; }
        }
    }
}