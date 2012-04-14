using System;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;

namespace Lyralabs.Net.RtlnowRipper
{
  public class Crawler
  {
    private static readonly Regex episodeParser = new Regex("<a href=\"(?<url>(/berlin-tag-nacht/[^\\\"]*))\" title=\"Berlin - Tag &amp; Nacht[^\\\"]*\">(?<name>([^<]+))</a>", RegexOptions.Compiled);
    private TextWriter output = null;

    public string Url { get; set; }
    public string Directory { get; set; }

    public Crawler(string url, string directory, TextWriter log)
    {
      this.Url = url;
      this.Directory = directory;
      this.output = log;
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
          this.output.WriteLine(name);

          Query select = new Query("SELECT `guid` FROM `video` WHERE `name` = ?name");
          select.Add("?name", name);
          List<Dictionary<string, string>> result = select.Execute();
          if (result == null || result.Count == 0)
          {
            this.output.WriteLine("ripping {0}", name);
            Ripper ripper = new Ripper(url, this.output);
            string guid = Guid.NewGuid().ToString();
            if (ripper.Prepare())
            {
              this.output.WriteLine("prepare succeeded");
              string filename = ripper.Download(this.Directory);
              if (filename != null)
              {
                this.output.WriteLine("download succeeded");
                File.Copy(filename, String.Concat(this.Directory, "/", guid, ".flv"));
                this.output.WriteLine("file saved: {0}/{1}.flv", this.Directory, guid);
                Query insert = new Query("INSERT INTO `video` (`guid`, `url`, `name`, `added`) VALUES (?guid, ?url, ?name, UNIX_TIMESTAMP());");
                insert.Add("?guid", guid);
                insert.Add("?url", url);
                insert.Add("?name", name);
                insert.Execute();
              }
              else
              {
                this.output.WriteLine("download failed");
              }
            }
            else
            {
              this.output.WriteLine("prepare failed");
            }
          }
          else
          {
            this.output.WriteLine("already exists...");
          }

          this.output.WriteLine("\ncontinuing with next match...\n");
        }
      }
    }
  }
}