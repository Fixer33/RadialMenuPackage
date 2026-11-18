# Radial Menu - Usage Guide

Package: `com.fixer33.radial-menu` 1.0.1, Unity 6000.0+, UI Toolkit based. Anything that was inferred rather than read directly from the code is marked (assumed).

## 1. Installation

Via the author's installer package `com.fixer33.package-installer` (Git URL `https://github.com/Fixer33/PackageInstaller.git`, assumed from the installer's README badge; install it first): open **Tools > Fixer33 > Package Installer** and choose **Radial Menu** (id `com.fixer33.radial-menu`). The installer's package list points to:

```
https://github.com/Fixer33/RadialMenuPackage.git
```

Manual alternative: **Window > Package Manager > + > Install package from git URL...** and paste the URL above.

The package has no declared dependencies. It uses only engine UI Toolkit (`UnityEngine.UIElements`, `Painter2D`).

## 2. Minimal setup

1. Create items that implement `IRadialMenuItem` (see section 3).
2. Build the menu once, for example in `Start`, and keep the returned `IRadialMenu`.
3. Call `Show()` / `Hide()` from your own input code and `Destroy()` on teardown.

No UXML, prefab or scene object is needed. The menu creates its own `GameObject` with a `UIDocument` (named `"<TypeName> document object"`, marked `DontDestroyOnLoad`). If you pass `null` as `PanelSettings`, the shipped `DefaultRadialMenuPanelSettings` asset (Resources) is used.

## 3. Step-by-step API use

### 3.1 Describe the items
Each item creates its own highlight element and its own centre element. Adapted from the sample:

```csharp
using RadialMenu;
using RadialMenu.Contracts;

public record UpgradeOption : IRadialMenuItem
{
    public IRadialMenuItemElement CreateItemElement() => new UpgradeMenuElement(/* icon, colours */);
    public IRadialMenuItemCenterElement CreateItemCenterElement() => new UpgradeMenuCenterElement(/* icon, name, description */);
    public void OnItemPerform() => Debug.Log("performed");
}
```

Item element (derives `RadialMenuItemElement`, which implements `IRadialMenuItemElement.SetHighlighted` and calls your overrides only when the state changes):

```csharp
public class UpgradeMenuElement : RadialMenuItemElement
{
    protected override void OnHighlightStarted() { /* tint, scale... */ }
    protected override void OnHighlightEnded()   { /* restore */ }
}
```

If you do not need highlight visuals, use `EmptyRadialMenuItemElement`. For the centre element, derive from `RadialMenuItemCenterElement` (its `SetVisible` toggles `style.display`) and add your icon and labels in the constructor.

### 3.2 Build
```csharp
_radialMenu = RadialMenuBuilder
    .Create(_panelSettings, _items)            // null panelSettings = package default
    .WithVisibilityAnimationTime(0.1f)         // seconds
    .InScreenPosition(new Vector2(Screen.width * .5f, Screen.height * .5f))
    .WithMainOuterRadius(236, 240)             // normal, highlighted
    .WithMainInnerRadius(150, 160)
    .WithMainColors(Color.white, Color.red, Color.white, Color.red) // fill, highlighted fill, stroke, highlighted stroke
    .WithCenterElementRadius(150)
    .WithActionOnClickOutOfBounds(RadialMenuAction.PerformItemAndClose)
    .Build<ScaledRadialMenu>();                // or InstantRadialMenu
```

Builder methods (all return the builder; a negative "highlighted" argument means "same as normal"): `WithVisibilityAnimationTime`, `WithMainOuterRadius`, `WithMainInnerRadius`, `WithSegmentSpacing` (degrees), `WithSegmentStrokeWidth`, `InScreenPosition`, `WithAdditionalStyleSheet`, `WithMainColors`, `WithCenterElementRadius`, `WithCenterElementStrokeWidth`, `WithCenterElementColors`, `WithActionOnClickOutOfBounds`, `WithActionOnClickInCenterElement`, `WithDefaultActionOnClick`, `WithDefaultCenterElement`. Defaults set by the builder: outer radius 100, inner radius 90, centre radius 50, animation time 0, out-of-bounds action `None`, centre-click action `Close`, default click action `Close`.

