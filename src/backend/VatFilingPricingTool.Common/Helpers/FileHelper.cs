using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;  // Version: 6.0.0

namespace VatFilingPricingTool.Common.Helpers
{
    /// <summary>
    /// Static helper class that provides utility methods for file operations.
    /// Used throughout the application for handling files in reports, document processing,
    /// and data import/export scenarios.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// List of allowed file extensions for upload and processing.
        /// </summary>
        public static readonly string[] AllowedFileExtensions = new[]
        {
            ".pdf", ".xlsx", ".xls", ".csv", ".txt", ".jpg",
            ".jpeg", ".png", ".tiff", ".tif", ".docx", ".doc"
        };

        /// <summary>
        /// Maximum allowed file size in bytes (10 MB).
        /// </summary>
        public static readonly long MaxFileSizeBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Gets the file extension from a file name or path.
        /// </summary>
        /// <param name="fileName">The file name or path.</param>
        /// <returns>The file extension including the dot, or empty string if no extension.</returns>
        public static string GetFileExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            return Path.GetExtension(fileName).ToLowerInvariant();
        }

        /// <summary>
        /// Gets the file name from a file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The file name without the path.</returns>
        public static string GetFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            return Path.GetFileName(filePath);
        }

        /// <summary>
        /// Gets the file name without extension from a file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The file name without the path and extension.</returns>
        public static string GetFileNameWithoutExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// Checks if a file has an allowed extension.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the file extension is allowed, false otherwise.</returns>
        public static bool IsValidFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            string extension = GetFileExtension(fileName);
            return AllowedFileExtensions.Contains(extension);
        }

        /// <summary>
        /// Checks if a file size is within the allowed limit.
        /// </summary>
        /// <param name="fileSizeBytes">The file size in bytes.</param>
        /// <returns>True if the file size is within the limit, false otherwise.</returns>
        public static bool IsValidFileSize(long fileSizeBytes)
        {
            return fileSizeBytes > 0 && fileSizeBytes <= MaxFileSizeBytes;
        }

        /// <summary>
        /// Gets the MIME content type for a file based on its extension.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The MIME content type, or application/octet-stream if unknown.</returns>
        public static string GetContentType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "application/octet-stream";

            string extension = GetFileExtension(fileName);

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".csv" => "text/csv",
                ".txt" => "text/plain",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".tiff" => "image/tiff",
                ".tif" => "image/tiff",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Ensures that a directory exists, creating it if necessary.
        /// </summary>
        /// <param name="directoryPath">The directory path to ensure exists.</param>
        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        /// <summary>
        /// Reads all text from a file asynchronously.
        /// </summary>
        /// <param name="filePath">The file path to read from.</param>
        /// <returns>The text content of the file.</returns>
        /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        public static async Task<string> ReadAllTextAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return await File.ReadAllTextAsync(filePath);
        }

        /// <summary>
        /// Writes text to a file asynchronously, creating the file if it doesn't exist.
        /// </summary>
        /// <param name="filePath">The file path to write to.</param>
        /// <param name="content">The content to write.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
        public static async Task WriteAllTextAsync(string filePath, string content)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            string directoryPath = Path.GetDirectoryName(filePath);
            EnsureDirectoryExists(directoryPath);

            await File.WriteAllTextAsync(filePath, content);
        }

        /// <summary>
        /// Reads all bytes from a file asynchronously.
        /// </summary>
        /// <param name="filePath">The file path to read from.</param>
        /// <returns>The byte content of the file.</returns>
        /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        public static async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return await File.ReadAllBytesAsync(filePath);
        }

        /// <summary>
        /// Writes bytes to a file asynchronously, creating the file if it doesn't exist.
        /// </summary>
        /// <param name="filePath">The file path to write to.</param>
        /// <param name="content">The content to write.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
        public static async Task WriteAllBytesAsync(string filePath, byte[] content)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (content == null)
                throw new ArgumentNullException(nameof(content), "Content cannot be null");

            string directoryPath = Path.GetDirectoryName(filePath);
            EnsureDirectoryExists(directoryPath);

            await File.WriteAllBytesAsync(filePath, content);
        }

        /// <summary>
        /// Copies a file from one location to another asynchronously.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationFilePath">The destination file path.</param>
        /// <param name="overwrite">Whether to overwrite the destination file if it exists.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when sourceFilePath or destinationFilePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the source file does not exist.</exception>
        public static async Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
                throw new ArgumentException("Source file path cannot be null or empty", nameof(sourceFilePath));

            if (string.IsNullOrEmpty(destinationFilePath))
                throw new ArgumentException("Destination file path cannot be null or empty", nameof(destinationFilePath));

            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Source file not found", sourceFilePath);

            string destinationDirectory = Path.GetDirectoryName(destinationFilePath);
            EnsureDirectoryExists(destinationDirectory);

            // Use Task.Run to wrap the synchronous File.Copy method
            await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, overwrite));
        }

        /// <summary>
        /// Deletes a file if it exists.
        /// </summary>
        /// <param name="filePath">The file path to delete.</param>
        /// <returns>True if the file was deleted, false if it didn't exist.</returns>
        /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
        public static bool DeleteFileIfExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates a temporary file name with an optional extension.
        /// </summary>
        /// <param name="extension">Optional file extension.</param>
        /// <returns>A temporary file path.</returns>
        public static string GetTempFileName(string extension = null)
        {
            string tempFile = Path.GetTempFileName();

            if (!string.IsNullOrEmpty(extension))
            {
                string tempPath = Path.GetDirectoryName(tempFile);
                string tempFileName = Path.GetFileNameWithoutExtension(tempFile);
                
                if (!extension.StartsWith("."))
                    extension = "." + extension;

                tempFile = Path.Combine(tempPath, tempFileName + extension);
            }

            return tempFile;
        }

        /// <summary>
        /// Generates a unique file name based on a base name and extension.
        /// </summary>
        /// <param name="baseName">The base file name.</param>
        /// <param name="extension">The file extension.</param>
        /// <returns>A unique file name.</returns>
        public static string GetUniqueFileName(string baseName = null, string extension = null)
        {
            if (string.IsNullOrEmpty(baseName))
                baseName = "file";

            if (string.IsNullOrEmpty(extension))
                extension = ".tmp";

            if (!extension.StartsWith("."))
                extension = "." + extension;

            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{baseName}_{uniqueId}{extension}";
        }

        /// <summary>
        /// Sanitizes a file name by removing invalid characters.
        /// </summary>
        /// <param name="fileName">The file name to sanitize.</param>
        /// <returns>A sanitized file name.</returns>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // Replace invalid file name characters with underscores
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Trim to a reasonable length if needed (e.g., 255 characters)
            const int maxFileNameLength = 255;
            if (sanitized.Length > maxFileNameLength)
                sanitized = sanitized.Substring(0, maxFileNameLength);

            return sanitized;
        }
    }
}