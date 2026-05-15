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
        
        var command1 = connection.CreateCommand();
        command1.CommandText = createOrdersTable;
        command1.ExecuteNonQuery();
        
        var command2 = connection.CreateCommand();
        command2.CommandText = createMenuCategoriesTable;
        command2.ExecuteNonQuery();
        
        var command3 = connection.CreateCommand();
        command3.CommandText = createMenuItemsTable;
        command3.ExecuteNonQuery();
        
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
}
