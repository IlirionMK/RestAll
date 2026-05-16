using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using RestAll.Desktop.Core.Offline;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Infrastructure.Offline;

public class SqliteOfflineStorage : IOfflineStorage
{
    private readonly string _databasePath;
    private readonly ILogger<SqliteOfflineStorage> _logger;
    private const string OrdersTable = "orders";
    private const string MenuCategoriesTable = "menu_categories";
    private const string MenuItemsTable = "menu_items";
    private const string SyncMetadataTable = "sync_metadata";
    private const string PendingOrdersTable = "pending_orders";
    private const string PendingOrderItemsTable = "pending_order_items";
    private const string PendingPaymentsTable = "pending_payments";

    public SqliteOfflineStorage(ILogger<SqliteOfflineStorage> logger)
    {
        _logger = logger;
        _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RestAll", "offline.db");
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
        
        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();
        
        var createOrdersTable = @"
            CREATE TABLE IF NOT EXISTS orders (
                id INTEGER PRIMARY KEY,
                table_id INTEGER NOT NULL,
                user_id INTEGER NOT NULL,
                total_amount REAL NOT NULL,
                status INTEGER NOT NULL,
                items TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
        
        var createMenuCategoriesTable = @"
            CREATE TABLE IF NOT EXISTS menu_categories (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                sort_order INTEGER NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
        
        var createMenuItemsTable = @"
            CREATE TABLE IF NOT EXISTS menu_items (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                price REAL NOT NULL,
                category_id INTEGER,
                description TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
        
        var createSyncMetadataTable = @"
            CREATE TABLE IF NOT EXISTS sync_metadata (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
        
        var command1 = connection.CreateCommand();
        command1.CommandText = createOrdersTable;
        command1.ExecuteNonQuery();
        
        var command2 = connection.CreateCommand();
        command2.CommandText = createMenuCategoriesTable;
        command2.ExecuteNonQuery();
        
        var command3 = connection.CreateCommand();
        command3.CommandText = createMenuItemsTable;
        command3.ExecuteNonQuery();
        
        var command5 = connection.CreateCommand();
        command5.CommandText = createSyncMetadataTable;
        command5.ExecuteNonQuery();
        
        var createOperationsTable = @"
            CREATE TABLE IF NOT EXISTS operation_queue (
                id TEXT PRIMARY KEY,
                type TEXT NOT NULL,
                payload TEXT NOT NULL,
                attempts INTEGER NOT NULL DEFAULT 0,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )";

        var command4 = connection.CreateCommand();
        command4.CommandText = createOperationsTable;
        command4.ExecuteNonQuery();
        
        // Create pending orders table for offline drafts
        var createPendingOrdersTable = @"
            CREATE TABLE IF NOT EXISTS pending_orders (
                local_id INTEGER PRIMARY KEY AUTOINCREMENT,
                server_id INTEGER NULL,
                table_id INTEGER NOT NULL,
                reservation_id INTEGER NULL,
                status TEXT NOT NULL CHECK(status IN ('pending', 'syncing', 'synced', 'conflict', 'failed')),
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                synced_at DATETIME NULL,
                error_message TEXT NULL
            )";
        
        var command6 = connection.CreateCommand();
        command6.CommandText = createPendingOrdersTable;
        command6.ExecuteNonQuery();
        
        // Create pending order items table
        var createPendingOrderItemsTable = @"
            CREATE TABLE IF NOT EXISTS pending_order_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                pending_order_local_id INTEGER NOT NULL,
                menu_item_id INTEGER NOT NULL,
                name TEXT NOT NULL,
                price REAL NOT NULL,
                quantity INTEGER NOT NULL DEFAULT 1,
                comment TEXT NULL,
                operation TEXT NOT NULL CHECK(operation IN ('add', 'remove')),
                synced INTEGER DEFAULT 0,
                FOREIGN KEY (pending_order_local_id) REFERENCES pending_orders(local_id) ON DELETE CASCADE
            )";
        
        var command7 = connection.CreateCommand();
        command7.CommandText = createPendingOrderItemsTable;
        command7.ExecuteNonQuery();
        
        // Create pending payments table
        var createPendingPaymentsTable = @"
            CREATE TABLE IF NOT EXISTS pending_payments (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                order_server_id INTEGER NOT NULL,
                amount REAL NOT NULL,
                payment_method TEXT NULL,
                status TEXT NOT NULL CHECK(status IN ('pending', 'syncing', 'synced', 'failed')),
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                synced_at DATETIME NULL,
                error_message TEXT NULL
            )";
        
        var command8 = connection.CreateCommand();
        command8.CommandText = createPendingPaymentsTable;
        command8.ExecuteNonQuery();
        
        _logger.LogInformation("SQLite database initialized at {Path}", _databasePath);
    }

    public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, table_id, user_id, total_amount, status, items FROM orders ORDER BY created_at DESC";
            
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var orders = new List<Order>();
            
            while (await reader.ReadAsync(cancellationToken))
            {
                var itemsJson = reader.GetString(5);
                var items = JsonSerializer.Deserialize<List<OrderItem>>(itemsJson) ?? new List<OrderItem>();
                
                var order = new Order(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetInt32(2),
                    reader.GetDecimal(3),
                    (OrderStatus)reader.GetInt32(4),
                    items
                );
                
                orders.Add(order);
            }
            
            _logger.LogDebug("Retrieved {Count} orders from offline storage", orders.Count);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders from offline storage");
            return new List<Order>();
        }
    }

    public async Task<List<MenuCategory>> GetMenuCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name, sort_order FROM menu_categories ORDER BY sort_order";
            
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var categories = new List<MenuCategory>();
            
            while (await reader.ReadAsync(cancellationToken))
            {
                var category = new MenuCategory(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    new List<MenuItem>()
                );
                
                categories.Add(category);
            }
            
            _logger.LogDebug("Retrieved {Count} menu categories from offline storage", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu categories from offline storage");
            return new List<MenuCategory>();
        }
    }

