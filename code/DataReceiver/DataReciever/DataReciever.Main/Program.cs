using System;
using System.Data.SQLite;
using System.IO;

namespace DataReciever.Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Data.SetupDatabase();

            var reciever = new Reciever();

            reciever.RecieveEntities();
            reciever.RecieveValues();

            Console.WriteLine("Waiting for values. Return to Exit.");
            Console.ReadLine();
        }


    }
}