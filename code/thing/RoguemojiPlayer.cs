﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum AimingSource { Throwing, UsingWieldedItem }
public enum AimingType { Direction, TargetCell }

public partial class RoguemojiPlayer : Thing
{
    public CActing Acting { get; private set; }

    public CIconPriority IconPriority { get; private set; }

    [Net] public IntVector CameraGridOffset { get; set; }
    public Vector2 CameraPixelOffset { get; set; } // Client-only
    public float CameraFade { get; set; } // Client-only

    [Net] public GridManager InventoryGridManager { get; private set; }
    [Net] public GridManager EquipmentGridManager { get; private set; }

    [Net] public Thing SelectedThing { get; private set; }

    [Net] public bool IsDead { get; set; }

    public Dictionary<TypeDescription, PlayerComponent> PlayerComponents = new Dictionary<TypeDescription, PlayerComponent>();

    public IQueuedAction QueuedAction { get; private set; }
    [Net] public string QueuedActionName { get; private set; }

    [Net] public bool IsAiming { get; set; }
    [Net] public AimingSource AimingSource { get; set; }
    [Net] public AimingType AimingType { get; set; }
    public HashSet<IntVector> AimingCells { get; set; } // Client-only

    [Net] public IList<ScrollType> IdentifiedScrollTypes { get; private set; }
    [Net] public IList<PotionType> IdentifiedPotionTypes { get; private set; }

    [Net] public int ConfusionSeed { get; set; }
    public bool IsConfused => ConfusionSeed > 0;

    [Net] public int HallucinatingSeed { get; set; }
    public bool IsHallucinating => HallucinatingSeed > 0;

    public RoguemojiPlayer()
	{
		IconDepth = 5;
        ShouldUpdate = true;
		DisplayName = "Player";
		Tooltip = "";
        PathfindMovementCost = 10f;

        if (Game.IsServer)
        {
			InventoryGridManager = new();
			InventoryGridManager.Init(RoguemojiGame.InventoryWidth, RoguemojiGame.InventoryHeight);
            InventoryGridManager.GridType = GridType.Inventory;
            InventoryGridManager.OwningPlayer = this;

            EquipmentGridManager = new();
            EquipmentGridManager.Init(RoguemojiGame.EquipmentWidth, RoguemojiGame.EquipmentHeight);
            EquipmentGridManager.GridType = GridType.Equipment;
            EquipmentGridManager.OwningPlayer = this;

            IdentifiedScrollTypes = new List<ScrollType>();
            IdentifiedPotionTypes = new List<PotionType>();
            SetStartingValues();
        }
        else
        {
            VisibleCells = new HashSet<IntVector>();
            SeenCells = new Dictionary<LevelId, HashSet<IntVector>>();
            SeenThings = new Dictionary<LevelId, Dictionary<IntVector, List<SeenThingData>>>();
            AimingCells = new HashSet<IntVector>();

            WieldedThingOffset = new Vector2(20f, 17f);
            WieldedThingFontSize = 18;
            InfoWieldedThingOffset = new Vector2(38f, 38f);
            InfoWieldedThingFontSize = 32;
        }
	}

    public override void Spawn()
    {
        base.Spawn();

        Acting = AddComponent<CActing>();
        IconPriority = AddComponent<CIconPriority>();
        IconPriority.SetDefaultIcon("😀");
    }

    void SetStartingValues()
    {
        DisplayIcon = "😀";
        Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanUseThings;
        IsDead = false;
        //ActionDelay = TimeSinceAction = 0.5f;
        //IsActionReady = true;
        QueuedAction = null;
        QueuedActionName = "";
        RefreshVisibility(firstRefresh: true);
        SightBlockAmount = 10;
        IsAiming = false;
        SelectedThing = null;
        Faction = FactionType.Player;
        CameraFade = 0f;
        IsInTransit = false;
        FloaterNum = 0;
        ConfusionSeed = 0;
        HallucinatingSeed = 0;

        IdentifiedScrollTypes.Clear();
        IdentifiedPotionTypes.Clear();

        ClearStats();
        InitStat(StatType.Health, 10, 0, 10);
        InitStat(StatType.Energy, 0, 0, 0);
        InitStat(StatType.Mana, 0, 0, 0);
        InitStat(StatType.Attack, 1);
        InitStat(StatType.Strength, 2);
        InitStat(StatType.Speed, 13);
        InitStat(StatType.Intelligence, 5);
        InitStat(StatType.Stamina, 5);
        InitStat(StatType.Stealth, 0, -999, 999);
        InitStat(StatType.Charisma, 3);
        InitStat(StatType.Sight, 9, min: 0);
        InitStat(StatType.Hearing, 3);
        //InitStat(StatType.Smell, 1);
        FinishInitStats();

        StaminaTimer = StaminaDelay;

        ClearTraits();

        InventoryGridManager.Restart();
        InventoryGridManager.SetWidth(RoguemojiGame.InventoryWidth);

        EquipmentGridManager.Restart();

        for (int x = 0; x < RoguemojiGame.InventoryWidth; x++)
            for (int y = 0; y < RoguemojiGame.InventoryHeight - 1; y++)
                SpawnRandomInventoryThing(new IntVector(x, y));

        RoguemojiGame.Instance.RefreshGridPanelClient(GridType.Inventory);
        RoguemojiGame.Instance.RefreshGridPanelClient(GridType.Equipment);
        RoguemojiGame.Instance.RefreshNearbyPanelClient();
    }

