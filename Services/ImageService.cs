using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace TRPGLogArrangeTool.Blazor.Services
{
    /// <summary>
    /// 画像データをキャッシュし、ハッシュによる一意なキーで管理するサービス
    /// </summary>
    public class ImageService
    {
        /// <summary>
        /// 画像のハッシュをキー、Base64文字列を値として保持するキャッシュ
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// キャッシュされている全ての画像を消去します
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Base64形式の画像データをキャッシュに追加し、対応するキー（ハッシュ）を取得します。
        /// すでに存在する場合は既存のキーを返します。
        /// </summary>
        /// <param name="base64">画像のBase64文字列</param>
        /// <param name="key">生成または取得されたキー（出力）</param>
        /// <returns>取得に成功した場合はキー、失敗した場合は null</returns>
        public string GetOrAddFromBase64(string base64, out string key)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64);
                key = ComputeHash(bytes);
                
                if (_cache.ContainsKey(key))
                {
                    return key; // 既存のキーを返す
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

        /// <summary>
        /// キーに対応するBase64形式の画像データを取得します
        /// </summary>
        /// <param name="key">画像のキー</param>
        /// <returns>Base64文字列。見つからない場合は null</returns>
        public string GetBase64ByKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            _cache.TryGetValue(key, out var base64);
            return base64;
        }

        /// <summary>
        /// バイト配列からSHA1ハッシュを計算し、16進数の文字列形式で返します
        /// </summary>
        /// <param name="bytes">対象のデータ</param>
        /// <returns>ハッシュ文字列</returns>
        private static string ComputeHash(byte[] bytes)
        {
            using (var sha1 = SHA1.Create())
            {
                return BitConverter.ToString(sha1.ComputeHash(bytes)).Replace("-", "");
            }
        }
    }
}
