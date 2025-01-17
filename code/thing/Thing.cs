﻿using Sandbox;

using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

[Flags]
public enum ThingFlags
{
	None = (1 << 0),
	Solid = (1 << 1),
	Selectable = (1 << 2),
	Equipment = (1 << 3),
	Useable = (1 << 4),
	UseRequiresAiming = (1 << 5),
	AimTypeTargetCell = (1 << 6),
	CanWieldThings = (1 << 7),
	CanBePickedUp = (1 << 8),
	Exclusive = (1 << 9),
	DoesntBumpThings = (1 << 10),
	Puddle = (1 << 11),
	CanGainMutations = (1 << 12),
	CantBePushed = (1 << 13),
}

public enum FactionType { Neutral, Player, Enemy, Ghost, }

public class TattooData
{
	public string Icon { get; set; }
	public float Scale { get; set; }
	public Vector2 Offset { get; set; }
	public Vector2 OffsetWielded { get; set; }
	public Vector2 OffsetInfo { get; set; }
	public Vector2 OffsetCharWielded { get; set; }
	public Vector2 OffsetInfoWielded { get; set; }
}

public partial class Thing : Entity
{
	[Net] public ThingBrain Brain { get; set; }
	public bool HasBrain => Brain != null;

	[Net] public IntVector GridPos { get; protected set; }
	public IntVector LastGridPos { get; protected set; }
	[Net] public GridType ContainingGridType { get; set; }
	[Net] public GridManager ContainingGridManager { get; set; }

	[Net] public string DisplayIcon { get; set; }
	[Net] public string DisplayName { get; set; }
	[Net] public string Description { get; protected set; }
	[Net] public string Tooltip { get; set; }
	public virtual string ChatDisplayIcons => DisplayIcon;

	public bool ShouldUpdate { get; set; }
	[Net] public int PlayerNum { get; set; }

	[Net] public int IconDepth { get; set; }
	[Net] public int StackNum { get; set; }
	[Net] public float PathfindMovementCost { get; set; }

	public Vector2 MoveOffset { get; set; } // Client-only
	public Vector2 ShakeOffset { get; set; } // Client-only
	public Vector2 TotalOffset => MoveOffset + ShakeOffset;
	public float RotationDegrees { get; set; } // Client-only
	public float IconScale { get; set; } // Client-only
	public float Opacity { get; set; } // Client-only
	public int CharSkip { get; set; } // Client-only

	[Net] public string DebugText { get; set; }

	[Net] public uint ThingId { get; private set; }

	[Net] public ThingFlags ThingFlags { get; set; }
	public bool HasFlag(ThingFlags flag) => ThingFlags.HasFlag(flag);
	public void AddFlag(ThingFlags flag) { if (!HasFlag(flag)) ThingFlags |= flag; }
	public void RemoveFlag(ThingFlags flag) { if (HasFlag(flag)) ThingFlags &= ~flag; }

	[Net] public Thing WieldedThing { get; protected set; }
	[Net] public Thing ThingWieldingThis { get; protected set; }
	public Vector2 WieldedThingOffset { get; set; } // Client-only
	public int WieldedThingFontSize { get; set; } // Client-only
	public Vector2 InfoWieldedThingOffset { get; set; } // Client-only
	public int InfoWieldedThingFontSize { get; set; } // Client-only

	[Net] public List<Thing> EquippedThings { get; private set; } = new();
	[Net] public Thing ThingOwningThis { get; set; } // in inventory, equipment, and/or wielding
	public void SetOwningThing(Thing owningThing) { ThingOwningThis = owningThing; }

	[Net] public float ActionRechargePercent { get; set; }

	[Net] public LevelId CurrentLevelId { get; set; }

	public bool HasTattoo { get; set; } // Client-only
	public TattooData TattooData { get; set; } // Client-only

	public bool IsDestroyed { get; set; }
	[Net] public bool IsRemoved { get; set; }
	[Net] public bool IsOnCooldown { get; set; }
	[Net] public float CooldownProgressPercent { get; set; }

	[Net] public float CooldownTimer { get; set; }
	[Net] public float CooldownDuration { get; set; }

	[Net] public float StaminaTimer { get; set; }
	[Net] public float StaminaDelay { get; set; }

