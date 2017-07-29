using Xunit;

namespace CatchBlock
{
    public class Test
    {
        [Fact]
        public void TestMethod()
        {
            Assert.Equal(new C().M(), 2);
        }
    }
}
