﻿using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public enum PanelType { None, ArenaGrid, InventoryGrid, EquipmentGrid, Wielding, CharPortrait, Log, Nearby, Info, Character, Stats, ChatPanel, Chatbox, LevelLabel };
public enum CursorMode { Point, Pinch, Invalid, Write, PointDown, ThumbsUp, Ok, Check, MiddleFinger, Point2, PointLeft, PointRight }

public struct FloaterData
{
	public string icon;
	public IntVector gridPos;
	public float time;
	public TimeSince timeSinceStart;
	public string text;
	public bool requireSight;
	public bool alwaysShowWhenAdjacent;
	public Vector2 offsetStart;
	public Vector2 offsetEnd;
	public float height;
	public EasingType offsetEasingType;
	public float fadeInTime;
	public float scale;
	public float opacity;
	public float shakeAmount;
	public GridType gridType;
	public Thing parent;

	public FloaterData( string icon, IntVector gridPos, float time, Vector2 offsetStart, Vector2 offsetEnd, float height, string text, bool requireSight, bool alwaysShowWhenAdjacent,
		EasingType offsetEasingType, float fadeInTime, float scale, float opacity, float shakeAmount, GridType gridType, Thing parent )
	{
		this.icon = icon;
		this.gridPos = gridPos;
		this.time = time;
		this.timeSinceStart = 0f;
		this.offsetStart = offsetStart;
		this.offsetEnd = offsetEnd;
		this.height = height;
		this.text = text;
		this.requireSight = requireSight;
		this.alwaysShowWhenAdjacent = alwaysShowWhenAdjacent;
		this.offsetEasingType = offsetEasingType;
		this.fadeInTime = fadeInTime;
		this.scale = scale;
		this.opacity = opacity;
		this.shakeAmount = shakeAmount;
		this.gridType = gridType;
		this.parent = parent;
	}
}

public partial class Hud : Panel
{
	public static Hud Instance { get; private set; }

	public int IndexClicked { get; private set; }

	public GridCell SelectedCell { get; private set; }

	public MainPanel MainPanel { get; private set; }
	public DebugDrawing DebugDrawing { get; set; }
	public CursorDisplay CursorDisplay { get; set; }

	public bool IsDraggingThing { get; set; }
	public bool IsDraggingRightClick { get; set; }
	public Thing DraggedThing { get; set; }
	public Panel DraggedPanel { get; set; }
	public PanelType DraggedPanelType { get; set; }
	public DragIcon DragIcon { get; private set; }
	public TimeSince TimeSinceStartDragging { get; private set; }
	public Vector2 DragStartPosition { get; private set; }
	private IntVector _dragStartPlayerGridPos;
	private IntVector _dragStartGridPos;
	private GridType _dragStartGridType;

	public Vector2 GetMousePos() { return MousePosition / ScaleToScreen; }

	public List<FloaterData> Floaters { get; private set; } = new List<FloaterData>();

	public override void Tick()
	{
		Instance = this;

		var dt = Time.Delta;

		if ( IsDraggingThing && (DraggedThing == null || !DraggedThing.IsValid) )
			StopDragging();

		// if thing we are dragging moved, stop dragging it
		if ( IsDraggingThing && (!_dragStartGridPos.Equals( DraggedThing.GridPos ) || !_dragStartGridType.Equals( DraggedThing.ContainingGridType )) )
			StopDragging();

		// if dragging a nearby thing, stop dragging if you move
		if ( IsDraggingThing && (!_dragStartPlayerGridPos.Equals( RoguemojiGame.Instance.LocalPlayer.ControlledThing.GridPos )) )
		{
			if ( DraggedThing != null && DraggedThing.ContainingGridType == GridType.Arena )
				StopDragging();
		}

		//var player = RoguemojiGame.Instance.LocalPlayer;
		//foreach (var gridPos in player.VisibleCells)
		//{
		//    DebugDrawing.GridCell(gridPos, new Color(0f, 0f, 1f, 1f), 0.02f);
		//}
		//DebugDrawing.Line(GetScreenPosForArenaGridPos(new IntVector(18, 19)), GetScreenPosForArenaGridPos(new IntVector(21, 22)), new Color(0f, 1f, 1f, 0.9f));
		//DebugDrawing.GridLine(new IntVector(19, 19), new IntVector(22, 22), Color.Blue);
		//DebugDrawing.GridCell(new IntVector(20, 20), Color.Red);

		for ( int i = Floaters.Count - 1; i >= 0; i-- )
		{
			var floater = Floaters[i];

			//DebugDrawing.GridLine(IntVector.Zero, floater.gridPos, new Color(0f, 1f, 0f, 0.5f));

			if ( floater.time > 0f && floater.timeSinceStart > floater.time )
				Floaters.RemoveAt( i );
		}
	}

