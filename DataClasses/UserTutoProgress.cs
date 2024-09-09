using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.DataClasses
{
    public class UserTutoProgress
    {
        [Key]
        public Guid Id { get; set; }
        public ulong UserId { get; set; }
        public int Page { get; set; }
        public int TaskIndex { get; set; }
    }
}
