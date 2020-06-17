﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPSecondSubtile.Parser
{
    public class CustomCP1256Encoding : Encoding
    {
        /// <summary>
        /// Gets the name registered with the
        /// Internet Assigned Numbers Authority (IANA) for the current encoding.
        /// </summary>
        /// <returns>
        /// The IANA name for the current <see cref="System.Text.Encoding"/>.
        /// </returns>
        public override string WebName
        {
            get
            {
                return "windows-1256";
            }
        }


        private char? fallbackCharacter;

        /// <summary>
        /// A character that can be set in order to make the encoding class
        /// more fault tolerant. If this property is set, the encoding class will
        /// use this property instead of throwing an exception if an unsupported
        /// byte value is being passed for decoding.
        /// </summary>
        public char? FallbackCharacter
        {
            get { return fallbackCharacter; }
            set
            {
                fallbackCharacter = value;

                if (value.HasValue && !charToByte.ContainsKey(value.Value))
                {
                    string msg = "Cannot use the character [{0}] (int value {1}) as fallback value "
                    + "- the fallback character itself is not supported by the encoding.";
                    msg = String.Format(msg, value.Value, (int)value.Value);
                    throw new EncoderFallbackException(msg);
                }

                FallbackByte = value.HasValue ? charToByte[value.Value] : (byte?)null;
            }
        }

        /// <summary>
        /// A byte value that corresponds to the <see cref="FallbackCharacter"/>.
        /// It is used in encoding scenarios in case an unsupported character is
        /// being passed for encoding.
        /// </summary>
        public byte? FallbackByte { get; private set; }


        public CustomCP1256Encoding()
        {
            FallbackCharacter = '?';
        }

        /// <summary>
        /// Encodes a set of characters from the specified character array into the specified byte array.
        /// </summary>
        /// <returns>
        /// The actual number of bytes written into <paramref name="bytes"/>.
        /// </returns>
        /// <param name="chars">The character array containing the set of characters to encode. 
        /// </param><param name="charIndex">The index of the first character to encode. 
        /// </param><param name="charCount">The number of characters to encode. 
        /// </param><param name="bytes">The byte array to contain the resulting sequence of bytes.
        /// </param><param name="byteIndex">The index at which to start writing the resulting sequence of bytes. 
        /// </param>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return FallbackByte.HasValue
                     ? GetBytesWithFallBack(chars, charIndex, charCount, bytes, byteIndex)
                     : GetBytesWithoutFallback(chars, charIndex, charCount, bytes, byteIndex);
        }


        private int GetBytesWithFallBack(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = 0; i < charCount; i++)
            {
                var character = chars[i + charIndex];
                byte byteValue;
                bool status = charToByte.TryGetValue(character, out byteValue);

                bytes[byteIndex + i] = status ? byteValue : FallbackByte.Value;
            }

            return charCount;
        }

        private int GetBytesWithoutFallback(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = 0; i < charCount; i++)
            {
                var character = chars[i + charIndex];
                byte byteValue;
                bool status = charToByte.TryGetValue(character, out byteValue);

                if (!status)
                {
                    //throw exception
                    string msg =
                      "The encoding [{0}] cannot encode the character [{1}] (int value {2}). Set the FallbackCharacter property in order to suppress this exception and encode a default character instead.";
                    msg = String.Format(msg, WebName, character, (int)character);
                    throw new EncoderFallbackException(msg);
                }

                bytes[byteIndex + i] = byteValue;
            }

            return charCount;
        }



        /// <summary>
        /// Decodes a sequence of bytes from the specified byte array into the specified character array.
        /// </summary>
        /// <returns>
        /// The actual number of characters written into <paramref name="chars"/>.
        /// </returns>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode. 
        /// </param><param name="byteIndex">The index of the first byte to decode. 
        /// </param><param name="byteCount">The number of bytes to decode. 
        /// </param><param name="chars">The character array to contain the resulting set of characters. 
        /// </param><param name="charIndex">The index at which to start writing the resulting set of characters. 
        /// </param>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return FallbackCharacter.HasValue
                     ? GetCharsWithFallback(bytes, byteIndex, byteCount, chars, charIndex)
                     : GetCharsWithoutFallback(bytes, byteIndex, byteCount, chars, charIndex);
        }


        private int GetCharsWithFallback(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = 0; i < byteCount; i++)
            {
                byte lookupIndex = bytes[i + byteIndex];

                //if the byte value is not in our lookup array, fall back to default character
                char result = lookupIndex >= byteToChar.Length
                                ? FallbackCharacter.Value
                                : byteToChar[lookupIndex];

                chars[charIndex + i] = result;
            }

            return byteCount;
        }



        private int GetCharsWithoutFallback(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = 0; i < byteCount; i++)
            {
                byte lookupIndex = bytes[i + byteIndex];
                if (lookupIndex >= byteToChar.Length)
                {
                    //throw exception
                    string msg = "The encoding [{0}] cannot decode byte value [{1}]. Set the FallbackCharacter property in order to suppress this exception and decode the value as a default character instead.";
                    msg = String.Format(msg, WebName, lookupIndex);
                    throw new EncoderFallbackException(msg);
                }


                chars[charIndex + i] = byteToChar[lookupIndex];
            }

            return byteCount;
        }



        /// <summary>
        /// Calculates the number of bytes produced by encoding a set of characters
        /// from the specified character array.
        /// </summary>
        /// <returns>
        /// The number of bytes produced by encoding the specified characters. This class
        /// alwas returns the value of <paramref name="count"/>.
        /// </returns>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }


        /// <summary>
        /// Calculates the number of characters produced by decoding a sequence
        /// of bytes from the specified byte array.
        /// </summary>
        /// <returns>
        /// The number of characters produced by decoding the specified sequence of bytes. This class
        /// alwas returns the value of <paramref name="count"/>. 
        /// </returns>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }


        /// <summary>
        /// Calculates the maximum number of bytes produced by encoding the specified number of characters.
        /// </summary>
        /// <returns>
        /// The maximum number of bytes produced by encoding the specified number of characters. This
        /// class alwas returns the value of <paramref name="charCount"/>.
        /// </returns>
        /// <param name="charCount">The number of characters to encode. 
        /// </param>
        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        /// <summary>
        /// Calculates the maximum number of characters produced by decoding the specified number of bytes.
        /// </summary>
        /// <returns>
        /// The maximum number of characters produced by decoding the specified number of bytes. This class
        /// alwas returns the value of <paramref name="byteCount"/>.
        /// </returns>
        /// <param name="byteCount">The number of bytes to decode.</param> 
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }


        /// <summary>
        /// Gets the number of characters that are supported by this encoding.
        /// This property returns a maximum value of 256, as the encoding class
        /// only supports single byte encodings (1 byte == 256 possible values).
        /// </summary>
        public static int CharacterCount
        {
            get { return byteToChar.Length; }
        }


        #region Character Table

        /// <summary>
        /// This table contains characters in an array. The index within the
        /// array corresponds to the encoding's mapping of bytes to characters
        /// (e.g. if a byte value of 5 is used to encode the character 'x', this
        /// character will be stored at the array index 5.
        /// </summary>
        private static char[] byteToChar = new char[]
    {
      (char)0 /* byte 0 */  ,
      (char)1 /* byte 1 */  ,
      (char)2 /* byte 2 */  ,
      (char)3 /* byte 3 */  ,
      (char)4 /* byte 4 */  ,
      (char)5 /* byte 5 */  ,
      (char)6 /* byte 6 */  ,
      (char)7 /* byte 7 */  ,
      (char)8 /* byte 8 */  ,
      (char)9 /* byte 9 */  ,
      (char)10 /* byte 10 */  ,
      (char)11 /* byte 11 */  ,
      (char)12 /* byte 12 */  ,
      (char)13 /* byte 13 */  ,
      (char)14 /* byte 14 */  ,
      (char)15 /* byte 15 */  ,
      (char)16 /* byte 16 */  ,
      (char)17 /* byte 17 */  ,
      (char)18 /* byte 18 */  ,
      (char)19 /* byte 19 */  ,
      (char)20 /* byte 20 */  ,
      (char)21 /* byte 21 */  ,
      (char)22 /* byte 22 */  ,
      (char)23 /* byte 23 */  ,
      (char)24 /* byte 24 */  ,
      (char)25 /* byte 25 */  ,
      (char)26 /* byte 26 */  ,
      (char)27 /* byte 27 */  ,
      (char)28 /* byte 28 */  ,
      (char)29 /* byte 29 */  ,
      (char)30 /* byte 30 */  ,
      (char)31 /* byte 31 */  ,
      (char)32 /* byte 32 */  ,
      (char)33 /* byte 33 */  ,
      (char)34 /* byte 34 */  ,
      (char)35 /* byte 35 */  ,
      (char)36 /* byte 36 */  ,
      (char)37 /* byte 37 */  ,
      (char)38 /* byte 38 */  ,
      (char)39 /* byte 39 */  ,
      (char)40 /* byte 40 */  ,
      (char)41 /* byte 41 */  ,
      (char)42 /* byte 42 */  ,
      (char)43 /* byte 43 */  ,
      (char)44 /* byte 44 */  ,
      (char)45 /* byte 45 */  ,
      (char)46 /* byte 46 */  ,
      (char)47 /* byte 47 */  ,
      (char)48 /* byte 48 */  ,
      (char)49 /* byte 49 */  ,
      (char)50 /* byte 50 */  ,
      (char)51 /* byte 51 */  ,
      (char)52 /* byte 52 */  ,
      (char)53 /* byte 53 */  ,
      (char)54 /* byte 54 */  ,
      (char)55 /* byte 55 */  ,
      (char)56 /* byte 56 */  ,
      (char)57 /* byte 57 */  ,
      (char)58 /* byte 58 */  ,
      (char)59 /* byte 59 */  ,
      (char)60 /* byte 60 */  ,
      (char)61 /* byte 61 */  ,
      (char)62 /* byte 62 */  ,
      (char)63 /* byte 63 */  ,
      (char)64 /* byte 64 */  ,
      (char)65 /* byte 65 */  ,
      (char)66 /* byte 66 */  ,
      (char)67 /* byte 67 */  ,
      (char)68 /* byte 68 */  ,
      (char)69 /* byte 69 */  ,
      (char)70 /* byte 70 */  ,
      (char)71 /* byte 71 */  ,
      (char)72 /* byte 72 */  ,
      (char)73 /* byte 73 */  ,
      (char)74 /* byte 74 */  ,
      (char)75 /* byte 75 */  ,
      (char)76 /* byte 76 */  ,
      (char)77 /* byte 77 */  ,
      (char)78 /* byte 78 */  ,
      (char)79 /* byte 79 */  ,
      (char)80 /* byte 80 */  ,
      (char)81 /* byte 81 */  ,
      (char)82 /* byte 82 */  ,
      (char)83 /* byte 83 */  ,
      (char)84 /* byte 84 */  ,
      (char)85 /* byte 85 */  ,
      (char)86 /* byte 86 */  ,
      (char)87 /* byte 87 */  ,
      (char)88 /* byte 88 */  ,
      (char)89 /* byte 89 */  ,
      (char)90 /* byte 90 */  ,
      (char)91 /* byte 91 */  ,
      (char)92 /* byte 92 */  ,
      (char)93 /* byte 93 */  ,
      (char)94 /* byte 94 */  ,
      (char)95 /* byte 95 */  ,
      (char)96 /* byte 96 */  ,
      (char)97 /* byte 97 */  ,
      (char)98 /* byte 98 */  ,
      (char)99 /* byte 99 */  ,
      (char)100 /* byte 100 */  ,
      (char)101 /* byte 101 */  ,
      (char)102 /* byte 102 */  ,
      (char)103 /* byte 103 */  ,
      (char)104 /* byte 104 */  ,
      (char)105 /* byte 105 */  ,
      (char)106 /* byte 106 */  ,
      (char)107 /* byte 107 */  ,
      (char)108 /* byte 108 */  ,
      (char)109 /* byte 109 */  ,
      (char)110 /* byte 110 */  ,
      (char)111 /* byte 111 */  ,
      (char)112 /* byte 112 */  ,
      (char)113 /* byte 113 */  ,
      (char)114 /* byte 114 */  ,
      (char)115 /* byte 115 */  ,
      (char)116 /* byte 116 */  ,
      (char)117 /* byte 117 */  ,
      (char)118 /* byte 118 */  ,
      (char)119 /* byte 119 */  ,
      (char)120 /* byte 120 */  ,
      (char)121 /* byte 121 */  ,
      (char)122 /* byte 122 */  ,
      (char)123 /* byte 123 */  ,
      (char)124 /* byte 124 */  ,
      (char)125 /* byte 125 */  ,
      (char)126 /* byte 126 */  ,
      (char)127 /* byte 127 */  ,
      (char)8364 /* byte 128 */  ,
      (char)1662 /* byte 129 */  ,
      (char)8218 /* byte 130 */  ,
      (char)402 /* byte 131 */  ,
      (char)8222 /* byte 132 */  ,
      (char)8230 /* byte 133 */  ,
      (char)8224 /* byte 134 */  ,
      (char)8225 /* byte 135 */  ,
      (char)710 /* byte 136 */  ,
      (char)8240 /* byte 137 */  ,
      (char)1657 /* byte 138 */  ,
      (char)8249 /* byte 139 */  ,
      (char)338 /* byte 140 */  ,
      (char)1670 /* byte 141 */  ,
      (char)1688 /* byte 142 */  ,
      (char)1672 /* byte 143 */  ,
      (char)1711 /* byte 144 */  ,
      (char)8216 /* byte 145 */  ,
      (char)8217 /* byte 146 */  ,
      (char)8220 /* byte 147 */  ,
      (char)8221 /* byte 148 */  ,
      (char)8226 /* byte 149 */  ,
      (char)8211 /* byte 150 */  ,
      (char)8212 /* byte 151 */  ,
      (char)1705 /* byte 152 */  ,
      (char)8482 /* byte 153 */  ,
      (char)1681 /* byte 154 */  ,
      (char)8250 /* byte 155 */  ,
      (char)339 /* byte 156 */  ,
      (char)8204 /* byte 157 */  ,
      (char)8205 /* byte 158 */  ,
      (char)1722 /* byte 159 */  ,
      (char)160 /* byte 160 */  ,
      (char)1548 /* byte 161 */  ,
      (char)162 /* byte 162 */  ,
      (char)163 /* byte 163 */  ,
      (char)164 /* byte 164 */  ,
      (char)165 /* byte 165 */  ,
      (char)166 /* byte 166 */  ,
      (char)167 /* byte 167 */  ,
      (char)168 /* byte 168 */  ,
      (char)169 /* byte 169 */  ,
      (char)1726 /* byte 170 */  ,
      (char)171 /* byte 171 */  ,
      (char)172 /* byte 172 */  ,
      (char)173 /* byte 173 */  ,
      (char)174 /* byte 174 */  ,
      (char)175 /* byte 175 */  ,
      (char)176 /* byte 176 */  ,
      (char)177 /* byte 177 */  ,
      (char)178 /* byte 178 */  ,
      (char)179 /* byte 179 */  ,
      (char)180 /* byte 180 */  ,
      (char)181 /* byte 181 */  ,
      (char)182 /* byte 182 */  ,
      (char)183 /* byte 183 */  ,
      (char)184 /* byte 184 */  ,
      (char)185 /* byte 185 */  ,
      (char)1563 /* byte 186 */  ,
      (char)187 /* byte 187 */  ,
      (char)188 /* byte 188 */  ,
      (char)189 /* byte 189 */  ,
      (char)190 /* byte 190 */  ,
      (char)1567 /* byte 191 */  ,
      (char)1729 /* byte 192 */  ,
      (char)1569 /* byte 193 */  ,
      (char)1570 /* byte 194 */  ,
      (char)1571 /* byte 195 */  ,
      (char)1572 /* byte 196 */  ,
      (char)1573 /* byte 197 */  ,
      (char)1574 /* byte 198 */  ,
      (char)1575 /* byte 199 */  ,
      (char)1576 /* byte 200 */  ,
      (char)1577 /* byte 201 */  ,
      (char)1578 /* byte 202 */  ,
      (char)1579 /* byte 203 */  ,
      (char)1580 /* byte 204 */  ,
      (char)1581 /* byte 205 */  ,
      (char)1582 /* byte 206 */  ,
      (char)1583 /* byte 207 */  ,
      (char)1584 /* byte 208 */  ,
      (char)1585 /* byte 209 */  ,
      (char)1586 /* byte 210 */  ,
      (char)1587 /* byte 211 */  ,
      (char)1588 /* byte 212 */  ,
      (char)1589 /* byte 213 */  ,
      (char)1590 /* byte 214 */  ,
      (char)215 /* byte 215 */  ,
      (char)1591 /* byte 216 */  ,
      (char)1592 /* byte 217 */  ,
      (char)1593 /* byte 218 */  ,
      (char)1594 /* byte 219 */  ,
      (char)1600 /* byte 220 */  ,
      (char)1601 /* byte 221 */  ,
      (char)1602 /* byte 222 */  ,
      (char)1603 /* byte 223 */  ,
      (char)224 /* byte 224 */  ,
      (char)1604 /* byte 225 */  ,
      (char)226 /* byte 226 */  ,
      (char)1605 /* byte 227 */  ,
      (char)1606 /* byte 228 */  ,
      (char)1607 /* byte 229 */  ,
      (char)1608 /* byte 230 */  ,
      (char)231 /* byte 231 */  ,
      (char)232 /* byte 232 */  ,
      (char)233 /* byte 233 */  ,
      (char)234 /* byte 234 */  ,
      (char)235 /* byte 235 */  ,
      (char)1609 /* byte 236 */  ,
      (char)1610 /* byte 237 */  ,
      (char)238 /* byte 238 */  ,
      (char)239 /* byte 239 */  ,
      (char)1611 /* byte 240 */  ,
      (char)1612 /* byte 241 */  ,
      (char)1613 /* byte 242 */  ,
      (char)1614 /* byte 243 */  ,
      (char)244 /* byte 244 */  ,
      (char)1615 /* byte 245 */  ,
      (char)1616 /* byte 246 */  ,
      (char)247 /* byte 247 */  ,
      (char)1617 /* byte 248 */  ,
      (char)249 /* byte 249 */  ,
      (char)1618 /* byte 250 */  ,
      (char)251 /* byte 251 */  ,
      (char)252 /* byte 252 */  ,
      (char)8206 /* byte 253 */  ,
      (char)8207 /* byte 254 */  ,
      (char)1746 /* byte 255 */  
    };

        #endregion


        #region Byte Lookup Dictionary

        /// <summary>
        /// This dictionary is used to resolve byte values for a given character.
        /// </summary>
        private static Dictionary<char, byte> charToByte = new Dictionary<char, byte>
    {
      { (char)0, 0 },
      { (char)1, 1 },
      { (char)2, 2 },
      { (char)3, 3 },
      { (char)4, 4 },
      { (char)5, 5 },
      { (char)6, 6 },
      { (char)7, 7 },
      { (char)8, 8 },
      { (char)9, 9 },
      { (char)10, 10 },
      { (char)11, 11 },
      { (char)12, 12 },
      { (char)13, 13 },
      { (char)14, 14 },
      { (char)15, 15 },
      { (char)16, 16 },
      { (char)17, 17 },
      { (char)18, 18 },
      { (char)19, 19 },
      { (char)20, 20 },
      { (char)21, 21 },
      { (char)22, 22 },
      { (char)23, 23 },
      { (char)24, 24 },
      { (char)25, 25 },
      { (char)26, 26 },
      { (char)27, 27 },
      { (char)28, 28 },
      { (char)29, 29 },
      { (char)30, 30 },
      { (char)31, 31 },
      { (char)32, 32 },
      { (char)33, 33 },
      { (char)34, 34 },
      { (char)35, 35 },
      { (char)36, 36 },
      { (char)37, 37 },
      { (char)38, 38 },
      { (char)39, 39 },
      { (char)40, 40 },
      { (char)41, 41 },
      { (char)42, 42 },
      { (char)43, 43 },
      { (char)44, 44 },
      { (char)45, 45 },
      { (char)46, 46 },
      { (char)47, 47 },
      { (char)48, 48 },
      { (char)49, 49 },
      { (char)50, 50 },
      { (char)51, 51 },
      { (char)52, 52 },
      { (char)53, 53 },
      { (char)54, 54 },
      { (char)55, 55 },
      { (char)56, 56 },
      { (char)57, 57 },
      { (char)58, 58 },
      { (char)59, 59 },
      { (char)60, 60 },
      { (char)61, 61 },
      { (char)62, 62 },
      { (char)63, 63 },
      { (char)64, 64 },
      { (char)65, 65 },
      { (char)66, 66 },
      { (char)67, 67 },
      { (char)68, 68 },
      { (char)69, 69 },
      { (char)70, 70 },
      { (char)71, 71 },
      { (char)72, 72 },
      { (char)73, 73 },
      { (char)74, 74 },
      { (char)75, 75 },
      { (char)76, 76 },
      { (char)77, 77 },
      { (char)78, 78 },
      { (char)79, 79 },
      { (char)80, 80 },
      { (char)81, 81 },
      { (char)82, 82 },
      { (char)83, 83 },
      { (char)84, 84 },
      { (char)85, 85 },
      { (char)86, 86 },
      { (char)87, 87 },
      { (char)88, 88 },
      { (char)89, 89 },
      { (char)90, 90 },
      { (char)91, 91 },
      { (char)92, 92 },
      { (char)93, 93 },
      { (char)94, 94 },
      { (char)95, 95 },
      { (char)96, 96 },
      { (char)97, 97 },
      { (char)98, 98 },
      { (char)99, 99 },
      { (char)100, 100 },
      { (char)101, 101 },
      { (char)102, 102 },
      { (char)103, 103 },
      { (char)104, 104 },
      { (char)105, 105 },
      { (char)106, 106 },
      { (char)107, 107 },
      { (char)108, 108 },
      { (char)109, 109 },
      { (char)110, 110 },
      { (char)111, 111 },
      { (char)112, 112 },
      { (char)113, 113 },
      { (char)114, 114 },
      { (char)115, 115 },
      { (char)116, 116 },
      { (char)117, 117 },
      { (char)118, 118 },
      { (char)119, 119 },
      { (char)120, 120 },
      { (char)121, 121 },
      { (char)122, 122 },
      { (char)123, 123 },
      { (char)124, 124 },
      { (char)125, 125 },
      { (char)126, 126 },
      { (char)127, 127 },
      { (char)8364, 128 },
      { (char)1662, 129 },
      { (char)8218, 130 },
      { (char)402, 131 },
      { (char)8222, 132 },
      { (char)8230, 133 },
      { (char)8224, 134 },
      { (char)8225, 135 },
      { (char)710, 136 },
      { (char)8240, 137 },
      { (char)1657, 138 },
      { (char)8249, 139 },
      { (char)338, 140 },
      { (char)1670, 141 },
      { (char)1688, 142 },
      { (char)1672, 143 },
      { (char)1711, 144 },
      { (char)8216, 145 },
      { (char)8217, 146 },
      { (char)8220, 147 },
      { (char)8221, 148 },
      { (char)8226, 149 },
      { (char)8211, 150 },
      { (char)8212, 151 },
      { (char)1705, 152 },
      { (char)8482, 153 },
      { (char)1681, 154 },
      { (char)8250, 155 },
      { (char)339, 156 },
      { (char)8204, 157 },
      { (char)8205, 158 },
      { (char)1722, 159 },
      { (char)160, 160 },
      { (char)1548, 161 },
      { (char)162, 162 },
      { (char)163, 163 },
      { (char)164, 164 },
      { (char)165, 165 },
      { (char)166, 166 },
      { (char)167, 167 },
      { (char)168, 168 },
      { (char)169, 169 },
      { (char)1726, 170 },
      { (char)171, 171 },
      { (char)172, 172 },
      { (char)173, 173 },
      { (char)174, 174 },
      { (char)175, 175 },
      { (char)176, 176 },
      { (char)177, 177 },
      { (char)178, 178 },
      { (char)179, 179 },
      { (char)180, 180 },
      { (char)181, 181 },
      { (char)182, 182 },
      { (char)183, 183 },
      { (char)184, 184 },
      { (char)185, 185 },
      { (char)1563, 186 },
      { (char)187, 187 },
      { (char)188, 188 },
      { (char)189, 189 },
      { (char)190, 190 },
      { (char)1567, 191 },
      { (char)1729, 192 },
      { (char)1569, 193 },
      { (char)1570, 194 },
      { (char)1571, 195 },
      { (char)1572, 196 },
      { (char)1573, 197 },
      { (char)1574, 198 },
      { (char)1575, 199 },
      { (char)1576, 200 },
      { (char)1577, 201 },
      { (char)1578, 202 },
      { (char)1579, 203 },
      { (char)1580, 204 },
      { (char)1581, 205 },
      { (char)1582, 206 },
      { (char)1583, 207 },
      { (char)1584, 208 },
      { (char)1585, 209 },
      { (char)1586, 210 },
      { (char)1587, 211 },
      { (char)1588, 212 },
      { (char)1589, 213 },
      { (char)1590, 214 },
      { (char)215, 215 },
      { (char)1591, 216 },
      { (char)1592, 217 },
      { (char)1593, 218 },
      { (char)1594, 219 },
      { (char)1600, 220 },
      { (char)1601, 221 },
      { (char)1602, 222 },
      { (char)1603, 223 },
      { (char)224, 224 },
      { (char)1604, 225 },
      { (char)226, 226 },
      { (char)1605, 227 },
      { (char)1606, 228 },
      { (char)1607, 229 },
      { (char)1608, 230 },
      { (char)231, 231 },
      { (char)232, 232 },
      { (char)233, 233 },
      { (char)234, 234 },
      { (char)235, 235 },
      { (char)1609, 236 },
      { (char)1610, 237 },
      { (char)238, 238 },
      { (char)239, 239 },
      { (char)1611, 240 },
      { (char)1612, 241 },
      { (char)1613, 242 },
      { (char)1614, 243 },
      { (char)244, 244 },
      { (char)1615, 245 },
      { (char)1616, 246 },
      { (char)247, 247 },
      { (char)1617, 248 },
      { (char)249, 249 },
      { (char)1618, 250 },
      { (char)251, 251 },
      { (char)252, 252 },
      { (char)8206, 253 },
      { (char)8207, 254 },
      { (char)1746, 255 }
    };

        #endregion
    }

}
