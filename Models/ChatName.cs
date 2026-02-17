using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TRPGLogArrangeTool.Blazor.Models
{
    /// <summary>
    /// チャットの参加者（キャラクター）とその画像を管理するクラス
    /// </summary>
    public class ChatName : INotifyPropertyChanged
    {
        private string _name;
        /// <summary>
        /// キャラクター名
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _defaultImageKey;
        /// <summary>
        /// デフォルトで使用される画像のキー
        /// </summary>
        public string DefaultImageKey
        {
            get => _defaultImageKey;
            set
            {
                _defaultImageKey = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// このキャラクターに関連付けられた全ての画像キーのリスト
        /// </summary>
        public List<string> ImageKeys { get; set; } = new List<string>();

        /// <summary>
        /// プロパティ値が変更されたときに発生するイベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChanged イベントを発生させます
        /// </summary>
        /// <param name="name">変更されたプロパティ名</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
