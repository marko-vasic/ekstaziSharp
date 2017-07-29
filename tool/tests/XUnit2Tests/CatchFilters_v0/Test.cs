using System;
using Xunit;

namespace CatchFilters
{
    public class Test
    {
        [Fact]
        public void TestMethod()
        {
            try
            {
                new C().M();
            }
            catch (Exception e) { }
        }
    }
}
