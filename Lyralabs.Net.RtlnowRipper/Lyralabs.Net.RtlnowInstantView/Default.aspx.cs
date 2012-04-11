using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lyralabs.Net.RtlnowRipper;

namespace Lyralabs.Net.RtlnowInstantView
{
  public partial class Default : Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      Query select = new Query("SELECT * FROM `video`");

      List<Dictionary<string, string>> result = select.Execute();

      foreach (Dictionary<string, string> video in result)
      {
        TableRow row = new TableRow();
        TableCell cell = new TableCell();
        HyperLink link = new HyperLink();
        link.NavigateUrl = String.Concat("Watch.aspx?v=", video["guid"]);
        link.Text = video["name"];
        cell.Controls.Add(link);
        row.Cells.Add(cell);
        this.table.Rows.Add(row);
      }
    }
  }
}