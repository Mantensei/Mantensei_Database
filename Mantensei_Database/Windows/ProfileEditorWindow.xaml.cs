using Mantensei_Database.Bindings;
using Mantensei_Database.Common;
using Mantensei_Database.Controls;
using Mantensei_Database.Models;
using Mantensei_Database.Services;
using Mantensei_Database.Windows;
using MantenseiLib;
using MantenseiLib.WPF;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace Mantensei_Database.Windows
{
    /// <summary>
    /// ProfileEditorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProfileEditorWindow : Window
    {
        private CharacterProfile _profile;

        public ProfileEditorWindow() : this(default) { }

        // 編集用コンストラクタ
        public ProfileEditorWindow(CharacterProfile profile)
        {
            InitializeComponent();
            DataContext = new PEW_Model(this);
            _profile = profile;

            Loaded += (s, e) =>
            { 
                InitializeProfile();
                SetShortcuts();
            };
        }

        void SetShortcuts()
        {
            // 既存のComboBoxのショートカット処理
            foreach (var combo in this.GetComponentsInChildren<ComboBox>())
            {
                combo.PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == Key.Delete || e.Key == Key.Back)
                    {
                        combo.SelectedIndex = -1;
                        e.Handled = true;
                    }
                };
            }

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape || (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
                {
                    Close();
                    e.Handled = true;
                }

                else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    Save();
                    e.Handled = true;
                }

                else if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    ExportPrompt();
                    e.Handled = true;
                }
            };
        }

        private void ExportPrompt()
        {
            try
            {
                // UIからデータを取得
                var profile = new CharacterProfile();
                ProfileConverter.LoadFromUI(this, profile);

                // プロンプトテキストを生成
                var prompt = profile.GeneratePromptText();

                // クリップボードにコピー
                Clipboard.SetText(prompt);

                MessageBox.Show("プロンプトをクリップボードにコピーしました。", "プロンプト出力完了",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"プロンプトの生成に失敗しました: {ex.Message}", "エラー",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            var img = ImageService.LoadImage(_profile.Id);
            if (img != null)
            {
                ProfileImage.Source = img;
            }
            else
            {
                //何も設定されていないとクリックが反応しないので、デフォルトの画像を設定
                ProfileImage.Source = ImageService.GetDefaultImage();
            }
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
                        ProfileImage.Source = cropWindow.CroppedImage;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"画像の処理に失敗しました: {ex.Message}", "エラー",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

                ImageService.SaveImage(profile.Id, ProfileImage.Source);

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

        private void GodDice_Click(object sender, RoutedEventArgs e)
        {
            var date = Models.GodDice.Roll();

            BirthMonth.Text = date.month.ToString();
            BirthDay.Text = date.day.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void PromptButton_Click(object sender, RoutedEventArgs e)
        {
            ExportPrompt();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
            if(SelectedSchool == null)
            {
                foreach(var combo in _window.SchoolInfo.GetComponentsInChildren<ComboBox>())
                {
                    combo.SelectedIndex = -1;
                    combo.IsEnabled = false;
                }

                _window.CurrentSchool.IsEnabled = true;

                return;
            }
            else
            {
                foreach(var combo in _window.SchoolInfo.GetComponentsInChildren<ComboBox>())
                {
                    combo.IsEnabled = true;
                }
            }

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
            // まず全てのコンボボックスの選択をクリア
            foreach (var collection in bindings)
            {
                GetComboBox(collection).SelectedIndex = -1;
            }

            // 選択された項目を含むコンボボックスのインデックスを見つける
            int selectedIndex = -1;
            for (int i = 0; i < bindings.Length; i++)
            {
                if (bindings[i].Contains(selected))
                {
                    selectedIndex = i;
                    break;
                }
            }

            // 各コンボボックスの有効/無効を設定
            for (int i = 0; i < bindings.Length; i++)
            {
                var combo = GetComboBox(bindings[i]);

                if (i > selectedIndex)
                {
                    combo.IsEnabled = true;
                }
                else if (i == selectedIndex)
                {
                    combo.IsEnabled = false;
                    combo.SelectedItem = selected;
                }
                else
                {
                    combo.IsEnabled = false;
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
