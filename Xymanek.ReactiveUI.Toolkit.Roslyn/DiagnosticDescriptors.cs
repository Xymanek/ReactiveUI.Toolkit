using Microsoft.CodeAnalysis;

namespace Xymanek.ReactiveUI.Toolkit.Roslyn;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor ReactivePropertyInNotReactiveClass = new(
        id: "XRUIT0001",
        title: "ReactiveProperty used in a non-reactive class",
        messageFormat: "The field {0}.{1} is located in a class that does not implement ReactiveUI.IReactiveObject, so [ReactiveProperty] cannot be used",
        category: typeof(ReactivePropertySourceGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[ReactiveProperty] can only be used on fields inside classes that implement ReactiveUI.IReactiveObject."
    );
}