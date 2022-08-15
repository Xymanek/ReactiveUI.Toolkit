using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xymanek.ReactiveUI.Toolkit.Roslyn;

// *Inspired* by
// https://github.com/CommunityToolkit/dotnet/blob/96517eace88d95ec52a881e3e1ed74ea51436b40/CommunityToolkit.Mvvm.SourceGenerators/ComponentModel/ObservablePropertyGenerator.cs
[Generator]
public partial class ReactivePropertySourceGenerator : IIncrementalGenerator
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
                    .Select(v => (IFieldSymbol)ModelExtensions.GetDeclaredSymbol(context.SemanticModel, v)!)
            )
            .SelectMany(static (item, _) => item);

        // Filter the fields using [ReactiveProperty]
        IncrementalValuesProvider<IFieldSymbol> fieldSymbolsWithAttribute = fieldSymbols
            .Where(static item => item
                .HasAttributeWithFullyQualifiedName("global::Xymanek.ReactiveUI.Toolkit.ReactiveProperty")
            );

        // From this point I diverge from ObservablePropertyGenerator's logic, since this generator (currently)
        // is much simpler and I don't want to pull in a bunch of their utility classes.

        // Separate fields into those which are declared in reactive objects and those which are not
        IncrementalValuesProvider<(bool IsValid, IFieldSymbol Symbol)> fieldValidityPairs = fieldSymbolsWithAttribute
            .Select(static (symbol, _) => (IsReactiveObject(symbol.ContainingType), symbol));

        IncrementalValuesProvider<Diagnostic> wrongClassDiagnostics = fieldValidityPairs
            .Where(static tuple => !tuple.IsValid)
            .Select(static (tuple, _) => Diagnostic.Create(
                DiagnosticDescriptors.ReactivePropertyInNotReactiveClass,
                tuple.Symbol.Locations.FirstOrDefault(), // TODO: find the attribute location instead
                tuple.Symbol.ContainingType,
                tuple.Symbol.Name
            ));

        context.RegisterSourceOutput(wrongClassDiagnostics, ProduceDiagnostics);

        // Generate properties only for reactive objects.
        // Group the fields by their declaration types (since that's how we will create the generated code files)
        IncrementalValuesProvider<(INamedTypeSymbol Type, ImmutableArray<IFieldSymbol> Fields)> groupedFieldSymbols =
            fieldValidityPairs
                .Where(static tuple => tuple.IsValid)
                .Select(static (tuple, _) => (tuple.Symbol.ContainingType, tuple.Symbol))
                .Group();

        context.RegisterSourceOutput(groupedFieldSymbols, ProduceProperties);
    }

    private static void ProduceDiagnostics(SourceProductionContext context, Diagnostic diagnostic)
    {
        context.ReportDiagnostic(diagnostic);
    }

    private const string ReactiveObjectBaseTypeFullyQualified = "global::ReactiveUI.IReactiveObject";

    [Pure]
    private static bool IsReactiveObject(INamedTypeSymbol typeSymbol)
        => typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedName(ReactiveObjectBaseTypeFullyQualified));

    private static void ProduceProperties(
        SourceProductionContext context,
        (INamedTypeSymbol Type, ImmutableArray<IFieldSymbol> Fields) tuple
    )
    {
        CompilationUnitSyntax compilationUnit = CodeGenHelpers.BuildPartial(
            tuple.Type,
            tuple.Fields.Select(CreatePropertyFromField).ToImmutableArray()
        );

        string fileNameWithoutExtension = tuple.Type.GetFullMetadataNameForFileName();

        context.AddSource($"{fileNameWithoutExtension}.g.cs", compilationUnit.GetText(Encoding.UTF8));
    }

    // https://github.com/CommunityToolkit/dotnet/blob/96517eace88d95ec52a881e3e1ed74ea51436b40/CommunityToolkit.Mvvm.SourceGenerators/ComponentModel/ObservablePropertyGenerator.Execute.cs#L999-L1019
    private static string GetGeneratedPropertyName(IFieldSymbol fieldSymbol)
    {
        string propertyName = fieldSymbol.Name;

        if (propertyName.StartsWith("m_"))
        {
            propertyName = propertyName.Substring(2);
        }
        else if (propertyName.StartsWith("_"))
        {
            propertyName = propertyName.TrimStart('_');
        }

        return $"{char.ToUpper(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
    }
}