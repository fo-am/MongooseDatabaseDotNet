using System;

using DataReciever.Main.Model;

namespace DataReciever.Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var reciever = new Reciever();

            reciever.Recieve<PackCreated>();
            reciever.Recieve<IndividualCreated>();
         

            Console.WriteLine("Waiting for values. Return to Exit.");
            Console.ReadLine();
        }
    }
}