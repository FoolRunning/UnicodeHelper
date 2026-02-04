# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Build Commands

```bash
dotnet restore                    # Restore dependencies
dotnet build --no-restore         # Build solution
dotnet test --no-build            # Run tests
dotnet test --verbosity normal    # Run tests with detailed output
```

The solution targets .NET Standard 2.0 (library) and .NET 8.0 (tests). Tests use MSTest framework.

## Architecture Overview

UnicodeHelper is a .NET library that provides Unicode property access and enhanced string handling for characters beyond the Basic Multilingual Plane (codepoints > U+FFFF). It supports Unicode 17.0.0.

### Core Types

**UCodepoint** (`UnicodeHelper/UCodepoint.cs`) - Readonly struct representing a single Unicode codepoint (0x0000-0x10FFFF). Primary API for querying Unicode properties. Implicitly converts from `char` for BMP characters.

**UString** (`UnicodeHelper/UString.cs`) - Sealed class representing a sequence of Unicode codepoints. Unlike .NET strings, treats upper-plane characters (emoji, etc.) as single units rather than surrogate pairs. Work-in-progress; not all methods fully implemented.

**UStringBuilder** (`UnicodeHelper/UStringBuilder.cs`) - Efficient builder for constructing UStrings. Uses ArrayPool for buffer management; must be disposed.

### Unicode Data Classes

**UnicodeData** (`UnicodeHelper/UnicodeData.cs`) - Core Unicode property data: categories, bidirectional classes, numeric values, case mappings, composition/decomposition mappings.

**UnicodeProperties** (`UnicodeHelper/UnicodeProperties.cs`) - Advanced Unicode properties from PropList.txt and DerivedCoreProperties.txt.

**UnicodeNames** (`UnicodeHelper/UnicodeNames.cs`) - Character names and aliases from DerivedName.txt and NameAliases.txt.

**UnicodeBlocks** (`UnicodeHelper/UnicodeBlocks.cs`) - Maps codepoints to Unicode blocks.

### Extension Methods

**DotNetStringExtensions** - Extends .NET strings with `Codepoints()` iterator and `DetermineDirection()` for text direction (implements Unicode TR9).

### Internal Components

Located in `UnicodeHelper/Internal/`:
- **DataHelper** - Reads embedded Unicode data from Resources.zip
- **NormalizationEngine** - Unicode normalization (NFC, NFD, NFKC, NFKD), includes Hangul handling
- **HelperUtils** - Text direction algorithm implementation

### Embedded Resources

All Unicode data files are embedded in `UnicodeHelper/Resources/Resources.zip` for offline use. Data is loaded lazily on first access via static constructors.

## Testing Approach

Every implemented method should be extensively tested. Tests compare behavior against .NET's built-in Unicode support where applicable. The `UnicodeHelper.Tests/TestData/NormalizationTest.txt` contains normalization test cases. Data-driven tests use `[DynamicData]` attributes.
