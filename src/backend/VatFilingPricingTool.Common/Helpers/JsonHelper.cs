using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging; // version 6.0.0
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Common.Extensions;

namespace VatFilingPricingTool.Common.Helpers
{
    /// <summary>
    /// Static helper class that provides utility methods for JSON serialization and deserialization operations
    /// throughout the VAT Filing Pricing Tool application.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Default JSON serializer options used throughout the application.
        /// </summary>
        public static JsonSerializerOptions DefaultSerializerOptions { get; } = CreateDefaultOptions();

        /// <summary>
        /// Creates a JsonSerializerOptions object with default settings for the application.
        /// </summary>
        /// <returns>JsonSerializerOptions with default settings.</returns>
        public static JsonSerializerOptions CreateDefaultOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            };
            
            return options;
        }

        /// <summary>
        /// Serializes an object to a JSON string using default or custom options.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="options">Custom serialization options, or null to use default options.</param>
        /// <returns>JSON string representation of the object.</returns>
        public static string Serialize(object value, JsonSerializerOptions options = null)
        {
            if (value == null)
                return null;

            options ??= DefaultSerializerOptions;
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of type T using default or custom options.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="options">Custom serialization options, or null to use default options.</param>
        /// <returns>Deserialized object of type T.</returns>
        public static T Deserialize<T>(string json, JsonSerializerOptions options = null)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            options ??= DefaultSerializerOptions;
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Attempts to serialize an object to a JSON string, returning a Result with the JSON string or error details.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="options">Custom serialization options, or null to use default options.</param>
        /// <returns>Result containing the JSON string or error details.</returns>
        public static Result<string> TrySerialize(object value, JsonSerializerOptions options = null)
        {
            if (value == null)
                return Result<string>.Success(null);

            options ??= DefaultSerializerOptions;

            try
            {
                string json = JsonSerializer.Serialize(value, options);
                return Result<string>.Success(json);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(
                    $"JSON serialization failed: {ex.GetDetailedMessage()}", 
                    ex.GetErrorCode());
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an object of type T, returning a Result with the object or error details.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="options">Custom serialization options, or null to use default options.</param>
        /// <returns>Result containing the deserialized object or error details.</returns>
        public static Result<T> TryDeserialize<T>(string json, JsonSerializerOptions options = null)
        {
            if (string.IsNullOrEmpty(json))
                return Result<T>.Failure("JSON string is null or empty");

            options ??= DefaultSerializerOptions;

            try
            {
                T result = JsonSerializer.Deserialize<T>(json, options);
                return Result<T>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(
                    $"JSON deserialization failed: {ex.GetDetailedMessage()}", 
                    ex.GetErrorCode());
            }
        }

        /// <summary>
        /// Serializes an object to a UTF-8 encoded byte array.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="options">Custom serialization options, or null to use default options.</param>
        /// <returns>UTF-8 encoded byte array containing the JSON representation.</returns>
        public static byte[] SerializeToBytes(object value, JsonSerializerOptions options = null)
        {
            if (value == null)
                return null;

            options ??= DefaultSerializerOptions;
            return JsonSerializer.SerializeToUtf8Bytes(value, options);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded byte array to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="utf8Json">The UTF-8 encoded byte array to deserialize.</param>
        /// <param name="options">Custom serialization options, or null to use default options.</param>
        /// <returns>Deserialized object of type T.</returns>
        public static T DeserializeFromBytes<T>(byte[] utf8Json, JsonSerializerOptions options = null)
        {
            if (utf8Json == null || utf8Json.Length == 0)
                return default;

            options ??= DefaultSerializerOptions;
            return JsonSerializer.Deserialize<T>(utf8Json, options);
        }

        /// <summary>
        /// Checks if a string is valid JSON.
        /// </summary>
        /// <param name="json">The string to check.</param>
        /// <returns>True if the string is valid JSON, false otherwise.</returns>
        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                using (JsonDocument.Parse(json))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts a property value from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="propertyName">The name of the property to extract.</param>
        /// <returns>The property value as a string, or null if not found.</returns>
        public static string GetJsonProperty(string json, string propertyName)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(propertyName))
                return null;

            try
            {
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    if (document.RootElement.TryGetProperty(propertyName, out JsonElement property))
                    {
                        return property.ToString();
                    }
                }
            }
            catch
            {
                // Silently handle any parsing errors
            }

            return null;
        }

        /// <summary>
        /// Extracts a property value from a JSON string and converts it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the property value to.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <param name="propertyName">The name of the property to extract.</param>
        /// <returns>The property value converted to type T, or default(T) if not found.</returns>
        public static T GetJsonPropertyTyped<T>(string json, string propertyName)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(propertyName))
                return default;

            try
            {
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    if (document.RootElement.TryGetProperty(propertyName, out JsonElement property))
                    {
                        return JsonSerializer.Deserialize<T>(property.GetRawText(), DefaultSerializerOptions);
                    }
                }
            }
            catch
            {
                // Silently handle any parsing or conversion errors
            }

            return default;
        }

        /// <summary>
        /// Merges two JSON objects, with the second object's properties overriding the first.
        /// </summary>
        /// <param name="json1">The first JSON object.</param>
        /// <param name="json2">The second JSON object, whose properties take precedence.</param>
        /// <returns>Merged JSON string.</returns>
        public static string MergeJsonObjects(string json1, string json2)
        {
            if (string.IsNullOrEmpty(json1))
                return json2;

            if (string.IsNullOrEmpty(json2))
                return json1;

            try
            {
                // Use memory streams and Utf8JsonWriter for efficient JSON manipulation
                using var ms = new MemoryStream();
                using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions 
                { 
                    Indented = DefaultSerializerOptions.WriteIndented 
                });
                
                // Parse the JSON documents
                using var doc1 = JsonDocument.Parse(json1);
                using var doc2 = JsonDocument.Parse(json2);
                
                // Track properties we've already written to avoid duplicates
                var processedProperties = new HashSet<string>();
                
                writer.WriteStartObject();
                
                // Add properties from the first document
                foreach (var property in doc1.RootElement.EnumerateObject())
                {
                    property.WriteTo(writer);
                    processedProperties.Add(property.Name);
                }
                
                // Add properties from the second document (overriding any from the first)
                foreach (var property in doc2.RootElement.EnumerateObject())
                {
                    // Only write properties that weren't in the first document
                    // Properties from the first document will already have been written
                    if (!processedProperties.Contains(property.Name))
                    {
                        property.WriteTo(writer);
                    }
                }
                
                writer.WriteEndObject();
                writer.Flush();
                
                // Get the resulting JSON string
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception)
            {
                // In case of error, return the second JSON if it's valid, otherwise the first
                if (IsValidJson(json2))
                    return json2;
                return json1;
            }
        }
    }
}