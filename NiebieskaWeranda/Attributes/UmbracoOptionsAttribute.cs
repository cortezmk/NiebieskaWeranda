using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiebieskaWeranda.Attributes
{
    public class UmbracoOptionsAttribute : Attribute
    {
        public string Alias { get; set; }
        public bool Ignore { get; set; } = false;
    }
}
