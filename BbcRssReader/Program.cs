using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace BbcRssReader
{
    class Program
    {
        public static string targetDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\feed\"; //BbcRssReader\bin\Debug\feed
        public static string rssFeedUrl = "http://feeds.bbci.co.uk/news/uk/rss.xml";

        static void Main(string[] args)
        {
            List<NewsItem> newsItems = GetNewsFeed();
            CreateJsonFile(newsItems);
        }

        public static bool IsAlreadyStored(XmlElement desc)
        {
            bool exists = false;

            try
            {
                string[] files = Directory.GetFiles(targetDirectory);

                foreach (var file in files)
                {
                    DateTime fileName = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file), "yyyy-MM-dd-HH", CultureInfo.InvariantCulture);

                    if (fileName.Date == DateTime.Now)
                    {
                        using (StreamReader r = new StreamReader(file))
                        {
                            string json = r.ReadToEnd();
                            List<NewsItem> items = JsonConvert.DeserializeObject<List<NewsItem>>(json);

                            foreach (var item in items)
                            {
                                if (item.Title == desc.GetElementsByTagName("title")[0].InnerText)
                                {
                                    exists = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Directory location may not exist.");
            }

            return exists;
        }

        public static List<NewsItem> GetNewsFeed()
        {
            List<NewsItem> newsItems = new List<NewsItem>();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(rssFeedUrl);

                foreach (XmlNode description in doc.SelectNodes("//rss/channel/item"))
                {
                    XmlElement desc = (XmlElement)description;

                    if (!IsAlreadyStored(desc))
                    {
                        NewsItem newsItem = new NewsItem
                        {
                            Title = desc.GetElementsByTagName("title")[0].InnerText,
                            Description = desc.GetElementsByTagName("description")[0].InnerText,
                            Link = desc.GetElementsByTagName("link")[0].InnerText,
                            PubDate = Convert.ToDateTime(desc.GetElementsByTagName("pubDate")[0].InnerText)
                        };

                        newsItems.Add(newsItem);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("An error has occurred while attempting to access the RSS feed.");
            }

            return newsItems;
        }

        public static void CreateJsonFile(List<NewsItem> newsItems)
        {
            try
            {
                string json = JsonConvert.SerializeObject(newsItems, Newtonsoft.Json.Formatting.Indented);

                using (FileStream fileStream = File.Open(String.Format("{0}{1}{2}",
                                                            targetDirectory,
                                                            DateTime.Now.ToString("yyyy-MM-dd-HH"),
                                                            ".json"),
                                                         FileMode.CreateNew))
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jsonWriter, newsItems);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("An error has occurred while attempting to create the json file.");
            }
        }
    }
}

