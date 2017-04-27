using System.Collections.Generic;
using System.Data.OleDb;

namespace DataImporter.Data
{
    public class AccessData
    {
        public HashSet<string> GetGroups()
        {
            var idsList = new HashSet<string>();

            using (var accessconn = new OleDbConnection())
            {
                accessconn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                                              @"Data source= C:\Users\aidan\Dropbox\foam\TheDatabase\" +
                                              @"Project replica August 2016.mdb";

                accessconn.Open();
                using (var cmd = new OleDbCommand())
                {
                    cmd.Connection = accessconn;
                    cmd.CommandText = "Select distinct group from weights";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = reader.GetString(0);
                            System.Console.WriteLine(item + " Added: " + idsList.Add(item));
                        }
                    }
                }
            }
            return idsList;
        }
    }
}
