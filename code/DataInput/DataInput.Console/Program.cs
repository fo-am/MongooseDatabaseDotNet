using System;

using DataInput.Core;
using System.Linq;
using Newtonsoft.Json;
using DataPipe.Main.Model;

using DataInput.Core;

namespace DataInput.Console
{
    internal class Program
    {
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

            var sender = new Sender();
            sender.PublishEntity(type, json);
        }
    }
}