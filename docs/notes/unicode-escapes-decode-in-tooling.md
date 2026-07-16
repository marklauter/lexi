---
title: Literal \uXXXX in test data gets decoded by tooling
summary: Writing the literal six characters backslash-u-0041 into CharacterLiteral's InlineData kept arriving on disk as the single character 'A'. The \u adjacency is unicode-unescaped somewhere between the edit tool and the file. Worked around by synthesizing the backslash from chr(92) in a Python writer; committed bytes verified with od -c.
tags: [note, lexi, tests, tooling, gotcha]
created: 2026-07-16
priority: low
effort: low
status: closed
---

# Literal \uXXXX in test data gets decoded by tooling

While implementing the CharacterLiteral escape grammar (commit `38fc237`), the
unicode-escape test cases needed the *literal* six-character sequence
`backslash u 0 0 4 1` inside `[InlineData(@"'\u0041'")]` -- the regex is what
decodes it, so the test string must carry the raw escape, not the decoded
character.

Every attempt to write that adjacency landed on disk as the single character
`A` (0x41) instead. The same happened for `ꯍ` → the U+ABCD character. The
`\u` + four-hex-digit adjacency was being unicode-unescaped somewhere between
the tool call and the file. Tellingly, the *malformed* cases in the same test
survived untouched (`\u041`, three digits; `\u00G1`, a non-hex digit) — they
aren't valid `\uXXXX` escapes, so nothing decoded them.

## Workaround

Build the backslash without ever typing the `\u` adjacency, then write the
file directly:

```python
bs = chr(92)              # backslash, never written as an escape
u  = "'" + bs + "u0041'"  # yields '\u0041'
```

The committed file has the correct raw bytes — verified with `od -c`:

```
@ " ' \ u 0 0 4 1 ' " ) ]
```

## Watch for

If those `InlineData` lines are ever regenerated through an editor or tool
that eagerly decodes `\u`, `CharacterLiteral_MatchesEscapeSequences` will
silently start asserting on decoded characters (which match the `[^'\\]`
branch) instead of the escape branch it is meant to cover — the test stays
green while no longer testing `\uXXXX`. Re-verify the raw bytes after any bulk
edit of that file.
