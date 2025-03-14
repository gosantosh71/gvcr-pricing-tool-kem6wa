using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace VatFilingPricingTool.Common.Helpers
{
    /// <summary>
    /// Static helper class providing security-related utility methods for the VAT Filing Pricing Tool
    /// </summary>
    public static class SecurityHelper
    {
        private const int DEFAULT_SALT_SIZE = 16;
        private const int DEFAULT_HASH_ITERATIONS = 10000;
        private const int DEFAULT_KEY_SIZE = 256;

        /// <summary>
        /// Hashes a password using PBKDF2 with a random salt
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>Base64-encoded string containing the salt and hash</returns>
        /// <exception cref="ArgumentException">Thrown when password is null or empty</exception>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Generate a random salt
            byte[] salt = new byte[DEFAULT_SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with the salt using PBKDF2
            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, DEFAULT_HASH_ITERATIONS, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(DEFAULT_KEY_SIZE / 8);
            }

            // Combine salt and hash for storage
            byte[] combined = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);

            // Convert to Base64 string for storage
            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Verifies a password against a stored hash
        /// </summary>
        /// <param name="password">The password to verify</param>
        /// <param name="hashedPassword">The stored hash to verify against</param>
        /// <returns>True if the password matches the hash, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when password or hashedPassword is null or empty</exception>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentException("Hashed password cannot be null or empty", nameof(hashedPassword));

            try
            {
                // Convert from Base64 string
                byte[] combined = Convert.FromBase64String(hashedPassword);

                // Ensure the combined array has enough bytes
                if (combined.Length < DEFAULT_SALT_SIZE + 1)
                    return false;

                // Extract salt and hash
                byte[] salt = new byte[DEFAULT_SALT_SIZE];
                byte[] hash = new byte[combined.Length - DEFAULT_SALT_SIZE];
                Buffer.BlockCopy(combined, 0, salt, 0, salt.Length);
                Buffer.BlockCopy(combined, salt.Length, hash, 0, hash.Length);

                // Hash the input password with the extracted salt
                byte[] computedHash;
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, DEFAULT_HASH_ITERATIONS, HashAlgorithmName.SHA256))
                {
                    computedHash = pbkdf2.GetBytes(hash.Length);
                }

                // Compare the computed hash with the stored hash in constant time
                return CryptographicOperations.FixedTimeEquals(hash, computedHash);
            }
            catch (FormatException)
            {
                // Invalid base64 format
                return false;
            }
        }

        /// <summary>
        /// Generates a cryptographically secure random string of the specified length
        /// </summary>
        /// <param name="length">The length of the string to generate</param>
        /// <returns>A random string of the specified length</returns>
        /// <exception cref="ArgumentException">Thrown when length is less than or equal to 0</exception>
        public static string GenerateSecureRandomString(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than 0", nameof(length));

            const string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+[]{}|;:,.<>?";
            
            // Create a byte array of appropriate size
            byte[] randomBytes = new byte[length];
            
            // Fill the array with random bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Convert the random bytes to characters from the charset
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                // Map the byte to a character in the charset
                result[i] = charset[randomBytes[i] % charset.Length];
            }

            return new string(result);
        }

        /// <summary>
        /// Generates cryptographically secure random bytes
        /// </summary>
        /// <param name="length">The number of bytes to generate</param>
        /// <returns>An array of random bytes</returns>
        /// <exception cref="ArgumentException">Thrown when length is less than or equal to 0</exception>
        public static byte[] GenerateSecureRandomBytes(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than 0", nameof(length));

            byte[] randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return randomBytes;
        }

        /// <summary>
        /// Encrypts a string using AES encryption
        /// </summary>
        /// <param name="plainText">The string to encrypt</param>
        /// <param name="key">The encryption key</param>
        /// <returns>Base64-encoded encrypted string</returns>
        /// <exception cref="ArgumentException">Thrown when plainText or key is null or empty</exception>
        public static string EncryptString(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            // Create a fixed salt for key derivation
            byte[] salt = Encoding.UTF8.GetBytes("VatFilingPricingTool_FixedSalt");
            
            // Derive the encryption key using PBKDF2
            byte[] keyBytes;
            using (var pbkdf2 = new Rfc2898DeriveBytes(key, salt, 10000, HashAlgorithmName.SHA256))
            {
                keyBytes = pbkdf2.GetBytes(32); // 256 bits
            }

            // Generate a random IV
            byte[] iv = new byte[16]; // AES block size is 16 bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            // Encrypt the plainText
            byte[] encrypted;
            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    encrypted = ms.ToArray();
                }
            }

            // Combine IV and encrypted data
            byte[] combined = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, combined, iv.Length, encrypted.Length);

            // Convert to Base64 string
            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Decrypts a string that was encrypted with EncryptString
        /// </summary>
        /// <param name="encryptedText">The encrypted text to decrypt</param>
        /// <param name="key">The encryption key</param>
        /// <returns>Decrypted string</returns>
        /// <exception cref="ArgumentException">Thrown when encryptedText or key is null or empty or when decryption fails</exception>
        public static string DecryptString(string encryptedText, string key)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedText));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            try
            {
                // Convert from Base64 string
                byte[] combined = Convert.FromBase64String(encryptedText);

                // Ensure the combined array has enough bytes for IV and data
                if (combined.Length < 16)
                    throw new ArgumentException("Invalid encrypted text format", nameof(encryptedText));

                // Extract IV and encrypted data
                byte[] iv = new byte[16]; // AES block size is 16 bytes
                byte[] encrypted = new byte[combined.Length - 16];
                Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(combined, iv.Length, encrypted, 0, encrypted.Length);

                // Create a fixed salt for key derivation (same as in EncryptString)
                byte[] salt = Encoding.UTF8.GetBytes("VatFilingPricingTool_FixedSalt");
                
                // Derive the encryption key using PBKDF2
                byte[] keyBytes;
                using (var pbkdf2 = new Rfc2898DeriveBytes(key, salt, 10000, HashAlgorithmName.SHA256))
                {
                    keyBytes = pbkdf2.GetBytes(32); // 256 bits
                }

                // Decrypt the data
                string plainText;
                using (var aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(encrypted))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        plainText = sr.ReadToEnd();
                    }
                }

                return plainText;
            }
            catch (CryptographicException ex)
            {
                throw new ArgumentException("Decryption failed. The key may be incorrect or the data corrupted.", nameof(encryptedText), ex);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid encrypted text format", nameof(encryptedText), ex);
            }
        }

        /// <summary>
        /// Sanitizes input to prevent XSS and injection attacks
        /// </summary>
        /// <param name="input">The input to sanitize</param>
        /// <returns>Sanitized input string</returns>
        public static string SanitizeInput(string input)
        {
            if (input == null)
                return null;

            // Replace potentially dangerous characters with their HTML encoded equivalents
            string sanitized = input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;")
                .Replace("/", "&#x2F;");

            // Remove any script tags and their contents
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);

            // Remove any HTML event handlers (onclick, onload, etc.)
            sanitized = Regex.Replace(sanitized, @"\s(on\w+)=", " data-removed-$1=", RegexOptions.IgnoreCase);

            return sanitized;
        }

        /// <summary>
        /// Computes a hash of the input data using the specified algorithm
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <param name="algorithmName">The hashing algorithm to use</param>
        /// <returns>Computed hash</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="ArgumentException">Thrown when the algorithm is not supported</exception>
        public static byte[] ComputeHash(byte[] data, HashAlgorithmName algorithmName)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var hashAlgorithm = HashAlgorithm.Create(algorithmName.Name))
            {
                if (hashAlgorithm == null)
                    throw new ArgumentException($"Hash algorithm {algorithmName.Name} is not supported", nameof(algorithmName));

                return hashAlgorithm.ComputeHash(data);
            }
        }

        /// <summary>
        /// Computes a SHA-256 hash of the input string
        /// </summary>
        /// <param name="input">The string to hash</param>
        /// <returns>Hexadecimal string representation of the hash</returns>
        /// <exception cref="ArgumentException">Thrown when input is null or empty</exception>
        public static string ComputeSha256Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be null or empty", nameof(input));

            // Convert the input string to bytes
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Compute the hash
            byte[] hashBytes;
            using (var sha256 = SHA256.Create())
            {
                hashBytes = sha256.ComputeHash(inputBytes);
            }

            // Convert the hash bytes to a hexadecimal string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a secure API key
        /// </summary>
        /// <returns>A secure API key</returns>
        public static string GenerateApiKey()
        {
            // Generate 32 random bytes (256 bits)
            byte[] keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            // Convert to Base64 string
            string apiKey = Convert.ToBase64String(keyBytes);

            // Replace characters that might cause URL encoding issues
            apiKey = apiKey.Replace("+", "-").Replace("/", "_").Replace("=", "");

            return apiKey;
        }

        /// <summary>
        /// Checks if a password meets strong password requirements
        /// </summary>
        /// <param name="password">The password to check</param>
        /// <returns>True if the password is strong, false otherwise</returns>
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Check minimum length (12 characters)
            if (password.Length < 12)
                return false;

            // Check for uppercase letters
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for lowercase letters
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for digits
            if (!Regex.IsMatch(password, @"[0-9]"))
                return false;

            // Check for special characters
            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return false;

            return true;
        }

        /// <summary>
        /// Generates a secure token for use in authentication or verification processes
        /// </summary>
        /// <returns>A secure token</returns>
        public static string GenerateSecureToken()
        {
            // Generate a GUID for uniqueness
            Guid guid = Guid.NewGuid();
            byte[] guidBytes = guid.ToByteArray();

            // Generate random bytes for additional entropy
            byte[] randomBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Combine GUID and random bytes
            byte[] combined = new byte[guidBytes.Length + randomBytes.Length];
            Buffer.BlockCopy(guidBytes, 0, combined, 0, guidBytes.Length);
            Buffer.BlockCopy(randomBytes, 0, combined, guidBytes.Length, randomBytes.Length);

            // Compute SHA-256 hash of the combined data
            byte[] hashBytes;
            using (var sha256 = SHA256.Create())
            {
                hashBytes = sha256.ComputeHash(combined);
            }

            // Convert to Base64 string
            string token = Convert.ToBase64String(hashBytes);

            // Format the token to be URL-friendly
            token = token.Replace("+", "-").Replace("/", "_").Replace("=", "");

            return token;
        }
    }
}