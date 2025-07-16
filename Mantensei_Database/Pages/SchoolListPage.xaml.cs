using Mantensei_Database.Bindings;
using Mantensei_Database.Controls;
using Mantensei_Database.Models;
using Mantensei_Database.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mantensei_Database.Pages
{
    public partial class SchoolListPage : Page
    {
        private static ObservableCollection<SchoolListItem> _schools = new();
        private ICollectionView _filteredView;

        public SchoolListPage()
        {
            InitializeComponent();

            _filteredView = CollectionViewSource.GetDefaultView(_schools);
            //_filteredView.Filter = filer

            SchoolDataGrid.ItemsSource = _filteredView;
            LoadAll();
            Update();
        }

        void LoadAll() => ProfileService.LoadAll<SchoolProfile, SchoolListItem>(_schools);
        /// <summary>
        /// 学校データを読み込み
        /// </summary>
        public void Update()
        {
            _schools.Clear();

            try
            {
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"学校データの読み込みに失敗しました: {ex.Message}",
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateStatus();
        }

        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        private void UpdateStatus()
        {
            if (_schools == null)
            {
                return;
            }

            var totalCount = _schools.Count;
            var filteredCount = _filteredView.Cast<object>().Count();

            if (totalCount == filteredCount)
            {
                StatusTextBlock.Text = $"{totalCount}件の学校";
            }
            else
            {
                StatusTextBlock.Text = $"{filteredCount}件の学校（全{totalCount}件中）";
            }
        }

        /// <summary>
        /// 選択された学校を取得
        /// </summary>
        private SchoolListItem GetSelectedSchool()
        {
            return SchoolDataGrid.SelectedItem as SchoolListItem;
        }

        #region イベントハンドラ

        private void SchoolTypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _filteredView?.Refresh();
            UpdateStatus();
        }

        private void SchoolDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedSchool = GetSelectedSchool();
            if (selectedSchool != null)
            {
                OpenProfileEditor(selectedSchool);
            }
        }

        private void SchoolDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hasSelection = GetSelectedSchool() != null;
            EditSchoolButton.IsEnabled = hasSelection;
            DeleteSchoolButton.IsEnabled = hasSelection;
        }

        private void NewSchoolButton_Click(object sender, RoutedEventArgs e)
        {
            var editorWindow = new SchoolEditorWindow();

            if (editorWindow.ShowDialog() == true)
            {
                Update();
            }
        }

        private void EditSchoolButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSchool = GetSelectedSchool();
            if (selectedSchool != null)
            {
                OpenProfileEditor(selectedSchool);
            }
        }

        private void DeleteSchoolButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSchool = GetSelectedSchool();
            if (selectedSchool == null) return;

            var result = MessageBox.Show(
                $"「{selectedSchool.Name}」を削除しますか？\nこの操作は取り消せません。",
                "削除確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // 実際の実装では、ファイルやデータベースから削除処理を行う
                    // 現在はダミーデータのため、リストから削除のみ
                    _schools.Remove(selectedSchool);

                    MessageBox.Show("学校情報を削除しました。", "削除完了",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    UpdateStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除に失敗しました: {ex.Message}", "エラー",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        #endregion

        /// <summary>
        /// 学校エディタを開く
        /// </summary>
        private void OpenProfileEditor(SchoolListItem character)
        {
            try
            {
                var profile = ProfileService.LoadFromXml<SchoolProfile>(character.FilePath);
                var editorWindow = new Mantensei_Database.Windows.SchoolEditorWindow(profile);
                editorWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"読み込みに失敗しました: {ex.Message}",
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

namespace Mantensei_Database.Models
{
    /// <summary>
    /// 学校一覧表示用のデータモデル
    /// </summary>
    public class SchoolListItem : ProfileListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SchoolType SchoolType { get; set; }
        public string SchoolTypeDisplay { get; set; }
        public int ClassCount { get; set; }
        public int ClubCount { get; set; }
        public string Description { get; set; }

        public override void InitProfile(IProfile prof, string filePath)
        {
            base.InitProfile(prof, filePath);
            var profile = prof as SchoolProfile;

            Id = profile.Id;
            Name = profile.Name ?? "";
            SchoolType = profile.SchoolType;
            SchoolTypeDisplay = GetSchoolTypeDisplay(profile.SchoolType);
            ClassCount = profile.Classes?.Count ?? 0;
            ClubCount = profile.Clubs?.Count ?? 0;
            Description = profile.Description ?? "";
        }

        private string GetSchoolTypeDisplay(SchoolType type)
        {
            return type switch
            {
                SchoolType.Elementary => "小学校",
                SchoolType.Middle => "中学校",
                SchoolType.High => "高等学校",
                SchoolType.Other => "その他",
                _ => "不明"
            };
        }
    }
}
