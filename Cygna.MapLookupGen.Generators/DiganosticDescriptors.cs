using Microsoft.CodeAnalysis;

namespace Cygna.MapLookupGen.Generators;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "MapLookup00001",
        title: "MapLookup object must be partial",
        messageFormat: "The MapLookup object '{0}' must be partial",
        category: "GenerateMapLookup",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}