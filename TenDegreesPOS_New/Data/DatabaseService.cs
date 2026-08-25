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





        #region Supplier Management

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
                    // Table doesn't exist yet, return empty list
                    return new List<Supplier>();
                }
                throw new Exception($"Error getting suppliers: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting suppliers: {ex.Message}", ex);
            }
        }

        public Supplier? GetSupplierById(int id)
        {
            try
            {
                return _context.Suppliers.Find(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting supplier by ID: {ex.Message}", ex);
            }
        }

        public void AddSupplier(Supplier supplier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(supplier.Name))
                    throw new ArgumentException("Supplier name is required");

                supplier.CreatedAt = DateTime.Now;
                supplier.IsActive = true;

                _context.Suppliers.Add(supplier);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding supplier: {ex.Message}", ex);
            }
        }

        public void UpdateSupplier(Supplier supplier)
        {
            try
            {
                supplier.UpdatedAt = DateTime.Now;
                _context.Suppliers.Update(supplier);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating supplier: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error deleting supplier: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error toggling supplier status: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting completed transactions: {ex.Message}", ex);
            }
        }

        public List<OrderCancellation> GetAllCancellations()
        {
            try
            {
                return _context.OrderCancellations
                    .Include(oc => oc.Transaction)
                    .Include(oc => oc.CancelledByUser)
                    .OrderByDescending(oc => oc.CancelledDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting cancellations: {ex.Message}", ex);
            }
        }

        public OrderCancellation? GetCancellationById(int id)
        {
            try
            {
                return _context.OrderCancellations
                    .Include(oc => oc.Transaction)
                    .ThenInclude(t => t.TransactionItems)
                    .Include(oc => oc.CancelledByUser)
                    .FirstOrDefault(oc => oc.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting cancellation: {ex.Message}", ex);
            }
        }

        public List<OrderCancellation> GetCancellationsByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _context.OrderCancellations
                    .Include(oc => oc.Transaction)
                    .Include(oc => oc.CancelledByUser)
                    .Where(oc => oc.CancelledDate >= startDate && oc.CancelledDate <= endDate)
                    .OrderByDescending(oc => oc.CancelledDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting cancellations by date: {ex.Message}", ex);
            }
        }

        public OrderCancellation CancelTransaction(int transactionId, int userId, string reason, string refundMethod, string? notes = null)
        {
            using var dbTransaction = _context.Database.BeginTransaction();

            try
            {
                var transaction = _context.Transactions
                    .Include(t => t.TransactionItems)
                    .FirstOrDefault(t => t.Id == transactionId);

                if (transaction == null)
                    throw new Exception("Transaction not found");

                if (transaction.Status != "Completed")
                    throw new Exception("Only completed transactions can be cancelled");

                // Create cancellation record
                var cancellation = new OrderCancellation
                {
                    TransactionId = transaction.Id,
                    TransactionNumber = transaction.TransactionNumber,
                    CancelledDate = DateTime.Now,
                    CancelledByUserId = userId,
                    CancellationType = "Full",
                    Reason = reason,
                    OriginalTotal = transaction.Total,
                    RefundAmount = transaction.Total,
                    RefundMethod = refundMethod,
                    Notes = notes
                };

                _context.OrderCancellations.Add(cancellation);
                _context.SaveChanges();

                // Create cancelled items records
                foreach (var item in transaction.TransactionItems)
                {
                    var cancelledItem = new CancelledOrderItem
                    {
                        CancellationId = cancellation.Id,
                        OriginalTransactionItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name ?? "Unknown Product",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice,
                        Size = item.Size
                    };
                    _context.CancelledOrderItems.Add(cancelledItem);

                    // Restore inventory
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
                                ingredientInventory.Quantity += pi.Quantity * item.Quantity;
                                ingredientInventory.LastUpdated = DateTime.Now;
                            }
                        }
                    }
                }

                // Update transaction status
                transaction.Status = "Cancelled";
                _context.SaveChanges();

                dbTransaction.Commit();
                return cancellation;
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw new Exception($"Error cancelling transaction: {ex.Message}", ex);
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

                if (cancellation == null)
                    throw new Exception("Cancellation record not found");

                if (cancellation.Transaction?.Status != "Cancelled")
                    throw new Exception("Only cancelled transactions can be restored");

                // Restore inventory (reverse the cancellation)
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

                // Update transaction status back to Completed
                if (cancellation.Transaction != null)
                {
                    cancellation.Transaction.Status = "Completed";
                }

                // Soft delete cancellation record or mark as restored
                // You could add a "IsRestored" boolean to OrderCancellation if needed

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
            catch (Exception ex)
            {
                throw new Exception($"Error getting supplier ingredients: {ex.Message}", ex);
            }
        }

        public void SaveSupplierIngredients(int supplierId, List<int> ingredientIds)
        {
            try
            {
                // Remove existing assignments for this supplier
                var existing = _context.SupplierIngredients.Where(si => si.SupplierId == supplierId);
                _context.SupplierIngredients.RemoveRange(existing);

                // Add new assignments
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
            catch (Exception ex)
            {
                throw new Exception($"Error saving supplier ingredients: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting all supplier ingredients: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting purchase orders: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting purchase order by ID: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error generating purchase order number: {ex.Message}", ex);
            }
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
                    // Get the ingredient to get its unit for proper conversion
                    var ingredient = _context.Ingredients.Find(item.ingredientId);
                    string ingredientUnit = ingredient?.Unit ?? "g";
                    decimal boxQuantity = item.boxQuantityPerUnit;
                    string boxUnit = item.unit;

                    // Calculate the quantity per box in the ingredient's unit
                    decimal quantityPerBoxInIngredientUnit = boxQuantity;

                    // Convert if units don't match
                    if (boxUnit.ToLower() != ingredientUnit.ToLower())
                    {
                        if (boxUnit.ToLower() == "l" && ingredientUnit.ToLower() == "ml")
                            quantityPerBoxInIngredientUnit = boxQuantity * 1000m;
                        else if (boxUnit.ToLower() == "ml" && ingredientUnit.ToLower() == "l")
                            quantityPerBoxInIngredientUnit = boxQuantity / 1000m;
                        else if (boxUnit.ToLower() == "kg" && ingredientUnit.ToLower() == "g")
                            quantityPerBoxInIngredientUnit = boxQuantity * 1000m;
                        else if (boxUnit.ToLower() == "g" && ingredientUnit.ToLower() == "kg")
                            quantityPerBoxInIngredientUnit = boxQuantity / 1000m;
                    }

                    decimal totalQuantityInIngredientUnit = item.boxes * quantityPerBoxInIngredientUnit;

                    // Debug output
                    System.Diagnostics.Debug.WriteLine($"Creating order item: {item.ingredientName}");
                    System.Diagnostics.Debug.WriteLine($"  Boxes: {item.boxes}");
                    System.Diagnostics.Debug.WriteLine($"  Box Unit: {boxUnit}, Box Quantity: {boxQuantity}");
                    System.Diagnostics.Debug.WriteLine($"  Ingredient Unit: {ingredientUnit}");
                    System.Diagnostics.Debug.WriteLine($"  Quantity per box in ingredient unit: {quantityPerBoxInIngredientUnit}");
                    System.Diagnostics.Debug.WriteLine($"  Total Quantity: {totalQuantityInIngredientUnit} {ingredientUnit}");

                    var orderItem = new PurchaseOrderItem
                    {
                        IngredientId = item.ingredientId,
                        IngredientName = item.ingredientName,
                        BoxesOrdered = item.boxes,
                        BoxesReceived = 0,
                        QuantityOrdered = totalQuantityInIngredientUnit,
                        QuantityReceived = 0,
                        BoxQuantityPerUnit = boxQuantity,
                        BoxUnit = boxUnit,
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

        public void ReceivePurchaseOrderPartially(int orderId, Dictionary<int, decimal> receivedBoxes)
        {
            using var dbTransaction = _context.Database.BeginTransaction();

            try
            {
                var order = _context.PurchaseOrders
                    .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Ingredient)
                    .FirstOrDefault(po => po.Id == orderId);

                if (order == null)
                    throw new Exception("Purchase order not found");

                if (order.Status == "Received")
                    throw new Exception("Order has already been fully received");

                bool allFullyReceived = true;

                foreach (var item in order.PurchaseOrderItems)
                {
                    if (receivedBoxes.ContainsKey(item.Id))
                    {
                        decimal boxesToReceive = receivedBoxes[item.Id];
                        decimal currentReceivedBoxes = item.BoxesReceived;
                        decimal newReceivedBoxes = currentReceivedBoxes + boxesToReceive;

                        if (newReceivedBoxes > item.BoxesOrdered + 0.01m)
                            throw new Exception($"Cannot receive more than ordered for {item.IngredientName}");

                        item.BoxesReceived = newReceivedBoxes;

                        // Calculate quantity received based on box quantity with proper unit conversion
                        decimal boxQuantity = item.BoxQuantityPerUnit ?? 1;
                        string boxUnit = item.BoxUnit ?? "kg";
                        string ingredientUnit = item.Ingredient?.Unit ?? "g";

                        decimal quantityPerBoxInIngredientUnit = boxQuantity;

                        // Convert if units don't match
                        if (boxUnit.ToLower() != ingredientUnit.ToLower())
                        {
                            if (boxUnit.ToLower() == "l" && ingredientUnit.ToLower() == "ml")
                                quantityPerBoxInIngredientUnit = boxQuantity * 1000m;
                            else if (boxUnit.ToLower() == "ml" && ingredientUnit.ToLower() == "l")
                                quantityPerBoxInIngredientUnit = boxQuantity / 1000m;
                            else if (boxUnit.ToLower() == "kg" && ingredientUnit.ToLower() == "g")
                                quantityPerBoxInIngredientUnit = boxQuantity * 1000m;
                            else if (boxUnit.ToLower() == "g" && ingredientUnit.ToLower() == "kg")
                                quantityPerBoxInIngredientUnit = boxQuantity / 1000m;
                        }

                        decimal quantityReceived = boxesToReceive * quantityPerBoxInIngredientUnit;
                        item.QuantityReceived = quantityReceived;

                        // Update inventory
                        var inventory = _context.Inventory.FirstOrDefault(i => i.IngredientId == item.IngredientId);
                        decimal quantityToAdd = boxesToReceive * quantityPerBoxInIngredientUnit;

                        if (inventory != null)
                        {
                            inventory.Quantity += quantityToAdd;
                            inventory.LastUpdated = DateTime.Now;
                        }
                        else
                        {
                            var newInventory = new Inventory
                            {
                                IngredientId = item.IngredientId,
                                Quantity = quantityToAdd,
                                Unit = ingredientUnit,
                                LastUpdated = DateTime.Now
                            };
                            _context.Inventory.Add(newInventory);
                        }

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

        // Add this helper method to DatabaseService.cs for unit conversion
        private decimal ConvertToIngredientUnit(decimal quantity, string fromUnit, string toUnit)
        {
            // If units are the same, no conversion needed
            if (fromUnit.ToLower() == toUnit.ToLower())
                return quantity;

            // Convert to base unit first, then to target unit
            switch (fromUnit.ToLower())
            {
                case "kg":
                    // Convert kg to g
                    if (toUnit.ToLower() == "g")
                        return quantity * 1000m;
                    return quantity;

                case "g":
                    // Convert g to kg
                    if (toUnit.ToLower() == "kg")
                        return quantity / 1000m;
                    return quantity;

                case "l":
                    // Convert L to ml
                    if (toUnit.ToLower() == "ml")
                        return quantity * 1000m;
                    return quantity;

                case "ml":
                    // Convert ml to L
                    if (toUnit.ToLower() == "l")
                        return quantity / 1000m;
                    return quantity;

                default:
                    return quantity;
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
            catch (Exception ex)
            {
                throw new Exception($"Error updating purchase order status: {ex.Message}", ex);
            }
        }

        #endregion



        #region Database Schema Updates

        /// <summary>
        /// Ensures the database schema matches the current model
        /// Adds missing columns if they don't exist
        /// </summary>
        public void EnsureDatabaseSchemaUpdated()
        {
            try
            {
                // Try to query a product to see if columns exist
                _context.Products.FirstOrDefault();
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                if (ex.Message.Contains("no such column: p.HasSizes") ||
                    ex.Message.Contains("no such column"))
                {
                    try
                    {
                        // Add HasSizes column
                        _context.Database.ExecuteSqlRaw("ALTER TABLE Products ADD COLUMN HasSizes INTEGER DEFAULT 0;");
                        Console.WriteLine("Added HasSizes column to Products table");
                    }
                    catch (Exception e) { Console.WriteLine($"Error adding HasSizes: {e.Message}"); }

                    try
                    {
                        // Add PriceSmall column
                        _context.Database.ExecuteSqlRaw("ALTER TABLE Products ADD COLUMN PriceSmall DECIMAL;");
                        Console.WriteLine("Added PriceSmall column to Products table");
                    }
                    catch (Exception e) { Console.WriteLine($"Error adding PriceSmall: {e.Message}"); }

                    try
                    {
                        // Add PriceMedium column
                        _context.Database.ExecuteSqlRaw("ALTER TABLE Products ADD COLUMN PriceMedium DECIMAL;");
                        Console.WriteLine("Added PriceMedium column to Products table");
                    }
                    catch (Exception e) { Console.WriteLine($"Error adding PriceMedium: {e.Message}"); }

                    try
                    {
                        // Add PriceLarge column
                        _context.Database.ExecuteSqlRaw("ALTER TABLE Products ADD COLUMN PriceLarge DECIMAL;");
                        Console.WriteLine("Added PriceLarge column to Products table");
                    }
                    catch (Exception e) { Console.WriteLine($"Error adding PriceLarge: {e.Message}"); }

                    try
                    {
                        // Add Size column to TransactionItems
                        _context.Database.ExecuteSqlRaw("ALTER TABLE TransactionItems ADD COLUMN Size TEXT;");
                        Console.WriteLine("Added Size column to TransactionItems table");
                    }
                    catch (Exception e) { Console.WriteLine($"Error adding Size: {e.Message}"); }

                    try
                    {
                        // Add ImagePath column to Products
                        _context.Database.ExecuteSqlRaw("ALTER TABLE Products ADD COLUMN ImagePath TEXT;");
                        Console.WriteLine("Added ImagePath column to Products table");
                    }
                    catch (Exception e) { Console.WriteLine($"Error adding ImagePath: {e.Message}"); }
                }
            }

            // Check if UnitCost column still exists and remove it
            try
            {
                _context.Ingredients.FirstOrDefault();
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                if (ex.Message.Contains("no such column: i.UnitCost"))
                {
                    try
                    {
                        // UnitCost column doesn't exist - good! No action needed
                        Console.WriteLine("UnitCost column not present - schema is correct");
                    }
                    catch (Exception e) { Console.WriteLine($"Error checking UnitCost: {e.Message}"); }
                }
                else if (ex.Message.Contains("UnitCost"))
                {
                    try
                    {
                        // UnitCost column exists, try to remove it
                        _context.Database.ExecuteSqlRaw("ALTER TABLE Ingredients DROP COLUMN UnitCost;");
                        Console.WriteLine("Removed UnitCost column from Ingredients table");
                    }
                    catch (Exception e) { Console.WriteLine($"Error removing UnitCost: {e.Message}"); }
                }
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Authentication error: {ex.Message}", ex);
            }
        }

        private void CreateDefaultUsers()
        {
            try
            {
                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                        Role = "Admin",
                        FullName = "System Administrator",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        Username = "manager",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager"),
                        Role = "Manager",
                        FullName = "Store Manager",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        Username = "cashier",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("cashier"),
                        Role = "Cashier",
                        FullName = "Cashier User",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                };

                _context.Users.AddRange(users);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating default users: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error creating default categories: {ex.Message}", ex);
            }
        }

        #endregion

        #region User Management

        public List<User> GetAllUsers()
        {
            try
            {
                return _context.Users.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting users: {ex.Message}", ex);
            }
        }

        public User? GetUserById(int id)
        {
            try
            {
                return _context.Users.Find(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user by ID: {ex.Message}", ex);
            }
        }

        public User? GetUserByUsername(string username)
        {
            try
            {
                return _context.Users.FirstOrDefault(u => u.Username == username);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user by username: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting all products: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting product by ID: {ex.Message}", ex);
            }
        }

        public Product? GetProductByBarcode(string barcode)
        {
            try
            {
                return _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Barcode == barcode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting product by barcode: {ex.Message}", ex);
            }
        }

        public List<Product> GetProductsByCategory(int categoryId)
        {
            try
            {
                return _context.Products
                    .Where(p => p.CategoryId == categoryId && p.IsActive)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting products by category: {ex.Message}", ex);
            }
        }

        public void AddProduct(Product product)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(product.Name))
                    throw new ArgumentException("Product name is required");

                if (product.Price <= 0)
                    throw new ArgumentException("Product price must be greater than 0");

                // Validate category exists
                if (product.CategoryId <= 0)
                {
                    // Try to assign default category
                    var defaultCategory = _context.Categories.FirstOrDefault();
                    if (defaultCategory != null)
                    {
                        product.CategoryId = defaultCategory.Id;
                    }
                    else
                    {
                        throw new Exception("No categories available. Please create a category first.");
                    }
                }
                else
                {
                    var categoryExists = _context.Categories.Any(c => c.Id == product.CategoryId);
                    if (!categoryExists)
                    {
                        throw new Exception($"Category with ID {product.CategoryId} does not exist");
                    }
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
            catch (Exception ex)
            {
                throw new Exception($"Error adding product: {ex.Message}", ex);
            }
        }

        public void UpdateProduct(Product product)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(product.Name))
                    throw new ArgumentException("Product name is required");

                if (product.Price <= 0)
                    throw new ArgumentException("Product price must be greater than 0");

                // Validate category exists
                if (product.CategoryId > 0)
                {
                    var categoryExists = _context.Categories.Any(c => c.Id == product.CategoryId);
                    if (!categoryExists)
                    {
                        throw new Exception($"Category with ID {product.CategoryId} does not exist");
                    }
                }

                product.UpdatedAt = DateTime.Now;

                // Detach any existing tracked entity with the same ID to avoid conflicts
                var tracked = _context.Products.Local.FirstOrDefault(p => p.Id == product.Id);
                if (tracked != null)
                {
                    _context.Entry(tracked).State = EntityState.Detached;
                }

                _context.Products.Update(product);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"Database error updating product: {innerMessage}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating product: {ex.Message}", ex);
            }
        }

        public void DeleteProduct(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product != null)
                {
                    // Soft delete - just mark as inactive
                    product.IsActive = false;
                    product.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting product: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Hard deletes a product permanently (only if it has no transactions)
        /// </summary>
        public void HardDeleteProduct(int id)
        {
            try
            {
                var product = _context.Products
                    .Include(p => p.ProductIngredients)
                    .FirstOrDefault(p => p.Id == id);

                if (product != null)
                {
                    // Check if product is used in any transactions
                    bool hasTransactions = _context.TransactionItems.Any(ti => ti.ProductId == id);

                    if (hasTransactions)
                    {
                        throw new Exception("Cannot delete product that has transaction history. Use soft delete instead.");
                    }

                    // Remove related ingredients first
                    if (product.ProductIngredients.Any())
                    {
                        _context.ProductIngredients.RemoveRange(product.ProductIngredients);
                    }

                    _context.Products.Remove(product);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error hard deleting product: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a product has any transaction history
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>True if the product has been sold before</returns>
        public bool ProductHasTransactions(int productId)
        {
            try
            {
                return _context.TransactionItems.Any(ti => ti.ProductId == productId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking product transactions: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves product ingredients by removing existing ones and adding the new list
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <param name="ingredients">List of ingredients to save</param>
        /// <returns>Number of ingredients saved</returns>
        public int SaveProductIngredients(int productId, List<ProductIngredient> ingredients)
        {
            try
            {
                // Remove existing ingredients
                var existingIngredients = _context.ProductIngredients
                    .Where(pi => pi.ProductId == productId)
                    .ToList();
                _context.ProductIngredients.RemoveRange(existingIngredients);
                _context.SaveChanges();

                int savedCount = 0;

                // Add new ingredients
                foreach (var ingredient in ingredients)
                {
                    var newIngredient = new ProductIngredient
                    {
                        ProductId = productId,
                        IngredientId = ingredient.IngredientId,
                        Quantity = ingredient.Quantity  // Already converted to ingredient's unit
                    };
                    _context.ProductIngredients.Add(newIngredient);
                    savedCount++;
                }

                _context.SaveChanges();
                return savedCount;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving product ingredients: {ex.Message}", ex);
            }
        }

        public List<Category> GetAllCategories()
        {
            try
            {
                return _context.Categories.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting categories: {ex.Message}", ex);
            }
        }

        public Category? GetCategoryById(int id)
        {
            try
            {
                return _context.Categories.Find(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting category by ID: {ex.Message}", ex);
            }
        }

        public Category? GetCategoryByName(string name)
        {
            try
            {
                return _context.Categories.FirstOrDefault(c => c.Name == name);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting category by name: {ex.Message}", ex);
            }
        }

        public void AddCategory(Category category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                    throw new ArgumentException("Category name is required");

                // Check if category already exists
                if (_context.Categories.Any(c => c.Name == category.Name))
                    throw new Exception($"Category '{category.Name}' already exists");

                _context.Categories.Add(category);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding category: {ex.Message}", ex);
            }
        }

        public void UpdateCategory(Category category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                    throw new ArgumentException("Category name is required");

                _context.Categories.Update(category);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating category: {ex.Message}", ex);
            }
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
                    // Check if category has products
                    if (category.Products.Any(p => p.IsActive))
                    {
                        throw new Exception("Cannot delete category that has active products");
                    }

                    _context.Categories.Remove(category);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting category: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting transactions by cashier: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting all transactions: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting transaction by ID: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting transaction by number: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting transactions by date: {ex.Message}", ex);
            }
        }

        public Transaction CreateTransaction(Transaction transaction, List<(int productId, int quantity, decimal unitPrice, string? size)> items)
        {
            using var dbTransaction = _context.Database.BeginTransaction();

            try
            {
                transaction.TransactionNumber = GenerateTransactionNumber();
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

                    // Update inventory (deduct ingredients)
                    var productIngredients = _context.ProductIngredients
                        .Where(pi => pi.ProductId == item.productId)
                        .Include(pi => pi.Ingredient)
                        .ToList();

                    foreach (var pi in productIngredients)
                    {
                        if (pi.Ingredient != null)
                        {
                            // Get quantity based on size if product has sizes
                            decimal quantityPerUnit = pi.Quantity;
                            if (product.HasSizes && !string.IsNullOrEmpty(item.size))
                            {
                                switch (item.size)
                                {
                                    case "Small":
                                        quantityPerUnit = pi.QuantitySmall ?? pi.Quantity;
                                        break;
                                    case "Medium":
                                        quantityPerUnit = pi.QuantityMedium ?? pi.Quantity;
                                        break;
                                    case "Large":
                                        quantityPerUnit = pi.QuantityLarge ?? pi.Quantity;
                                        break;
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

                // Use the values passed from the cashier - DO NOT RECALCULATE
                // The totals are already correctly calculated in CashierDashboard
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
                string prefix = "INV";
                string datePart = DateTime.Now.ToString("yyyyMMdd");
                int count = _context.Transactions
                    .Count(t => t.TransactionDate.Date == DateTime.Now.Date) + 1;
                return $"{prefix}{datePart}{count:D4}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating transaction number: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error voiding transaction: {ex.Message}", ex);
            }
        }

        public decimal GetDailySales(DateTime date)
        {
            try
            {
                // Get the transactions and sum in memory instead of SQL
                var transactions = _context.Transactions
                    .Where(t => t.TransactionDate.Date == date.Date && t.Status == "Completed")
                    .ToList();

                return transactions.Sum(t => t.Total);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting daily sales: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting monthly sales: {ex.Message}", ex);
            }
        }

        #endregion

        #region Ingredient Management (UnitCost REMOVED)

        public List<Ingredient> GetAllIngredients()
        {
            try
            {
                return _context.Ingredients.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting ingredients: {ex.Message}", ex);
            }
        }

        public Ingredient? GetIngredientById(int id)
        {
            try
            {
                return _context.Ingredients.Find(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting ingredient by ID: {ex.Message}", ex);
            }
        }

        public Ingredient? GetIngredientByName(string name)
        {
            try
            {
                return _context.Ingredients.FirstOrDefault(i => i.Name == name);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting ingredient by name: {ex.Message}", ex);
            }
        }
        public void AddIngredient(Ingredient ingredient)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ingredient.Name))
                    throw new ArgumentException("Ingredient name is required");

                ingredient.IsActive = true;
                _context.Ingredients.Add(ingredient);
                _context.SaveChanges();

                // Create inventory record for this ingredient with converted stock
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
            catch (Exception ex)
            {
                throw new Exception($"Error adding ingredient: {ex.Message}", ex);
            }
        }

        public void UpdateIngredient(Ingredient ingredient)
        {
            try
            {
                ingredient.UpdatedAt = DateTime.Now;
                _context.Ingredients.Update(ingredient);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating ingredient: {ex.Message}", ex);
            }
        }

        public decimal GetCurrentIngredientStock(int ingredientId)
        {
            try
            {
                var inventory = _context.Inventory.FirstOrDefault(i => i.IngredientId == ingredientId);
                return inventory?.Quantity ?? 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting ingredient stock: {ex.Message}", ex);
            }
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
                    // Check if ingredient is used in any products
                    if (ingredient.ProductIngredients.Any())
                    {
                        throw new Exception("Cannot delete ingredient that is used in products");
                    }

                    // Remove inventory record first
                    var inventory = _context.Inventory.FirstOrDefault(i => i.IngredientId == id);
                    if (inventory != null)
                    {
                        _context.Inventory.Remove(inventory);
                    }

                    _context.Ingredients.Remove(ingredient);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting ingredient: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting staff: {ex.Message}", ex);
            }
        }

        public User? GetStaffById(int id)
        {
            try
            {
                return _context.Users.Find(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting staff by ID: {ex.Message}", ex);
            }
        }

        public void AddStaff(User user, string password)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(user.Username))
                    throw new ArgumentException("Username is required");

                if (string.IsNullOrWhiteSpace(user.FullName))
                    throw new ArgumentException("Full name is required");

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Password is required");

                // Check if username already exists
                if (_context.Users.Any(u => u.Username == user.Username))
                    throw new Exception($"Username '{user.Username}' already exists");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding staff: {ex.Message}", ex);
            }
        }

        public void UpdateStaff(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.Now;
                _context.Users.Update(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating staff: {ex.Message}", ex);
            }
        }


        public void DeleteStaff(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    // Soft delete
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting staff: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error toggling staff status: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error restoring staff: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error resetting staff password: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting inventory: {ex.Message}", ex);
            }
        }

        public Inventory? GetInventoryForProduct(int productId)
        {
            try
            {
                return _context.Inventory.FirstOrDefault(i => i.ProductId == productId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting inventory for product: {ex.Message}", ex);
            }
        }

        public Inventory? GetInventoryForIngredient(int ingredientId)
        {
            try
            {
                return _context.Inventory.FirstOrDefault(i => i.IngredientId == ingredientId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting inventory for ingredient: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error updating inventory: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error adjusting inventory: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting low stock items: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting out of stock items: {ex.Message}", ex);
            }
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}