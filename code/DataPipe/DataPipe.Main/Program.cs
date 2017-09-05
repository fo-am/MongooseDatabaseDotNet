using System;
using System.Threading;

namespace DataPipe.Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Get data not yet sent from the database
            // place the data on a queue
            // mark the data sent
            var send = new Sender();
            while (true)
            {
                foreach (var entity in Data.GetUnsyncedStreamAttribute())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.id}");
                }

                foreach (var entity in Data.GetUnsyncedStreamEntity())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedStreamValueFile())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedStreamValueInt())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedStreamValueReal())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedStreamValueVarchar())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedSyncAttribute())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.id}");
                }

                foreach (var entity in Data.GetUnsyncedSyncEntity())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedSyncValueFile())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedSyncValueInt())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedSyncValueReal())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }

                foreach (var entity in Data.GetUnsyncedSyncValueVarchar())
                {
                    send.PublishEntity(entity);
                    Console.WriteLine($"{entity} {entity.entity_id}");
                }


                Console.WriteLine("Scanning for more data");
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
            Console.ReadLine();
        }
    }
}