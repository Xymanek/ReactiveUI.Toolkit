using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xymanek.ReactiveUI.Toolkit.Roslyn;

public static class SymbolExtensions
{
    // https://github.com/CommunityToolkit/dotnet/blob/4b1adebb7674ded31320914387032083471e67ad/CommunityToolkit.Mvvm.SourceGenerators/Extensions/ISymbolExtensions.cs#L35-L44
    /// <summary>
    /// Checks whether or not a given type symbol has a specified full name.
    /// </summary>
    /// <param name="symbol">The input <see cref="ISymbol"/> instance to check.</param>
    /// <param name="name">The full name to check.</param>
    /// <returns>Whether <paramref name="symbol"/> has a full name equals to <paramref name="name"/>.</returns>
    public static bool HasFullyQualifiedName(this ISymbol symbol, string name)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == name;
    }

    // https://github.com/CommunityToolkit/dotnet/blob/4b1adebb7674ded31320914387032083471e67ad/CommunityToolkit.Mvvm.SourceGenerators/Extensions/ISymbolExtensions.cs#L46-L65
    /// <summary>
    /// Checks whether or not a given symbol has an attribute with the specified full name.
    /// </summary>
    /// <param name="symbol">The input <see cref="ISymbol"/> instance to check.</param>
    /// <param name="name">The attribute name to look for.</param>
    /// <returns>Whether or not <paramref name="symbol"/> has an attribute with the specified name.</returns>
    public static bool HasAttributeWithFullyQualifiedName(this ISymbol symbol, string name)
    {
        ImmutableArray<AttributeData> attributes = symbol.GetAttributes();

        foreach (AttributeData attribute in attributes)
        {
            if (attribute.AttributeClass?.HasFullyQualifiedName(name) == true)
            {
                return true;
            }
        }

        return false;
    }

    // https://github.com/CommunityToolkit/dotnet/blob/4b1adebb7674ded31320914387032083471e67ad/CommunityToolkit.Mvvm.SourceGenerators/Extensions/ISymbolExtensions.cs#L25-L33
    /// <summary>
    /// Gets the fully qualified name for a given symbol, including nullability annotations
    /// </summary>
    /// <param name="symbol">The input <see cref="ISymbol"/> instance.</param>
    /// <returns>The fully qualified name for <paramref name="symbol"/>.</returns>
    public static string GetFullyQualifiedNameWithNullabilityAnnotations(this ISymbol symbol)
    {
        return symbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
                .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier)
        );
    }
    
    // https://github.com/CommunityToolkit/dotnet/blob/157436d2592d3487041aaea066c6bc161e95de26/CommunityToolkit.Mvvm.SourceGenerators/Extensions/INamedTypeSymbolExtensions.cs#L16-L44
    /// <summary>
    /// Gets a valid filename for a given <see cref="INamedTypeSymbol"/> instance.
    /// </summary>
    /// <param name="symbol">The input <see cref="INamedTypeSymbol"/> instance.</param>
    /// <returns>The full metadata name for <paramref name="symbol"/> that is also a valid filename.</returns>
    public static string GetFullMetadataNameForFileName(this INamedTypeSymbol symbol)
    {
        static StringBuilder BuildFrom(ISymbol? symbol, StringBuilder builder)
        {
            return symbol switch
            {
                INamespaceSymbol ns when ns.IsGlobalNamespace => builder,
                INamespaceSymbol ns when ns.ContainingNamespace is { IsGlobalNamespace: false }
                    => BuildFrom(ns.ContainingNamespace, builder.Insert(0, $".{ns.MetadataName}")),
                ITypeSymbol ts when ts.ContainingType is ISymbol pt
                    => BuildFrom(pt, builder.Insert(0, $"+{ts.MetadataName}")),
                ITypeSymbol ts when ts.ContainingNamespace is ISymbol pn and not INamespaceSymbol { IsGlobalNamespace: true }
                    => BuildFrom(pn, builder.Insert(0, $".{ts.MetadataName}")),
                ISymbol => BuildFrom(symbol.ContainingSymbol, builder.Insert(0, symbol.MetadataName)),
                _ => builder
            };
        }

        // Build the full metadata name by concatenating the metadata names of all symbols from the input
        // one to the outermost namespace, if any. Additionally, the ` and + symbols need to be replaced
        // to avoid errors when generating code. This is a known issue with source generators not accepting
        // those characters at the moment, see: https://github.com/dotnet/roslyn/issues/58476.
        return BuildFrom(symbol, new StringBuilder(256)).ToString().Replace('`', '-').Replace('+', '.');
    }
}