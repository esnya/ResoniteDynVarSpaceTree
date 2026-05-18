using System.Collections;
using System.Reflection;
using System.Text;

using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using HarmonyLib;
using ResoniteModLoader;

#if USE_RESONITE_HOT_RELOAD_LIB
using ResoniteHotReloadLib;
#endif

namespace DynVarSpaceTree;

public sealed class DynVarSpaceTree : ResoniteMod
{
    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnableLinkedVariablesList = new("EnableLinkedVariablesList", "Allow generating a list of dynamic variable definitions for a space.", () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnableVariableHierarchy = new("EnableVariableHierarchy", "Allow generating a hierarchy of dynamic variable components for a space.", () => true);

    private static ModConfiguration? Config { get; set; }
    private static FieldInfo? dynamicValuesField;
    private static FieldInfo? identityNameField;
    private static FieldInfo? identityTypeField;
    private static PropertyInfo? keysProperty;
#if USE_RESONITE_HOT_RELOAD_LIB
    private static Harmony? harmony;
#endif

    public override string Author => "esnya, Banane9";
    public override string Link => "https://github.com/esnya/ResoniteDynVarSpaceTree";
    public override string Name => "DynVarSpaceTree";
    public override string Version => typeof(DynVarSpaceTree).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(static attribute => attribute.Key == "ModVersion")?.Value ?? "0.0.0";

    public override void OnEngineInit()
    {
        Init(this);

#if USE_RESONITE_HOT_RELOAD_LIB
        HotReloader.RegisterForHotReload(this);
#endif
    }

    private static ModConfiguration LoadedConfig => Config ?? throw new InvalidOperationException("Mod configuration has not been initialized.");

    private static void Init(ResoniteMod mod)
    {
        Harmony patcher = new($"{mod.Author}.{mod.Name}");
        ModConfiguration modConfig = mod.GetConfiguration()
            ?? throw new InvalidOperationException("Mod configuration could not be loaded.");
#if USE_RESONITE_HOT_RELOAD_LIB
        harmony = patcher;
#endif
        Config = modConfig;
        modConfig.Save(true);
        patcher.PatchAll();
    }

#if USE_RESONITE_HOT_RELOAD_LIB
    internal static void BeforeHotReload()
    {
        harmony?.UnpatchAll(harmony.Id);
    }

    internal static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
    }
#endif

    private static void BuildInspectorUI(DynamicVariableSpace space, UIBuilder ui)
    {
        if (LoadedConfig.GetValue(EnableLinkedVariablesList))
        {
            MakeButton(ui, "[Mod] Output names of linked Variables", () => OutputVariableNames(space));
        }

        if (LoadedConfig.GetValue(EnableVariableHierarchy))
        {
            MakeButton(ui, "[Mod] Output tree of linked Variable Hierarchy", () => OutputVariableHierarchy(space));
        }
    }

    private static void MakeButton(UIBuilder ui, string text, Action action)
    {
        Button button = ui.Button(text);
        button.RequireLockInToPress.Value = true;

        Sync<bool> valueField = button.Slot.AttachComponent<ValueField<bool>>().Value;

        ButtonToggle toggle = button.Slot.AttachComponent<ButtonToggle>();
        toggle.TargetValue.Target = valueField;

        valueField.OnValueChange += _ => action();
    }

    private static void SpawnText(Worker worker, string heading, string text)
    {
        Slot slot = worker.World.LocalUserSpace.AddSlot(heading);
        slot.PositionInFrontOfUser();
        UniversalImporter.SpawnText(slot, heading, text);
        slot.CreateSpawnUndoPoint();
    }

    private static void OutputVariableHierarchy(DynamicVariableSpace space)
    {
        SpaceTree hierarchy = new(space);

        if (hierarchy.Process())
        {
            SpawnText(space, "Dynamic Variable Hierarchy", hierarchy.ToString());
        }
    }

    private static IEnumerable GetDynamicValueKeys(DynamicVariableSpace space)
    {
        dynamicValuesField ??= GetRequiredField(space.GetType(), "_dynamicValues");

        object values = dynamicValuesField.GetValue(space)
            ?? throw new InvalidOperationException("Dynamic variable values field returned null.");

        keysProperty ??= GetRequiredProperty(values.GetType(), "Keys");

        return keysProperty.GetValue(values) as IEnumerable
            ?? throw new InvalidOperationException("Dynamic variable values keys property did not return an enumerable.");
    }

    private static (string Name, Type Type) GetIdentityFields(object identity)
    {
        if (identityNameField is null || identityTypeField is null)
        {
            Type identityType = identity.GetType();
            identityNameField = GetRequiredField(identityType, "name");
            identityTypeField = GetRequiredField(identityType, "type");
        }

        string name = identityNameField.GetValue(identity) as string
            ?? throw new InvalidOperationException("Dynamic variable identity name field returned null.");
        Type type = identityTypeField.GetValue(identity) as Type
            ?? throw new InvalidOperationException("Dynamic variable identity type field returned null.");

        return (name, type);
    }

    private static FieldInfo GetRequiredField(Type type, string name)
    {
        return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new MissingFieldException(type.FullName, name);
    }

    private static PropertyInfo GetRequiredProperty(Type type, string name)
    {
        return type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new MissingMemberException(type.FullName, name);
    }

    private static void OutputVariableNames(DynamicVariableSpace space)
    {
        StringBuilder names = new("Variables linked to Namespace ");
        names.Append(space.SpaceName);
        names.AppendLine(":");

        foreach (object identity in GetDynamicValueKeys(space))
        {
            (string name, Type type) = GetIdentityFields(identity);
            names.Append(name);
            names.Append(" (");
            names.AppendTypeName(type);
            names.AppendLine(")");
        }

        if (names.Length >= Environment.NewLine.Length)
        {
            names.Remove(names.Length - Environment.NewLine.Length, Environment.NewLine.Length);
        }

        SpawnText(space, "Dynamic Variables", names.ToString());
    }

    [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))]
    private static class WorkerInspectorPatch
    {
        private static void Postfix(Worker worker, UIBuilder ui)
        {
            if (worker is DynamicVariableSpace space)
            {
                BuildInspectorUI(space, ui);
            }
            else if (worker is IDynamicVariable variable and IComponent component)
            {
                DynamicVariableHelper.ParsePath(variable.VariableName, out string spaceName, out _);
                DynamicVariableSpace? foundSpace = component.Slot.FindSpace(spaceName);
                if (foundSpace is not null)
                {
                    BuildInspectorUI(foundSpace, ui);
                }
            }
        }
    }
}
