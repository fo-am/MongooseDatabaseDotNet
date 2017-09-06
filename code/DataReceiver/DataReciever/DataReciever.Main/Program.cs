using System;
using System.CodeDom;
using System.Linq;

namespace DataReciever.Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Data.SetupDatabase();

            var reciever = new Reciever();

            reciever.Recieve<stream_attribute>();
            reciever.Recieve<stream_entity>();
            reciever.Recieve<stream_value_file>();
            reciever.Recieve<stream_value_int>();
            reciever.Recieve<stream_value_real>();
            reciever.Recieve<stream_value_varchar>();
            reciever.Recieve<sync_attribute>();
            reciever.Recieve<sync_entity>();
            reciever.Recieve<sync_value_file>();
            reciever.Recieve<sync_value_int>();
            reciever.Recieve<sync_value_real>();
            reciever.Recieve<sync_value_varchar>();


            Console.WriteLine("Waiting for values. Return to Exit.");
            Console.ReadLine();
        }


    }
}