    public override void Restart()
    {
        base.Restart();

        RestartClient();

        ThingComponents.Clear();
        Acting = AddComponent<CActing>();
        Acting.IsActionReady = false;
        IconPriority = AddComponent<CIconPriority>();
        IconPriority.SetDefaultIcon("😀");

        SetStartingValues();
    }

    [ClientRpc]
    public void RestartClient()
    {
        SeenCells.Clear();
        SeenThings.Clear();
    }

    void SpawnRandomInventoryThing(IntVector gridPos)
    {
        int rand = Game.Random.Int(0, 23);
        switch (rand)
        {
            case 0: InventoryGridManager.SpawnThing<Leaf>(gridPos); break;
            case 1: InventoryGridManager.SpawnThing<Potato>(gridPos); break;
            //case 2: InventoryGridManager.SpawnThing<Nut>(gridPos); break;
            case 2: InventoryGridManager.SpawnThing<PotionHallucination>(gridPos); break;
            case 3: InventoryGridManager.SpawnThing<Mushroom>(gridPos); break;
            //case 4: InventoryGridManager.SpawnThing<Trumpet>(gridPos); break;
            case 4: InventoryGridManager.SpawnThing<PotionConfusion>(gridPos); break;
            //case 5: InventoryGridManager.SpawnThing<Bouquet>(gridPos); break;
            case 5: InventoryGridManager.SpawnThing<PotionPoison>(gridPos); break;
            //case 6: InventoryGridManager.SpawnThing<Cheese>(gridPos); break;
            case 6: InventoryGridManager.SpawnThing<PotionBlindness>(gridPos); break;
            //case 7: InventoryGridManager.SpawnThing<Coat>(gridPos); break;
            case 7: InventoryGridManager.SpawnThing<ScrollTelekinesis>(gridPos); break;
            //case 8: InventoryGridManager.SpawnThing<SafetyVest>(gridPos); break;
            case 8: InventoryGridManager.SpawnThing<PotionSpeed>(gridPos); break;
            case 9: InventoryGridManager.SpawnThing<Sunglasses>(gridPos); break;
            //case 10: InventoryGridManager.SpawnThing<Telescope>(gridPos); break;
            //case 10: InventoryGridManager.SpawnThing<Refreshment>(gridPos); break;
            case 10: InventoryGridManager.SpawnThing<PotionSleeping>(gridPos); break;
            case 11: InventoryGridManager.SpawnThing<WhiteCane>(gridPos); break;
            //case 11: InventoryGridManager.SpawnThing<Cigarette>(gridPos); break;
            case 12: InventoryGridManager.SpawnThing<ScrollBlink>(gridPos); break;
            case 13: InventoryGridManager.SpawnThing<BowAndArrow>(gridPos); break;
            case 14: InventoryGridManager.SpawnThing<Backpack>(gridPos); break;
            //case 14: InventoryGridManager.SpawnThing<Juicebox>(gridPos); break;
            case 15: InventoryGridManager.SpawnThing<BookBlink>(gridPos); break;
            case 16: InventoryGridManager.SpawnThing<PotionMana>(gridPos); break;
            case 17: InventoryGridManager.SpawnThing<PotionHealth>(gridPos); break;
            case 18: InventoryGridManager.SpawnThing<PotionEnergy>(gridPos); break;
            case 19: InventoryGridManager.SpawnThing<ScrollTeleport>(gridPos); break;
            case 20: InventoryGridManager.SpawnThing<BookTeleport>(gridPos); break;
            //case 21: InventoryGridManager.SpawnThing<AcademicCap>(gridPos); break;
            case 21: InventoryGridManager.SpawnThing<RugbyBall>(gridPos); break;
            case 22: InventoryGridManager.SpawnThing<Joystick>(gridPos); break;
            case 23: InventoryGridManager.SpawnThing<ScrollFear>(gridPos); break;
        }
    }

    public override void OnClientActive(IClient client)
    {
        base.OnClientActive(client);

		DisplayName = Client.Name;
		Tooltip = Client.Name;
	}

    public override void Update(float dt)
	{
		base.Update(dt);

        //DebugText = $"{Acting.ActionDelay}";

        InventoryGridManager.Update(dt);

        for(int i = PlayerComponents.Count - 1; i >= 0; i--)
        {
            KeyValuePair<TypeDescription, PlayerComponent> pair = PlayerComponents.ElementAt(i);

            var component = pair.Value;
            if (component.ShouldUpdate)
                component.Update(dt);
        }

        //DebugText = "";
        //if (QueuedAction != null)
        //    DebugText = QueuedActionName;

        //DebugText = $"{IsInTransit}";
    }

