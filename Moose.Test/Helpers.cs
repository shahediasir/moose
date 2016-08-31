using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moose.Test
{
    public static class Helpers
    {
        static string connectionString = "Host=localhost;Username=postgres;Password=lokkuc1;Database=moose";

        public static void ResetDb(string seedSchema = null)
        {
            if (string.IsNullOrWhiteSpace(seedSchema))
            {
                seedSchema = "default";
            }

            dynamic db = new Db(connectionString);
            var schemata = db.Run("select schema_name from information_schema.schemata where catalog_name = 'moose' and schema_name not like 'pg_%' and schema_name not like 'information_schema'");

            foreach (var schema in schemata)
            {
                db.Run("drop schema " + schema.schema_name + " cascade");
            }

            db.schemata.loader();
        }

    }
}
