using System;

using DataReciever.Main.Model;

using NLog;
using NLog.Config;

namespace DataReciever.Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");

            var reciever = new Reciever();

            reciever.Recieve<PackCreated>();
            reciever.Recieve<IndividualCreated>();

            reciever.Recieve<WeightMeasure>();


            Console.WriteLine("Waiting for values. Return to Exit.");
            Console.ReadLine();
        }
    }
}