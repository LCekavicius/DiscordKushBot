using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KushBot
{
    public class Pet
    {

        public string Name { get; set; }
        public int Level { get; set; }


        public Pet(string name, int level)
        {
            Name = name;
            Level = level;
        }
    }
}
