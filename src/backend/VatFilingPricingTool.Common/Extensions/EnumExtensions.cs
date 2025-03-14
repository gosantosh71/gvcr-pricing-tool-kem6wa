using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace VatFilingPricingTool.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for enum types to enhance their functionality throughout the VAT Filing Pricing Tool application.
    /// These methods facilitate common operations like converting enums to display strings, parsing string values to enums,
    /// and retrieving descriptions from enum values.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description of an enum value from its Description attribute.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The description from the Description attribute, or the enum name if no attribute exists.</returns>
        public static string GetDescription(this Enum value)
        {
            // Get the FieldInfo for the enum value using reflection
            FieldInfo field = value.GetType().GetField(value.ToString());

            // Check if the FieldInfo has a DescriptionAttribute
            if (field?.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] descriptionAttributes && descriptionAttributes.Length > 0)
            {
                // If it does, return the Description property value
                return descriptionAttributes[0].Description;
            }

            // If not, check if it has a DisplayAttribute and return its Name property
            if (field?.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] displayAttributes && displayAttributes.Length > 0)
            {
                return displayAttributes[0].Name;
            }

            // If neither attribute exists, return the enum value's name as a string
            return value.ToString();
        }

        /// <summary>
        /// Gets the display name of an enum value from its Display attribute.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The name from the Display attribute, or the enum name if no attribute exists.</returns>
        public static string GetDisplayName(this Enum value)
        {
            // Get the FieldInfo for the enum value using reflection
            FieldInfo field = value.GetType().GetField(value.ToString());

            // Check if the FieldInfo has a DisplayAttribute
            if (field?.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] displayAttributes && displayAttributes.Length > 0)
            {
                // If it does, return the Name property value
                return displayAttributes[0].Name;
            }

            // If not, check if it has a DescriptionAttribute and return its Description property
            if (field?.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] descriptionAttributes && descriptionAttributes.Length > 0)
            {
                return descriptionAttributes[0].Description;
            }

            // If neither attribute exists, return the enum value's name as a string
            return value.ToString();
        }

        /// <summary>
        /// Converts an enum type to a list of its values.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>A list containing all values of the enum type T.</returns>
        public static List<T> ToList<T>() where T : Enum
        {
            // Use Enum.GetValues to get all values of the enum type T
            // Convert the result to a List<T>
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        /// <summary>
        /// Converts an enum type to a list of key-value pairs for dropdown lists.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>A list of key-value pairs where the key is the enum value and the value is the display name.</returns>
        public static List<KeyValuePair<int, string>> ToSelectList<T>() where T : Enum
        {
            // Use Enum.GetValues to get all values of the enum type T
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            
            // For each enum value, create a KeyValuePair with the integer value and display name
            return values.Select(v => new KeyValuePair<int, string>(
                Convert.ToInt32(v), 
                ((Enum)(object)v).GetDisplayName()
            )).ToList();
        }

        /// <summary>
        /// Parses a string to an enum value of type T.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The enum value corresponding to the string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the value is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the value cannot be parsed to the enum type.</exception>
        public static T Parse<T>(string value) where T : Enum
        {
            // Check if the string is null or empty and throw an exception if it is
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null or empty.");
            }

            // Use Enum.Parse to convert the string to an enum value of type T
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Attempts to parse a string to an enum value of type T.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="result">When this method returns, contains the parsed enum value if parsing succeeded, or the default value of T if parsing failed.</param>
        /// <returns>True if parsing was successful, otherwise false.</returns>
        public static bool TryParse<T>(string value, out T result) where T : Enum
        {
            // Check if the string is null or empty and return false if it is
            if (string.IsNullOrEmpty(value))
            {
                result = default;
                return false;
            }

            // Use Enum.TryParse to attempt to convert the string to an enum value of type T
            // Set the out parameter result to the parsed value if successful
            return Enum.TryParse(value, true, out result);
        }

        /// <summary>
        /// Gets all values of an enum type as an array.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>An array containing all values of the enum type T.</returns>
        public static T[] GetValues<T>() where T : Enum
        {
            // Use Enum.GetValues to get all values of the enum type T
            // Cast the result to T[]
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        /// <summary>
        /// Gets all names of an enum type as an array of strings.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>An array containing all names of the enum type T.</returns>
        public static string[] GetNames<T>() where T : Enum
        {
            // Use Enum.GetNames to get all names of the enum type T
            return Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// Gets all display names of an enum type as an array of strings.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>An array containing all display names of the enum type T.</returns>
        public static string[] GetDisplayNames<T>() where T : Enum
        {
            // Use Enum.GetValues to get all values of the enum type T
            var values = Enum.GetValues(typeof(T)).Cast<Enum>();
            
            // For each enum value, get its display name using GetDisplayName
            return values.Select(v => v.GetDisplayName()).ToArray();
        }

        /// <summary>
        /// Checks if an enum value has a specific flag set (for flag enums).
        /// </summary>
        /// <param name="value">The enum value to check.</param>
        /// <param name="flag">The flag to check for.</param>
        /// <returns>True if the flag is set, otherwise false.</returns>
        public static bool HasFlag(this Enum value, Enum flag)
        {
            // Convert both enum values to their underlying integer values
            var valueInt = Convert.ToInt64(value);
            var flagInt = Convert.ToInt64(flag);
            
            // Perform a bitwise AND operation
            // Return true if the result equals the flag's integer value, otherwise false
            return (valueInt & flagInt) == flagInt;
        }
    }
}