﻿using System;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Roguemoji;

public abstract class GridPanel : Panel
{
    public virtual int GridWidth => 0;
    public virtual int GridHeight => 0;

    public string GetSelectedIndexString()
    {
        if (Hud.Instance.SelectedCell != null)
            return Hud.Instance.SelectedCell.GridIndex.ToString();
        else
            return "None";
    }

    public IntVector GetGridPos(int index)
    {
        return new IntVector(index % GridWidth, MathX.FloorToInt((float)index / (float)GridWidth));
    }
    public int GetIndex(IntVector gridPos)
    {
        return gridPos.y * GridWidth + gridPos.x;
    }

    public Vector2 GetCellScreenPos(IntVector gridPos)
    {
        return PanelPositionToScreenPosition(new Vector2(gridPos.x + 0.5f, gridPos.y + 0.5f) * (RoguemojiGame.CellSize / ScaleFromScreen));
    }

    public IntVector GetGridPos(Vector2 screenPos)
    {
        float cellSize = RoguemojiGame.CellSize / ScaleFromScreen;
        return new IntVector(MathX.FloorToInt(screenPos.x / cellSize), MathX.FloorToInt(screenPos.y / cellSize));
    }

    protected virtual List<Thing> GetThings()
    {
        return null;
    }
}