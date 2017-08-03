using System;
using System.Collections.Generic;
using System.IO;
using Dapper;

namespace DataPipe.Main
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get data not yet sent from the database
            // place the data on a queue
            // mark the data sent
            var send = new Sender();
          

           foreach(var data in GetData())
            {
                //http://www.rabbitmq.com/blog/2011/02/10/introducing-publisher-confirms/
                send.Publish(data.data);
            }
          
            Console.ReadLine();
        }
        public static IEnumerable<Model> GetData()
        {
         
            if (!File.Exists(Path.GetFullPath(Model.DbFile))) return null;

            using (var cnn = Model.SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<Model>(
                    @"SELECT id, data FROM test");
                return result;
            }
        }
    }
}
