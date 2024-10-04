using Basic.Reference.Assemblies;
using Cygna.CodeGen.Test;
using Cygna.MapLookupGen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cygna.MapLookupGen.Tests;

public class GeneratorTests
{
    [Fact]
    public void Generator()
    {
        var result = SyntaxTreeBuilder.GenerateCode<LookupMapGenerator>(Definitions.Valid, typeof(MapLookupAttribute));

        var allOutputs = result.TrackedOutputSteps.SelectMany(outputStep => outputStep.Value)
            .SelectMany(output => output.Outputs);

        Assert.Collection(allOutputs, output => Assert.Equal(IncrementalStepRunReason.New, output.Reason));
    }
}