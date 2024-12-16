using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelGen
{
    public class Props : MonoBehaviour
    {
        /* -------- Object references --------*/
        private Tilemap tilemap;
        
        
        /* -------- Variables --------*/
        private List<TileBase> tiles;
        // Start is called before the first frame update
        
        
        /* -------- Start --------*/
        void Start()
        {
            //get object reference to tilemap
            tilemap = GetComponent<Tilemap>();
            
            //check if map has loaded
            if (LevelMap.IsLoaded)
            {
                LevelMap levelMap = FindObjectOfType<LevelMap>();
                GenerateProps(levelMap);
            }
            //subscribe to events
            LevelMap.OnLevelLoaded += GenerateProps;
            LevelMap.OnLevelUnloaded += ClearProps;
        }

        
        /* -------- Functions --------*/
        private void GenerateProps(LevelMap map)
        {
            //TODO: Load props
        }

        private void ClearProps()
        {
            tilemap.ClearAllTiles();
        }
    }
}
