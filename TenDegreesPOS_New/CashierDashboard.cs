using System;
using System.Collections.Generic;
using System.Drawing;
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

        // Quantity adjustment controls
        private NumericUpDown? nudEditQuantity;
        private Button? btnUpdateQty;

        // Discount controls
        private Panel? pnlDiscount;
        private CheckBox? chkApplyDiscount;
        private NumericUpDown? nudDiscountPercent;
        private Label? lblDiscountAmount;
        private Button? btnApplyDiscount;

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
            this.Size = new Size(1800, 1000);
            InitializeComponent();
            SetUserInfo(username, "Cashier");
            SetupCashierControls();
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

        private bool CheckProductAvailability(Product product, int quantity, out string? insufficientMessage)
        {
            insufficientMessage = null;

            // Get the complete product with ingredients
            var fullProduct = dbService.GetProductById(product.Id);

            // If product has no ingredients, it cannot be made - mark as unavailable
            if (fullProduct?.ProductIngredients == null || !fullProduct.ProductIngredients.Any())
            {
                insufficientMessage = $"Product '{product.Name}' has no ingredients configured.\n\nPlease ask the administrator to set up ingredients for this product.";
                return false;
            }

            var insufficientIngredients = new List<string>();
            string selectedSize = cmbSize?.SelectedItem?.ToString() ?? "";

            foreach (var productIngredient in fullProduct.ProductIngredients)  // Changed from 'pi' or 'ingredient'
            {
                if (productIngredient.IngredientId > 0)
                {
                    // Get current stock from cache or database
                    if (!ingredientStockCache.ContainsKey(productIngredient.IngredientId))
                    {
                        var inventory = dbService.GetInventoryForIngredient(productIngredient.IngredientId);
                        decimal currentStock = inventory?.Quantity ?? 0;
                        var ingredientData = dbService.GetIngredientById(productIngredient.IngredientId);  // Changed variable name
                        if (ingredientData != null)
                        {
                            ingredientStockCache[productIngredient.IngredientId] = (ingredientData, currentStock);
                        }
                    }

                    if (!ingredientStockCache.ContainsKey(productIngredient.IngredientId))
                    {
                        insufficientIngredients.Add($"• Ingredient not found in inventory");
                        continue;
                    }

                    var (ingredientStock, availableStock) = ingredientStockCache[productIngredient.IngredientId];  // Changed variable name

                    // Get required quantity based on selected size
                    decimal requiredPerUnit = 0;
                    if (product.HasSizes && !string.IsNullOrEmpty(selectedSize))
                    {
                        switch (selectedSize)
                        {
                            case "Small":
                                requiredPerUnit = productIngredient.QuantitySmall ?? productIngredient.Quantity;
                                break;
                            case "Medium":
                                requiredPerUnit = productIngredient.QuantityMedium ?? productIngredient.Quantity;
                                break;
                            case "Large":
                                requiredPerUnit = productIngredient.QuantityLarge ?? productIngredient.Quantity;
                                break;
                            default:
                                requiredPerUnit = productIngredient.Quantity;
                                break;
                        }
                    }
                    else
                    {
                        requiredPerUnit = productIngredient.Quantity;
                    }

                    // If requiredPerUnit is 0, that means no quantity set for this size
                    if (requiredPerUnit == 0)
                    {
                        insufficientIngredients.Add($"• {ingredientStock.Name}: No quantity set for {selectedSize} size");
                        continue;
                    }

                    decimal required = requiredPerUnit * quantity;

                    if (availableStock < required)
                    {
                        insufficientIngredients.Add($"• {ingredientStock.Name}: Need {required:F1}{ingredientStock.Unit ?? ""}, Have {availableStock:F1}{ingredientStock.Unit ?? ""}");
                    }
                }
            }

            if (insufficientIngredients.Any())
            {
                insufficientMessage = "Cannot add product. " + (fullProduct.ProductIngredients.Any() ? "Insufficient ingredients:" : "Missing ingredient configuration:") + "\n\n" +
                                     string.Join("\n", insufficientIngredients);
                return false;
            }

            return true;
        }

        private void UpdateProductAvailabilityDisplay()
        {
            if (flpProducts == null) return;

            foreach (Control control in flpProducts.Controls)
            {
                if (control is Panel card && card.Tag is Product product)
                {
                    int quantity = (int)(nudQuantity?.Value ?? 1);
                    bool isAvailable = CheckProductAvailability(product, quantity, out _);

                    foreach (Control child in card.Controls)
                    {
                        if (child is Button btn && btn.Text == "Add to Cart")
                        {
                            btn.Enabled = isAvailable;
                            btn.BackColor = isAvailable ? DeepSeekSuccess : DeepSeekTextMuted;
                            break;
                        }
                    }
                    card.BackColor = isAvailable ? DeepSeekCard : Color.FromArgb(40, 30, 30);
                }
            }
        }

        #region Home/Order Panel

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
                Location = new Point(15, 150),
                Size = new Size(1050, 50),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = DeepSeekBg
            };
            AddCategoryButtons();

            // REMOVED: Quantity and Size dropdown controls

            Label lblProducts = new Label
            {
                Text = "Products",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DeepSeekText,
                Location = new Point(15, 210),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            flpProducts = new FlowLayoutPanel
            {
                Location = new Point(15, 250),
                Size = new Size(1050, 650),
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

            // Add selection changed event
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

            // REMOVED: Update Qty button

            // Add up/down buttons for quick adjustment with auto-update
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

            // Add ValueChanged event to auto-update when typing
            nudEditQuantity.ValueChanged += (s, e) =>
            {
                // Only auto-update if there's a selected item
                if (dgvCart?.CurrentRow != null)
                {
                    ApplyQuantityUpdate();
                }
            };

            pnlQtyEdit.Controls.AddRange(new Control[] {
        lblEditQty, nudEditQuantity, btnQtyUp, btnQtyDown
    });

            // Buttons: Remove and Clear All
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

            // Totals Panel
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

            // BUTTONS
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
                Size = new Size(140, 45),
                BackColor = DeepSeekAccent,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            btnPayGCash.Click += BtnPayGCash_Click!;

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

            // Add ALL controls to the right panel
            pnlRight.Controls.AddRange(new Control[] {
        lblCartTitle, dgvCart, pnlQtyEdit,
        btnRemoveItem, btnClearCart,
        pnlTotals, btnPrintBill, btnPayCash, btnPayGCash, pnlDiscount
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
                    if (CheckProductAvailability(product, newQuantity, out string? message))
                    {
                        cartItems[index].Quantity = newQuantity;
                        RefreshCart();

                        // Keep the selection and update the numeric up/down
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
                        // Revert the numeric up/down to the original quantity
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

            string[] categories = { "All", "Drinks", "Foods", "Snacks", "Desserts" };
            Color[] colors = { DeepSeekAccent, DeepSeekSuccess, DeepSeekInfo, DeepSeekWarning, DeepSeekDanger };

            for (int i = 0; i < categories.Length; i++)
            {
                Button btnCategory = new Button
                {
                    Text = categories[i],
                    Size = new Size(100, 35),
                    Margin = new Padding(5, 0, 5, 0),
                    BackColor = colors[i % colors.Length],
                    ForeColor = DeepSeekText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Tag = categories[i],
                    FlatAppearance = { BorderSize = 0 }
                };
                btnCategory.Click += BtnCategory_Click!;
                flpCategories.Controls.Add(btnCategory);
            }
        }

        private void LoadCategories()
        {
            if (cmbCategory == null) return;
            var categories = dbService.GetAllCategories();
            foreach (var category in categories)
            {
                cmbCategory.Items.Add(category.Name);
            }
        }

        private void LoadProductCards()
        {
            if (flpProducts == null) return;

            flpProducts.Controls.Clear();
            var products = dbService.GetAllProducts().Where(p => p.IsActive);

            string searchText = txtSearch?.Text?.ToLower() ?? "";
            string? categoryFilter = cmbCategory?.SelectedItem?.ToString();

            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(searchText) && !product.Name.ToLower().Contains(searchText))
                    continue;

                if (categoryFilter != "All Categories" && categoryFilter != null && product.Category?.Name != categoryFilter)
                    continue;

                int quantity = (int)(nudQuantity?.Value ?? 1);
                bool isAvailable = CheckProductAvailability(product, quantity, out string? availabilityMessage);

                // Create product card
                Panel card = new Panel
                {
                    Size = new Size(210, 420),
                    BackColor = isAvailable ? DeepSeekCard : Color.FromArgb(40, 30, 30),
                    Margin = new Padding(10),
                    Tag = product
                };

                card.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(DeepSeekBorder, 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                    }
                    using (Pen accentPen = new Pen(isAvailable ? DeepSeekAccent : DeepSeekDanger, 2))
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

                // Product name
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

                // Product category
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
                    // Size label
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

                    // Size ComboBox
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

                    // Price display for each size
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

                    // Selected price display
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

                    // Update price when size changes
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
                            lblSelectedPrice.Text = $"Price: ₱{price:F2}";
                        }
                    };

                    card.Controls.Add(cmbCardSize);

                    // Add to Cart button
                    Button btnAdd = new Button
                    {
                        Text = isAvailable ? "Add to Cart" : (availabilityMessage?.Contains("no ingredients") == true ? "No Recipe" : "Out of Stock"),
                        Size = new Size(130, 35),
                        Location = new Point(40, 260),
                        BackColor = isAvailable ? DeepSeekSuccess : (availabilityMessage?.Contains("no ingredients") == true ? DeepSeekWarning : DeepSeekTextMuted),
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        Enabled = isAvailable,
                        FlatAppearance = { BorderSize = 0 }
                    };

                    // Add tooltip for unavailable products
                    if (!isAvailable && availabilityMessage != null)
                    {
                        ToolTip tooltip = new ToolTip();
                        tooltip.SetToolTip(btnAdd, availabilityMessage);
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

                        if (CheckProductAvailability(product, qty, out string? insufficientMessage))
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
                            Text = availabilityMessage?.Contains("no ingredients") == true ? "⚠ NO RECIPE" : "⚠ OUT OF STOCK",
                            Font = new Font("Segoe UI", 8, FontStyle.Bold),
                            ForeColor = DeepSeekDanger,
                            Location = new Point(10, 305),
                            AutoSize = true,
                            BackColor = Color.Transparent
                        };
                        card.Controls.Add(lblOutOfStock);
                    }
                }
                else
                {
                    // No sizes - regular product
                    Label lblPrice = new Label
                    {
                        Text = $"₱{product.Price:F2}",
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
                        Text = isAvailable ? "Add to Cart" : (availabilityMessage?.Contains("no ingredients") == true ? "No Recipe" : "Out of Stock"),
                        Size = new Size(130, 35),
                        Location = new Point(40, 210),
                        BackColor = isAvailable ? DeepSeekSuccess : (availabilityMessage?.Contains("no ingredients") == true ? DeepSeekWarning : DeepSeekTextMuted),
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Enabled = isAvailable,
                        FlatAppearance = { BorderSize = 0 }
                    };

                    // Add tooltip for unavailable products
                    if (!isAvailable && availabilityMessage != null)
                    {
                        ToolTip tooltip = new ToolTip();
                        tooltip.SetToolTip(btnAdd, availabilityMessage);
                    }

                    btnAdd.Click += (s, ev) =>
                    {
                        int qty = (int)(nudQuantity?.Value ?? 1);
                        if (CheckProductAvailability(product, qty, out string? insufficientMessage))
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
                            Text = availabilityMessage?.Contains("no ingredients") == true ? "⚠ NO RECIPE" : "⚠ OUT OF STOCK",
                            Font = new Font("Segoe UI", 8, FontStyle.Bold),
                            ForeColor = DeepSeekDanger,
                            Location = new Point(10, 260),
                            AutoSize = true,
                            BackColor = Color.Transparent
                        };
                        card.Controls.Add(lblOutOfStock);
                    }
                }

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

        private void DebugStockLevels()
        {
            foreach (var kvp in ingredientStockCache)
            {
                var (ingredient, stock) = kvp.Value;
                System.Diagnostics.Debug.WriteLine($"Ingredient: {ingredient.Name}, Stock: {stock}");
            }
        }

        private void NudQuantity_ValueChanged(object sender, EventArgs e)
        {
            RefreshProductAvailability();
            UpdateProductAvailabilityDisplay();
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

                if (CheckProductAvailability(product, quantity, out string? insufficientMessage))
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
            var existingItem = cartItems.FirstOrDefault(item =>
                item.ProductId == product.Id && item.Size == size);

            if (existingItem != null)
            {
                int newQuantity = existingItem.Quantity + quantity;
                if (CheckProductAvailability(product, newQuantity, out string? message))
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
                    Size = size
                });
            }

            RefreshCart();
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
                    $"₱{item.Price:F2}",
                    $"₱{total:F2}"
                );

                if (!string.IsNullOrEmpty(item.Size))
                {
                    dgvCart.Rows[rowIndex].Cells["Size"].Style.ForeColor = DeepSeekAccent;
                }
            }

            // Update totals
            UpdateTotals();

            // Reset discount when cart changes
            if (chkApplyDiscount != null)
            {
                chkApplyDiscount.Checked = false;
                currentDiscountAmount = 0;
            }

            // Debug output
            System.Diagnostics.Debug.WriteLine($"Cart refreshed with {cartItems.Count} items, Subtotal: ₱{displaySubtotal}");
        }

        private void UpdateTotals()
        {
            if (lblSubTotal == null || lblTax == null || lblDiscount == null || lblTotal == null) return;

            decimal subTotal = cartItems.Sum(item => item.Price * item.Quantity);
            decimal discountAmount = currentDiscountAmount;
            decimal discountedSubTotal = subTotal - discountAmount;

            // VAT is already included in the price
            // Tax is the VAT component of the discounted subtotal
            // Formula: Tax = Total * 12 / 112 (since 12% VAT on a price that includes VAT)
            decimal taxRate = 0.12m;
            decimal tax = discountedSubTotal * taxRate / (1 + taxRate);
            decimal total = discountedSubTotal; // Total is the discounted subtotal (VAT already included)

            lblSubTotal.Text = $"₱{subTotal:F2}";
            lblTax.Text = $"₱{tax:F2}";
            lblDiscount.Text = $"₱{discountAmount:F2}";
            lblTotal.Text = $"₱{total:F2}";
        }

        private void ProcessPayment(string paymentMethod)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty.", "No Items",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check stock availability for all items before processing
            foreach (var item in cartItems)
            {
                var product = dbService.GetProductById(item.ProductId);
                if (product != null)
                {
                    if (!CheckProductAvailability(product, item.Quantity, out string? message))
                    {
                        MessageBox.Show($"Stock has changed for {product.Name}:\n\n{message}",
                            "Stock Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            try
            {
                // Calculate totals from cart items - THIS IS THE SOURCE OF TRUTH
                decimal subTotal = cartItems.Sum(item => item.Price * item.Quantity);
                decimal discountAmount = currentDiscountAmount;
                decimal discountedSubTotal = subTotal - discountAmount;

                // Tax is the VAT component (12% of the discounted price, but price includes VAT)
                // Formula: Tax = DiscountedSubTotal * 12 / 112
                decimal tax = discountedSubTotal * 0.12m / 1.12m;
                decimal total = discountedSubTotal; // Total is the discounted subtotal (VAT already included)

                // DEBUG: Print values
                System.Diagnostics.Debug.WriteLine($"=== PAYMENT CALCULATION ===");
                System.Diagnostics.Debug.WriteLine($"Subtotal from cart: {subTotal}");
                System.Diagnostics.Debug.WriteLine($"Discount: {discountAmount}");
                System.Diagnostics.Debug.WriteLine($"Discounted Subtotal: {discountedSubTotal}");
                System.Diagnostics.Debug.WriteLine($"Tax (12% of discounted): {tax}");
                System.Diagnostics.Debug.WriteLine($"Total: {total}");

                // Create payment dialog to get amount paid
                using (Form paymentDialog = new Form())
                {
                    paymentDialog.Text = "Payment";
                    paymentDialog.Size = new Size(350, 200);
                    paymentDialog.StartPosition = FormStartPosition.CenterParent;
                    paymentDialog.BackColor = DeepSeekSurface;
                    paymentDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    paymentDialog.MaximizeBox = false;
                    paymentDialog.MinimizeBox = false;

                    Label lblTotal = new Label
                    {
                        Text = $"Total Amount: ₱{total:F2}",
                        Location = new Point(20, 20),
                        Size = new Size(200, 30),
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = DeepSeekText
                    };

                    Label lblAmountPaid = new Label
                    {
                        Text = "Amount Paid:",
                        Location = new Point(20, 65),
                        Size = new Size(100, 25),
                        ForeColor = DeepSeekTextMuted
                    };

                    NumericUpDown nudAmountPaid = new NumericUpDown
                    {
                        Location = new Point(130, 62),
                        Size = new Size(120, 30),
                        Minimum = total,
                        Maximum = total * 2,
                        DecimalPlaces = 2,
                        Value = total,
                        BackColor = DeepSeekBg,
                        ForeColor = DeepSeekText
                    };

                    Label lblChange = new Label
                    {
                        Text = "Change: ₱0.00",
                        Location = new Point(20, 105),
                        Size = new Size(200, 25),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = DeepSeekSuccess
                    };

                    nudAmountPaid.ValueChanged += (s, ev) =>
                    {
                        decimal change = nudAmountPaid.Value - total;
                        lblChange.Text = change >= 0 ? $"Change: ₱{change:F2}" : $"Short: ₱{-change:F2}";
                        lblChange.ForeColor = change >= 0 ? DeepSeekSuccess : DeepSeekDanger;
                    };

                    Button btnConfirm = new Button
                    {
                        Text = "Confirm Payment",
                        Location = new Point(20, 140),
                        Size = new Size(120, 35),
                        BackColor = DeepSeekSuccess,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        DialogResult = DialogResult.OK
                    };

                    Button btnCancel = new Button
                    {
                        Text = "Cancel",
                        Location = new Point(150, 140),
                        Size = new Size(80, 35),
                        BackColor = DeepSeekDanger,
                        ForeColor = DeepSeekText,
                        FlatStyle = FlatStyle.Flat,
                        DialogResult = DialogResult.Cancel
                    };

                    paymentDialog.Controls.AddRange(new Control[] { lblTotal, lblAmountPaid, nudAmountPaid, lblChange, btnConfirm, btnCancel });

                    if (paymentDialog.ShowDialog() == DialogResult.OK)
                    {
                        decimal amountPaid = nudAmountPaid.Value;
                        decimal change = amountPaid - total;

                        if (change < 0)
                        {
                            MessageBox.Show($"Insufficient payment. Short by ₱{-change:F2}", "Payment Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Create transaction with CORRECT values
                        var transaction = new Transaction
                        {
                            UserId = userId,
                            PaymentMethod = paymentMethod,
                            Status = "Completed",
                            SubTotal = subTotal,           // Use the calculated subtotal
                            Discount = discountAmount,
                            Tax = tax,
                            Total = total,                 // Use the calculated total
                            AmountPaid = amountPaid,
                            Change = change
                        };

                        // Create the items list with unit price and size
                        var items = cartItems.Select(item => (
                            productId: item.ProductId,
                            quantity: item.Quantity,
                            unitPrice: item.Price,
                            size: item.Size
                        )).ToList();

                        var savedTransaction = dbService.CreateTransaction(transaction, items);

                        // Show receipt with correct amounts using the ACTUAL cart items
                        string receipt = GenerateReceiptString(savedTransaction, cartItems);
                        MessageBox.Show(receipt, "Payment Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Reset discount
                        currentDiscountAmount = 0;
                        if (chkApplyDiscount != null) chkApplyDiscount.Checked = false;
                        if (nudDiscountPercent != null) nudDiscountPercent.Value = 0;

                        // Refresh stock cache
                        LoadStockCache();
                        LoadProductCards();

                        cartItems.Clear();
                        RefreshCart();

                        currentSelectedProduct = null;
                        if (cmbSize != null) cmbSize.Enabled = false;

                        // Ask to print receipt
                        DialogResult printResult = MessageBox.Show("Print receipt?", "Receipt",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (printResult == DialogResult.Yes)
                        {
                            PrintReceipt(savedTransaction);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing payment: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateReceiptString(Transaction transaction, List<CartItem> items)
        {
            string receipt = "═══════════════════════════════════\n";
            receipt += "         TEN DEGREES COFFEE          \n";
            receipt += "═══════════════════════════════════\n";
            receipt += $"Transaction #: {transaction.TransactionNumber}\n";
            receipt += $"Date: {transaction.TransactionDate:yyyy-MM-dd HH:mm:ss}\n";
            receipt += $"Cashier: {userName}\n";
            receipt += "───────────────────────────────────\n";

            decimal calculatedSubtotal = 0;
            foreach (var item in items)
            {
                string sizeInfo = string.IsNullOrEmpty(item.Size) ? "" : $" [{item.Size}]";
                string itemName = item.Name ?? "Unknown Product";
                if (itemName.Length > 25) itemName = itemName.Substring(0, 22) + "...";

                decimal itemTotal = item.Price * item.Quantity;
                calculatedSubtotal += itemTotal;

                receipt += $"{itemName}{sizeInfo}\n";
                receipt += $"  {item.Quantity} x ₱{item.Price:F2} = ₱{itemTotal:F2}\n";
            }

            receipt += "───────────────────────────────────\n";
            receipt += $"Subtotal:        ₱{calculatedSubtotal:F2}\n";

            if (transaction.Discount > 0)
            {
                receipt += $"Discount:       -₱{transaction.Discount:F2}\n";
                receipt += $"Discounted Total: ₱{calculatedSubtotal - transaction.Discount:F2}\n";
            }

            receipt += $"VAT (12%):       ₱{transaction.Tax:F2}\n";
            receipt += $"Total:           ₱{transaction.Total:F2}\n";
            receipt += $"Payment:         ₱{transaction.AmountPaid:F2}\n";
            receipt += $"Change:          ₱{transaction.Change:F2}\n";
            receipt += "═══════════════════════════════════\n";
            receipt += "     Thank you for your purchase!   \n";
            receipt += "          Come again!               \n";
            receipt += "═══════════════════════════════════\n";

            return receipt;
        }

        private void PrintReceipt(Transaction transaction)
        {
            string receipt = "═══════════════════════════════════\n";
            receipt += "         TEN DEGREES COFFEE          \n";
            receipt += "═══════════════════════════════════\n";
            receipt += $"Transaction #: {transaction.TransactionNumber}\n";
            receipt += $"Date: {transaction.TransactionDate:yyyy-MM-dd HH:mm:ss}\n";
            receipt += $"Cashier: {userName}\n";
            receipt += "───────────────────────────────────\n";

            // Get the items from the transaction
            var fullTransaction = dbService.GetTransactionById(transaction.Id);
            var items = fullTransaction?.TransactionItems ?? new List<TransactionItem>();

            foreach (var item in items)
            {
                string sizeInfo = string.IsNullOrEmpty(item.Size) ? "" : $" [{item.Size}]";
                string itemName = item.Product?.Name ?? "Unknown Product";
                if (itemName.Length > 25) itemName = itemName.Substring(0, 22) + "...";

                receipt += $"{itemName}{sizeInfo}\n";
                receipt += $"  {item.Quantity} x ₱{item.UnitPrice:F2} = ₱{item.TotalPrice:F2}\n";
            }

            receipt += "───────────────────────────────────\n";
            receipt += $"Subtotal:        ₱{transaction.SubTotal:F2}\n";

            if (transaction.Discount > 0)
            {
                receipt += $"Discount:       -₱{transaction.Discount:F2}\n";
                receipt += $"Discounted Total: ₱{transaction.SubTotal - transaction.Discount:F2}\n";
            }

            receipt += $"VAT (12%):       ₱{transaction.Tax:F2}\n";
            receipt += $"Total:           ₱{transaction.Total:F2}\n";
            receipt += $"Payment:         ₱{transaction.AmountPaid:F2}\n";
            receipt += $"Change:          ₱{transaction.Change:F2}\n";
            receipt += "═══════════════════════════════════\n";
            receipt += "     Thank you for your purchase!   \n";
            receipt += "          Come again!               \n";
            receipt += "═══════════════════════════════════\n";

            MessageBox.Show(receipt, "Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) => LoadProductCards();
        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e) => LoadProductCards();

        private void BtnCategory_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                string category = btn.Tag?.ToString() ?? "All";
                if (cmbCategory != null)
                {
                    if (category == "All")
                        cmbCategory.SelectedIndex = 0;
                    else
                        cmbCategory.SelectedItem = category;
                }
            }
        }

        private void RefreshProductAvailability()
        {
            if (flpProducts == null) return;

            foreach (Control control in flpProducts.Controls)
            {
                if (control is Panel card && card.Tag is Product product)
                {
                    int quantity = (int)(nudQuantity?.Value ?? 1);
                    bool isAvailable = CheckProductAvailability(product, quantity, out _);

                    // Find the Add to Cart button in the card
                    Button? addButton = null;
                    foreach (Control child in card.Controls)
                    {
                        if (child is Button btn && (btn.Text == "Add to Cart" || btn.Text.Contains("Add")))
                        {
                            addButton = btn;
                            break;
                        }
                    }

                    if (addButton != null)
                    {
                        addButton.Enabled = isAvailable;
                        addButton.BackColor = isAvailable ? DeepSeekSuccess : DeepSeekTextMuted;
                        addButton.Text = isAvailable ? "Add to Cart" : "Out of Stock";
                    }

                    // Change card appearance for out of stock items
                    card.BackColor = isAvailable ? DeepSeekCard : Color.FromArgb(40, 30, 30);
                }
            }
        }

        private void BtnUpdateQuantity_Click(object sender, EventArgs e)
        {
            if (dgvCart?.CurrentRow == null)
            {
                MessageBox.Show("Please select an item to update.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int index = dgvCart.CurrentRow.Index;
            if (index >= 0 && index < cartItems.Count)
            {
                int newQuantity = (int)(nudEditQuantity?.Value ?? 1);
                var item = cartItems[index];
                var product = dbService.GetProductById(item.ProductId);

                if (product != null)
                {
                    if (CheckProductAvailability(product, newQuantity, out string? message))
                    {
                        cartItems[index].Quantity = newQuantity;
                        RefreshCart();

                        // Reset the numeric up/down after update
                        if (nudEditQuantity != null)
                        {
                            nudEditQuantity.Value = newQuantity;
                        }
                    }
                    else
                    {
                        MessageBox.Show(message, "Insufficient Stock",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
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
                Tax = cartItems.Sum(item => item.Price * item.Quantity) * 0.12m,
                Total = cartItems.Sum(item => item.Price * item.Quantity) * 1.12m
            };

            PrintReceipt(tempTransaction);
        }

        private void BtnPayCash_Click(object sender, EventArgs e) => ProcessPayment("Cash");
        // Change this method name from BtnPayCard_Click to BtnPayGCash_Click
        private void BtnPayGCash_Click(object sender, EventArgs e)
        {
            ProcessPayment("GCash");
        }

        #endregion

        #region Discount Methods

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
                lblDiscountAmount!.Text = $"₱{discountAmount:F2}";
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

            lblSubTotal.Text = $"₱{subTotal:F2}";
            lblTax.Text = $"₱{tax:F2}";
            lblDiscount.Text = $"₱{discountAmount:F2}";
            lblTotal.Text = $"₱{total:F2}";

            
        }

        // Add this field to store discount amount
        private decimal currentDiscountAmount = 0;

        #endregion

        #region Promos Panel

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

        #endregion

        #region History Panel

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
            dgvTransactions.Columns.Add("Status", "Status");

            LoadTransactions();

            pnlContent.Controls.AddRange(new Control[] { lblTitle, lblFrom, dtpFrom, lblTo, dtpTo, btnFilter, dgvTransactions });
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
                    $"₱{t.SubTotal:F2}",
                    $"₱{t.Tax:F2}",
                    $"₱{t.Total:F2}",
                    t.PaymentMethod,
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
                    $"₱{t.SubTotal:F2}",
                    $"₱{t.Tax:F2}",
                    $"₱{t.Total:F2}",
                    t.PaymentMethod,
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

        #endregion

        #region Inventory Panel

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

                    int rowIndex = dgvInventory.Rows.Add(itemName, type, $"{item.Quantity:F1}", item.Unit, item.Ingredient.MinimumStock, status);

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

        #endregion

        #region Settings Panel

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

        #endregion

        #region Navigation

        private void BtnHome_Click(object sender, EventArgs e) => ShowHome();
        private void BtnPromos_Click(object sender, EventArgs e) => ShowPromos();
        private void BtnHistory_Click(object sender, EventArgs e) => ShowHistory();
        private void BtnInventory_Click(object sender, EventArgs e) => ShowInventory();
        private void BtnSettings_Click(object sender, EventArgs e) => ShowSettings();

        #endregion

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