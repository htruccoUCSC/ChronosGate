# Contributing

## How to add a unit

Current flow for adding a unit:

1. Add the unit to the spreadsheet.
2. Export to `Assets/StreamingAssets/units.json`.
3. Create a unit prefab at `Assets/Resources/Prefabs/Units/<unit_id>.prefab` by copying the template prefab in the same directory.
4. Create a unit script at `Assets/Scripts/Units/<UnitName>.cs` and that inherets `Assets/Scripts/Units/BaseUnit.cs`.
    1. The only function that it needs to override is CastAbility, but you can also override PerformBasicAttack and ScanTargeting when needed.
    2. If your unit also needs a custom projectile script add that.
5. In the unit prefab, add the unit script and set the `SpriteRenderer` sprite.
    1. If you unit uses an animator leave the sprite renderer empty

Notes:

- Keep prefab naming aligned with the unit ID in `units.json`.
