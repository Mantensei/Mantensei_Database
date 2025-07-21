using Mantensei_Database.DataAccess;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mantensei_Database.Controls
{
    public partial class StatusSlider : UserControl, ISaveDataProvider
    {
        public StatusSlider()
        {
            InitializeComponent();
        }

        StatusSliderModel ViewModel => (StatusSliderModel)DataContext;

        /// <summary>
        /// スライダーの値を取得・設定
        /// </summary>
        public double Value
        {
            get => ViewModel?.Value ?? 0;
            set
            {
                if (ViewModel != null)
                    ViewModel.Value = value;
            }
        }

        public object GetSaveItems()
        {
            return Value;
        }

        public void LoadItem(object value)
        {
            if (value is double doubleValue)
                Value = doubleValue;
            else if (double.TryParse(value?.ToString(), out var parsedValue))
                Value = parsedValue;
        }

        public void LoadItem(IEnumerable<string> items)
        {
            Value = int.Parse(items.FirstOrDefault() ?? "0");
        }

        IEnumerable<string> ISaveDataProvider.GetSaveItems()
        {
            yield return Value.ToString();
        }

        private void Slider_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int number = -1;

            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                number = e.Key - Key.D0;
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                number = e.Key - Key.NumPad0;
            }

            if (number >= 0)
            {
                var slider = (Slider)sender;
                number = (int)Math.Clamp(number, slider.Minimum, slider.Maximum);
                slider.Value = number;
                e.Handled = true;
            }
        }


    }

    public class StatusSliderModel : INotifyPropertyChanged
    {
        private double _value = 0;

        string _title;
        public string Title => $"[{Value}] {_title}";
        public string ColorHex { get; }
        public string Tag { get; }
        public Brush TitleBrush { get; }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Title)); // ←これを追加！！
                }
            }
        }

        public StatusSliderModel(string title, string colorHex, string tag)
        {
            _title = title;
            ColorHex = colorHex;
            Tag = tag;

            try
            {
                TitleBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            }
            catch
            {
                TitleBrush = new SolidColorBrush(Colors.Black);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}