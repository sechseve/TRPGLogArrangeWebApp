using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TRPGLogArrangeTool.Blazor.Models
{
    /// <summary>
    /// チャットメッセージの1件分のデータを表すクラス
    /// </summary>
    public class ChatMessage : INotifyPropertyChanged
    {
        private bool _isAddedByMessage;
        /// <summary>
        /// ユーザーによって後から追加されたメッセージかどうか
        /// </summary>
        public bool IsAddedMessage
        {
            get => _isAddedByMessage;
            set
            {
                _isAddedByMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isSecretMessage;
        /// <summary>
        /// 秘匿メッセージかどうか
        /// </summary>
        public bool IsSecretMessage
        {
            get => _isSecretMessage;
            set
            {
                _isSecretMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isEventImage;
        /// <summary>
        /// イベント画像（ログの中間に挿入される画像）かどうか
        /// </summary>
        public bool IsEventImage
        {
            get => _isEventImage;
            set
            {
                _isEventImage = value;
                OnPropertyChanged();
            }
        }

        private bool _isIconEventImage;
        /// <summary>
        /// アイコンとしてのイベント画像かどうか
        /// </summary>
        public bool IsIconEventImage
        {
            get => _isIconEventImage;
            set
            {
                _isIconEventImage = value;
                OnPropertyChanged();
            }
        }

        private string _area;
        /// <summary>
        /// 発言が行われたタブ（エリア）名
        /// </summary>
        public string Area
        {
            get => _area;
            set
            {
                _area = value;
                OnPropertyChanged();
            }
        }

        private long _timeStamp;
        /// <summary>
        /// 発言のタイムスタンプ（ミリ秒）
        /// </summary>
        public long TimeStamp
        {
            get => _timeStamp;
            set
            {
                _timeStamp = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// キャラクター名または表示名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// メッセージ本文（HTML形式を含む場合がある）
        /// </summary>
        public string Text { get; set; }

        private string _imageKey;
        /// <summary>
        /// 表示する画像のキー（ImageServiceで管理される一意のID）
        /// </summary>
        public string ImageKey
        {
            get => _imageKey;
            set
            {
                if (_imageKey != value)
                {
                    _imageKey = value;
                    OnPropertyChanged();
                }
            }
        }

        // Blazorとの互換性のために BitmapImage への依存を削除しました。
        // 画像の表示は、UI/サービスにおいて ImageKey を Base64 文字列に解決することで行われます。

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
