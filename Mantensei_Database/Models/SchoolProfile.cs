namespace Mantensei_Database.Models
{
    public class SchoolProfile : IProfile
    {
        [SaveTarget("ID")]
        public int Id { get; set; }

        [SaveTarget("学校タイプ")]
        public string SchoolType { get; set; }

        [SaveTarget("学校名")]
        public string Name { get; set; }

        [SaveTarget("クラス", SaveTargetType.Multiple)]
        public List<string> Classes { get; set; } = new();

        [SaveTarget("部活", SaveTargetType.Multiple)]
        public List<string> Clubs { get; set; } = new();

        [SaveTarget("説明")]
        public string Description { get; set; }

        [SaveTarget("備考")]
        public string NotesSupplement { get; set; }

        public const string SchoolTypeElementary = "小学校";
        public const string SchoolTypeMiddle = "中学校";
        public const string SchoolTypeHigh = "高等学校";
        public const string SchoolTypeUniversity = "大学";
        public const string SchoolTypeVocational = "専門学校";
        public const string SchoolTypeOther = "その他";

        public static readonly string[] SchoolTypes = new string[]
        {
            SchoolTypeElementary,
            SchoolTypeMiddle,
            SchoolTypeHigh,
            SchoolTypeUniversity,
            SchoolTypeVocational,
            SchoolTypeOther,
        };
    }
}
