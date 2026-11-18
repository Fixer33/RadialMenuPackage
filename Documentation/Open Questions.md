# Radial Menu - Open Questions

## Still open

### Documentation vs code
1. `BUILDER.md` omits `WithCenterElementStrokeWidth`. Omission or intentional?
2. `BUILDER.md` says a negative `WithSegmentStrokeWidth` highlighted value "will just use radius value" (copy-paste wording); code uses the normal width value.

### Tests
3. `Tests~/Runtime` asmdef has `rootNamespace` `ItemCombining.Tests`, which looks copied from another package. Intended?

### Design and behaviour
4. The default centre element (`WithDefaultCenterElement`, or the empty fallback) is never added to the visual tree in `RadialMenuElement`, so it cannot be seen. Unfinished feature, or should it be added to the centre holder?
5. `RadialMenuBase` has a finalizer calling `Destroy()`, which uses Unity APIs. Finalizers run on another thread, so the call may fail if the owner forgot `Destroy()`. Intended safety net?
6. Hover and click maths compare pointer event positions (panel space) with `Screen.height - ScreenPosition.y` (screen pixels). This holds only when panel scale equals screen scale. The shipped default `PanelSettings` has ScaleMode 0 (constant pixel size); other scale modes are untested (assumed).
7. `IRadialMenu.Initialize` is `internal`, so only `RadialMenuBase` descendants can be menu types. Intended, or should custom `IRadialMenu` implementations be possible?
8. A built menu has fixed items and settings (setters are `internal`). Is rebuilding the menu the intended way to change items?

### Dependencies and platform
9. The sample uses legacy `Input.GetMouseButtonDown`, which fails if only the new Input System is enabled. Should the sample support the Input System?
10. Is touch or gamepad use a goal? Code handles only pointer events and the sample uses the right mouse button.
11. `RadialMenuRoot` has an unused private field `_element`. Leftover?

## Answered
- `Editor~` and `Tests~` (asmdefs only, no code): the author confirmed they are leftovers; no editor tooling or tests exist at the moment.
- README and `BUILDER.md` showed `Create(params IRadialMenuItem[] items)`: the author confirmed the code signature `Create(PanelSettings, IRadialMenuItem[])` is correct; README and `BUILDER.md` were updated to match.
