// CharacterProfile.cs - 純粋なデータモデル
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

        [SaveTarget("クラス")]
        public string Class { get; set; }

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