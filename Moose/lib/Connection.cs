using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace Moose
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class Connection
    {
        public Connection()
        {
        }

        public void Execute(){
            
            using (var conn = new NpgsqlConnection("Host=localhost;Username=rob;Database=chinook"))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;

                    // Retrieve all rows
                    cmd.CommandText = "SELECT * FROM album";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader["title"].ToString());
                        }
                    }
                }
            }
        }
    }
}
