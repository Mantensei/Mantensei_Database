using Mantensei_Database.Common;
using Mantensei_Database.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace Mantensei_Database.Pages
{
    public partial class CharacterListPage : Page
    {
        private static ObservableCollection<CharacterListItem> _characters;

        private ICollectionView _filteredView;

        public CharacterListPage()
        {
            InitializeComponent();

            _characters = new ObservableCollection<CharacterListItem>();
            _filteredView = CollectionViewSource.GetDefaultView(_characters);
            _filteredView.Filter = CharacterFilter;

            CharacterDataGrid.ItemsSource = _filteredView;
            LoadCharacters();
            SetupFilterComboBoxes();
        }

        /// <summary>
        /// キャラクターデータを読み込み
        /// </summary>
        public void LoadCharacters()
        {
            _characters.Clear();

            try
            {
                var profilesDir = FileSystemUtility.GetProfilesDirectory();
                if (!Directory.Exists(profilesDir))
                {
                    UpdateStatus();
                    return;
                }

                var xmlFiles = Directory.GetFiles(profilesDir, "*.xml");

                foreach (var filePath in xmlFiles)
                {
                    try
                    {
                        var profile = CharacterProfileService.LoadFromXml(filePath);
                        var listItem = new CharacterListItem(profile, filePath);
                        _characters.Add(listItem);
                        CharacterDataBase.AddProfile(profile);
                    }
                    catch (Exception ex)
                    {
                        // 個別ファイルの読み込みエラーは無視して続行
                        System.Diagnostics.Debug.WriteLine($"Failed to load {filePath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"キャラクターデータの読み込みに失敗しました: {ex.Message}",
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateStatus();
        }

        /// <summary>
        /// フィルタ用コンボボックスの設定
        /// </summary>
        private void SetupFilterComboBoxes()
        {
            // クラスフィルタの設定
            var classes = _characters.Select(c => c.Class)
                                   .Where(c => !string.IsNullOrEmpty(c))
                                   .Distinct()
                                   .OrderBy(c => c)
                                   .ToList();

            ClassFilterComboBox.Items.Clear();
            ClassFilterComboBox.Items.Add(new ComboBoxItem { Content = "すべて", IsSelected = true });
            foreach (var cls in classes)
            {
                ClassFilterComboBox.Items.Add(new ComboBoxItem { Content = cls });
            }

            // 部活フィルタの設定
            var clubs = _characters.Select(c => c.Club)
                                  .Where(c => !string.IsNullOrEmpty(c))
                                  .Distinct()
                                  .OrderBy(c => c)
                                  .ToList();

            ClubFilterComboBox.Items.Clear();
            ClubFilterComboBox.Items.Add(new ComboBoxItem { Content = "すべて", IsSelected = true });
            foreach (var club in clubs)
            {
                ClubFilterComboBox.Items.Add(new ComboBoxItem { Content = club });
            }
        }

        /// <summary>
        /// フィルタ条件
        /// </summary>
        private bool CharacterFilter(object item)
        {
            if (!(item is CharacterListItem character)) return false;

            // 検索テキストフィルタ
            var searchText = SearchTextBox?.Text?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                var searchTargets = new[]
                {
                    character.FullName,
                    character.Kana,
                    character.Class,
                    character.Club
                };

                if (!searchTargets.Any(target => target?.ToLower().Contains(searchText) == true))
                {
                    return false;
                }
            }

            // クラスフィルタ
            var selectedClass = (ClassFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(selectedClass) && selectedClass != "すべて")
            {
                if (character.Class != selectedClass)
                {
                    return false;
                }
            }

            // 部活フィルタ
            var selectedClub = (ClubFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(selectedClub) && selectedClub != "すべて")
            {
                if (character.Club != selectedClub)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        private void UpdateStatus()
        {
            if (_characters == null)
            {
                return;
            }

            var totalCount = _characters.Count;
            var filteredCount = _filteredView.Cast<object>().Count();

            if (totalCount == filteredCount)
            {
                StatusTextBlock.Text = $"{totalCount}件のキャラクター";
            }
            else
            {
                StatusTextBlock.Text = $"{filteredCount}件のキャラクター（全{totalCount}件中）";
            }
        }

        /// <summary>
        /// 選択されたキャラクターを取得
        /// </summary>
        private CharacterListItem GetSelectedCharacter()
        {
            return CharacterDataGrid.SelectedItem as CharacterListItem;
        }

        #region イベントハンドラ

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _filteredView.Refresh();
            UpdateStatus();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _filteredView.Refresh();
            UpdateStatus();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            _filteredView.Refresh();
            UpdateStatus();
        }

        private void ClassFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _filteredView?.Refresh();
            UpdateStatus();
        }

        private void ClubFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _filteredView?.Refresh();
            UpdateStatus();
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            ClassFilterComboBox.SelectedIndex = 0;
            ClubFilterComboBox.SelectedIndex = 0;
            _filteredView.Refresh();
            UpdateStatus();
        }

        private void CharacterDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedCharacter = GetSelectedCharacter();
            if (selectedCharacter != null)
            {
                OpenProfileEditor(selectedCharacter);
            }
        }

        private void CharacterDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hasSelection = GetSelectedCharacter() != null;
            EditCharacterButton.IsEnabled = hasSelection;
            DeleteCharacterButton.IsEnabled = hasSelection;
        }

        private void NewCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var editorWindow = new Mantensei_Database.Windows.ProfileEditorWindow();
            //editorWindow.Owner = this;

            if (editorWindow.ShowDialog() == true)
            {
                LoadCharacters();
                SetupFilterComboBoxes();
            }
        }

        private void EditCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCharacter = GetSelectedCharacter();
            if (selectedCharacter != null)
            {
                OpenProfileEditor(selectedCharacter);
            }
        }

        private void DeleteCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCharacter = GetSelectedCharacter();
            if (selectedCharacter == null) return;

            var result = MessageBox.Show(
                $"「{selectedCharacter.FullName}」を削除しますか？\nこの操作は取り消せません。",
                "削除確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.Delete(selectedCharacter.FilePath);
                    LoadCharacters();
                    SetupFilterComboBoxes();
                    MessageBox.Show("キャラクターを削除しました。", "削除完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCharacters();
            SetupFilterComboBoxes();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }

        #endregion

        /// <summary>
        /// プロファイルエディタを開く
        /// </summary>
        private void OpenProfileEditor(CharacterListItem character)
        {
            try
            {
                var profile = CharacterProfileService.LoadFromXml(character.FilePath);
                var editorWindow = new Mantensei_Database.Windows.ProfileEditorWindow(profile);
                //editorWindow.Owner = this;

                if (editorWindow.ShowDialog() == true)
                {
                    LoadCharacters();
                    SetupFilterComboBoxes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"キャラクターの読み込みに失敗しました: {ex.Message}",
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

namespace Mantensei_Database.Models
{
    /// <summary>
    /// キャラクター一覧表示用のデータモデル
    /// </summary>
    public class CharacterListItem
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Kana { get; set; }
        public string Class { get; set; }
        public string Club { get; set; }
        public CharacterStatus Status { get; set; }
        public DateTime LastModified { get; set; }
        public string FilePath { get; set; }

        public CharacterListItem(CharacterProfile profile, string filePath)
        {
            Id = profile.Id;
            FullName = profile.FullName ?? "";
            Kana = profile.Kana ?? "";
            Class = profile.Class ?? "";
            Club = profile.Club ?? "";
            Status = profile.Status;
            LastModified = File.GetLastWriteTime(filePath);
            FilePath = filePath;
        }
    }
}

namespace Mantensei_Database.Models
{
    public static class CharacterDataBase
    {
        static List<CharacterProfile> _allProfiles = new List<CharacterProfile>();
        public static CharacterProfile[] AllProfiles => _allProfiles.ToArray();

        public static void AddProfile(CharacterProfile profile)
        {
            _allProfiles.Add(profile);
        }
    }
}