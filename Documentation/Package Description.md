# Radial Menu - Package Description

Package: `com.fixer33.radial-menu` 1.0.1 ("Radial Menu"), Unity 6000.0+, author Fixer33, MIT licence.
Git URL: `https://github.com/Fixer33/RadialMenuPackage.git` (taken from the README links).
Anything not read directly from the files is marked (assumed).

## 1. Purpose and users

A procedural radial (pie/ring) menu for runtime UI built on **UI Toolkit**. The segments are drawn in code with `Painter2D`, so no UXML or prefab is needed. The caller describes the menu with a fluent builder and supplies items that create their own `VisualElement`s. Intended users are game developers who want a "hold a button, point at a segment, release" menu such as the Rust building-upgrade wheel (the included sample). Typical consumers are the author's own projects (assumed).

## 2. Architecture and main components

Namespaces: `RadialMenu`, `RadialMenu.Contracts`, `RadialMenu.Elements`, `RadialMenu.Enums`, `RadialMenu.MenuTypes`. Single runtime assembly `com.fixer33.radial-menu` (root namespace `RadialMenu`).

Flow: `RadialMenuBuilder.Create(panelSettings, items)` -> fluent `With...` calls fill a `RadialMenuSettings` record -> `Build<T>()` creates `T` (a menu type) and calls the internal `IRadialMenu.Initialize`. `RadialMenuBase` initialization:
1. creates a `GameObject` named "<TypeName> document object" (`DontDestroyOnLoad`) with a `UIDocument` using the supplied (or default) `PanelSettings`;
2. adds the base style sheet (`Resources/RadialMenuBaseStyle.uss`) and the optional additional style sheet;
3. adds a `RadialMenuRoot` (full-screen, receives pointer move and click events) containing a `RadialMenuElement` (the wheel);
4. starts hidden (`RadialMenuState.Hidden`, root `display: none`).

Components:
- `RadialMenuBuilder`: fluent configuration (see section 3). Defaults set in its constructor: outer radius 100, inner radius 90, stroke width 1, white fill/stroke with black highlighted stroke, centre radius 50, animation time 0, out-of-bounds action `None`, centre-click action `Close`, default click action `Close`.
- `RadialMenuSettings`: a `record` holding all values; properties have `internal set`, so only the package mutates it. It is exposed to menu types as `protected Settings`.
- `RadialMenuBase` (abstract, implements `IRadialMenu`): state machine (`Hidden`, `ShowTransition`, `Visible`, `HideTransition`), hover detection (angle plus distance from the centre), click handling, `Show/Hide/SetScreenPosition/Destroy`. Calling `Hide()` during the show transition sets a flag so the menu closes right after opening. A finalizer calls `Destroy()`.
- `RadialMenuElement` (`VisualElement`): draws the segments and centre circle in `generateVisualContent`, creates one item element and one centre element per item, positions item icons at segment mid-angles, switches highlight and centre-element visibility.
- `RadialMenuRoot` (`VisualElement`): raises `PointerMoved` and `PointerClick` (`Action<Vector2>`).
- Menu types (derive from `RadialMenuBase`): `InstantRadialMenu` (no animation) and `ScaledRadialMenu` (scale 0 to 1 and back over `VisibilityAnimationTime` ms using `experimental.animation`).
- Item base classes: `RadialMenuItemElement` (abstract; tracks `IsHighlighted`, calls `OnHighlightStarted/OnHighlightEnded`), `EmptyRadialMenuItemElement` (no-op highlight), `RadialMenuItemCenterElement` (`SetVisible` toggles `display`).

Click behaviour (read from `RadialMenuBase`): a click beyond `MainOuterRadius` applies the out-of-bounds action; inside `CenterElementRadius` applies the centre action; on the ring over an active segment performs the item (`OnItemPerform`) and hides; otherwise applies the default action. Hover detection depends on the out-of-bounds action: for `PerformItem`, `Close` and `CustomAction` the ring is bounded by the outer radius; otherwise any distance beyond the inner radius still selects the segment by angle (so "click anywhere outside to pick the segment" works).

## 3. Public API surface

Entry point:
- `RadialMenuBuilder.Create(PanelSettings panelSettings, IRadialMenuItem[] items)` (static), then `Build<T>() where T : IRadialMenu, new()`.
  If `panelSettings` is null, `Resources.Load<PanelSettings>("DefaultRadialMenuPanelSettings")` is used (an asset shipped in `Runtime/Resources`).

Builder methods (all return the builder): `WithVisibilityAnimationTime(float seconds)`, `WithMainOuterRadius(int, int = -1)`, `WithMainInnerRadius(int, int = -1)`, `WithSegmentSpacing(int)` (degrees), `WithSegmentStrokeWidth(int, int = -1)`, `InScreenPosition(Vector2)`, `WithAdditionalStyleSheet(StyleSheet)`, `WithMainColors(fill, highlighted, stroke, highlightedStroke)`, `WithCenterElementRadius(int, int = -1)`, `WithCenterElementStrokeWidth(int, int = -1)`, `WithCenterElementColors(four colours)`, `WithActionOnClickOutOfBounds(RadialMenuAction, Action = null)`, `WithActionOnClickInCenterElement(...)`, `WithDefaultActionOnClick(...)`, `WithDefaultCenterElement(Func<VisualElement>)`. The action methods log a warning when `CustomAction` has no callback or a callback is given with another action. A negative "highlighted" argument means "same as normal".

