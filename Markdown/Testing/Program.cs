using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdown;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(MDProcessor.Render("_Вот это_"));
            
        }
    }
}
