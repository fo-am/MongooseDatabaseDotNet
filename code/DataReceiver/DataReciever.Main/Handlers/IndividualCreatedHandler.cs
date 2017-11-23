using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Npgsql;
using System.Linq;
using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;

namespace DataReciever.Main.Handlers
{
    internal class IndividualCreatedHandler : IHandle<IndividualCreated>
    {
        public static string DbFile => GetAppSettings.Get().PostgresConnection;

        public void HandleMessage(IndividualCreated message)
        {
            var data = new PgRepository();
            data.InsertNewIndividual(message);

            // insert pack
            // insert individual
            // link both
            // insert litter
            // link litter and pack
            // born event
        }
    }
}