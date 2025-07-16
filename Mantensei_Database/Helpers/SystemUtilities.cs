// AppConfig.cs - アプリケーション設定
using Mantensei_Database.Common;
using Mantensei_Database.Models;
using MantenseiLib;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace Mantensei_Database.Common
{
    public static class AppConfig
    {
        public const string AUTHOR_NAME = "Mantensei";
        public const string APP_NAME = "Mantensei_Database";
        public const string PROFILES_DIRECTORY = "profiles";
        public const string SCHOOOL_PROFILES_DIRECTORY = "schools";
        public const string FILE_EXTENSION = ".xml";

        // 他のアプリケーション全体の設定もここに追加可能
        public const string DEFAULT_ENCODING = "UTF-8";
        public const int MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
    }
}

namespace Mantensei_Database.Common
{
    public static class FileSystemUtility
    {
        /// <summary>
        /// プロファイルディレクトリのパスを取得
        /// </summary>
        public static string GetProfilesDirectory(IProfile profile)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string directory = string.Empty;

            switch(profile)
            {
                case CharacterProfile _:
                    directory = AppConfig.PROFILES_DIRECTORY;
                    break;
                case SchoolProfile _:
                    directory = AppConfig.SCHOOOL_PROFILES_DIRECTORY;
                    break;
                default:
                    throw new ArgumentException("Unsupported profile type");
            }

            var profilesPath = Path.Combine(localAppData, AppConfig.AUTHOR_NAME, AppConfig.APP_NAME, directory);

            if (!Directory.Exists(profilesPath))
            {
                Directory.CreateDirectory(profilesPath);
            }

            return profilesPath;
        }

        /// <summary>
        /// 一意なファイル名を生成（ID + タイムスタンプベース）
        /// </summary>
        public static string CreateUniqueFileName(IProfile profile)
        {
            // ID がある場合は ID を使用
            if (profile.Id > 0)
            {
                string header = string.Empty;
                switch(profile)
                {
                    case CharacterProfile _:
                        header = "character";
                        break;
                    case SchoolProfile _:
                        header = "school";
                        break;
                    default:
                        throw new ArgumentException("Unsupported profile type");
                }

                return $"{header}_{profile.Id:D6}{AppConfig.FILE_EXTENSION}";
            }

            // ID がない場合はタイムスタンプを使用
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            return $"character_{timestamp}{AppConfig.FILE_EXTENSION}";
        }

        /// <summary>
        /// 表示用のファイル名を生成（キャラクター名＋ID）
        /// </summary>
        public static string CreateDisplayFileName(CharacterProfile profile)
        {
            var displayName = GetDisplayName(profile);
            var idPart = profile.Id > 0 ? $"_{profile.Id:D6}" : "";
            var safeName = MakeSafeFileName(displayName);

            return $"{safeName}{idPart}{AppConfig.FILE_EXTENSION}";
        }

        /// <summary>
        /// キャラクターの表示名を取得
        /// </summary>
        private static string GetDisplayName(CharacterProfile profile)
        {
            // フルネームがある場合
            if (!string.IsNullOrWhiteSpace(profile.FullName))
                return profile.FullName;

            // かな名がある場合
            if (!string.IsNullOrWhiteSpace(profile.Kana))
                return profile.Kana;

            // 姓のみ
            if (!string.IsNullOrWhiteSpace(profile.Sei))
                return profile.Sei;

            // 名のみ
            if (!string.IsNullOrWhiteSpace(profile.Mei))
                return profile.Mei;

            // かな姓のみ
            if (!string.IsNullOrWhiteSpace(profile.SeiKana))
                return profile.SeiKana;

            // かな名のみ
            if (!string.IsNullOrWhiteSpace(profile.MeiKana))
                return profile.MeiKana;

            // すべて空の場合
            return "unnamed";
        }

        /// <summary>
        /// 安全なファイル名を生成
        /// </summary>
        private static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "character";

            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = string.Join("_", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // 長すぎる場合は切り詰める（Windows のファイル名制限を考慮）
            if (safeName.Length > 50)
                safeName = safeName.Substring(0, 50);

            return safeName;
        }

        /// <summary>
        /// 重複しないファイル名を生成
        /// </summary>
        public static string CreateNonDuplicateFileName(string directory, string baseFileName)
        {
            var fullPath = Path.Combine(directory, baseFileName);

            if (!File.Exists(fullPath))
                return baseFileName;

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
            var extension = Path.GetExtension(baseFileName);
            var counter = 1;

            do
            {
                var newFileName = $"{nameWithoutExtension}_{counter:D2}{extension}";
                fullPath = Path.Combine(directory, newFileName);
                counter++;
            } while (File.Exists(fullPath));

            return Path.GetFileName(fullPath);
        }

        /// <summary>
        /// ディレクトリを安全に作成
        /// </summary>
        public static void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}

// CharacterProfile に ID 自動生成機能を追加
namespace Mantensei_Database.Models
{
    public partial class CharacterProfile
    {

        /// <summary>
        /// 新しいIDを生成
        /// </summary>
      
    }
}