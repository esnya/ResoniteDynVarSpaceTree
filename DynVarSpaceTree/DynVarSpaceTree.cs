using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DynVarSpaceTree
{
    public class DynVarSpaceTree : ResoniteMod
    {
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EnableLinkedVariablesList = new("EnableLinkedVariablesList", "Allow generating a list of dynamic variable definitions for a space.", () => true);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EnableVariableHierarchy = new("EnableVariableHierarchy", "Allow generating a hierarchy of dynamic variable components for a space.", () => true);

        private static ModConfiguration Config;

        public override string Author => "esnya, Banane9";
        public override string Link => "https://github.com/esnya/ResoniteDynVarSpaceTree";
        public override string Name => "DynVarSpaceTree";
        public override string Version => "3.0.0";

        private static FieldInfo _DynamicValues = null, _IdentityName = null, _IdentityType = null;
        private static PropertyInfo _Keys = null;
        
        public override void OnEngineInit()
        {
            var harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))]
        class WorkerInspectorPatch
        {
            static void Postfix(WorkerInspector __instance, Worker worker, UIBuilder ui)
            {
                if (worker is DynamicVariableSpace space)
                {
                    BuildInspectorUI(space, ui);
                }
            }
        }

        private static void BuildInspectorUI(DynamicVariableSpace space, UIBuilder ui)
        {
            if (Config.GetValue(EnableLinkedVariablesList))
                MakeButton(ui, "[Mod] Output names of linked Variables", () => OutputVariableNames(space));

            if (Config.GetValue(EnableVariableHierarchy))
                MakeButton(ui, "[Mod] Output tree of linked Variable Hierarchy", () => OutputVariableHierarchy(space));
        }

        private static void MakeButton(UIBuilder ui, string text, Action action)
        {
            var button = ui.Button(text);
            button.RequireLockInToPress.Value = true;

            var valueField = button.Slot.AttachComponent<ValueField<bool>>().Value;

            var toggle = button.Slot.AttachComponent<ButtonToggle>();
            toggle.TargetValue.Target = valueField;

            valueField.OnValueChange += field => action();
        }

        private static void SpawnText(Worker worker, string text)
        {
            var position = worker.LocalUserRoot.ViewPosition;
            var rotation = worker.LocalUserRoot.ViewRotation;

            UniversalImporter.Import(AssetClass.Unknown, Enumerable.Repeat(text, 1), worker.World, position + rotation * new float3(0, 0.5f, 1.0f), rotation);
        }

        private static void OutputVariableHierarchy(DynamicVariableSpace space)
        {
            var hierarchy = new SpaceTree(space);

            if (hierarchy.Process()) {
                SpawnText(space, hierarchy.ToString());
            }
        }

        private static IEnumerable GetDynamicValueKeys(DynamicVariableSpace space)
        {
            if (_DynamicValues is null)
            {
                _DynamicValues = space.GetType().GetField("_dynamicValues", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            var values = _DynamicValues.GetValue(space);
            if (_Keys is null)
            {
                _Keys = values.GetType().GetProperty("Keys");
            }

            return _Keys.GetValue(values) as IEnumerable;
        }

        private static (string, Type) GetIdentityFields(object identity)
        {
            if (_IdentityName is null)
            {
                var type = identity.GetType();
                _IdentityName = type.GetField("name");
                _IdentityType = type.GetField("type");
            }

            return (_IdentityName.GetValue(identity) as string, _IdentityType.GetValue(identity) as Type);
        }

        private static void OutputVariableNames(DynamicVariableSpace space)
        {
            var names = new StringBuilder("Variables linked to Namespace ");
            names.Append(space.SpaceName);
            names.AppendLine(":");

            foreach (var identity in GetDynamicValueKeys(space)) 
            {
                var (name, type) = GetIdentityFields(identity);
                names.Append(name);
                names.Append(" (");
                names.AppendTypeName(type);
                names.AppendLine(")");
            }

            names.Remove(names.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            SpawnText(space, names.ToString());
        }
    }
}
