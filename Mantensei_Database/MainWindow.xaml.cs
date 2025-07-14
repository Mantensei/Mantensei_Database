using Mantensei_Database.Pages;
using Mantensei_Database.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace Mantensei_Database
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// シングルトンハブとして機能
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private static MainWindow _instance;
        private readonly Dictionary<NavigationPageType, Page> _pages;
        private NavigationPageType _currentPage;

        public CharacterListPage CharacterListPage => _pages[NavigationPageType.CharacterList] as CharacterListPage;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static MainWindow Instance => _instance;

        /// <summary>
        /// 現在のページタイプ
        /// </summary>
        public NavigationPageType CurrentPage => _currentPage;

        public MainWindow()
        {
            InitializeComponent();
            WarmUpJIT();

            // シングルトンチェック
            if (_instance != null && _instance.IsLoaded)
            {
                _instance.Activate();
                this.Close();
                return;
            }

            _instance = this;
            _pages = new Dictionary<NavigationPageType, Page>();

            // 初期ページの設定
            NavigateToPage(NavigationPageType.Home);

            new Mantensei_Database.Windows.SchoolEditorWindow().Show();
        }

        /// <summary>
        /// 指定されたページに移動
        /// </summary>
        public void NavigateToPage(NavigationPageType pageType)
        {
            try
            {
                // 既にそのページを表示中なら何もしない
                if (_currentPage == pageType && MainFrame.Content != null)
                    return;

                // ページインスタンスを取得または作成
                if (!_pages.TryGetValue(pageType, out Page page))
                {
                    page = CreatePage(pageType);
                    _pages[pageType] = page;
                }

                // フレームにページを設定
                MainFrame.Content = page;
                _currentPage = pageType;

                // ナビゲーションボタンの状態を更新
                UpdateNavigationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ページの読み込みに失敗しました: {ex.Message}",
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ページインスタンスを作成
        /// </summary>
        private Page CreatePage(NavigationPageType pageType)
        {
            return pageType switch
            {
                //NavigationPageType.Home => new HomePage(),
                NavigationPageType.CharacterList => new CharacterListPage(),
                //NavigationPageType.Statistics => new StatisticsPage(),
                //NavigationPageType.Settings => new SettingsPage(),
                _ => throw new ArgumentException($"未対応のページタイプ: {pageType}")
            };
        }

        /// <summary>
        /// ナビゲーションボタンの状態を更新
        /// </summary>
        private void UpdateNavigationButtons()
        {
            // 全ボタンを非アクティブ状態に
            HomeButton.Style = (Style)FindResource("NavButton");
            CharacterListButton.Style = (Style)FindResource("NavButton");
            StatisticsButton.Style = (Style)FindResource("NavButton");
            SettingsButton.Style = (Style)FindResource("NavButton");

            // 現在のページに対応するボタンをアクティブ状態に
            var activeStyle = (Style)FindResource("ActiveNavButton");
            switch (_currentPage)
            {
                case NavigationPageType.Home:
                    HomeButton.Style = activeStyle;
                    break;
                case NavigationPageType.CharacterList:
                    CharacterListButton.Style = activeStyle;
                    break;
                case NavigationPageType.Statistics:
                    StatisticsButton.Style = activeStyle;
                    break;
                case NavigationPageType.Settings:
                    SettingsButton.Style = activeStyle;
                    break;
            }
        }

        // JITを温めておく
        void WarmUpJIT()
        {
            _ = DateTime.TryParse("2000/1/1", out _);
            _ = new Random().Next();
            _ = typeof(List<int>).GetHashCode();
        }


        #region ナビゲーションボタンイベントハンドラ

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(NavigationPageType.Home);
        }

        private void CharacterListButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(NavigationPageType.CharacterList);
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(NavigationPageType.Statistics);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(NavigationPageType.Settings);
        }

        #endregion
    }
}

// NavigationConstants.cs
namespace Mantensei_Database.Models
{
    /// <summary>
    /// ナビゲーションページの種類
    /// </summary>
    public enum NavigationPageType
    {
        Home,
        CharacterList,
        Statistics,
        Settings
    }
}