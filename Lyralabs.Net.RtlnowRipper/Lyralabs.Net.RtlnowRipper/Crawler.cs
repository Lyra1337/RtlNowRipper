using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;

namespace Lyralabs.Net.RtlnowRipper
{
  public class Crawler
  {
    private static readonly Regex episodeParser = new Regex("<a href=\"(?<url>(/berlin-tag-nacht/[^\\\"]*))\" title=\"Berlin - Tag &amp; Nacht[^\\\"]*\">(?<name>([^<]+))</a>", RegexOptions.Compiled);

    public string Url { get; set; }

    public Crawler(string url)
    {
      this.Url = url;
    }

    public void Start()
    {
      WebClient client = new WebClient();

      string html = client.DownloadString(this.Url);

      MatchCollection mc = Crawler.episodeParser.Matches(html);

      foreach (Match m in mc)
      {
        if (m.Success)
        {
          string name = HttpUtility.HtmlDecode(m.Groups["name"].Value);
          string url = HttpUtility.HtmlDecode(m.Groups["url"].Value);
          Console.WriteLine(name);

          Query insert = new Query("INSERT INTO `video` (`guid`, `url`, `name`, `added`) VALUES (?guid, ?url, ?name, UNIX_TIMESTAMP());");
          string guid = Guid.NewGuid().ToString();
          insert.Add("?guid", guid);
          insert.Add("?url", url);
          insert.Add("?name", name);
          insert.Execute();
          Console.WriteLine("ripping {0}", name);
          Ripper ripper = new Ripper(url);

          if (ripper.Prepare())
          {
            Console.WriteLine("prepare succeeded");
            string filename = ripper.Download("files");
            if (filename != null)
            {
              Console.WriteLine("download succeeded");
              File.Move(filename, String.Concat("files/", guid, ".flv"));
              Console.WriteLine("file saved: files/{0}.flv", guid);
            }
            else
            {
              Console.WriteLine("download failed");
            }
          }
          else
          {
            Console.WriteLine("prepare failed");
          }

          Console.WriteLine("\ncontinuing with next match...\n");
        }
      }
    }
  }
}