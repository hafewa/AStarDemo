using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarExample : MonoBehaviour {

    public Transform start, end;

    public Tilemap blockLayer;

    public Tilemap pathLayer;

    public Tilemap openLayer, closedLayer;

    public Tile tile;

    private AStar aStar = new AStar();

    public float stepTime = 0.2f;

    public HeuristicFunctionType functionType = HeuristicFunctionType.Manhattan;

    // Use this for initialization
    void Start() {
        bool[,] walkableData = new bool[blockLayer.size.x, blockLayer.size.y];
        for (int i = 0; i < blockLayer.size.x; i++) {
            for (int j = 0; j < blockLayer.size.y; j++) {
                if (blockLayer.GetTile(new Vector3Int(i, j, 0)) == null) {
                    walkableData[i, j] = true;
                }
            }
        }
        aStar.InitNodes(walkableData);
    }

    public void OnGUI() {
        if (GUILayout.Button("计算路径")) {
            aStar.stepTime = stepTime;
            aStar.SetHeuristicFunction(functionType);
            var startLoc = pathLayer.WorldToCell(start.position);
            var endLoc = pathLayer.WorldToCell(end.position);
            pathLayer.ClearAllTiles();
            openLayer.ClearAllTiles();
            closedLayer.ClearAllTiles();
            StopAllCoroutines();
            StartCoroutine(aStar.GeneratePath(new Vector2Int(startLoc.x, startLoc.y), new Vector2Int(endLoc.x, endLoc.y), (result, path) => {
                if (!result) {
                    Debug.Log("目标点无法到达");
                    return;
                }
                Debug.LogError("路径长度：" + path.Count);
                foreach (var loc in path) {
                    pathLayer.SetTile(new Vector3Int(loc.x, loc.y, 0), tile);
                }
            }, node => {
                openLayer.SetTile(new Vector3Int(node.x, node.y, 0), tile);
            }, node => {
                openLayer.SetTile(new Vector3Int(node.x, node.y, 0), null);
            }, node => {
                closedLayer.SetTile(new Vector3Int(node.x, node.y, 0), tile);
            }));
        }
    }
}
