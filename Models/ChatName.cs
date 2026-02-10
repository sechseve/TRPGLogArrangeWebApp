using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TRPGLogArrangeTool.Blazor.Models
{
    public class ChatName : INotifyPropertyChanged
    {
        private string _name;
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
        public string DefaultImageKey
        {
            get => _defaultImageKey;
            set
            {
                _defaultImageKey = value;
                OnPropertyChanged();
            }
        }

        public List<string> ImageKeys { get; set; } = new List<string>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
