using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Moose
{
    public class Db : DynamicObject
    {
        readonly string _connectionString;

        Dictionary<string, string> _scripts;

        public Db(string connectionString, string scriptsFolder = "db")
        {
            _connectionString = connectionString;
            _scripts = PopulateScripts(scriptsFolder);
        }

        public List<dynamic> Run(string commandText)
        {
            var results = new List<dynamic>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = commandText;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(RecordToExpando(reader));
                        }
                    }
                }
            }
            return results;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var scriptPart = binder.Name;

            var finder = new Finder(_scripts, this);
            result = finder.Find(scriptPart);

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var scriptPart = string.Join("?", indexes);
            var finder = new Finder(_scripts, this);
            result = finder.Find(scriptPart);

            return true;
        }

        Dictionary<string, string> PopulateScripts(string scriptsFolder)
        {
            var scripts = new Dictionary<string, string>();

            foreach (string file in Directory.EnumerateFiles(scriptsFolder, "*.sql", SearchOption.AllDirectories))
            {
                scripts.Add(
                    file.Replace($"{scriptsFolder}\\", "").Replace("\\", "?").Replace(".sql", ""),
                    File.ReadAllText(file));
            }

            return scripts;
        }

        static dynamic RecordToExpando(IDataReader reader)
        {
            dynamic e = new ExpandoObject();
            var d = (IDictionary<string, object>)e;
            object[] values = new object[reader.FieldCount];
            reader.GetValues(values);
            for (int i = 0; i < values.Length; i++)
            {
                var v = values[i];
                d.Add(reader.GetName(i), DBNull.Value.Equals(v) ? null : v);
            }
            return e;
        }

    }

    public class Finder : DynamicObject
    {
        readonly Dictionary<string, string> _scripts;

        readonly Db _db;

        readonly StringBuilder _scriptLocation;

        public Finder(Dictionary<string, string> scripts, Db db)
        {
            _scriptLocation = new StringBuilder();
            _scripts = scripts;
            _db = db;
        }

        public Finder Find(string scriptPart)
        {
            _scriptLocation.Append($"{scriptPart}?");
            return this;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            _scriptLocation.Append($"{binder.Name}?");
            result = this;

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            _scriptLocation.Append($"{binder.Name}");

            _db.Run(_scripts[_scriptLocation.ToString()]);

            result = null;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var scriptPart = string.Join("?", indexes);
            _scriptLocation.Append($"{scriptPart}?");

            result = this;

            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            _db.Run(_scripts[_scriptLocation.ToString().TrimEnd('?')]);

            result = null;
            return true;
        }
    }
}
