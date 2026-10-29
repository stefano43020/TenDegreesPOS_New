using System;
using System.Collections.Generic;

namespace TenDegreesPOS_New.Services
{
    public static class UnitConverter
    {
        // Base unit is grams (g) for weight, milliliters (ml) for volume, pieces (pcs) for count.

        private static readonly Dictionary<string, decimal> ToGrams = new(StringComparer.OrdinalIgnoreCase)
        {
            { "g", 1m }, { "kg", 1000m }, { "mg", 0.001m }, { "oz", 28.3495m }, { "lb", 453.592m }
        };

        private static readonly Dictionary<string, decimal> ToMilliliters = new(StringComparer.OrdinalIgnoreCase)
        {
            { "ml", 1m }, { "l", 1000m }, { "fl oz", 29.5735m }, { "tsp", 4.92892m }, { "tbsp", 14.7868m }, { "cup", 236.588m }
        };

        private static readonly Dictionary<string, decimal> ToPieces = new(StringComparer.OrdinalIgnoreCase)
        {
            { "pcs", 1m }, { "pc", 1m }, { "piece", 1m }
        };

        public static string InferCategory(string? unit)
        {
            if (string.IsNullOrWhiteSpace(unit)) return "count";
            var u = unit.Trim();
            if (ToGrams.ContainsKey(u)) return "weight";
            if (ToMilliliters.ContainsKey(u)) return "volume";
            if (ToPieces.ContainsKey(u)) return "count";
            return "count";
        }

        public static decimal ConvertToBase(decimal quantity, string fromUnit, string baseUnit)
        {
            string category = InferCategory(baseUnit);
            string fromCat = InferCategory(fromUnit);

            if (category != fromCat)
                throw new InvalidOperationException($"Cannot convert {fromUnit} to {baseUnit} (different categories).");

            if (string.Equals(fromUnit, baseUnit, StringComparison.OrdinalIgnoreCase))
                return quantity;

            decimal canonical = ToCanonical(quantity, fromUnit, category);
            decimal divisor = ToCanonical(1m, baseUnit, category);

            return canonical / divisor;
        }

        private static decimal ToCanonical(decimal quantity, string unit, string category)
        {
            if (category == "weight" && ToGrams.TryGetValue(unit, out var w)) return quantity * w;
            if (category == "volume" && ToMilliliters.TryGetValue(unit, out var v)) return quantity * v;
            if (category == "count" && ToPieces.TryGetValue(unit, out var p)) return quantity * p;

            throw new InvalidOperationException($"Unknown unit '{unit}' for category {category}.");
        }

        public static List<string> GetUnitsForCategory(string category)
        {
            return category switch
            {
                "weight" => new List<string> { "g", "kg", "mg", "oz", "lb" },
                "volume" => new List<string> { "ml", "L", "fl oz", "tsp", "tbsp", "cup" },
                _ => new List<string> { "pcs" }
            };
        }

        public static string GetDefaultBaseUnit(string category)
        {
            return category switch
            {
                "weight" => "g",
                "volume" => "ml",
                _ => "pcs"
            };
        }

        public static bool IsValidConversion(string fromUnit, string toUnit)
        {
            return InferCategory(fromUnit) == InferCategory(toUnit);
        }
    }
}