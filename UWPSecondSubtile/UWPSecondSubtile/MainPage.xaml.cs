using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UWPSecondSubtile.Parser;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The subtile parser code is from https://github.com/ramtinak/UltraPlayer

namespace UWPSecondSubtile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaPlayer player;
        MediaSource mediaSource1;
        MediaSource mediaSource2;

        double seekDouble = 0;
        List<SubtitleItem> SubtitleList;
        SubtitleItem CurrentSubtitleItem;
        DispatcherTimer dt = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            dt.Interval = TimeSpan.FromMilliseconds(1);
            dt.Tick += Dt_Tick;
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            player = new MediaPlayer();
            mediaSource1 = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/elephantsdream-clip-h264_sd-aac_eng-aac_spa-aac_eng_commentary-srt_eng-srt_por-srt_swe.mkv"));
            mediaSource2 = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/bbb_sunflower_1080p_60fps_normal.mp4"));
            player.Source = mediaSource1;
            MyPlayer.SetMediaPlayer(player);
        }

        private async void LoadCC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/secondCC.srt"));
            if (file == null)
                return;

            try
            {
                Stream stream = await file.OpenStreamForReadAsync();

                var parser = new SubParser();
                var fileName = Path.GetFileName(file.Path);
                SubtitleEncoding Subencoding = SubtitleEncodingHelper.GetSubtitleEncoding(stream);
                Encoding encoding = new UTF8Encoding();
                if (Subencoding == SubtitleEncoding.ASCII)
                    encoding = new ASCIIEncoding();
                else if (Subencoding == SubtitleEncoding.Unicode)
                    encoding = new UnicodeEncoding();
                else if (Subencoding == SubtitleEncoding.UTF8)
                    encoding = new UTF8Encoding();
                else if (Subencoding == SubtitleEncoding.Windows1256)
                    encoding = new CustomCP1256Encoding();
                else if (Subencoding == SubtitleEncoding.UTF7)
                    encoding = new UTF7Encoding();
                else if (Subencoding == SubtitleEncoding.UTF32)
                    encoding = Encoding.UTF32;
                else if (Subencoding == SubtitleEncoding.BigEndianUnicode)
                    encoding = Encoding.BigEndianUnicode;
                var mostLikelyFormat = parser.GetMostLikelyFormat(fileName, stream, encoding);

                Debug.WriteLine("mostLikelyFormat: " + mostLikelyFormat.Name);
                SubtitleList = parser.ParseStream(await file.OpenStreamForReadAsync(), encoding, mostLikelyFormat);
                dt.Start();
            }
            catch (Exception ex) { }
        }

        private void GetSystemCCStyle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.BackgroundColor != Windows.Media.ClosedCaptioning.ClosedCaptionColor.Default)
            {
                border.Background = new SolidColorBrush(Windows.Media.ClosedCaptioning.ClosedCaptionProperties.ComputedBackgroundColor);

                Color backColor = Windows.Media.ClosedCaptioning.ClosedCaptionProperties.ComputedBackgroundColor;
                switch (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.BackgroundOpacity)
                {
                    case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.OneHundredPercent:
                        border.Background = new SolidColorBrush(Color.FromArgb(255, backColor.R, backColor.G, backColor.B));//.Opacity = 1.0;
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.SeventyFivePercent:
                        border.Background = new SolidColorBrush(Color.FromArgb(192, backColor.R, backColor.G, backColor.B));
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.TwentyFivePercent:
                        border.Background = new SolidColorBrush(Color.FromArgb(64, backColor.R, backColor.G, backColor.B));
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.ZeroPercent:
                        border.Background = new SolidColorBrush(Color.FromArgb(0, backColor.R, backColor.G, backColor.B));
                        break;
                    default:
                        border.Background = new SolidColorBrush(Color.FromArgb(0, backColor.R, backColor.G, backColor.B));
                        break;
                }
            }
            else
            {
                Color backColor = Colors.Black;
                switch (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.BackgroundOpacity)
                {
                    case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.OneHundredPercent:
                        border.Background = new SolidColorBrush(Color.FromArgb(255, backColor.R, backColor.G, backColor.B));//.Opacity = 1.0;
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.SeventyFivePercent:
                        border.Background = new SolidColorBrush(Color.FromArgb(192, backColor.R, backColor.G, backColor.B));
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.TwentyFivePercent:
                        border.Background = new SolidColorBrush(Color.FromArgb(64, backColor.R, backColor.G, backColor.B));
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.ZeroPercent:
                        border.Background = new SolidColorBrush(Color.FromArgb(0, backColor.R, backColor.G, backColor.B));
                        break;
                    default:
                        border.Background = new SolidColorBrush(Color.FromArgb(0, backColor.R, backColor.G, backColor.B));
                        break;
                }
            }

            if (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.FontColor != Windows.Media.ClosedCaptioning.ClosedCaptionColor.Default)
                richtextblock.Foreground = new SolidColorBrush(Windows.Media.ClosedCaptioning.ClosedCaptionProperties.ComputedFontColor);
            else
                richtextblock.Foreground = new SolidColorBrush(Colors.White);

            switch (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.FontOpacity)
            {
                case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.OneHundredPercent:
                    richtextblock.Opacity = 1.0;
                    break;
                case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.SeventyFivePercent:
                    richtextblock.Opacity = 0.75;
                    break;
                case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.TwentyFivePercent:
                    richtextblock.Opacity = 0.25;
                    break;
                case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.ZeroPercent:
                    richtextblock.Opacity = 0.0;
                    break;
                case Windows.Media.ClosedCaptioning.ClosedCaptionOpacity.Default:
                    richtextblock.Opacity = 1.0;
                    break;
            }

            //if(Windows.Media.ClosedCaptioning.ClosedCaptionProperties.FontStyle != Windows.Media.ClosedCaptioning.ClosedCaptionStyle.Default)
            {
                switch (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.FontStyle)
                {
                    case Windows.Media.ClosedCaptioning.ClosedCaptionStyle.Casual:
                        richtextblock.FontFamily = new FontFamily("Segoe Print");
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionStyle.Cursive:
                        richtextblock.FontFamily = new FontFamily("Segoe Script");
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionStyle.MonospacedWithoutSerifs:
                        richtextblock.FontFamily = new FontFamily("Lucida Sans Unicode");
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionStyle.MonospacedWithSerifs:
                        richtextblock.FontFamily = new FontFamily("Courier New");
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionStyle.ProportionalWithoutSerifs:
                        richtextblock.FontFamily = new FontFamily("Arial");
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionStyle.ProportionalWithSerifs:
                        richtextblock.FontFamily = new FontFamily("Times New Roman");
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionStyle.SmallCapitals:
                        richtextblock.FontFamily = new FontFamily("Arial");
                        break;
                    default:
                        richtextblock.FontFamily = new FontFamily("Arial");
                        break;
                }
            }

            //if (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.FontSize != Windows.Media.ClosedCaptioning.ClosedCaptionSize.Default)
            {
                //系统默认不返回字体的具体大小，而是一个愚蠢的百分比。官方解释说具体的字体大小会根据窗体大小等一系列因素决定，但是又不给你说怎么个计算方法
                //所以这里就先给一个初始值。如果你知道怎么计算或者获取最终大小，请create PR。
                double defaultSize = 50;
                switch (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.FontSize)
                {
                    case Windows.Media.ClosedCaptioning.ClosedCaptionSize.FiftyPercent:
                        richtextblock.FontSize = defaultSize * .5;
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionSize.OneHundredPercent:
                        richtextblock.FontSize = defaultSize * 1;
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionSize.OneHundredFiftyPercent:
                        richtextblock.FontSize = defaultSize * 1.5;
                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionSize.TwoHundredPercent:
                        richtextblock.FontSize = defaultSize * 2.0;
                        break;
                    default:
                        richtextblock.FontSize = defaultSize * 1.0;
                        break;
                }
            }

            if (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.FontEffect != Windows.Media.ClosedCaptioning.ClosedCaptionEdgeEffect.Default)
            {
                switch (Windows.Media.ClosedCaptioning.ClosedCaptionProperties.FontEffect)
                {
                    case Windows.Media.ClosedCaptioning.ClosedCaptionEdgeEffect.None:

                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionEdgeEffect.Raised:

                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionEdgeEffect.Depressed:

                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionEdgeEffect.Uniform:

                        break;
                    case Windows.Media.ClosedCaptioning.ClosedCaptionEdgeEffect.DropShadow:

                        break;
                    default:

                        break;
                }
            }

        }

        private void SwitchVideoSource_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(player.Source == mediaSource1)
                player.Source = mediaSource2;
            else
                player.Source = mediaSource1;
        }

        private void Dt_Tick(object sender, object e)
        {
            try
            {
                if (SubtitleList != null && SubtitleList.Any())
                {
                    var v = (from item in SubtitleList
                             where item != null
                             && item.StartTime + seekDouble <= MyPlayer.MediaPlayer.PlaybackSession.Position.TotalMilliseconds
                             && item.EndTime + seekDouble >= MyPlayer.MediaPlayer.PlaybackSession.Position.TotalMilliseconds
                             orderby item descending
                             select item).FirstOrDefault();
                    CurrentSubtitleItem = v;
                    if (v != null)
                    {
                        richtextblock.Blocks.Clear();

                        Paragraph myParagraph = new Paragraph();
                        int nextParagraph = 1;
                        string paragraph = "";
                        foreach (string item in v.Lines)
                        {
                            paragraph += item.Trim().ToString() + "\r\n";
                            if (GetRun(item) != null)
                            {
                                myParagraph.Inlines.Add(GetRun(item.Trim()));
                                try
                                {
                                    if (v.Lines[nextParagraph] != null)
                                    {
                                        myParagraph.Inlines.Add(new LineBreak());
                                    }
                                }
                                catch (Exception ex) { Debug.WriteLine("nextParagraph ex: " + ex.Message); }
                            }
                            nextParagraph++;
                        }
                        //Run run = new Run();
                        //run.Text = paragraph.Trim();
                        //myParagraph.Inlines.Add(run);
                        richtextblock.Blocks.Add(myParagraph);
                        border.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        border.Visibility = Visibility.Collapsed;
                        richtextblock.Blocks.Clear();
                    }
                }
                else
                    richtextblock.Blocks.Clear();
            }
            catch (Exception ex) { Debug.WriteLine("mediaPlayer_PositionChanged ex: " + ex.Message); }

        }

        Run GetRun(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = StringHelper.FixInvalidItalicTags(text);
                text = StringHelper.FixInvalidFontTags(text);
                text = StringHelper.FixInvalidUnderlineTags(text);
                text = StringHelper.FixInvalidBoldTags(text);
                Run run = new Run();
                string pattern = @"<b>(?<bold>.*)</b>";
                MatchCollection reg = Regex.Matches(text, pattern);
                if (reg.Count > 0)
                    run.FontWeight = new Windows.UI.Text.FontWeight { Weight = 700 };

                pattern = @"<i>(?<italic>.*)</i>";
                reg = Regex.Matches(text, pattern);
                if (reg.Count > 0)
                    run.FontStyle = Windows.UI.Text.FontStyle.Italic;

                pattern = @"<font color=([^ >]*)"; //pattern = @"color=""(?<color>.*)""";
                reg = Regex.Matches(text, pattern);
                if (reg.Count > 0)
                {
                    foreach (Match item in reg)
                    {
                        string ss = item.Groups[1].ToString();
                        string property = "1";
                        try
                        {
                            if (item.Groups[property].ToString().StartsWith("#"))
                            {
                                if (item.Groups[property].ToString().Length == 7)
                                    run.Foreground = new SolidColorBrush(ColorHelper.GetColorFromHex("#ff" + item.Groups["color"].ToString().Substring(1)));
                                else
                                    run.Foreground = new SolidColorBrush(ColorHelper.GetColorFromHex(item.Groups["color"].ToString()));
                            }
                            else if (item.Groups[property].ToString().ToLower().Equals("white"))
                                run.Foreground = new SolidColorBrush(Colors.White);
                            else if (item.Groups[property].ToString().ToLower().Equals("red"))
                                run.Foreground = new SolidColorBrush(Colors.Red);
                            else if (item.Groups[property].ToString().ToLower().Equals("cyan"))
                                run.Foreground = new SolidColorBrush(Colors.Cyan);
                            else if (item.Groups[property].ToString().ToLower().Equals("yellow"))
                                run.Foreground = new SolidColorBrush(Colors.Yellow);
                            else if (item.Groups[property].ToString().ToLower().Equals("orange"))
                                run.Foreground = new SolidColorBrush(Colors.Orange);
                            else if (item.Groups[property].ToString().ToLower().Equals("blue"))
                                run.Foreground = new SolidColorBrush(Colors.Blue);
                            else if (item.Groups[property].ToString().ToLower().Equals("black"))
                                run.Foreground = new SolidColorBrush(Colors.Black);
                            else if (item.Groups[property].ToString().ToLower().Equals("brown"))
                                run.Foreground = new SolidColorBrush(Colors.Brown);
                            else if (item.Groups[property].ToString().ToLower().Equals("green"))
                                run.Foreground = new SolidColorBrush(Colors.Green);
                            else if (item.Groups[property].ToString().ToLower().Equals("pink"))
                                run.Foreground = new SolidColorBrush(Colors.Pink);
                            else
                                Debug.WriteLine(string.Format("Color '{0}' not supported", item.Groups[property].ToString()));
                            // white,red,cyan,yellow,orange,blue,black,brown,green
                        }
                        catch (Exception Exception) { Debug.WriteLine("GetRun ex: " + Exception.Message); }
                    }

                }
                text = StringHelper.RemoveHtmlTags(text).Replace("</font>", "").Replace("</i>", "").Replace("</b>", "");
                run.Text = text;
                return run;
            }
            return null;
        }


    }
}
