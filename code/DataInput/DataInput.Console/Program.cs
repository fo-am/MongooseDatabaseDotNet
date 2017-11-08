using System;

using DataInput.Core;

namespace DataInput.Console
{
    class Program
    {
        static void Main(string[] args)
        {
           //Connect to sqlite
           // connect to postgres
           // read from sqlite and write to postgres

            // find out the data structure of sqlite objects.
            // and map them to the postgres stuff

            // scan the database every 5 mins for new entities
            // copy them to the postgres and mark them copied.

            // find what updates look like
            // write them to the postgres

            // log loads of stuff. and so it feels good and nice!
            // maybe update times on postgres tables
            // maybe write the IDS from the sqlite database to help match data up.

            // sing and be happy


             
            var pgConnectionString = GetAppSettings.Get().PostgresConnectionString;
            
            System.Console.ReadLine();
        }
    }
}