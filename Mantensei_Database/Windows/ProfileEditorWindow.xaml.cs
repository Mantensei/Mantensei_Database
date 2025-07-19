using Mantensei_Database;
using Mantensei_Database.Bindings;
using Mantensei_Database.Common;
using Mantensei_Database.Controls;
using Mantensei_Database.Models;
using Mantensei_Database.Services;
using Mantensei_Database.ViewModels;
using Mantensei_Database.Windows;
using MantenseiLib;
using MantenseiLib.WPF;
using Microsoft.Win32;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;
using Image = System.Windows.Controls.Image;

namespace Mantensei_Database.Windows
{
    /// <summary>
    /// ProfileEditorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProfileEditorWindow : Window
    {
        private CharacterProfile _profile;
        private BitmapSource _currentImage = null;

        public ProfileEditorWindow() : this(default) { }

        // 編集用コンストラクタ
        public ProfileEditorWindow(CharacterProfile profile)
        {
            InitializeComponent();
            DataContext = new PEW_Model(this);
            _profile = profile;

            Loaded += (s, e) => InitializeProfile();
        }

        /// <summary>
        /// プロファイルデータの初期化
        /// </summary>
        private void InitializeProfile()
        {
            if (_profile == null)
            {
                _profile = new CharacterProfile();
                _profile.Id = ProfileDataBase.GenerateNewId<CharacterProfile>();
                Title = "新規作成";
            }
            else
            {
                Title = $"編集 - {_profile.FullName}";
                // UIにデータを反映
            }

            ProfileConverter.LoadToUI(this, _profile);
            LoadProfileImage();
        }

        /// <summary>
        /// プロファイル画像を読み込み
        /// </summary>
        private void LoadProfileImage()
        {
            if (_profile?.Id > 0)
            {
                _currentImage = ImageService.LoadImage(_profile.Id);
                if (_currentImage != null && ProfileImage != null)
                {
                    ProfileImage.Source = _currentImage;
                }
                else
                {
                    //何も設定されていないとクリックが反応しないので、デフォルトの画像を設定
                    SetDefaultImage();
                }
            }
        }

        /// <summary>
        /// デフォルトの白いビットマップを設定
        /// </summary>
        private void SetDefaultImage()
        {
            // 150x150の白いビットマップを作成
            var bitmap = new WriteableBitmap(150, 150, 96, 96, PixelFormats.Bgr32, null);

            // 白色で塗りつぶし
            var whiteColor = Colors.Black;
            var colorBytes = new byte[] { whiteColor.B, whiteColor.G, whiteColor.R, whiteColor.A };
            var stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
            var pixelData = new byte[bitmap.PixelHeight * stride];

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                Array.Copy(colorBytes, 0, pixelData, i, 4);
            }

            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                                pixelData, stride, 0);