    public async Task<List<MenuItem>> GetMenuItemsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name, price, category_id, description FROM menu_items ORDER BY name";
            
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var items = new List<MenuItem>();
            
            while (await reader.ReadAsync(cancellationToken))
            {
                var item = new MenuItem(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(4) ? "" : reader.GetString(4),
                    reader.GetDecimal(2),
                    null,
                    true,
                    reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    null
                );
                
                items.Add(item);
            }
            
            _logger.LogDebug("Retrieved {Count} menu items from offline storage", items.Count);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu items from offline storage");
            return new List<MenuItem>();
        }
    }

    public async Task SaveOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO orders (id, table_id, user_id, total_amount, status, items)
                VALUES (@id, @table_id, @user_id, @total_amount, @status, @items)";
            
            command.Parameters.AddWithValue("@id", order.Id);
            command.Parameters.AddWithValue("@table_id", order.TableId);
            command.Parameters.AddWithValue("@user_id", order.UserId);
            command.Parameters.AddWithValue("@total_amount", order.TotalAmount);
            command.Parameters.AddWithValue("@status", (int)order.Status);
            command.Parameters.AddWithValue("@items", JsonSerializer.Serialize(order.Items));
            
            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Saved order {OrderId} to offline storage", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving order {OrderId} to offline storage", order.Id);
        }
    }

    public async Task SaveMenuCategoriesAsync(List<MenuCategory> categories, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            foreach (var category in categories)
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO menu_categories (id, name, sort_order)
                    VALUES (@id, @name, @sort_order)";
                
                command.Parameters.AddWithValue("@id", category.Id);
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@sort_order", category.SortOrder);
                
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            
            _logger.LogDebug("Saved {Count} menu categories to offline storage", categories.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving menu categories to offline storage");
        }
    }

    public async Task SaveMenuItemsAsync(List<MenuItem> items, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            foreach (var item in items)
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO menu_items (id, name, price, category_id, description)
                    VALUES (@id, @name, @price, @category_id, @description)";
                
                command.Parameters.AddWithValue("@id", item.Id);
                command.Parameters.AddWithValue("@name", item.Name);
                command.Parameters.AddWithValue("@price", item.Price);
                command.Parameters.AddWithValue("@category_id", item.MenuCategoryId);
                command.Parameters.AddWithValue("@description", item.Description ?? (object)DBNull.Value);
                
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            
            _logger.LogDebug("Saved {Count} menu items to offline storage", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving menu items to offline storage");
        }
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM orders; DELETE FROM menu_categories; DELETE FROM menu_items";
            
            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Cleared all offline data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing offline storage");
        }
    }

    public async Task<bool> HasDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM menu_categories";
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if offline storage has data");
            return false;
        }
    }

    // Operation queue methods
    public async Task EnqueueOperationAsync(string id, string type, string payloadJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO operation_queue (id, type, payload, attempts)
                VALUES (@id, @type, @payload, 0)";

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@payload", payloadJson);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Enqueued operation {Id} of type {Type}", id, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueuing operation {Id}", id);
        }
    }

    public async Task<List<(string Id, string Type, string Payload, int Attempts)>> GetPendingOperationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, type, payload, attempts FROM operation_queue ORDER BY created_at ASC";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var list = new List<(string, string, string, int)>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetString(0);
                var type = reader.GetString(1);
                var payload = reader.GetString(2);
                var attempts = reader.GetInt32(3);
                list.Add((id, type, payload, attempts));
            }

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading pending operations");
            return new List<(string, string, string, int)>();
        }
    }

    public async Task RemoveOperationAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM operation_queue WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Removed operation {Id} from queue", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing operation {Id}", id);
        }
    }

    public async Task IncrementOperationAttemptsAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE operation_queue SET attempts = attempts + 1 WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Incremented attempts for operation {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing attempts for operation {Id}", id);
        }
    }

    // Sync metadata methods
    public async Task SetSyncTimeAsync(string entityKey, DateTime syncTime, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO sync_metadata (key, value, updated_at)
                VALUES (@key, @value, @updated_at)";

            command.Parameters.AddWithValue("@key", entityKey);
            command.Parameters.AddWithValue("@value", syncTime.ToString("O")); // ISO 8601 format
            command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Set sync time for {Key} to {Time}", entityKey, syncTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting sync time for {Key}", entityKey);
        }
    }

    public async Task<DateTime?> GetSyncTimeAsync(string entityKey, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT value FROM sync_metadata WHERE key = @key";
            command.Parameters.AddWithValue("@key", entityKey);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result != null && result != DBNull.Value)
            {
                var valueStr = result.ToString();
                if (DateTime.TryParse(valueStr, out var syncTime))
                {
                    _logger.LogDebug("Retrieved sync time for {Key}: {Time}", entityKey, syncTime);
                    return syncTime;
                }
            }

            _logger.LogDebug("No sync time found for {Key}", entityKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync time for {Key}", entityKey);
            return null;
        }
    }

    // Pending orders methods
    public async Task<int> CreatePendingOrderAsync(int tableId, int? reservationId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO pending_orders (table_id, reservation_id, status)
                VALUES (@table_id, @reservation_id, 'pending')
                RETURNING local_id";

            command.Parameters.AddWithValue("@table_id", tableId);
            command.Parameters.AddWithValue("@reservation_id", reservationId.HasValue ? (object)reservationId.Value : DBNull.Value);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var localId = Convert.ToInt32(result);

            _logger.LogInformation("Created pending order with local ID {LocalId} for table {TableId}", localId, tableId);
            return localId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pending order for table {TableId}", tableId);
            throw;
        }
    }

    public async Task<List<PendingOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT local_id, server_id, table_id, reservation_id, status, created_at FROM pending_orders WHERE status IN ('pending', 'syncing', 'failed') ORDER BY created_at ASC";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var orders = new List<PendingOrder>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var order = new PendingOrder(
                    reader.GetInt32(0),
                    reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                    reader.GetInt32(2),
                    reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                    reader.GetString(4),
                    DateTime.Parse(reader.GetString(5))
                );

                orders.Add(order);
            }

            _logger.LogDebug("Retrieved {Count} pending orders", orders.Count);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending orders");
            return new List<PendingOrder>();
        }
    }

    public async Task<PendingOrder?> GetPendingOrderAsync(int localId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT local_id, server_id, table_id, reservation_id, status, created_at FROM pending_orders WHERE local_id = @local_id";
            command.Parameters.AddWithValue("@local_id", localId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new PendingOrder(
                    reader.GetInt32(0),
                    reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                    reader.GetInt32(2),
                    reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                    reader.GetString(4),
                    DateTime.Parse(reader.GetString(5))
                );
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending order {LocalId}", localId);
            return null;
        }
    }

    public async Task MarkOrderSyncedAsync(int localId, int serverId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE pending_orders 
                SET server_id = @server_id, status = 'synced', synced_at = @synced_at
                WHERE local_id = @local_id";

            command.Parameters.AddWithValue("@local_id", localId);
            command.Parameters.AddWithValue("@server_id", serverId);
            command.Parameters.AddWithValue("@synced_at", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Marked order {LocalId} as synced with server ID {ServerId}", localId, serverId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {LocalId} as synced", localId);
        }
    }

    public async Task MarkOrderFailedAsync(int localId, string errorMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE pending_orders 
                SET status = 'failed', error_message = @error_message
                WHERE local_id = @local_id";

            command.Parameters.AddWithValue("@local_id", localId);
            command.Parameters.AddWithValue("@error_message", errorMessage);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogWarning("Marked order {LocalId} as failed: {ErrorMessage}", localId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {LocalId} as failed", localId);
        }
    }

    public async Task MarkOrderSyncingAsync(int localId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE pending_orders SET status = 'syncing' WHERE local_id = @local_id";
            command.Parameters.AddWithValue("@local_id", localId);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Marked order {LocalId} as syncing", localId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {LocalId} as syncing", localId);
        }
    }

    // Pending order items methods
    public async Task AddPendingOrderItemAsync(int localOrderId, PendingOrderItem item, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO pending_order_items (pending_order_local_id, menu_item_id, name, price, quantity, comment, operation)
                VALUES (@local_order_id, @menu_item_id, @name, @price, @quantity, @comment, @operation)";

            command.Parameters.AddWithValue("@local_order_id", localOrderId);
            command.Parameters.AddWithValue("@menu_item_id", item.MenuItemId);
            command.Parameters.AddWithValue("@name", item.Name);
            command.Parameters.AddWithValue("@price", item.Price);
            command.Parameters.AddWithValue("@quantity", item.Quantity);
            command.Parameters.AddWithValue("@comment", item.Comment ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@operation", item.Operation);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Added pending item {MenuItemId} to order {LocalOrderId}", item.MenuItemId, localOrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding pending item to order {LocalOrderId}", localOrderId);
        }
    }

    public async Task<List<PendingOrderItem>> GetPendingOrderItemsAsync(int localOrderId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT menu_item_id, name, price, quantity, comment, operation FROM pending_order_items WHERE pending_order_local_id = @local_order_id AND synced = 0";
            command.Parameters.AddWithValue("@local_order_id", localOrderId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var items = new List<PendingOrderItem>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var item = new PendingOrderItem(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDecimal(2),
                    reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.GetString(5)
                );

                items.Add(item);
            }

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending items for order {LocalOrderId}", localOrderId);
            return new List<PendingOrderItem>();
        }
    }

    public async Task MarkPendingOrderItemSyncedAsync(int itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE pending_order_items SET synced = 1 WHERE id = @id";
            command.Parameters.AddWithValue("@id", itemId);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Marked pending item {ItemId} as synced", itemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking pending item {ItemId} as synced", itemId);
        }
    }

    public async Task RemovePendingOrderItemAsync(int localOrderId, int menuItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO pending_order_items (pending_order_local_id, menu_item_id, name, price, quantity, operation)
                VALUES (@local_order_id, @menu_item_id, '', 0, 1, 'remove')";

            command.Parameters.AddWithValue("@local_order_id", localOrderId);
            command.Parameters.AddWithValue("@menu_item_id", menuItemId);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Queued removal of item {MenuItemId} from order {LocalOrderId}", menuItemId, localOrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing item removal for order {LocalOrderId}", localOrderId);
        }
    }

    // Pending payments methods
    public async Task AddPendingPaymentAsync(int orderServerId, decimal amount, string? paymentMethod, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO pending_payments (order_server_id, amount, payment_method, status)
                VALUES (@order_server_id, @amount, @payment_method, 'pending')";

            command.Parameters.AddWithValue("@order_server_id", orderServerId);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@payment_method", paymentMethod ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Added pending payment for order {OrderId}, amount {Amount}", orderServerId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding pending payment for order {OrderId}", orderServerId);
        }
    }

    public async Task<List<PendingPayment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, order_server_id, amount, payment_method, status FROM pending_payments WHERE status IN ('pending', 'failed') ORDER BY created_at ASC";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var payments = new List<PendingPayment>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var payment = new PendingPayment(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetDecimal(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.GetString(4)
                );

                payments.Add(payment);
            }

            return payments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            return new List<PendingPayment>();
        }
    }

    public async Task MarkPaymentSyncedAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE pending_payments 
                SET status = 'synced', synced_at = @synced_at
                WHERE id = @id";

            command.Parameters.AddWithValue("@id", paymentId);
            command.Parameters.AddWithValue("@synced_at", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Marked payment {PaymentId} as synced", paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment {PaymentId} as synced", paymentId);
        }
    }

    public async Task MarkPaymentFailedAsync(int paymentId, string errorMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE pending_payments 
                SET status = 'failed', error_message = @error_message
                WHERE id = @id";

            command.Parameters.AddWithValue("@id", paymentId);
            command.Parameters.AddWithValue("@error_message", errorMessage);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogWarning("Marked payment {PaymentId} as failed: {ErrorMessage}", paymentId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment {PaymentId} as failed", paymentId);
        }
    }

    // Count pending operations
    public async Task<int> GetPendingOperationsCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    (SELECT COUNT(*) FROM pending_orders WHERE status IN ('pending', 'syncing')) +
                    (SELECT COUNT(*) FROM pending_payments WHERE status IN ('pending', 'syncing'))";

            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting pending operations");
            return 0;
        }
    }
}
