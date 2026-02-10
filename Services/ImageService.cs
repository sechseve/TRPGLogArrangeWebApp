using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace TRPGLogArrangeTool.Blazor.Services
{
    public class ImageService
    {
        // Key: Hash, Value: Base64 string
        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        public void Clear()
        {
            _cache.Clear();
        }

        public string GetOrAddFromBase64(string base64, out string key)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64);
                key = ComputeHash(bytes);
                
                if (_cache.ContainsKey(key))
                {
                    return key; // Return existing key
                }

                _cache[key] = base64;
                return key;
            }
            catch
            {
                key = null;
                return null;
            }
        }

        public string GetBase64ByKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            _cache.TryGetValue(key, out var base64);
            return base64;
        }

        private static string ComputeHash(byte[] bytes)
        {
            using (var sha1 = SHA1.Create())
            {
                return BitConverter.ToString(sha1.ComputeHash(bytes)).Replace("-", "");
            }
        }
    }
}
