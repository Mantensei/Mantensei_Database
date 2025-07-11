using Mantensei_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using MantenseiLib;
using System.Diagnostics;

namespace Mantensei_Database.Models
{
    // データ変換ロジッククラス
    // 対象探索のスコープ（自己要素 or 子要素）
    public enum SaveTargetScope { Self, Children }

    // セーブ対象属性
    [AttributeUsage(AttributeTargets.Property)]
    public class SaveTargetAttribute : Attribute
    {
        public string Key { get; }
        public SaveTargetScope Scope { get; set; } = SaveTargetScope.Self;
        public SaveTargetAttribute(string key) => Key = key;
        public SaveTargetAttribute(string key, SaveTargetScope scope)
        {
            Key = key;
            Scope = scope;
        }
    }

    // データ変換ロジッククラス
    public static class CharacterProfileConverter
    {
        public static CharacterProfile LoadFromUI(DependencyObject root)
        {
            var profile = new CharacterProfile();
            SaveFromUI(root, profile);
            return profile;
        }

        public static void SaveFromUI(DependencyObject root, CharacterProfile profile)
        {
            foreach (var pair in AttributeUtility.GetAttributedFields<SaveTargetAttribute>(profile))
            {
                var attr = pair.Attribute;
                var element = FindElementByKey(root, attr.Key, attr.Scope);
                if (element == null) continue;

                var value = ExtractValue(element);
                if (value != null)
                {
                    try
                    {
                        switch (pair.MemberInfo)
                        {
                            case PropertyInfo prop when prop.CanWrite:
                                prop.SetValue(profile, Convert.ChangeType(value, prop.PropertyType));
                                break;
                            case FieldInfo field:
                                field.SetValue(profile, Convert.ChangeType(value, field.FieldType));
                                break;
                        }

                    }
                    catch 
                    {
                        Debug.WriteLine($"Failed to set value for {pair.MemberInfo.Name} with value {value}");
                    }
                }
            }
        }

        public static void LoadToUI(DependencyObject root, CharacterProfile profile)
        {
            foreach (var pair in AttributeUtility.GetAttributedFields<SaveTargetAttribute>(profile))
            {
                var attr = pair.Attribute;
                var element = FindElementByKey(root, attr.Key, attr.Scope);
                if (element == null) continue;
                var value = pair.GetValue(profile);
                ApplyValue(element, value);
            }
        }

        private static DependencyObject? FindElementByKey(DependencyObject root, string key, SaveTargetScope scope)
        {
            // 自分自身をチェック
            if (root is FrameworkElement fe && (fe.Tag?.ToString() == key || fe.Name == key))
                return root;

            // 子要素を再帰的に検索
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var found = FindElementByKey(child, key, scope);  // scopeを維持
                if (found != null) return found;
            }

            return null;
        }

        private static object? ExtractValue(DependencyObject element) => element switch
        {
            TextBox tb => tb.Text,
            ComboBox cb => cb.SelectedItem,
            CheckBox chk => chk.IsChecked ?? false,
            Slider slider => slider.Value,
            StackPanel sp => sp.Children.OfType<DependencyObject>().Select(x => ExtractValue(x)).ToList(),
            _ => null
        };

        private static void ApplyValue(DependencyObject element, object? value)
        {
            switch (element)
            {
                case TextBox tb:
                    tb.Text = value?.ToString() ?? string.Empty;
                    break;
                case ComboBox cb:
                    // ComboBoxの場合、SelectedItemの設定方法を改善
                    if (value != null)
                    {
                        // 文字列の場合はTextで設定
                        if (value is string stringValue)
                        {
                            cb.Text = stringValue;
                        }
                        else
                        {
                            cb.SelectedItem = value;
                        }
                    }
                    break;
                case CheckBox chk when value is bool b:
                    chk.IsChecked = b;
                    break;
                case Slider slider when value is double d:
                    slider.Value = d;
                    break;
                case Slider slider when value is int i:
                    slider.Value = i;
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"未対応の要素タイプ: {element.GetType().Name}");
                    break;
            }
        }
    }
}
