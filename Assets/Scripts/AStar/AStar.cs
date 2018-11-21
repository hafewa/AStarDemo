using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AStar {

    public Node[,] nodes;

    public int width { get; private set; }

    public int height { get; private set; }

    int m_LoopCount = 0;
    public int loopCount {
        get {
            return m_LoopCount;
        }
    }

    BinaryHeap<Node> m_OpenHeap = new BinaryHeap<Node>(Comparer<Node>.Create((a, b) => {
        return a.f.CompareTo(b.f);
    }));

    private HashSet<Node> m_CloseSet = new HashSet<Node>();

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

    public void SetHeuristicFunction(Func<Node, Node, int> func) {
        m_HeuristicFunction = func;
    }

    public float stepTime = 0.2f;
    public IEnumerator GeneratePath(Vector2Int start, Vector2Int end, Action<bool, List<Vector2Int>> onGenerated, Action<Node> onAddToOpen, Action<Node> onRemoveFromOpen, Action<Node> onAddToClose) {
        var startNode = nodes[start.x, start.y];
        var endNode = nodes[end.x, end.y];
        startNode.Reset();
        m_LoopCount = 0;
        m_OpenHeap.Clear();
        m_CloseSet.Clear();
        m_OpenHeap.Insert(startNode);
        onAddToOpen(startNode);
        yield return new WaitForSeconds(stepTime);
        Node curNode = null;
        List<Vector2Int> path = new List<Vector2Int>();
        StringBuilder sb = new StringBuilder();
        while (m_OpenHeap.Count > 0) {
            //寻找OpenList中代价最小的节点
            curNode = m_OpenHeap.PopTop();
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
            //m_OpenList.Remove(curNode);
            onRemoveFromOpen(curNode);
            m_CloseSet.Add(curNode);
            onAddToClose(curNode);
            yield return new WaitForSeconds(stepTime);
            var neighborNodes = GetNeighborNodes(curNode);
            for (int i = 0; i < neighborNodes.Count; i++) {
                var node = neighborNodes[i];
                if (!node.walkable || m_CloseSet.Contains(node))
                    continue;
                int newCost = curNode.g + m_HeuristicFunction(curNode, node);
                bool isOpen = m_OpenHeap.Contains(node);
                if (newCost < node.g || !isOpen) {
                    node.g = newCost;
                    node.h = m_HeuristicFunction(node, endNode);
                    node.f = node.g + node.h;
                    node.parent = curNode;
                    if (!isOpen) {
                        m_OpenHeap.Insert(node);
                        onAddToOpen(node);
                        yield return new WaitForSeconds(stepTime);
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
                //斜线障碍物检测
                if ((Mathf.Abs(i) + Mathf.Abs(j)) == 2) {
                    if (!nodes[node.x, y].walkable || !nodes[x, node.y].walkable)
                        continue;
                }
                list.Add(nodes[x, y]);
            }
        }
        return list;
    }

    bool CheckBounds(int x, int y) {
        return x < width && x >= 0 && y < height && y >= 0;
    }

    /// <summary>
    /// 曼哈顿距离
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    int Manhattan(Node a, Node b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// 网格对角线距离，Diagonal算法的特殊情况
    /// 为了简化计算，用10,14分别代表走直线（代价1）和走斜线（代价根号2）的代价
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    int Octile(Node a, Node b) {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return 10 * Mathf.Max(dx, dy) + (14 - 10) * Mathf.Min(dx, dy);
    }
}

public class Node {

    public Node parent;

    public bool walkable;

    public int x, y;

    /// <summary>
    /// 移动成本
    /// </summary>
    public int g;

    /// <summary>
    /// 启发法得到的与终点的距离
    /// </summary>
    public int h;

    /// <summary>
    /// 整体成本
    /// </summary>
    public int f;

    public Node(int x, int y, bool walkable) {
        this.x = x;
        this.y = y;
        this.walkable = walkable;
    }

    public void Reset() {
        g = h = 0;
        parent = null;
    }

    public Vector2Int GetLocation() {
        return new Vector2Int(x, y);
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