	public void GridCellClicked( IntVector gridPos, GridType gridType, bool rightClick, bool shift, bool doubleClick, bool visible = true )
	{
		RoguemojiGame.GridCellClickedCmd( gridPos.x, gridPos.y, gridType, rightClick, shift, doubleClick, visible );
	}

	public void WieldingClicked( bool rightClick, bool shift )
	{
		RoguemojiGame.WieldingClickedCmd( rightClick, shift );
	}

	public void PlayerIconClicked( bool rightClick, bool shift )
	{
		RoguemojiGame.PlayerIconClickedCmd( rightClick, shift );
	}

	public void UnfocusChatbox()
	{
		MainPanel.Chatbox.Unfocus();
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		if ( IsDraggingThing )
		{
			var player = RoguemojiGame.Instance.LocalPlayer;
			PanelType destinationPanelType = GetContainingPanelType( MousePosition );

			IntVector targetGridPos = IntVector.Zero;
			var gridType = GetGridType( destinationPanelType );
			if ( gridType != GridType.None )
			{
				GridPanel gridPanel = GetGridPanel( gridType );
				targetGridPos = gridPanel.GetGridPos( gridPanel.MousePosition );

				var gridManager = player.GetGridManager( gridType );
				if ( !gridManager.IsGridPosInBounds( targetGridPos ) )
				{
					destinationPanelType = PanelType.None;
				}
			}

			if ( destinationPanelType == PanelType.Chatbox || destinationPanelType == PanelType.ChatPanel )
			{
				string displayIcon = DraggedThing.ChatDisplayIcons;

				if ( player.IsHallucinating )
				{
					displayIcon = GetHallucinationTextThing( DraggedThing, DraggedThing.DisplayIcon.Substring( DraggedThing.CharSkip ), HallucinationTextType.Icon );

					if ( DraggedThing.HasTattoo )
						displayIcon += GetHallucinationTextThing( DraggedThing, DraggedThing.TattooData.Icon, HallucinationTextType.Icon, tattoo: true );
				}

				MainPanel.Chatbox.AddIcon( displayIcon );
				StopDragging();
				return;
			}

			if ( DraggedThing.ContainingGridType == GridType.Inventory )
				RoguemojiGame.InventoryThingDraggedCmd( DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y, wieldedThingDragged: DraggedPanelType == PanelType.Wielding );
			else if ( DraggedThing.ContainingGridType == GridType.Equipment )
				RoguemojiGame.EquipmentThingDraggedCmd( DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y );
			else
				RoguemojiGame.NearbyThingDraggedCmd( DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y );

			StopDragging();
		}
		else
		{
			PanelType panelType = GetContainingPanelType( MousePosition );
			if ( panelType == PanelType.None )
			{
				RoguemojiGame.ClickedNothing();
			}
		}
	}

	public void StartDragging( Thing thing, Panel panel, bool rightClick, PanelType draggedPanelType )
	{
		IsDraggingThing = true;
		IsDraggingRightClick = rightClick;
		DraggedThing = thing;
		DraggedPanel = panel;
		DraggedPanelType = draggedPanelType;
		_dragStartPlayerGridPos = RoguemojiGame.Instance.LocalPlayer.ControlledThing.GridPos;
		_dragStartGridPos = thing.GridPos;
		_dragStartGridType = thing.ContainingGridType;
		TimeSinceStartDragging = 0f;
		DragStartPosition = MousePosition;

		CreateDragIcon( thing );
	}

	public void StopDragging()
	{
		IsDraggingThing = false;
		DraggedThing = null;

		if ( DraggedPanel != null )
		{
			DraggedPanel.SkipTransitions();
			DraggedPanel = null;
		}

		RemoveDragIcon();
	}

	void CreateDragIcon( Thing thing )
	{
		RemoveDragIcon();
		DragIcon = AddChild<DragIcon>();
		DragIcon.Thing = thing;
	}

	void RemoveDragIcon()
	{
		if ( DragIcon != null )
		{
			DragIcon.SkipTransitions();
			DragIcon.Delete();
		}
	}

