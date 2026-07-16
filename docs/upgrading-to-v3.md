# Upgrading MSL.Lexi v2 → v3

v3.0.0 is a breaking release. The headline win is the **span-first redesign**:
a `Symbol` is now a plain, collectible value — the matched text lives in
`Source`, not in the token — so you can stream tokens into `List<Symbol>`,
`IEnumerable<Symbol>`, fields, and across `await`, which the old `ref struct`
`Symbol` made impossible. This guide lists every break and the mechanical fix.

If you only do one thing: **retarget to net10.0** (§1) and **replace implicit
`Source`⇄`string` conversions** (§4). Those two cover most call sites.

---

## 1. Target framework: net6/7/8 → net10.0

v2 multi-targeted `net6.0;net7.0;net8.0`; v3 targets `net10.0` only. Every
consumer must retarget to `net10.0` to resolve the package at all. This is the
change that forces the major bump.

```xml
<TargetFramework>net10.0</TargetFramework>
```

---

## 2. `Symbol` is now a `readonly record struct` (was `readonly ref struct`)

This is the core of the redesign, and mostly a gain:

- **Tokens are now collectible.** `List<Symbol>`, `IEnumerable<Symbol>`,
  storing a `Symbol` in a field, returning one across `await` — all legal now.
  Under v2 the `ref struct` forbade every one of these.
- **Value equality for free** (record struct): two `Symbol`s with the same
  `Offset`/`Length`/`TokenId` compare equal.
- `Offset`, `Length`, `TokenId` are now **properties** (`get`) rather than
  public readonly fields. Reads are source-compatible; you must recompile
  (binary break).

`Symbol` still holds no text — only `Offset`, `Length`, `TokenId`. See §3.

## 3. Token text comes from `Source`, never from `Symbol`

The span lives only in `Source`. To get a token's text, read it from the
`Source` that produced it:

```csharp
ReadOnlySpan<char> text = result.Source.ReadSymbol(in result.Symbol);
```

`MatchResult` is still a `ref struct` (it carries a `Source`), so a
`MatchResult` cannot be stored — but the `Symbol` you pull out of it can.
The streaming pattern is: keep the `Symbol`s, keep one `Source` (or the
original string) to resolve their text on demand.

## 4. `Source` implicit operators removed

The implicit `Source`⇄`string` conversions are gone:

```csharp
// v2 — implicit conversions
Source src = "1 + 2";          // string  -> Source
string s   = src;              // Source  -> string

// v3 — explicit
Source src = new("1 + 2");     // or Source.FromString("1 + 2")
string s   = src.ToString();
```

`lexer.NextMatch("…")` **still works** — v3 removes the implicit operator but
adds an explicit `NextMatch(string)` overload to replace it, so the string
entry point is unchanged. Only sites that relied on the conversion *elsewhere*
(assigning a `Source` to a `string`, or a bare `string` where a `Source` was
expected) need the explicit form above.

## 5. `Source.ReadSymbol` returns `ReadOnlySpan<char>` (was `string`)

No substring is allocated now. If you need a `string`, materialize it:

```csharp
// v2
string token = source.ReadSymbol(in symbol);
// v3
string token = source.ReadSymbol(in symbol).ToString();
```

Its diagnostic text for a failed match also changed — see §8.

## 6. `Source.ToString()` returns the source text (was `"Lexi.Source"`)

v2 inherited the default `object.ToString()`. v3 returns the underlying text.
If you logged, hashed, or asserted on `Source.ToString()`, the output is now
the source text.

## 7. Sealing / static changes

- **`CommonPatterns` is now `static`** (was `public partial class`).
  `new CommonPatterns()` and subclassing no longer compile. Calling the
  patterns — `CommonPatterns.Identifier()`, `CommonPatterns.Whitespace()`, … —
  is unchanged (they were already static).
- **`VocabularyBuilder` is now `sealed`.** Subclassing no longer compiles;
  normal `Create()`/`Match()`/`Ignore()`/`Build()` use is unaffected.

## 8. Behavioral & grammar changes (same API, different output)

These don't change signatures, but they change what the lexer returns for some
inputs. Several are bug fixes shipped in this release.

- **Trailing ignorable content now returns `EndOfSource`**, not a spurious
  `NoMatch` error token. A stream that ends in whitespace/comments now
  terminates cleanly on `Symbol.IsEndOfSource`.
- **Interleaved runs of different ignore patterns are fully skipped.** v2 made
  a single pass over the ignore list, so e.g. whitespace → newline → whitespace
  could strand the offset; v3 loops until nothing advances.
- **A `NoMatch` symbol now spans the single offending character** (`Length 1`,
  was `Length 0`). You can slice it for the raw text, and `ReadSymbol` now
  names it: `"lexer error at offset 14: unexpected '@'"` (was
  `"lexer error at offset: 14"`). The offset still does not advance — recovery
  remains the caller's choice.
- **`ScientificNotationLiteral` accepts a signed positive exponent** (`1e+5`,
  `-1.5E+10`), which v2 rejected.
- **`CharacterLiteral` supports escape sequences** — `\b \t \n \r \f \' \" \\`
  and `\uXXXX`. As a consequence a **lone backslash no longer matches**: v2's
  `'\'` (backslash as a raw character) is now invalid; write `'\\'`.

## 9. Packaging

v3 ships a **`.snupkg` symbols package** alongside the main package for the
first time, enabling symbol/source debugging into Lexi. Additive — no action
required.

---

## Not a break (contributor-facing only)

The repository moved `Lexi/` → `src/Lexi/`, `Lexi.Tests/` → `tests/Lexi.Tests/`,
and reorganized the samples. The public namespace is still `Lexi`, so consumers
see no difference; only anyone building from source is affected.

## Quick reference

| v2 | v3 |
| --- | --- |
| `net6.0;net7.0;net8.0` | `net10.0` |
| `Symbol` is a `ref struct` (not collectible) | `readonly record struct` (collectible, value equality) |
| `Source src = "text";` | `new Source("text")` / `Source.FromString("text")` |
| `string s = source;` | `source.ToString()` |
| `string t = source.ReadSymbol(in sym);` | `string t = source.ReadSymbol(in sym).ToString();` |
| `Source.ToString()` → `"Lexi.Source"` | → the source text |
| `new CommonPatterns()` / subclass | not allowed (`static`) — call members directly |
| subclass `VocabularyBuilder` | not allowed (`sealed`) |
| `'\'` matches a backslash char | invalid — use `'\\'` |
