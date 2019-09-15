using System;

namespace Pomelo.WoW.Web.Models
{
    public class Announce : ModelBase
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public DateTime Time { get; set; }

        public string Content { get; set; }

        public bool IsPinned { get; set; }
    }
}
