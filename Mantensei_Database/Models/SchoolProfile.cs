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

        public static readonly string[] SchoolTypes = new string[]
        {
            "小学校", "中学校", "高等学校", "大学", "専門学校", "その他",
        };
    }
}
