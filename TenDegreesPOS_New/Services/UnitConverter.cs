using System;
using System.Collections.Generic;

namespace TenDegreesPOS_New.Services
{
    public static class UnitConverter
    {
        // Weight conversions (to grams)
        private static readonly Dictionary<string, decimal> ToGrams = new Dictionary<string, decimal>
        {
            { "g", 1m },
            { "kg", 1000m },
            { "tsp", 5m },      // 1 teaspoon = 5g (water/density dependent)
            { "tbsp", 15m },    // 1 tablespoon = 15g
            { "can", 400m }     // 1 standard can = 400g (adjustable)
        };

        // Volume conversions (to milliliters)
        private static readonly Dictionary<string, decimal> ToMilliliters = new Dictionary<string, decimal>
        {
            { "ml", 1m },
            { "L", 1000m },
            { "tsp", 5m },      // 1 teaspoon = 5ml
            { "tbsp", 15m },    // 1 tablespoon = 15ml
            { "can", 400m }     // 1 standard can = 400ml
        };

        public static decimal ConvertToGrams(decimal quantity, string fromUnit)
        {
            fromUnit = fromUnit.ToLower();
            if (ToGrams.ContainsKey(fromUnit))
                return quantity * ToGrams[fromUnit];
            return quantity; // Unknown unit, return as-is
        }

        public static decimal ConvertToMilliliters(decimal quantity, string fromUnit)
        {
            fromUnit = fromUnit.ToLower();
            if (ToMilliliters.ContainsKey(fromUnit))
                return quantity * ToMilliliters[fromUnit];
            return quantity;
        }

        public static decimal Convert(decimal quantity, string fromUnit, string toUnit)
        {
            // If units are the same, no conversion needed
            if (fromUnit.ToLower() == toUnit.ToLower())
                return quantity;

            // Weight-based conversions (g, kg, tsp, tbsp, can -> g or kg)
            if (IsWeightUnit(toUnit))
            {
                decimal grams = ConvertToGrams(quantity, fromUnit);
                if (toUnit.ToLower() == "kg")
                    return grams / 1000m;
                return grams;
            }

            // Volume-based conversions (ml, L, tsp, tbsp, can -> ml or L)
            if (IsVolumeUnit(toUnit))
            {
                decimal ml = ConvertToMilliliters(quantity, fromUnit);
                if (toUnit.ToLower() == "l")
                    return ml / 1000m;
                return ml;
            }

            // Unit doesn't match, return original
            return quantity;
        }

        private static bool IsWeightUnit(string unit)
        {
            unit = unit.ToLower();
            return unit == "g" || unit == "kg" || unit == "tsp" || unit == "tbsp" || unit == "can";
        }

        private static bool IsVolumeUnit(string unit)
        {
            unit = unit.ToLower();
            return unit == "ml" || unit == "l" || unit == "tsp" || unit == "tbsp" || unit == "can";
        }

        public static List<string> GetWeightUnits()
        {
            return new List<string> { "g", "kg", "tsp", "tbsp", "can" };
        }

        public static List<string> GetVolumeUnits()
        {
            return new List<string> { "ml", "L", "tsp", "tbsp", "can" };
        }

        public static List<string> GetAllUnits()
        {
            return new List<string> { "g", "kg", "ml", "L", "pcs", "can", "tsp", "tbsp" };
        }
    }
}