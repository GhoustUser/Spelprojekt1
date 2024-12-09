using System.Collections.Generic;
using UnityEngine;
using static Default.Default;
using LevelGen;

public class Pathfinding
{
    private List<Vector2> searchedCells;
    private List<Vector2> cellsToSearch;
    private List<Vector2> finalPath;

    private Dictionary<Vector2, Cell> cells;

    private int GetDistance(Vector2 startPos, Vector2 endPos)
    {
        Vector2Int distance = new Vector2Int((int)Mathf.Abs(startPos.x - endPos.x), (int)Mathf.Abs(startPos.y - endPos.y));

        int lowest = Mathf.Min(distance.x, distance.y);
        int highest = Mathf.Max(distance.x, distance.y);

        // 14 is the diagonal distance between two tiles multiplied by 10, while 10 is the vertical / horizontal distance multiplied by 10.
        return lowest * 14 + (highest - lowest) * 10;
    }

    public List<Vector2> FindPath(Vector2 startPos, Vector2 endPos)
    {
        cells = LevelMap.cells;

        if (!cells.ContainsKey(startPos) || !cells.ContainsKey(endPos)) return new List<Vector2>();

        ResetCosts();

        cellsToSearch = new List<Vector2>() { startPos };
        searchedCells = new List<Vector2>();
        finalPath = new List<Vector2>();

        Cell startCell = cells[startPos];

        startCell.gCost = 0;
        startCell.hCost = GetDistance(startPos, endPos);
        startCell.fCost = GetDistance(startPos, endPos);

        while (cellsToSearch.Count > 0)
        {
            Vector2 cellToSearch = cellsToSearch[0];
            List<Vector2> optionList = new List<Vector2>();

            foreach (Vector2 pos in cellsToSearch)
            {
                Cell c = cells[pos];

                /*if (c.hCost < cells[cellToSearch].hCost)
                {
                    optionList.Clear();
                    optionList.Add(pos);
                }
                else if (c.hCost <= cells[cellToSearch].hCost)
                {
                    optionList.Add(pos);
                }*/

                if (c.fCost <= cells[cellToSearch].fCost && 
                    c.hCost < cells[cellToSearch].hCost)
                {
                    cellToSearch = pos;
                }
            }

            // cellToSearch = optionList[UnityEngine.Random.Range(0, optionList.Count - 1)];

            cellsToSearch.Remove(cellToSearch);
            searchedCells.Add(cellToSearch);

            if (cellToSearch != endPos)
            {
                SearchCellNeighbors(cellToSearch, endPos);
                continue;
            }

            Cell pathCell = cells[endPos];

            while (pathCell.position != startPos)
            {
                finalPath.Add(pathCell.position);
                pathCell = cells[pathCell.connection];
            }

            return finalPath;
        }
        return new List<Vector2>();
    }

    private void SearchCellNeighbors(Vector2 cellPos, Vector2 endPos)
    {
        for (float x = cellPos.x - TILE_SIZE; x <= TILE_SIZE + cellPos.x; x+= TILE_SIZE)
        {
            for (float y = cellPos.y - TILE_SIZE; y <= TILE_SIZE + cellPos.y; y += TILE_SIZE)
            {
                if (new Vector2(x - cellPos.x, y - cellPos.y).magnitude > 1.4f) continue;

                Vector2 neighborPos = new Vector2(x, y);

                if (!(cells.TryGetValue(neighborPos, out Cell c) & !searchedCells.Contains(neighborPos) && cells[neighborPos].walkable)) continue;
                
                int GcostToNeighbour = cells[cellPos].gCost + GetDistance(cellPos, neighborPos);

                if (GcostToNeighbour >= cells[neighborPos].gCost) continue;
                
                Cell neighbourNode = cells[neighborPos];

                neighbourNode.connection = cellPos;
                neighbourNode.gCost = GcostToNeighbour;
                neighbourNode.hCost = GetDistance(neighborPos, endPos);
                neighbourNode.fCost = neighbourNode.gCost + neighbourNode.hCost;

                if (!cellsToSearch.Contains(neighborPos))
                {
                    cellsToSearch.Add(neighborPos);
                }
            }
        }
    }

    private void ResetCosts()
    {
        foreach (var cell in cells.Values)
        {
            cell.gCost = int.MaxValue;
            cell.hCost = 0;
            cell.fCost = int.MaxValue;
            cell.connection = Vector2.zero;
        }
    }
}
