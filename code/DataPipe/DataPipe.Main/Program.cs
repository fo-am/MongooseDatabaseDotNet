using System;
using System.Threading;

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
            while (true)
            {
                foreach (sync_entity entity in Data.GetUnsyncedEntitys())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id.ToString()}");
                }
                foreach (sync_value_varchar entity in Data.GetUnsyncedEntityValueVarchars())
                {
                    send.PublishEntityVarchar(entity);
                    Console.WriteLine($"{entity} {entity.id.ToString()}");
                }

                Console.WriteLine("Scanning for more data");
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
            Console.ReadLine();
        }
    }
}