### 3.3 Show, hide, move, destroy
```csharp
if (Input.GetMouseButtonDown(1)) _radialMenu.Show();
else if (Input.GetMouseButtonUp(1)) _radialMenu.Hide();

_radialMenu.SetScreenPosition(new Vector2(x, y)); // screen pixels, origin bottom-left
_radialMenu.Destroy();                            // in OnDestroy
```
`RadialMenuBase.State` (`RadialMenuState`: `Hidden`, `ShowTransition`, `Visible`, `HideTransition`) is available on the concrete menu object (cast the `IRadialMenu` to `RadialMenuBase`, assumed from the type hierarchy). `Show()` only works from `Hidden`. `Hide()` during `ShowTransition` closes the menu as soon as the open animation finishes.

### 3.4 Click behaviour (`RadialMenuAction`)
Values: `None`, `Close`, `PerformItem`, `PerformItemAndClose`, `CustomAction`, `CustomActionAndClose`. On a click (only while `Visible`):
- beyond the outer radius: the out-of-bounds action;
- inside the centre radius: the centre action;
- on the ring over an active segment: `OnItemPerform()` on that item, then `Hide()`;
- anywhere else: the default action.

For `CustomAction` / `CustomActionAndClose` pass the callback as the second argument, for example `.WithActionOnClickOutOfBounds(RadialMenuAction.CustomAction, () => ...)`. The builder logs a warning if `CustomAction` has no callback, or a callback is given with another action. Note the warning check compares only against `CustomAction`, not `CustomActionAndClose`, so `CustomActionAndClose` with a callback also logs a warning even though the callback is used.

## 4. Editor windows and menu paths

The package adds none. The `Editor~` folder contains only an asmdef and no scripts (the author confirmed it is a leftover). The only related menu is the installer's **Tools > Fixer33 > Package Installer**. The sample is imported via **Package Manager > Radial Menu > Samples > Rust Upgrade Menu**.

## 5. Configuration and scripting defines

- No scripting define symbols, `versionDefines` or `defineConstraints` are used.
- Configuration is code-only through `RadialMenuBuilder`. The resulting `RadialMenuSettings` record is exposed to menu types as `protected Settings` and is read-only from outside the package (`internal set`).
- Visuals: your own `PanelSettings`, an extra `StyleSheet` (`WithAdditionalStyleSheet`), and the base style sheet `RadialMenuBaseStyle.uss` that targets `#radial-menu-parent` (full-screen root) and `#radial-menu-element` (the wheel).
- Input: the menu reacts to UI Toolkit pointer move and click events only. The sample uses legacy `UnityEngine.Input`, so it needs the old Input Manager enabled.

## 6. Extension points and a worked example

- Implement `IRadialMenuItem`, `IRadialMenuItemElement`, `IRadialMenuItemCenterElement` (their `GetVisualElement()` default casts `this` to `VisualElement`, so implementers should derive from `VisualElement`, or the provided base classes).
- Derive from `RadialMenuBase` for a custom open/close animation. You must implement `AnimateShow(Action)`, `AnimateHide(Action)` and `InitializeHiddenState()`. Protected members available: `Settings`, `Document`, `Root`, `Element`, `ElementCenterScreenPos`. You must invoke the callback when the animation finishes, because it moves the state to `Visible` / `Hidden`.
- Styling through `WithAdditionalStyleSheet`.

Worked example: a fade menu type.

