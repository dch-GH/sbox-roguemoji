using System;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Diagnostics;
using Sandbox.Network;
using System.Threading.Tasks;
using System.Threading.Channels;


namespace Roguemoji;

public class PanelFlickerData
{
    public Panel panel;
    public int numFrames;

    public PanelFlickerData(Panel _panel)
    {
        panel = _panel;
        numFrames = 0;
    }
}

public enum LevelId
{
    None,
    Forest1, Forest2, Forest3,
    Test0,
}

public partial class RoguemojiGame : Sandbox.Component, Component.INetworkListener
{
    public static RoguemojiGame Instance { get; private set; }
    public static Dictionary<Guid, Client> Clients = new();

    public static int PlayerNum { get; set; }
    public static uint ThingId { get; set; }

    [Property] public GameObject _hudPrefab { get; set; }
    [Property] public Level _level { get; set; }
    public Hud Hud { get; set; }

    public const int CellSize = 42;

    public const int ArenaPanelWidth = 25;
    public const int ArenaPanelHeight = 19;

    // todo: move to player
    public const int InventoryWidth = 22; //5;
    public const int InventoryHeight = 6;
    public const int EquipmentWidth = 4;
    public const int EquipmentHeight = 2;

    public int LevelWidth { get; set; }
    public int LevelHeight { get; set; }

    public record struct LogData(string text, int playerNum);
    public Queue<LogData> LogMessageQueue = new Queue<LogData>();
    public Queue<LogData> ChatMessageQueue = new Queue<LogData>();

    public List<RoguemojiPlayer> Players { get; private set; } = new();

	public RoguemojiPlayer LocalPlayer;

    public List<PanelFlickerData> _panelsToFlicker;

    public Dictionary<LevelId, Level> Levels { get; private set; } = new();

    public List<string> UnidentifiedScrollSymbols { get; private set; } = new();
    public List<string> UnidentifiedScrollNames { get; private set; } = new();
    public List<string> UnidentifiedPotionSymbols { get; private set; } = new();
    public List<string> UnidentifiedPotionNames { get; private set; } = new();

    /// <summary>
    /// Create a server (if we're not joining one)
    /// </summary>
    [Property] public bool StartServer { get; set; } = true;

    /// <summary>
    /// The prefab to spawn for the player to control.
    /// </summary>
    [Property] public GameObject PlayerPrefab { get; set; }

    /// <summary>
    /// A list of points to choose from randomly to spawn the player in. If not set, we'll spawn at the
    /// location of the NetworkHelper object.
    /// </summary>
    [Property] public List<GameObject> SpawnPoints { get; set; }
    private RealTimeSince _sinceStart;

    public static GameObject SpawnGameObject()
    {
        return Instance.Scene.CreateObject();
    }

    protected override async Task OnLoad()
    {
        if (Scene.IsEditor)
            return;

        if (StartServer && !GameNetworkSystem.IsActive)
        {
            LoadingScreen.Title = "Creating Lobby";
            await Task.DelayRealtimeSeconds(0.1f);
            GameNetworkSystem.CreateLobby();
        }
    }

    protected override void OnEnabled()
    {
        Instance = this;
        _panelsToFlicker = new List<PanelFlickerData>();
    }

    public void ResetUnidentifiedScrolls()
    {
        UnidentifiedScrollSymbols = new List<string>() { "🈁", "🈂️", "🈷️", "🈯️", "🈹", "🈳", "🈚️", "🈸", "🈴", "🔠", "🔢", "🔣", "🔤", "🈶", "🈺", "🈵", "🆎", "🅰️", "🅱️", "🆑", "🅾️", "🅿️", "🆖", "🆚",
            "➿", "🚾", "⏺️", "♈️", "♉️", "♊️", "♋️", "♌️", "♍️", "♎️", "♏️", "♐️", "♑️", "♒️", "♓️", "⛎", "☦️", "🕉️", "☸️", "☯️", "🕎", };
        UnidentifiedScrollSymbols.Shuffle();

        UnidentifiedScrollNames = new List<string>();
        GenerateUnidentifiedScrollNames();
    }

