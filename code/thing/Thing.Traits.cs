﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Trait : Entity
{
    [Net] public string TraitName { get; set; }
    [Net] public string Icon { get; set; }
    [Net] public string Description { get; set; }
    [Net] public Color BackgroundColor { get; set; }
    [Net] public float BarPercent { get; set; }
    [Net] public string Source { get; set; }
    [Net] public Vector2 Offset { get; set; }

    [Net] public bool HasTattoo { get; set; }
    [Net] public string TattooIcon { get; set; }
    [Net] public float TattooScale { get; set; }
    [Net] public Vector2 TattooOffset { get; set; }

    [Net] public bool HasLabel { get; set; }
    [Net] public string LabelText { get; set; }
    [Net] public int LabelFontSize { get; set; }
    [Net] public Vector2 LabelOffset { get; set; }
    [Net] public Color LabelColor { get; set; }
    [Net] public bool IsAbility { get; set; }
    public int Hash => HashCode.Combine(Icon, HasTattoo ? TattooIcon : "", HasLabel ? LabelText : "", BarPercent);

    public void SetTattoo(string icon, float scale, Vector2 offset)
    {
        HasTattoo = true;
        TattooIcon = icon;
        TattooScale = scale;
        TattooOffset = offset;
    }

    public void SetLabel(string text, int fontSize, Vector2 offset, Color color)
    {
        HasLabel = !string.IsNullOrEmpty(text);
        LabelText = text;
        LabelFontSize = fontSize;
        LabelOffset = offset;
        LabelColor = color;
    }
}

public partial class Thing : Entity
{
    [Net] public List<Trait> Traits { get; private set; } = new();
    [Net] public int TraitHash { get; private set; }

    public Trait AddTrait(string name, string icon, string description, Vector2 offset, bool isAbility = false, string source = "")
    {
        var trait = new Trait()
        {
            TraitName = name,
            Icon = icon,
            Description = description,
            Offset = offset,
            Source = source,
            IsAbility = isAbility,
        };

        if (Traits == null)
            Traits = new List<Trait>();

        Traits.Add(trait);
        RefreshTraitHash();
        return trait;
    }

    public Trait AddTrait(string name, string icon, string description, Vector2 offset, string tattooIcon, float tattooScale, Vector2 tattooOffset, bool isAbility = false, string source = "")
    {
        Trait trait = AddTrait(name, icon, description, offset, isAbility, source);
        trait.SetTattoo(tattooIcon, tattooScale, tattooOffset);
        RefreshTraitHash();
        return trait;
    }

    public Trait AddTrait(string name, string icon, string description, Vector2 offset, string tattooIcon, float tattooScale, Vector2 tattooOffset, string labelText, int labelFontSize, Vector2 labelOffset, Color labelColor, bool isAbility = false, string source = "")
    {
        Trait trait = AddTrait(name, icon, description, offset, isAbility, source);
        trait.SetTattoo(tattooIcon, tattooScale, tattooOffset);
        trait.SetLabel(labelText, labelFontSize, labelOffset, labelColor);
        RefreshTraitHash();
        return trait;
    }

    public Trait AddTrait(string name, string icon, string description, Vector2 offset, string labelText, int labelFontSize, Vector2 labelOffset, Color labelColor, bool isAbility = false, string source = "")
    {
        Trait trait = AddTrait(name, icon, description, offset, isAbility, source);
        trait.SetLabel(labelText, labelFontSize, labelOffset, labelColor);
        RefreshTraitHash();
        return trait;
    }

    public void RemoveTrait(Trait trait)
    {
        if (Traits.Contains(trait))
        {
            trait.Delete();
            Traits.Remove(trait);
            RefreshTraitHash();
        }
    }

    public void RefreshTraitHash()
    {
        TraitHash = 0;
        foreach (var trait in Traits)
            TraitHash += trait.Hash;
    }

    public void ClearTraits()
    {
        foreach (var trait in Traits)
            trait.Delete();

        Traits.Clear();
    }
}
