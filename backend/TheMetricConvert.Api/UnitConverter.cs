namespace TheMetricConvert.Api;

/// <summary>
/// Conversion engine used by the API.
/// </summary>
/// <remarks>
/// This is intentionally "teaching-first": we return human-readable steps and small tips
/// (especially for metric-prefix-to-metric-prefix conversions) to reinforce intuition.
/// </remarks>
public static class UnitConverter
{
    /// <summary>
    /// Converts a value from one unit to another.
    /// </summary>
    /// <remarks>
    /// For linear units (length/mass/volume), we convert via the category base unit using a factor.
    /// For temperature, we convert via Celsius using offset formulas.
    /// </remarks>
    public static ConvertResult Convert(ConvertRequest request)
    {
        if (request is null)
        {
            return new ConvertResult(
                IsOk: false,
                Error: "Request body is required.",
                Input: null,
                OutputValue: null,
                OutputUnit: null,
                Steps: Array.Empty<string>(),
                Tip: null);
        }

        if (string.IsNullOrWhiteSpace(request.From) || string.IsNullOrWhiteSpace(request.To))
        {
            return Fail(request, "Both 'from' and 'to' unit symbols are required.");
        }

        if (double.IsNaN(request.Value) || double.IsInfinity(request.Value))
        {
            return Fail(request, "'value' must be a finite number.");
        }

        // Normalize lookup by symbol (case-insensitive); the catalog owns which symbols are allowed.
        if (!UnitCatalog.TryGet(request.From, out var fromUnit))
        {
            return Fail(request, $"Unknown unit '{request.From}'. Try calling GET /api/units.");
        }

        if (!UnitCatalog.TryGet(request.To, out var toUnit))
        {
            return Fail(request, $"Unknown unit '{request.To}'. Try calling GET /api/units.");
        }

        // Prevent nonsensical conversions (e.g. meters -> grams).
        if (fromUnit.Category != toUnit.Category)
        {
            return Fail(request, $"Cannot convert {fromUnit.Category} to {toUnit.Category}.");
        }

        // Temperature is non-linear (offset + scale), so it needs a dedicated path.
        return fromUnit.Category == UnitCategory.Temperature
            ? ConvertTemperature(request, fromUnit.Symbol, toUnit.Symbol)
            : ConvertLinear(request, fromUnit, toUnit);
    }

    private static ConvertResult ConvertLinear(ConvertRequest input, UnitDefinition fromUnit, UnitDefinition toUnit)
    {
        var steps = new List<string>();
        var value = input.Value;

        steps.Add($"Start: {value} {fromUnit.Symbol}");

        var baseValue = value * fromUnit.FactorToBase;
        steps.Add($"To base ({fromUnit.BaseSymbol}): {value} × {fromUnit.FactorToBase} = {baseValue} {fromUnit.BaseSymbol}");

        var output = baseValue / toUnit.FactorToBase;
        steps.Add($"To target ({toUnit.Symbol}): {baseValue} ÷ {toUnit.FactorToBase} = {output} {toUnit.Symbol}");

        var tip = BuildPowerOfTenTip(fromUnit, toUnit);

        return new ConvertResult(
            IsOk: true,
            Error: null,
            Input: input,
            OutputValue: output,
            OutputUnit: toUnit.Symbol,
            Steps: steps,
            Tip: tip);
    }

    private static ConvertResult ConvertTemperature(ConvertRequest input, string fromSymbol, string toSymbol)
    {
        var steps = new List<string>();
        var v = input.Value;

        steps.Add($"Start: {v} {fromSymbol}");

        // Temperature conversions are most understandable via a common reference point.
        // We use Celsius as the base because it's the everyday metric reference.
        double celsius = fromSymbol.ToUpperInvariant() switch
        {
            "C" => v,
            "F" => (v - 32.0) * (5.0 / 9.0),
            "K" => v - 273.15,
            _ => double.NaN,
        };

        if (double.IsNaN(celsius))
        {
            return Fail(input, $"Unsupported temperature unit '{fromSymbol}'.");
        }

        steps.Add($"To base (C): {celsius} C");

        // Convert from Celsius to the target scale.
        double output = toSymbol.ToUpperInvariant() switch
        {
            "C" => celsius,
            "F" => (celsius * (9.0 / 5.0)) + 32.0,
            "K" => celsius + 273.15,
            _ => double.NaN,
        };

        if (double.IsNaN(output))
        {
            return Fail(input, $"Unsupported temperature unit '{toSymbol}'.");
        }

        steps.Add($"To target ({toSymbol}): {output} {toSymbol}");

        var tip = "Temperature conversions aren't simple powers of ten because they include an offset (like +32 or +273.15).";

        return new ConvertResult(
            IsOk: true,
            Error: null,
            Input: input,
            OutputValue: output,
            OutputUnit: toSymbol,
            Steps: steps,
            Tip: tip);
    }

    private static string? BuildPowerOfTenTip(UnitDefinition fromUnit, UnitDefinition toUnit)
    {
        if (fromUnit.System != UnitSystem.Metric || toUnit.System != UnitSystem.Metric)
        {
            return null;
        }

        if (fromUnit.PowerOfTen is null || toUnit.PowerOfTen is null)
        {
            return null;
        }

        var delta = toUnit.PowerOfTen.Value - fromUnit.PowerOfTen.Value;
        if (delta == 0)
        {
            return "Same metric prefix: no decimal shift needed.";
        }

        var direction = delta > 0 ? "left" : "right";
        var places = Math.Abs(delta);

        return $"Metric tip: going from {fromUnit.Symbol} to {toUnit.Symbol} shifts the decimal {direction} by {places} place(s).";
    }

    private static ConvertResult Fail(ConvertRequest input, string message) =>
        new(
            IsOk: false,
            Error: message,
            Input: input,
            OutputValue: null,
            OutputUnit: null,
            Steps: Array.Empty<string>(),
            Tip: null);
}