	[Net] public FactionType Faction { get; set; }

	[Net] public bool DontRender { get; set; }
	public TimeSince TimeSinceLocalPlayerSaw { get; set; } // Client-only

	// thing is entering/exiting a level and should not be able to act or be interacted with
	public bool IsInTransit { get; set; }

	public int FloaterNum { get; set; }

	public int Flammability { get; set; }
	[Net] public int IgnitionAmount { get; set; }
	private float _ignitionCoolTimer;

	public virtual string AbilityName => "Ability";

	protected override void OnAwake()
	{
		ShouldUpdate = false;
		DisplayIcon = ".";
		DisplayName = Name;
		Tooltip = "";
		IconDepth = (int)IconDepthLevel.Normal;
		IconScale = 1f;
		Opacity = 1f;

		ThingId = RoguemojiGame.ThingId++;
		IsRemoved = false;
		IsOnCooldown = false;
		IsInTransit = false;
		TimeSinceLocalPlayerSaw = 999f;

		if ( Game.IsClient )
		{
			WieldedThingOffset = new Vector2( 20f, 17f );
			WieldedThingFontSize = 18;
			InfoWieldedThingOffset = new Vector2( 38f, 38f );
			InfoWieldedThingFontSize = 32;
		}
	}

	public virtual void Update(float dt)
	{
		//DebugText = IgnitionAmount > 0 ? $"{IgnitionAmount}" : "";
		ClientTick();
		if (Brain != null)
			Brain.Update(dt);

		// HANDLE IGNITION COOLING
		if (IgnitionAmount > 0 && !HasComponent<CBurning>())
		{
			_ignitionCoolTimer += dt;
			if (_ignitionCoolTimer > Globals.IGNITION_COOL_DELAY)
			{
				IgnitionAmount = Math.Max(IgnitionAmount - Globals.IGNITION_COOL_AMOUNT, 0);
				_ignitionCoolTimer -= Globals.IGNITION_COOL_DELAY;
			}
		}

		if (IsOnCooldown)
			HandleCooldown(dt);

		if (HasStat(StatType.Energy) && HasStat(StatType.Stamina))
			HandleStamina(dt);

		// the wielded thing of NPCs has no ContainingGridManager, so must be updated manually (note: currently, if the wielded has ShouldUpdate=false, they will still not be getting updated)
		if (WieldedThing != null && WieldedThing.NeedsUpdate() && WieldedThing.ContainingGridType == GridType.None)
			WieldedThing.Update(dt);
	}

	public bool NeedsUpdate()
	{
		return ShouldUpdate || ThingComponents.Count > 0 || IsOnCooldown || Brain != null || IgnitionAmount > 0;
	}

	// todo: dont call this for everything
	public virtual void ClientTick()
	{
		float dt = Time.Delta;

		if (!string.IsNullOrEmpty(DebugText))
			DrawDebugText(DebugText);

		if (HasFloaters)
			HandleFloaters(dt);

		//DrawDebugText(ContainingGridManager?.Name.ToString() ?? "null");
		//DrawDebugText($"{GridPos}");

		//if (IgnitionAmount > 0)
		//    DrawDebugText($"{IgnitionAmount}");
	}

