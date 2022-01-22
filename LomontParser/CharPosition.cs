namespace Lomont.Parser
{
    /// <summary>
    /// Store a character position (line, pos) and total pos
    /// </summary>
    public struct CharPosition
    {
        public CharPosition(int line, int pos, int totalPos)
        {
            Line = line;
            Pos = pos;
            TotalPos = totalPos;
        }
        public int Line { get; }
        public int Pos { get; }
        public int TotalPos { get; }

        public CharPosition Next(char ch)
        {
            if (ch == '\n')
                return new CharPosition(Line + 1, 1, TotalPos + 1);
            return new CharPosition(Line, Pos + 1, TotalPos + 1);
        }

        public override string ToString()
        {
            return $"{Line}:{Pos}";
        }

        // todo - Next increments line, pos, etc.
    }




}
