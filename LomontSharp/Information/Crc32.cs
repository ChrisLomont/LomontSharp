using System.Collections.Generic;

namespace Lomont.Information
{
    /// <summary>
    /// Simple CRC32 class
    /// </summary>
    public class CRC32 : CRC
    {
        /// <summary>
        /// Make CRC-32. Defaults to normal CRC 32
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="initialValueValue"></param>
        /// <param name="finalXorValue"></param>
        /// <param name="reflect">Input and output reflection</param>
        public CRC32(
            uint polynomial = 0x04C11DB7,
            uint initialValueValue = 0xFFFFFFFF,
            uint finalXorValue = 0xFFFFFFFF,
            bool reflect = true 
            )
            : base(32, polynomial, initialValueValue, reflect, reflect, finalXorValue)
        {
        }
    }
}