	public virtual bool TryMove(Direction direction, out bool switchedLevel, out bool actionWasntReady, bool shouldAnimate = true, bool shouldQueueAction = false, bool dontRequireAction = false)
	{
		if (ContainingGridType == GridType.None)
		{
			Log.Info($"TryMove - ContainingGridType == GridType.None - {DisplayIcon}");
		}

		Sandbox.Diagnostics.Assert.True(ContainingGridType != GridType.None);

		switchedLevel = false;
		actionWasntReady = false;

		if (IsInTransit)
			return false;

		if (direction == Direction.None)
			return true;

		if (!dontRequireAction)
		{
			CActing acting = null;
			if (GetComponent<CActing>(out var cActing))
				acting = (CActing)cActing;

			if (acting != null)
			{
				if (!acting.IsActionReady)
				{
					if (shouldQueueAction && HasBrain && Brain is RoguemojiPlayer p)
						p.QueueAction(new TryMoveAction(direction));

					actionWasntReady = true;
					return false;
				}

				acting.PerformedAction();
			}
		}

		if (HasComponent<CConfused>() && Game.Random.Int(0, 2) == 0)
		{
			Direction oldDir = direction;
			direction = GridManager.GetRandomDirection(cardinalOnly: false);
		}

		IntVector vec = GridManager.GetIntVectorForDirection(direction);
		IntVector newGridPos = GridPos + vec;

		var oldLevelId = CurrentLevelId;

		if (!ContainingGridManager.IsGridPosInBounds(newGridPos))
		{
			VfxNudge(direction, 0.1f, 10f);
			OnBumpedOutOfBounds(direction);
			return false;
		}

		if (!HasFlag(ThingFlags.DoesntBumpThings))
		{
			Thing other = ContainingGridManager.GetThingsAt(newGridPos).WithAll(ThingFlags.Solid).Where(x => !x.IsInTransit).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
			if (other != null)
			{
				BumpInto(other, direction);
				//RoguemojiGame.Instance.LogMessage(DisplayIcon + "(" + DisplayName + ") bumped into " + other.DisplayIcon + "(" + other.DisplayName + ")", PlayerNum);

				return false;
			}
		}

		SetGridPos(newGridPos);

		switchedLevel = oldLevelId != CurrentLevelId;
		if (shouldAnimate && !switchedLevel)
		{
			VfxSlide(direction, 0.15f, RoguemojiGame.CellSize);
		}

		OnMove(direction);

		if (HasFlag(ThingFlags.Solid))
			PlaySfx(SoundActionType.Move);

		if (HasBrain && Brain is RoguemojiPlayer player)
			player.RecenterCamera(shouldAnimate: true);

		return true;
	}

	public virtual void BumpInto(Thing target, Direction direction)
	{
		VfxNudge(direction, 0.1f, 10f);

		bool hasWieldedThing = WieldedThing != null;

		var bumpingThing = hasWieldedThing ? WieldedThing : this;

		bumpingThing.HitOther(target, direction);

		if (bumpingThing != null && !bumpingThing.IsRemoved)
			bumpingThing.OnBumpedIntoThing(target, direction);

		if (target != null && !target.IsRemoved)
			target.OnBumpedIntoBy(bumpingThing, direction);

		if (hasWieldedThing)
			OnWieldedThingBumpedOther(target, direction);
	}

	public virtual void HitOther(Thing target, Direction direction)
	{
		// todo: a way to force-feed food to other units
		//if (shouldUse && HasFlag(ThingFlags.Useable) && target.CanUseThing(this))
		//    Use(target);
		PlaySfx(SoundActionType.HitOther);

		target.HitBy(this, direction);
	}

	public virtual void HitBy(Thing hittingThing, Direction direction)
	{
		PlaySfx(SoundActionType.GetHit);
		VfxShake(0.2f, 4f);
		TakeDamageFrom(hittingThing);
	}

	public virtual void TakeDamageFrom(Thing thing)
	{
		//if (!HasStat(StatType.Health))
		//    return;

		OnTakeDamageFrom(thing);

		Hurt(thing.GetAttackDamage(), showImpactFloater: true);
	}

