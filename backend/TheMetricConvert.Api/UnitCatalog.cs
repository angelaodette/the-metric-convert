namespace TheMetricConvert.Api;

/// <summary>
/// Central catalog of supported units.
/// </summary>
/// <remarks>
/// This is intentionally small to start; adding more units is a straightforward data expansion.
/// For linear categories we store a factor to a chosen base unit (m, g, L). Temperature is handled
/// specially in <see cref="UnitConverter"/> because it is not linear.
/// </remarks>
public static class UnitCatalog
{
    private static readonly Dictionary<string, UnitDefinition> BySymbol =
        All!.ToDictionary(u => u.Symbol, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// All supported units.
    /// </summary>
    public static IReadOnlyList<UnitDefinition> All { get; } = new List<UnitDefinition>
    {
        // Length (base: m)
        new("mm", "millimeter", UnitCategory.Length, UnitSystem.Metric, 1e-3, "m", -3),
        new("cm", "centimeter", UnitCategory.Length, UnitSystem.Metric, 1e-2, "m", -2),
        new("m", "meter", UnitCategory.Length, UnitSystem.Metric, 1, "m", 0),
        new("km", "kilometer", UnitCategory.Length, UnitSystem.Metric, 1e3, "m", 3),
        new("in", "inch", UnitCategory.Length, UnitSystem.Imperial, 0.0254, "m"),
        new("ft", "foot", UnitCategory.Length, UnitSystem.Imperial, 0.3048, "m"),
        new("yd", "yard", UnitCategory.Length, UnitSystem.Imperial, 0.9144, "m"),
        new("mi", "mile", UnitCategory.Length, UnitSystem.Imperial, 1609.344, "m"),

        // Mass (base: g)
        new("mg", "milligram", UnitCategory.Mass, UnitSystem.Metric, 1e-3, "g", -3),
        new("g", "gram", UnitCategory.Mass, UnitSystem.Metric, 1, "g", 0),
        new("kg", "kilogram", UnitCategory.Mass, UnitSystem.Metric, 1e3, "g", 3),
        new("oz", "ounce", UnitCategory.Mass, UnitSystem.Imperial, 28.349523125, "g"),
        new("lb", "pound", UnitCategory.Mass, UnitSystem.Imperial, 453.59237, "g"),

        // Volume (base: L)
        new("mL", "milliliter", UnitCategory.Volume, UnitSystem.Metric, 1e-3, "L", -3),
        new("L", "liter", UnitCategory.Volume, UnitSystem.Metric, 1, "L", 0),
        new("tsp", "teaspoon", UnitCategory.Volume, UnitSystem.Imperial, 0.00492892159375, "L"),
        new("tbsp", "tablespoon", UnitCategory.Volume, UnitSystem.Imperial, 0.01478676478125, "L"),
        new("fl oz", "fluid ounce", UnitCategory.Volume, UnitSystem.Imperial, 0.0295735295625, "L"),
        new("cup", "cup", UnitCategory.Volume, UnitSystem.Imperial, 0.2365882365, "L"),
        new("pt", "pint", UnitCategory.Volume, UnitSystem.Imperial, 0.473176473, "L"),
        new("qt", "quart", UnitCategory.Volume, UnitSystem.Imperial, 0.946352946, "L"),
        new("gal", "gallon", UnitCategory.Volume, UnitSystem.Imperial, 3.785411784, "L"),

        // Temperature (base: C)
        new("C", "celsius", UnitCategory.Temperature, UnitSystem.Metric, 1, "C"),
        new("F", "fahrenheit", UnitCategory.Temperature, UnitSystem.Imperial, 1, "C"),
        new("K", "kelvin", UnitCategory.Temperature, UnitSystem.Metric, 1, "C"),
    };

    /// <summary>
    /// Looks up a unit by symbol (case-insensitive).
    /// </summary>
    public static bool TryGet(string symbol, out UnitDefinition unit) =>
        BySymbol.TryGetValue(symbol.Trim(), out unit!);
}
