﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class Squirrel : Thing
{
    public CTargeting Targeting { get; private set; }
    public CActing Acting { get; private set; }

    public override void OnSpawned()
    {
        base.OnSpawned();

		DisplayIcon = "🐿";
		DisplayName = "Squirrel";
		Description = "A bushy-tailed rodent";
		IconDepth = (int)IconDepthLevel.Solid;
		ShouldUpdate = true;
		Tooltip = "A squirrel";
		ThingFlags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanWieldThings | ThingFlags.CanGainMutations;
		PathfindMovementCost = 5f;
		Faction = FactionType.Enemy;
		Flammability = 13;

		if ( Game.IsServer )
		{
			InitStat( StatType.Health, 5, 0, 5 );
			InitStat( StatType.Attack, 1 );
			InitStat( StatType.Speed, 2 );
			InitStat( StatType.SightDistance, 7 );
			InitStat( StatType.SightPenetration, 7 );
			InitStat( StatType.Hearing, 6 );
			InitStat( StatType.SightBlockAmount, 5 );
			//InitStat(StatType.Smell, 3);
		}
		else
		{
			WieldedThingOffset = new Vector2( -3, 14f );
			WieldedThingFontSize = 15;
			InfoWieldedThingOffset = new Vector2( 38f, 38f );
			InfoWieldedThingFontSize = 30;
		}

		Targeting = AddComponent<CTargeting>();
        Acting = AddComponent<CActing>();
        Acting.ActionDelay = CActing.CalculateActionDelay(GetStatClamped(StatType.Speed));
        Acting.ActionTimer = Game.Random.Float(0f, 1f);

        Brain = new SquirrelBrain();
        Brain.ControlThing(this);
    }

    protected override void OnDestroy()
    {
        // todo: dont spawn blood if burned to death
        if (!ContainingGridManager.DoesGridPosContainThingType<PuddleBlood>(GridPos))
        {
            ContainingGridManager.RemovePuddles(GridPos, fadeOut: true);
            ContainingGridManager.SpawnThing<PuddleBlood>(GridPos);
        }

        if (Game.Random.Float(0f, 1f) < 0.5f)
            ContainingGridManager.SpawnThing<Bone>(GridPos);

        base.OnDestroy();
    }
}