	public override void Simulate(IClient cl )
	{
		if(Game.IsServer)
		{
            if (Input.Pressed(InputButton.View)) 
                CharacterHotkeyPressed();

            if (!IsDead && !IsInTransit)
            {
                if (!IsAiming)
                {
                    if (Input.Pressed(InputButton.Slot1))                                                       WieldHotbarSlot(0);
                    else if (Input.Pressed(InputButton.Slot2))                                                  WieldHotbarSlot(1);
                    else if (Input.Pressed(InputButton.Slot3))                                                  WieldHotbarSlot(2);
                    else if (Input.Pressed(InputButton.Slot4))                                                  WieldHotbarSlot(3);
                    else if (Input.Pressed(InputButton.Slot5))                                                  WieldHotbarSlot(4);
                    else if (Input.Pressed(InputButton.Slot6))                                                  WieldHotbarSlot(5);
                    else if (Input.Pressed(InputButton.Slot7))                                                  WieldHotbarSlot(6);
                    else if (Input.Pressed(InputButton.Slot8))                                                  WieldHotbarSlot(7);
                    else if (Input.Pressed(InputButton.Slot9))                                                  WieldHotbarSlot(8);
                    else if (Input.Pressed(InputButton.Slot0))                                                  WieldHotbarSlot(9);
                    else if (Input.Pressed(InputButton.Use))                                                    PickUpTopItem();
                    else if (Input.Pressed(InputButton.Drop))                                                   DropWieldedItem();
                    else if (Input.Pressed(InputButton.Jump))                                                   UseWieldedThing();
                    else if (Input.Pressed(InputButton.Menu))                                                   WieldThing(null);
                    else if (Input.Pressed(InputButton.Left))                                                   TryMove(Direction.Left, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Right))                                                  TryMove(Direction.Right, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Back))                                                   TryMove(Direction.Down, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Forward))                                                TryMove(Direction.Up, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Flashlight))                                             StartAimingThrow();
                    else if (Input.Down(InputButton.Left))                                                      TryMove(Direction.Left);
                    else if (Input.Down(InputButton.Right))                                                     TryMove(Direction.Right);
                    else if (Input.Down(InputButton.Back))                                                      TryMove(Direction.Down);
                    else if (Input.Down(InputButton.Forward))                                                   TryMove(Direction.Up);
                }
                else
                {
                    if (Input.Pressed(InputButton.Left))                                                        ConfirmAiming(Direction.Left);
                    else if (Input.Pressed(InputButton.Right))                                                  ConfirmAiming(Direction.Right);
                    else if (Input.Pressed(InputButton.Back))                                                   ConfirmAiming(Direction.Down);
                    else if (Input.Pressed(InputButton.Forward))                                                ConfirmAiming(Direction.Up);
                    else if (Input.Pressed(InputButton.Jump) && AimingSource == AimingSource.UsingWieldedItem)  StopAiming();
                    else if (Input.Pressed(InputButton.Flashlight) && AimingSource == AimingSource.Throwing)    StopAiming();
                }
            }

            if (Input.Pressed(InputButton.Reload))
            {
                RoguemojiGame.Instance.Restart();
            }
        }
	}

    [Event.Tick.Client]
    public override void ClientTick()
    {
        base.ClientTick();

        float dt = Time.Delta;
        foreach (KeyValuePair<TypeDescription, PlayerComponent> pair in PlayerComponents)
        {
            var component = pair.Value;
            if (component.ShouldUpdate)
                component.Update(dt);
        }

        //DrawDebugText("" + CameraGridOffset + ", " + CameraPixelOffset);
        //DrawDebugText("# Things: " + InventoryGridManager.Things.Count);
        //Log.Info("Player:Client - Sight: " + GetStat(StatType.Sight));
    }

    public override void OnActionRecharged()
    {
        if(QueuedAction != null)
        {
            QueuedAction.Execute(this);
            QueuedAction = null;
            QueuedActionName = "";
        }
    }

    public void ClearQueuedAction()
    {
        QueuedAction = null;
    }

    void WieldHotbarSlot(int index)
    {
        if (index >= InventoryGridManager.GridWidth)
            return;

        var thing = InventoryGridManager.GetThingsAt(InventoryGridManager.GetGridPos(index)).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing != null && Input.Down(InputButton.Run))
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else
        {
            if(thing != null && thing.HasFlag(ThingFlags.Equipment))
                TryEquipThing(thing);
            else
                WieldThing(thing);
        }
    }

    public override void OnWieldThing(Thing thing) 
    {
        base.OnWieldThing(thing);

        RoguemojiGame.Instance.FlickerWieldingPanel();
    }

    public override bool TryMove(Direction direction, bool shouldAnimate = true, bool shouldQueueAction = false, bool dontRequireAction = false)
	{
        if (!Acting.IsActionReady && !dontRequireAction)
        {
            if(shouldQueueAction)
            {
                QueuedAction = new TryMoveAction(direction);
                QueuedActionName = QueuedAction.ToString();
            }
            
            return false;
        }

        if(IsConfused && Game.Random.Int(0, 2) == 0)
            direction = GridManager.GetRandomDirection(cardinalOnly: false);

        var oldLevelId = CurrentLevelId;

        var success = base.TryMove(direction, shouldAnimate: false, shouldQueueAction: false);
		if (success)
		{
            var switchedLevel = oldLevelId != CurrentLevelId;
            var movedCamera = RecenterCamera(shouldAnimate: !switchedLevel);

            if(shouldAnimate && !switchedLevel)
                VfxSlide(direction, 0.1f, RoguemojiGame.CellSize);
            //VfxSlide(direction, movedCamera ? 0.1f : 0.2f, RoguemojiGame.CellSize);

            if(Game.Random.Int(0, 5) == 0)
                IconPriority.AddIconPriority(Utils.GetRandomIcon("😄", "🙂"), (int)PlayerIconPriority.Move, 1.0f);
        }
        else 
		{
            IconPriority.AddIconPriority("😠", (int)PlayerIconPriority.Attack, 0.4f);
        }

        if(!dontRequireAction)
            Acting.PerformedAction();

		return success;
	}

    public override void BumpInto(Thing other, Direction direction)
    {
        base.BumpInto(other, direction);

        //if(other is Hole)
        //{
        //    if(CurrentLevelId == LevelId.Forest0)
        //        RoguemojiGame.Instance.SetPlayerLevel(this, LevelId.Forest1);
        //    else if (CurrentLevelId == LevelId.Forest1)
        //        RoguemojiGame.Instance.SetPlayerLevel(this, LevelId.Forest2);
        //}
        if(other is Door)
        {
            if (CurrentLevelId == LevelId.Forest1)
                RoguemojiGame.Instance.ChangeThingLevel(this, LevelId.Forest0);
            else if (CurrentLevelId == LevelId.Forest2)
                RoguemojiGame.Instance.ChangeThingLevel(this, LevelId.Forest1);
        }
    }

    public override void SetGridPos(IntVector gridPos)
	{
		base.SetGridPos(gridPos);

        RoguemojiGame.Instance.FlickerNearbyPanelCellsClient();
    }

	public void SelectThing(Thing thing)
	{
		if (SelectedThing == thing)
			return;

		if (SelectedThing != null)
			SelectedThing.RefreshGridPanelClient();

		SelectedThing = thing;
	}

    /// <summary>Returns true if offset changed.</summary>
    public bool RecenterCamera(bool shouldAnimate = false)
    {
        var middleCell = new IntVector(MathX.FloorToInt((float)RoguemojiGame.ArenaPanelWidth / 2f), MathX.FloorToInt((float)RoguemojiGame.ArenaPanelHeight / 2f));
        var oldCamGridOffset = CameraGridOffset;
        var movedCamera = SetCameraGridOffset(GridPos - middleCell);

        if(movedCamera && shouldAnimate)
        {
            // todo: make an option to turn this off
            var dir = GridManager.GetDirectionForIntVector(CameraGridOffset - oldCamGridOffset);
            VfxSlideCamera(dir, 0.25f, RoguemojiGame.CellSize);
        }

        return movedCamera;
    }

    /// <summary>Returns true if offset changed.</summary>
    public bool SetCameraGridOffset(IntVector offset)
    {
        var currOffset = CameraGridOffset;

        CameraGridOffset = new IntVector(
            Math.Clamp(offset.x, 0, ContainingGridManager.GridWidth - RoguemojiGame.ArenaPanelWidth),
            Math.Clamp(offset.y, 0, ContainingGridManager.GridHeight - RoguemojiGame.ArenaPanelHeight)
        );

        return !CameraGridOffset.Equals(currOffset);
    }

    public void SetCameraPixelOffset(Vector2 offset)
    {
        CameraPixelOffset = new Vector2(MathF.Round(offset.x), MathF.Round(offset.y));
    }

    public bool IsGridPosOnCamera(IntVector gridPos)
    {
        return
            (gridPos.x >= CameraGridOffset.x - 1) &&
            (gridPos.x < CameraGridOffset.x + RoguemojiGame.ArenaPanelWidth + 1) &&
            (gridPos.y >= CameraGridOffset.y - 1) &&
            (gridPos.y < CameraGridOffset.y + RoguemojiGame.ArenaPanelHeight + 1);
    }

    public PlayerComponent AddPlayerComponent(TypeDescription type)
    {
        if (PlayerComponents.ContainsKey(type))
        {
            var component = PlayerComponents[type];
            component.ReInitialize();
            return component;
        }
        else
        {
            var component = type.Create<PlayerComponent>();
            component.Init(this);
            PlayerComponents.Add(type, component);
            return component;
        }
    }

    public T AddPlayerComponent<T>() where T : PlayerComponent
    {
        return AddPlayerComponent(TypeLibrary.GetType(typeof(T))) as T;
    }

    public void RemovePlayerComponent(TypeDescription type)
    {
        if (PlayerComponents.ContainsKey(type))
        {
            var component = PlayerComponents[type];
            component.OnRemove();
            PlayerComponents.Remove(type);
        }
    }

    public void RemovePlayerComponent<T>() where T : PlayerComponent
    {
        RemovePlayerComponent(TypeLibrary.GetType(typeof(T)));
    }

    public void ForEachPlayerComponent(Action<PlayerComponent> action)
    {
        foreach (var (_, component) in PlayerComponents)
        {
            action(component);
        }
    }

    [ClientRpc]
    public void VfxSlideCamera(Direction direction, float lifetime, float distance)
    {
        var slide = AddPlayerComponent<VfxPlayerSlideCamera>();
        slide.Direction = direction;
        slide.Lifetime = lifetime;
        slide.Distance = distance;
    }

    [ClientRpc]
    public void VfxShakeCamera(float lifetime, float distance)
    {
        var shake = AddPlayerComponent<VfxPlayerShakeCamera>(); ;
        shake.Lifetime = lifetime;
        shake.Distance = distance;
    }

    [ClientRpc]
    public void VfxFadeCamera(float lifetime, bool shouldFadeOut)
    {
        var fade = AddPlayerComponent<VfxPlayerFadeCamera>();
        fade.Lifetime = lifetime;
        fade.ShouldFadeOut = shouldFadeOut;
    }

    public override void TakeDamage(Thing source)
    {
        if (IsDead)
            return;

        base.TakeDamage(source);

        IconPriority.AddIconPriority(Utils.GetRandomIcon("😲", "😲", "😧", "😨") , (int)PlayerIconPriority.TakeDamage, 1.0f);
    }

    public override void Destroy()
    {
        if (IsDead)
            return;

        IsDead = true;
        StopAiming();

        IconPriority.AddIconPriority("💀", (int)PlayerIconPriority.Dead);

        OnDied();
    }

    public void PickUpTopItem()
    {
        var thing = ContainingGridManager.GetThingsAt(GridPos).WithAll(ThingFlags.CanBePickedUp).WithNone(ThingFlags.Solid).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing == null)
            return;

        if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            MoveThingTo(thing, GridType.Inventory, emptyGridPos, wieldIfPossible: true);
        else if(thing.HasFlag(ThingFlags.Equipment) && EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPosEquipment))
            MoveThingTo(thing, GridType.Equipment, emptyGridPosEquipment);
    }

    public void ThrowWieldedThing(Direction direction)
    {
        if (!Acting.IsActionReady)
        {
            QueuedAction = new ThrowThingAction(WieldedThing, direction);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        if (WieldedThing == null || direction == Direction.None)
            return;

        var projectile = WieldedThing.AddComponent<CProjectile>();
        projectile.Direction = direction;
        projectile.MoveDelay = 0.1f;
        projectile.RemainingDistance = 5;
        projectile.Thrower = this;

        MoveThingTo(WieldedThing, GridType.Arena, GridPos);
    }

    public void DropWieldedItem()
    {
        if (WieldedThing != null)
            MoveThingTo(WieldedThing, GridType.Arena, GridPos);
    }

    void TryEquipThing(Thing thing)
    {
        if (EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            MoveThingTo(thing, GridType.Equipment, emptyGridPos);
    }

    public override void UseWieldedThing()
    {
        if (WieldedThing == null)
        {
            //if (SelectedThing != null && IsInInventory(SelectedThing))
            //{
            //    if (SelectedThing.HasFlag(ThingFlags.Equipment))
            //        TryEquipThing(SelectedThing);
            //    else
            //        WieldThing(SelectedThing);
            //}

            return;
        }

        if (!WieldedThing.HasFlag(ThingFlags.Useable))
            return;

        if (WieldedThing.IsOnCooldown)
            return;
            
        if(!WieldedThing.CanBeUsedBy(this, shouldLogMessage: true))
            return;

        if (WieldedThing.HasFlag(ThingFlags.UseRequiresAiming))
        {
            AimingType aimingType = WieldedThing.HasFlag(ThingFlags.AimTypeTargetCell) ? AimingType.TargetCell : AimingType.Direction;
            StartAiming(AimingSource.UsingWieldedItem, aimingType);
        }
        else
        {
            if (!Acting.IsActionReady)
            {
                QueuedAction = new UseWieldedThingAction();
                QueuedActionName = QueuedAction.ToString();
                return;
            }

            WieldedThing.Use(this);
        }
    }

    public override void UseWieldedThing(Direction direction)
    {
        if (!Acting.IsActionReady)
        {
            QueuedAction = new UseWieldedThingDirectionAction(direction);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        base.UseWieldedThing(direction);
    }

    public override void UseWieldedThing(IntVector targetGridPos)
    {
        if (!Acting.IsActionReady)
        {
            QueuedAction = new UseWieldedThingTargetAction(targetGridPos);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        base.UseWieldedThing(targetGridPos);
    }

    public void MoveThingTo(Thing thing, GridType targetGridType, IntVector targetGridPos, bool dontRequireAction = false, bool wieldIfPossible = false)
    {
        if (IsDead) 
            return;

        if (IsAiming)
            StopAiming();

        if (!Acting.IsActionReady && !dontRequireAction)
        {
            QueuedAction = new MoveThingAction(thing, targetGridType, targetGridPos, thing.ContainingGridType, thing.GridPos, wieldIfPossible);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        var sourceGridType = thing.ContainingGridType;
        Sandbox.Diagnostics.Assert.True(sourceGridType != targetGridType);

        var owningPlayer = thing.ContainingGridManager.OwningPlayer;

        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: sourceGridType);
        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: targetGridType);

        if(targetGridType == GridType.Arena || sourceGridType == GridType.Arena)
        {
            RoguemojiGame.Instance.RefreshNearbyPanelClient(To.Single(this));
            RoguemojiGame.Instance.FlickerNearbyPanelCellsClient(To.Single(this));
        }

        thing.ContainingGridManager?.RemoveThing(thing);
        var targetGridManager = GetGridManager(targetGridType);

        Thing targetThing = targetGridType != GridType.Arena ? targetGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault() : null;
        IntVector sourceGridPos = thing.GridPos;

        targetGridManager.AddThing(thing);
        thing.SetGridPos(targetGridPos);

        if (targetThing != null)
        {
            if(sourceGridType == GridType.Equipment && targetGridType == GridType.Inventory && !targetThing.HasFlag(ThingFlags.Equipment))
            {
                if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    SwapGridThingPos(targetThing, GridType.Inventory, emptyGridPos);
                else
                    MoveThingTo(targetThing, GridType.Arena, GridPos, dontRequireAction: true);
            }
            else
            {
                MoveThingTo(targetThing, sourceGridType, sourceGridPos, dontRequireAction: true);
            }
        }

        if (sourceGridType == GridType.Equipment && owningPlayer != null)
            owningPlayer.UnequipThing(thing);

        if (targetGridType == GridType.Arena)
        {
            if(thing == WieldedThing)
                WieldThing(null, dontRequireAction: true);

            thing.CurrentLevelId = CurrentLevelId;
        }
            
        if (targetGridType == GridType.Inventory && wieldIfPossible && WieldedThing == null && !thing.HasFlag(ThingFlags.Equipment))
            WieldThing(thing, dontRequireAction: true);

        if (targetGridType == GridType.Equipment)
            targetGridManager.OwningPlayer.EquipThing(thing);

        if (!dontRequireAction)
            Acting.PerformedAction();
    }

    public void WieldThing(Thing thing, bool dontRequireAction = false)
    {
        if (IsDead || WieldedThing == thing)
            return;

        if (IsAiming)
            StopAiming();

        if (!Acting.IsActionReady && !dontRequireAction)
        {
            QueuedAction = new WieldThingAction(thing);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        base.WieldThing(thing);

        if (!dontRequireAction)
            Acting.PerformedAction();
    }

    public void SwapGridThingPos(Thing thing, GridType gridType, IntVector targetGridPos)
    {
        if (IsDead || gridType == GridType.Arena)
            return;

        var gridManager = GetGridManager(gridType);
        Thing targetThing = gridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        IntVector sourceGridPos = thing.GridPos;

        gridManager.DeregisterGridPos(thing, thing.GridPos);
        thing.SetGridPos(targetGridPos);

        if (targetThing != null)
        {
            gridManager.DeregisterGridPos(targetThing, targetThing.GridPos);
            targetThing.SetGridPos(sourceGridPos);
        }

        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType);
    }

    public void GridCellClicked(IntVector gridPos, GridType gridType, bool rightClick, bool shift, bool doubleClick, bool visible = true)
    {
        if (gridType == GridType.Arena)
        {
            //RoguemojiGame.Instance.AddFloater("💢", gridPos, 1.5f, CurrentLevelId, new Vector2(15f, -8f), new Vector2(15, -10f), "", requireSight: true, EasingType.ExpoIn, 0.1f, 0.75f, parent: this);
            //RoguemojiGame.Instance.AddFloater("❗️", gridPos, 1f, CurrentLevelId, new Vector2(0f, -25f), new Vector2(0, -35f), "", requireSight: true, EasingType.Linear, 0.1f, 1.1f, parent: this);
            //RoguemojiGame.Instance.AddFloater("❔", gridPos, 1.1f, CurrentLevelId, new Vector2(0f, -29f), new Vector2(0, -33f), "", requireSight: true, EasingType.SineIn, 0.25f, 1f, parent: this);

            var level = RoguemojiGame.Instance.GetLevel(CurrentLevelId);
            var thing = level.GridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!visible && thing != null)
                return;

            if (!rightClick)
                SelectThing(thing);
        }
        else if (gridType == GridType.Inventory)
        {
            var thing = InventoryGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (thing != null && (doubleClick || rightClick))
            {
                if (thing.HasFlag(ThingFlags.Equipment))
                    TryEquipThing(thing);
                else
                    WieldThing(thing);
            }
            else if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingTo(thing, GridType.Arena, GridPos);
                else
                    SelectThing(thing);
            }
        }
        else if (gridType == GridType.Equipment)
        {
            var thing = EquipmentGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingTo(thing, GridType.Arena, GridPos);
                else
                    SelectThing(thing);
            }
            else
            {
                if (thing != null && InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    MoveThingTo(thing, GridType.Inventory, emptyGridPos);
            }
        }
    }

    public void NearbyThingClicked(Thing thing, bool rightClick, bool shift, bool doubleClick)
    {
        if (shift || rightClick || doubleClick)
        {
            if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                MoveThingTo(thing, GridType.Inventory, emptyGridPos, wieldIfPossible: true);
        }
        else
        {
            SelectThing(thing);
        }
    }

    public void InventoryThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos, bool draggedWieldedThing)
    {
        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby)// || destinationPanelType == PanelType.None)
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else if (destinationPanelType == PanelType.InventoryGrid)
        {
            if(draggedWieldedThing)
            {
                if (thing.GridPos.Equals(targetGridPos))
                {
                    WieldThing(null);
                }
                else
                {
                    var targetThing = InventoryGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
                    WieldThing(targetThing == null || targetThing.HasFlag(ThingFlags.Equipment) ? null : targetThing);
                    SwapGridThingPos(thing, GridType.Inventory, targetGridPos);
                }
            }
            else
            {
                if (!thing.GridPos.Equals(targetGridPos))
                    SwapGridThingPos(thing, GridType.Inventory, targetGridPos);
                else
                    SelectThing(thing);
            }
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.HasFlag(ThingFlags.Equipment))
                return;

            MoveThingTo(thing, GridType.Equipment, targetGridPos);
        }
        else if (destinationPanelType == PanelType.Wielding)
        {
            if (WieldedThing == thing)
                SelectThing(thing);
            else if (!thing.HasFlag(ThingFlags.Equipment))
                WieldThing(thing);
        }
        else if (destinationPanelType == PanelType.CharPortrait)
        {
            if (thing.HasFlag(ThingFlags.Equipment))
                TryEquipThing(thing);
            else
                WieldThing(thing);
        }
        else if (destinationPanelType == PanelType.Info)
        {
            SelectThing(thing);
        }
    }

    public void EquipmentThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos)
    {
        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby)// || destinationPanelType == PanelType.None)
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else if (destinationPanelType == PanelType.InventoryGrid)
        {
            MoveThingTo(thing, GridType.Inventory, targetGridPos);
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.GridPos.Equals(targetGridPos))
                SwapGridThingPos(thing, GridType.Equipment, targetGridPos);
            else
                SelectThing(thing);
        }
        else if (destinationPanelType == PanelType.Info)
        {
            SelectThing(thing);
        }
    }

    public void NearbyThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos)
    {
        // dont allow dragging nearby thing from different cells, or if the thing has been picked up by someone else
        if (!GridPos.Equals(thing.GridPos) || thing.ContainingGridType == GridType.Inventory)
            return;

        if (destinationPanelType == PanelType.InventoryGrid)
        {
            MoveThingTo(thing, GridType.Inventory, targetGridPos);
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.HasFlag(ThingFlags.Equipment))
                return;

            MoveThingTo(thing, GridType.Equipment, targetGridPos);
        }
        else if (destinationPanelType == PanelType.Nearby)
        {
            SelectThing(thing);
        }
        else if (destinationPanelType == PanelType.Wielding)
        {
            if (thing.HasFlag(ThingFlags.Equipment))
                return;

            if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            {
                MoveThingTo(thing, GridType.Inventory, emptyGridPos);
                WieldThing(thing, dontRequireAction: true);
            }
        }
        else if (destinationPanelType == PanelType.CharPortrait)
        {
            // todo
        }
        else if (destinationPanelType == PanelType.Info)
        {
            SelectThing(thing);
        }
    }

    public void WieldingClicked(bool rightClick, bool shift)
    {
        if (WieldedThing == null)
            return;

        if (rightClick)
            WieldThing(null);
        else if (shift)
            MoveThingTo(WieldedThing, GridType.Arena, GridPos);
        else
            SelectThing(WieldedThing);
    }

    public void PlayerIconClicked(bool rightClick, bool shift)
    {
        SelectThing(this);
    }

    public void CharacterHotkeyPressed()
    {
        SelectThing(this);
    }

    public GridManager GetGridManager(GridType gridType)
    {
        switch (gridType)
        {
            case GridType.Arena:
                return ContainingGridManager;
            case GridType.Inventory:
                return InventoryGridManager;
            case GridType.Equipment:
                return EquipmentGridManager;
        }

        return null;
    }

    public override void OnChangedStat(StatType statType, int changeCurrent, int changeMin, int changeMax)
    {
        if (statType == StatType.Sight)
        {
            RefreshVisibility(To.Single(this));
        }

        base.OnChangedStat(statType, changeCurrent, changeMin, changeMax);
    }

    public override void FinishInitStats()
    {
        base.FinishInitStats();

        var mana = GetStat(StatType.Mana);
        if (mana != null)
            mana.CurrentValue = mana.MaxValue;

        var energy = GetStat(StatType.Energy);
        if (energy != null)
            energy.CurrentValue = energy.MaxValue;
    }

    public void StartAimingThrow()
    {
        if (WieldedThing == null)
            return;

        StartAiming(AimingSource.Throwing, AimingType.Direction);
        //RoguemojiGame.Instance.LogMessageClient(To.Single(this), "Press WASD to throw or F to cancel", playerNum: 0);
    }

    public void StartAiming(AimingSource aimingSource, AimingType aimingType)
    {
        if (QueuedAction != null)
        {
            QueuedAction = null;
            QueuedActionName = "";
        }

        IsAiming = true;
        AimingSource = aimingSource;
        AimingType = aimingType;

        if(aimingType == AimingType.Direction)
        {
            AimDirectionClient(To.Single(this));
        }
        else if(aimingSource == AimingSource.UsingWieldedItem && aimingType == AimingType.TargetCell)
        {
            if(WieldedThing != null)
                AimTargetCellsClient(To.Single(this), WieldedThing.NetworkIdent);
            else
                StopAiming();
        }
    }

    [ClientRpc]
    public void AimDirectionClient()
    {
        AimingCells.Clear();

        foreach (var dir in GridManager.GetCardinalDirections())
        {
            IntVector gridPos = GridPos + GridManager.GetIntVectorForDirection(dir);
            AimingCells.Add(gridPos);
        }
    }

    public void RefreshWieldedThingTargetAiming()
    {
        if (WieldedThing == null || !IsAiming || AimingSource != AimingSource.UsingWieldedItem || AimingType != AimingType.TargetCell)
            return;

        AimTargetCellsClient(To.Single(this), WieldedThing.NetworkIdent);
    }

    [ClientRpc]
    public void AimTargetCellsClient(int networkIdent)
    {
        AimingCells.Clear();

        Thing usedThing = FindByIndex(networkIdent) as Thing;
        var aimingCells = usedThing.GetAimingTargetCellsClient();

        foreach (IntVector gridPos in aimingCells)
            AimingCells.Add(gridPos);
    }

    public void ConfirmAiming(Direction direction)
    {
        if (!IsAiming || AimingType != AimingType.Direction)
            return;

        StopAiming();

        if (IsConfused && Game.Random.Int(0, 2) == 0)
            direction = GridManager.GetRandomDirection(cardinalOnly: false);

        if (AimingSource == AimingSource.Throwing)
            ThrowWieldedThing(direction);
        else if (AimingSource == AimingSource.UsingWieldedItem)
            UseWieldedThing(direction);
    }

    public void ConfirmAiming(IntVector gridPos)
    {
        if (!IsAiming)
            return;

        if(AimingType == AimingType.Direction)
        {
            var direction = GridManager.GetDirectionForIntVector(gridPos - GridPos);
            ConfirmAiming(direction);
        }
        else if(AimingType == AimingType.TargetCell && AimingSource == AimingSource.UsingWieldedItem)
        {
            UseWieldedThing(gridPos);
            StopAiming();
        }
    }

    public void StopAiming()
    {
        IsAiming = false;
    }

    public override void OnChangedGridPos()
    {
        base.OnChangedGridPos();

        RefreshVisibility(To.Single(this));
        ContainingGridManager.PlayerChangedGridPos(this);

        if(IsAiming)
            RefreshWieldedThingTargetAiming();
    }

    public bool IsInInventory(Thing thing)
    {
        return thing.ContainingGridManager.GridType == GridType.Inventory && thing.ContainingGridManager.OwningPlayer == this;
    }

    public void IdentifyScroll(ScrollType scrollType)
    {
        if (!IdentifiedScrollTypes.Contains(scrollType))
        {
            IdentifiedScrollTypes.Add(scrollType);
            RoguemojiGame.Instance.AddFloater("💡", GridPos, 1f, CurrentLevelId, new Vector2(-1f, -10f), new Vector2(-1f, -30f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.5f, scale: 0.8f, opacity: 0.66f, parent: this);
            RoguemojiGame.Instance.LogMessageClient(To.Single(this), $"💡Identified 📜{RoguemojiGame.Instance.GetUnidentifiedScrollIcon(scrollType)} as {Scroll.GetDisplayName(scrollType)}{Scroll.GetChatDisplayIcons(scrollType)}", playerNum: 0);
        }
    }

    public bool IsScrollTypeIdentified(ScrollType scrollType)
    {
        return IdentifiedScrollTypes.Contains(scrollType);
    }

    public void IdentifyPotion(PotionType potionType)
    {
        if (!IdentifiedPotionTypes.Contains(potionType))
        {
            IdentifiedPotionTypes.Add(potionType);
            RoguemojiGame.Instance.AddFloater("💡", GridPos, 1f, CurrentLevelId, new Vector2(0f, -10f), new Vector2(0, -30f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.5f, scale: 0.8f, opacity: 0.66f, parent: this);
            RoguemojiGame.Instance.LogMessageClient(To.Single(this), $"💡Identified 🧉{RoguemojiGame.Instance.GetUnidentifiedPotionIcon(potionType)} as {Potion.GetDisplayName(potionType)}{Potion.GetChatDisplayIcons(potionType)}", playerNum: 0);
        }
    }

    public bool IsPotionTypeIdentified(PotionType potionType)
    {
        return true;
        return IdentifiedPotionTypes.Contains(potionType);
    }
}

public interface IQueuedAction
{
    public void Execute(RoguemojiPlayer player);
}

public class TryMoveAction : IQueuedAction
{
    public Direction Direction { get; set; }

    public TryMoveAction(Direction direction)
    {
        Direction = direction;
    }

    public void Execute(RoguemojiPlayer player)
    {
        player.TryMove(Direction, shouldQueueAction: false);
    }

    public override string ToString()
    {
        return $"TryMove {Direction}";
    }
}

public class MoveThingAction : IQueuedAction
{
    public Thing Thing { get; set; }
    public GridType TargetGridType { get; set; }
    public IntVector TargetGridPos { get; set; }
    public GridType SourceGridType { get; set; }
    public IntVector SourceGridPos { get; set; }
    public bool WieldIfPossible { get; set; }

    public MoveThingAction(Thing thing, GridType targetGridType, IntVector targetGridPos, GridType sourceGridType, IntVector sourceGridPos, bool wieldIfPossible = false)
    {
        Thing = thing;
        TargetGridType = targetGridType;
        TargetGridPos = targetGridPos;
        SourceGridType = sourceGridType;
        SourceGridPos = sourceGridPos;
        WieldIfPossible = wieldIfPossible;
    }

    public void Execute(RoguemojiPlayer player)
    {
        if (Thing.ContainingGridType != SourceGridType || !Thing.GridPos.Equals(SourceGridPos))
            return;

        player.MoveThingTo(Thing, TargetGridType, TargetGridPos, wieldIfPossible: WieldIfPossible);
    }

    public override string ToString()
    {
        return $"Move {Thing.DisplayName} -> {TargetGridType} in {TargetGridPos}";
    }
}

public class WieldThingAction : IQueuedAction
{
    public Thing Thing { get; set; }

    public WieldThingAction(Thing thing)
    {
        Thing = thing;
    }

    public void Execute(RoguemojiPlayer player)
    {
        if (Thing != null && Thing.ContainingGridManager.OwningPlayer != player)
            return;

        player.WieldThing(Thing);
    }

    public override string ToString()
    {
        return $"Wield {Thing?.DisplayName ?? null}";
    }
}

public class ThrowThingAction : IQueuedAction
{
    public Thing Thing { get; set; }
    public Direction Direction { get; set; }

    public ThrowThingAction(Thing thing, Direction direction)
    {
        Thing = thing;
        Direction = direction;
    }

    public void Execute(RoguemojiPlayer player)
    {
        if (Thing == null || Thing != player.WieldedThing)
            return;

        player.ThrowWieldedThing(Direction);
    }

    public override string ToString()
    {
        return $"Throw {Thing?.DisplayName ?? null} {Direction}";
    }
}

public class UseWieldedThingAction : IQueuedAction
{
    public void Execute(RoguemojiPlayer player)
    {
        player.UseWieldedThing();
    }

    public override string ToString()
    {
        return $"UseWieldedThing";
    }
}

public class UseWieldedThingDirectionAction : IQueuedAction
{
    public Direction Direction { get; set; }

    public UseWieldedThingDirectionAction(Direction direction)
    {
        Direction = direction;
    }

    public void Execute(RoguemojiPlayer player)
    {
        player.UseWieldedThing(Direction);
    }

    public override string ToString()
    {
        return $"UseWieldedThing {Direction}";
    }
}

public class UseWieldedThingTargetAction : IQueuedAction
{
    public IntVector TargetGridPos { get; set; }

    public UseWieldedThingTargetAction(IntVector targetGridPos)
    {
        TargetGridPos = targetGridPos;
    }

    public void Execute(RoguemojiPlayer player)
    {
        player.UseWieldedThing(TargetGridPos);
    }

    public override string ToString()
    {
        return $"UseWieldedThing {TargetGridPos}";
    }
}
