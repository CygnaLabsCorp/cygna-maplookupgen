using System.CodeDom.Compiler;
using System.Text;
using Cygna.CodeGen.Generator;
using Cygna.CodeGen.Metadata;
using Cygna.CodeGen.Writer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SymbolDisplayFormat = Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace Cygna.MapLookupGen.Generators.Generator;

public class BaseGenerator
{
    public string Generate(IGeneratorContext context, TypeMetadata typeMetadata, Symbols symbols,
        TypeDeclarationSyntax syntax, SemanticModel semanticModel)
    {
        var buffer = new StringWriter(new StringBuilder(capacity: 4096));
        var writer = new IndentedTextWriter(buffer);

        writer.WriteLine(Structure.NullableEnable);
        
        writer.WriteUsings(new HashSet<string>()
        {
            "System",
            "System.Data",
            "System.Collections.Immutable",
            "System.Collections.Generic",
            "System.IO.Hashing"
        }, typeMetadata);

        writer.WriteNamespaceSymbol(context, typeMetadata);

        writer.WriteLine();
        var typeName = typeMetadata.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        writer.WriteLine($"partial class {typeName}");
        
        writer.AppendOpenBracket();
        
        //typeMetadata.Symbol.get
        writer.WriteLine("public static Dictionary<ulong, string> MapLookup = new()");
        writer.AppendOpenBracket();
        
        foreach (var member in typeMetadata.Symbol.GetAllMembers())
        {
            if (member is IFieldSymbol s)
            {        
                writer.WriteLine($"{{ XxHash64.HashToUInt64(\"{s.ConstantValue}\"u8), \"{s.ConstantValue}\" }},");
            }
        }
        
        writer.AppendCloseBracket();
        writer.Write(';');
        writer.AppendCloseBracket();
        
        
        return buffer.ToString();

    }
    
}