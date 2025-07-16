using Mantensei_Database;
using Mantensei_Database.Bindings;
using Mantensei_Database.Common;
using Mantensei_Database.Controls;
using Mantensei_Database.Models;
using Mantensei_Database.ViewModels;
using MantenseiLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace Mantensei_Database.Windows
{
    /// <summary>
    /// ProfileEditorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProfileEditorWindow : Window
    {
        private CharacterProfile _profile;
        private string _currentFilePath = null;

        public ProfileEditorWindow() : this(default) { }

        // 編集用コンストラクタ
        public ProfileEditorWindow(CharacterProfile profile)
        {
            InitializeComponent();
            DataContext = new PEW_Model();
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        // ProfileEditorWindow.xaml.cs の修正版 (Save部分のみ)
        void Save()
        {
            try
            {
                // UIからデータを取得
                var profile = new CharacterProfile();
                ProfileConverter.LoadFromUI(this, profile);

                // ファイルパスが未設定の場合はデフォルトパスに保存
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    ProfileService.SaveToDefaultPath(profile);
                    var profilesDir = FileSystemUtility.GetProfilesDirectory(profile);
                    var fileName = ProfileService.GetDefaultFileName(profile);
                    _currentFilePath = Path.Combine(profilesDir, fileName);

                    MessageBox.Show($"保存しました。\n保存場所: {_currentFilePath}",
                                  "保存完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // 既存ファイルに上書き保存
                    ProfileService.SaveToXml(profile, _currentFilePath);
                    MessageBox.Show("保存しました。", "保存完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }

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
    public class PEW_Model
    {
        public TagInputViewModel FavoriteThings { get; private set; }
        public TagInputViewModel NickNames { get; private set; }
        public TagInputViewModel Traits { get; private set; }
        public TagInputViewModel Dees { get; private set; }

        public PEW_Model()
        {
            FavoriteThings = new TagInputViewModel("好き・趣味", "趣味");
            NickNames = new TagInputViewModel("あだ名", "あだ名");
            Traits = new TagInputViewModel("タグ", "タグ");
            Dees = new TagInputViewModel("ネタ", "ネタ");
        }
    }
}