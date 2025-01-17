﻿using Sandbox;

namespace Roguemoji;

public partial class Level : Component
{
	[Net] public GridManager GridManager { get; private set; }

    [Net] public LevelId LevelId { get; private set; }

    [Net] public string LevelName { get; private set; }
    [Net] public string Icon { get; private set; }

    public LevelData LevelData { get; private set; }

    [Net] public SurfaceType SurfaceType { get; private set; }
	
    public void Init(LevelId levelId)
    {
        LevelId = levelId;

        LevelData = FileSystem.Mounted.ReadJsonCustom<LevelData>($"levels/{levelId}.json");

        if(LevelData == null)
        {
            Log.Error($"Level {levelId} not loaded!");
        }
	
		// TODO: ENTITY SYSTEM
		var gridManagerGO = RoguemojiGame.SpawnGameObject();
		GridManager = gridManagerGO.Components.Create<GridManager>();

        GridManager.Init(LevelData.Width, LevelData.Height);
        GridManager.GridType = GridType.Arena;
        GridManager.LevelId = LevelId;

        LevelName = LevelData.Name;
        Icon = LevelData.Icon;
        SurfaceType = LevelData.SurfaceType;

		SpawnStartingThings();
	}

    public void Update(float dt)
    {
        GridManager.Update(dt);
    }

    public void UpdateClient(float dt)
    {
        GridManager.UpdateClient(dt);
    }

    public void Restart()
    {
        GridManager.Restart();
        SpawnStartingThings();
    }

    public static string GetLevelBgColor(SurfaceType surfaceType, bool odd)
    {
        switch (surfaceType)
        {
            case SurfaceType.Grass: return odd ? "#082b0f" : "#07270e";
            default: return "#000000";
        }
    }

    void SpawnStartingThings()
    {
        if (LevelData.Things != null)
        {
            foreach (var pair in LevelData.Things)
            {
                var type = TypeLibrary.GetType<Thing>(pair.Key);

                foreach (var gridPos in pair.Value)
                    GridManager.SpawnThing(type, gridPos);
            }
        }

        if (LevelData.RandomThings != null)
        {
            foreach (var pair in LevelData.RandomThings)
            {
                var type = TypeLibrary.GetType<Thing>(pair.Key);

                for (int i = 0; i < pair.Value; i++)
                {
                    if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                        GridManager.SpawnThing(type, gridPos);
                }
            }
        }
    }
}
