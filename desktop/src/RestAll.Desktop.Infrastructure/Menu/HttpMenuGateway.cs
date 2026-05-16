using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Menu;

public sealed class HttpMenuGateway : IMenuGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly ILogger<HttpMenuGateway> _logger;

    public HttpMenuGateway(HttpClient httpClient, RestAllApiOptions options, ILogger<HttpMenuGateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"{_options.BaseUrl}/menu/categories";
            _logger.LogInformation("Fetching menu categories from {Endpoint}", endpoint);
            
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Menu categories response: HTTP {StatusCode}, Content: {ContentLength} bytes", 
                response.StatusCode, responseContent.Length);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch menu categories (HTTP {StatusCode}): {Response}", 
                    response.StatusCode, responseContent.Length > 500 ? responseContent.Substring(0, 500) : responseContent);
                return new List<MenuCategory>();
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("Unexpected response format: expected array, got {ValueKind}", data.ValueKind);
                return new List<MenuCategory>();
            }

            var categories = new List<MenuCategory>();
            var rejectedCategories = 0;
            var rejectedItemsTotal = 0;
            var sampleRejectedItems = new List<string>();

            var globalReasonCodes = new HashSet<string>();
            foreach (var element in data.EnumerateArray())
            {
                var category = ParseCategory(element, out var rejectedInCategory, out var rejectedExamples, out var rejectedReasonCodes);
                if (category is not null)
                {
                    categories.Add(category);
                    rejectedItemsTotal += rejectedInCategory;
                    foreach (var ex in rejectedExamples)
                    {
                        if (sampleRejectedItems.Count < 5)
                        {
                            sampleRejectedItems.Add(ex.Length > 300 ? ex.Substring(0, 300) : ex);
                        }
                    }
                    foreach (var rc in rejectedReasonCodes)
                    {
                        globalReasonCodes.Add(rc);
                    }
                }
                else
                {
                    rejectedCategories++;
                }
            }

            if (rejectedCategories > 0 || rejectedItemsTotal > 0)
            {
                // Log concise warning with reason codes; full samples go to Debug to avoid noisy/large logs in higher environments
                var reasonCodesSummary = string.Join(',', globalReasonCodes);
                var evt = new EventId(1001, "MenuParseRejected");
                _logger.LogWarning(evt, "Menu parsing: {CategoryRejected} categories rejected, {ItemRejected} items rejected. ReasonCodes: {ReasonCodes}",
                    rejectedCategories, rejectedItemsTotal, reasonCodesSummary);

                if (sampleRejectedItems.Count > 0)
                {
                    _logger.LogDebug(evt, "Menu parsing samples (truncated): {Samples}", string.Join(" || ", sampleRejectedItems));
                }
            }

            _logger.LogInformation("Loaded {CategoryCount} menu categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching menu categories from API");
            return new List<MenuCategory>();
        }
    }

    public async Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching menu items via menu categories endpoint");
            var categories = await GetCategoriesAsync(cancellationToken);
            return categories.SelectMany(category => category.Items).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching menu items from API");
            return new List<MenuItem>();
        }
    }

    public async Task<MenuItem?> CreateItemAsync(MenuItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"{_options.BaseUrl}/menu/items";
            _logger.LogInformation("Creating menu item: {Name}", request.Name);

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to create menu item (HTTP {StatusCode}): {Response}",
                    response.StatusCode, responseContent);
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var menuItem = ParseMenuItem(data);

            if (menuItem is not null)
            {
                _logger.LogInformation("Menu item created successfully with ID {Id}", menuItem.Id);
            }

            return menuItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item");
            return null;
        }
    }

    public async Task<MenuItem?> UpdateItemAsync(int id, MenuItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"{_options.BaseUrl}/menu/items/{id}";
            _logger.LogInformation("Updating menu item {Id}: {Name}", id, request.Name);

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update menu item (HTTP {StatusCode}): {Response}",
                    response.StatusCode, responseContent);
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var menuItem = ParseMenuItem(data);

            if (menuItem is not null)
            {
                _logger.LogInformation("Menu item {Id} updated successfully", id);
            }

            return menuItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item {Id}", id);
            return null;
        }
    }

    public async Task<bool> ToggleAvailabilityAsync(int id, bool isAvailable, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"{_options.BaseUrl}/menu/items/{id}/availability";
            _logger.LogInformation("Toggling availability for menu item {Id}: {IsAvailable}", id, isAvailable);

            var requestBody = new { is_available = isAvailable };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PatchAsync(endpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to toggle availability (HTTP {StatusCode}): {Response}",
                    response.StatusCode, responseContent);
                return false;
            }

            _logger.LogInformation("Menu item {Id} availability toggled to {IsAvailable}", id, isAvailable);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling availability for menu item {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeleteItemAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"{_options.BaseUrl}/menu/items/{id}";
            _logger.LogInformation("Deleting menu item {Id}", id);

            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to delete menu item (HTTP {StatusCode}): {Response}",
                    response.StatusCode, responseContent);
                return false;
            }

            _logger.LogInformation("Menu item {Id} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu item {Id}", id);
            return false;
        }
    }

    private MenuCategory? ParseCategory(JsonElement element, out int rejectedItemCount, out List<string> rejectedExamples, out List<string> rejectedReasonCodes)
    {
        rejectedItemCount = 0;
        rejectedExamples = new List<string>();
        rejectedReasonCodes = new List<string>();

        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetStringProperty(element, "name", out var name) ||
            !JsonParserHelper.TryGetIntProperty(element, "sort_order", out var sortOrder))
        {
            return null;
        }

        var items = new List<MenuItem>();
        if (element.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var itemElement in itemsElement.EnumerateArray())
            {
                if (TryParseItem(itemElement, out var item, out var reason))
                {
                    if (item is not null) items.Add(item);
                }
                else
                {
                    rejectedItemCount++;
                    var reasonCode = GetReasonCode(reason);
                    if (!rejectedReasonCodes.Contains(reasonCode)) rejectedReasonCodes.Add(reasonCode);
                    if (rejectedExamples.Count < 5)
                    {
                        var raw = itemElement.GetRawText();
                        rejectedExamples.Add($"Reason: {reason}; Raw: { (raw.Length > 300 ? raw.Substring(0, 300) : raw) }");
                    }
                }
            }
        }

        return new MenuCategory(id, name ?? string.Empty, sortOrder, items);
    }

    private bool TryParseItem(JsonElement element, out MenuItem? item, out string? reason)
    {
        item = null;
        reason = null;

        // id
        if (!element.TryGetProperty("id", out var idProp))
        {
            reason = "missing id";
            return false;
        }
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id))
        {
            reason = "id wrong type";
            return false;
        }

        // name
        if (!element.TryGetProperty("name", out var nameProp))
        {
            reason = "missing name";
            return false;
        }
        if (!JsonParserHelper.TryGetStringProperty(element, "name", out var name))
        {
            reason = "name wrong type or empty";
            return false;
        }

        // price
        if (!element.TryGetProperty("price", out var priceProp))
        {
            reason = "missing price";
            return false;
        }
        if (!JsonParserHelper.TryGetDecimalProperty(element, "price", out var price))
        {
            var raw = priceProp.ValueKind == JsonValueKind.String ? priceProp.GetString() : priceProp.GetRawText();
            reason = $"price parse error or wrong type (raw: {raw})";
            return false;
        }

        // availability
        var hasIsAvailable = JsonParserHelper.TryGetBoolProperty(element, "is_available", out var isAvailable1);
        var hasAvailable = JsonParserHelper.TryGetBoolProperty(element, "available", out var isAvailable2);
        if (!hasIsAvailable && !hasAvailable)
        {
            reason = "missing availability flag";
            return false;
        }
        var isAvailable = hasIsAvailable ? isAvailable1 : isAvailable2;

        var description = JsonParserHelper.TryGetStringProperty(element, "description", out var desc) ? desc ?? string.Empty : string.Empty;
        var photoUrl = JsonParserHelper.TryGetStringProperty(element, "photo_url", out var photo) ? photo : null;
        var menuCategoryId = JsonParserHelper.TryGetIntProperty(element, "menu_category_id", out var catId) ? catId : 0;
        var categoryName = JsonParserHelper.TryGetNestedStringProperty(element, "category", "name", out var catName) ? catName : null;

        item = new MenuItem(id, name ?? string.Empty, description, price, photoUrl, isAvailable, menuCategoryId, categoryName);
        return true;
    }

    private static string GetReasonCode(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return "UNKNOWN";
        reason = reason.ToLowerInvariant();
        if (reason.Contains("missing id")) return "MISSING_ID";
        if (reason.Contains("missing name")) return "MISSING_NAME";
        if (reason.Contains("missing price")) return "MISSING_PRICE";
        if (reason.Contains("price parse")) return "PRICE_PARSE_ERROR";
        if (reason.Contains("availability")) return "MISSING_AVAILABILITY";
        if (reason.Contains("id wrong type")) return "ID_WRONG_TYPE";
        if (reason.Contains("name wrong")) return "NAME_WRONG_TYPE";
        return "OTHER_PARSE_ERROR";
    }

    private static MenuItem? ParseMenuItem(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id))
            return null;

        if (!JsonParserHelper.TryGetStringProperty(element, "name", out var name))
            return null;

        if (!JsonParserHelper.TryGetDecimalProperty(element, "price", out var price))
            return null;

        var hasIsAvailable = JsonParserHelper.TryGetBoolProperty(element, "is_available", out var isAvailable1);
        var hasAvailable = JsonParserHelper.TryGetBoolProperty(element, "available", out var isAvailable2);
        if (!hasIsAvailable && !hasAvailable)
            return null;
        var isAvailable = hasIsAvailable ? isAvailable1 : isAvailable2;

        var description = JsonParserHelper.TryGetStringProperty(element, "description", out var desc) ? desc ?? string.Empty : string.Empty;
        var photoUrl = JsonParserHelper.TryGetStringProperty(element, "photo_url", out var photo) ? photo : null;
        var menuCategoryId = JsonParserHelper.TryGetIntProperty(element, "menu_category_id", out var catId) ? catId : 0;
        var categoryName = JsonParserHelper.TryGetNestedStringProperty(element, "category", "name", out var catName) ? catName : null;

        return new MenuItem(id, name ?? string.Empty, description, price, photoUrl, isAvailable, menuCategoryId, categoryName);
    }
}
