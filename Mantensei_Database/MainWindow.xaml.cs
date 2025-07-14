using Mantensei_Database.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mantensei_Database
{
    /// <summary>
    /// ナビゲーションアイテムの情報
    /// </summary>
    public class NavigationItem
    {
        public Type PageType { get; set; }
        public string Title { get; set; }
        public Button Button { get; set; }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// シングルトンハブとして機能
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow _instance;
        private readonly Dictionary<Type, Page> _pages = new();
        private readonly List<NavigationItem> _navigationItems = new();
        private Type _currentPageType;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static MainWindow Instance => _instance;

        // プロパティアクセス用
        public HomePage HomePage => GetPage<HomePage>();
        public CharacterListPage CharacterListPage => GetPage<CharacterListPage>();
        public SchoolListPage SchoolListPage => GetPage<SchoolListPage>();
        public StatisticsPage StatisticsPage => GetPage<StatisticsPage>();
        public SettingsPage SettingsPage => GetPage<SettingsPage>();

        public MainWindow()
        {
            InitializeComponent();
            WarmUpJIT();
            InitSingleton();
            InitNavigationItems();

            new Mantensei_Database.Windows.SchoolEditorWindow().Show();
        }

        void InitSingleton()
        {
            // シングルトンチェック
            if (_instance != null && _instance.IsLoaded)
            {
                _instance.Activate();
                this.Close();
                return;
            }

            _instance = this;
        }

        void InitNavigationItems()
        {
            // ナビゲーションアイテムを登録
            RegisterNavigationItem<HomePage>("🏠 ホーム");
            RegisterNavigationItem<CharacterListPage>("👥 一覧");
            RegisterNavigationItem<SchoolListPage>("🏫 登録");
            RegisterNavigationItem<StatisticsPage>("📊 統計");
            RegisterNavigationItem<SettingsPage>("⚙️ 設定");

            // 初期ページに移動
            NavigateToPage<HomePage>();
        }

        /// <summary>
        /// ナビゲーションアイテムを登録
        /// </summary>
        public void RegisterNavigationItem<T>(string title) where T : Page, new()
        {
            var button = new Button
            {
                Content = title,
                Style = (Style)FindResource("NavButton")
            };

            var navigationItem = new NavigationItem
            {
                PageType = typeof(T),
                Title = title,
                Button = button
            };

            button.Click += (sender, e) => NavigateToPage(typeof(T));

            _navigationItems.Add(navigationItem);
            NavigationPanel.Children.Add(button);
        }

        /// <summary>
        /// ページインスタンスを取得
        /// </summary>
        private T GetPage<T>() where T : Page
        {
            return _pages.TryGetValue(typeof(T), out var page) ? page as T : null;
        }

        /// <summary>
        /// 指定されたページに移動（ジェネリック版）
        /// </summary>
        public void NavigateToPage<T>() where T : Page
        {
            NavigateToPage(typeof(T));
        }

        /// <summary>
        /// 指定されたページに移動
        /// </summary>
        public void NavigateToPage(Type pageType)
        {
            try
            {
                // 既にそのページを表示中なら何もしない
                if (_currentPageType == pageType && MainFrame.Content != null)
                    return;

                // ページインスタンスを取得または作成
                if (!_pages.TryGetValue(pageType, out Page page))
                {
                    page = (Page)Activator.CreateInstance(pageType);
                    _pages[pageType] = page;
                }

                // フレームにページを設定
                MainFrame.Content = page;
                _currentPageType = pageType;

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
        /// ナビゲーションボタンの状態を更新
        /// </summary>
        private void UpdateNavigationButtons()
        {
            var activeStyle = (Style)FindResource("ActiveNavButton");
            var inactiveStyle = (Style)FindResource("NavButton");

            foreach (var item in _navigationItems)
            {
                item.Button.Style = item.PageType == _currentPageType ? activeStyle : inactiveStyle;
            }
        }

        /// <summary>
        /// JITを温めておく
        /// </summary>
        void WarmUpJIT()
        {
            _ = DateTime.TryParse("2000/1/1", out _);
            _ = new Random().Next();
            _ = typeof(List<int>).GetHashCode();
        }
    }
}

// 空のページクラス（実装予定）
namespace Mantensei_Database.Pages
{
    public partial class HomePage : Page
    {
    }

    public partial class SchoolListPage : Page
    {
    }

    public partial class CharacterListPage : Page
    {
    }

    public partial class StatisticsPage : Page
    {
    }

    public partial class SettingsPage : Page
    {
    }
}