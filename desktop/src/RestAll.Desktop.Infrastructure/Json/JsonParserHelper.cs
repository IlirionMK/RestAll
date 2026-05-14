using System.Globalization;
using System.Text.Json;

namespace RestAll.Desktop.Infrastructure.Json;

public static class JsonParserHelper
{
    public static bool TryGetStringProperty(JsonElement element, string propertyName, out string? value)
    {
        value = null;
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return !string.IsNullOrEmpty(value);
        }
        return false;
    }

    public static bool TryGetIntProperty(JsonElement element, string propertyName, out int value)
    {
        value = default;
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
        {
            value = property.GetInt32();
            return true;
        }
        return false;
    }

    public static bool TryGetDecimalProperty(JsonElement element, string propertyName, out decimal value)
    {
        value = default;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            value = property.GetDecimal();
            return true;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var raw = property.GetString();
            if (!string.IsNullOrWhiteSpace(raw) &&
                decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryGetBoolProperty(JsonElement element, string propertyName, out bool value)
    {
        value = default;
        if (element.TryGetProperty(propertyName, out var property) &&
            (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False))
        {
            value = property.GetBoolean();
            return true;
        }

        return false;
    }

    public static bool TryGetNestedStringProperty(JsonElement element, string parentPropertyName, string childPropertyName, out string? value)
    {
        value = null;
        if (element.TryGetProperty(parentPropertyName, out var parent) && parent.ValueKind == JsonValueKind.Object)
        {
            return TryGetStringProperty(parent, childPropertyName, out value);
        }
        return false;
    }
}
