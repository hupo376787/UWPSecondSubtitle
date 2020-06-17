﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace UWPSecondSubtile.Parser
{

    public class JSONParser : ISubtitlesParser
    {
        public List<SubtitleItem> ParseStream(Stream subStream, Encoding encoding)
        {
            subStream.Position = 0;
            var items = new List<SubtitleItem>();
            var reader = new StreamReader(subStream, encoding, true);
            string reshte = reader.ReadToEnd();

            JsonArray jsonArray;
            if (JsonArray.TryParse(reshte, out jsonArray))
            {
                //try
                {
                    foreach (JsonValue groupValue in jsonArray)
                    {
                        JsonObject groupObject = groupValue.GetObject();
                        JsonObject itemObject = groupObject.GetObject();
                        //try
                        {
                            double startText = itemObject["start"].GetNumber();
                            double endText = itemObject["end"].GetNumber();
                            int start = ParseJSONTimecode(startText.ToString());
                            int end = ParseJSONTimecode(endText.ToString());

                            string text = itemObject["text"].GetString();
                            //<br />
                            items.Add(new SubtitleItem
                            {
                                StartTime = start,
                                EndTime = end,
                                Lines = new List<string>(text.Split(new string[] { "<br />", Environment.NewLine,
                                "<br/>","/<br>"}, StringSplitOptions.RemoveEmptyEntries))

                            });

                        }
                        //catch { }


                    }
                }
                //catch (Exception ex)
                //{

                //}
            }
            if (items.Any())
            {
                return items;
            }
            else
            {
                throw new ArgumentException("Stream is not in a valid JSON format");
            }
        }



        
        private int ParseJSONTimecode(string s)
        {
            s = s.Replace(".", "");
            int d;
            if (int.TryParse(s, out d))
                return d;
            return -1;
        }

    }
}
