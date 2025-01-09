# UnicodeHelper
[![Test status](https://github.com/FoolRunning/UnicodeHelper/actions/workflows/dotnet.yml/badge.svg)](https://github.com/FoolRunning/UnicodeHelper/actions/workflows/dotnet.yml)

*Note: This is a work-in-progress. In particular, some methods on UString have not been implemented yet.* 

.Net library to get Unicode property information about codepoints and to better handle the 
upper planes (1-16) of Unicode where having to deal with surrogate pairs can get messy. 
Current version supoorts **Unicode 16.0.0**.

A goal for this library is to have *every* implemented method be extensively tested, so it should already be fairly stable.

Available as a [NuGet package](https://www.nuget.org/packages/UnicodeHelper).

## Main API
### UCodepoint
Represents an entire Unicode codepoint (32-bit value).

Also provides access to Unicode properties of individual codepoints via static methods. It can be implicitly casted from a .Net character, so most methods will accept a normal .Net ``char`` as well as a ``UCodepoint`` without any explicit casting:
```c#
char c = 'a';
UnicodeCategory cat = UCodepoint.GetUnicodeCategory(c); // No casting needed here
```

### UString (WIP)
Represents a sequence of Unicode codepoints.

The idea is to be able to replace a .Net ``string`` with a ``UString`` in as many cases as possible and as efficiently as possible.

### UnicodeProperties
Used to get advanced properties of a Unicode codepoint.

Represents the data in the Unicode specification [PropList.txt](https://www.unicode.org/reports/tr44/#PropList.txt) and [DerivedCoreProperties.txt](https://www.unicode.org/reports/tr44/#DerivedCoreProperties.txt).

### UnicodeNames
Used to get the defined names of a Unicode codepoint.

Represents the data in the Unicode specification [DerivedName.txt](https://www.unicode.org/Public/UCD/latest/ucd/extracted/DerivedName.txt) and [NameAliases.txt](https://www.unicode.org/reports/tr44/#NameAliases.txt).

### UnicodeBlocks
Used to get the block for which a Unicode codepoint belongs.

Represents the data in the Unicode specification [Blocks.txt](https://www.unicode.org/reports/tr44/#Blocks.txt).