            ProfileImage.Source = bitmap;
            _currentImage = bitmap;
        }

        /// <summary>
        /// プロファイル画像クリック時の処理
        /// </summary>
        private void ProfileImage_Click(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "画像を選択",
                Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp;*.gif|すべてのファイル|*.*",
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var cropWindow = new ImageCropWindow(openFileDialog.FileName)
                    {
                        Owner = this
                    };

                    if (cropWindow.ShowDialog() == true)
                    {
                        _currentImage = cropWindow.CroppedImage;
                        if (ProfileImage != null)
                        {
                            ProfileImage.Source = _currentImage;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"画像の処理に失敗しました: {ex.Message}", "エラー",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        void Save()
        {
            try
            {
                // UIからデータを取得
                var profile = new CharacterProfile();
                ProfileConverter.LoadFromUI(this, profile);

                // ファイルパスが未設定の場合はデフォルトパスに保存
                ProfileService.SaveToDefaultPath(profile);
                var profilesDir = FileSystemUtility.GetProfilesDirectory(profile);
                var fileName = ProfileService.GetDefaultFileName(profile);
                var fflePath = Path.Combine(profilesDir, fileName);

                ImageService.SaveImage(profile.Id, _currentImage);

                MessageBox.Show($"保存しました。\n保存場所: {fflePath}",
                              "保存完了", MessageBoxButton.OK, MessageBoxImage.Information);


                Close();
                MainWindow.Instance.CharacterListPage.LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void GodDice_Click(object sender, RoutedEventArgs e)
        {
            List<Day> Birthdays = new List<Day>();

            void AddBirthdays(int year)
            {
                for (int month = 1; month <= 12; month++)
                {
                    for (int day = 1; day <= 31; day++)
                    {
                        var birthday = new Day(month, day);
                        if (birthday.IsValid(year))
                        {
                            Birthdays.Add(birthday);
                        }
                    }
                }
            }

            const int commonYear = 2001;
            const int lerpYear = 2000;
            AddBirthdays(commonYear);
            AddBirthdays(commonYear);
            AddBirthdays(commonYear);
            AddBirthdays(lerpYear);

            var rand = Birthdays.GetRandomElementOrDefault();
            BirthMonth.Text = $"{rand.birthMonth}";
            BirthDay.Text = $"{rand.birthDay}";
        }

        struct Day
        {
            public int birthMonth;
            public int birthDay;

            public Day(int month, int day)
            {
                birthMonth = month;
                birthDay = day;
            }

            public bool IsValid(int year = 2000)
            {
                return DateTime.TryParse($"{year}/{birthMonth}/{birthDay}", out _);
            }
        }
    }
}

namespace Mantensei_Database.Bindings
{
    public class PEW_Model : INotifyPropertyChanged
    {
        ProfileEditorWindow _window;

        public TagInputViewModel FavoriteThings { get; private set; }
        public TagInputViewModel NickNames { get; private set; }
        public TagInputViewModel Traits { get; private set; }
        public TagInputViewModel Dees { get; private set; }


        public ObservableCollection<SchoolProfile> Schools { get; private set; } = new();
        public ObservableCollection<string> Classes { get; private set; } = new();
        public ObservableCollection<string> Grade1Classes { get; private set; } = new();
        public ObservableCollection<string> Grade2Classes { get; private set; } = new();
        public ObservableCollection<string> Grade3Classes { get; private set; } = new();
        public ObservableCollection<string> Clubs { get; private set; } = new();

        public ObservableCollection<string> High { get; private set; } = new();
        public ObservableCollection<string> Middle { get; private set; } = new();
        public ObservableCollection<string> Elementary { get; private set; } = new();

        public ObservableCollection<string>[] ClassCollections => new[]
        {
            Grade3Classes,
            Grade2Classes, 
            Grade1Classes, 
        };

        public ObservableCollection<string>[] SchoolCollections => new[]
        {
            High,
            Middle, 
            Elementary,
        };

        private string _selectedClass;
        public string SelectedClass
        {
            get => _selectedClass;
            set
            {
                _selectedClass = value;
                OnPropertyChanged();

                if (_window.CurrentClass.SelectedIndex != -1)
                    NormalizeComboBox(ClassCollections, _selectedClass);
            }
        }

        private SchoolProfile _selectedSchool;
        public SchoolProfile SelectedSchool
        {
            get => _selectedSchool;
            set
            {
                _selectedSchool = value;
                OnPropertyChanged();
                UpdateSchoolRelatedData();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PEW_Model(ProfileEditorWindow profileEditorWindow)
        {
            _window = profileEditorWindow;

            FavoriteThings = new TagInputViewModel("好き・趣味", "趣味");
            NickNames = new TagInputViewModel("あだ名", "あだ名");
            Traits = new TagInputViewModel("タグ", "タグ");
            Dees = new TagInputViewModel("ネタ", "ネタ");   
            
            LoadSchools();
        }

        private void LoadSchools()
        {
            try
            {
                var schools = ProfileDataBase.GetAllProfiles<SchoolProfile>();
                Schools.Clear();
                foreach (var school in schools)
                {
                    Schools.Add(school);

                    switch(school.SchoolType)
                    {
                        case SchoolProfile.SchoolTypeHigh:
                            High.Add(school.Name);
                            break;
                        case SchoolProfile.SchoolTypeMiddle:
                            Middle.Add(school.Name);
                            break;
                        case SchoolProfile.SchoolTypeElementary:
                            Elementary.Add(school.Name);
                            break;

                        default: break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"学校データの読み込みエラー: {ex.Message}");
            }
        }

        private void UpdateSchoolRelatedData()
        {
            // クラス情報を更新
            Classes.Clear();
            foreach(var collection in ClassCollections)
                collection.Clear();

            foreach (var className in SelectedSchool.Classes)
            {
                Classes.Add(className);

                if(className.StartsWith('1') || className.StartsWith('１') || className.StartsWith('一'))
                    Grade1Classes.Add(className);
                else if (className.StartsWith('2') || className.StartsWith('２') || className.StartsWith('二'))
                    Grade2Classes.Add(className);
                else if (className.StartsWith('3') || className.StartsWith('３') || className.StartsWith('三'))
                    Grade3Classes.Add(className);
                else
                    foreach(var collection in ClassCollections)
                        collection.Add(className);
            }

            // 部活動情報を更新
            Clubs.Clear();
            foreach (var club in SelectedSchool.Clubs)
            {
                Clubs.Add(club);
            }           

            if(SelectedSchool != null)
                NormalizeComboBox(SchoolCollections, SelectedSchool.Name);

        }

        void NormalizeComboBox(ObservableCollection<string>[] bindings, string selected)
        {
            if (bindings.Any(x => x.Contains(selected)))
            {
                foreach (var collection in bindings)
                {
                    GetComboBox(collection).SelectedIndex = -1;
                }
            }

            bool found = false;
            for (int i = 0; i < bindings.Length; i++)
            {
                ObservableCollection<string>? collection = bindings[i];
                ComboBox combo = GetComboBox(collection);

                for (int j = 0; j < collection.Count; j++)
                {
                    string current = collection[j];
                    combo.IsEnabled = found;

                    if (!found)
                    {
                        combo.SelectedIndex = -1; // 初期化
                        if (current == selected)
                        {
                            combo.SelectedItem = current;
                            found = true;
                        }
                    }

                }
            }
        }

        ComboBox GetComboBox(ObservableCollection<string> bindings)
        {
            foreach(var combo in _window.GetComponentsInChildren<ComboBox>())
            {
                if (combo.ItemsSource == bindings)
                {
                    return combo;
                }
            }

            return null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
