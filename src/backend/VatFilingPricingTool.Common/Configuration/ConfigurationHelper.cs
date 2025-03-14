using Microsoft.Extensions.Configuration; // Version 6.0.0
using Microsoft.Extensions.Configuration.Binder; // Version 6.0.0
using System;
using System.Collections.Generic;

namespace VatFilingPricingTool.Common.Configuration
{
    /// <summary>
    /// Static helper class that provides utility methods for accessing and managing application 
    /// configuration settings across the VAT Filing Pricing Tool application.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Private constructor to prevent instantiation of static class
        /// </summary>
        private ConfigurationHelper()
        {
            // Private constructor to prevent instantiation
        }

        /// <summary>
        /// Gets a configuration section by path
        /// </summary>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="sectionPath">Path to the configuration section</param>
        /// <returns>The configuration section at the specified path</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or sectionPath is null</exception>
        /// <exception cref="ArgumentException">Thrown when sectionPath is empty</exception>
        public static IConfigurationSection GetSection(IConfiguration configuration, string sectionPath)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(sectionPath))
                throw new ArgumentException("Section path cannot be null or empty", nameof(sectionPath));
            
            return configuration.GetSection(sectionPath);
        }

        /// <summary>
        /// Gets a typed configuration value by path with optional default value
        /// </summary>
        /// <typeparam name="T">The type to convert the value to</typeparam>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="path">Path to the configuration value</param>
        /// <param name="defaultValue">Default value to return if the configuration value is not found</param>
        /// <returns>The configuration value or default if not found</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or path is null</exception>
        /// <exception cref="ArgumentException">Thrown when path is empty</exception>
        public static T GetValue<T>(IConfiguration configuration, string path, T defaultValue = default)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            
            return configuration.GetValue<T>(path, defaultValue);
        }

        /// <summary>
        /// Gets a required typed configuration value by path, throwing an exception if not found
        /// </summary>
        /// <typeparam name="T">The type to convert the value to</typeparam>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="path">Path to the configuration value</param>
        /// <returns>The configuration value</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or path is null</exception>
        /// <exception cref="ArgumentException">Thrown when path is empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the required configuration value is missing</exception>
        public static T GetRequiredValue<T>(IConfiguration configuration, string path)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            
            T value = configuration.GetValue<T>(path);
            
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new InvalidOperationException($"Required configuration value at path '{path}' is missing");
            
            return value;
        }

        /// <summary>
        /// Binds a configuration section to a new instance of a specified type
        /// </summary>
        /// <typeparam name="T">The type to bind to</typeparam>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="sectionPath">Path to the configuration section</param>
        /// <returns>A new instance of T with properties set from the configuration</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or sectionPath is null</exception>
        /// <exception cref="ArgumentException">Thrown when sectionPath is empty</exception>
        public static T Bind<T>(IConfiguration configuration, string sectionPath) where T : new()
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(sectionPath))
                throw new ArgumentException("Section path cannot be null or empty", nameof(sectionPath));
            
            T instance = new T();
            configuration.GetSection(sectionPath).Bind(instance);
            return instance;
        }

        /// <summary>
        /// Binds a configuration section to an existing instance of a specified type
        /// </summary>
        /// <typeparam name="T">The type to bind to</typeparam>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="sectionPath">Path to the configuration section</param>
        /// <param name="existingInstance">The existing instance to bind to</param>
        /// <exception cref="ArgumentNullException">Thrown when configuration, sectionPath, or existingInstance is null</exception>
        /// <exception cref="ArgumentException">Thrown when sectionPath is empty</exception>
        public static void BindExisting<T>(IConfiguration configuration, string sectionPath, T existingInstance)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(sectionPath))
                throw new ArgumentException("Section path cannot be null or empty", nameof(sectionPath));
            
            if (existingInstance == null)
                throw new ArgumentNullException(nameof(existingInstance), "Existing instance cannot be null");
            
            configuration.GetSection(sectionPath).Bind(existingInstance);
        }

        /// <summary>
        /// Gets a connection string by name from the configuration
        /// </summary>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="name">Name of the connection string</param>
        /// <returns>The connection string value</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or name is null</exception>
        /// <exception cref="ArgumentException">Thrown when name is empty</exception>
        public static string GetConnectionString(IConfiguration configuration, string name)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Connection string name cannot be null or empty", nameof(name));
            
            return configuration.GetConnectionString(name);
        }

        /// <summary>
        /// Gets a required connection string by name, throwing an exception if not found
        /// </summary>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="name">Name of the connection string</param>
        /// <returns>The connection string value</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or name is null</exception>
        /// <exception cref="ArgumentException">Thrown when name is empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the required connection string is missing</exception>
        public static string GetRequiredConnectionString(IConfiguration configuration, string name)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Connection string name cannot be null or empty", nameof(name));
            
            string connectionString = configuration.GetConnectionString(name);
            
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Required connection string '{name}' is missing");
            
            return connectionString;
        }

        /// <summary>
        /// Gets the immediate children of a configuration section
        /// </summary>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="sectionPath">Path to the configuration section</param>
        /// <returns>The child configuration sections</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or sectionPath is null</exception>
        /// <exception cref="ArgumentException">Thrown when sectionPath is empty</exception>
        public static IEnumerable<IConfigurationSection> GetChildren(IConfiguration configuration, string sectionPath)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(sectionPath))
                throw new ArgumentException("Section path cannot be null or empty", nameof(sectionPath));
            
            return configuration.GetSection(sectionPath).GetChildren();
        }

        /// <summary>
        /// Converts a configuration section to a dictionary of key-value pairs
        /// </summary>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="sectionPath">Path to the configuration section</param>
        /// <returns>Dictionary containing the configuration key-value pairs</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or sectionPath is null</exception>
        /// <exception cref="ArgumentException">Thrown when sectionPath is empty</exception>
        public static Dictionary<string, string> GetConfigurationDictionary(IConfiguration configuration, string sectionPath)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(sectionPath))
                throw new ArgumentException("Section path cannot be null or empty", nameof(sectionPath));
            
            IConfigurationSection section = configuration.GetSection(sectionPath);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            
            foreach (IConfigurationSection child in section.GetChildren())
            {
                dictionary[child.Key] = child.Value;
            }
            
            return dictionary;
        }

        /// <summary>
        /// Checks if a configuration path exists
        /// </summary>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="path">Path to check</param>
        /// <returns>True if the path exists, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when configuration or path is null</exception>
        /// <exception cref="ArgumentException">Thrown when path is empty</exception>
        public static bool Exists(IConfiguration configuration, string path)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            
            IConfigurationSection section = configuration.GetSection(path);
            return section.Exists();
        }
    }
}