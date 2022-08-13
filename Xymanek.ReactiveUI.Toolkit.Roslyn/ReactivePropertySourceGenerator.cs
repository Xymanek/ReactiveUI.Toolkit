using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xymanek.ReactiveUI.Toolkit.Roslyn;

// *Inspired* by
// https://github.com/CommunityToolkit/dotnet/blob/96517eace88d95ec52a881e3e1ed74ea51436b40/CommunityToolkit.Mvvm.SourceGenerators/ComponentModel/ObservablePropertyGenerator.cs
[Generator]
public class ReactivePropertySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all field declarations with at least one attribute
        IncrementalValuesProvider<IFieldSymbol> fieldSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is FieldDeclarationSyntax
                {
                    Parent: ClassDeclarationSyntax or RecordDeclarationSyntax,
                    AttributeLists.Count: > 0
                },
                static (context, _) => ((FieldDeclarationSyntax)context.Node)
                    .Declaration
                    .Variables
                    .Select(v => (IFieldSymbol)context.SemanticModel.GetDeclaredSymbol(v)!)
            )
            .SelectMany(static (item, _) => item);

        // Filter the fields using [ReactiveProperty]
        IncrementalValuesProvider<IFieldSymbol> fieldSymbolsWithAttribute = fieldSymbols
            .Where(static item => item
                .HasAttributeWithFullyQualifiedName("global::Xymanek.ReactiveUI.Toolkit.ReactiveProperty")
            );

        // From this point I diverge from ObservablePropertyGenerator's logic, since this generator (currently)
        // is much simpler and I don't want to pull in a bunch of their utility classes.

        // Group the fields by their declaration types (since that's how we will create the generated code files)
        IncrementalValuesProvider<(INamedTypeSymbol Type, ImmutableArray<IFieldSymbol> Fields)> groupedFieldSymbols =
            fieldSymbolsWithAttribute
                .Select(static (symbol, _) => (symbol.ContainingType, symbol))
                .Group();

        // Generate properties only for reactive objects. TODO: error when in wrong class
        IncrementalValuesProvider<(INamedTypeSymbol Type, ImmutableArray<IFieldSymbol> Fields)> validGroupedFieldSymbols
            = groupedFieldSymbols.Where(static tuple => IsReactiveObject(tuple.Type));
    }

    private const string ReactiveObjectBaseTypeFullyQualified = "global::ReactiveUI.IReactiveObject";

    [Pure]
    private static bool IsReactiveObject(INamedTypeSymbol typeSymbol)
    {
        for (
            INamedTypeSymbol? parent = typeSymbol;
            parent is not null;
            parent = parent.ContainingType
        )
        {
            if (parent.HasFullyQualifiedName(ReactiveObjectBaseTypeFullyQualified))
            {
                return true;
            }
        }

        return false;
    }
}