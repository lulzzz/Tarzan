using System;
using Xunit;
using Tarzan.Nfx.Model;
namespace Tarzan.Nfx.Model.Tests
{
    public class FlowKeyTest
    {
        [Fact]
        public void CompareTest()
        {
            var random = new Random();
            var bytes1 = new byte[40];
            random.NextBytes(bytes1);

            var bytes2 = new byte[40];
            random.NextBytes(bytes2);

            var bytes3 = new byte[40];
            bytes1.CopyTo(bytes3,0);

            var fk1 = new FlowKey(bytes1);
            var fk2 = new FlowKey(bytes2);
            var fk3 = new FlowKey(bytes3);

            Assert.True(FlowKey.Compare(fk1,fk1));     
            Assert.False(FlowKey.Compare(fk1,fk2));
            Assert.True(FlowKey.Compare(fk1,fk3));

        }
    }
}