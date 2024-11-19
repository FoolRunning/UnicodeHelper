namespace UnicodeHelper
{
    public static class AssertExtensions
    {
        public static void ThrowsException(this Assert assert, Type exceptionType, Action action)
        {
            try
            {
                action();
                Assert.Fail($"Did not get any exception. Expected exception of type {exceptionType}.");
            }
            catch (Exception ex)
            {
                if (ex.GetType() != exceptionType)
                    throw;
            }
        }

        public static void SequenceEqual<T>(this Assert assert, IEnumerable<T> e1, IEnumerable<T> e2)
        {
            IReadOnlyList<T> e1List;
            if (e1 is IReadOnlyList<T> el1)
                e1List = el1;
            else
                e1List = e1.ToList();

            IReadOnlyList<T> e2List;
            if (e1 is IReadOnlyList<T> el2)
                e2List = el2;
            else
                e2List = e2.ToList();

            int end = Math.Min(e1List.Count, e2List.Count);
            for (int i = 0; i < end; i++)
                Assert.AreEqual(e1List[i], e2List[i], "Sequence differs at index " + i);

            Assert.AreEqual(e1List.Count, e2List.Count, "Sequences have different number of items");
        }
    }
}
