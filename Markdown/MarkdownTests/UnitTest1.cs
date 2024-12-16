using Markdown;

namespace MarkdownTests
{
    public class Tests
    {
        private void AssertMarkdownRender(string source, string expected)
        {
            Assert.That(MdProcessor.Render(source), Is.EqualTo(expected));
        }

        [TestCase("_окруженный с двух сторон_", "<em>окруженный с двух сторон</em>")]
        public void IsValidItalic(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }

        [TestCase("__Выделенный двумя символами текст__", "<strong>Выделенный двумя символами текст</strong>")]
        public void IsValidBold(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }

        [TestCase("\\_Вот это\\_", "_Вот это_")]
        [TestCase("Здесь сим\\волы экранирования\\ \\должны остаться.\\", "Здесь сим\\волы экранирования\\ \\должны остаться.\\")]
        public void IsValidEscape(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }

        [TestCase("__двойного выделения _одинарное_ тоже__", "<strong>двойного выделения <em>одинарное</em> тоже</strong>")]
        [TestCase("_одинарного __двойное__ не_", "<em>одинарного __двойное__ не</em>")]
        [TestCase("цифрами_12_3", "цифрами_12_3")]
        [TestCase("_нач_але, и в сер_еди_не, и в кон_це._", "<em>нач</em>але, и в сер<em>еди</em>не, и в кон<em>це.</em>")]
        [TestCase("__Непарные_", "__Непарные_")]
        [TestCase("эти_ подчерки_", "эти_ подчерки_")]
        [TestCase("эти _подчерки _не считаются_", "эти _подчерки _не считаются_")]
        [TestCase("__пересечения _двойных__ и одинарных_", "__пересечения _двойных__ и одинарных_")]
        [TestCase("____", "____")]
        public void IsValidNested(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }

        [TestCase("# Заголовок", "<h1>Заголовок</h1>")]
        [TestCase("# Заголовок __с _разными_ символами__", "<h1>Заголовок <strong>с <em>разными</em> символами</strong></h1>")]
        public void IsValidHeader(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }
    }
}
