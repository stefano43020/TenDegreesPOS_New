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

        private string currentOrderNotes = "";
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

        private Label? lblQueueBadge;

        // Home/Order panel controls
        private FlowLayoutPanel? flpCategories;
        private FlowLayoutPanel flpPreparingQueue;
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

        private HashSet<string> preparingTransactions = new HashSet<string>();

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

        private Panel pnlQueueView;
        private FlowLayoutPanel flpCashierQueue;
        private Button btnQueue;

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
            this.FormBorderStyle = FormBorderStyle.Sizable; // Adds the standard window border
            this.WindowState = FormWindowState.Maximized;   // Opens full screen

            InitializeComponent();
            SetUserInfo(username, "Cashier");
            SetupCashierControls();
            LoadStockCache();
            LoadProductCache();

            _tcpServer.Start(8888);
            _tcpServer.OnMessageReceived += HandleServerMessage;

            ShowHome();
            // Add a timer to refresh the badge every 10 seconds
            System.Windows.Forms.Timer queueTimer = new System.Windows.Forms.Timer();
            queueTimer.Interval = 10000; // 10 seconds
            queueTimer.Tick += (s, e) => {
                if (btnQueue != null)
                {
                    UpdateQueueBadge();
                }
            };
            queueTimer.Start();
        }

        private void InitializeComponent()
        {
            this.Text = "Cashier Dashboard";
        }

        private void UpdateQueueBadge()
        {
            if (lblQueueBadge == null) return;

            int queueCount = 0;

            if (flpCashierQueue != null)
            {
                // Count only the ones still in the Waiting panel
                queueCount = flpCashierQueue.Controls.Count;
            }
            else
            {
                // Fallback to DB if tab isn't open
                queueCount = dbService.GetAllTransactions()
                    .Count(t => t.TransactionDate.Date == DateTime.Today && t.Status == "Completed");
            }

            if (queueCount > 0)
            {
                lblQueueBadge.Text = queueCount.ToString();
                lblQueueBadge.Visible = true;
            }
            else
            {
                lblQueueBadge.Visible = false;
            }
        }
        private void SetupCashierControls()
        {
            btnHome = CreateSidebarButton("🏠 Home", 10, BtnHome_Click!);

            // ▼▼▼ ONLY CREATE btnQueue ONCE ▼▼▼
            btnQueue = CreateSidebarButton("📋 Order Queue", 60, BtnQueue_Click!);

            // Setup the badge
            lblQueueBadge = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(239, 68, 68), // Red
                Size = new Size(22, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(160, 10), // Positioned on the right side of the button
                Visible = false // Hidden by default
            };

            // Make it round
            lblQueueBadge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, lblQueueBadge.Width - 1, lblQueueBadge.Height - 1);
                    lblQueueBadge.Region = new Region(path);
                }
            };

            // ▼▼▼ THIS WAS MISSING: Add the badge to the button ▼▼▼
            btnQueue.Controls.Add(lblQueueBadge);
            // ▲▲▲ END OF MISSING CODE ▲▲▲

            btnHistory = CreateSidebarButton("📋 History", 110, BtnHistory_Click!);
            btnInventory = CreateSidebarButton("📦 Inventory", 160, BtnInventory_Click!);
            btnSettings = CreateSidebarButton("⚙ Settings", 210, BtnSettings_Click!);

            if (pnlSidebar != null)
            {
                pnlSidebar.Controls.AddRange(new Control[] {
                    btnHome, btnQueue, btnHistory, btnInventory, btnSettings
                });
            }
        }

        private void HandleServerMessage(string message)
        {
            try
            {
                var orderMsg = System.Text.Json.JsonSerializer.Deserialize<NetworkOrder>(message);

                if (orderMsg != null)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (flpCashierQueue == null) return;

                        // Find the specific card in EITHER queue
                        Panel? targetCard = null;
                        foreach (Control control in flpCashierQueue.Controls)
                        {
                            if (control is Panel card && card.Tag is string orderNum && orderNum == orderMsg.TransactionNumber)
                            {
                                targetCard = card;
                                break;
                            }
                        }
                        // If not found in the waiting queue, check the preparing queue
                        if (targetCard == null && flpPreparingQueue != null)
                        {
                            foreach (Control control in flpPreparingQueue.Controls)
                            {
                                if (control is Panel card && card.Tag is string orderNum && orderNum == orderMsg.TransactionNumber)
                                {
                                    targetCard = card;
                                    break;
                                }
                            }
                        }

                        if (orderMsg.MessageType == "NEW_ORDER")
                        {
                            if (targetCard == null)
                            {
                                AddCardToCashierQueue(orderMsg);
                                UpdateQueueBadge();
                            }
                        }
                        else if (orderMsg.MessageType == "ORDER_PREPARING")
                        {
                            preparingTransactions.Add(orderMsg.TransactionNumber);

                            if (targetCard != null)
                            {
                                var lblStatus = targetCard.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblStatus");
                                if (lblStatus != null)
                                {
                                    lblStatus.Text = "PREPARING";
                                    lblStatus.ForeColor = DeepSeekAccent;
                                }
                                targetCard.BackColor = Color.FromArgb(30, 40, 55);

                                // Move the card visually from waiting to preparing
                                flpCashierQueue.Controls.Remove(targetCard);
                                if (flpPreparingQueue != null) flpPreparingQueue.Controls.Add(targetCard);
                            }
                        }
                        else if (orderMsg.MessageType == "ORDER_COMPLETED")
                        {
                            preparingTransactions.Remove(orderMsg.TransactionNumber);

                            // Remove the card from whichever queue it's in
                            if (targetCard != null)
                            {
                                if (flpCashierQueue.Controls.Contains(targetCard))
                                    flpCashierQueue.Controls.Remove(targetCard);
                                if (flpPreparingQueue != null && flpPreparingQueue.Controls.Contains(targetCard))
                                    flpPreparingQueue.Controls.Remove(targetCard);

                                targetCard.Dispose();
                            }
                            else
                            {
                                // ▼▼▼ FALLBACK: If we can't find the card reference, remove by TransactionNumber ▼▼▼
                                RemoveCardByTransactionNumber(flpCashierQueue, orderMsg.TransactionNumber);
                                if (flpPreparingQueue != null) RemoveCardByTransactionNumber(flpPreparingQueue, orderMsg.TransactionNumber);
                                // ▲▲▲ END OF FALLBACK ▲▲▲
                            }
                            // Update DB status
                            var tx = dbService.GetTransactionByNumber(orderMsg.TransactionNumber);
                            if (tx != null)
                            {
                                dbService.UpdateTransactionStatus(tx.Id, "Fulfilled");
                            }

                            // ▼▼▼ Refresh the badge (which should now show fewer waiting orders)
                            UpdateQueueBadge();
                            // ▲▲▲ END OF FIX ▲▲▲

                            MessageBox.Show($"Order #{orderMsg.TransactionNumber.Split('-').Last()} has been completed by the Barista.",
                                "Order Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    });
                }
            }
            catch { }
        }

        private void RemoveCardByTransactionNumber(FlowLayoutPanel panel, string transactionNumber)
        {
            if (panel == null) return;
            for (int i = panel.Controls.Count - 1; i >= 0; i--)
            {
                if (panel.Controls[i] is Panel card && card.Tag is string orderNum && orderNum == transactionNumber)
                {
                    panel.Controls.Remove(card);
                    card.Dispose();
                    return;
                }
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

        private void BtnQueue_Click(object? sender, EventArgs e)
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label { Text = "Live Kitchen Queue", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(30, 20), AutoSize = true };

            // === WAITING SECTION ===
            Label lblWaitingTitle = new Label { Text = "⏳ Waiting to be Prepared", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekWarning, Location = new Point(30, 80), AutoSize = true };

            flpCashierQueue = new FlowLayoutPanel
            {
                Location = new Point(30, 110),
                Size = new Size(pnlContent.Width - 60, 250),
                AutoScroll = true,
                BackColor = DeepSeekBg,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // === PREPARING SECTION ===
            Label lblPreparingTitle = new Label { Text = "🔥 Currently Preparing", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = DeepSeekAccent, Location = new Point(30, 380), AutoSize = true };

            flpPreparingQueue = new FlowLayoutPanel
            {
                Location = new Point(30, 410),
                Size = new Size(pnlContent.Width - 60, 250),
                AutoScroll = true,
                BackColor = DeepSeekBg,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            pnlContent.Controls.AddRange(new Control[] { lblTitle, lblWaitingTitle, flpCashierQueue, lblPreparingTitle, flpPreparingQueue });

            // Handle Resize
            pnlContent.Resize += (s, e) => {
                flpCashierQueue.Width = pnlContent.Width - 60;
                flpPreparingQueue.Width = pnlContent.Width - 60;
            };

            // Load Backlog (Oldest First)
            // Load Backlog
            var todaysTransactions = dbService.GetAllTransactions()
                .Where(t => t.TransactionDate.Date == DateTime.Today
                         && t.Status == "Completed")
                .OrderBy(t => t.TransactionDate)
                .ToList();

            foreach (var tx in todaysTransactions)
            {
                var netOrder = new NetworkOrder
                {
                    TransactionNumber = tx.TransactionNumber,
                    Notes = tx.Notes ?? "",
                    Items = new List<NetworkOrderItem>()
                };

                // Group add-ons under their parent (Heuristic: Add-ons are consecutive items with no Size)
                foreach (var ti in tx.TransactionItems.OrderBy(t => t.Id))
                {
                    netOrder.Items.Add(new NetworkOrderItem
                    {
                        ProductName = ti.Product?.Name ?? "Unknown",
                        Size = ti.Size ?? "",
                        Quantity = ti.Quantity,
                        IsAddOn = string.IsNullOrEmpty(ti.Size)
                    });
                }

                AddCardToCashierQueue(netOrder);
            }
            UpdateQueueBadge();
        }

        private void HandleCashierNetworkMessage(NetworkOrder order)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (order.MessageType == "NEW_ORDER")
                {
                    if (flpCashierQueue != null && flpCashierQueue.Visible)
                    {
                        AddCardToCashierQueue(order);
                    }
                }
            });
        }

        private void AddCardToCashierQueue(NetworkOrder order)
        {
            string dailyNumber = "0";
            if (order.TransactionNumber.Length >= 4)
            {
                string suffix = order.TransactionNumber.Substring(order.TransactionNumber.Length - 4);
                if (int.TryParse(suffix, out int parsedNum)) dailyNumber = parsedNum.ToString();
            }

            // Dynamically size the card based on items + notes
            int itemCount = order.Items.Count;
            int cardHeight = 120 + (itemCount * 18);
            if (!string.IsNullOrEmpty(order.Notes)) cardHeight += 35; // Add space for notes

            Panel card = new Panel { Size = new Size(300, cardHeight), BackColor = DeepSeekSurface, Margin = new Padding(10), Tag = order.TransactionNumber };

            Label lblOrderNo = new Label { Text = $"#{dailyNumber}", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(15, 15), AutoSize = true };
            Label lblStatus = new Label { Name = "lblStatus", Text = "WAITING", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = DeepSeekWarning, Location = new Point(200, 18), AutoSize = true };

            int currentY = 45;

            // Display Notes
            if (!string.IsNullOrEmpty(order.Notes))
            {
                Label lblNotes = new Label { Text = $"📝 Note: {order.Notes}", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = DeepSeekWarning, Location = new Point(15, currentY), Size = new Size(270, 35), AutoEllipsis = true };
                card.Controls.Add(lblNotes);
                currentY += 35;
            }

            // Build itemized text list
            string itemsText = "";
            foreach (var item in order.Items)
            {
                // ▼▼▼ FIX: Check if the item has a size ▼▼▼
                bool isAddOn = string.IsNullOrEmpty(item.Size);

                if (isAddOn)
                {
                    itemsText += $"      + {item.Quantity}x {item.ProductName}\n";
                }
                else
                {
                    itemsText += $"• {item.Quantity}x {item.ProductName} {item.Size}\n";
                }
                // ▲▲▲ END OF FIX ▲▲▲
            }

            Label lblItems = new Label { Text = itemsText.TrimEnd(), Font = new Font("Segoe UI", 10), ForeColor = DeepSeekTextMuted, Location = new Point(15, currentY), Size = new Size(270, cardHeight - currentY - 20) };

            card.Controls.AddRange(new Control[] { lblOrderNo, lblStatus, lblItems });

            if (preparingTransactions.Contains(order.TransactionNumber))
            {
                lblStatus.Text = "PREPARING";
                lblStatus.ForeColor = DeepSeekAccent;
                card.BackColor = Color.FromArgb(30, 40, 55);
                flpPreparingQueue.Controls.Add(card);
            }
            else
            {
                flpCashierQueue.Controls.Add(card);
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

            // ▼▼▼ TOP 3 BEST SELLERS SECTION ▼▼▼
            Panel pnlTop3 = new Panel
            {
                Location = new Point(15, 245), // Just above the "Products" label (which was at 245)
                Size = new Size(1050, 80),
                BackColor = Color.Transparent
            };

            Label lblTop3Title = new Label
            {
                Text = "🏆 Top 3 Best Sellers",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = DeepSeekAccent,
                Location = new Point(0, 0),
                AutoSize = true
            };
            pnlTop3.Controls.Add(lblTop3Title);

            var top3 = GetTop3BestSellers();

            int cardX = 0;
            foreach (var (name, qty) in top3)
            {
                Panel card = new Panel
                {
                    Location = new Point(cardX, 25),
                    Size = new Size(240, 50),
                    BackColor = DeepSeekSurface
                };
                card.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(DeepSeekBorder, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                };

                Label lblName = new Label
                {
                    Text = name,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = DeepSeekText,
                    Location = new Point(10, 6),
                    Size = new Size(220, 20),
                    AutoEllipsis = true
                };

                Label lblQty = new Label
                {
                    Text = $"{qty} sold",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = DeepSeekSuccess,
                    Location = new Point(10, 28),
                    AutoSize = true
                };

                card.Controls.AddRange(new Control[] { lblName, lblQty });
                pnlTop3.Controls.Add(card);
                cardX += 255;
            }

            pnlLeft.Controls.Add(pnlTop3);
            // ▲▲▲ END OF TOP 3 ▲▲▲

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
                Location = new Point(15, 335), // Was 245
                AutoSize = true,
                BackColor = Color.Transparent
            };

            flpProducts = new FlowLayoutPanel
            {
                Location = new Point(15, 375), // Was 285
                Size = new Size(1050, 525),    // Reduce height slightly to fit (was 615)
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Let it fill the space
                Font = new Font("Segoe UI", 10),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                GridColor = DeepSeekBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowTemplate = { Height = 35 }
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

            // Define columns
            dgvCart.Columns.Add("Name", "Name");
            dgvCart.Columns.Add("Size", "Size");
            dgvCart.Columns.Add("Quantity", "Qty");
            dgvCart.Columns.Add("Price", "Price");
            dgvCart.Columns.Add("Total", "Total");

            // ▼▼▼ ADD MINUS AND PLUS BUTTON COLUMNS ▼▼▼
            DataGridViewButtonColumn btnMinusCol = new DataGridViewButtonColumn
            {
                Name = "MinusBtn",
                HeaderText = "",
                Text = "-",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };
            btnMinusCol.DefaultCellStyle.BackColor = DeepSeekDanger;
            btnMinusCol.DefaultCellStyle.ForeColor = DeepSeekText;
            btnMinusCol.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 50, 50);

            DataGridViewButtonColumn btnPlusCol = new DataGridViewButtonColumn
            {
                Name = "PlusBtn",
                HeaderText = "",
                Text = "+",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };
            btnPlusCol.DefaultCellStyle.BackColor = DeepSeekSuccess;
            btnPlusCol.DefaultCellStyle.ForeColor = DeepSeekText;
            btnPlusCol.DefaultCellStyle.SelectionBackColor = Color.FromArgb(10, 150, 100);

            dgvCart.Columns.Add(btnMinusCol);
            dgvCart.Columns.Add(btnPlusCol);
            // ▲▲▲ END OF ADDITION ▲▲▲

            // Set fill weights so all columns share the width proportionally
            dgvCart.Columns["Name"].FillWeight = 35;
            dgvCart.Columns["Size"].FillWeight = 10;
            dgvCart.Columns["Quantity"].FillWeight = 8;
            dgvCart.Columns["Price"].FillWeight = 15;
            dgvCart.Columns["Total"].FillWeight = 15;
            dgvCart.Columns["MinusBtn"].FillWeight = 6;
            dgvCart.Columns["PlusBtn"].FillWeight = 6;

            // Alignment
            dgvCart.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvCart.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvCart.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Disable sorting so clicks don't reorder
            foreach (DataGridViewColumn col in dgvCart.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvCart.SelectionChanged += DgvCart_SelectionChanged!;
            dgvCart.CellClick += DgvCart_CellClick!;

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
                Location = new Point(15, 715),
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



            // ▼▼▼ ADD ORDER NOTES INPUT ▼▼▼
            Label lblNotes = new Label
            {
                Text = "Order Notes:",
                Location = new Point(15, 775),
                AutoSize = true,
                ForeColor = DeepSeekTextMuted,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            TextBox txtOrderNotes = new TextBox
            {
                Location = new Point(115, 772),
                Size = new Size(480, 28),
                BackColor = DeepSeekSurface,
                ForeColor = DeepSeekText,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "e.g., Less ice, extra hot..."
            };
            txtOrderNotes.TextChanged += (s, e) => { currentOrderNotes = txtOrderNotes.Text; };

            pnlRight.Controls.Add(lblNotes);
            pnlRight.Controls.Add(txtOrderNotes);
            // ▲▲▲ END OF FIX ▲▲▲

            // Add lblNotes and txtOrderNotes to pnlRight.Controls.AddRange

            // DISCOUNT PANEL
            pnlDiscount = new Panel
            {
                Location = new Point(15, 660), // Moved up from 740
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
                pnlTotals,
                pnlDiscount,               // Added
                btnPrintBill, btnPayCash, btnPayGCash, btnPayHybrid,
                lblNotes, txtOrderNotes   
            });

            splitContainer.Panel1.Controls.Add(pnlLeft);
            splitContainer.Panel2.Controls.Add(pnlRight);

            pnlContent.Controls.Add(splitContainer);

            RefreshCart();
        }

        private List<(string Name, int Qty)> GetTop3BestSellers()
        {
            var result = new List<(string Name, int Qty)>();

            try
            {
                var transactions = dbService.GetAllTransactions()
                    .Where(t => t.Status == "Completed")
                    .ToList();

                if (!transactions.Any()) return result;

                var productSales = new Dictionary<int, (string Name, int Quantity)>();

                foreach (var transaction in transactions)
                {
                    if (transaction.TransactionItems == null) continue;

                    foreach (var item in transaction.TransactionItems)
                    {
                        string productName = item.Product?.Name ?? "Unknown";
                        if (productName == "Unknown" && item.ProductId > 0)
                        {
                            var product = dbService.GetProductById(item.ProductId);
                            productName = product?.Name ?? $"Product #{item.ProductId}";
                        }

                        if (productSales.ContainsKey(item.ProductId))
                        {
                            var existing = productSales[item.ProductId];
                            productSales[item.ProductId] = (productName, existing.Quantity + item.Quantity);
                        }
                        else
                        {
                            productSales[item.ProductId] = (productName, item.Quantity);
                        }
                    }
                }

                result = productSales.Values
                    .OrderByDescending(p => p.Quantity)
                    .Take(3)
                    .Select(p => (p.Name, p.Quantity))
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting top 3 best sellers: {ex.Message}");
            }

            return result;
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

                // ============================================================
                //  EXTRAS / ADD-ONS CATEGORY HANDLING
                // ============================================================
                if (product.Category?.Name == "Extras/Add-ons")
                {
                    Panel extraCard = new Panel
                    {
                        Size = new Size(210, 140),
                        BackColor = DeepSeekCard,
                        Margin = new Padding(10),
                        Tag = product
                    };
                    extraCard.Paint += (s, e) =>
                    {
                        using (Pen pen = new Pen(DeepSeekBorder, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, extraCard.Width - 1, extraCard.Height - 1);
                        using (Pen accentPen = new Pen(DeepSeekWarning, 2))
                            e.Graphics.DrawLine(accentPen, 0, 0, extraCard.Width, 0);
                    };

                    // ▼▼▼ RENAMED from lblName to lblExtraName ▼▼▼
                    Label lblExtraName = new Label
                    {
                        Text = product.Name,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        ForeColor = DeepSeekText,
                        Location = new Point(10, 15),
                        Size = new Size(190, 40),
                        AutoEllipsis = true
                    };

                    Label lblExtraPrice = new Label
                    {
                        Text = $"₱{product.Price:N2}",
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = DeepSeekSuccess,
                        Location = new Point(10, 55),
                        AutoSize = true
                    };

                    Label lblHint = new Label
                    {
                        Text = "Click an item in cart first, then add.",
                        Font = new Font("Segoe UI", 8, FontStyle.Italic),
                        ForeColor = DeepSeekTextMuted,
                        Location = new Point(10, 80),
                        Size = new Size(190, 20)
                    };

                    Button btnAddExtra = new Button
                    {
                        Text = "+ Add to Selected",
                        Size = new Size(190, 30),
                        Location = new Point(10, 105),
                        BackColor = DeepSeekWarning,
                        ForeColor = Color.Black,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        FlatAppearance = { BorderSize = 0 },
                        Cursor = Cursors.Hand
                    };

                    btnAddExtra.Click += (s, ev) =>
                    {
                        if (dgvCart.CurrentRow == null || cartItems.Count == 0)
                        {
                            MessageBox.Show("Please click on an item in the cart first, then add the extra.", "Select Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        int index = dgvCart.CurrentRow.Index;
                        if (index >= 0 && index < cartItems.Count)
                        {
                            var cartItem = cartItems[index];

                            var existingExtra = cartItem.AddOns.FirstOrDefault(a => a.ProductId == product.Id);
                            if (existingExtra != null)
                            {
                                existingExtra.Quantity++;
                            }
                            else
                            {
                                cartItem.AddOns.Add(new CartItem
                                {
                                    ProductId = product.Id,
                                    Name = product.Name,
                                    Quantity = 1,
                                    Price = product.Price
                                });
                            }

                            RefreshCart();
                        }
                    };

                    // Use the renamed variables here
                    extraCard.Controls.AddRange(new Control[] { lblExtraName, lblExtraPrice, lblHint, btnAddExtra });
                    flpProducts.Controls.Add(extraCard);
                    continue; // Skip the normal card layout for extras
                }

                // ============================================================
                //  NORMAL PRODUCTS
                // ============================================================

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
                decimal total = item.TotalPrice;
                displaySubtotal += total;

                // 1. Add the Main Item
                int rowIndex = dgvCart.Rows.Add(
                    item.Name,
                    item.Size ?? "-",
                    item.Quantity,
                    $"₱{item.Price:N2}",
                    $"₱{total:N2}",
                    "-",
                    "+"
                );

                // ▼▼▼ CRITICAL: Store the cart item so clicks know what to update ▼▼▼
                dgvCart.Rows[rowIndex].Tag = item;
                dgvCart.Rows[rowIndex].Cells["Name"].Tag = "MAIN"; // marker for main item
                // ▲▲▲ END OF FIX ▲▲▲

                if (!string.IsNullOrEmpty(item.Size))
                {
                    dgvCart.Rows[rowIndex].Cells["Size"].Style.ForeColor = DeepSeekAccent;
                }

                // 2. Add the Add-ons
                foreach (var addon in item.AddOns)
                {
                    string addonName = $"      + {addon.Name}";

                    int addonIndex = dgvCart.Rows.Add(
                        addonName,
                        "",
                        addon.Quantity,
                        "",
                        "",
                        "-",
                        "+"
                    );

                    // ▼▼▼ CRITICAL: Store add-on and its parent so clicks work ▼▼▼
                    dgvCart.Rows[addonIndex].Tag = (addon, item); // tuple: (addon, parent)
                    dgvCart.Rows[addonIndex].Cells["Name"].Tag = "ADDON";
                    // ▲▲▲ END OF FIX ▲▲▲

                    dgvCart.Rows[addonIndex].DefaultCellStyle.ForeColor = DeepSeekTextMuted;
                    dgvCart.Rows[addonIndex].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                    dgvCart.Rows[addonIndex].DefaultCellStyle.BackColor = Color.FromArgb(28, 30, 34);
                    dgvCart.Rows[addonIndex].Cells["Name"].Style.ForeColor = DeepSeekWarning;
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

        private void DgvCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Only respond to the +/- button columns
            string columnName = dgvCart.Columns[e.ColumnIndex].Name;
            if (columnName != "PlusBtn" && columnName != "MinusBtn") return;

            var row = dgvCart.Rows[e.RowIndex];
            if (row.Tag == null) return;

            // What kind of row is this?
            string rowType = row.Cells["Name"].Tag as string ?? "";
            bool isAddOn = rowType == "ADDON";
            bool isMainItem = rowType == "MAIN";

            // ============ ADD-ON CLICK ============
            if (isAddOn)
            {
                // Tag is a tuple (addon, parent)
                var tag = ((CartItem addon, CartItem parent))row.Tag;

                if (columnName == "PlusBtn")
                {
                    tag.addon.Quantity++;
                }
                else if (columnName == "MinusBtn")
                {
                    tag.addon.Quantity--;
                    if (tag.addon.Quantity <= 0)
                    {
                        tag.parent.AddOns.Remove(tag.addon);
                    }
                }
            }
            // ============ MAIN ITEM CLICK ============
            else if (isMainItem)
            {
                var item = row.Tag as CartItem;
                if (item == null) return;

                var product = dbService.GetProductById(item.ProductId);
                if (product == null) return;

                if (columnName == "PlusBtn")
                {
                    // Check stock before adding
                    int oldQty = item.Quantity;
                    item.Quantity = 0;
                    bool ok = CheckProductAvailability(product, oldQty + 1, item.Size, out string? msg);
                    item.Quantity = oldQty;

                    if (ok)
                    {
                        item.Quantity++;
                    }
                    else
                    {
                        MessageBox.Show(msg, "Insufficient Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (columnName == "MinusBtn")
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                    {
                        cartItems.Remove(item);
                    }
                }
            }
            else
            {
                return;
            }

            // Re-render cart
            RefreshCart();

            // Try to restore selection to a nearby row
            if (e.RowIndex < dgvCart.Rows.Count)
            {
                dgvCart.ClearSelection();
                dgvCart.Rows[e.RowIndex].Selected = true;
            }
        }

        private void UpdateTotals()
        {
            if (lblSubTotal == null || lblTax == null || lblDiscount == null || lblTotal == null) return;

            // 1. The cart sum (including Add-ons) is the TOTAL (VAT Inclusive)
            decimal total = cartItems.Sum(item => item.TotalPrice) - currentDiscountAmount;

            // 2. Tax is exactly 12% of the Total
            decimal taxRate = 0.12m;
            decimal tax = total * taxRate;

            // 3. Subtotal is the Total minus the Tax
            decimal subTotal = total - tax;

            lblSubTotal.Text = $"₱{subTotal:N2}";
            lblTax.Text = $"₱{tax:N2}";
            lblDiscount.Text = $"₱{currentDiscountAmount:N2}";
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
                
                decimal discountAmount = currentDiscountAmount;

                // 1. Cart sum is the TOTAL
                decimal grossTotal = cartItems.Sum(item => item.TotalPrice);
                decimal total = grossTotal - currentDiscountAmount;

                // 2. Tax is 12% of the Total
                decimal taxRate = 0.12m;
                decimal tax = total * taxRate;

                // 3. Subtotal is Total minus Tax
                decimal subTotal = total - tax;

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
                            GcashReferenceNumber = refNum,
                            Notes = currentOrderNotes // <-- ADD THIS
                        };

                        var items = new List<(int productId, int quantity, decimal unitPrice, string? size)>();

                        foreach (var item in cartItems)
                        {
                            // 1. Add the main product (default to "Regular" if no size)
                            string mainSize = string.IsNullOrEmpty(item.Size) ? "Regular" : item.Size;
                            items.Add((item.ProductId, item.Quantity, item.Price, mainSize));

                            // 2. Add each extra as a separate line item (no size, so it's identified as add-on)
                            foreach (var addon in item.AddOns)
                            {
                                items.Add((addon.ProductId, addon.Quantity, addon.Price, null));
                            }
                        }

                        var savedTransaction = dbService.CreateTransaction(transaction, items);

                        // Send the order to the Barista via TCP
                        var networkOrder = new NetworkOrder
                        {
                            TransactionNumber = savedTransaction.TransactionNumber,
                            MessageType = "NEW_ORDER",
                            Notes = savedTransaction.Notes ?? "", // <-- Get from DB
                            Items = new List<NetworkOrderItem>()
                        };

                        // Group add-ons under their parent products
                        foreach (var ti in savedTransaction.TransactionItems.OrderBy(t => t.Id))
                        {
                            networkOrder.Items.Add(new NetworkOrderItem
                            {
                                ProductName = ti.Product?.Name ?? "Unknown",
                                Size = ti.Size ?? "",
                                Quantity = ti.Quantity,
                                IsAddOn = string.IsNullOrEmpty(ti.Size) // Add-ons have no size
                            });
                        }
                        _tcpServer.BroadcastOrder(networkOrder);
                        // Update the Cashier's own UI if they have the Queue tab open
                        HandleCashierNetworkMessage(networkOrder);

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

                        // Show the preview form instead of asking directly
                        using (var preview = new ReceiptPreviewForm(savedTransaction, userName, GeneratePDFReceipt))
                        {
                            preview.ShowDialog();
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
                decimal subTotal = cartItems.Sum(item => item.TotalPrice); 
                decimal discountAmount = currentDiscountAmount;
                decimal discountedSubTotal = subTotal - discountAmount;
                decimal tax = discountedSubTotal * 0.12m;
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

                        var items = new List<(int productId, int quantity, decimal unitPrice, string? size)>();

                        foreach (var item in cartItems)
                        {
                            // 1. Add the main product
                            items.Add((item.ProductId, item.Quantity, item.Price, item.Size));

                            // 2. Add each extra as a separate line item
                            foreach (var addon in item.AddOns)
                            {
                                items.Add((addon.ProductId, addon.Quantity, addon.Price, null));
                            }
                        }

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

                        // Show the preview form instead of asking directly
                        using (var preview = new ReceiptPreviewForm(savedTransaction, userName, GeneratePDFReceipt))
                        {
                            preview.ShowDialog();
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

                // Define fonts
                XFont fontOrderNo = new XFont("Arial", 16, XFontStyleEx.Bold);
                XFont fontDate = new XFont("Arial", 8, XFontStyleEx.Regular);
                XFont fontTypeHeader = new XFont("Arial", 12, XFontStyleEx.Bold);
                XFont fontItemLarge = new XFont("Arial", 12, XFontStyleEx.Bold);
                XFont fontStoreHeader = new XFont("Arial", 14, XFontStyleEx.Bold);
                XFont fontNormal = new XFont("Arial", 9, XFontStyleEx.Regular);
                XFont fontBold = new XFont("Arial", 10, XFontStyleEx.Bold);
                XFont fontTotal = new XFont("Arial", 14, XFontStyleEx.Bold);
                XFont fontSmall = new XFont("Arial", 8, XFontStyleEx.Regular);
                XFont fontThanks = new XFont("Arial", 9, XFontStyleEx.Bold);

                // ------------------------------------------------
                // STEP 1: CALCULATE REQUIRED HEIGHT
                // ------------------------------------------------
                double neededHeight = 50; // Start padding
                neededHeight += 25; // Order Number
                neededHeight += 20; // Date + Owner
                neededHeight += 15; // Separator
                neededHeight += 20; // Dine In header

                // Large summary items (exclude add-ons)
                var largeItems = items.Where(i => !(i.Product?.Name?.Contains("+") == true) &&
                                                 !(string.IsNullOrEmpty(i.Size) && i.UnitPrice < 50)).ToList();
                neededHeight += largeItems.Count * 18;
                neededHeight += 20; // Padding after summary
                neededHeight += 15; // Separator
                neededHeight += 20; // Logo margin

                // Logo (Fixed size)
                neededHeight += 70; // assume 70x70 max
                neededHeight += 10; // Spacing

                neededHeight += 18; // Store Header
                neededHeight += 24; // Address lines
                neededHeight += 20; // Spacing
                neededHeight += 36; // Transaction details (Order, Employee, POS)
                neededHeight += 20; // Spacing
                neededHeight += 15; // Separator
                neededHeight += 20; // Dine In header

                // Detailed items
                neededHeight += items.Count * 30; // Name + Price + Qty line

                neededHeight += 20; // Spacing
                neededHeight += 15; // Separator
                neededHeight += 25; // Total
                neededHeight += 15; // Cash
                neededHeight += 15; // Change
                neededHeight += 25; // Spacing
                neededHeight += 15; // Separator
                neededHeight += 12 * 4; // Footer lines
                neededHeight += 10;
                neededHeight += 12; // Acknowledge
                neededHeight += 12; // Thank you
                neededHeight += 40; // Bottom padding

                // Create Page with calculated height
                PdfPage page = document.AddPage();
                page.Width = 226;
                page.Height = neededHeight;

                // ------------------------------------------------
                // STEP 2: DRAW ON THE PAGE
                // ------------------------------------------------
                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                {
                    XPen penDotted = new XPen(XColors.Black, 1) { DashStyle = XDashStyle.Dot };
                    XBrush brushBlack = new XSolidBrush(XColors.Black);
                    XBrush brushGray = new XSolidBrush(XColors.DarkGray);

                    double yPos = 50;
                    double centerX = page.Width / 2;
                    double leftMargin = 15;
                    double rightCol = page.Width - 15;

                    // 1. Order Number
                    string orderNo = $"#{transaction.TransactionNumber.Split('-').Last()}";
                    double orderWidth = gfx.MeasureString(orderNo, fontOrderNo).Width;
                    gfx.DrawString(orderNo, fontOrderNo, brushBlack, new XPoint(centerX - orderWidth / 2, yPos));
                    yPos += 25;

                    // 2. Date & Cashier
                    gfx.DrawString($"{transaction.TransactionDate:MM/dd/yy h:mm tt}", fontDate, brushBlack, new XPoint(leftMargin, yPos));
                    yPos += 12;
                    gfx.DrawString($"Owner, Ten Degrees POS", fontDate, brushBlack, new XPoint(leftMargin, yPos));
                    yPos += 20;

                    gfx.DrawLine(penDotted, leftMargin, yPos, rightCol, yPos);
                    yPos += 15;

                    // 3. Dine In
                    string dineInText = "Dine In";
                    gfx.DrawString(dineInText, fontTypeHeader, brushBlack, new XPoint(centerX - gfx.MeasureString(dineInText, fontTypeHeader).Width / 2, yPos));
                    yPos += 20;

                    // 4. Large Summary Items
                    foreach (var item in largeItems)
                    {
                        string displayName = item.Product?.Name ?? "Unknown";
                        gfx.DrawString($"{item.Quantity} x {displayName}", fontItemLarge, brushBlack, new XPoint(leftMargin, yPos));
                        yPos += 18;
                    }

                    yPos += 10;
                    gfx.DrawLine(penDotted, leftMargin, yPos, rightCol, yPos);
                    yPos += 20;

                    // 5. Logo
                    try
                    {
                        string[] possiblePaths = {
                            Path.Combine(Application.StartupPath, "Resources", "logo.png"),
                            Path.Combine(Application.StartupPath, "Resources", "logo.jpg"),
                            Path.Combine(Application.StartupPath, "logo.png"),
                            Path.Combine(Application.StartupPath, "logo.jpg")
                        };

                        string? logoPath = possiblePaths.FirstOrDefault(File.Exists);
                        if (logoPath != null)
                        {
                            XImage logo = XImage.FromFile(logoPath);
                            double logoWidth = 70;
                            double logoHeight = (logo.PixelHeight * logoWidth) / logo.PixelWidth;

                            gfx.DrawImage(logo, centerX - (logoWidth / 2), yPos, logoWidth, logoHeight);
                            yPos += logoHeight + 10;
                        }
                        else
                        {
                            yPos += 70; // If no logo, skip the space
                        }
                    }
                    catch
                    {
                        yPos += 70;
                    }

                    // 6. Store Details
                    gfx.DrawString("TEN DEGREES", fontStoreHeader, brushBlack, new XPoint(centerX - gfx.MeasureString("TEN DEGREES", fontStoreHeader).Width / 2, yPos));
                    yPos += 18;

                    string addrLine1 = "Cotabato Street, Brgy. Batasan Hills,";
                    string addrLine2 = "Quezon City, Metro Manila, 1121";
                    gfx.DrawString(addrLine1, fontNormal, brushBlack, new XPoint(centerX - gfx.MeasureString(addrLine1, fontNormal).Width / 2, yPos));
                    yPos += 12;
                    gfx.DrawString(addrLine2, fontNormal, brushBlack, new XPoint(centerX - gfx.MeasureString(addrLine2, fontNormal).Width / 2, yPos));
                    yPos += 20;

                    // 7. Transaction Details
                    gfx.DrawString($"Order: {transaction.TransactionNumber.Split('-').Last()}", fontNormal, brushBlack, new XPoint(leftMargin, yPos)); yPos += 12;
                    gfx.DrawString($"Employee: Owner", fontNormal, brushBlack, new XPoint(leftMargin, yPos)); yPos += 12;
                    gfx.DrawString($"POS: Ten Degrees POS", fontNormal, brushBlack, new XPoint(leftMargin, yPos)); yPos += 20;

                    gfx.DrawLine(penDotted, leftMargin, yPos, rightCol, yPos);
                    yPos += 15;

                    // 8. Dine In (Again)
                    gfx.DrawString("Dine In", fontBold, brushBlack, new XPoint(leftMargin, yPos));
                    yPos += 20;

                    // 9. Detailed Items
                    foreach (var item in items)
                    {
                        string itemName = item.Product?.Name ?? "Unknown";
                        string sizeInfo = string.IsNullOrEmpty(item.Size) ? "" : $" ({item.Size})";

                        bool isAddOn = itemName.StartsWith("+") || (string.IsNullOrEmpty(item.Size) && item.UnitPrice < 50);
                        if (isAddOn) itemName = $"   + {itemName}";

                        // Name
                        gfx.DrawString(itemName, fontNormal, brushBlack, new XPoint(leftMargin, yPos));

                        // Price
                        string priceStr = $"₱{item.TotalPrice:N2}";
                        double priceWidth = gfx.MeasureString(priceStr, fontNormal).Width;
                        gfx.DrawString(priceStr, fontNormal, brushBlack, new XPoint(rightCol - priceWidth, yPos));

                        yPos += 14;

                        // Qty x Price
                        gfx.DrawString($"{item.Quantity} x ₱{item.UnitPrice:N2}", fontSmall, brushGray, new XPoint(leftMargin, yPos));
                        yPos += 16;
                    }

                    yPos += 5;
                    gfx.DrawLine(penDotted, leftMargin, yPos, rightCol, yPos);
                    yPos += 15;

                    // 10. Totals
                    gfx.DrawString("Total", fontTotal, brushBlack, new XPoint(leftMargin, yPos));
                    string totalStr = $"₱{transaction.Total:N2}";
                    double totalWidth = gfx.MeasureString(totalStr, fontTotal).Width;
                    gfx.DrawString(totalStr, fontTotal, brushBlack, new XPoint(rightCol - totalWidth, yPos));
                    yPos += 25;

                    gfx.DrawString("Cash", fontBold, brushBlack, new XPoint(leftMargin, yPos));
                    string cashStr = $"₱{transaction.AmountPaid:N2}";
                    double cashWidth = gfx.MeasureString(cashStr, fontBold).Width;
                    gfx.DrawString(cashStr, fontBold, brushBlack, new XPoint(rightCol - cashWidth, yPos));
                    yPos += 15;

                    gfx.DrawString("Change", fontBold, brushBlack, new XPoint(leftMargin, yPos));
                    string changeStr = $"₱{transaction.Change:N2}";
                    double changeWidth = gfx.MeasureString(changeStr, fontBold).Width;
                    gfx.DrawString(changeStr, fontBold, brushBlack, new XPoint(rightCol - changeWidth, yPos));
                    yPos += 25;

                    gfx.DrawLine(penDotted, leftMargin, yPos, rightCol, yPos);
                    yPos += 15;

                    // 11. Footer
                    gfx.DrawString("B", fontSmall, brushBlack, new XPoint(leftMargin, yPos));

                    string[] footerLines = {
                        "Follow us like. Tag us.",
                        "FB: Ten Degreedcafe",
                        "IG @tendegreescafe",
                        "TikTok tendegreescafe"
                    };

                    double footerTextX = leftMargin + 20;
                    foreach (var line in footerLines)
                    {
                        gfx.DrawString(line, fontSmall, brushBlack, new XPoint(footerTextX, yPos));
                        yPos += 12;
                    }

                    yPos += 10;
                    string ack = "Acknowledge Receipt Only";
                    double ackWidth = gfx.MeasureString(ack, fontSmall).Width;
                    gfx.DrawString(ack, fontSmall, brushGray, new XPoint(centerX - ackWidth / 2, yPos));
                    yPos += 12;

                    string thanks = "Thank you!";
                    double thanksWidth = gfx.MeasureString(thanks, fontThanks).Width;
                    gfx.DrawString(thanks, fontThanks, brushBlack, new XPoint(centerX - thanksWidth / 2, yPos));
                }

                // Save logic
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
                // ▼▼▼ FIX: Use TotalPrice to include add-ons ▼▼▼
                SubTotal = cartItems.Sum(item => item.TotalPrice),
                Tax = cartItems.Sum(item => item.TotalPrice) * 0.12m,
                Total = cartItems.Sum(item => item.TotalPrice)
                // ▲▲▲ END OF FIX ▲▲▲
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

            // ▼▼▼ ADDED: Request Cancellation Button ▼▼▼
            Button btnRequestCancel = new Button
            {
                Text = "🚫 Request Cancellation",
                Location = new Point(580, 77),
                Size = new Size(180, 35),
                BackColor = DeepSeekDanger,
                ForeColor = DeepSeekText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };

            btnRequestCancel.Click += (s, e) =>
            {
                if (dgvTransactions?.CurrentRow == null)
                {
                    MessageBox.Show("Please select a transaction first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string status = dgvTransactions.CurrentRow.Cells["Status"].Value?.ToString() ?? "";
                if (status != "Completed" && status != "Fulfilled")
                {
                    MessageBox.Show("Only 'Completed' or 'Fulfilled' orders can be requested for cancellation.", "Invalid Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the Transaction Number (since the ID column isn't visible, we look it up)
                string txNumber = dgvTransactions.CurrentRow.Cells["Id"].Value?.ToString() ?? "";
                var transaction = dbService.GetTransactionByNumber(txNumber);

                if (transaction == null) return;

                using (Form prompt = new Form())
                {
                    prompt.Text = "Request Cancellation";
                    prompt.Size = new Size(400, 250);
                    prompt.StartPosition = FormStartPosition.CenterParent;
                    prompt.BackColor = DeepSeekSurface;
                    prompt.FormBorderStyle = FormBorderStyle.FixedDialog;

                    Label lblMsg = new Label { Text = "Reason for cancellation:", Location = new Point(20, 20), AutoSize = true, ForeColor = DeepSeekText };
                    ComboBox cmbReason = new ComboBox { Location = new Point(20, 50), Size = new Size(340, 35), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = DeepSeekBg, ForeColor = DeepSeekText };
                    cmbReason.Items.AddRange(new object[] { "Customer requested cancellation", "Order processing error", "Item out of stock", "Payment issue", "Duplicate order", "Other" });
                    cmbReason.SelectedIndex = 0;

                    Button btnSubmit = new Button { Text = "Submit Request", Location = new Point(20, 100), Size = new Size(150, 40), BackColor = DeepSeekDanger, ForeColor = DeepSeekText, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, DialogResult = DialogResult.OK };

                    prompt.Controls.AddRange(new Control[] { lblMsg, cmbReason, btnSubmit });

                    if (prompt.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            dbService.RequestCancellation(transaction.Id, userId, cmbReason.SelectedItem.ToString()!);
                            MessageBox.Show("Cancellation request submitted. Waiting for Admin approval.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadTransactions(); // Refresh grid
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };
            // ▲▲▲ END OF ADDED BUTTON ▲▲▲

            dgvTransactions = CreateDataGridView(new Point(30, 130), new Size(1700, 700));
            dgvTransactions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            dgvTransactions.Columns.Add("Id", "Transaction #");
            dgvTransactions.Columns.Add("QueueNum", "Queue #");
            dgvTransactions.Columns.Add("Date", "Date");
            dgvTransactions.Columns.Add("Items", "Items");
            dgvTransactions.Columns.Add("SubTotal", "Sub Total");
            dgvTransactions.Columns.Add("Tax", "Tax");
            dgvTransactions.Columns.Add("Total", "Total");
            dgvTransactions.Columns.Add("Payment", "Payment");
            dgvTransactions.Columns.Add("PaymentDetail", "Payment Detail");
            dgvTransactions.Columns.Add("Status", "Status");

            // Right-align total amount column
            dgvTransactions.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            LoadTransactions();

            // ▼▼▼ FIXED: Ensure btnRequestCancel is added to the screen ▼▼▼
            pnlContent.Controls.AddRange(new Control[] {
                lblTitle,
                lblFrom, dtpFrom,
                lblTo, dtpTo,
                btnFilter,
                btnRequestCancel, // Added to the UI
                dgvTransactions
            });
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
            var transactions = dbService.GetTransactionsByCashier(userId).OrderBy(t => t.TransactionDate).ToList();

            // Group by date so we can calculate the daily queue number
            var groupedByDate = transactions.GroupBy(t => t.TransactionDate.Date);

            foreach (var group in groupedByDate)
            {
                int dailyQueueCounter = 1;
                foreach (var t in group.OrderBy(x => x.TransactionDate))
                {
                    int itemCount = t.TransactionItems?.Count ?? 0;
                    int rowIndex = dgvTransactions.Rows.Add(
                        t.TransactionNumber,
                        $"#{dailyQueueCounter}", // ▼▼▼ Queue Number ▼▼▼
                        t.TransactionDate.ToString("yyyy-MM-dd HH:mm"),
                        itemCount,
                        $"₱{t.SubTotal:N2}",
                        $"₱{t.Tax:N2}",
                        $"₱{t.Total:N2}",
                        t.PaymentMethod,
                        GetPaymentDetail(t),
                        t.Status
                    );

                    // Apply coloring
                    if (t.Status == "Completed")
                        dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
                    else if (t.Status == "Refunded")
                        dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekWarning;
                    else if (t.Status == "Cancelled")
                        dgvTransactions.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;

                    dailyQueueCounter++;
                }
            }

            // If you want newest at the top (like the original view), reverse the rows
            dgvTransactions.Sort(dgvTransactions.Columns["Date"], System.ComponentModel.ListSortDirection.Descending);
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            if (dtpFrom == null || dtpTo == null || dgvTransactions == null) return;

            dgvTransactions.Rows.Clear();
            var allTransactions = dbService.GetTransactionsByCashier(userId);
            var filtered = allTransactions.Where(t =>
                t.TransactionDate.Date >= dtpFrom.Value.Date &&
                t.TransactionDate.Date <= dtpTo.Value.Date).OrderBy(t => t.TransactionDate).ToList();

            // Group by date so we can calculate the daily queue number
            var groupedByDate = filtered.GroupBy(t => t.TransactionDate.Date);

            foreach (var group in groupedByDate)
            {
                int dailyQueueCounter = 1;
                foreach (var t in group.OrderBy(x => x.TransactionDate))
                {
                    int itemCount = t.TransactionItems?.Count ?? 0;
                    int rowIndex = dgvTransactions.Rows.Add(
                        t.TransactionNumber,
                        $"#{dailyQueueCounter}",
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

                    dailyQueueCounter++;
                }
            }

            dgvTransactions.Sort(dgvTransactions.Columns["Date"], System.ComponentModel.ListSortDirection.Descending);
        }

        // ============================================================
        //  INVENTORY
        // ============================================================

        private void ShowInventory()
        {
            if (pnlContent == null) return;
            pnlContent.Controls.Clear();

            Label lblTitle = new Label { Text = "Inventory", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = DeepSeekText, Location = new Point(30, 20), AutoSize = true };

            TabControl tcInventory = new TabControl
            {
                Location = new Point(30, 80),
                Size = new Size(pnlContent.Width - 60, pnlContent.Height - 120),
                Font = new Font("Segoe UI", 11),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            TabPage tpIngredients = new TabPage("Ingredients");
            TabPage tpMaterials = new TabPage("Materials");
            tpIngredients.BackColor = DeepSeekBg;
            tpMaterials.BackColor = DeepSeekBg;

            DataGridView dgvIng = CreateDataGridView(new Point(10, 10), new Size(tcInventory.Width - 30, tcInventory.Height - 40));
            DataGridView dgvMat = CreateDataGridView(new Point(10, 10), new Size(tcInventory.Width - 30, tcInventory.Height - 40));

            dgvIng.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            foreach (var grid in new[] { dgvIng, dgvMat })
            {
                grid.Columns.Add("Item", "Item");
                grid.Columns.Add("Quantity", "Quantity");
                grid.Columns.Add("Unit", "Unit");
                grid.Columns.Add("MinStock", "Min Stock");
                grid.Columns.Add("Status", "Status");
            }

            // Load Data
            var inventory = dbService.GetAllInventory();
            foreach (var item in inventory)
            {
                if (item.Ingredient == null) continue; // Only show Ingredients, not finished Products

                var grid = item.Ingredient.ItemType == "Material" ? dgvMat : dgvIng;

                string status = item.Quantity <= 0 ? "Out of Stock" :
                               item.Quantity < item.Ingredient.MinimumStock ? "Low Stock" : "In Stock";

                int rowIndex = grid.Rows.Add(
                    item.Ingredient.Name,
                    $"{item.Quantity:F1}",
                    item.Unit,
                    item.Ingredient.MinimumStock,
                    status
                );

                if (item.Quantity <= 0) grid.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekDanger;
                else if (item.Quantity < item.Ingredient.MinimumStock) grid.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekWarning;
                else grid.Rows[rowIndex].Cells["Status"].Style.ForeColor = DeepSeekSuccess;
            }

            tpIngredients.Controls.Add(dgvIng);
            tpMaterials.Controls.Add(dgvMat);

            tcInventory.TabPages.Add(tpIngredients);
            tcInventory.TabPages.Add(tpMaterials);

            pnlContent.Controls.AddRange(new Control[] { lblTitle, tcInventory });
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

            // ▼▼▼ NEW: Store Add-ons ▼▼▼
            public List<CartItem> AddOns { get; set; } = new List<CartItem>();
            // ▲▲▲ END OF NEW ▲▲▲

            public decimal TotalPrice => (Price * Quantity) + (AddOns.Sum(a => a.Price * a.Quantity));
        }
    }
}