using System.Text;

using FrooxEngine;
using HarmonyLib;

namespace DynVarSpaceTree;

internal sealed class SpaceTree
{
    private readonly Slot slot;
    private readonly DynamicVariableSpace space;
    private SpaceTree[] children = [];
    private IDynamicVariable[] dynVars = [];

    public SpaceTree(DynamicVariableSpace space, Slot? slot = null)
    {
        this.space = space;
        this.slot = slot ?? space.Slot;
    }

    public bool Process()
    {
        dynVars = [.. slot.GetComponents<IDynamicVariable>(IsLinkedDynVar)];

        children = [.. slot.Children.Select(child => new SpaceTree(space, child)).Where(static tree => tree.Process())];

        return dynVars.Length > 0 || children.Length > 0;
    }

    public override string ToString()
    {
        StringBuilder builder = new(space.Slot.Name);
        builder.Append(": Namespace ").AppendLine(space.SpaceName);

        BuildString(builder, "");
        builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);

        return builder.ToString();
    }

    private static void AppendDynVar(StringBuilder builder, string indent, IDynamicVariable dynVar, bool last = false)
    {
        builder.Append(indent);
        builder.Append(last ? "└─" : "├─");
        builder.Append(dynVar.VariableName);
        builder.Append(" (");
        builder.AppendTypeName(dynVar.GetType());
        builder.AppendLine(")");
    }

    private static void AppendSlot(StringBuilder builder, string indent, SpaceTree child, bool first, bool last)
    {
        if (!first)
        {
            builder.Append(indent);
            builder.AppendLine("│");
        }

        builder.Append(indent);
        builder.Append(last ? "└─" : "├─");
        builder.AppendLine(child.slot.Name);

        child.BuildString(builder, indent + (last ? "  " : "│ "));
    }

    private void BuildString(StringBuilder builder, string indent)
    {
        if (dynVars.Length > 0)
        {
            for (int i = 0; i < dynVars.Length - 1; ++i)
            {
                AppendDynVar(builder, indent, dynVars[i]);
            }

            AppendDynVar(builder, indent, dynVars[^1], children.Length == 0);

            if (children.Length > 0)
            {
                builder.Append(indent);
                builder.AppendLine("│");
            }
        }

        for (int i = 0; i < children.Length; ++i)
        {
            AppendSlot(builder, indent, children[i], i == 0, i == children.Length - 1);
        }
    }

    private bool IsLinkedDynVar(IDynamicVariable dynVar)
    {
        // Concrete dynamic variable handler types are generic, so Harmony Traverse keeps this reflection localized.
        return ReferenceEquals(Traverse.Create(dynVar).Field("handler").Field("_currentSpace").GetValue(), space);
    }
}