    public void ResetUnidentifiedPotions()
    {
        UnidentifiedPotionSymbols = new List<string>() { "🉑", "🔘", "🧿", "🌐", "🌓", "🌑", "🌕️", "🌙", "©️", "®️", "㊗️", "㊙️", "⭕️", "Ⓜ️", "🍥", "🉐", }; // 🌒🌔🌖🌘🌗 🔺🔻 ♠️♥️♦️♣️🎱 💿️📀 🌍️🌎️🌏️ 🌚🌝🌞 🔅🔆 ❥
        UnidentifiedPotionSymbols.Shuffle();
        UnidentifiedPotionNames = new List<string>() { "cloudy", "misty", "murky", "sparkling", "fizzy", "bubbly", "smoky", "congealed", "chalky", "radiant", "milky", "thick", "pasty", "glossy", "dull", "dusty", "syrupy", "pungent",
                "viscous", "sludgy", "pale", "filmy", "rusty", "chunky", "creamy", "hazy", "silky", "foggy", "pulpy", "dark", "oily", "opaque", "shiny", "frothy", "wavy" };
        UnidentifiedPotionNames.Shuffle();
    }

    public string GetUnidentifiedScrollIcon(ScrollType scrollType) { return UnidentifiedScrollSymbols[(int)scrollType]; }
    public string GetUnidentifiedScrollName(ScrollType scrollType) { return UnidentifiedScrollNames[(int)scrollType]; }
    public string GetUnidentifiedPotionIcon(PotionType potionType) { return UnidentifiedPotionSymbols[(int)potionType]; }
    public string GetUnidentifiedPotionName(PotionType potionType) { return UnidentifiedPotionNames[(int)potionType]; }

    HashSet<LevelId> _occupiedLevelIds = new HashSet<LevelId>();

    protected override void OnUpdate()
    {
        var dt = Time.Delta;

        foreach (var levelId in GetOccupiedLevels())
        {
            Level level = Levels[levelId];
            level.Update(dt);
        }

        //if ( Hud.MainPanel.LogPanel != null )
        //{
        //	while ( LogMessageQueue.Count > 0 )
        //	{
        //		var data = LogMessageQueue.Dequeue();
        //		Hud.MainPanel.LogPanel.WriteMessage( data.text, data.playerNum );
        //	}
        //}

        //if ( Hud.MainPanel.ChatPanel != null )
        //{
        //	while ( ChatMessageQueue.Count > 0 )
        //	{
        //		var data = ChatMessageQueue.Dequeue();
        //		Hud.MainPanel.ChatPanel.WriteMessage( data.text, data.playerNum );
        //	}
        //}

        for (int i = _panelsToFlicker.Count - 1; i >= 0; i--)
        {
            var data = _panelsToFlicker[i];
            data.numFrames++;

            if (data.numFrames >= 2)
            {
                if (data.panel != null)
                    data.panel.Style.PointerEvents = PointerEvents.All;

                _panelsToFlicker.RemoveAt(i);
            }
        }

        foreach (var levelId in GetOccupiedLevels())
        {
            Level level = Levels[levelId];
            level.UpdateClient(dt);
        }

        //Sound.Listener = new Transform(Vector3.Zero);
    }

    HashSet<LevelId> GetOccupiedLevels()
    {
        _occupiedLevelIds.Clear();
        foreach (RoguemojiPlayer player in Players)
        {
            if (player != null && player.IsValid && player.ControlledThing != null)
                _occupiedLevelIds.Add(player.ControlledThing.CurrentLevelId);
        }

        return _occupiedLevelIds;
    }

    public void OnActive(Connection channel)
    {
        Instance = this;
        Levels = new Dictionary<LevelId, Level>();
        CreateLevel(LevelId.Forest1);

        Players = new List<RoguemojiPlayer>();

        ResetUnidentifiedScrolls();
        ResetUnidentifiedPotions();

        _sinceStart = 0;
        Log.Info($"Player '{channel.DisplayName}' has joined the game");

        if (PlayerPrefab is null)
        {
            Log.Error("player prefab is null");
            return;
        }

        var pawn = PlayerPrefab.Clone(new global::Transform(Vector3.Zero, Rotation.Identity, 1), name: $"Player - {channel.DisplayName}");
        pawn.BreakFromPrefab();
        pawn.Name = $"Player - {channel.DisplayName}";

        var client = pawn.Components.GetOrCreate<Client>();
        client.Connection = channel;
        client.Pawn = pawn;
        client.Pawn.NetworkSpawn(channel);

        if (!Clients.ContainsKey(channel.Id))
        {
            Clients.Add(channel.Id, client);
        }
        var levelId = LevelId.Forest1;
        var level0 = Levels[levelId];

        level0.GridManager.GetRandomEmptyGridPos(out var gridPos);
        var smiley = level0.GridManager.SpawnThing<Smiley>(gridPos);

        var player = pawn.Components.GetOrCreate<RoguemojiPlayer>();
        client.PlayerComponent = player;
        player.PlayerNum = ++PlayerNum;
        player.ControlThing(smiley);
        player.Client = client;

        Game.LocalPlayer ??= player;
		Instance.LocalPlayer = player;
		var hud = _hudPrefab.Clone();
		hud.BreakFromPrefab();

		var hc = hud.Components.Get<HudComponent>();
		Hud = hc.Hud;

		level0.GridManager.AddPlayer(player);

        player.Restart();

        smiley.DisplayName = $"{channel.DisplayName}";
        smiley.Tooltip = $"{channel.DisplayName}";

        Players.Add(player);

        player.RecenterCamera();
        player.RefreshVisibility();

        //player.ControlledThing.AddComponent<COrganizeDebug>();
    }

