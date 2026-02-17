using System.Collections.Generic;

namespace TRPGLogArrangeTool.Blazor.Models
{
    /// <summary>
    /// キャラクターの立ち絵情報を保持するクラス
    /// </summary>
    public class CharacterStandInfo
    {
        /// <summary>
        /// キャラクター名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 立ち絵の名称と画像キー（ImageServiceで管理されるキー）の辞書
        /// </summary>
        public Dictionary<string, string> StandDictionary { get; set; } = new Dictionary<string, string>();
    }
}
