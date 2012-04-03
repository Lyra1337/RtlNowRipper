﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;
using System.Threading;
using System.IO;

namespace Lyralabs.Net.RtlnowRipper
{
  class Ripper
  {
    private static readonly Regex urlParser = new Regex("data:'(?<url>(%2Flogic%2Fgenerate_film_xml08.php[^']+))',", RegexOptions.Compiled);
    private static readonly Regex rtmpeUriParser = new Regex("rtmpe://fms-fra[0-9]*\\.rtl\\.de/rtl2now/(?<mp4path>(.+))", RegexOptions.Compiled);

    public string Url { get; set; }
    public string Rtmpe { get; set; }

    public Ripper(string url)
    {
      this.Url = url;
    }

    public bool Prepare()
    {
      WebClient client = new WebClient();

      string html = client.DownloadString(this.Url);
      Match match = Ripper.urlParser.Match(html);
      if (match.Success)
      {
        string apiUrl = String.Concat("http://rtl2now.rtl2.de", Uri.UnescapeDataString(match.Groups["url"].Value));

        Debug.Write(String.Concat("API-URL:\n\t", apiUrl, "\n"));

        string xml = client.DownloadString(apiUrl);

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        XmlNode node = doc.SelectSingleNode("/data/playlist/videoinfo/filename");

        if (node == null)
        {
          Console.WriteLine("ERR <<<< Node is null");
          return false;
        }

        this.Rtmpe = node.InnerText;

        Debug.Write(String.Concat("RTMPE-URL:\n\t", this.Rtmpe, "\n"));

        return true;
      }
      else
      {
        Console.WriteLine("ERR <<<< Regex failed");
        return false;
      }
    }

    public string Download()
    {
      if (Directory.Exists("files") == false)
      {
        Directory.CreateDirectory("files");
      }

      string filename = String.Concat("files/", Guid.NewGuid().ToString(), ".flv");

      string y = this.ParsePlaypath();

      string rtmpdump = String.Concat("-r \"rtmpe://fms-fra18.rtl.de:1935/rtl2now/\" -q -a \"rtl2now/\" -f \"WIN 11,1,102,63\" -W \"http://rtl2now.rtl2.de/includes/vodplayer.swf\" -p \"http://rtl2now.rtl2.de/berlin-tag-nacht/berlin-tag-nacht-folge-140.php?container_id=81971&player=1&season=2\" -y \"", y, "\" -o \"", filename, "\"");

      ProcessStartInfo psi = new ProcessStartInfo("rtmpdump", rtmpdump);
      //psi.UseShellExecute = false;
      //psi.RedirectStandardOutput = true;
      //psi.RedirectStandardInput = true;

      Process downloader = Process.Start(psi);

      while (!downloader.HasExited)
        Thread.Sleep(100);

      return filename;
    }

    private string ParsePlaypath()
    {
      Match match = Ripper.rtmpeUriParser.Match(this.Rtmpe);
      if (match.Success)
      {
        return String.Concat("mp4:", match.Groups["mp4path"].Value);
      }
      else
      {
        throw new UriFormatException(String.Concat("Failed to parse RTMPE Uri!", "\r\n", this.Rtmpe));
      }
    }
  }
}