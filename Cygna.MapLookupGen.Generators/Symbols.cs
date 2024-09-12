using Cygna.CodeGen;
using Microsoft.CodeAnalysis;

namespace Cygna.MapLookupGen.Generators;

public class Symbols : BaseSymbols
{
    private const string MapLookupAttribute = "Cygna.MapLookupGen.MapLookupAttribute";
    
    public INamedTypeSymbol MapLookupAttributeSymbol { get; }

    
    public Symbols(Compilation compilation) : base(compilation)
    {
        MapLookupAttributeSymbol = GetTypeByMetadataName(MapLookupAttribute);

    }
}