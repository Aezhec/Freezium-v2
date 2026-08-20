using System;
using Newtonsoft.Json.Linq;

namespace Freezium.Helpers
{
    /// <summary>
    /// Helper class for JSON parsing operations.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Safely parses a JSON string into a JObject without throwing exceptions. Returns true if successful, false otherwise.
        /// </summary>
        public static bool TryParseJObject(string jsonString, out JObject result)
        {
            result = null;

            try
            {
                result = JObject.Parse(jsonString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