    public void OnDisconnected(Connection conn)
    {
        var client = Clients[conn.Id];
        var player = client.PlayerComponent;

        var level = Levels[player.ControlledThing.CurrentLevelId];
        level.GridManager.RemoveThing(player.ControlledThing);

        // todo: drop or remove items in player's inventory
        Players.Remove(player);

        // TODO: destroy player pawn?
        //client.Pawn.Destroy();
    }

    public void LogPersonalMessage(RoguemojiPlayer player, string text)
    {
        LogMessageClient(To.Single(player), text, playerNum: 0);
    }

    //[TargetedRPC]
    public void LogMessageClient(To to, string text, int playerNum)
    {
        if (Hud.MainPanel.LogPanel == null)
        {
            LogMessageQueue.Enqueue(new LogData(text, playerNum));
            return;
        }

        Hud.MainPanel.LogPanel.WriteMessage(text, playerNum);
    }

    //[Broadcast]
    public static void ChatMessageCmd(string text, int playerNum)
    {
        RoguemojiGame.Instance.ChatMessageClient(text, playerNum);
    }

    ////[TargetedRPC]
    public void ChatMessageClient(string text, int playerNum)
    {
        if (Hud.MainPanel.ChatPanel == null)
        {
            ChatMessageQueue.Enqueue(new LogData(text, playerNum));
            return;
        }

        Hud.MainPanel.ChatPanel.WriteMessage(text, playerNum);
    }

    ////[Broadcast]
    public static void GridCellClickedCmd(int x, int y, GridType gridType, bool rightClick, bool shift, bool doubleClick, bool visible = true)
    {
        //var player =  RoguemojiGame.Instance.LocalPlayer;
        var player = RoguemojiGame.Instance.LocalPlayer;
        player.GridCellClicked(new IntVector(x, y), gridType, rightClick, shift, doubleClick, visible);
    }

