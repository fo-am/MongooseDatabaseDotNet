using System;
using DataInput.Core;
using Xunit;

namespace DataInput.Test
{
    public class DataTests
    {
        [Fact]
        public void Test1()
        {
            var data = new GetSqliteData();
            Assert.NotNull(data);
        }
    }
}
