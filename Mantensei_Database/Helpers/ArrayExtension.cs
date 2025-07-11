using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
#endif
using System.Linq;
using System;

namespace MantenseiLib
{
    public static partial class MantenseiUtilities
    {
        public static void Log(object message)
        {
#if UNITY_EDITOR
    Debug.Log(message);
#else
            if (IsWpf())
            {
                System.Diagnostics.Debug.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
#endif
        }

        private static bool IsWpf()
        {
            // WPFが使えるかどうかを動的に判定
            // PresentationFramework.dll が読み込まれていたら WPF
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Any(a => a.GetName().Name == "PresentationFramework");
        }


        public static int RandomRange(int begin, int end)
        {
#if UNITY_EDITOR
            return UnityEngine.Random.Range(begin, end);
#else
            return new System.Random().Next(begin, end);
#endif
        }
    }

    public static partial class ArrayExtension
    {
        public static bool ExistsNext<T>(this IEnumerable<T> source, T item)
        {
            var index = source.ToList().IndexOf(item);
            return index < source.Count() - 1;
        }

        public static bool ExistsNext<T>(this IEnumerable<T> source, T item, out T next)
        {
            var index = source.ToList().IndexOf(item);
            var result = source.ExistsNext(item);

            if (result == true)
                next = source.GetNextOrDefault(item);
            else
                next = default!;

            return result;
        }

        public static T GetNextOrDefault<T>(this IEnumerable<T> source, T item)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (Equals(enumerator.Current, item))
                    {
                        if (enumerator.MoveNext())
                        {
                            return enumerator.Current;
                        }
                    }
                }
            }

            return default!;
        }

        public static T GetNextLoop<T>(this IEnumerable<T> source, T item)
        {
            if (source.ExistsNext(item))
                return GetNextOrDefault(source, item);
            else
                return source.First();
        }

        public static IEnumerable<T> Log<T, TResult>(this IEnumerable<T> values, System.Func<T, TResult> func)
        {
            MantenseiUtilities.Log(values.JoinToString(func));
            return values;
        }
        
        public static IEnumerable<T> Log<T>(this IEnumerable<T> values, string fore = "", string back = "")
        {
            MantenseiUtilities.Log(fore + values.JoinToString() + back);
            return values;
        }

        public static string JoinToString<T, TResult>(this IEnumerable<T> values, System.Func<T, TResult> func)
        {
            return values.Select(func).JoinToString();
        }

        public static string JoinToString<T>(this IEnumerable<T> values)
        {
            return values.JoinToString(", ");
        }

        public static string JoinToString<T>(this IEnumerable<T> values, string separation)
        {
            return values.JoinToString(separation, "(", ")");
        }

        public static string JoinToString<T>(this IEnumerable<T> values, string separation, string begin, string end)
        {
            return $"{begin}{string.Join(separation, values)}{end}";
        }

        public static IEnumerable<T> LogAll<T>(this IEnumerable<T> values)
        {
            MantenseiUtilities.Log(values.JoinToString());
            return values;
        }
            
        public static bool IsWithinRange<T>(this IEnumerable<T> values, int index)
        {
            return 0 <= index && index < values.Count();
        }

        public static bool IsNullOrEmptyArray<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (!source.Any())
                yield break;

            List<T> list = source.ToList();
            int count = list.Count;

            while (count > 1)
            {
                count--;
                int index = MantenseiUtilities.RandomRange(0, count + 1);
                yield return list[index];
                list[index] = list[count];
            }

            yield return list[0];
        }

        public static T GetRandomElementOrDefault<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            // IEnumerable<T>をリストに変換してランダムな要素を取得
            List<T> list = new List<T>(source);

            if (list.Count == 0)
                return default!;

            int index = new System.Random().Next(0, list.Count);
            return list[index];
        }

        public static IEnumerable<T> GetRandomElements<T>(this IEnumerable<T> source, int count)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }
            if (source.Count() <= count)
            {
                foreach (var element in source)
                    yield return element;
            }

            List<T> list = new List<T>(source);
            var random = new System.Random();

            for (int i = 0; i < count; i++)
            {
                int index = random.Next(0, list.Count);
                yield return list[index];
            }
        }

        public static T LoopElementAt<T>(this IEnumerable<T> source, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.Any())
            {
                throw new InvalidOperationException("The source sequence is empty.");
            }

            // インデックスが負の場合、正のインデックスに変換
            if (index < 0)
            {
                index = source.Count() + (index % source.Count());
            }

            return source.ElementAt(index % source.Count());
        }

        public static void AddIfNotContains<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
        public static IEnumerable<T> Trim<T>(this IEnumerable<T> array, int start, int length)
        {
            return array.Skip(start).Take(length);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> self)
        {
            return self.SelectMany(c => c);
        }

        public static bool MatchAll<T>(this IEnumerable<T> array)
        {
            foreach(var i in array)
            {
                foreach(var j in array)
                {
                    if (!i?.Equals(j) == true)
                        return false;
                }
            }
            return true;

        }
        
        public static bool MatchAll<T>(this IEnumerable<T> array, Func<T, T, bool> predicate)
        {
            foreach(var i in array)
            {
                foreach(var j in array)
                {
                    if (!predicate(i, j))
                        return false;
                }
            }
            return true;

        }


        public static bool Contains<T>(this IEnumerable<T> array, Func<T, bool> predicate)
        {
            if (predicate is null)
                return array.Contains(default(T));

            return array
                .Where(predicate)
                .Any();
        }

        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> source) where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Where(item => item != null);
        }
        
        /// <summary>
        /// 一意のペアに関数を適用
        /// </summary>
        public static IEnumerable<Result> ApplyFuncToUniquePairsExcludingSelf<T, Result>(this IEnumerable<T> source, Func<T, T, Result> func)
        {
            var array = source.ToArray();
            int count = array.Length;

            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    yield return func.Invoke(array[i], array[j]);
                }
            }
        }

        public static void ApplyActionToUniquePairsExcludingSelf<T>(this IEnumerable<T> source, Action<T, T> action)
        {
            var array = source.ToArray();
            int count = array.Length;

            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    action(array[i], array[j]);
                }
            }
        }


        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static void RemoveRange<T>(this List<T> list, IEnumerable<T> itemsToRemove)
        {
            var array = itemsToRemove.ToArray();

            for (int i = 0; i < array.Length; i++)
                list.Remove(array[i]);
        }

        //public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        //{
        //    foreach (var element in source)
        //    {
        //        if (predicate(element))
        //        {
        //            return element;
        //        }
        //    }

        //    return default(TSource);
        //}
    }


    public static class DictionaryExtension
    {
        /// <summary>
        /// 値を取得、keyがなければデフォルト値を設定し、デフォルト値を取得
        /// </summary>
        public static TV? GetOrDefault<TK, TV>(this Dictionary<TK, TV> dic, TK key, TV? defaultValue = default)
            where TK : notnull
        {
            return dic.TryGetValue(key, out var result) ? result : defaultValue;
        }
    }



    public static class StackExtension
    {
        public static T PeekOrDefault<T>(this Stack<T> stack)
        {
            if (stack.Count <= 0)
                return default(T)!;

            else
                return stack.Peek();
        }
    }
}