namespace TheMetricConvert.Api;

/// <summary>
/// Broad unit families supported by The Metric Convert.
/// </summary>
public enum UnitCategory
{
    /// <summary>Distance and size (base: meter).</summary>
    Length,

    /// <summary>Weight and mass (base: gram).</summary>
    Mass,

    /// <summary>Liquid volume (base: liter).</summary>
    Volume,

    /// <summary>Temperature (base: celsius; handled with offsets).</summary>
    Temperature,
}

/// <summary>
/// The "system" a unit is commonly associated with.
/// </summary>
public enum UnitSystem
{
    /// <summary>Metric system units/prefixes.</summary>
    Metric,

    /// <summary>US/Imperial customary units.</summary>
    Imperial,

    /// <summary>Other/uncategorized (reserved for future expansion).</summary>
    Other,
}

/// <summary>
/// Describes a unit and how it relates to its category's base unit.
/// </summary>
/// <param name="Symbol">Short symbol used in requests (e.g. "cm", "ft").</param>
/// <param name="Name">Human-readable unit name (e.g. "centimeter").</param>
/// <param name="Category">Unit category (length/mass/etc.).</param>
/// <param name="System">Metric vs Imperial.</param>
/// <param name="FactorToBase">
/// For linear units, multiply a value in this unit by <paramref name="FactorToBase"/> to get the base unit.
/// </param>
/// <param name="BaseSymbol">The base unit symbol for this category (e.g. "m", "g", "L").</param>
/// <param name="PowerOfTen">
/// For metric prefix learning: exponent relative to base (e.g. cm = -2 because 10^-2 m).
/// </param>
public sealed record UnitDefinition(
    string Symbol,
    string Name,
    UnitCategory Category,
    UnitSystem System,
    double FactorToBase,
    string BaseSymbol,
    int? PowerOfTen = null);

/// <summary>
/// A conversion request.
/// </summary>
/// <param name="From">Unit symbol to convert from (e.g. "cm").</param>
/// <param name="To">Unit symbol to convert to (e.g. "m").</param>
/// <param name="Value">Numeric value in the <paramref name="From"/> unit.</param>
public sealed record ConvertRequest(
    string From,
    string To,
    double Value);

/// <summary>
/// Conversion response including educational steps and a learning tip when available.
/// </summary>
public sealed record ConvertResult(
    bool IsOk,
    string? Error,
    ConvertRequest? Input,
    double? OutputValue,
    string? OutputUnit,
    IReadOnlyList<string> Steps,
    string? Tip);
