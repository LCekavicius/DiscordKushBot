using System;
using System.Collections.Generic;
using System.Text;

namespace KushBot
{
    public class Package
    {
        public string Code { get; set; }
        public int Baps { get; set; }
        public ulong Author { get; set; }
        public ulong Recipient { get; set; }

        public Package(string code, int baps, ulong author, ulong recipient)
        {
            Author = author;
            Code = code;
            Baps = baps;
            Recipient = recipient;
        }

    }
}
