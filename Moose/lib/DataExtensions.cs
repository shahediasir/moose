using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Moose
{
  public static class DataExtensions
  {
    public static List<T> ToList<T>(this IDataReader rdr) where T : new()
    {
      var result = new List<T>();
      while (rdr.Read())
      {
        result.Add(rdr.ToSingle<T>());
      }
      return result;
    }

    public static T ToSingle<T>(this IDataReader rdr) where T : new()
    {

      var item = new T();
      Type t = item.GetType();
      
      var props = item.GetType().GetProperties();

      foreach (var prop in props)
      {
        for (int i = 0; i < rdr.FieldCount; i++)
        {
          if (rdr.GetName(i).Equals(prop.Name, StringComparison.CurrentCultureIgnoreCase))
          {
            var val = rdr.GetValue(i);
            if (val != DBNull.Value)
            {
              prop.SetValue(item, val);
            }
            else
            {
              prop.SetValue(item, null);
            }
          }
        }

      }

      return item;
    }
    /// <summary>
    /// Extension method for adding in a bunch of parameters
    /// </summary>
    public static void AddParams(this NpgsqlCommand cmd, params object[] args)
    {
      foreach (var item in args)
      {
        AddParam(cmd, item);
      }
    }
    /// <summary>
    /// Extension for adding single parameter
    /// </summary>
    public static void AddParam(this NpgsqlCommand cmd, object item)
    {
      var p = cmd.CreateParameter();
      p.ParameterName = string.Format("@{0}", cmd.Parameters.Count);
      if (item == null)
      {
        p.Value = DBNull.Value;
      }
      else
      {
        if (item.GetType() == typeof(Guid))
        {
          p.Value = item.ToString();
          p.DbType = DbType.String;
          p.Size = 4000;
        }
        else
        {
          p.Value = item;
        }
        if (item.GetType() == typeof(string))
          p.Size = ((string)item).Length > 4000 ? -1 : 4000;
      }
      cmd.Parameters.Add(p);
    }
  }
}