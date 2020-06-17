using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPSecondSubtile.Parser
{
    public enum SubtitleEncoding
    {
        ASCII = -1,
        UTF7,
        UTF8,
        Unicode,
        BigEndianUnicode,
        UTF32,
        Windows1256
    }

    public class SubtitleEncodingHelper
    {
        public static SubtitleEncoding GetSubtitleEncoding(Stream stream)
        {
            SubtitleEncoding encoding;
            byte[] buffer = new byte[4];
            var fStream = stream;
            fStream.Read(buffer, 0, 4);
            if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
            {
                encoding = SubtitleEncoding.UTF7;
                System.Diagnostics.Debug.WriteLine("Auto Detecting Encoder: Encoding.UTF7");
            }
            //Buffers: 239 187 191
            else if (IsUtf8(stream) || buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf ||
                buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
            {
                encoding = SubtitleEncoding.UTF8;
                System.Diagnostics.Debug.WriteLine("Auto Detecting Encoder: Encoding.UTF8");
            }
            else if (buffer[0] == 0xff && buffer[1] == 0xfe)
            {
                encoding = SubtitleEncoding.Unicode;
                System.Diagnostics.Debug.WriteLine("Auto Detecting Encoder: Encoding.Unicode");
            }
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
            {
                encoding = SubtitleEncoding.BigEndianUnicode;
                System.Diagnostics.Debug.WriteLine("Auto Detecting Encoder: Encoding.BigEndianUnicode");
            }
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
            {
                encoding = SubtitleEncoding.UTF32;
                System.Diagnostics.Debug.WriteLine("Auto Detecting Encoder: Encoding.UTF32");
            }
            else if (buffer[0] == 49 && buffer[1] == 13
                  || buffer[0] == 60 && buffer[1] == 63 && buffer[2] == 120
                  || buffer[0] == 91 && buffer[1] == 83 && buffer[2] == 99
                  || buffer[0] == 123 && buffer[1] == 50 && buffer[2] == 52
                  || buffer[0] == 123 && buffer[1] == 72 && buffer[2] == 69
                  || buffer[0] == 91 && buffer[1] == 84 && buffer[2] == 73
                  || buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191
                  || buffer[0] == 42 && buffer[1] == 80 && buffer[2] == 65
                  || buffer[0] == 48 && buffer[1] == 48 && buffer[2] == 48
                  || buffer[0] == 48 && buffer[1] == 48 && buffer[2] == 58
                  || buffer[0] == 91 && buffer[1] == 73 && buffer[2] == 78)
            {
                encoding = SubtitleEncoding.Windows1256;
                System.Diagnostics.Debug.WriteLine("Auto Decting Encoder: Windows-1256");
            }
            else
            {
                encoding = SubtitleEncoding.ASCII;
                System.Diagnostics.Debug.WriteLine("Auto Detecting Encoder: Encoding.ASCII");
            }
            return encoding;
        }

        private static bool IsUtf8(Stream stream)
        {
            int count = 4 * 1024;
            byte[] buffer;
            int read;
            while (true)
            {
                buffer = new byte[count];
                stream.Seek(0, SeekOrigin.Begin);
                read = stream.Read(buffer, 0, count);
                if (read < count)
                {
                    break;
                }
                buffer = null;
                count *= 2;
            }
            return IsUtf8(buffer, read);
        }
        private static bool IsUtf8(byte[] buffer, int length)
        {
            int position = 0;
            int bytes = 0;
            while (position < length)
            {
                if (!IsValid(buffer, position, length, ref bytes))
                {
                    return false;
                }
                position += bytes;
            }
            return true;
        }
        private static bool IsValid(byte[] buffer, int position, int length, ref int bytes)
        {
            if (length > buffer.Length)
            {
                throw new ArgumentException("Invalid length");
            }

            if (position > length - 1)
            {
                bytes = 0;
                return true;
            }

            byte ch = buffer[position];

            if (ch <= 0x7F)
            {
                bytes = 1;
                return true;
            }

            if (ch >= 0xc2 && ch <= 0xdf)
            {
                if (position >= length - 2)
                {
                    bytes = 0;
                    return false;
                }
                if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }
                bytes = 2;
                return true;
            }

            if (ch == 0xe0)
            {
                if (position >= length - 3)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0xa0 || buffer[position + 1] > 0xbf ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }
                bytes = 3;
                return true;
            }


            if (ch >= 0xe1 && ch <= 0xef)
            {
                if (position >= length - 3)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 3;
                return true;
            }

            if (ch == 0xf0)
            {
                if (position >= length - 4)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0x90 || buffer[position + 1] > 0xbf ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                    buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 4;
                return true;
            }

            if (ch == 0xf4)
            {
                if (position >= length - 4)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0x8f ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                    buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 4;
                return true;
            }

            if (ch >= 0xf1 && ch <= 0xf3)
            {
                if (position >= length - 4)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                    buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 4;
                return true;
            }

            return false;
        }
    }
}
