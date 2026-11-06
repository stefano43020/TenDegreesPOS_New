using Microsoft.EntityFrameworkCore;
using TenDegreesPOS_New.Models;
using TenDegreesPOS_New.Data;

namespace TenDegreesPOS_New.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly POSDbContext _context;

        public DatabaseService()
        {
            _context = new POSDbContext();
            _context.Database.EnsureCreated();

            EnsureDatabaseSchemaUpdated();

            if (!_context.Users.Any())
            {
                CreateDefaultUsers();
            }

            // Ensure categories exist
            if (!_context.Categories.Any())
            {
                CreateDefaultCategories();
            }
        }

        #region Database Schema Updates

        /// <summary>
        /// Ensures the database schema matches the current model
        /// Adds missing columns if they don't exist
        /// </summary>
        public void EnsureDatabaseSchemaUpdated()
        {
            // Helper to safely add columns without crashing if they already exist
            void TryAddColumn(string table, string column, string type)
            {
                try
                {
                    _context.Database.ExecuteSqlRaw($"ALTER TABLE {table} ADD COLUMN {column} {type};");
                    Console.WriteLine($"Added {column} to {table}");
                }
                catch (Exception e)
                {
                    // Ignore errors (mostly "duplicate column name")
                    Console.WriteLine($"{table}.{column} skipped: {e.Message}");
                }
            }
            try
            {
                _context.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS InventoryBatches (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        InventoryId INTEGER NOT NULL,
                        Quantity DECIMAL NOT NULL,
                        ExpirationDate TEXT NOT NULL,
                        ReceivedDate TEXT NOT NULL,
                        FOREIGN KEY(InventoryId) REFERENCES Inventory(Id) ON DELETE CASCADE
                    );");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Create InventoryBatches skipped: {e.Message}");
            }
            // ---- Disposal Records ----
            try
            {
                _context.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS DisposalRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        IngredientId INTEGER NOT NULL,
                        IngredientName TEXT NOT NULL,
                        Quantity DECIMAL NOT NULL,
                        Unit TEXT NOT NULL,
                        ExpirationDate TEXT NOT NULL,
                        DisposalDate TEXT NOT NULL,
                        DisposedByUserId INTEGER NOT NULL,
                        Reason TEXT,
                        FOREIGN KEY(IngredientId) REFERENCES Ingredients(Id) ON DELETE RESTRICT,
                        FOREIGN KEY(DisposedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT
                    );");
            }
            catch (Exception e) { Console.WriteLine($"Create DisposalRecords skipped: {e.Message}"); }

            // ---- Products ----
            TryAddColumn("Products", "HasSizes", "INTEGER DEFAULT 0");
            TryAddColumn("Products", "PriceSmall", "DECIMAL");
            TryAddColumn("Products", "PriceMedium", "DECIMAL");
            TryAddColumn("Products", "PriceLarge", "DECIMAL");
            TryAddColumn("Products", "ImagePath", "TEXT");

            // ---- Inventory Tracking ----
            TryAddColumn("Inventory", "ExpirationDate", "TEXT");
            TryAddColumn("Inventory", "DamagedQuantity", "DECIMAL DEFAULT 0");

            // ---- Ingredients (Item Type) ----
            TryAddColumn("Ingredients", "ItemType", "TEXT DEFAULT 'Ingredient'");

            // ---- Order Cancellations ----
            TryAddColumn("OrderCancellations", "RequestedDate", "TEXT");
            TryAddColumn("OrderCancellations", "RequestedByUserId", "INTEGER");
            TryAddColumn("OrderCancellations", "ApprovedDate", "TEXT");
            TryAddColumn("OrderCancellations", "ApprovedByUserId", "INTEGER");
            TryAddColumn("OrderCancellations", "Status", "TEXT DEFAULT 'Pending'");

            // ---- TransactionItems ----
            TryAddColumn("TransactionItems", "Size", "TEXT");

            // ---- Transactions ----
            TryAddColumn("Transactions", "CashPortion", "DECIMAL DEFAULT 0");
            TryAddColumn("Transactions", "GcashPortion", "DECIMAL DEFAULT 0");
            TryAddColumn("Transactions", "GcashReferenceNumber", "TEXT");

            // ---- Ingredients (Reorder settings) ----
            TryAddColumn("Ingredients", "ManualReorderQuantity", "DECIMAL");
            TryAddColumn("Ingredients", "ReorderPoint", "DECIMAL DEFAULT 10");

            // ---- PackagingVariants table ----
            try
            {
                _context.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS PackagingVariants (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        IngredientId INTEGER NOT NULL,
                        VariantName TEXT NOT NULL,
                        Quantity DECIMAL NOT NULL,
                        Unit TEXT NOT NULL,
                        Cost DECIMAL NOT NULL DEFAULT 0,
                        FOREIGN KEY(IngredientId) REFERENCES Ingredients(Id) ON DELETE CASCADE
                    );");
            }
            catch (Exception e) { Console.WriteLine($"Create PackagingVariants skipped: {e.Message}"); }

            // Add this line to handle existing databases
            TryAddColumn("PackagingVariants", "Cost", "DECIMAL DEFAULT 0");
        }

        #endregion

        #region Supplier Management

        public void LogDisposal(int inventoryId, decimal quantity, int userId, string reason)
        {
            using var dbTransaction = _context.Database.BeginTransaction();
            try
            {
                var inventory = _context.Inventory
                    .Include(i => i.Ingredient)
                    .FirstOrDefault(i => i.Id == inventoryId);

                if (inventory == null) throw new Exception("Inventory item not found");
                if (quantity <= 0) throw new Exception("Quantity must be greater than zero");
                if (quantity > inventory.Quantity) throw new Exception("Cannot dispose more than current stock");

                // Find the batch to deduct from (FIFO: Oldest first)
                var batch = _context.InventoryBatches
                    .Where(b => b.InventoryId == inventoryId && b.Quantity > 0)
                    .OrderBy(b => b.ExpirationDate)
                    .FirstOrDefault();

                // 1. Deduct from Inventory
                inventory.Quantity -= quantity;
                inventory.LastUpdated = DateTime.Now;

                // 2. Deduct from the Batch if one exists
                if (batch != null)
                {
                    batch.Quantity -= quantity;
                    if (batch.Quantity < 0) batch.Quantity = 0;
                }

                // 3. Create Disposal Record
                var record = new DisposalRecord
                {
                    IngredientId = inventory.IngredientId ?? 0,
                    IngredientName = inventory.Ingredient?.Name ?? "Unknown",
                    Quantity = quantity,
                    Unit = inventory.Unit,
                    // If the batch has an expiration date, log it; otherwise default
                    ExpirationDate = batch?.ExpirationDate ?? DateTime.Today,
                    DisposalDate = DateTime.Now,
                    DisposedByUserId = userId,
                    Reason = reason
                };

                _context.DisposalRecords.Add(record);
                _context.SaveChanges();
                dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw new Exception($"Error logging disposal: {ex.Message}", ex);
            }
        }

        public int GetPendingOrderCount()
        {
            try
            {
                return _context.Transactions
                    .Count(t => t.Status == "Completed" && t.TransactionDate.Date == DateTime.Today);
            }
            catch { return 0; }
        }

        public List<DisposalRecord> GetAllDisposalRecords()
        {
            return _context.DisposalRecords
                .Include(dr => dr.DisposedByUser)
                .OrderByDescending(dr => dr.DisposalDate)
                .ToList();
        }
        public List<Supplier> GetAllSuppliers()
        {
            try
            {
                return _context.Suppliers.OrderBy(s => s.Name).ToList();
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                if (ex.Message.Contains("no such table"))
                {
                    return new List<Supplier>();
                }
                throw new Exception($"Error getting suppliers: {ex.Message}", ex);
            }
            catch (Exception ex) { throw new Exception($"Error getting suppliers: {ex.Message}", ex); }
        }

        public Supplier? GetSupplierById(int id)
        {
            try { return _context.Suppliers.Find(id); }
            catch (Exception ex) { throw new Exception($"Error getting supplier by ID: {ex.Message}", ex); }
        }

        public void AddSupplier(Supplier supplier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(supplier.Name)) throw new ArgumentException("Supplier name is required");
                supplier.CreatedAt = DateTime.Now;
                supplier.IsActive = true;
                _context.Suppliers.Add(supplier);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error adding supplier: {ex.Message}", ex); }
        }

        public void UpdateSupplier(Supplier supplier)
        {
            try
            {
                supplier.UpdatedAt = DateTime.Now;
                _context.Suppliers.Update(supplier);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error updating supplier: {ex.Message}", ex); }
        }

        public void DeleteSupplier(int id)
        {
            try
            {
                var supplier = _context.Suppliers.Find(id);
                if (supplier != null)
                {
                    supplier.IsActive = false;
                    supplier.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error deleting supplier: {ex.Message}", ex); }
        }

        public void ToggleSupplierStatus(int id)
        {
            try
            {
                var supplier = _context.Suppliers.Find(id);
                if (supplier != null)
                {
                    supplier.IsActive = !supplier.IsActive;
                    supplier.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error toggling supplier status: {ex.Message}", ex); }
        }

        #endregion

        #region Order Cancellation

        public List<Transaction> GetAllCompletedTransactions()
        {
            try
            {
                return _context.Transactions
                    .Include(t => t.Cashier)
                    .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                    .Where(t => t.Status == "Completed")
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting completed transactions: {ex.Message}", ex); }
        }

        public List<OrderCancellation> GetAllCancellations()
        {
            try
            {
                return _context.OrderCancellations
                    .Include(oc => oc.Transaction)
                    .Include(oc => oc.RequestedByUser)
                    .Include(oc => oc.ApprovedByUser)
                    .OrderByDescending(oc => oc.RequestedDate)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting cancellations: {ex.Message}", ex); }
        }

        public List<OrderCancellation> GetCancellationsByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _context.OrderCancellations
                    .Include(oc => oc.Transaction)
                    .Include(oc => oc.RequestedByUser)
                    .Include(oc => oc.ApprovedByUser)
                    .Where(oc => (oc.ApprovedDate ?? oc.RequestedDate) >= startDate && (oc.ApprovedDate ?? oc.RequestedDate) <= endDate)
                    .OrderByDescending(oc => oc.ApprovedDate ?? oc.RequestedDate)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting cancellations by date: {ex.Message}", ex); }
        }

        public void RequestCancellation(int transactionId, int userId, string reason)
        {
            try
            {
                var transaction = _context.Transactions.Find(transactionId);
                if (transaction == null) throw new Exception("Transaction not found");
                if (transaction.Status != "Completed" && transaction.Status != "Fulfilled")
                    throw new Exception("Only completed or fulfilled transactions can be cancelled");

                var existingRequest = _context.OrderCancellations.FirstOrDefault(oc => oc.TransactionId == transactionId && oc.Status == "Pending");
                if (existingRequest != null) throw new Exception("A cancellation request is already pending for this order.");

                var cancellation = new OrderCancellation
                {
                    TransactionId = transactionId,
                    TransactionNumber = transaction.TransactionNumber,
                    RequestedDate = DateTime.Now,
                    RequestedByUserId = userId,
                    Status = "Pending",
                    Reason = reason,
                    OriginalTotal = transaction.Total
                };

                _context.OrderCancellations.Add(cancellation);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // ▼▼▼ UNWRAP THE INNER EXCEPTION ▼▼▼
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += "\n\nDETAILS: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMsg += "\n\nDEEPER DETAILS: " + ex.InnerException.InnerException.Message;
                    }
                }
                throw new Exception(errorMsg, ex);
                // ▲▲▲ END OF UNWRAP ▲▲▲
            }
        }

        public void ApproveCancellation(int cancellationId, int adminUserId)
        {
            using var dbTransaction = _context.Database.BeginTransaction();
            try
            {
                var cancellation = _context.OrderCancellations
                    .Include(oc => oc.Transaction)
                    .FirstOrDefault(oc => oc.Id == cancellationId);

                if (cancellation == null) throw new Exception("Cancellation request not found");
                if (cancellation.Status != "Pending") throw new Exception("This request has already been processed");

                cancellation.Status = "Approved";
                cancellation.ApprovedDate = DateTime.Now;
                cancellation.ApprovedByUserId = adminUserId;

                if (cancellation.Transaction != null)
                {
                    cancellation.Transaction.Status = "Cancelled";
                }

                _context.SaveChanges();
                dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw new Exception($"Error approving cancellation: {ex.Message}", ex);
            }
        }

        public List<OrderCancellation> GetPendingCancellations()
        {
            return _context.OrderCancellations
                .Include(oc => oc.Transaction)
                .Include(oc => oc.RequestedByUser)
                .Where(oc => oc.Status == "Pending")
                .OrderByDescending(oc => oc.RequestedDate)
                .ToList();
        }

        public OrderCancellation? GetCancellationById(int id)
        {
            try
            {
                return _context.OrderCancellations
                    .Include(oc => oc.Transaction)
                    .ThenInclude(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                    .Include(oc => oc.RequestedByUser)
                    .Include(oc => oc.ApprovedByUser)
                    .FirstOrDefault(oc => oc.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting cancellation: {ex.Message}", ex);
            }
        }

        public void RestoreCancelledTransaction(int cancellationId)
        {
            using var dbTransaction = _context.Database.BeginTransaction();
            try
            {
                var cancellation = _context.OrderCancellations
                    .Include(oc => oc.Transaction)
                    .FirstOrDefault(oc => oc.Id == cancellationId);

                if (cancellation == null) throw new Exception("Cancellation record not found");
                if (cancellation.Transaction?.Status != "Cancelled") throw new Exception("Only cancelled transactions can be restored");

                var cancelledItems = _context.CancelledOrderItems
                    .Where(coi => coi.CancellationId == cancellationId)
                    .ToList();

                foreach (var item in cancelledItems)
                {
                    var productIngredients = _context.ProductIngredients
                        .Where(pi => pi.ProductId == item.ProductId)
                        .Include(pi => pi.Ingredient)
                        .ToList();

                    foreach (var pi in productIngredients)
                    {
                        if (pi.Ingredient != null)
                        {
                            var ingredientInventory = _context.Inventory
                                .FirstOrDefault(i => i.IngredientId == pi.Ingredient.Id);

                            if (ingredientInventory != null)
                            {
                                ingredientInventory.Quantity -= pi.Quantity * item.Quantity;
                                ingredientInventory.LastUpdated = DateTime.Now;
                            }
                        }
                    }
                }

                if (cancellation.Transaction != null)
                {
                    cancellation.Transaction.Status = "Completed";
                }

                _context.SaveChanges();
                dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw new Exception($"Error restoring transaction: {ex.Message}", ex);
            }
        }

        #endregion

        #region Supplier Ingredient Assignment

        public List<int> GetSupplierIngredients(int supplierId)
        {
            try
            {
                return _context.SupplierIngredients
                    .Where(si => si.SupplierId == supplierId)
                    .Select(si => si.IngredientId)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting supplier ingredients: {ex.Message}", ex); }
        }

        public void SaveSupplierIngredients(int supplierId, List<int> ingredientIds)
        {
            try
            {
                var existing = _context.SupplierIngredients.Where(si => si.SupplierId == supplierId);
                _context.SupplierIngredients.RemoveRange(existing);

                foreach (var ingredientId in ingredientIds)
                {
                    _context.SupplierIngredients.Add(new SupplierIngredient
                    {
                        SupplierId = supplierId,
                        IngredientId = ingredientId
                    });
                }
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error saving supplier ingredients: {ex.Message}", ex); }
        }

        public Dictionary<int, List<int>> GetAllSupplierIngredients()
        {
            try
            {
                return _context.SupplierIngredients
                    .GroupBy(si => si.SupplierId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(si => si.IngredientId).ToList()
                    );
            }
            catch (Exception ex) { throw new Exception($"Error getting all supplier ingredients: {ex.Message}", ex); }
        }

        #endregion

        #region Purchase Order Management

        public List<PurchaseOrder> GetAllPurchaseOrders()
        {
            try
            {
                return _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.PurchaseOrderItems)
                    .OrderByDescending(po => po.OrderDate)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting purchase orders: {ex.Message}", ex); }
        }

        public PurchaseOrder? GetPurchaseOrderById(int id)
        {
            try
            {
                return _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Ingredient)
                    .FirstOrDefault(po => po.Id == id);
            }
            catch (Exception ex) { throw new Exception($"Error getting purchase order by ID: {ex.Message}", ex); }
        }

        public string GeneratePurchaseOrderNumber()
        {
            try
            {
                string prefix = "PO";
                string datePart = DateTime.Now.ToString("yyyyMMdd");
                int count = _context.PurchaseOrders.Count(po => po.OrderDate.Date == DateTime.Now.Date) + 1;
                return $"{prefix}{datePart}{count:D4}";
            }
            catch (Exception ex) { throw new Exception($"Error generating purchase order number: {ex.Message}", ex); }
        }

        public PurchaseOrder CreatePurchaseOrder(PurchaseOrder order, List<(int ingredientId, string ingredientName, decimal boxes, decimal quantity, string unit, string boxDisplayName, decimal boxQuantityPerUnit, decimal unitPrice)> items)
        {
            using var dbTransaction = _context.Database.BeginTransaction();
            try
            {
                order.OrderNumber = GeneratePurchaseOrderNumber();
                order.OrderDate = DateTime.Now;
                order.Status = "Pending";

                decimal totalAmount = 0;
                var orderItems = new List<PurchaseOrderItem>();

                foreach (var item in items)
                {
                    var ingredient = _context.Ingredients.Find(item.ingredientId);
                    string ingredientUnit = ingredient?.Unit ?? "g";
                    decimal boxQuantity = item.boxQuantityPerUnit;
                    string boxUnit = item.unit;

                    // Calculate how many base units (e.g., grams) are in ONE pack
                    decimal quantityPerBoxInBaseUnit = boxQuantity;
                    if (boxUnit.ToLower() != ingredientUnit.ToLower())
                    {
                        try
                        {
                            quantityPerBoxInBaseUnit = UnitConverter.ConvertToBase(boxQuantity, boxUnit, ingredientUnit);
                        }
                        catch
                        {
                            if (boxUnit.ToLower() == "kg" && ingredientUnit.ToLower() == "g") quantityPerBoxInBaseUnit = boxQuantity * 1000m;
                            if (boxUnit.ToLower() == "l" && ingredientUnit.ToLower() == "ml") quantityPerBoxInBaseUnit = boxQuantity * 1000m;
                        }
                    }

                    // NOTE: item.boxes now represents the NUMBER OF PACKS (e.g., 2 packs)
                    // We create a separate PO item for each pack type so they show up individually
                    var orderItem = new PurchaseOrderItem
                    {
                        IngredientId = item.ingredientId,
                        IngredientName = item.ingredientName,
                        BoxesOrdered = item.boxes,           // e.g., 2
                        BoxesReceived = 0,
                        QuantityOrdered = item.boxes * quantityPerBoxInBaseUnit, // e.g., 2 * 500 = 1000g
                        QuantityReceived = 0,
                        BoxQuantityPerUnit = boxQuantity,    // e.g., 500
                        BoxUnit = boxUnit,                   // e.g., "g"
                        UnitPrice = item.unitPrice,
                        TotalPrice = item.boxes * item.unitPrice
                    };

                    totalAmount += orderItem.TotalPrice;
                    orderItems.Add(orderItem);
                }

                order.TotalAmount = totalAmount;

                _context.PurchaseOrders.Add(order);
                _context.SaveChanges();

                foreach (var item in orderItems)
                {
                    item.PurchaseOrderId = order.Id;
                    _context.PurchaseOrderItems.Add(item);
                }

                _context.SaveChanges();
                dbTransaction.Commit();
                return order;
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw new Exception($"Error creating purchase order: {ex.Message}", ex);
            }
        }

        public List<InventoryBatch> GetAllInventoryBatches()
        {
            return _context.InventoryBatches
                .Include(ib => ib.Inventory)
                .ThenInclude(i => i.Ingredient)
                .OrderBy(ib => ib.ExpirationDate)
                .ToList();
        }
        public void ReceivePurchaseOrderWithBatches(int orderId, Dictionary<int, (decimal Qty, DateTime ExpDate)> receivedData)
        {
            using var dbTransaction = _context.Database.BeginTransaction();
            try
            {
                var order = _context.PurchaseOrders
                    .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Ingredient)
                    .FirstOrDefault(po => po.Id == orderId);

                if (order == null) throw new Exception("Purchase order not found");
                if (order.Status == "Received") throw new Exception("Order has already been fully received");

                bool allFullyReceived = true;

                foreach (var item in order.PurchaseOrderItems)
                {
                    if (receivedData.ContainsKey(item.Id))
                    {
                        var data = receivedData[item.Id];
                        decimal boxesToReceive = data.Qty;
                        DateTime expirationDate = data.ExpDate;

                        decimal newReceivedBoxes = item.BoxesReceived + boxesToReceive;
                        if (newReceivedBoxes > item.BoxesOrdered + 0.01m)
                            throw new Exception($"Cannot receive more than ordered for {item.IngredientName}");

                        item.BoxesReceived = newReceivedBoxes;

                        // Unit conversion logic
                        decimal boxQuantity = item.BoxQuantityPerUnit ?? 1;
                        string boxUnit = item.BoxUnit ?? "kg";
                        string ingredientUnit = item.Ingredient?.Unit ?? "g";
                        decimal quantityPerBoxInIngredientUnit = boxQuantity;

                        if (boxUnit.ToLower() != ingredientUnit.ToLower())
                        {
                            if (boxUnit.ToLower() == "l" && ingredientUnit.ToLower() == "ml") quantityPerBoxInIngredientUnit = boxQuantity * 1000m;
                            else if (boxUnit.ToLower() == "ml" && ingredientUnit.ToLower() == "l") quantityPerBoxInIngredientUnit = boxQuantity / 1000m;
                            else if (boxUnit.ToLower() == "kg" && ingredientUnit.ToLower() == "g") quantityPerBoxInIngredientUnit = boxQuantity * 1000m;
                            else if (boxUnit.ToLower() == "g" && ingredientUnit.ToLower() == "kg") quantityPerBoxInIngredientUnit = boxQuantity / 1000m;
                        }

                        decimal quantityReceived = boxesToReceive * quantityPerBoxInIngredientUnit;
                        item.QuantityReceived += quantityReceived;

                        // Get or create the parent Inventory record
                        var inventory = _context.Inventory.FirstOrDefault(i => i.IngredientId == item.IngredientId);
                        if (inventory == null)
                        {
                            inventory = new Inventory
                            {
                                IngredientId = item.IngredientId,
                                Quantity = 0,
                                Unit = ingredientUnit,
                                LastUpdated = DateTime.Now
                            };
                            _context.Inventory.Add(inventory);
                            _context.SaveChanges(); // Need the ID for the batch
                        }

                        // ▼▼▼ FIX: Only add to sellable stock if it hasn't expired yet ▼▼▼
                        if (expirationDate.Date >= DateTime.Today)
                        {
                            inventory.Quantity += quantityReceived;
                        }
                        else
                        {
                            // If it's already expired upon arrival, log it to the batch but don't add to sellable inventory.
                            // It will sit in the "Expired" monitoring tab.
                            System.Diagnostics.Debug.WriteLine($"Received expired item: {item.IngredientName}. Quantity not added to sellable stock.");
                        }

                        inventory.LastUpdated = DateTime.Now;

                        // Create the new Batch record
                        var batch = new InventoryBatch
                        {
                            InventoryId = inventory.Id,
                            Quantity = quantityReceived,
                            ExpirationDate = expirationDate,
                            ReceivedDate = DateTime.Now
                        };
                        _context.InventoryBatches.Add(batch);

                        if (item.BoxesReceived < item.BoxesOrdered - 0.01m)
                            allFullyReceived = false;
                    }
                }

                order.Status = allFullyReceived ? "Received" : "Partially Received";
                _context.SaveChanges();
                dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw new Exception($"Error receiving purchase order: {ex.Message}", ex);
            }
        }

        public void ProcessExpiredBatches()
        {
            try
            {
                DateTime today = DateTime.Today;

                // Find batches that are expired and are still counted in the active inventory
                // (We can use a boolean flag in the model, or check if the parent inventory Qty > 0)
                var expiredBatches = _context.InventoryBatches
                    .Include(ib => ib.Inventory)
                    .Where(ib => ib.ExpirationDate.Date < today)
                    .ToList();

                bool changesMade = false;

                foreach (var batch in expiredBatches)
                {
                    if (batch.Inventory != null && batch.Quantity > 0)
                    {
                        // Deduct from sellable stock
                        batch.Inventory.Quantity -= batch.Quantity;

                        // Ensure it doesn't drop below 0
                        if (batch.Inventory.Quantity < 0) batch.Inventory.Quantity = 0;

                        batch.Inventory.LastUpdated = DateTime.Now;

                        // Log that we processed this batch (so we don't deduct it again next time)
                        // Since we don't have a flag, we'll just set the batch quantity to 0
                        // and rely on the Monitoring module to show the historical data if needed.
                        // For simplicity, we will leave the batch quantity, but we know it's expired.

                        // To prevent infinite deduction, we need to mark the batch as "deducted".
                        // Best way without changing schema: Delete the batch or set its Quantity to 0.
                        // Let's set the quantity to 0 so it doesn't get deducted again.
                        // (The Monitoring tab will still show it based on the ExpirationDate).
                        batch.Quantity = 0;

                        changesMade = true;
                    }
                }

                if (changesMade)
                {
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing expired batches: {ex.Message}");
            }
        }
        public void LogDamagedItem(int inventoryId, decimal damagedQty)
        {
            try
            {
                var inventory = _context.Inventory.Find(inventoryId);
                if (inventory != null)
                {
                    // Ensure we don't subtract more than we have
                    decimal actualDamage = Math.Min(inventory.Quantity, damagedQty);

                    inventory.Quantity -= actualDamage;       // Remove from sellable stock
                    inventory.DamagedQuantity += actualDamage; // Add to damaged log
                    inventory.LastUpdated = DateTime.Now;

                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error logging damaged item: {ex.Message}", ex); }
        }
        public void UpdateTransactionStatus(int transactionId, string newStatus)
        {
            try
            {
                var transaction = _context.Transactions.Find(transactionId);
                if (transaction != null)
                {
                    transaction.Status = newStatus;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating transaction status: {ex.Message}", ex);
            }
        }

        public void UpdatePurchaseOrderStatus(int orderId, string status)
        {
            try
            {
                var order = _context.PurchaseOrders.Find(orderId);
                if (order != null)
                {
                    order.Status = status;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error updating purchase order status: {ex.Message}", ex); }
        }

        #endregion

        #region Authentication

        public User? AuthenticateUser(string username, string password)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username && u.IsActive);

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    user.LastLogin = DateTime.Now;
                    _context.SaveChanges();
                    return user;
                }

                return null;
            }
            catch (Exception ex) { throw new Exception($"Authentication error: {ex.Message}", ex); }
        }

        private void CreateDefaultUsers()
        {
            try
            {
                var users = new List<User>
                {
                    new User { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"), Role = "Admin", FullName = "System Administrator", IsActive = true, CreatedAt = DateTime.Now },
                    new User { Username = "manager", PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager"), Role = "Manager", FullName = "Store Manager", IsActive = true, CreatedAt = DateTime.Now },
                    new User { Username = "cashier", PasswordHash = BCrypt.Net.BCrypt.HashPassword("cashier"), Role = "Cashier", FullName = "Cashier User", IsActive = true, CreatedAt = DateTime.Now }
                };

                _context.Users.AddRange(users);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error creating default users: {ex.Message}", ex); }
        }

        private void CreateDefaultCategories()
        {
            try
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Drinks", Description = "Beverages and drinks" },
                    new Category { Name = "Foods", Description = "Food items" },
                    new Category { Name = "Raw Materials", Description = "Ingredients and supplies" },
                    new Category { Name = "Snacks", Description = "Snack items" },
                    new Category { Name = "Desserts", Description = "Dessert items" }
                };

                _context.Categories.AddRange(categories);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error creating default categories: {ex.Message}", ex); }
        }

        #endregion

        #region User Management

        public List<User> GetAllUsers()
        {
            try { return _context.Users.ToList(); }
            catch (Exception ex) { throw new Exception($"Error getting users: {ex.Message}", ex); }
        }

        public User? GetUserById(int id)
        {
            try { return _context.Users.Find(id); }
            catch (Exception ex) { throw new Exception($"Error getting user by ID: {ex.Message}", ex); }
        }

        public User? GetUserByUsername(string username)
        {
            try { return _context.Users.FirstOrDefault(u => u.Username == username); }
            catch (Exception ex) { throw new Exception($"Error getting user by username: {ex.Message}", ex); }
        }

        #endregion

        #region Product Management

        public List<Product> GetAllProducts()
        {
            try
            {
                return _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductIngredients)
                    .ThenInclude(pi => pi.Ingredient)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting all products: {ex.Message}", ex); }
        }

        public Product? GetProductById(int id)
        {
            try
            {
                return _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductIngredients)
                    .ThenInclude(pi => pi.Ingredient)
                    .FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex) { throw new Exception($"Error getting product by ID: {ex.Message}", ex); }
        }

        public Product? GetProductByBarcode(string barcode)
        {
            try
            {
                return _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Barcode == barcode);
            }
            catch (Exception ex) { throw new Exception($"Error getting product by barcode: {ex.Message}", ex); }
        }

        public List<Product> GetProductsByCategory(int categoryId)
        {
            try
            {
                return _context.Products
                    .Where(p => p.CategoryId == categoryId && p.IsActive)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting products by category: {ex.Message}", ex); }
        }

        public void AddProduct(Product product)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(product.Name)) throw new ArgumentException("Product name is required");
                if (product.Price <= 0) throw new ArgumentException("Product price must be greater than 0");

                if (product.CategoryId <= 0)
                {
                    var defaultCategory = _context.Categories.FirstOrDefault();
                    if (defaultCategory != null) product.CategoryId = defaultCategory.Id;
                    else throw new Exception("No categories available. Please create a category first.");
                }
                else
                {
                    var categoryExists = _context.Categories.Any(c => c.Id == product.CategoryId);
                    if (!categoryExists) throw new Exception($"Category with ID {product.CategoryId} does not exist");
                }

                product.CreatedAt = DateTime.Now;
                product.IsActive = true;

                _context.Products.Add(product);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"Database error adding product: {innerMessage}", ex);
            }
            catch (Exception ex) { throw new Exception($"Error adding product: {ex.Message}", ex); }
        }

        public void UpdateProduct(Product product)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(product.Name)) throw new ArgumentException("Product name is required");
                if (product.Price <= 0) throw new ArgumentException("Product price must be greater than 0");

                if (product.CategoryId > 0)
                {
                    var categoryExists = _context.Categories.Any(c => c.Id == product.CategoryId);
                    if (!categoryExists) throw new Exception($"Category with ID {product.CategoryId} does not exist");
                }

                product.UpdatedAt = DateTime.Now;

                var tracked = _context.Products.Local.FirstOrDefault(p => p.Id == product.Id);
                if (tracked != null) _context.Entry(tracked).State = EntityState.Detached;

                _context.Products.Update(product);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"Database error updating product: {innerMessage}", ex);
            }
            catch (Exception ex) { throw new Exception($"Error updating product: {ex.Message}", ex); }
        }

        public void DeleteProduct(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product != null)
                {
                    product.IsActive = false;
                    product.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error deleting product: {ex.Message}", ex); }
        }

        public void HardDeleteProduct(int id)
        {
            try
            {
                var product = _context.Products
                    .Include(p => p.ProductIngredients)
                    .FirstOrDefault(p => p.Id == id);

                if (product != null)
                {
                    bool hasTransactions = _context.TransactionItems.Any(ti => ti.ProductId == id);
                    if (hasTransactions) throw new Exception("Cannot delete product that has transaction history. Use soft delete instead.");

                    if (product.ProductIngredients.Any())
                    {
                        _context.ProductIngredients.RemoveRange(product.ProductIngredients);
                    }

                    _context.Products.Remove(product);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error hard deleting product: {ex.Message}", ex); }
        }

        public bool ProductHasTransactions(int productId)
        {
            try { return _context.TransactionItems.Any(ti => ti.ProductId == productId); }
            catch (Exception ex) { throw new Exception($"Error checking product transactions: {ex.Message}", ex); }
        }

        public int SaveProductIngredients(int productId, List<ProductIngredient> ingredients)
        {
            try
            {
                var existingIngredients = _context.ProductIngredients
                    .Where(pi => pi.ProductId == productId)
                    .ToList();
                _context.ProductIngredients.RemoveRange(existingIngredients);
                _context.SaveChanges();

                int savedCount = 0;

                foreach (var ingredient in ingredients)
                {
                    var newIngredient = new ProductIngredient
                    {
                        ProductId = productId,
                        IngredientId = ingredient.IngredientId,
                        Quantity = ingredient.Quantity
                    };
                    _context.ProductIngredients.Add(newIngredient);
                    savedCount++;
                }

                _context.SaveChanges();
                return savedCount;
            }
            catch (Exception ex) { throw new Exception($"Error saving product ingredients: {ex.Message}", ex); }
        }

        public List<Category> GetAllCategories()
        {
            try { return _context.Categories.ToList(); }
            catch (Exception ex) { throw new Exception($"Error getting categories: {ex.Message}", ex); }
        }

        public Category? GetCategoryById(int id)
        {
            try { return _context.Categories.Find(id); }
            catch (Exception ex) { throw new Exception($"Error getting category by ID: {ex.Message}", ex); }
        }

        public Category? GetCategoryByName(string name)
        {
            try { return _context.Categories.FirstOrDefault(c => c.Name == name); }
            catch (Exception ex) { throw new Exception($"Error getting category by name: {ex.Message}", ex); }
        }

        public void AddCategory(Category category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.Name)) throw new ArgumentException("Category name is required");
                if (_context.Categories.Any(c => c.Name == category.Name)) throw new Exception($"Category '{category.Name}' already exists");
                _context.Categories.Add(category);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error adding category: {ex.Message}", ex); }
        }

        public void UpdateCategory(Category category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.Name)) throw new ArgumentException("Category name is required");
                _context.Categories.Update(category);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error updating category: {ex.Message}", ex); }
        }

        public void DeleteCategory(int id)
        {
            try
            {
                var category = _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefault(c => c.Id == id);

                if (category != null)
                {
                    if (category.Products.Any(p => p.IsActive)) throw new Exception("Cannot delete category that has active products");
                    _context.Categories.Remove(category);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error deleting category: {ex.Message}", ex); }
        }

        #endregion

        #region Transaction Management

        public List<Transaction> GetTransactionsByCashier(int cashierId)
        {
            try
            {
                return _context.Transactions
                    .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                    .Where(t => t.UserId == cashierId)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting transactions by cashier: {ex.Message}", ex); }
        }

        public List<Transaction> GetAllTransactions()
        {
            try
            {
                return _context.Transactions
                    .Include(t => t.Cashier)
                    .Include(t => t.Customer)
                    .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting all transactions: {ex.Message}", ex); }
        }

        public Transaction? GetTransactionById(int id)
        {
            try
            {
                return _context.Transactions
                    .Include(t => t.Cashier)
                    .Include(t => t.Customer)
                    .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                    .FirstOrDefault(t => t.Id == id);
            }
            catch (Exception ex) { throw new Exception($"Error getting transaction by ID: {ex.Message}", ex); }
        }

        public Transaction? GetTransactionByNumber(string number)
        {
            try
            {
                return _context.Transactions
                    .Include(t => t.Cashier)
                    .Include(t => t.Customer)
                    .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                    .FirstOrDefault(t => t.TransactionNumber == number);
            }
            catch (Exception ex) { throw new Exception($"Error getting transaction by number: {ex.Message}", ex); }
        }

        public List<Transaction> GetTransactionsByDate(DateTime date)
        {
            try
            {
                return _context.Transactions
                    .Where(t => t.TransactionDate.Date == date.Date)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting transactions by date: {ex.Message}", ex); }
        }

        public Transaction CreateTransaction(Transaction transaction, List<(int productId, int quantity, decimal unitPrice, string? size)> items)
        {
            using var dbTransaction = _context.Database.BeginTransaction();
            try
            {
                transaction.TransactionNumber = GenerateTransactionNumber();
                if (transaction.TransactionDate == default(DateTime))
                    transaction.TransactionDate = DateTime.Now;

                var transactionItems = new List<TransactionItem>();

                foreach (var item in items)
                {
                    var product = _context.Products.Find(item.productId);
                    if (product == null) throw new Exception($"Product {item.productId} not found");

                    decimal totalPrice = item.unitPrice * item.quantity;

                    var transactionItem = new TransactionItem
                    {
                        ProductId = item.productId,
                        Quantity = item.quantity,
                        UnitPrice = item.unitPrice,
                        TotalPrice = totalPrice,
                        Size = item.size
                    };
                    transactionItems.Add(transactionItem);

                    var productIngredients = _context.ProductIngredients
                        .Where(pi => pi.ProductId == item.productId)
                        .Include(pi => pi.Ingredient)
                        .ToList();

                    foreach (var pi in productIngredients)
                    {
                        if (pi.Ingredient != null)
                        {
                            decimal quantityPerUnit = pi.Quantity;
                            if (product.HasSizes && !string.IsNullOrEmpty(item.size))
                            {
                                switch (item.size)
                                {
                                    case "Small": quantityPerUnit = pi.QuantitySmall ?? pi.Quantity; break;
                                    case "Medium": quantityPerUnit = pi.QuantityMedium ?? pi.Quantity; break;
                                    case "Large": quantityPerUnit = pi.QuantityLarge ?? pi.Quantity; break;
                                }
                            }

                            decimal totalQuantity = quantityPerUnit * item.quantity;

                            var ingredientInventory = _context.Inventory
                                .FirstOrDefault(i => i.IngredientId == pi.Ingredient.Id);

                            if (ingredientInventory != null)
                            {
                                ingredientInventory.Quantity -= totalQuantity;
                                ingredientInventory.LastUpdated = DateTime.Now;
                            }
                        }
                    }
                }

                _context.Transactions.Add(transaction);
                _context.SaveChanges();

                foreach (var ti in transactionItems)
                {
                    ti.TransactionId = transaction.Id;
                    _context.TransactionItems.Add(ti);
                }

                _context.SaveChanges();
                dbTransaction.Commit();
                return transaction;
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw new Exception($"Error creating transaction: {ex.Message}", ex);
            }
        }

        public string GenerateTransactionNumber()
        {
            try
            {
                // We still use the INV prefix internally for the database to keep unique IDs across all time
                string prefix = "INV";
                string datePart = DateTime.Now.ToString("yyyyMMdd");

                // Find how many transactions were made TODAY
                int countToday = _context.Transactions
                    .Count(t => t.TransactionDate.Date == DateTime.Now.Date);

                // The new daily number is today's count + 1
                int dailyNumber = countToday + 1;

                // We append the daily number to the end so it's sequential for the day
                return $"{prefix}{datePart}{dailyNumber:D4}";
            }
            catch (Exception ex) { throw new Exception($"Error generating transaction number: {ex.Message}", ex); }
        }

        public void VoidTransaction(int transactionId, int userId)
        {
            try
            {
                var transaction = _context.Transactions.Find(transactionId);
                if (transaction != null)
                {
                    transaction.Status = "Voided";
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error voiding transaction: {ex.Message}", ex); }
        }

        public decimal GetDailySales(DateTime date)
        {
            try
            {
                var transactions = _context.Transactions
                    .Where(t => t.TransactionDate.Date == date.Date && t.Status == "Completed")
                    .ToList();

                return transactions.Sum(t => t.Total);
            }
            catch (Exception ex) { throw new Exception($"Error getting daily sales: {ex.Message}", ex); }
        }

        public decimal GetMonthlySales(int year, int month)
        {
            try
            {
                var transactions = _context.Transactions
                    .Where(t => t.TransactionDate.Year == year &&
                               t.TransactionDate.Month == month &&
                               t.Status == "Completed")
                    .ToList();

                return transactions.Sum(t => t.Total);
            }
            catch (Exception ex) { throw new Exception($"Error getting monthly sales: {ex.Message}", ex); }
        }

        #endregion

        #region Ingredient Management

        public List<Ingredient> GetAllIngredients()
        {
            try { return _context.Ingredients.ToList(); }
            catch (Exception ex) { throw new Exception($"Error getting ingredients: {ex.Message}", ex); }
        }

        public Ingredient? GetIngredientById(int id)
        {
            try { return _context.Ingredients.Find(id); }
            catch (Exception ex) { throw new Exception($"Error getting ingredient by ID: {ex.Message}", ex); }
        }

        public Ingredient? GetIngredientByName(string name)
        {
            try { return _context.Ingredients.FirstOrDefault(i => i.Name == name); }
            catch (Exception ex) { throw new Exception($"Error getting ingredient by name: {ex.Message}", ex); }
        }

        public void AddIngredient(Ingredient ingredient)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ingredient.Name)) throw new ArgumentException("Ingredient name is required");

                ingredient.IsActive = true;
                _context.Ingredients.Add(ingredient);
                _context.SaveChanges();

                var inventory = new Inventory
                {
                    IngredientId = ingredient.Id,
                    Quantity = 0,
                    Unit = ingredient.Unit ?? "g",
                    LastUpdated = DateTime.Now
                };
                _context.Inventory.Add(inventory);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error adding ingredient: {ex.Message}", ex); }
        }

        public void UpdateIngredient(Ingredient ingredient)
        {
            try
            {
                ingredient.UpdatedAt = DateTime.Now;
                _context.Ingredients.Update(ingredient);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error updating ingredient: {ex.Message}", ex); }
        }

        public decimal GetCurrentIngredientStock(int ingredientId)
        {
            try
            {
                var inventory = _context.Inventory.FirstOrDefault(i => i.IngredientId == ingredientId);
                return inventory?.Quantity ?? 0;
            }
            catch (Exception ex) { throw new Exception($"Error getting ingredient stock: {ex.Message}", ex); }
        }

        public void DeleteIngredient(int id)
        {
            try
            {
                var ingredient = _context.Ingredients
                    .Include(i => i.ProductIngredients)
                    .FirstOrDefault(i => i.Id == id);

                if (ingredient != null)
                {
                    if (ingredient.ProductIngredients.Any()) throw new Exception("Cannot delete ingredient that is used in products");

                    var inventory = _context.Inventory.FirstOrDefault(i => i.IngredientId == id);
                    if (inventory != null) _context.Inventory.Remove(inventory);

                    _context.Ingredients.Remove(ingredient);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error deleting ingredient: {ex.Message}", ex); }
        }

        #endregion

        #region Staff Management

        public List<User> GetAllStaff()
        {
            try
            {
                return _context.Users
                    .Where(u => u.Role != "Admin")
                    .OrderBy(u => u.FullName)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting staff: {ex.Message}", ex); }
        }

        public User? GetStaffById(int id)
        {
            try { return _context.Users.Find(id); }
            catch (Exception ex) { throw new Exception($"Error getting staff by ID: {ex.Message}", ex); }
        }

        public void AddStaff(User user, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentException("Username is required");
                if (string.IsNullOrWhiteSpace(user.FullName)) throw new ArgumentException("Full name is required");
                if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required");
                if (_context.Users.Any(u => u.Username == user.Username)) throw new Exception($"Username '{user.Username}' already exists");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error adding staff: {ex.Message}", ex); }
        }

        public void UpdateStaff(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.Now;
                _context.Users.Update(user);
                _context.SaveChanges();
            }
            catch (Exception ex) { throw new Exception($"Error updating staff: {ex.Message}", ex); }
        }

        public void DeleteStaff(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error deleting staff: {ex.Message}", ex); }
        }

        public void ToggleStaffStatus(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    user.IsActive = !user.IsActive;
                    user.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error toggling staff status: {ex.Message}", ex); }
        }

        public void RestoreStaff(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    user.IsActive = true;
                    user.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error restoring staff: {ex.Message}", ex); }
        }

        public void ResetStaffPassword(int id, string newPassword)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    user.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error resetting staff password: {ex.Message}", ex); }
        }

        #endregion

        #region Inventory Management

        public List<Inventory> GetAllInventory()
        {
            try
            {
                return _context.Inventory
                    .Include(i => i.Product)
                    .Include(i => i.Ingredient)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting inventory: {ex.Message}", ex); }
        }

        public Inventory? GetInventoryForProduct(int productId)
        {
            try { return _context.Inventory.FirstOrDefault(i => i.ProductId == productId); }
            catch (Exception ex) { throw new Exception($"Error getting inventory for product: {ex.Message}", ex); }
        }

        public Inventory? GetInventoryForIngredient(int ingredientId)
        {
            try { return _context.Inventory.FirstOrDefault(i => i.IngredientId == ingredientId); }
            catch (Exception ex) { throw new Exception($"Error getting inventory for ingredient: {ex.Message}", ex); }
        }

        public void UpdateInventory(int id, decimal quantity)
        {
            try
            {
                var inventory = _context.Inventory.Find(id);
                if (inventory != null)
                {
                    inventory.Quantity = quantity;
                    inventory.LastUpdated = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error updating inventory: {ex.Message}", ex); }
        }

        public void AdjustInventory(int id, decimal adjustment)
        {
            try
            {
                var inventory = _context.Inventory.Find(id);
                if (inventory != null)
                {
                    inventory.Quantity += adjustment;
                    inventory.LastUpdated = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { throw new Exception($"Error adjusting inventory: {ex.Message}", ex); }
        }

        public List<Inventory> GetLowStockItems(int threshold = 10)
        {
            try
            {
                return _context.Inventory
                    .Include(i => i.Product)
                    .Include(i => i.Ingredient)
                    .Where(i => i.Quantity < threshold && i.Quantity > 0)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting low stock items: {ex.Message}", ex); }
        }

        public List<Inventory> GetOutOfStockItems()
        {
            try
            {
                return _context.Inventory
                    .Include(i => i.Product)
                    .Include(i => i.Ingredient)
                    .Where(i => i.Quantity <= 0)
                    .ToList();
            }
            catch (Exception ex) { throw new Exception($"Error getting out of stock items: {ex.Message}", ex); }
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}