using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Extensions;
using ArchUnitNET.Loader;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchitectureModel = ArchUnitNET.Domain.Architecture;

namespace Lexi.Tests.Architecture;

// Encodes the design invariants from the writing-csharp guidance so drift trips the build, not code review.
public sealed class ArchitectureTests
{
    private static readonly ArchitectureModel LexiArchitecture = new ArchLoader()
        .LoadAssemblies(typeof(Lexer).Assembly)
        .Build();

    [Fact]
    public void AllTypesResideInLexiNamespaceTree() =>
        Verify(Types()
            .That()
            .DoNotHaveNameContaining("<") // exclude compiler-generated closures / async state machines
            .Should()
            .ResideInNamespaceMatching(@"^Lexi(\..*)?$")
            .Because("New top-level namespaces outside the Lexi tree require explicit design review."));

    [Fact]
    public void ConcreteClassesAreSealed() =>
        Verify(Classes()
            .That()
            .AreNotAbstract() // C# 'static' compiles to 'abstract sealed' — this also excludes static helpers
            .And()
            .DoNotHaveNameContaining("<")
            .Should()
            .BeSealed()
            .Because("writing-csharp: seal records and classes by default (enables devirtualization)."));

    // Deliberate deviation from pool/dynamodblite, which assert instance fields are NOT public.
    // Lexi's public surface is span-first value types — Source and MatchResult are ref structs, Symbol is a
    // plain readonly record struct — and MatchResult exposes readonly instance fields by design (a CA1051
    // suppression says so) to keep the lexer's hot path allocation-free. The invariant those repos are
    // protecting is "no public MUTABLE state", which Lexi honours: every public instance field is readonly.
    // That is what this asserts.
    [Fact]
    public void InstanceFieldsAreReadOnly() =>
        Verify(FieldMembers()
            .That()
            .AreNotStatic() // const / static fields are not instance state
            .And()
            .DoNotHaveNameContaining("<") // exclude compiler-generated backing fields
            .And()
            .DoNotHaveName("value__") // exclude the implicit instance field every C# enum compiles to
            .Should()
            .BeReadOnly()
            .Because("writing-csharp: immutable by default; a settable field is mutable shared state."));

    [Fact]
    public void LexiDoesNotDependOnAspNetCore() =>
        Verify(Types()
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^Microsoft\.AspNetCore.*")
            .Because("Lexi is a host-free, in-process library; pulling in ASP.NET Core would defeat its purpose."));

    [Fact]
    public void LexiDoesNotDependOnHosting() =>
        Verify(Types()
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^Microsoft\.Extensions\.Hosting.*")
            .Because("Lexi targets host-free .NET; the consumer owns the host, not Lexi."));

    [Fact]
    public void LexiDoesNotDependOnConsole() =>
        Verify(Types()
            .Should()
            // HaveFullName is used instead of the typed overload — that one requires the type to be loaded
            // into the architecture, but only Lexi.dll is loaded. The name predicate matches against
            // dependency targets recorded by the loader without needing the BCL assembly.
            .NotDependOnAnyTypesThat()
            .HaveFullName("System.Console")
            .Because("A lexer is a pure transform; direct Console writes leak into hosts that suppress stdout."));

    private static void Verify(IArchRule rule)
    {
        if (!rule.HasNoViolations(LexiArchitecture))
        {
            Assert.Fail(rule.Evaluate(LexiArchitecture).ToErrorMessage());
        }
    }
}
