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
- `UnicodeData.normalizationFlags[]` - per-codepoint `NormalizationFlags` used by the normalization quick check
- Case mappings use `CodepointMapTable`, a two-level table (256-codepoint pages, unallocated page = identity) that is far cheaper to probe than a dictionary
- Dictionary lookups only for the remaining sparse data (numeric values, composition/decomposition mappings)

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
- `DataHelper` - Resource loading and Unicode data-file parsing
- `NormalizationEngine` - Unicode normalization (ported from W3C reference)
- `HelperUtils` - Canonical sorting, text direction algorithm
- `UnicodeConversion` - String-to-enum conversions for Unicode data files

## Key Implementation Details

### Unicode Data File Format
Unicode Consortium data files are parsed by `DataHelper.ReadDataFile`: semicolon-separated fields (trimmed), `#` starts a comment, blank lines are skipped. The embedded `Resources.zip` is read with `System.IO.Compression.ZipArchive`; the library has no third-party parsing or compression dependencies.

### Normalization Implementation
`NormalizationEngine` is ported from the W3C reference implementation. Handles Hangul syllable decomposition/composition separately using algorithmic approach (constants `SBase`, `LBase`, `VBase`, `TBase`). Decomposition mappings are pre-expanded fully during initialization.

`Normalize` first runs a quick check over the `NormalizationFlags` table and returns the input instance unchanged when no codepoint could be affected by (or affect its neighbours under) the requested form, so already-normalized text costs one table read per codepoint and no allocation. The flags are derived from the finished tables in `UnicodeData.Loader.ComputeNormalizationFlags`; note the propagation of `ComposesAsSecond` to codepoints whose decomposition *begins* with a composing codepoint (e.g. U+16D68), without which `16D63 16D68` would wrongly be treated as already NFC. The quick check is conservative (it can only prove that a string *is* normalized). `UString.IsNormalized` (implemented by `NormalizationEngine.IsNormalized`) follows the detection algorithm of UAX #15: codepoints that pass the quick check are skipped, and only the segments containing a "maybe" codepoint are examined: a canonical-ordering scan for NFD/NFKD, and for NFC/NFKC the segment (typically one starter and its marks) is normalized into a pooled buffer and compared. It never normalizes the whole string and allocates no strings.

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

3. **Initialization time**: First access to `UnicodeData` takes ~80ms, `UnicodeNames` ~60ms, `UnicodeProperties` ~30ms, `UnicodeBlocks` ~15ms (less when another class has already been initialized). Consider calling `Init()` during app startup.

4. **Explicit cast required**: Converting `UCodepoint` to `int` or `char` requires explicit cast: `(int)uc`, `(char)uc`.

5. **Static constructor call overhead**: While a class's static constructor is running, every call into a method of that same class goes through a class-initialization check (~80ns per call). Data-loading code invoked from a static constructor must therefore never make a per-codepoint call into the class being initialized. `UnicodeData` and `UnicodeNames` do their loading in a private nested `Loader` instance class whose results the static constructor copies into the static fields; `UnicodeProperties.Load` keeps the per-codepoint work in a plain loop.

6. **Startup code runs unoptimized**: Tiered JIT compilation only promotes hot methods after a quiet period with no new methods being compiled, which never happens during initialization. The loaders' hot methods are therefore marked `[MethodImpl(HelperUtils.AggressiveOptimization)]` so they are fully optimized on first call (about 25% faster initialization), and `DataHelper.ReadDataFile` is a hand-written enumerator rather than a `yield` iterator so its `MoveNext` can carry the attribute too.
