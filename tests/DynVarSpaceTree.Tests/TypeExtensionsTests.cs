using System.Text;

namespace DynVarSpaceTree.Tests;

public sealed class TypeExtensionsTests
{
    [Fact]
    public void AppendTypeNameShouldWriteNonGenericTypeName()
    {
        StringBuilder builder = new();

        builder.AppendTypeName(typeof(string));

        Assert.Equal("String", builder.ToString());
    }

    [Fact]
    public void AppendTypeNameShouldWriteGenericTypeName()
    {
        StringBuilder builder = new();

        builder.AppendTypeName(typeof(Dictionary<string, int>));

        Assert.Equal("Dictionary<String, Int32>", builder.ToString());
    }

    [Fact]
    public void AppendTypeNameShouldWriteNestedGenericTypeName()
    {
        StringBuilder builder = new();

        builder.AppendTypeName(typeof(Dictionary<string, List<int>>));

        Assert.Equal("Dictionary<String, List<Int32>>", builder.ToString());
    }
}
