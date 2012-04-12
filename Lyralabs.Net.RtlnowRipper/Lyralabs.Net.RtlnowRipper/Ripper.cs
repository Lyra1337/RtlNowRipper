using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Text;

namespace Lyralabs.Net.RtlnowRipper
{
  public class Ripper
  {
    private static readonly string FFMPEG_PATH = Environment.OSVersion.Platform == PlatformID.Unix ? "ffmpeg" : "lib\\ffmpeg.exe";
    private static readonly string RTMPDUMP_PATH = Environment.OSVersion.Platform == PlatformID.Unix ? "rtmpdump" : "lib\\rtmpdump.exe";

    private static readonly Regex urlParser = new Regex("data:'(?<url>(%2Flogic%2Fgenerate_film_xml08.php[^']+))',", RegexOptions.Compiled);
    private static readonly Regex rtmpeUriParser = new Regex("rtmpe://fms-fra[0-9]*\\.rtl\\.de/rtl2now/(?<mp4path>(.+))", RegexOptions.Compiled);

    private TextWriter output = null;

    public string Url { get; set; }
    public string Rtmpe { get; set; }

    public Ripper(string url, TextWriter log)
    {
      this.output = log;
      if (url.StartsWith("/"))
      {
        this.Url = String.Concat("http://rtl2now.rtl2.de", url);
      }
      else
      {
        this.Url = url;
      }
    }

    public bool Prepare()
    {
      WebClient client = new WebClient();

      string html = client.DownloadString(this.Url);
      Match match = Ripper.urlParser.Match(html);
      if (match.Success)
      {
        string apiUrl = String.Concat("http://rtl2now.rtl2.de", Uri.UnescapeDataString(match.Groups["url"].Value));

        string xml = client.DownloadString(apiUrl);

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        XmlNode node = doc.SelectSingleNode("/data/playlist/videoinfo/filename");

        if (node == null)
        {
          this.output.WriteLine("ERROR <<<< Node 'filename' is null");
          return false;
        }

        this.Rtmpe = node.InnerText;

        return true;
      }
      else
      {
        this.output.WriteLine("ERROR <<<< Regex failed");
        return false;
      }
    }

    public string Download(string directory)
    {
      directory = String.Concat(Environment.OSVersion.Platform == PlatformID.Unix ? String.Empty : Thread.GetDomain().BaseDirectory, directory);

      if (Directory.Exists(directory) == false)
      {
        Directory.CreateDirectory(directory);
      }

      string filename = String.Concat(directory, "/", Guid.NewGuid().ToString(), ".flv");

      string y = this.ParsePlaypath();

      string rtmpdump = String.Concat("-r \"rtmpe://fms-fra18.rtl.de:1935/rtl2now/\" -a \"rtl2now/\" -f \"WIN 11,1,102,63\" -W \"http://rtl2now.rtl2.de/includes/vodplayer.swf\" -p \"http://rtl2now.rtl2.de/berlin-tag-nacht/berlin-tag-nacht-folge-140.php?container_id=81971&player=1&season=2\" -y \"", y, "\" -o \"", filename, "\"");

      ProcessStartInfo psi = new ProcessStartInfo(String.Concat(Environment.OSVersion.Platform == PlatformID.Unix ? String.Empty : Thread.GetDomain().BaseDirectory, Ripper.RTMPDUMP_PATH), rtmpdump);
      psi.WorkingDirectory = Environment.OSVersion.Platform == PlatformID.Unix ? String.Empty : Thread.GetDomain().BaseDirectory;
      psi.UseShellExecute = false;
      psi.RedirectStandardOutput = true;
      psi.RedirectStandardError = true;
      psi.CreateNoWindow = true;
      //psi.RedirectStandardInput = true;

      Process downloader = Process.Start(psi);
      StringBuilder sb = new StringBuilder();
      while (!downloader.StandardOutput.EndOfStream)
      {
        string line = downloader.StandardOutput.ReadLine();
        if (String.IsNullOrEmpty(line) == false)
          sb.AppendFormat(line);
      }

      do
      {
        Thread.Sleep(1000);
      }
      while (!downloader.HasExited);


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