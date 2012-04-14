<%@ Page Title="Startseite" Language="C#" AutoEventWireup="true" CodeBehind="Watch.aspx.cs" Inherits="Lyralabs.Net.RtlnowInstantView.Watch" %>

<!DOCTYPE html>
<html>
<head>
  <title></title>
  <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.7/jquery.js"></script>
  <script src="http://releases.flowplayer.org/js/flowplayer-3.2.8.min.js"></script>
</head>
<body>
  <form runat="server">
  <div>
    <a id="player" style="width: 720px; height: 404px;"></a>
    <script lang="javascript">
      flashembed('player', 'http://releases.flowplayer.org/swf/flowplayer-3.2.8.swf', {
        config: {
          clip: 'http://<%=ConfigurationManager.AppSettings["steamerhost"] %>/files/<%=HttpUtility.HtmlEncode(Context.Request.QueryString["v"]) %>.flv',
          plugins: {
            controlbar: null
          }
        }
      });
    </script>
  </div>
  </form>
</body>
</html>