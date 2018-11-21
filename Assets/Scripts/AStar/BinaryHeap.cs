using System;
using System.Collections.Generic;
/// <summary>
/// https://blog.csdn.net/Colton_Null/article/details/80963217 最小二叉堆的Java实现
/// </summary>
/// <typeparam name="T"></typeparam>
public class BinaryHeap<T> {

    const int DEFAULT_CAPACITY = 10;

    private T[] m_Heap;

    private Comparer<T> m_Comparer;

    public T[] heap {
        get {
            return m_Heap;
        }
    }

    private int m_Count;

    public int Count {
        get {
            return m_Count;
        }
    }

    public BinaryHeap() : this(DEFAULT_CAPACITY) { }

    public BinaryHeap(int capacity) {
        m_Count = 0;
        m_Heap = new T[capacity + 1];
        m_Comparer = Comparer<T>.Default;
    }

    public BinaryHeap(Comparer<T> comparer) : this(DEFAULT_CAPACITY) {
        m_Comparer = comparer;
    }

    public BinaryHeap(T[] items) {
        m_Comparer = Comparer<T>.Default;
        m_Count = m_Heap.Length;
        m_Heap = (T[])new T[(m_Count + 2) * 11 / 10];
        int i = 1;
        foreach (T item in m_Heap) {
            m_Heap[i++] = item;
        }
        BuildHeap();
    }

    public void Insert(T item) {
        if (m_Count == m_Heap.Length - 1) {
            EnlargeHeap(m_Heap.Length * 2 + 1);
        }
        int hole = ++m_Count;
        // arr[0] = t初始化，最后如果循环到顶点，t.compartTo(arr[hole / 2])即arr[0]为0，循环结束
        for (m_Heap[0] = item; m_Comparer.Compare(item, m_Heap[hole / 2]) < 0; hole /= 2) {
            // 根节点的值赋值到子节点
            m_Heap[hole] = m_Heap[hole / 2];
        }
        // 根节点(或树叶节点)赋值为t
        m_Heap[hole] = item;
    }

    /// <summary>
    /// 直接返回堆顶数据
    /// </summary>
    /// <returns></returns>
    public T PeekTop() {
        if (m_Count == 0) {
            throw new IndexOutOfRangeException();
        }
        return m_Heap[1];
    }

    /// <summary>
    /// 返回并移除堆顶数据
    /// </summary>
    /// <returns></returns>
    public T PopTop() {
        if (m_Count == 0) {
            throw new IndexOutOfRangeException();
        }
        T minItem = PeekTop();
        // 将最后一个节点赋值到根节点
        m_Heap[1] = m_Heap[m_Count--];
        // 从根节点执行下滤
        PercolateDown(1);
        return minItem;
    }

    public bool Contains(T item) {
        return Array.IndexOf(m_Heap, item) >= 1;
    }

    public void Clear() {
        m_Count = 0;
        Array.Clear(m_Heap, 0, m_Heap.Length);
    }

    private void BuildHeap() {
        for (int i = m_Count / 2; i > 0; i--) {
            PercolateDown(i);
        }
    }

    private void PercolateDown(int hole) {
        int child;
        T tmp = m_Heap[hole];

        for (; hole * 2 <= m_Count; hole = child) {
            child = hole * 2;
            if (child != m_Count && m_Comparer.Compare(m_Heap[child + 1], m_Heap[child]) < 0) {
                // 做子节点不为左后一个节点（说明有右节点）且右节点比做节点小，索引改为右节点节点
                child++;
            }
            if (m_Comparer.Compare(m_Heap[child], tmp) < 0) {
                // 如果遍历到的这个节点比最后一个元素小
                m_Heap[hole] = m_Heap[child];
            }
            else {
                break;
            }
        }
        // 将最后一个元素补到前面的空位
        m_Heap[hole] = tmp;
    }

    private void EnlargeHeap(int newCapacity) {
        Array.Resize(ref m_Heap, newCapacity);
    }
}