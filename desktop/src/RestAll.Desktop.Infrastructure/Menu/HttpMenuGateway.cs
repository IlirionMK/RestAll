using System.Text;
using System.Text.Json;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Menu;

public sealed class HttpMenuGateway : IMenuGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;

    public HttpMenuGateway(HttpClient httpClient, RestAllApiOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<List<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/menu/categories", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new List<MenuCategory>();
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Array)
            {
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

            return categories;
        }
        catch
        {
            return new List<MenuCategory>();
        }
    }

    public async Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/menu/items", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new List<MenuItem>();
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Array)
            {
                return new List<MenuItem>();
            }

            var items = new List<MenuItem>();
            foreach (var element in data.EnumerateArray())
            {
                var item = ParseItem(element);
                if (item is not null)
                {
                    items.Add(item);
                }
            }

            return items;
        }
        catch
        {
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
