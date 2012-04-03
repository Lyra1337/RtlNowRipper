using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Web;

namespace Lyralabs.Net.RtlnowRipper
{
  class Crawler
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
        }
      }
    }
  }
}