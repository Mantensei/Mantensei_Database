// CharacterProfile.cs - 純粋なデータモデル
using Mantensei_Database.Common;
using Mantensei_Database.Models;
using MantenseiLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Mantensei_Database.Models
{
    public partial class CharacterProfile
    {
        [SaveTarget("id")]
        public int Id { get; set; }

        [SaveTarget("seiKana")]
        public string SeiKana { get; set; }

        [SaveTarget("meiKana")]
        public string MeiKana { get; set; }

        public string Kana => $"{SeiKana}{MeiKana}";

        [SaveTarget("sei")]
        public string Sei { get; set; }

        [SaveTarget("mei")]
        public string Mei { get; set; }

        public string FullName => $"{Sei}{Mei}";

        [SaveTarget("nicknames")]
        public List<string> Nicknames { get; set; } = new();

        [SaveTarget("class")]
        public string Class { get; set; }

        [SaveTarget("club")]
        public string Club { get; set; }

        [SaveTarget("favoriteThings")]
        public List<string> FavoriteThings { get; set; } = new();

        [SaveTarget("speechStyle")]
        public string SpeechStyle { get; set; }

        [SaveTarget("description")]
        public string Description { get; set; }

        [SaveTarget("notesSupplement")]
        public string NotesSupplement { get; set; }

        [SaveTarget("intelligence")]
        public int Intelligence { get; set; }

        [SaveTarget("physical")]
        public int Physical { get; set; }

        [SaveTarget("mental")]
        public int Mental { get; set; }

        [SaveTarget("luck")]
        public int Luck { get; set; }

        [SaveTarget("charisma")]
        public int Charisma { get; set; }

        public CharacterStatus Status =>
            new CharacterStatus(Intelligence, Physical, Mental, Luck, Charisma);

        [SaveTarget("traits")]
        public List<string> Traits { get; set; } = new();

        [SaveTarget("dees")]
        public List<DeesItem> Dees { get; set; } = new();

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
    public static class CharacterProfileService
    {
        /// <summary>
        /// デフォルトパスに保存
        /// </summary>
        public static void SaveToDefaultPath(CharacterProfile profile)
        {
            var profilesDir = FileSystemUtility.GetProfilesDirectory();
            var fileName = GetDefaultFileName(profile);
            var filePath = Path.Combine(profilesDir, fileName);
            SaveToXml(profile, filePath);
        }

        /// <summary>
        /// XMLファイルに保存
        /// </summary>
        public static void SaveToXml(CharacterProfile profile, string filePath)
        {
            try
            {
                FileSystemUtility.EnsureDirectoryExists(filePath);

                var serializer = new XmlSerializer(typeof(CharacterProfile));
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
        public static CharacterProfile LoadFromXml(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(CharacterProfile));
                using var reader = new StreamReader(filePath, Encoding.UTF8);
                return (CharacterProfile)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"XMLの読み込みに失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// デフォルトファイル名を生成
        /// </summary>
        public static string GetDefaultFileName(CharacterProfile profile)
        {
            return FileSystemUtility.CreateUniqueFileName(profile);
        }
    }
}