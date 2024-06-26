﻿#nullable enable

using System;
using Xunit;
using Barotrauma;
using FluentAssertions;
using FsCheck;

namespace TestProject;

public sealed class GenericToolBoxTests
{
    public class CustomGenerators
    {
        public static Arbitrary<DifferentIdentifierPair> IdentifierPairGenerator()
        {
            return Arb.From(from Identifier first in Arb.Generate<Identifier>().Where(first => !first.Value.Contains('~'))
                            from Identifier second in Arb.Generate<Identifier>().Where(second => second != first && !second.Value.Contains('~'))
                            select new DifferentIdentifierPair(first, second));
        }
    }

    public readonly struct DifferentIdentifierPair
    {
        public readonly Identifier First,
                                   Second;

        public DifferentIdentifierPair(Identifier first, Identifier second)
        {
            if (first == second) { throw new InvalidOperationException("Identifiers must be different"); }
            //tildes have a special meaning in stat identifiers, don't use them
            if (first.Value.Contains('~')) { throw new InvalidOperationException($"{first} is not a valid identifier."); }
            if (second.Value.Contains('~')) { throw new InvalidOperationException($"{second} is not a valid identifier."); }

            First = first;
            Second = second;
        }
    }

    public GenericToolBoxTests()
    {
        Arb.Register<TestProject.CustomGenerators>();
        Arb.Register<CustomGenerators>();
    }

    [Fact]
    public void MatchesStatIdentifier()
    {
        Prop.ForAll<DifferentIdentifierPair>(static pair =>
        {
            ToolBox.StatIdentifierMatches(pair.First, $"{pair.First}~{pair.Second}".ToIdentifier()).Should().BeTrue();
            ToolBox.StatIdentifierMatches($"{pair.First}~{pair.Second}".ToIdentifier(), pair.First).Should().BeTrue();
            ToolBox.StatIdentifierMatches(pair.First, pair.First).Should().BeTrue();

            ToolBox.StatIdentifierMatches(pair.First, $"{pair.Second}~{pair.First}".ToIdentifier()).Should().BeFalse();
            ToolBox.StatIdentifierMatches(pair.First, pair.Second).Should().BeFalse();
        }).VerboseCheckThrowOnFailure();
    }
}
