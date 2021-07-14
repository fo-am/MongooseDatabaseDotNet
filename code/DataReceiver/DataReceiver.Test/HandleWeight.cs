using System;
using NLog;
using NUnit.Framework;
using DataReceiver.Main;
using DataReceiver.Main.Data;
using DataReceiver.Main.Handlers;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;
using FakeItEasy;
using System.Data;
using Dapper;

namespace DataReceiver.Test
{
    [TestFixture]
    public class HandleWeight
    {
        [Test]
        public void ThingDoesAThing()
        {
            var db = A.Fake<IPgRepository>();

            var test = new WeightHandler(db);

            var weight = new WeightMeasure
            {
                Accurate = 0,
                IndividualName = "foo",
                PackId = "34"
            };

            test.HandleMessage(weight);

            A.CallTo(() => db.InsertNewWeight(weight)).MustHaveHappened();
        }

        [Test]
        public void Weight2()
        {
            var log = A.Fake<ILogger>();
            var manager = A.Fake<IConnectionManager>();
            var con = A.Fake<IDbConnection>();

            A.CallTo(() => manager.GetConn()).Returns(con);

            A.CallTo(() => con.ExecuteScalar<int?>(A<string>.Ignored, A<object>.Ignored, null, null, null)).Returns(2);

            var db = new PgRepository(log, manager);
            var weight = new WeightMeasure
            {
                Accurate = 0,
                IndividualName = "foo",
                PackId = "34"
            };
            db.InsertNewWeight(weight);

        //    A.CallTo(() => db.InsertNewWeight(weight)).MustHaveHappened();
        }
    }
    
}
