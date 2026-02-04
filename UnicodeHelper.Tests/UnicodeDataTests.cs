namespace UnicodeHelper
{
    [TestClass]
    public class UnicodeDataTests
    {
        [TestMethod]
        public void Version()
        {
            Assert.AreEqual(new Version(17, 0, 0), UnicodeData.UnicodeVersion);
        }
    }
}