    //[Broadcast]
    public static void ClickedNothing()
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        player.ClickedNothing();
    }

    //[Broadcast]
    public static void ConfirmAimingCmd(GridType gridType, int x, int y)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        player.ConfirmAiming(gridType, new IntVector(x, y));
    }

    //[Broadcast]
    public static void StopAimingCmd()
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        player.StopAiming();
    }

    ////[TargetedRPC]
    public void RefreshGridPanelClient(To to, GridType gridType)
    {
        GridPanel panel = Hud.Instance.GetGridPanel(gridType);

        if (panel != null)
            panel.StateHasChanged();
    }

    //[Broadcast]
    public static void NearbyThingClickedCmd(Guid networkIdent, bool rightClick, bool shift, bool doubleClick)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        Thing thing = player.FindByIndex(networkIdent) as Thing;

        if (thing.ContainingGridType != GridType.Arena)
        {
            Log.Info("Trying to pick up " + (thing?.Name ?? "null") + " but it's no longer on the ground!");
            return;
        }

        player.NearbyThingClicked(thing, rightClick, shift, doubleClick);
    }

    //[TargetedRPC]
    public void RefreshNearbyPanelClient(To to)
    {
        Hud.Instance.MainPanel.NearbyPanel?.StateHasChanged();
    }

    //[TargetedRPC]
    public void FlickerNearbyPanelCellsClient(To to)
    {
        var nearbyPanel = Hud.Instance.MainPanel?.NearbyPanel;
        if (nearbyPanel == null)
            return;

        nearbyPanel.FlickerCells();
    }

    //[TargetedRPC]
    public void FlickerWieldingPanel()
    {
        var wieldingPanel = Hud.Instance.MainPanel.CharacterPanel.WieldingPanel;
        FlickerPanel(wieldingPanel);
    }

    public void FlickerPanel(Panel panel)
    {
        Game.AssertClient();

        if (panel == null)
            return;

        panel.Style.PointerEvents = PointerEvents.None;
        _panelsToFlicker.Add(new PanelFlickerData(panel));
    }

    //[Broadcast]
    public static void InventoryThingDraggedCmd(Guid networkIdent, PanelType destinationPanelType, int x, int y, bool wieldedThingDragged)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        Thing thing = player.FindByIndex(networkIdent) as Thing;
        player.InventoryThingDragged(thing, destinationPanelType, new IntVector(x, y), wieldedThingDragged);
    }

    //[Broadcast]
    public static void EquipmentThingDraggedCmd(Guid networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        Thing thing = player.FindByIndex(networkIdent) as Thing;
        player.EquipmentThingDragged(thing, destinationPanelType, new IntVector(x, y));
    }

    //[Broadcast]
    public static void NearbyThingDraggedCmd(Guid networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        Thing thing = player.FindByIndex(networkIdent) as Thing;
        player.NearbyThingDragged(thing, destinationPanelType, new IntVector(x, y));
    }

    //[Broadcast]
    public static void WieldingClickedCmd(bool rightClick, bool shift)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        player.WieldingClicked(rightClick, shift);
    }

    //[Broadcast]
    public static void PlayerIconClickedCmd(bool rightClick, bool shift)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        player.PlayerIconClicked(rightClick, shift);
    }

    public RoguemojiPlayer GetClosestPlayer(IntVector gridPos)
    {
        int closestDistance = int.MaxValue;
        RoguemojiPlayer closestPlayer = null;

        foreach (var player in Players)
        {
            if (!player.IsValid)
                continue;

            int dist = (player.ControlledThing.GridPos - gridPos).ManhattanLength;
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public void Restart()
    {
        ResetUnidentifiedScrolls();
        ResetUnidentifiedPotions();

        foreach (var pair in Levels)
            pair.Value.Restart();

        foreach (RoguemojiPlayer player in Players)
        {
            //Log.Info($"Restart - player: {player.PlayerNum}");

            var levelId = LevelId.Forest1;
            //var levelId = LevelId.Test0;
            var level0 = Levels[levelId];
            level0.GridManager.GetRandomEmptyGridPos(out var gridPos);
            var smiley = level0.GridManager.SpawnThing<Smiley>(gridPos);
            player.ControlThing(smiley);

            level0.GridManager.AddPlayer(player);

            player.Restart();
            player.RestartClient();
            //player.RestartClient(To.Everyone);

            player.RecenterCamera();
            player.RefreshVisibility();
            ResetHudClient(To.Single(player));

            //ChangeThingLevel(smiley, LevelId.Forest0);
            //ChangeThingLevel(player, LevelId.Test0);

            //player.RecenterCamera();
            //player.RefreshVisibility();
            player.ControlledThing.AddComponent<COrganizeDebug>();
        }

        Log.Info($"# Entities: {Entity.All.Count()}");
    }

    //[Broadcast]
    public void ResetHudClient(To to)
    {
        Hud.Restart();
    }

    public void ChangeThingLevel(Thing thing, LevelId levelId, bool shouldAnimateFall = false)
    {
        if (thing.CurrentLevelId != LevelId.None)
        {
            var oldLevel = Levels[thing.CurrentLevelId];
            oldLevel.GridManager.RemoveThing(thing);
        }

        var level = Levels.ContainsKey(levelId) ? Levels[levelId] : CreateLevel(levelId);
        var gridManager = level.GridManager;

        gridManager.AddThing(thing);
        thing.CurrentLevelId = levelId;

        gridManager.GetRandomEmptyGridPos(out var gridPos);
        thing.SetGridPos(gridPos, setLastGridPosSame: true);

        if (thing.Brain is RoguemojiPlayer player)
        {
            player.RecenterCamera();
            ResetHudClient(To.Single(player));
        }
    }

    Level CreateLevel(LevelId levelId)
    {
        _level.Init(levelId);
        Levels.Add(levelId, _level);

        return _level;
    }

    public Level GetLevel(LevelId levelId)
    {
        if (Levels.ContainsKey(levelId))
            return Levels[levelId];

        return null;
    }

    public T SpawnThing<T>(LevelId levelId) where T : Thing
    {
        Game.AssertServer();

        var thing = TypeLibrary.GetType(typeof(T)).Create<T>();
        thing.CurrentLevelId = levelId;

        thing.OnSpawned();

        return thing;
    }

    public void RevealScroll(ScrollType scrollType, IntVector gridPos, LevelId levelId)
    {
        foreach (var player in Players)
        {
            if (player.ControlledThing.CurrentLevelId == levelId)
                RevealScrollClient(To.Single(player), scrollType, gridPos);
        }
    }

    //[TargetedRPC]
    public void RevealScrollClient(To to, ScrollType scrollType, IntVector gridPos)
    {
        var player = LocalPlayer;
        if (player.IsCellVisible(gridPos) && !player.IsScrollTypeIdentified(scrollType))
        {
            RevealScrollCmd(scrollType);
        }
    }

    //[Broadcast]
    public static void RevealScrollCmd(ScrollType scrollType)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        player.IdentifyScroll(scrollType);
    }

    public void RevealPotion(PotionType potionType, IntVector gridPos, LevelId levelId)
    {
        foreach (var player in Players)
        {
            if (player.ControlledThing.CurrentLevelId == levelId)
                RevealPotionClient(To.Single(player), potionType, gridPos);
        }
    }

    //[TargetedRPC]
    public void RevealPotionClient(To to, PotionType potionType, IntVector gridPos)
    {
        var player = LocalPlayer;
        if (player.IsCellVisible(gridPos) && !player.IsPotionTypeIdentified(potionType))
        {
            RevealPotionCmd(potionType);
        }
    }

    //[Broadcast]
    public static void RevealPotionCmd(PotionType potionType)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        player.IdentifyPotion(potionType);
    }

    void GenerateUnidentifiedScrollNames()
    {
        UnidentifiedScrollNames.Clear();
        int numScrollTypes = Enum.GetValues(typeof(ScrollType)).Length;
        for (int i = 0; i < numScrollTypes; i++)
        {
            UnidentifiedScrollNames.Add(GenerateUnidentifiedScrollName());
        }
    }

    string GenerateUnidentifiedScrollName()
    {
        StringBuilder sb = new StringBuilder();

        int length = Game.Random.Int(3, 7);
        bool isConsonant = Game.Random.Int(0, 2) == 0;

        for (int i = 0; i < length; i++)
        {
            char c = isConsonant ? GetRandomConsonant() : GetRandomVowel();
            sb.Append(c);

            string validDoubleChars = "BẞÐEFĶŁMꞤOPRSTXZƵΩӘØƎƩꝆƧϷꝐ";
            if (Game.Random.Int(0, 8) == 0 && validDoubleChars.Contains(c) && i > 0 && i < length - 1)
            {
                sb.Append(c);
            }
            else if (Game.Random.Int(0, 8) == 0 && i < length - 1)
            {
                if (c == 'C' || c == 'Ͼ' || c == 'T')
                    sb.Append(("HЋ")[Game.Random.Int(0, 1)]);
                else if (c == 'Q')
                    sb.Append(("UƱ")[Game.Random.Int(0, 1)]);
                if (c == 'B' || c == 'ẞ' || c == 'Ð' || c == 'F' || c == 'G' || c == 'P' || c == 'Ꝑ' || c == 'Ꝁ')
                    sb.Append(("RLꝆY")[Game.Random.Int(0, 3)]);
            }

            isConsonant = !isConsonant;
        }

        return sb.ToString();
    }

    char GetRandomConsonant()
    {
        string consonants = "BẞCÐFGЋJĶŁMꞤPQRSTVWXYZƵΩӜꝀꝆƧϷꝐϾ";
        return consonants[Game.Random.Int(0, consonants.Length - 1)];
    }

    char GetRandomVowel()
    {
        string vowels = "AĀEIOԱӘØƎƩƱꝎꝘ";
        return vowels[Game.Random.Int(0, vowels.Length - 1)];
    }

    public void DebugGridLine(IntVector a, IntVector b, Color color, float time, GridType gridTypeA = GridType.Arena, GridType gridTypeB = GridType.Arena)
    {
        DebugGridLineClient(a, b, color, time, gridTypeA, gridTypeB);
    }

    //[TargetedRPC]
    public void DebugGridLineClient(IntVector a, IntVector b, Color color, float time, GridType gridTypeA = GridType.Arena, GridType gridTypeB = GridType.Arena)
    {
        Hud.Instance.DebugDrawing.GridLine(a, b, color, time, gridTypeA, gridTypeB);
    }

    public void DebugGridCell(IntVector gridPos, Color color, float time, GridType gridType = GridType.Arena)
    {
        DebugGridCellClient(gridPos, color, time, gridType);
    }

    //[TargetedRPC]
    public void DebugGridCellClient(IntVector gridPos, Color color, float time, GridType gridType)
    {
        Hud.Instance.DebugDrawing.GridCell(gridPos, color, time, gridType);
    }
}
