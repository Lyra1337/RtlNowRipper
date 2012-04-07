using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace Lyralabs.Web.MobileStreamer.MySql
{
  public class Connection
  {
    private static readonly object queryLock = new object();
    private static int queryCount;
    private static bool isConnected = false;
    public static MySqlConnection SqlConnection { get; set; }

    public static int QueryCount
    {
      get
      {
        return Connection.queryCount;
      }
      private set
      {
        Connection.queryCount = value;
      }
    }

    static Connection()
    {
      Connection.connectWithSettings();
    }

    private static void connectWithSettings()
    {
      string host = ConfigurationManager.AppSettings["dbhost"];
      string user = ConfigurationManager.AppSettings["dbuser"];
      string pass = ConfigurationManager.AppSettings["dbpass"];
      string name = ConfigurationManager.AppSettings["dbname"];

      connect(host, user, pass, name);
    }

    private static void connect(string host, string user, string pass, string name)
    {
      isConnected = true;
      Connection.queryCount = 0;
      string myConnectionString = String.Concat("SERVER=", host, ";",
                                  "DATABASE=", name, ";",
                                  "UID=", user, ";",
                                  "PASSWORD=", pass, ";");

      Connection.SqlConnection = new MySqlConnection(myConnectionString);
      Connection.SqlConnection.Open();

      if (!Connection.CheckConnection())
        throw new Exception("connection failed!");
    }

    public static bool CheckConnection()
    {
      return (Connection.SqlConnection.State == System.Data.ConnectionState.Open);
    }

    public Connection()
    {
    }

    public static List<Dictionary<string, string>> SqlQuery(Query query)
    {
      lock (queryLock)
      {
        if (!isConnected)
          Connection.connectWithSettings();

        List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
        MySqlCommand command = query.GetCommand();
        using (MySqlDataReader reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            Dictionary<string, string> currentResult = new Dictionary<string, string>();

            for (int i = 0; i < reader.FieldCount; i++)
              currentResult.Add(reader.GetName(i).ToString(), reader.GetValue(i).ToString());

            result.Add(currentResult);
          }
        }
        //reader.Close();

        Connection.queryCount++;
        return result;
      }
    }
  }
}