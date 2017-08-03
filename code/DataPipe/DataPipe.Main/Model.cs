
using System;
using System.Data.SQLite;
namespace DataPipe.Main
{
    public class Model
{
    public int Id { get; set; }
    public string data { get; set; }


    public static string DbFile
    {
        get { return "~\\..\\..\\..\\..\\..\\TestDatabase.db"; }
    }

    public static SQLiteConnection SimpleDbConnection()
    {
        return new SQLiteConnection("Data Source=" + DbFile);

    }
    }
}
