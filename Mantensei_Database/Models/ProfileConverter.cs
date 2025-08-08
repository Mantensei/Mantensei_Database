using Mantensei_Database.Models;
using Mantensei_Database.DataAccess;
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
using MantenseiLib.WPF;

namespace Mantensei_Database.Models
{
    public interface IProfile 
    {
        int Id { get; set; }
    }

    // データ変換ロジッククラス
    // 対象探索のスコープ（自己要素 or 子要素）
    public enum SaveTargetScope { Self, Children }

    // セーブ対象属性
    [AttributeUsage(AttributeTargets.Property)]
    public class SaveTargetAttribute : Attribute
    {
        public string Key { get; }
        public SaveTargetScope Scope { get; set; } = SaveTargetScope.Self;
        public SaveTargetType DataType { get; set; } = SaveTargetType.Single;

        public SaveTargetAttribute(string key) => Key = key;

        public SaveTargetAttribute(string key, SaveTargetType dataType)
        {
            Key = key;
            DataType = dataType;
        }

        public SaveTargetAttribute(string key, SaveTargetScope scope, SaveTargetType dataType)
        {
            Key = key;
            Scope = scope;
            DataType = dataType;
        }
    }

    public enum SaveTargetType
    {
        Single,     // 単一データ (string, int, etc.)
        Multiple,   // 複数データ (List<string>)
        Complex     // 複雑なオブジェクト (List<DeesItem>)
    }


    // データ変換ロジッククラス
    // 改良されたIProfileConverter
    public static class ProfileConverter
    {
        public static IProfile LoadFromUI(DependencyObject root, in IProfile profile)
        {
            SaveFromUI(root, profile);
            return profile;
        }

        public static void SaveFromUI(DependencyObject root, IProfile profile)
        {
            foreach (var pair in AttributeUtility.GetAttributedFields<SaveTargetAttribute>(profile))
            {
                var attr = pair.Attribute;
                var element = FindElementByKey(root, attr.Key, attr.Scope);
                if (element == null) continue;

                try
                {
                    Debug.WriteLine($" -- Setting value for {pair.MemberInfo.Name} with key {attr.Key} and scope {attr.Scope}");

                    switch (attr.DataType)
                    {
                        case SaveTargetType.Single:
                            SetSingleValue(element, pair, profile);
                            break;
                        case SaveTargetType.Multiple:
                            SetMultipleValue(element, pair, profile);
                            break;
                        case SaveTargetType.Complex:
                            SetComplexValue(element, pair, profile);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to set value for {pair.MemberInfo.Name}: {ex.Message}");
                }
            }
        }

        private static void SetSingleValue(DependencyObject element, AttributedMember<SaveTargetAttribute> pair, IProfile profile)
        {
            var value = ExtractValue(element);
            Debug.WriteLine($"Extracted value for {pair.MemberInfo.Name}: {value}");

            if (value == null) return;

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

        private static void SetMultipleValue(DependencyObject element, AttributedMember<SaveTargetAttribute> pair, IProfile profile)
        {
            var stringList = ExtractStringList(element);

            if (stringList == null) return;

            switch (pair.MemberInfo)
            {
                case PropertyInfo prop when prop.CanWrite && prop.PropertyType == typeof(List<string>):
                    prop.SetValue(profile, stringList);
                    break;
                case FieldInfo field when field.FieldType == typeof(List<string>):
                    field.SetValue(profile, stringList);
                    break;
            }
        }

        private static void SetComplexValue(DependencyObject element, AttributedMember<SaveTargetAttribute> pair, IProfile profile)
        {
            switch(pair.MemberInfo.Name)
            {
                case "Dees":
                    var stringList = ExtractStringList(element);
                    var deesList = stringList.Select(s => new DeesItem { Text = s, Used = false }).ToList();

                    switch (pair.MemberInfo)
                    {
                        case PropertyInfo prop when prop.CanWrite:
                            prop.SetValue(profile, deesList);
                            break;
                        case FieldInfo field:
                            field.SetValue(profile, deesList);
                            break;
                    }
                    break;

                default:
                    Debug.WriteLine($"Setting complex value for unknown type: {pair.MemberInfo.Name}");
                    break;
            }
        }

        // 複数データを抽出する新しいメソッド
        private static List<string> ExtractStringList(DependencyObject element)
        {
            switch (element)
            {
                case ISaveDataProvider saveDataProvider:
                    return saveDataProvider.GetSaveItems().ToList();
            }

            return
            element.GetComponentsInChildren<DependencyObject>()
                        .Select(child => ExtractValue(child)?.ToString())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .OfType<string>()
                        .ToList();
        }

        // LoadToUIメソッドも同様に更新
        public static void LoadToUI(DependencyObject root, IProfile profile)
        {
            foreach (var pair in AttributeUtility.GetAttributedFields<SaveTargetAttribute>(profile))
            {
                var attr = pair.Attribute;
                var element = FindElementByKey(root, attr.Key, attr.Scope);
                if (element == null) continue;

                var value = pair.GetValue(profile);

                switch (attr.DataType)
                {
                    case SaveTargetType.Single:
                        ApplyValue(element, value);
                        break;
                    case SaveTargetType.Multiple:
                        ApplyStringList(element, value as List<string>);
                        break;
                    case SaveTargetType.Complex:
                        ApplyComplexValue(element, value);
                        break;
                }
            }
        }

        private static void ApplyStringList(DependencyObject element, List<string> values)
        {
            if (values?.Count == 0) return;

            switch (element)
            {
                case ISaveDataProvider saveDataProvider:
                    saveDataProvider.LoadItem(values);
                    break;

                default:
                    Debug.WriteLine($"Unsupported element type for string list: {element.GetType().Name}");
                    break;
            }
        }

        private static void ApplyComplexValue(DependencyObject element, object value)
        {
            if (value is List<DeesItem> deesList)
            {
                var stringList = deesList.Select(d => d.Text).ToList();
                ApplyStringList(element, stringList);
            }
        }

        private static object? ExtractValue(DependencyObject element) => element switch
        {
            ComboBox cb when cb.SelectedItem is IProfile profile => profile.Id.ToString(),
            ComboBox cb => cb.Text,
            TextBox tb => tb.Text,
            CheckBox chk => chk.IsChecked ?? false,
            Slider slider => slider.Value,
            TextBlock textBlock => textBlock.Text,
            _ => null
        };

        private static void ApplyValue(DependencyObject element, object? value)
        {
            switch (element)
            {
                case ComboBox cb when cb.ItemsSource != null && int.TryParse(value?.ToString(), out int id):
                    // ItemsSourceからIDに一致するアイテムを検索
                    foreach (var item in cb.ItemsSource)
                    {
                        if (item is IProfile profile && profile.Id == id)
                        {
                            cb.SelectedItem = item;
                            return;
                        }
                    }
                    cb.SelectedIndex = -1;
                    break;
                case ComboBox cb:
                    if (value != null)
                    {
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
                case TextBox tb:
                    tb.Text = value?.ToString() ?? string.Empty;
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
            }
        }
        private static DependencyObject? FindElementByKey(DependencyObject root, string key, SaveTargetScope scope)
        {
            if (root is FrameworkElement fe && (fe.Tag?.ToString() == key || fe.Name == key))
                return root;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var found = FindElementByKey(child, key, scope);
                if (found != null) return found;
            }

            return null;
        }
    }
}
