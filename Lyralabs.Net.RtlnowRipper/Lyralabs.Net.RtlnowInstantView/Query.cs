using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Lyralabs.Web.MobileStreamer.MySql
{
  public class Query
  {
    private MySqlCommand command = null;
    private bool firstAdd = true;
    
    public Query(string query)
    {
      if(Connection.SqlConnection != null)
        this.command = Connection.SqlConnection.CreateCommand();
      this.command.CommandText = query;
    }
    
    public void Add(string parameterName, string parameterValue)
    {
      if(this.firstAdd)
      {
        this.firstAdd = false;
        this.command.Prepare();
      }
      this.command.Parameters.AddWithValue(parameterName, parameterValue);
    }
    
    public MySqlCommand GetCommand()
    {
      if(this.command != null)
        return this.command;
      else
        return null;
    }
    
    public List<Dictionary<string, string>> Execute()
    {
      return Connection.SqlQuery(this);
    }
  }
}