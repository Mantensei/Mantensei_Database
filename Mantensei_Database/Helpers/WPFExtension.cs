using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MantenseiLib.WPF
{
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// 指定した型の子要素（孫以降も含む）をすべて取得します（UnityのGetComponentsInChildrenと同様）
        /// </summary>
        /// <typeparam name="T">取得したい要素の型</typeparam>
        /// <param name="parent">親要素</param>
        /// <param name="includeParent">親要素自体も検索対象に含めるか</param>
        /// <returns>指定した型の子要素のコレクション</returns>
        public static IEnumerable<T> GetComponentsInChildren<T>(this DependencyObject parent, bool includeParent = false)
            where T : DependencyObject
        {
            if (parent == null)
                yield break;

            // 親要素自体をチェック
            if (includeParent && parent is T parentAsT)
            {
                yield return parentAsT;
            }

            // 子要素を再帰的に検索
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // 子要素が指定した型の場合は返す
                if (child is T childAsT)
                {
                    yield return childAsT;
                }

                // 再帰的に孫要素以降を検索
                foreach (var descendant in GetComponentsInChildren<T>(child, false))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// 指定した型の子要素を1つだけ取得します（UnityのGetComponentInChildrenと同様）
        /// </summary>
        /// <typeparam name="T">取得したい要素の型</typeparam>
        /// <param name="parent">親要素</param>
        /// <param name="includeParent">親要素自体も検索対象に含めるか</param>
        /// <returns>指定した型の子要素（見つからない場合はnull）</returns>
        public static T GetComponentInChildren<T>(this DependencyObject parent, bool includeParent = false)
            where T : DependencyObject
        {
            return GetComponentsInChildren<T>(parent, includeParent).FirstOrDefault();
        }

        /// <summary>
        /// 指定した名前の子要素を取得します
        /// </summary>
        /// <typeparam name="T">取得したい要素の型</typeparam>
        /// <param name="parent">親要素</param>
        /// <param name="name">要素の名前</param>
        /// <param name="includeParent">親要素自体も検索対象に含めるか</param>
        /// <returns>指定した名前と型の子要素（見つからない場合はnull）</returns>
        public static T GetComponentInChildren<T>(this DependencyObject parent, string name, bool includeParent = false)
            where T : FrameworkElement
        {
            return GetComponentsInChildren<T>(parent, includeParent)
                .FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// 指定した名前の子要素をすべて取得します
        /// </summary>
        /// <typeparam name="T">取得したい要素の型</typeparam>
        /// <param name="parent">親要素</param>
        /// <param name="name">要素の名前</param>
        /// <param name="includeParent">親要素自体も検索対象に含めるか</param>
        /// <returns>指定した名前と型の子要素のコレクション</returns>
        public static IEnumerable<T> GetComponentsInChildren<T>(this DependencyObject parent, string name, bool includeParent = false)
            where T : FrameworkElement
        {
            return GetComponentsInChildren<T>(parent, includeParent)
                .Where(x => x.Name == name);
        }

    }
}