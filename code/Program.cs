using DataImporter.Data;

namespace DataImporter.Console
{
    //Get all groupIds from access database
    // put them in some sort of data strucutre.
    // maybe check they are okay? all two chars? or something
    // write them all to the postgres database.


    public class Program
    {
        private static void Main(string[] args)
        {
            var accessData = new AccessData();

            var idsList = accessData.GetGroups();
            System.Console.WriteLine("Total Groups: " + idsList.Count);
            var pgresData = new PostgresData();
            pgresData.InsertData(idsList);
            System.Console.ReadLine();
        }
    }
}