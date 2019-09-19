using System;

namespace Pomelo.WoW.Web.Models
{
    public class Blob : ModelBase
    {
        public ulong Id { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public DateTime Time { get; set; }

        public byte[] Bytes { get; set; }
    }
}
