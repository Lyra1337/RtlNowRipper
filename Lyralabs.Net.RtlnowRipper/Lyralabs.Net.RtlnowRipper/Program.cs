using System;
using System.Diagnostics;
using System.Threading;

namespace Lyralabs.Net.RtlnowRipper
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length == 1)
      {
        Console.WriteLine("   --  RTL2NOW Ripper  --");
        Ripper ripper = new Ripper(args[0]);
        Console.Write(" > Prepare Stream...");
        if (ripper.Prepare())
        {
          WriteColoredLine(" success", ConsoleColor.Green);
        }
        else
        {
          WriteColoredLine(" failed", ConsoleColor.Red);
          Console.WriteLine(" > Exiting...");
          Thread.Sleep(1000);
          return;
        }
        Console.Write(" > Downloading Stream...");
        Stopwatch sw = new Stopwatch();
        sw.Start();
        string filename = ripper.Download("files");
        sw.Stop();
        double seconds = sw.Elapsed.TotalSeconds;
        WriteColored(" success", ConsoleColor.Green);
        Console.WriteLine(" (took {0} second{1})", seconds, seconds == 1 ? String.Empty : "s");
        Console.WriteLine(" > {0}", filename);

        return;
      }
      else
      {
        Crawler crawler = new Crawler("http://rtl2now.rtl2.de/berlin-tag-nacht.php");
        crawler.Start();

        Console.ReadKey();

        Stopwatch sw = new Stopwatch();
        sw.Start();
        Ripper ripper = new Ripper("http://rtl2now.rtl2.de/berlin-tag-nacht/berlin-tag-nacht-folge-141.php?container_id=82189&player=1&season=2");
        ripper.Prepare();
        sw.Stop();

        Console.Write("Took {0} ms", sw.ElapsedMilliseconds);

        ripper.Download("files");

        Console.ReadKey();
      }
    }

    static void WriteColored(string text, ConsoleColor color)
    {
      ConsoleColor temp = Console.ForegroundColor;
      Console.ForegroundColor = color;
      Console.Write(text);
      Console.ForegroundColor = temp;
    }

    static void WriteColoredLine(string text, ConsoleColor color)
    {
      WriteColored(String.Concat(text, "\n"), color);
    }
  }
}