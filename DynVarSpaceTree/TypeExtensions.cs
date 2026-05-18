using System.Text;

namespace DynVarSpaceTree;

internal static class TypeExtensions
{
    public static void AppendTypeName(this StringBuilder builder, Type type)
    {
        if (!type.IsGenericType)
        {
            builder.Append(type.Name);
            return;
        }

        builder.Append(type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)]);
        builder.Append('<');

        bool appendComma = false;
        foreach (Type arg in type.GetGenericArguments())
        {
            if (appendComma)
            {
                builder.Append(", ");
            }

            builder.AppendTypeName(arg);
            appendComma = true;
        }

        builder.Append('>');
    }
}
