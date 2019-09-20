using System;
using System.Collections.Generic;

namespace Pomelo.WoW.Web.Models
{
    public class Thread : ModelBase
    {
        public uint Id { get; set; }

        public string ForumId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public uint? ParentId { get; set; }

        public uint VisitCount { get; set; }

        public bool IsPinned { get; set; }

        public bool IsLocked { get; set; }

        public ulong AccountId { get; set; }

        public string CharacterNickname { get; set; }

        public string AccountName { get; set; }

        public AccountLevel Role { get; set; }

        public Race CharacterRace { get; set; }

        public Class CharacterClass{ get; set; }

        public int CharacterLevel { get; set; }

        public DateTime Time { get; set; }

        public DateTime ReplyTime { get; set; }

        public List<Thread> SubThreads { get; set; } = new List<Thread>();
    }
}
