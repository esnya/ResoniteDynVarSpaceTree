using Mono.Cecil;

namespace DynVarSpaceTree.Tests;

public sealed class ReflectionContractTests
{
    [Fact]
    public void DynamicVariableSpaceShouldExposeExpectedBackingField()
    {
        using AssemblyDefinition frooxEngineAssembly = AssemblyDefinition.ReadAssembly(GetAssemblyPath("FrooxEngine.dll"));
        TypeDefinition dynamicVariableSpace = GetRequiredType(frooxEngineAssembly, "FrooxEngine.DynamicVariableSpace");
        FieldDefinition? field = dynamicVariableSpace.Fields.FirstOrDefault(static field => field.Name == "_dynamicValues");

        Assert.NotNull(field);
    }

    [Fact]
    public void DynamicVariableInterfaceShouldExposeExpectedNameProperty()
    {
        using AssemblyDefinition frooxEngineAssembly = AssemblyDefinition.ReadAssembly(GetAssemblyPath("FrooxEngine.dll"));
        TypeDefinition dynamicVariable = GetRequiredType(frooxEngineAssembly, "FrooxEngine.IDynamicVariable");
        PropertyDefinition? property = dynamicVariable.Properties.FirstOrDefault(static property => property.Name == "VariableName");

        Assert.NotNull(property);
    }

    [Fact]
    public void ModAssemblyShouldDeclareWorkerInspectorPatchMetadata()
    {
        using AssemblyDefinition modAssembly = AssemblyDefinition.ReadAssembly(GetAssemblyPath("DynVarSpaceTree.dll"));
        TypeDefinition patchType = GetRequiredType(modAssembly, "DynVarSpaceTree.DynVarSpaceTree/WorkerInspectorPatch");

        Assert.Contains(
            patchType.CustomAttributes,
            static attribute => attribute.AttributeType.FullName is "HarmonyLib.HarmonyPatch");
    }

    [Fact]
    public void RequiredExternalAssembliesShouldResolveInTestOutput()
    {
        Assert.True(File.Exists(GetAssemblyPath("FrooxEngine.dll")));
        Assert.True(File.Exists(GetAssemblyPath("Elements.Core.dll")));
        Assert.True(File.Exists(GetAssemblyPath("Elements.Assets.dll")));
        Assert.True(File.Exists(GetAssemblyPath("ResoniteModLoader.dll")));
        Assert.True(File.Exists(GetAssemblyPath("0Harmony.dll")));
    }

    private static string GetAssemblyPath(string assemblyFileName)
    {
        return Path.Combine(AppContext.BaseDirectory, assemblyFileName);
    }

    private static TypeDefinition GetRequiredType(AssemblyDefinition assembly, string fullName)
    {
        return assembly.MainModule.GetType(fullName)
            ?? throw new InvalidOperationException($"Type '{fullName}' was not found in '{assembly.MainModule.FileName}'.");
    }
}
