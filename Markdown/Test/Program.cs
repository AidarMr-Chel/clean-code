using Markdown;
namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(MdProcessor.Render("# Заголовок __с _разными_ символами__"));

        }
    }
}
