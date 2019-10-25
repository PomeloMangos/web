namespace Pomelo.WoW.Web.Models
{
    public class About : ModelBase
    {
        public uint Id { get; set; }
        public string Group { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Priority { get; set; }
    }
}
