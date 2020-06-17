using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UWPSecondSubtile.Parser
{
    public interface ISubtitlesParser
    {
        List<SubtitleItem> ParseStream(Stream stream, Encoding encoding);
    }
}