```csharp
using System;
using RadialMenu;

public class FadeRadialMenu : RadialMenuBase
{
    protected override void InitializeHiddenState() => Element.style.opacity = 0f;

    protected override void AnimateShow(Action callback) =>
        Element.experimental.animation
            .Start(0f, 1f, Settings.VisibilityAnimationTime, (e, v) => e.style.opacity = v)
            .OnCompleted(() => callback?.Invoke());

    protected override void AnimateHide(Action callback) =>
        Element.experimental.animation
            .Start(1f, 0f, Settings.VisibilityAnimationTime, (e, v) => e.style.opacity = v)
            .OnCompleted(() => callback?.Invoke());
}

// usage
var menu = RadialMenuBuilder.Create(null, items).WithVisibilityAnimationTime(0.15f).Build<FadeRadialMenu>();
```

`VisibilityAnimationTime` is stored in milliseconds (the builder converts seconds). The `experimental.animation.Start` call is a Unity API, modelled on how `ScaledRadialMenu` uses `experimental.animation.Scale` (assumed to compile as written; not compiled here). A menu type must have a parameterless constructor (`Build<T>() where T : IRadialMenu, new()`).

## 7. Common pitfalls (current behaviour)

- Items array: pass a non-empty array; the angle step is `360 / Items.Length` (assumed to misbehave with zero items). `CreateItemElement()` and `CreateItemCenterElement()` must not return null (the element builder calls `GetVisualElement()` on both).
- Menu contents are fixed once built (setters are `internal`); to change items, `Destroy()` and build a new menu (assumed intended, see open questions).
- Always call `Destroy()` yourself. The `RadialMenuBase` finalizer also calls `Destroy()`, but finalizers do not run on the Unity main thread, so it is not a reliable safety net.
- Coordinates: `InScreenPosition` / `SetScreenPosition` use screen pixels with the origin at the bottom-left; hover and click maths compare pointer positions with `Screen.height - y`. This matches only when the panel scale equals the screen scale. The shipped default `PanelSettings` uses constant pixel size; other scale modes are untested (assumed to be off).
- Default position is `Vector2.zero` (bottom-left corner) if you do not call `InScreenPosition`.
- `WithDefaultCenterElement` (and the empty fallback) creates an element that is never added to the visual tree, so it is not visible. Only per-item centre elements show.
- The hovered segment depends on the out-of-bounds action: for `PerformItem`, `Close` and `CustomAction` the ring is bounded by the outer radius; with other actions (including the default `None`) any distance beyond the inner radius selects the segment by angle.
- Hover highlighting and the centre element update on pointer move; clicking relies on the pointer being over the full-screen root while the menu is `Visible`.
- `Hide()` is ignored in `Hidden` and `HideTransition`; `Show()` is ignored in any state other than `Hidden`.
- `IRadialMenu.Initialize` is `internal`, so menu types outside the package must derive from `RadialMenuBase`.
- `Documentation/BUILDER.md` does not list `WithCenterElementStrokeWidth` (the method exists), and its wording for the negative highlighted stroke width ("use radius value") is inaccurate: the code uses the normal width.
- Touch and gamepad are not handled by the package (pointer events only).

## 8. Working with other com.fixer33.* packages

- `com.fixer33.package-installer` (`https://github.com/Fixer33/PackageInstaller.git`, assumed from its README badge): installs this package by name from its package list.
- This package does not reference or depend on any other `com.fixer33.*` package.

## 9. Remaining open questions

Questions from the package's open list that affect usage:
- Q4: Is the default centre element (`WithDefaultCenterElement` / empty fallback) meant to be shown? Today it is never in the visual tree.
- Q5: Is the finalizer calling `Destroy()` an intended safety net? It may fail off the main thread.
- Q6: Are non-constant-pixel-size `PanelSettings` scale modes supported? Hover and click maths assume panel scale equals screen scale.
- Q7: Should custom `IRadialMenu` implementations be possible? Currently only `RadialMenuBase` descendants work.
- Q8: Is rebuilding the menu the intended way to change items or settings?
- Q9: Should the sample support the new Input System? It uses legacy `Input.GetMouseButtonDown`.
- Q10: Are touch or gamepad inputs a goal?
- Q1 / Q2: Documentation gaps in `BUILDER.md` (missing `WithCenterElementStrokeWidth`, wrong wording for negative highlighted stroke width).
