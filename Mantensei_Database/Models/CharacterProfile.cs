﻿// CharacterProfile.cs - 純粋なデータモデル
using Mantensei_Database.Common;
using Mantensei_Database.Models;
using MantenseiLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace Mantensei_Database.Models
{
    public partial class CharacterProfile : IProfile
    {
        [SaveTarget("ID")]
        public int Id { get; set; }

        [SaveTarget("セイ")]
        public string SeiKana { get; set; }

        [SaveTarget("メイ")]
        public string MeiKana { get; set; }

        public string Kana => $"{SeiKana}_{MeiKana}";

        [SaveTarget("姓")]
        public string Sei { get; set; }

        [SaveTarget("名")]
        public string Mei { get; set; }
        public string FullName => $"{Sei}　{Mei}";

        [SaveTarget("誕生日")]
        public string BirthDay { get; set; }

        [SaveTarget("誕生月")]
        public string BirthMonth { get; set; }

        [SaveTarget("あだ名", SaveTargetType.Multiple)]
        public List<string> Nicknames { get; set; } = new();

        [SaveTarget("所属")]
        public string Affiliation { get; set; }

        [SaveTarget("高等学校")]
        public string HighSchool { get; set; }

        [SaveTarget("中学校")]
        public string MiddleSchool { get; set; }

        [SaveTarget("小学校")]
        public string ElementarySchool { get; set; }

        [SaveTarget("クラス")]
        public string Class { get; set; }

        [SaveTarget("1年")]
        public string Grade1Class { get; set; }

        [SaveTarget("2年")]
        public string Grade2Class { get; set; }

        [SaveTarget("3年")]
        public string Grade3Class { get; set; }

        [SaveTarget("部活")]
        public string Club { get; set; }

        [SaveTarget("趣味", SaveTargetType.Multiple)]
        public List<string> FavoriteThings { get; set; } = new();

        [SaveTarget("話し方")]
        public string SpeechStyle { get; set; }

        [SaveTarget("説明")]
        public string Description { get; set; }

        [SaveTarget("備考")]
        public string NotesSupplement { get; set; }

        [SaveTarget("知力")]
        public int Intelligence { get; set; }

        [SaveTarget("フィジカル")]
        public int Physical { get; set; }

        [SaveTarget("メンタル")]
        public int Mental { get; set; }

        [SaveTarget("運")]
        public int Luck { get; set; }

        [SaveTarget("カリスマ")]
        public int Charisma { get; set; }

        public CharacterStatus Status =>
            new CharacterStatus(Intelligence, Physical, Mental, Luck, Charisma);

        [SaveTarget("タグ", SaveTargetType.Multiple)]
        public List<string> Traits { get; set; } = new();

        [SaveTarget("ネタ", SaveTargetType.Complex)]
        public List<DeesItem> Dees { get; set; } = new();
    }

    public partial class CharacterProfile
    {
        /// <summary>
        /// デバッグ用ログ出力
        /// </summary>
        public string Log()
        {
            var sb = new StringBuilder();
            var type = this.GetType();
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

            foreach (var member in members)
            {
                object? value = member switch
                {
                    PropertyInfo prop => prop.GetValue(this),
                    FieldInfo field => field.GetValue(this),
                    _ => null
                };

                string text = value switch
                {
                    null => "null",
                    string s => s,
                    IEnumerable<string> list => list.JoinToString(),
                    IEnumerable<object> objList => objList.JoinToString(),
                    _ => value.ToString() ?? "null"
                };

                sb.AppendLine($"{member.Name}: {text}");
            }

            return sb.ToString();
        }
    }

    // ステータス構造体
    public class CharacterStatus
    {
        public int Intelligence { get; }
        public int Physical { get; }
        public int Mental { get; }
        public int Luck { get; }
        public int Charisma { get; }

        public CharacterStatus(int intelligence, int physical, int mental, int luck, int charisma)
        {
            Intelligence = intelligence;
            Physical = physical;
            Mental = mental;
            Luck = luck;
            Charisma = charisma;
        }

        public int Sum => Intelligence + Physical + Mental + Luck + Charisma;

        public override string ToString()
        {
            return $"知{Intelligence} 力{Physical} 心{Mental} 運{Luck} 気{Charisma}";
        }
    }

    // ネタアイテム
    public class DeesItem
    {
        public string Text { get; set; }
        public bool Used { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public partial class CharacterProfile
    {
        public string GeneratePromptText() => GeneratePromptText(this);

        /// <summary>
        /// キャラクタープロファイルからプロンプトテキストを生成
        /// </summary>
        public static string GeneratePromptText(CharacterProfile profile)
        {
            var sb = new StringBuilder();

            sb.AppendLine("# キャラクター設定");
            sb.AppendLine();

            // 基本情報
            sb.AppendLine("## 基本情報");
            if (!string.IsNullOrEmpty(profile.FullName))
                sb.AppendLine($"**名前**: {profile.FullName}");

            if (!string.IsNullOrEmpty(profile.Kana))
                sb.AppendLine($"**読み**: {profile.Kana}");

            // 学籍情報
            if (!string.IsNullOrEmpty(profile.Affiliation) || !string.IsNullOrEmpty(profile.Class) || !string.IsNullOrEmpty(profile.Club))
            {
                sb.AppendLine();
                sb.AppendLine("## 学籍情報");

                if (!string.IsNullOrEmpty(profile.Affiliation))
                    sb.AppendLine($"**所属**: {profile.Affiliation}");

                if (!string.IsNullOrEmpty(profile.Class))
                    sb.AppendLine($"**クラス**: {profile.Class}");

                if (!string.IsNullOrEmpty(profile.Club))
                    sb.AppendLine($"**部活**: {profile.Club}");
            }

            // ステータス
            var status = profile.Status;
            if (status.Sum > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## ステータス");
                sb.AppendLine($"**知力**: {status.Intelligence}/7");
                sb.AppendLine($"**フィジカル**: {status.Physical}/7");
                sb.AppendLine($"**メンタル**: {status.Mental}/7");
                sb.AppendLine($"**運**: {status.Luck}/7");
                sb.AppendLine($"**カリスマ**: {status.Charisma}/7");
            }

            // 紹介文
            if (!string.IsNullOrEmpty(profile.Description))
            {
                sb.AppendLine();
                sb.AppendLine("## 紹介文");
                sb.AppendLine(profile.Description);
            }

            // タグ・特徴
            if (profile.Traits.Any())
            {
                sb.AppendLine();
                sb.AppendLine("## 特徴・タグ");
                sb.AppendLine(string.Join(", ", profile.Traits));
            }

            // タグ・特徴
            if (profile.Traits.Any())
            {
                sb.AppendLine();
                sb.AppendLine("## 好き・趣味");
                sb.AppendLine(string.Join(", ", profile.FavoriteThings));
            }

            // 口調メモ
            if (!string.IsNullOrEmpty(profile.SpeechStyle))
            {
                sb.AppendLine();
                sb.AppendLine("## 口調・話し方");
                sb.AppendLine(profile.SpeechStyle);
            }

            // 補足
            if (!string.IsNullOrEmpty(profile.NotesSupplement))
            {
                sb.AppendLine();
                sb.AppendLine("## 補足");
                sb.AppendLine(profile.NotesSupplement);
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("このキャラクターになりきって会話してください。");

            return sb.ToString();
        }
    }
}

namespace Mantensei_Database.Models
{

    public class GodDice
    {
        public static Date Roll()
        {
            List<Date> Birthdays = new List<Date>();

            void AddBirthdays(int year)
            {
                for (int month = 1; month <= 12; month++)
                {
                    for (int day = 1; day <= 31; day++)
                    {
                        var birthday = new Date(month, day);
                        if (birthday.IsValid(year))
                        {
                            Birthdays.Add(birthday);
                        }
                    }
                }
            }

            const int commonYear = 2001;
            const int leapYear = 2000;
            AddBirthdays(commonYear);
            AddBirthdays(commonYear);
            AddBirthdays(commonYear);
            AddBirthdays(leapYear);

            return Birthdays.GetRandomElementOrDefault();
        }

        public struct Date
        {
            public int month;
            public int day;

            public Date(int month, int day)
            {
                this.month = month;
                this.day = day;
            }

            public bool IsValid(int year = 2000)
            {
                return DateTime.TryParse($"{year}/{month}/{day}", out _);
            }
        }
    }
}

namespace Mantensei_Database.Models
{
    public static class ProfileService
    {
        /// <summary>
        /// キャラクターデータを読み込み
        /// </summary>
        public static void LoadAll<T1, T2>(in ObservableCollection<T2> profiles) where T1 : IProfile, new() where T2 : ProfileListItem, new()
        {
            profiles.Clear();

            try
            {
                var profilesDir = FileSystemUtility.GetProfilesDirectory(new T1());
                if (!Directory.Exists(profilesDir))
                {
                    return;
                }

                var xmlFiles = Directory.GetFiles(profilesDir, "*.xml");

                foreach (var filePath in xmlFiles)
                {
                    try
                    {
                        var profile = ProfileService.LoadFromXml<T1>(filePath);
                        var listItem = new T2();
                        listItem.InitProfile(profile, filePath);
                        profiles.Add(listItem);
                        ProfileDataBase.AddProfile(profile);
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
        }

        /// <summary>
        /// デフォルトパスに保存
        /// </summary>
        public static void SaveToDefaultPath(IProfile profile)
        {
            var profilesDir = FileSystemUtility.GetProfilesDirectory(profile);
            var fileName = GetDefaultFileName(profile);
            var filePath = Path.Combine(profilesDir, fileName);
            SaveToXml(profile, filePath);
        }

        /// <summary>
        /// XMLファイルに保存
        /// </summary>
        public static void SaveToXml(IProfile profile, string filePath)
        {
            try
            {
                FileSystemUtility.EnsureDirectoryExists(filePath);

                var serializer = new XmlSerializer(profile.GetType());
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
                serializer.Serialize(writer, profile);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"XMLの保存に失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// XMLファイルから読み込み
        /// </summary>
        public static T LoadFromXml<T>(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var reader = new StreamReader(filePath, Encoding.UTF8);
                return (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"XMLの読み込みに失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// デフォルトファイル名を生成
        /// </summary>
        public static string GetDefaultFileName(IProfile profile)
        {
            return FileSystemUtility.CreateUniqueFileName(profile);
        }
    }
}