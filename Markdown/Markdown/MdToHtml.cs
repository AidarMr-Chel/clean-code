using System.Text;
using static Markdown.MdRenderer;

namespace Markdown
{
    public static class MdToHtml
    {
        private static readonly Dictionary<Tags, string> tagToHtml = new()
        {
            { Tags.Italic, "em" },
            { Tags.Bold, "strong" },
            { Tags.Escape, "" },
            { Tags.H1, "h1" }
        };

        public static string Convert(string text, List<PairTag> pairTags, List<Tag> singleTags)
        {
            var (tagsStart, tagsEnd, escapePositions) = BuildTagDictionaries(pairTags, singleTags);

            return ConvertToHtml(text, tagsStart, tagsEnd, escapePositions, singleTags.Any(tag => tag.Name == Tags.H1));
        }

        private static (Dictionary<int, Tag> tagsStart, Dictionary<int, Tag> tagsEnd, HashSet<int> escapePositions)
            BuildTagDictionaries(List<PairTag> pairTags, List<Tag> singleTags)
        {
            var tagsStart = new Dictionary<int, Tag>();
            var tagsEnd = new Dictionary<int, Tag>();
            var escapePositions = new HashSet<int>();

            foreach (var pairTag in pairTags)
            {
                tagsStart[pairTag.Start.Position] = pairTag.Start;
                tagsEnd[pairTag.End.Position] = pairTag.End;
            }

            foreach (var tag in singleTags.Where(tag => tag.Name == Tags.Escape))
            {
                escapePositions.Add(tag.Position);
            }

            return (tagsStart, tagsEnd, escapePositions);
        }

        private static string ConvertToHtml(
            string text,
            Dictionary<int, Tag> tagsStart,
            Dictionary<int, Tag> tagsEnd,
            HashSet<int> escapePositions,
            bool isH1)
        {
            var html = new StringBuilder(isH1 ? "<h1>" : string.Empty);

            for (var i = isH1 ? 2 : 0; i < text.Length; i++)
            {
                if (tagsStart.TryGetValue(i, out var startTag))
                {
                    html.Append($"<{tagToHtml[startTag.Name]}>");
                    i += startTag.Length - 1; 
                }

                else if (tagsEnd.TryGetValue(i, out var endTag))
                {
                    html.Append($"</{tagToHtml[endTag.Name]}>");
                    i += endTag.Length - 1; 
                }

                else if (!escapePositions.Contains(i))
                {
                    html.Append(text[i]);
                }
            }

            if (isH1)
            {
                html.Append("</h1>");
            }

            return html.ToString();
        }
    }
}
