using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Freezium.Infrastructure.Crypto
{
    /// <summary>
    /// XOR based encryption/decryption and token generation helpers.
    /// </summary>
    public static class CryptoHelper
    {
        public static long GetTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static string Encrypt(string text, string specialKey)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(specialKey))
                return null;

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                byte[] keyBytes = Encoding.UTF8.GetBytes(specialKey);
                byte[] result = new byte[bytes.Length];

                for (int i = 0; i < bytes.Length; i++)
                {
                    result[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
                }

                var sb = new StringBuilder(result.Length * 2);
                for (int i = 0; i < result.Length; i++)
                {
                    sb.Append(result[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encryption Error: " + ex.Message);
                return null;
            }
        }

        public static string Decrypt(string hexText, string specialKey)
        {
            if (string.IsNullOrEmpty(hexText) || string.IsNullOrEmpty(specialKey))
                return null;

            try
            {
                if (hexText.Length % 2 != 0)
                    throw new ArgumentException("Invalid hex string (odd length).");

                int byteCount = hexText.Length / 2;
                byte[] encryptedBytes = new byte[byteCount];
                for (int i = 0; i < byteCount; i++)
                {
                    encryptedBytes[i] = Convert.ToByte(hexText.Substring(i * 2, 2), 16);
                }

                byte[] keyBytes = Encoding.UTF8.GetBytes(specialKey);
                byte[] original = new byte[encryptedBytes.Length];

                for (int i = 0; i < encryptedBytes.Length; i++)
                {
                    original[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
                }

                return Encoding.UTF8.GetString(original);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Decryption Error: " + ex.Message);
                return null;
            }
        }

        public static string TokenCreate(string tokenKey)
        {
            DateTime trTime;
            try
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                trTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch
            {
                // Fallback to UTC+3 if "Turkey Standard Time" is not present on this system
                trTime = DateTime.UtcNow.AddHours(3);
            }

            string dayName = trTime.ToString("dddd", new CultureInfo("en-US")).ToLower();
            string combinedKey = tokenKey + "_" + dayName;
            string randomKey = GenerateRandomString(6);

            return Encrypt(JsonConvert.SerializeObject(new Dictionary<string, long>
            {
                { randomKey, GetTime() }
            }), combinedKey);
        }

        public static string BodyEncrypt(object data, string clientKey)
        {
            if (data == null) return null;

            try
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(data));
                dictionary["date"] = GetTime();
                return Encrypt(JsonConvert.SerializeObject(dictionary), clientKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encryption Error: " + ex.Message);
                return null;
            }
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var result = new char[length];
            var randomBytes = new byte[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[randomBytes[i] % chars.Length];
            }

            return new string(result);
        }
    }
}