	public PanelType GetContainingPanelType( Vector2 pos )
	{
		if ( Contains( GetRect( PanelType.LevelLabel ), pos ) ) return PanelType.LevelLabel;
		else if ( Contains( GetRect( PanelType.ArenaGrid ), pos ) ) return PanelType.ArenaGrid;
		else if ( Contains( GetRect( PanelType.InventoryGrid ), pos ) ) return PanelType.InventoryGrid;
		else if ( Contains( GetRect( PanelType.Nearby ), pos ) ) return PanelType.Nearby;
		else if ( Contains( GetRect( PanelType.Log ), pos ) ) return PanelType.Log;
		else if ( Contains( GetRect( PanelType.EquipmentGrid ), pos ) ) return PanelType.EquipmentGrid;
		else if ( Contains( GetRect( PanelType.Wielding ), pos ) ) return PanelType.Wielding;
		else if ( Contains( GetRect( PanelType.CharPortrait ), pos ) ) return PanelType.CharPortrait;
		else if ( Contains( GetRect( PanelType.Character ), pos ) ) return PanelType.Character;
		else if ( Contains( GetRect( PanelType.Info ), pos ) ) return PanelType.Info;
		else if ( Contains( GetRect( PanelType.ChatPanel ), pos ) ) return PanelType.ChatPanel;
		else if ( Contains( GetRect( PanelType.Chatbox ), pos ) ) return PanelType.Chatbox;

		return PanelType.None;
	}

	public Rect GetRect( PanelType panelType )
	{
		return GetPanel( panelType )?.Box.Rect ?? new Rect();
	}

	public Panel GetPanel( PanelType panelType )
	{
		if ( MainPanel is null )
			Log.Info( "MAIN PANEL IS NULL??" );
		switch ( panelType )
		{
			case PanelType.ArenaGrid: return MainPanel.ArenaPanel;
			case PanelType.InventoryGrid: return MainPanel.InventoryPanel;
			case PanelType.Log: return MainPanel.LogPanel;
			case PanelType.Nearby: return MainPanel.NearbyPanel;
			case PanelType.Character: return MainPanel.CharacterPanel;
			case PanelType.EquipmentGrid: return MainPanel.CharacterPanel.EquipmentPanel;
			case PanelType.Wielding: return MainPanel.CharacterPanel.WieldingPanel;
			case PanelType.CharPortrait: return MainPanel.CharacterPanel.CharPortrait;
			case PanelType.Info: return MainPanel.InfoPanel;
			case PanelType.ChatPanel: return MainPanel.ChatPanel;
			case PanelType.Chatbox: return MainPanel.Chatbox;
			case PanelType.LevelLabel: return MainPanel.ArenaPanel.LevelLabel;
		}

		return null;
	}

	public GridPanel GetGridPanel( GridType gridType )
	{
		Game.AssertClient();

		switch ( gridType )
		{
			case GridType.Arena: return MainPanel?.ArenaPanel ?? null;
			case GridType.Inventory: return MainPanel?.InventoryPanel ?? null;
			case GridType.Equipment: return MainPanel?.CharacterPanel?.EquipmentPanel ?? null;
		}

		return null;
	}

	public GridType GetGridType( PanelType panelType )
	{
		switch ( panelType )
		{
			case PanelType.ArenaGrid: return GridType.Arena;
			case PanelType.InventoryGrid: return GridType.Inventory;
			case PanelType.EquipmentGrid: return GridType.Equipment;
		}

		return GridType.None;
	}

	bool Contains( Rect rect, Vector2 point )
	{
		return point.x > rect.Left && point.x < rect.Right && point.y > rect.Top && point.y < rect.Bottom;
	}

	public Vector2 GetScreenPosForGridPos( GridType gridType, IntVector gridPos, bool relative = false )
	{
		var player = RoguemojiGame.Instance.LocalPlayer;
		var panel = GetGridPanel( gridType );

		if ( panel == null )
			return Vector2.Zero;

		if ( gridType == GridType.Arena )
		{
			var offsetGridPos = gridPos - player.CameraGridOffset;

			if ( relative )
				return new Vector2( offsetGridPos.x + 0.5f, offsetGridPos.y + 0.5f ) * (RoguemojiGame.CellSize / ScaleFromScreen) + player.CameraPixelOffset;
			else
				return panel.GetCellScreenPos( offsetGridPos ) + player.CameraPixelOffset;
		}
		else
		{
			if ( relative )
				return new Vector2( gridPos.x + 0.5f, gridPos.y + 0.5f ) * (RoguemojiGame.CellSize / ScaleFromScreen);
			else
				return panel.GetCellScreenPos( gridPos ) + new Vector2( 2f, 2f ) / ScaleFromScreen;
		}
	}

	public static string GetUnusableClass( Thing thing )
	{
		var gridManager = thing.ContainingGridManager;
		if ( thing != null && thing.HasFlag( ThingFlags.Useable ) && gridManager.GridType == GridType.Inventory )
		{
			var owningPlayer = gridManager.OwningPlayer;
			if ( owningPlayer != null && !thing.CanBeUsedBy( owningPlayer.ControlledThing, ignoreResources: true ) )
				return "unusable_item";
		}

		return "";
	}

	public string GetEquipmentHighlightClass( Thing thing )
	{
		if ( GetContainingPanelType( MousePosition ) == PanelType.EquipmentGrid && !IsDraggingThing )
		{
			var gridManager = thing.ContainingGridManager;
			if ( thing.HasFlag( ThingFlags.Equipment ) && gridManager.GridType == GridType.Inventory )
			{
				return "equipment_item_highlight";
			}
		}

		return "";
	}

