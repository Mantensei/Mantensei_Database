using Mantensei_Database.Bindings;
using Mantensei_Database.Common;
using Mantensei_Database.Controls;
using Mantensei_Database.Models;
using Mantensei_Database.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace Mantensei_Database.Windows
{
    /// <summary>
    /// SchoolEditorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SchoolEditorWindow : Window
    {
        private SchoolProfile _profile;
        private string _currentFilePath = null;

        public SchoolEditorWindow() : this(default) { }

        // 編集用コンストラクタ
        public SchoolEditorWindow(SchoolProfile profile)
        {
            InitializeComponent();
            DataContext = new SEW_Model();
            _profile = profile;

            Loaded += (s, e) => InitializeProfile();
        }

        /// <summary>
        /// プロファイルデータの初期化
        /// </summary>
        private void InitializeProfile()
        {
            SchoolTypeCombo.Items.Clear();
            foreach(var type in SchoolProfile.SchoolTypes)
            {
                SchoolTypeCombo.Items.Add(type);
            }

            if (_profile == null)
            {
                _profile = new SchoolProfile();
                _profile.Id = ProfileDataBase.GenerateNewId<SchoolProfile>();
                Title = "新規作成";
            }
            else
            {
                Title = $"編集 - {_profile.Name}";
                // UIにデータを反映
            }

            ProfileConverter.LoadToUI(this, _profile);
        }

        void Save()
        {
            try
            {
                // UIからデータを取得
                var profile = new SchoolProfile();
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

        /// <summary>
        /// 保存ボタンクリック
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
            MainWindow.Instance.SchoolListPage.Update();
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult = false;
            Close();
        }
    }
}

namespace Mantensei_Database.Bindings
{
    /// <summary>
    /// SchoolEditorWindow用のデータコンテキスト
    /// </summary>
    public class SEW_Model
    {
        public TagInputViewModel Classes { get; private set; }
        public TagInputViewModel Clubs { get; private set; }

        public SEW_Model()
        {
            Classes = new TagInputViewModel("クラス", "クラス");
            Clubs = new TagInputViewModel("部活", "部活");
        }
    }
}