namespace psDataImporter.Contracts.Postgres
{
    public class Individual
    {
        public Individual(int pgIndivididualId, string name, int? litterId = null)
        {
            IndividualId = pgIndivididualId;
            Name = name;
            LitterId = litterId;
        }

        public int IndividualId { get; set; }
        public string Name { get; set; }
        public int? LitterId { get; set; }
    }
}