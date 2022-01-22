namespace Lomont.Parser
{
    /// <summary>
    /// Store a token, which is a piece of input text, where it came from, 
    /// and its Type
    /// </summary>
    public class Token<TTokenType>
    {
        public Token(string text, TTokenType type, CharPosition start, CharPosition end)
        {
            Text = text;
            Type = type;
            Start = start;
            End = end;
        }
        public string Text { get; }
        public TTokenType Type { get; }
        public CharPosition Start { get; }
        public CharPosition End { get; }

        public override string ToString()
        {
            // clean up endlines, tabs, etc
            var t = Text.Replace("\n", "\\n").Replace("\r","\\r").Replace("\t","\\t");
            var s = $"({Start})".PadLeft(9);
            var e = $"({End})".PadLeft(9);
            var pos = $"{s} - {e}";
            var tt = $"{Type}".PadLeft(12);
            return $"Token: {pos}, {tt}, [{t}]";
        }
    }

}
