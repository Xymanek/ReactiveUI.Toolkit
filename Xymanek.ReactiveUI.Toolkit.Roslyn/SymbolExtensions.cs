using System.Collections.Immutable;
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
}