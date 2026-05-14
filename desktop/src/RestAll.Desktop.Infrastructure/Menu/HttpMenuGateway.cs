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
            foreach (var element in data.EnumerateArray())
            {
                var category = ParseCategory(element);
                if (category is not null)
                {
                    categories.Add(category);
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

    private MenuCategory? ParseCategory(JsonElement element)
    {
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
                var item = ParseItem(itemElement);
                if (item is not null)
                {
                    items.Add(item);
                }
            }
        }

        return new MenuCategory(id, name ?? string.Empty, sortOrder, items);
    }

    private MenuItem? ParseItem(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetStringProperty(element, "name", out var name) ||
            !JsonParserHelper.TryGetDecimalProperty(element, "price", out var price) ||
            (!JsonParserHelper.TryGetBoolProperty(element, "is_available", out var isAvailable) &&
             !JsonParserHelper.TryGetBoolProperty(element, "available", out isAvailable)))
        {
            return null;
        }

        var description = JsonParserHelper.TryGetStringProperty(element, "description", out var desc) ? desc ?? string.Empty : string.Empty;
        var photoUrl = JsonParserHelper.TryGetStringProperty(element, "photo_url", out var photo) ? photo : null;
        var menuCategoryId = JsonParserHelper.TryGetIntProperty(element, "menu_category_id", out var catId) ? catId : 0;
        var categoryName = JsonParserHelper.TryGetNestedStringProperty(element, "category", "name", out var catName) ? catName : null;

        return new MenuItem(id, name ?? string.Empty, description, price, photoUrl, isAvailable, menuCategoryId, categoryName);
    }
}
