## Migration to ResoniteEsnyaMods

Development of this feature has moved to **ResoniteEsnyaMods 3.0.0**, in **EsnyaMods.Tools.dll**. Get the [distribution ZIP on resnyadb](https://resnyadb.nekometer.com/assets/c7210f0b70708ba66c93a5f47aa383db2dd76c8b60d03238283c425be2001495.zip), or import the [Static File into Resonite](https://resnyadb.nekometer.com/saved-graphs/6eacbb66ffb2b0675a81837e26408ad39e143c481d5f8c8aa72bfb4111d49eb9.brson). Requires .NET 10 Resonite and ResoniteModLoader 5.

Close Resonite, remove **DynVarSpaceTree.dll** from its loading directories (including HotReloadMods, if used), place the integrated DLL in `rml_mods`, and restart. Do not load the legacy and integrated versions together.

Back up your settings. Manually copy `EnableLinkedVariablesList` and `EnableVariableHierarchy` to the same keys prefixed with `DynVarSpaceTree.` in `rml_config/EsnyaMods.Tools.json`. Keep the existing JSON structure; do not replace the entire configuration file. The feature switch is `DynVarSpaceTree.Enabled`.

---

Dynamic Variable Space Tree
===========================

[![CI](https://github.com/esnya/ResoniteDynVarSpaceTree/actions/workflows/ci.yml/badge.svg)](https://github.com/esnya/ResoniteDynVarSpaceTree/actions/workflows/ci.yml)

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that adds buttons to the DynamicVariableSpace component in the inspector that allow copying all linked variable definitions or the whole hierarchy of linked dynamic variable components.

This maintained fork is based on [Banane9/NeosDynVarSpaceTree](https://github.com/Banane9/NeosDynVarSpaceTree) and targets the current Resonite runtime and .NET 10.

## Installation

1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place [DynVarSpaceTree.dll](https://github.com/esnya/ResoniteDynVarSpaceTree/releases/latest/download/DynVarSpaceTree.dll) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Resonite logs.

## Trimmed Sample Outputs

Linked Variable Definitions:

```
Variables linked to Namespace SpotifyStatus:
MainRim (color)
MainBG (color)
MainGradient (color)
Cover (Uri)
Cover (IAssetProvider<ITexture2D>)
Cover (IAssetProvider<Sprite>)
CoverAccent (color)
Album (String)
Album (Uri)
FontRegular (IAssetProvider<FontSet>)
Title (String)
Title (Uri)
FontBold (IAssetProvider<FontSet>)
WSClient (WebsocketClient)
...
```

Linked Dynamic Variable Components in Hierarchy:

```
Spotify Control 2.1.3: Namespace SpotifyStatus
├─Fixed UI
│ ├─Main
│ │ ├─Background
│ │ │ ├─SpotifyStatus/MainRim (DynamicValueVariableDriver<color>)
│ │ │ │
│ │ │ ├─Background Mask
│ │ │ │ ├─SpotifyStatus/MainBG (DynamicValueVariableDriver<color>)
│ │ │ │ │
│ │ │ │ └─Gradient
│ │ │ │   └─SpotifyStatus/MainGradient (DynamicValueVariableDriver<color>)
│ │ │ │
│ │ │ └─Content
│ │ │   ├─Song Info
│ │ │   │ ├─Cover
│ │ │   │ │ ├─Image
│ │ │   │ │ │ ├─SpotifyStatus/Cover (DynamicField<Uri>)
│ │ │   │ │ │ ├─SpotifyStatus/Cover (DynamicReferenceVariable<IAssetProvider<ITexture2D>>)
│ │ │   │ │ │ └─SpotifyStatus/Cover (DynamicReferenceVariable<IAssetProvider<Sprite>>)
│ │ │   │ │ │
│ │ │   │ │ └─Rim
│ │ │   │ │   └─SpotifyStatus/CoverAccent (DynamicValueVariableDriver<color>)
...
```

## AI-assisted development notice

Generative-AI coding agents were used in the development of esnya's modifications in this maintained fork. The project describes use of this maintained version as indirect use of generative AI. If applicable law or the terms or policies of a platform or service applicable to your use requires disclosure of that indirect use, provide the required disclosure as specified by those requirements. This notice does not redefine any external requirement or add restrictions to the MIT No Attribution (MIT-0) License.
