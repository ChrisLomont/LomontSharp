namespace Lomont.Parser
{
    // allow looking ahead on a string
    public class CharStream : ItemStream<char, CharPosition>
    {
        private string text;
        public CharStream(string text)
        {
            this.text = text;
        }

        public override bool More()
        {
            return curPos.TotalPos < text.Length;
        }

        public override char Next()
        {
            curPos = curPos.Next(text[curPos.TotalPos]);
            if (More())
                return text[curPos.TotalPos];
            return (char)0;
        }

        /// <summary>
        /// Rest of string from cur pos
        /// todo - nice to remove?, but need for regex!
        /// makes many string copies - maybe hash them by total curpos?
        /// </summary>
        /// <returns></returns>
        public string Rest()
        {
            return text.Substring(curPos.TotalPos);
        }

    }

}
