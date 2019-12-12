using System;

using System.Data;
using System.Linq;

using Dapper;

using Npgsql;
using DataInput.Core;

namespace DataInput.Console
{
    internal class Program
    {
        private static AppSettings appSettings;

        public Program()
        {
        }

        private static void Main(string[] args)
        {
            // from remote postgres database, get list of recieved messages
            // deserialise into objects
            // publish to queue
            // ensure data reciever is configured to point at new database (local postgres?)
            // Example row
            // event_log_id, message_id, delivered_count, type, object, success, error, date_created
            // 2 "3b6190ab-ab08-4fbe-bfc9-00cb998eb834"  1   "DataReceiver.Main.Model.PackCreated"   "{"sent":0,"UniqueId":"Mwesige Kenneth Araali - 1446615300:186285","entity_id":158,"entity_type":"pack","Name":"1HA","CreatedDate":"2015 - 11 - 04T08: 35:00"}"    true        "2018-11-18 19:15:56.417023"

            string type = "DataReceiver.Main.Model.PackCreated";
            string json = @"{""sent"":0,""UniqueId"":""Mwesige Kenneth Araali - 1446615300:186285"",""entity_id"":158,""entity_type"":""pack"",""Name"":""1HA"",""CreatedDate"":""2015 - 11 - 04T08: 35:00""}";
            string messageId = "";
            appSettings = GetAppSettings.Get();
            using (IDbConnection conn = new NpgsqlConnection(appSettings.SourcePostgres))
            {
                var list = conn.Query<Tuple<string, string, string>>(@"SELECT message_id as Item1, type as Item2, object as Item3
	                                     FROM mongoose.event_log order by event_log_id asc;").ToList();
                var sender = new Sender();

                foreach (var item in list)
                {
                    sender.PublishEntity(item.Item1, item.Item2, item.Item3);
                }
            }
        }
    }
}