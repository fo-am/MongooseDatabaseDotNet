using System;

namespace psDataImporter.Contracts.Postgres
{
    public class PackHistory
    {
        public int PackId { get; set; }
        public int IndividualId { get; set; }
        public DateTime DateJoined { get; set; }
    }
}