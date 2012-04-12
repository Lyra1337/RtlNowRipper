using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Lyralabs.Net.RtlnowRipper;
using System.Configuration;

namespace Lyralabs.Net.RtlnowInstantView
{
  /// <summary>
  /// Zusammenfassungsbeschreibung für Refresh1
  /// </summary>
  public class Refresh1 : IHttpHandler
  {

    public void ProcessRequest(HttpContext context)
    {
      context.Response.ContentType = "text/plain";
      context.Response.ContentEncoding = new UTF8Encoding(false);

      Crawler crawler = new Crawler("http://rtl2now.rtl2.de/berlin-tag-nacht.php", ConfigurationManager.AppSettings["filesdir"], context.Response.Output);
      crawler.Start();
    }

    public bool IsReusable
    {
      get
      {
        return false;
      }
    }
  }
}