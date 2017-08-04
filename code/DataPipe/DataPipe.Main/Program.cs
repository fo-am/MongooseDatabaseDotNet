using System;

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
                send.PublishEntity(entity);
                Console.WriteLine(entity);
            }
            foreach (sync_value_varchar entity in Data.GetUnsyncedEntityValueVarchars())
            {
                send.PublishEntityVarchar(entity);
                Console.WriteLine(entity);
            }

            Console.ReadLine();
        }

    }
}