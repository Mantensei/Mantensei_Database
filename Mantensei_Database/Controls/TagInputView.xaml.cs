using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
//using Mantensei_Database.Helpers;
using Mantensei_Database.Controls;
using Mantensei_Database.ViewModels;

using System.Windows.Controls;

namespace Mantensei_Database.Controls
{
    public partial class TagInputView : UserControl
    {
        public TagInputView()
        {
            InitializeComponent();
        }
    }
}


namespace Mantensei_Database.Controls
{
    public class TagInputViewModel : INotifyPropertyChanged
    {
        private string _inputText = string.Empty;
        public string Title { get; set; }
        public string TagId { get; set; }

        public ObservableCollection<string> Items { get; } = new();

        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddCommand { get; }

        public TagInputViewModel()
        {
            Title = "プレビュー用タイトル";
            TagId = "preview";

            // 仮タグをいくつか入れておく
            Items.Add("仮タグ1");
            Items.Add("仮タグ2");
            Items.Add("仮タグ3");

            // コマンドはnullでよい（ボタン押せなくていい）
            AddCommand = null!;
        }

        public TagInputViewModel(string title, string tagId)
        {
            Title = title;
            TagId = tagId;
            AddCommand = new RelayCommand(AddItem);
        }

        private void AddItem()
        {
            if (!string.IsNullOrWhiteSpace(InputText))
            {
                Items.Add(InputText.Trim());
                InputText = string.Empty;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}


namespace Mantensei_Database.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
