using System;

namespace Pomelo.WoW.Web.Models
{
    public class Thread : ModelBase
    {
        public uint Id { get; set; }

        public uint ForumId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public uint? ParentId { get; set; }

        public uint VisitCount { get; set; }

        public bool IsPinned { get; set; }

        public bool IsLocked { get; set; }

        public ulong AccountId { get; set; }

        public DateTime Time { get; set; }
    }
}
