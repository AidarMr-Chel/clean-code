using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown
{
    public class Tag
    {
        public readonly int Position;
        public readonly Tags Name;
        public readonly int Length;

        public Tag(Tags name, int position, int length)
        {
            Name = name;
            Position = position;
            Length = length;
        }
    }
}
