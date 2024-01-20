using Sandbox;
using System;

namespace Roguemoji;
public partial class Bouquet : Thing
{
    protected override void OnAwake()
    {
        DisplayIcon = "💐";
        DisplayName = "Bouquet";
        Description = "A lovely bunch of flowers";
        Tooltip = "A bouquet";
        IconDepth = (int)IconDepthLevel.Normal;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;
        Flammability = 18;
    }
}
