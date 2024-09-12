using Basic.Reference.Assemblies;
using Cygna.MapLookupGen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cygna.MapLookupGen.Tests;

public class GeneratorTests
{
    [Fact]
    public void Generator()
    {
        var refs = new List<MetadataReference>()
        {
            MetadataReference.CreateFromFile(typeof(MapLookupAttribute).Assembly.Location)
        };
        
        refs.AddRange(Net80.References.All);

        var compilation = CSharpCompilation.Create("TestProject",
            new[]
            {
                CSharpSyntaxTree.ParseText(@"
namespace Cygna.MapLookupGen.Tests.Model;

[MapLookup]
public partial class MyTestClass 
{

}"),
            },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new LookupMapGenerator();
        var sourceGenerator = generator.AsSourceGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [sourceGenerator],
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);

        var result = driver.GetRunResult().Results.Single();
        
        var allOutputs = result.TrackedOutputSteps.SelectMany(outputStep => outputStep.Value)
            .SelectMany(output => output.Outputs);
        
        Assert.Collection(allOutputs, output => Assert.Equal(IncrementalStepRunReason.New, output.Reason));
    }
}