	public static string GetHallucinationTextThing( Thing thing, string input, HallucinationTextType textType, bool tattoo = false )
	{
		var keyString = tattoo ? thing.TattooData.Icon : thing.DisplayIcon.Substring( thing.CharSkip );

		var player = RoguemojiGame.Instance.LocalPlayer;
		if ( player.IsHallucinating )
		{
			return Globals.GetHallucinationText( keyString, player.HallucinatingSeed, textType );
		}

		return input;
	}

	public static string GetHallucinationTextStr( string str, HallucinationTextType textType )
	{
		var player = RoguemojiGame.Instance.LocalPlayer;
		if ( player.IsHallucinating )
		{
			return Globals.GetHallucinationText( str, player.HallucinatingSeed, textType );
		}

		return str;
	}

	public static string GetHallucinationTextKeyStr( string keyString, string str, HallucinationTextType textType )
	{
		var player = RoguemojiGame.Instance.LocalPlayer;
		if ( player.IsHallucinating )
		{
			return Globals.GetHallucinationText( keyString, player.HallucinatingSeed, textType );
		}

		return str;
	}

	public static string GetTattooIcon( Thing thing )
	{
		var player = RoguemojiGame.Instance.LocalPlayer;

		if ( thing is Scroll scroll )
		{
			if ( !player.IsScrollTypeIdentified( scroll.ScrollType ) )
				return RoguemojiGame.Instance.GetUnidentifiedScrollIcon( scroll.ScrollType );
		}
		else if ( thing is Potion potion )
		{
			if ( !player.IsPotionTypeIdentified( potion.PotionType ) )
				return RoguemojiGame.Instance.GetUnidentifiedPotionIcon( potion.PotionType );
		}

		return thing.TattooData.Icon;
	}

	public static string GetTooltip( Thing thing )
	{
		var player = RoguemojiGame.Instance.LocalPlayer;

		var str = thing.Tooltip;

		if ( thing is Scroll scroll )
		{
			if ( !player.IsScrollTypeIdentified( scroll.ScrollType ) )
				str = $"A scroll of {RoguemojiGame.Instance.GetUnidentifiedScrollName( scroll.ScrollType )}";
		}
		else if ( thing is Potion potion )
		{
			if ( !player.IsPotionTypeIdentified( potion.PotionType ) )
			{
				var potionName = RoguemojiGame.Instance.GetUnidentifiedPotionName( potion.PotionType );
				str = $"{(StartsWithVowel( potionName ) ? "An" : "A")} {potionName} potion";
			}
		}

		return str;
	}

	public static string GetConfusedText( string text )
	{
		var player = RoguemojiGame.Instance.LocalPlayer;
		var str = text;

		if ( player.ConfusionSeed > 0 )
			str = Utils.MakeConfused( str, player.ConfusionSeed );

		return str;
	}

	public static bool StartsWithVowel( string name )
	{
		return "aeiou".IndexOf( name[0].ToString(), StringComparison.InvariantCultureIgnoreCase ) >= 0;
	}

	public void Restart()
	{
		Floaters.Clear();
		StopDragging();
		MainPanel.Chatbox.Restart();
		DebugDrawing.Restart();
	}

	public static float GetOpacity( Thing thing )
	{
		return thing.Opacity * (thing.GetStatClamped( StatType.Invisible ) > 0 ? 0.4f : 1f);
	}

	public static string GetBrightness( Thing thing )
	{
		return "";

		//var color = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 0f, 0f, 0.5f), Utils.Map(thing.IgnitionAmount, 0, Globals.IGNITION_MAX, 0f, 1f, EasingType.QuintIn));
		//return thing.IgnitionAmount > 0 && thing.IgnitionAmount < Globals.IGNITION_MAX
		//    ? $"background-color: {color.Hex}; border-radius: 10px;"
		//    : $"background-color: #00000000; border-radius: 0px;";

		//var color = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 0f, 0f, 1f), Utils.Map(thing.IgnitionAmount, 0, Globals.IGNITION_MAX, 0f, 1f, EasingType.QuintIn));
		//return thing.IgnitionAmount > 0
		//    ? $"text-decoration: overline 4px {color.Hex} wavy; text-decoration-skip-ink: none;"
		//    : "text-decoration: 0px;";

		//return thing.IgnitionAmount > 0 ? $"filter: brightness({Utils.Map(thing.IgnitionAmount, 0, Globals.IGNITION_MAX, 1.0f, Globals.IGNITION_BRIGHTNESS_MAX, EasingType.QuintIn)});" : "filter: none;"; 
	}
}
