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

    private int m_Count;

    public int Count {
        get {
            return m_Count;
        }
    }

    public BinaryHeap() : this(DEFAULT_CAPACITY) { }

    public BinaryHeap(int capacity) {
        m_Count = 0;
        m_Heap = new T[capacity];
        m_Comparer = Comparer<T>.Default;
    }

    public BinaryHeap(Comparer<T> comparer) : this(DEFAULT_CAPACITY) {
        m_Comparer = comparer;
    }

    /// <summary>
    /// 插入数据
    /// </summary>
    /// <param name="element"></param>
    public void Push(T element) {
        if (m_Count >= m_Heap.Length) {
            EnlargeHeap(m_Heap.Length * 2);
        }
        m_Heap[m_Count++] = element;
        SinkDown(m_Count - 1);
    }

    /// <summary>
    /// 返回并移除堆顶数据
    /// </summary>
    /// <returns></returns>
    public T Pop() {
        if (m_Count == 0) {
            throw new IndexOutOfRangeException();
        }
        T topElement = m_Heap[0];
        T end = m_Heap[--m_Count];
        if (m_Count > 0) {
            m_Heap[0] = end;
            BubbleUp(0);
        }
        return topElement;
    }

    /// <summary>
    /// 直接返回堆顶数据
    /// </summary>
    /// <returns></returns>
    public T Peek() {
        if (m_Count == 0) {
            throw new IndexOutOfRangeException();
        }
        return m_Heap[0];
    }

    public void Clear() {
        m_Count = 0;
        Array.Clear(m_Heap, 0, m_Heap.Length);
    }

    /// <summary>
    /// 维护堆
    /// </summary>
    /// <param name="element"></param>
    public void Maintain(T element) {
        int index = Array.IndexOf(m_Heap, element);
        if (index != -1)
            SinkDown(index);
    }

    private void SinkDown(int pos) {
        var element = m_Heap[pos];
        while (pos > 0) {
            var parentPos = ((pos + 1) >> 1) - 1;
            var parent = m_Heap[parentPos];
            if (m_Comparer.Compare(element, parent) < 0) {
                m_Heap[parentPos] = element;
                m_Heap[pos] = parent;
                pos = parentPos;
            }
            else break;
        }
    }

    private void BubbleUp(int pos) {
        var element = m_Heap[pos];
        int swap;
        T child1 = default(T);
        T child2 = default(T);
        while (true) {
            swap = -1;
            var child2Pos = (pos + 1) << 1;
            var child1Pos = child2Pos - 1;
            if (child1Pos < m_Count) {
                child1 = m_Heap[child1Pos];
                if (m_Comparer.Compare(child1, element) < 0) {
                    swap = child1Pos;
                }
            }
            if (child2Pos < m_Count) {
                child2 = m_Heap[child2Pos];
                if (m_Comparer.Compare(child2, (swap == -1 ? element : child1)) < 0) {
                    swap = child2Pos;
                }
            }
            if (swap != -1) {
                m_Heap[pos] = m_Heap[swap];
                m_Heap[swap] = element;
                pos = swap;
            }
            else break;
        }
    }

    private void EnlargeHeap(int newCapacity) {
        Array.Resize(ref m_Heap, newCapacity);
    }
}