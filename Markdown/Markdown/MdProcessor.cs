using System.Text;

namespace Markdown
{
    public static class MdProcessor
    {
        public static string Render(string textInMd)
        {
            if (string.IsNullOrWhiteSpace(textInMd))
                throw new ArgumentException("Input text cannot be null or empty.", nameof(textInMd));

            var renderedText = new StringBuilder();
            foreach (var paragraph in GetParagraphs(textInMd))
            {
                var tags = TagParser.BuildTags(paragraph);
                var renderedParagraphs = MdToHtml.Convert(paragraph, tags.PairTags, tags.SingleTags);
                renderedText.Append(renderedParagraphs);
            }

            return renderedText.ToString();
        }

        private static IEnumerable<string> GetParagraphs(string text)
        {
            var paragraphs = text.Split(Environment.NewLine, StringSplitOptions.None);
            foreach (var paragraph in paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(paragraph))
                {
                    yield return paragraph.Trim();
                }
            }
        }
    }
}
