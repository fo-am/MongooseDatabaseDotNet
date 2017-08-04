using System;
using System.Collections.Generic;
using System.IO;
using Dapper;
using Dapper.Contrib.Extensions;

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


            foreach (sync_entity entity in Data.GetUnsyncedEntitys())
            {
                //http://www.rabbitmq.com/blog/2011/02/10/introducing-publisher-confirms/
                send.PublishEntity(entity);
                Console.WriteLine(entity);
            }
            foreach (sync_value_varchar entity in Data.GetUnsyncedEntityValueVarchars())
            {
                //http://www.rabbitmq.com/blog/2011/02/10/introducing-publisher-confirms/
                send.PublishEntityVarchar(entity);
                Console.WriteLine(entity);
            }
            Console.ReadLine();
        }

    }
}