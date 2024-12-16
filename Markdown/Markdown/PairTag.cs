namespace Markdown
{
    public class MdRenderer
    {
        public class PairTag
        {
            public Tag Start;
            public Tag End;
            public int Length => End.Position - Start.Position;

            public PairTag(Tag start, Tag end)
            {
                Start = start;
                End = end;
            }
        }
    }
}
