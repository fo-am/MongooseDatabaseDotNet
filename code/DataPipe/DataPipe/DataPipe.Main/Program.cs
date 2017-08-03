using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace DataPipe.Main
{
    class Program
    {
        static void Main(string[] args)
        {
           
            Console.WriteLine(GetCustomer().data);
            Console.ReadLine();
        }
        public static Model GetCustomer()
        {
         
            if (!File.Exists(Path.GetFullPath(Model.DbFile))) return null;

            using (var cnn = Model.SimpleDbConnection())
            {
                cnn.Open();
                Model result = cnn.Query<Model>(
                    @"SELECT id, data FROM test").FirstOrDefault();
                return result;
            }
        }
    }
}