	public virtual void Hurt(int amount, bool showImpactFloater = true)
	{
		if (!HasStat(StatType.Health))
			return;

		if (amount > 0)
		{
			AdjustStat(StatType.Health, -amount);

			var floaterOffset = new Vector2(Game.Random.Float(3f, 10f) * (FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f));

			if (showImpactFloater)
				AddFloater(Globals.Icon(IconType.Explosion), 0.45f, floaterOffset, floaterOffset, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineIn, fadeInTime: 0.025f, scale: 1f, opacity: 1f, shakeAmount: 0f, moveToGridOnDeath: true);

			AddFloater("💔", 1.2f, floaterOffset, new Vector2(Game.Random.Float(10f, 20f) * (FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 25f), text: $"-{amount}", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, opacity: 1f, shakeAmount: 0f, moveToGridOnDeath: true);

			if (GetStatClamped(StatType.Health) <= 0)
			{
				Destroy();
			}
		}
	}

	public void AddSideFloater(string icon, string text = "", float time = 1.33f, bool requireSight = true, EasingType offsetEasingType = EasingType.Linear, float fadeInTime = 0.1f, float scale = 0.75f, float opacity = 1f)
	{
		AddSideFloater(icon, offsetStart: new Vector2(Game.Random.Float(8f, 12f) * (FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), offsetEnd: new Vector2(Game.Random.Float(12f, 15f) * (FloaterNum++ % 2 == 0 ? -1 : 1)), text, time, requireSight, offsetEasingType, fadeInTime, scale, opacity);
	}

	public void AddSideFloater(string icon, Vector2 offsetStart, Vector2 offsetEnd, string text = "", float time = 1.33f, bool requireSight = true, EasingType offsetEasingType = EasingType.Linear, float fadeInTime = 0.1f, float scale = 0.75f, float opacity = 1f)
	{
		AddFloater(icon, time, new Vector2(Game.Random.Float(8f, 12f) * (FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text, requireSight, alwaysShowWhenAdjacent: false, offsetEasingType, fadeInTime, scale);
	}

	public virtual void UseWieldedThing()
	{
		if (WieldedThing == null)
			return;

		// todo: AI needs to choose direction/target cell for aimed useables
		OnUseThing(WieldedThing);
		WieldedThing.Use(this);
	}

	public virtual void UseWieldedThing(Direction direction)
	{
		if (WieldedThing == null)
			return;

		OnUseThing(WieldedThing);
		WieldedThing.Use(this, direction);
	}

	public virtual void UseWieldedThing(GridType gridType, IntVector targetGridPos)
	{
		if (WieldedThing == null)
			return;

		OnUseThing(WieldedThing);
		WieldedThing.Use(this, gridType, targetGridPos);
	}

	// Override and return false when user doesn't have required stats (IsOnCooldown already handled elsewhere).
	public virtual bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
	{
		return true;
	}

	public virtual void Use(Thing user)
	{
		PlaySfx(SoundActionType.Use);
		PerformedAction(user);
	}

	public virtual void Use(Thing user, Direction direction)
	{
		PlaySfx(SoundActionType.Use);
		PerformedAction(user);
	}

	public virtual void Use(Thing user, GridType gridType, IntVector targetGridPos)
	{
		PlaySfx(SoundActionType.Use);
		PerformedAction(user);
	}

	public virtual void PerformedAction(Thing user)
	{
		if (user.GetComponent<CActing>(out var acting))
			((CActing)acting).PerformedAction();
	}

	protected override void OnDestroy()
	{
		if (IsDestroyed)
			return;

		IsDestroyed = true;

		OnDestroyed();

		DestroyFloatersClient();

		if (ThingWieldingThis != null)
			ThingWieldingThis.WieldThing(null);

		if (WieldedThing != null)
			WieldThing(null);

		Remove();
	}

	public virtual void SetGridPos(IntVector gridPos, bool setLastGridPosSame = false)
	{
		Sandbox.Diagnostics.Assert.True(ContainingGridManager.IsGridPosInBounds(gridPos));

		//if ( GridPos.Equals( gridPos ) && !forceRefresh )
		//	return;

		if (this == null || !IsValid || IsRemoved)
			return;

		ContainingGridManager.SetGridPos(this, gridPos);
		LastGridPos = setLastGridPosSame ? gridPos : GridPos;
		var lastGridPos = LastGridPos;
		GridPos = gridPos;

		if (ContainingGridManager.GridType == GridType.Inventory || ContainingGridManager.GridType == GridType.Equipment)
			RefreshGridPanelClient(To.Single(ContainingGridManager.OwningPlayer));
		else
			RefreshGridPanelClient(To.All);

		OnChangedGridPos();

		var existingThings = ContainingGridManager.GetThingsAt(gridPos);
		for (int i = existingThings.Count() - 1; i >= 0; i--)
		{
			var thing = existingThings.ElementAt(i);
			if (thing == this)
				continue;

			OnMovedOntoThing(thing);
			thing.OnMovedOntoBy(this);
		}
	}

	private void Remove()
	{
		if (IsRemoved)
			return;

		IsRemoved = true;

		if (ContainingGridType != GridType.None)
			ContainingGridManager.RemoveThing(this);

		CleanUpAndDelete();
	}

	public void CleanUpAndDelete()
	{
		if (WieldedThing != null)
			WieldedThing.CleanUpAndDelete();

		if (Brain != null && Brain is not RoguemojiPlayer)
			Brain.Delete();

		ClearStats();
		ClearTraits();
		Delete();
	}

	// //[TargetedRPC]
	public void RefreshGridPanelClient(To to)
	{
		if (Hud.Instance == null)
			return;

		GridPanel panel = Hud.Instance.GetGridPanel(ContainingGridType);
		if (panel == null)
			return;

		panel.StateHasChanged();
	}

	// //[TargetedRPC]
	public void SetTransformClient(float moveOffsetX = 0f, float moveOffsetY = 0f, float shakeOffsetX = 0f, float shakeOffsetY = 0f, float degrees = 0f, float scale = 1f)
	{
		MoveOffset = new Vector2(moveOffsetX, moveOffsetY);
		ShakeOffset = new Vector2(shakeOffsetX, shakeOffsetY);
		RotationDegrees = degrees;
		IconScale = scale;
	}

	// Client-only
	public void SetMoveOffset(Vector2 moveOffset)
	{
		MoveOffset = moveOffset;
	}

	// Client-only
	public void SetShakeOffset(Vector2 shakeOffset)
	{
		ShakeOffset = shakeOffset;
	}

	// Client-only
	public void SetRotation(float rotationDegrees)
	{
		RotationDegrees = rotationDegrees;
	}

	// Client-only
	public void SetScale(float scale)
	{
		IconScale = scale;
	}

	public void SetOpacity(float opacity)
	{
		Opacity = opacity;
	}

	public void DrawDebugText(string text, Color color, int line = 0, float time = 0f)
	{
		if (Game.IsServer)
		{
			DebugOverlay.ScreenText(text, new Vector2(20f, 20f), 0, color, time);
		}
		else
		{
			var player = RoguemojiGame.Instance.LocalPlayer;
			var offsetGridPos = GridPos - player.CameraGridOffset;

			var panel = Hud.Instance.GetGridPanel(ContainingGridType);
			if (panel != null)
				DebugOverlay.ScreenText(text, panel.GetCellScreenPos(offsetGridPos), line, color, time);
		}
	}

	public void DrawDebugText(string text)
	{
		DrawDebugText(text, new Color(1f, 1f, 1f, 0.4f));
	}

	public void SetIcon(string icon)
	{
		DisplayIcon = icon;
		//GridManager.RefreshGridPos(GridPos);
	}

	public override int GetHashCode()
	{
		var transformHash = HashCode.Combine(RotationDegrees, IconScale, IconDepth, ThingFlags, Opacity);
		return HashCode.Combine(DisplayIcon, WieldedThing?.ThingId ?? 0, PlayerNum + ThingId, transformHash);
		//return HashCode.Combine((DisplayIcon + ThingId.ToString()), PlayerNum, Offset, RotationDegrees, IconScale, IconDepth, Flags);
	}

	public int GetInfoDisplayHash()
	{
		// todo: check all stats
		return HashCode.Combine(NetworkIdent, DisplayIcon, WieldedThing?.DisplayIcon ?? "", ThingFlags, HasStats);
	}

	public int GetNearbyCellHash()
	{
		return HashCode.Combine(DisplayIcon, PlayerNum, IconDepth, ThingFlags, NetworkIdent, ThingId);
	}

	public int GetZPos()
	{
		return (IconDepth * 100) + StackNum;
	}

	public virtual void WieldThing(Thing thing)
	{
		if (thing == WieldedThing)
			return;

		if (WieldedThing != null)
		{
			OnNoLongerWieldingThing(WieldedThing);
			WieldedThing.OnNoLongerWieldedBy(this);
		}

		WieldedThing = thing;

		OnWieldThing(thing); // thing may be null here

		if (WieldedThing != null)
		{
			thing.OnWieldedBy(this);
		}
	}

	public void WieldAndRemoveFromGrid(Thing thing)
	{
		//if (ThingWieldingThis != null)
		//    ThingWieldingThis.WieldThing(null);

		WieldThing(thing);

		thing.SetOwningThing(this);

		if (thing.ContainingGridType != GridType.None)
			thing.ContainingGridManager.RemoveThing(thing);
	}

	/// <summary> For players, use MoveThingTo to equip things, and this will be called automatically. </summary>
	public void EquipThing(Thing thing)
	{
		if (EquippedThings == null)
			EquippedThings = new List<Thing>();

		AddFloater(thing.DisplayIcon, 0.6f, new Vector2(0f, 0f), new Vector2(0f, -7f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.SineOut, 0.05f);

		EquippedThings.Add(thing);

		OnEquipThing(thing);
		thing.OnEquippedTo(this);
	}

	/// <summary> For players, use MoveThingTo to unequip things, and this will be called automatically. </summary>
	public void UnequipThing(Thing thing)
	{
		AddFloater(thing.DisplayIcon, 0.6f, new Vector2(0f, -8f), new Vector2(0f, 0f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.SineOut, 0.05f);

		EquippedThings.Remove(thing);

		OnUnequipThing(thing);
		thing.OnUnequippedFrom(this);
	}

	public bool HasEquipmentType(TypeDescription type)
	{
		if (EquippedThings == null)
			return false;

		foreach (var thing in EquippedThings)
		{
			if (type == TypeLibrary.GetType(thing.GetType()))
				return true;
		}

		return false;
	}

	public virtual void Restart()
	{
		CurrentLevelId = LevelId.None;
		IsRemoved = false;
		EquippedThings.Clear();
		SetOwningThing(null);
		WieldedThing = null;
		ThingWieldingThis = null;
		IsOnCooldown = false;
		StaminaTimer = 0f;
		StaminaDelay = 0f;
		StatHash = 0;
		DontRender = false;
		IgnitionAmount = 0;
	}

	public virtual GridType AimingGridType => GridType.Arena;
	public virtual HashSet<IntVector> GetAimingTargetCellsClient() { return null; }
	public virtual bool IsPotentialAimingTargetCell(IntVector gridPos) { return false; }

	public void SetTattoo(string icon, float scale, Vector2 offset, Vector2 offsetWielded, Vector2 offsetInfo, Vector2 offsetCharWielded, Vector2 offsetInfoWielded)
	{
		SetTattooClient(icon, scale, offset, offsetWielded, offsetInfo, offsetCharWielded, offsetInfoWielded);
	}

	// //[TargetedRPC]
	public void SetTattooClient(string icon, float scale, Vector2 offset, Vector2 offsetWielded, Vector2 offsetInfo, Vector2 offsetCharWielded, Vector2 offsetInfoWielded)
	{
		HasTattoo = true;
		TattooData = new TattooData()
		{
			Icon = icon,
			Scale = scale,
			Offset = offset,
			OffsetWielded = offsetWielded,
			OffsetInfo = offsetInfo,
			OffsetCharWielded = offsetCharWielded,
			OffsetInfoWielded = offsetInfoWielded,
		};
	}

	public void RemoveTattoo()
	{
		HasTattoo = false;
	}

	public void StartCooldown(float time)
	{
		CooldownDuration = time;
		CooldownTimer = time;
		IsOnCooldown = true;
		CooldownProgressPercent = 0f;
		OnCooldownStart();
	}

	void HandleCooldown(float dt)
	{
		CooldownTimer -= dt;
		if (CooldownTimer < 0f)
			FinishCooldown();
		else
			CooldownProgressPercent = Utils.Map(CooldownTimer, CooldownDuration, 0f, 0f, 1f);
	}

	public void FinishCooldown()
	{
		IsOnCooldown = false;
		OnCooldownFinish();
	}

	void HandleStamina(float dt)
	{
		int energy = GetStatClamped(StatType.Energy);
		int energyMax = GetStatMax(StatType.Energy);

		if (energy < energyMax)
		{
			StaminaTimer -= dt;
			if (StaminaTimer < 0f)
			{
				StaminaTimer += StaminaDelay;
				AdjustStat(StatType.Energy, 1);
			}
		}
	}

	public bool CanSeeThing(Thing other)
	{
		if (other == this)
			return true;

		if (other == null || other.IsRemoved || other.IsInTransit)
			return false;

		if (!CanPerceiveAnyPartOfThing(other))
			return false;

		int adjustedSightDistance = Math.Max(GetStatClamped(StatType.SightDistance) - other.GetStatClamped(StatType.Stealth), 1);
		int distance = Utils.GetDistance(GridPos, other.GridPos);
		if (distance <= adjustedSightDistance)
		{
			if (ContainingGridManager.HasLineOfSight(GridPos, other.GridPos, GetStatClamped(StatType.SightPenetration), out IntVector collisionCell))
				return true;
		}

		return false;
	}

	/// <summary> Whether the gridPos is in range and line of sight is not blocked. </summary>
	public bool CanSeeGridPos(IntVector gridPos, int sight)
	{
		return Utils.GetDistance(GridPos, gridPos) <= sight && ContainingGridManager.HasLineOfSight(GridPos, gridPos, sight, out IntVector collisionCell);
	}

	/// <summary> Whether the thing is invisible and we can see invisible. Does not consider range or line of sight. </summary>
	public bool CanPerceiveThing(Thing thing)
	{
		if (thing.ThingWieldingThis == null)
		{
			return
				thing.GetStatClamped(StatType.Invisible) <= 0 ||
				(thing.ContainingGridType == GridType.Arena && (GridPos.Equals(thing.GridPos) || (GetStatClamped(StatType.Perception) > 0 && GridManager.GetDistance(GridPos, thing.GridPos) <= GetStatClamped(StatType.Perception)))) ||
				((thing.ContainingGridType == GridType.Equipment || thing.ContainingGridType == GridType.Inventory) && thing.ThingOwningThis == this) ||
				WieldedThing == thing ||
				thing == this;
		}
		else
		{
			var parent = thing.ThingWieldingThis;

			return
				thing.GetStatClamped(StatType.Invisible) <= 0 ||
				(parent.ContainingGridType == GridType.Arena && (GridPos.Equals(parent.GridPos) || (GetStatClamped(StatType.Perception) > 0 && GridManager.GetDistance(GridPos, parent.GridPos) <= GetStatClamped(StatType.Perception)))) ||
				((thing.ContainingGridType == GridType.Equipment || thing.ContainingGridType == GridType.Inventory) && thing.ThingOwningThis == this) ||
				WieldedThing == thing ||
				thing == this;
		}
	}

	/// <summary> Whether the thing is visible, or its wielded thing is visible (eg. an invisible thing carrying a non-invisible item) </summary>
	public bool CanPerceiveAnyPartOfThing(Thing thing)
	{
		return CanPerceiveThing(thing) || (thing.WieldedThing != null && CanPerceiveThing(thing.WieldedThing));
	}

	/// <summary> If conditionalGridPos is visible to player, declare that this thing has been noticed by them, so keep rendering it for moment even if it moves to a non-visible gridpos. </summary>
	// //[TargetedRPC]
	public void CanBeSeenByPlayerClient(IntVector conditionalGridPos)
	{
		var player = RoguemojiGame.Instance.LocalPlayer;
		if (player.IsCellVisible(conditionalGridPos))
		{
			TimeSinceLocalPlayerSaw = 0f;
		}
	}

	public int GetAttackDamage(bool checkWielded = false)
	{
		if (checkWielded && WieldedThing != null)
			return WieldedThing.GetAttackDamage();

		if (HasStat(StatType.Attack))
			return GetStatClamped(StatType.Attack);

		if (HasStat(StatType.Strength))
			return GetStatClamped(StatType.Strength);

		return 0;
	}

	public bool TryDropThingNearby(Thing thing)
	{
		if (thing.ThingOwningThis != this)
			return false;

		if (ContainingGridManager.GetRandomEmptyAdjacentGridPos(GridPos, out var dropGridPos, allowNonSolid: true))
		{
			if (thing.ContainingGridManager != null)
			{
				thing.ContainingGridManager.RemoveThing(thing);
			}

			ContainingGridManager.AddThing(thing);
			thing.SetGridPos(dropGridPos);
			thing.PlaySfx(SoundActionType.Drop);
			thing.VfxFly(GridPos, lifetime: 0.25f, heightY: 30f, progressEasingType: EasingType.Linear, heightEasingType: EasingType.SineInOut);

			thing.CanBeSeenByPlayerClient(GridPos);

			var tempIconDepth = thing.AddComponent<CTempIconDepth>();
			tempIconDepth.Lifetime = 0.35f;
			tempIconDepth.SetTempIconDepth((int)IconDepthLevel.Projectile);

			if (WieldedThing == thing)
				WieldThing(null);

			thing.SetOwningThing(null);

			return true;
		}

		return false;
	}
}
