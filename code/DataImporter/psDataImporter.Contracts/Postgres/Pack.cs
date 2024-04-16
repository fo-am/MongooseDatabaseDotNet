using System;

namespace pgDataImporter.Contracts.Postgres
{
    public class Pack
    {
        public Pack()
        {
        }

        public Pack(int newid, string name)
        {
            PackId = newid;
            Name = name;
        }

        public int PackId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}