Runtime object (`IRadialMenu`): `Show()`, `Hide()`, `SetScreenPosition(Vector2)`, `Destroy()`; `RadialMenuBase.State` (`RadialMenuState`). `IRadialMenu.Initialize` is `internal`.

Enums: `RadialMenuAction` { `None`, `Close`, `PerformItem`, `PerformItemAndClose`, `CustomAction`, `CustomActionAndClose` }; `RadialMenuState` { `Hidden`, `ShowTransition`, `Visible`, `HideTransition` }.

Extension points:
- Implement `IRadialMenuItem` (`CreateItemElement()`, `CreateItemCenterElement()`, `OnItemPerform()`); element contracts `IRadialMenuItemElement` (`SetHighlighted`), `IRadialMenuItemCenterElement` (`SetVisible`), base `IRadialMenuVisualElement.GetVisualElement()` (default implementation casts `this` to `VisualElement`).
- Derive from `RadialMenuItemElement` / `RadialMenuItemCenterElement` (the sample does this).
- Derive from `RadialMenuBase` and implement `AnimateShow(Action)`, `AnimateHide(Action)`, `InitializeHiddenState()` for custom animations; protected members `Settings`, `Document`, `Root`, `Element`, `ElementCenterScreenPos`.
- Styling through `WithAdditionalStyleSheet` and the element names `radial-menu-parent` and `radial-menu-element`.
- Because `IRadialMenu.Initialize` is internal, a menu class outside the package can only work by deriving from `RadialMenuBase` (assumed from C# rules).

## 4. Dependencies

- Declared in `package.json`: none. Unity version `6000.0`.
- Assembly `com.fixer33.radial-menu`: no asmdef references, no `versionDefines`, no `defineConstraints`, autoReferenced. Uses `UnityEngine.UIElements` and `Painter2D`, built in to the engine. Uses C# `record` and `is ... or` patterns (needs C# 9+, satisfied by Unity 6).
- No scripting define symbols are used. No other `com.fixer33.*` package is referenced.
- Input: runtime logic uses UI Toolkit pointer events (no Input System dependency). The sample uses the legacy `UnityEngine.Input` (`GetMouseButtonDown`), so it needs the old Input Manager enabled (sample-only limitation, assumed).

## 5. Editor tools and menu items

None active. A disabled `Editor~/` folder contains only an asmdef (`com.fixer33.radial-menu.Editor`, Editor platform, references the runtime assembly) and no scripts. Unity ignores folders ending with `~`. The author confirmed it is a leftover; no editor tooling exists.

## 6. Samples and tests

- Sample "Rust Upgrade Menu" (`Samples~/Rust Upgrade Menu`, imported through Package Manager): `RustUpgradeRadialMenuExample` (MonoBehaviour; builds a 5-item `ScaledRadialMenu`, shows on right mouse button down, hides on up, destroys in `OnDestroy`), `UpgradeOption` (record implementing `IRadialMenuItem`), `UpgradeMenuElement`, `UpgradeMenuCenterElement`, `Styles/AdditionalStyle.uss`, `PanelSettings.asset`, `Scene.unity`. Sprites are serialized fields to be assigned in the scene.
- Tests: `Tests~/Editor` and `Tests~/Runtime` contain only asmdefs (`com.fixer33.radial-menu.Editor.Tests`, `com.fixer33.radial-menu.Tests`; NUnit, `UNITY_INCLUDE_TESTS`, not auto-referenced). No test code exists and both folders are disabled by `~`. The author confirmed these are leftovers; no tests exist at the moment.

## 7. How other projects consume it

Install by Git URL in Package Manager (`https://github.com/Fixer33/RadialMenuPackage.git`), or through the author's package installer tool (assumed). Then create items, call `RadialMenuBuilder.Create(panelSettings, items)...Build<ScaledRadialMenu>()` once (for example in `Start`), keep the returned `IRadialMenu`, call `Show()`/`Hide()` from input code and `Destroy()` on teardown. Builder method reference: `BUILDER.md` in this folder.

## 8. Current state

- Functional and finished at 1.0.1 (changelog: 1.0.0 on 2025-03-16; 1.0.1 on 2025-03-21 "Panel settings must now be provided before creating").
- Documentation: the README and `BUILDER.md` now show the real `Create(PanelSettings, IRadialMenuItem[])` signature (confirmed by the author). `BUILDER.md` still does not list `WithCenterElementStrokeWidth`.
- Code oddities (read, not changed): the default centre element (from `WithDefaultCenterElement`, or an empty `VisualElement`) is never added to the visual tree in `RadialMenuElement`, so `UpdateCenterElement` toggles a detached element; `RadialMenuRoot` has an unused `_element` field; the runtime Tests asmdef has `rootNamespace` `ItemCombining.Tests`; the `RadialMenuBase` finalizer calls `Destroy()`, which touches Unity objects (finalizers run off the main thread).
- No TODO comments in code. Tests and editor tooling are stubs only.
