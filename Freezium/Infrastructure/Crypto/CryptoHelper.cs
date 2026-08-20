using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                byte[] keyBytes = Encoding.UTF8.GetBytes(specialKey);
                byte[] result = new byte[bytes.Length];

                for (int i = 0; i < bytes.Length; i++)
                {
                    result[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
                }

                return BitConverter.ToString(result).Replace("-", "").ToLower();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encryption Error: " + ex.Message);
                return null;
            }
        }

        public static string Decrypt(string hexText, string specialKey)
        {
            if (hexText == null) return null;

            try
            {
                if (hexText.Length % 2 != 0)
                    throw new ArgumentException("Invalid hex string (odd length).");

                byte[] encryptedBytes = Enumerable.Range(0, hexText.Length / 2)
                    .Select(i => Convert.ToByte(hexText.Substring(i * 2, 2), 16))
                    .ToArray();

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
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            string dayName = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz)
                .ToString("dddd", new CultureInfo("en-US")).ToLower();

            string combinedKey = tokenKey + "_" + dayName;
            string randomKey = GenerateRandomString(6);

            return Encrypt(JsonConvert.SerializeObject(new Dictionary<string, long>
            {
                { randomKey, GetTime() }
            }), combinedKey);
        }

        public static string BodyEncrypt(object data, string clientKey)
        {
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
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }
    }
}
