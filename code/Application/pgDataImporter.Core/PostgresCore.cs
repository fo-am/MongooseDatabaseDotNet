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
            weights = weights as IList<Weights> ?? weights.ToList();
            var pg = new PostgresRepository();


            pg.AddPacks(weights.Select(s => s.Group).Distinct());
            pg.AddIndividuals(weights.GroupBy(s => new {Name = s.Indiv, s.Sex})
                .Select(i => new Individual {Name = i.Key.Name, Sex = i.Key.Sex}));

            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();
            AddPackHistories(weights.Select(ph => new PackHistoryDto(ph.Indiv, ph.Group, ph.TimeMeasured)), pgPacks,
                pgIndividuals, pg);

            pg.AddWeights(weights, pgIndividuals);
        }

        public void ProccessUltrasoundData(IEnumerable<Ultrasound> ultrasoundData)
        {
            ultrasoundData = ultrasoundData as IList<Ultrasound> ?? ultrasoundData.ToList();
            var pg = new PostgresRepository();
            pg.AddPacks(ultrasoundData.Select(s => s.PACK).Distinct());
            pg.AddIndividuals(ultrasoundData.Select(s => new Individual {Name = s.INDIV}).Distinct());

            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();
            AddPackHistories(ultrasoundData.Select(ph => new PackHistoryDto(ph.INDIV, ph.PACK, ph.DATE)), pgPacks,
                pgIndividuals, pg);
        }

        private void AddPackHistories(IEnumerable<PackHistoryDto> packHistorys, IEnumerable<Pack> pgPacks,
            IEnumerable<Individual> pgIndividuals, PostgresRepository pg)
        {
            //select and see if we have an entry

            foreach (var membership in packHistorys.OrderByDescending(ph => ph.DateJoined))
            {
                var packId = pgPacks.Single(p => p.Name == membership.PackName).PackId;
                var individualId = pgIndividuals.Single(i => i.Name == membership.IndividualName).IndividualId;

                var databasePackHistory = pg.GetPackHistory(packId, individualId);

                if (databasePackHistory != null)
                {
                    if (databasePackHistory.DateJoined > membership.DateJoined)
                    {
                        pg.UpdatePackHistory(membership, databasePackHistory);
                    }
                }
                else
                {
                    // if not insert new info
                    pg.InsertPackHistory(packId, individualId, membership);
                }
            }
        }
    }
}