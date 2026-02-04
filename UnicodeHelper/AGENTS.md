# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Module Purpose

This is the core library implementing Unicode property access and codepoint-aware string handling. It abstracts away .NET's UTF-16 surrogate pair complexity by treating all Unicode codepoints (including upper-plane characters like emoji) as single units.

## Architectural Decisions

### Data Storage Strategy
Unicode property data uses large arrays indexed directly by codepoint value (0-0x10FFFF = 1,114,112 entries). This trades memory for O(1) lookup performance:
- `UnicodeData.categories[]` - byte array for UnicodeCategory
- `UnicodeData.bidiClasses[]` - enum array for bidirectional class
- `UnicodeProperties.props[]` - UnicodeProperty flags array
- Dictionary lookups only for sparse data (numeric values, case mappings, decomposition mappings)

### Lazy Initialization Pattern
All static data classes (`UnicodeData`, `UnicodeProperties`, `UnicodeNames`, `UnicodeBlocks`) use static constructors that load from embedded resources on first access. Each provides an empty `Init()` method to allow explicit initialization timing (e.g., during splash screen).

### Substring Sharing
`UString` instances share their backing `UCodepoint[]` array. Substrings store `_startIndex` and `Length` offsets into the parent array rather than copying. This makes substring operations O(1) but means the parent array stays in memory as long as any substring exists.

### Memory Pooling
`UStringBuilder` uses `ArrayPool<UCodepoint>.Shared` for buffer management. **Must be disposed** to return buffers to the pool. The finalizer also returns the buffer, but relying on it is inefficient.

## Design Patterns

### Facade Pattern
`UCodepoint` acts as a facade for Unicode property lookups. Static methods like `GetUnicodeCategory()`, `IsWhiteSpace()`, `ToUpper()` delegate to `UnicodeData` or `UnicodeProperties` internally.

### Value Type Wrapper
`UCodepoint` is a readonly struct wrapping a single `int _value`. Provides implicit conversion from `char` (for BMP characters) and explicit conversions to/from `int`. Operator overloads allow direct comparison with integers and chars.

### Internal Namespace Separation
`UnicodeHelper.Internal` contains implementation helpers not part of the public API:
- `DataHelper` - Resource loading and CSV parsing configuration
- `NormalizationEngine` - Unicode normalization (ported from W3C reference)
- `HelperUtils` - Canonical sorting, text direction algorithm
- `UnicodeConversion` - String-to-enum conversions for Unicode data files

## Key Implementation Details

### Unicode Data File Format
Uses CsvHelper with semicolon delimiter (`Delimiter = ";"`) to parse Unicode Consortium data files. Comment character is `#`. Configuration in `DataHelper.CsvConfiguration`.

### Normalization Implementation
`NormalizationEngine` is ported from the W3C reference implementation. Handles Hangul syllable decomposition/composition separately using algorithmic approach (constants `SBase`, `LBase`, `VBase`, `TBase`). Decomposition mappings are pre-expanded fully during initialization.

### Bidi Default Values
`UnicodeData.Init()` sets default bidirectional classes by range before loading actual data. Ranges follow DerivedBidiClass.txt specification (e.g., 0x0590-0x05FF defaults to RightToLeft).

### Combining Key Encoding
Composition/decomposition lookups use packed keys:
- Decomposition: `(compatMapping << 21) | codepoint` as `int`
- Composition: `(compatMapping << 42) | (base << 21) | combining` as `long`

## Work-in-Progress Methods

These `UString` methods throw `NotImplementedException`:
- `IndexOf(UString, ...)` - substring search
- `LastIndexOf(UString, ...)`
- `StartsWith(UString, ...)`
- `EndsWith(UString, ...)`
- `Contains(...)`

## Gotchas

1. **UStringBuilder disposal**: Failure to dispose leaks pooled arrays. Always use `using` statement.

2. **CharLength vs Length**: `UString.Length` counts codepoints; `CharLength` counts UTF-16 chars (different for upper-plane characters).

3. **Initialization time**: First access to `UnicodeData` takes ~300ms, `UnicodeProperties` ~150ms. Consider calling `Init()` during app startup.

4. **Explicit cast required**: Converting `UCodepoint` to `int` or `char` requires explicit cast: `(int)uc`, `(char)uc`.
