using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AStar {

    //为了简化计算，用10,14分别代表走直线（代价1）和走斜线（代价根号2）的代价
    public const int STRAIGHT_COST = 10;

    public const int DIAGONAL_COST = 14;

    public Node[,] nodes;

    public int width { get; private set; }

    public int height { get; private set; }

    public bool allowDiagonalMove { get; set; } = true;

    int m_LoopCount = 0;
    public int loopCount {
        get {
            return m_LoopCount;
        }
    }

    BinaryHeap<Node> m_OpenHeap = new BinaryHeap<Node>();

    private List<Node> m_DirtyList = new List<Node>();

    /// <summary>
    /// 启发函数
    /// </summary>
    private Func<Node, Node, int> m_HeuristicFunction;

    public void InitNodes(bool[,] walkableData) {
        width = walkableData.GetLength(0);
        height = walkableData.GetLength(1);
        nodes = new Node[width, height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                nodes[i, j] = new Node(i, j, walkableData[i, j]);
            }
        }
        SetHeuristicFunction(HeuristicFunctionType.Octile);
    }

    /// <summary>
    /// 指定启发函数
    /// </summary>
    /// <param name="algorithmType"></param>
    public void SetHeuristicFunction(HeuristicFunctionType algorithmType) {
        switch (algorithmType) {
            case HeuristicFunctionType.Manhattan:
                m_HeuristicFunction = Manhattan;
                break;
            case HeuristicFunctionType.Octile:
                m_HeuristicFunction = Octile;
                break;
        }
    }

    /// <summary>
    /// 在自定义启发函数时，距离的计算必须使用NORMAL_COST和DIAGONAL_COST
    /// </summary>
    /// <param name="func"></param>
    public void SetHeuristicFunction(Func<Node, Node, int> func) {
        m_HeuristicFunction = func;
    }

    public float stepTime = 0.2f;
    public IEnumerator GeneratePath(Vector2Int start, Vector2Int end, Action<bool, List<Vector2Int>> onGenerated, Action<Node> onAddToOpen, Action<Node> onRemoveFromOpen, Action<Node> onAddToClose) {
        m_LoopCount = 0;
        CleanDirtyNodes();
        m_OpenHeap.Clear();
        var startNode = nodes[start.x, start.y];
        var closestNode = startNode;
        var endNode = nodes[end.x, end.y];
        startNode.h = m_HeuristicFunction(startNode, endNode);
        m_DirtyList.Add(startNode);
        m_OpenHeap.Push(startNode);
        onAddToOpen(startNode);
        yield return new WaitForSeconds(stepTime);
        List<Vector2Int> path = new List<Vector2Int>();
        while (m_OpenHeap.Count > 0) {
            //寻找OpenList中代价最小的节点
            var curNode = m_OpenHeap.Pop();
            onRemoveFromOpen(curNode);
            curNode.isClosed = true;
            onAddToClose(curNode);
            yield return new WaitForSeconds(stepTime);
            if (curNode == endNode) {
                Node tempNode = endNode;
                while (tempNode != startNode) {
                    path.Add(tempNode.GetLocation());
                    tempNode = tempNode.parent;
                }
                path.Reverse();
                Debug.LogError("LoopCount:" + m_LoopCount);
                onGenerated(true, path);
                yield break;
            }
            yield return new WaitForSeconds(stepTime);
            var neighborNodes = GetNeighborNodes(curNode);
            for (int i = 0; i < neighborNodes.Count; i++) {
                var neighbor = neighborNodes[i];
                if (neighbor.isClosed || !neighbor.walkable)
                    continue;
                int gScore = curNode.g + CalculateWalkCost(curNode, neighbor);
                bool isVisited = neighbor.isVisited;
                if (!isVisited || gScore < neighbor.g) {
                    neighbor.isVisited = true;
                    neighbor.parent = curNode;
                    neighbor.h = m_HeuristicFunction(neighbor, endNode);
                    neighbor.g = gScore;
                    neighbor.f = neighbor.g + neighbor.h;
                    m_DirtyList.Add(neighbor);
                    if (!isVisited) {
                        m_OpenHeap.Push(neighbor);
                        onAddToOpen(neighbor);
                    }
                    else {
                        m_OpenHeap.Maintain(neighbor);
                    }
                }
            }
            m_LoopCount++;
        }
        //OpenList列表检索完毕，仍未发现目标节点，则说明是死路
        onGenerated.Invoke(false, path);
    }

    List<Node> GetNeighborNodes(Node node) {
        List<Node> list = new List<Node>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                //跳过自身
                if (i == 0 && j == 0)
                    continue;
                int x = node.x + i;
                int y = node.y + j;
                //越界检测
                if (!CheckBounds(x, y))
                    continue;
                //走斜线
                if (x != node.x && y != node.y) {
                    if (!allowDiagonalMove)
                        continue;
                    //斜线障碍物检测
                    if (!nodes[node.x, y].walkable || !nodes[x, node.y].walkable)
                        continue;
                }
                list.Add(nodes[x, y]);
            }
        }
        return list;
    }

    /// <summary>
    /// 计算移动到相邻节点的开销
    /// </summary>
    /// <param name="node"></param>
    /// <param name="neighbor"></param>
    /// <returns></returns>
    int CalculateWalkCost(Node node, Node neighbor) {
        if (node.x != neighbor.x && node.y != neighbor.y) {
            return DIAGONAL_COST;
        }
        return STRAIGHT_COST;
    }

    bool CheckBounds(int x, int y) {
        return x < width && x >= 0 && y < height && y >= 0;
    }
    void CleanDirtyNodes() {
        for (int i = 0; i < m_DirtyList.Count; i++) {
            m_DirtyList[i].Reset();
        }
        m_DirtyList.Clear();
    }


    /// <summary>
    /// 曼哈顿距离（乘上移动单格的开销）
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    int Manhattan(Node a, Node b) {
        return (Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y)) * STRAIGHT_COST;
    }

    /// <summary>
    /// 网格对角线距离，Diagonal算法的特殊情况
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    int Octile(Node a, Node b) {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return STRAIGHT_COST * Mathf.Max(dx, dy) + (DIAGONAL_COST - STRAIGHT_COST) * Mathf.Min(dx, dy);
    }
}

public class Node : IComparable<Node> {

    public Node parent;

    public bool walkable;

    public bool isVisited;

    public bool isClosed;

    public int x, y;

    public int g, h;

    public int f;

    public Node() { }

    public Node(int x, int y, bool walkable) {
        this.x = x;
        this.y = y;
        this.walkable = walkable;
    }

    public void Reset() {
        parent = null;
        g = h = f = 0;
        isVisited = isClosed = false;
    }

    public Vector2Int GetLocation() {
        return new Vector2Int(x, y);
    }

    public int CompareTo(Node other) {
        return f.CompareTo(other.f);
    }
}

public enum HeuristicFunctionType {
    /// <summary>
    /// 曼哈顿距离，适合4向行走
    /// </summary>
    Manhattan,
    /// <summary>
    /// 对角线距离，适合8向行走
    /// </summary>
    Octile
}