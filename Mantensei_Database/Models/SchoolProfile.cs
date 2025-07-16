namespace Mantensei_Database.Models
{
    public class SchoolProfile : IProfile
    {
        [SaveTarget("ID")]
        public int Id { get; set; }

        [SaveTarget("学校タイプ")]
        public SchoolType SchoolType { get; set; }

        [SaveTarget("学校名")]
        public string Name { get; set; }

        [SaveTarget("クラス")]
        public List<string> Classes { get; set; }

        [SaveTarget("部活")]
        public List<string> Clubs { get; set; }

        [SaveTarget("説明")]
        public string Description { get; set; }

        [SaveTarget("備考")]
        public string NotesSupplement { get; set; }
    }

    public enum SchoolType
    {
        Elementary,
        Middle,
        High,
        Other
    }

    public static class SchoolTypeExtension
    {
        public static int GetRankCount(this SchoolType type)
        {
            return type switch
            {
                SchoolType.Elementary => 6,
                SchoolType.Middle => 3,
                SchoolType.High => 3,
                _ => 0,
            };
        }
    }
}
