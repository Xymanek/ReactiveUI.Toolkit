using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xymanek.ReactiveUI.Toolkit.Roslyn;

public partial class ReactivePropertySourceGenerator
{
    private static MemberDeclarationSyntax CreatePropertyFromField(IFieldSymbol sourceField)
    {
        TypeSyntax propertyType = IdentifierName(sourceField.Type.GetFullyQualifiedNameWithNullabilityAnnotations());
        SyntaxToken propertyIdentifier = Identifier(GetGeneratedPropertyName(sourceField));
        
        ExpressionSyntax fieldExpression = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            ThisExpression(),
            IdentifierName(sourceField.Name)
        );

        AccessorDeclarationSyntax getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithExpressionBody(ArrowExpressionClause(fieldExpression))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        // Note: no need to pass the property name manually - we are still in the setter,
        // even if calling not via the extension syntax
        InvocationExpressionSyntax setAndRaiseInvocation = InvocationExpression(MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("global::ReactiveUI.IReactiveObjectExtensions"),
                IdentifierName("RaiseAndSetIfChanged")
            ))
            .AddArgumentListArguments(Argument(ThisExpression()))
            .AddArgumentListArguments(Argument(null, Token(SyntaxKind.RefKeyword), fieldExpression))
            .AddArgumentListArguments(Argument(IdentifierName("value")));

        AccessorDeclarationSyntax setter = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            .WithExpressionBody(ArrowExpressionClause(setAndRaiseInvocation))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        return PropertyDeclaration(propertyType, propertyIdentifier)
            .AddGeneratedByAttributes(typeof(ReactivePropertySourceGenerator))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(getter, setter)
            .WithLeadingTrivia(TriviaList(
                Comment($"/// <inheritdoc cref=\"{sourceField.Name}\"/>")
            ));
    }
}