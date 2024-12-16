using static Markdown.MdRenderer;

namespace Markdown
{
    public class TagParser
    {
        public List<PairTag> PairTags { get; private set; }
        public List<Tag> SingleTags { get; private set; }

        private static readonly List<Tags> PairTagTypes = new() { Tags.Italic, Tags.Bold };

        private TagParser(List<PairTag> pairTags, List<Tag> singleTags)
        {
            PairTags = pairTags;
            SingleTags = singleTags;
        }

        public static TagParser BuildTags(string text)
        {
            var allTags = GetAllTags(text).ToList();
            var unescapedTags = GetUnescapedTags(allTags).ToList();
            var singleTags = GetSingleTags(unescapedTags).ToList();
            var pairTags = GetPairTags(unescapedTags, PairTagTypes).OrderBy(x => x.Start.Position).ToList();
            var cleanedPairTags = CleanUpTags(pairTags, Tags.Bold, Tags.Italic).ToList();
            var properlySpacedTags = AdjustTagSpaces(text, cleanedPairTags).ToList();

            return new TagParser(properlySpacedTags, singleTags);
        }

        private static IEnumerable<PairTag> AdjustTagSpaces(string text, List<PairTag> pairTags)
        {
            foreach (var pairTag in pairTags)
            {
                if (pairTag.Length == pairTag.Start.Length)
                    continue;


                if (IsInsideWord(text, pairTag.Start.Position, pairTag.Start.Length)
                    && char.IsDigit(text[pairTag.Start.Position + pairTag.Start.Length]))
                    continue;


                if (IsNotSurroundedBySpaces(text, pairTag))
                    yield return pairTag;
            }
        }

        private static bool IsInsideWord(string text, int position, int length) =>
            text.Length > position + length && position > 0
            && text[position + length] != ' ' && text[position - 1] != ' ';

        private static bool IsNotSurroundedBySpaces(string text, PairTag pairTag) =>
            text[pairTag.Start.Position + pairTag.Start.Length] != ' '
            && text[pairTag.End.Position - 1] != ' ';

        private static IEnumerable<PairTag> CleanUpTags(List<PairTag> pairTags, Tags includeTag, Tags excludeTag)
        {
            for (var i = 0; i < pairTags.Count; i++)
            {
                if (pairTags[i].Start.Name == excludeTag)
                {
                    var pairTag = pairTags[i];
                    yield return pairTags[i];

                    while (i + 1 < pairTags.Count && pairTags[i + 1].Start.Position < pairTag.End.Position)
                    {
                        if (pairTags[i + 1].Start.Name != includeTag)
                            yield return pairTags[i + 1];

                        i++;
                    }
                }
                else
                {
                    yield return pairTags[i];
                }
            }
        }

        private static IEnumerable<Tag> GetSingleTags(List<Tag> tags) =>
            tags.Where(tag => tag.Name is Tags.H1 or Tags.Escape);

        private static IEnumerable<PairTag> GetPairTags(List<Tag> tags, List<Tags> pairTagTypes)
        {
            var tagStack = new Stack<Tag>();
            var openTags = new HashSet<Tags>();
            var ignoredTags = new HashSet<Tags>();

            foreach (var tag in tags.Where(tag => pairTagTypes.Contains(tag.Name)))
            {
                if (ignoredTags.Remove(tag.Name))
                    continue;

                if (!openTags.Remove(tag.Name))
                {
                    tagStack.Push(tag);
                    openTags.Add(tag.Name);
                    continue;
                }

                var openTag = tagStack.Pop();
                if (openTag.Name == tag.Name)
                {
                    yield return new PairTag(openTag, tag);
                }
                else
                {
                    while (openTag.Name != tag.Name)
                    {
                        openTags.Remove(openTag.Name);
                        ignoredTags.Add(openTag.Name);
                        openTag = tagStack.Pop();
                    }
                }
            }
        }

        private static IEnumerable<Tag> GetUnescapedTags(List<Tag> allTags)
        {
            for (var i = 0; i < allTags.Count; i++)
            {
                if (allTags[i].Name == Tags.Escape && i + 1 < allTags.Count && allTags[i].Position + 1 == allTags[i + 1].Position)
                {
                    yield return allTags[i];
                    i++;
                }
                else if (allTags[i].Name != Tags.Escape)
                {
                    yield return allTags[i];
                }
            }
        }

        private static IEnumerable<Tag> GetAllTags(string text)
        {
            for (var i = 0; i < text.Length; i++)
            {
                var tag = GetTagAtPosition(i, text);
                if (tag != null)
                {
                    i += tag.Length - 1;
                    yield return tag;
                }
            }
        }

        private static Tag? GetTagAtPosition(int position, string text)
        {
            switch (text[position])
            {
                case '#':
                    return position == 0 ? new Tag(Tags.H1, position, 1) : null;
                case '_':
                    return position + 1 < text.Length && text[position + 1] == '_'
                        ? new Tag(Tags.Bold, position, 2)
                        : new Tag(Tags.Italic, position, 1);
                case '\\':
                    return new Tag(Tags.Escape, position, 1);
                default:
                    return null;
            }
        }
    }
}
