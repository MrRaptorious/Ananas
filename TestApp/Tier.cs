using AnanasCore;
using AnanasCore.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp
{
    [Persistent]
    public class Tier : PersistentObject
    {
        public Tier(ObjectSpace os) : base(os)
        {
        }

        
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
