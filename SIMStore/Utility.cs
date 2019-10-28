using System;
using System.Collections.Generic;
using System.Text;

namespace SIMStore
{
    class Utility
    {

        internal static IEnumerable<string> ChunkString(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        internal static string StringFromHex(string hexString)
        {
            byte[] bytes = GetBytesFromHex(hexString);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Get hexadecimal value from ASCII string
        /// </summary>
        internal static string HexFromAscii(string asciiValue)
        {
            string hexOut = "";
            byte[] inBytes = UTF8Encoding.UTF8.GetBytes(asciiValue);

            for (int j = 0; j < inBytes.Length; j++)
            {
                hexOut += inBytes[j].ToString("X2");
            }
            return hexOut;
        }

        internal static byte[] GetBytesFromHex(string inData)
        {
            inData = inData.Trim();
            byte[] outByteArray = new byte[0];

            if (inData.Length == 0)
            {
                return outByteArray;
            }
            outByteArray = new byte[inData.Length / 2];
            for (int j = 0; j < inData.Length - 1; j += 2)
            {
                outByteArray[j / 2] = Byte.Parse(inData.Substring(j, 2),
                                               System.Globalization.NumberStyles.HexNumber);
            }
            return outByteArray;
        }

        internal static string GetHexFromBytes(byte[] inBytes)
        {
            if (inBytes.Length == 0)
            {
                return "";
            }
            return GetHexFromBytes(inBytes, 0, inBytes.Length);
        }

        /// <summary>
        ///	Function to obtain an hexadecimal string (0x00 format)
        /// from a bytes array.
        /// </summary>
        /// <param name="inBytes">Bytes array to encoding</param>
        /// <param name="offSet">OffSet to start encoding</param>
        /// <param name="len">Encoding length</param>
        internal static string GetHexFromBytes(byte[] inBytes, int offSet, int len)
        {
            string tmpHexOut = "";
            if (inBytes.Length < (offSet + len))
            {
                return "";
            }
            for (int j = offSet; j < (offSet + len); j++)
            {
                tmpHexOut = tmpHexOut + inBytes[j].ToString("X2");
            }
            return tmpHexOut;
        }

        internal static int GetIntFromHex(string v)
        {
            if (v.Length == sizeof(int) * 2)
            {
                byte[] b = GetBytesFromHex(v);
                return BitConverter.ToInt32(b, 0);
            }
            return -1;
        }
    }
}
