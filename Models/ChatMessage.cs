using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TRPGLogArrangeTool.Blazor.Models
{
    public class ChatMessage : INotifyPropertyChanged
    {
        private bool _isAddedByMessage;
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
        public bool IsEventImage
        {
            get => _isEventImage;
            set
            {
                _isEventImage = value;
                OnPropertyChanged();
            }
        }

        private string _area;
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
        public long TimeStamp
        {
            get => _timeStamp;
            set
            {
                _timeStamp = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; set; }
        public string Text { get; set; }

        private string _imageKey;
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

        // BitmapImage dependency removed for Blazor compatibility.
        // Image display will be handled via ImageKey resolving to a Base64 string in the UI/Service.

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
