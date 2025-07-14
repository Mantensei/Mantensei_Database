using Mantensei_Database.Bindings;
using Mantensei_Database.Controls;
using Mantensei_Database.Models;
using Mantensei_Database.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mantensei_Database.Windows
{
    /// <summary>
    /// SchoolEditorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SchoolEditorWindow : Window
    {
        private SchoolInfo _schoolInfo;
        private bool _isEditing;

        public SchoolEditorWindow() : this(null) { }

        public SchoolEditorWindow(SchoolInfo schoolInfo)
        {
            InitializeComponent();
            DataContext = new SEW_Model();

            _schoolInfo = schoolInfo;
            _isEditing = schoolInfo != null;

            InitializeSchoolInfo();
            LoadDataToUI();
        }

        /// <summary>
        /// 学校情報の初期化
        /// </summary>
        private void InitializeSchoolInfo()
        {
            if (_schoolInfo == null)
            {
                _schoolInfo = new SchoolInfo
                {
                    Id = GenerateNewId(),
                    SchoolType = SchoolType.High,
                    Name = string.Empty,
                    Classes = new List<string>(),
                    Clubs = new List<string>(),
                    Description = string.Empty,
                    NotesSupplement = string.Empty
                };
                Title = "新規学校情報登録";
            }
            else
            {
                Title = $"学校情報編集 - {_schoolInfo.Name}";
            }
        }

        /// <summary>
        /// データをUIに反映
        /// </summary>
        private void LoadDataToUI()
        {
            // 学校タイプの設定
            var schoolTypeItem = SchoolTypeCombo.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag.ToString() == _schoolInfo.SchoolType.ToString());
            if (schoolTypeItem != null)
            {
                SchoolTypeCombo.SelectedItem = schoolTypeItem;
            }

            // TagInputView のデータ設定
            var model = (SEW_Model)DataContext;
            model.Classes.Items.Clear();
            model.Clubs.Items.Clear();

            foreach (var className in _schoolInfo.Classes)
            {
                model.Classes.Items.Add(className);
            }

            foreach (var clubName in _schoolInfo.Clubs)
            {
                model.Clubs.Items.Add(clubName);
            }
        }

        /// <summary>
        /// UIからデータを取得
        /// </summary>
        private void SaveDataFromUI()
        {
            // 学校タイプの取得
            if (SchoolTypeCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                if (Enum.TryParse<SchoolType>(selectedItem.Tag.ToString(), out var schoolType))
                {
                    _schoolInfo.SchoolType = schoolType;
                }
            }

            // TagInputView のデータ取得
            var model = (SEW_Model)DataContext;
            _schoolInfo.Classes = model.Classes.Items.ToList();
            _schoolInfo.Clubs = model.Clubs.Items.ToList();
        }

        /// <summary>
        /// 新しいIDを生成
        /// </summary>
        private int GenerateNewId()
        {
            // 実際の実装では、既存のIDから最大値を取得して+1する
            return new Random().Next(1000, 9999);
        }

        /// <summary>
        /// 保存ボタンクリック
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 入力チェック
                if (string.IsNullOrWhiteSpace(SchoolNameTextBox.Text))
                {
                    MessageBox.Show("学校名を入力してください。", "入力エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    SchoolNameTextBox.Focus();
                    return;
                }

                // データ保存
                SaveDataFromUI();

                // 実際の保存処理（ここでは省略）
                // SchoolInfoService.Save(_schoolInfo);

                MessageBox.Show("学校情報を保存しました。", "保存完了",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました: {ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
            Classes = new TagInputViewModel("クラス", "classes");
            Clubs = new TagInputViewModel("部活", "clubs");
        }
    }
}