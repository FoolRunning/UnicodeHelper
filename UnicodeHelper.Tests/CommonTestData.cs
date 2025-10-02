namespace UnicodeHelper
{
    internal static class CommonTestData
    {
        public static IEnumerable<object?[]> DetermineDirectionTestData =>
        [
            [null, TextDirection.Undefined],
            ["", TextDirection.Undefined],
            ["Left to right", TextDirection.LtR],
            ["العربية", TextDirection.RtL],
            ["😁🤔😮", TextDirection.Undefined],
            ["😁\u2066Ignore Me", TextDirection.Undefined],
            ["😁\u2067Ignore Me", TextDirection.Undefined],
            ["😁\u2066Ignore Me\u2069العربية", TextDirection.RtL],
            ["\u2067العربية\u2069This is the direction", TextDirection.LtR],
            [".,$", TextDirection.Undefined],
        ];
    }
}
