using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.DataClasses
{
    public class NyaClaim
    {
        [Key]
        public int Id { get; set; }
        public ulong OwnerId { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public int Keys { get; set; }
        public int SortIndex { get; set; }
        public DateTime ClaimDate { get; set; }
    }
}
