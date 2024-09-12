using System.Runtime.CompilerServices;
using Cygna.CodeGen;
using Cygna.CodeGen.Generator;
using Cygna.CodeGen.Metadata;
using Cygna.CodeGen.Types;
using Cygna.MapLookupGen.Generators.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cygna.MapLookupGen.Generators;

/// <summary>
/// Main generator entry point
/// </summary>
[Generator(LanguageNames.CSharp)]
public class LookupMapGenerator : IncrementalSourceGenerator
{
    private static readonly string Logs = "build_property.MapLookupGen_SerializationInfoOutputDirectory";

    public LookupMapGenerator() : base(Logs)
    {

    }

    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        GenerateCodeAction(context, "Cygna.MapLookupGen.MapLookupAttribute", Generate);
    }

    /// <summary>
    /// Main source generator handler
    /// </summary>
    /// <param name="syntax"></param>
    /// <param name="compilation"></param>
    /// <param name="context"></param>
    /// <exception cref="GeneratorException"></exception>
    private static void Generate(TypeDeclarationSyntax syntax, Compilation compilation, IGeneratorContext context)
    {
        GetGenerateCodePreamble(syntax, compilation, context, out var typeSymbol);

        if (typeSymbol == null)
        {
            return;
        }

        Symbols symbols = new(compilation);
        BaseGenerator generator = new();
        var typeMeta = new TypeMetadata(typeSymbol);

        try
        {
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

            var result = generator.Generate(context, typeMeta, symbols, syntax, semanticModel);

            context.AddSource($"{typeSymbol.GetFullTypeName()}.MapLookupGen.g.cs", result);
        }
        catch (Exception e)
        {
            throw new GeneratorException(typeSymbol, "Source generator failed to run", e);
        }
    }

    private static void GetGenerateCodePreamble(TypeDeclarationSyntax syntax, Compilation compilation,
        IGeneratorContext context, out INamedTypeSymbol? typeSymbol)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
        typeSymbol = semanticModel.GetDeclaredSymbol(syntax);

        if (typeSymbol == null)
        {
            throw new GeneratorException("TypeSymbol is null");
        }

        var isPartial = syntax.IsPartial();

        if (!isPartial)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustBePartial,
                syntax.Identifier.GetLocation(), typeSymbol.Name));
        }
    }
}