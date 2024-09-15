using System;
using System.ComponentModel.DataAnnotations;

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
