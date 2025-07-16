using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
//using Mantensei_Database.Helpers;
using Mantensei_Database.Controls;
using Mantensei_Database.ViewModels;

using System.Windows.Controls;
using System.Windows;
using MantenseiLib.WPF;
using Mantensei_Database.DataAccess;

namespace Mantensei_Database.Controls
{
    public partial class TagInputView : UserControl, ISaveDataProvider
    {
        public TagInputView()
        {
            InitializeComponent();
        }

        TagInputViewModel ViewModel => (TagInputViewModel)DataContext;
        public void AddItem(string item) => ViewModel?.AddItem(item);
        public void AddItem(IEnumerable<string> items) => ViewModel?.AddItem(items);
        public ObservableCollection<string> Items => ViewModel.Items;

        public IEnumerable<string> GetSaveItems()
        {
            return Items;
        }

        public void LoadItem(IEnumerable<string> items)
        {
            ViewModel.AddItem(items);
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

        public TagInputViewModel(string title, string tagId)
        {
            Title = title;
            TagId = tagId;
            AddCommand = new RelayCommand(AddItem);
        }

        public void AddItem(string item)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                Items.Add(item.Trim());
                InputText = string.Empty;
            }
        }

        public void AddItem(IEnumerable<string> items)
        {
            if(items == null) return;

            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        private void AddItem()
        {
            AddItem(InputText);
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
