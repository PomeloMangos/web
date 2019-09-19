using System.Collections.Generic;

namespace Pomelo.WoW.Web.Models
{
    public class Forum : ModelBase
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string IconUrl { get; set; }

        public string Description { get; set; }

        public int Priority { get; set; }

        public bool IsHidden { get; set; }

        public bool IsReadOnly { get; set; }

        public string ParentId { get; set; }

        public List<Forum> SubForums { get; set; } = new List<Forum>();
    }
}
