using Mantensei_Database.DataAccess;
using Mantensei_Database.Models;
using MantenseiLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Mantensei_Database.Controls
{
    /// <summary>
    /// ISaveDataProviderを実装したシンプルなコンボボックス
    /// </summary>
    public partial class IdBasedComboBox : UserControl, ISaveDataProvider
    {
        public IdBasedComboBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 内部のComboBoxを取得
        /// </summary>
        public ComboBox ComboBox => InternalComboBox;

        #region ISaveDataProvider実装

        /// <summary>
        /// 選択されたアイテムのIDを文字列として返す
        /// </summary>
        public IEnumerable<string> GetSaveItems()
        {
            if (InternalComboBox.SelectedItem is IProfile profile)
            {
                yield return profile.Id.ToString();
            }
        }

        /// <summary>
        /// IDからアイテムを復元選択
        /// </summary>
        public void LoadItem(IEnumerable<string> items)
        {
            var idString = items?.FirstOrDefault();
            if (string.IsNullOrEmpty(idString) || !int.TryParse(idString, out int id))
            {
                InternalComboBox.SelectedIndex = -1;
                return;
            }

            // アイテムソースからIDに一致するアイテムを検索して選択
            if (InternalComboBox.ItemsSource != null)
            {
                foreach (var item in InternalComboBox.ItemsSource)
                {
                    if (item is IProfile profile && profile.Id == id)
                    {
                        InternalComboBox.SelectedItem = item;
                        return;
                    }
                }
            }

            // 見つからない場合は選択解除
            InternalComboBox.SelectedIndex = -1;
        }

        #endregion
    }
}