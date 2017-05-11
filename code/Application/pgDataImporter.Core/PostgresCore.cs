using System.Collections.Generic;
using System.Linq;
using psDataImporter.Contracts.Access;
using psDataImporter.Contracts.dtos;
using psDataImporter.Contracts.Postgres;
using psDataImporter.Data;

namespace pgDataImporter.Core
{
    public class PostgresCore
    {
        public void ProcessWeights(IEnumerable<Weights> weights)
        {
            var pg = new PostgresRepository();

             weights = weights as IList<Weights> ?? weights.ToList();
            pg.AddGroups(weights.Select(s => s.Group).Distinct());
            pg.AddIndividuals(weights.Select(s => new Individual{ Name = s.Indiv, Sex = s.Sex }).Distinct());

            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();
            pg.AddPackHistory(weights.Select(ph=>new PackHistoryDto( ph.Group, ph.Indiv, ph.TimeMeasured)), pgPacks, pgIndividuals);
            pg.AddWeights(weights, pgIndividuals);
        }
    }
}