using System;

namespace DataReciever.Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var reciever = new Reciever();

            reciever.Recieve<IndividualCreated>();
            reciever.Recieve<IndividualDied>();

            Console.WriteLine("Waiting for values. Return to Exit.");
            Console.ReadLine();
        }
    }
}