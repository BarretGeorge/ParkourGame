using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通用对象池 - 支持任何类型的对象
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class GenericObjectPool<T> where T : class, new()
{
    private readonly Stack<T> pool;
    private readonly System.Func<T> createFn;
    private readonly System.Action<T> resetFn;
    private readonly System.Action<T> onGetFn;
    private readonly System.Action<T> onReleaseFn;
    private readonly int maxSize;

    public int Count { get; private set; }
    public int ActiveCount { get; private set; }
    public int InactiveCount => pool.Count;

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <param name="initialSize">初始大小</param>
    /// <param name="maxSize">最大大小</param>
    /// <param name="createFn">创建函数</param>
    /// <param name="resetFn">重置函数</param>
    /// <param name="onGetFn">获取时回调</param>
    /// <param name="onReleaseFn">释放时回调</param>
    public GenericObjectPool(int initialSize = 10, int maxSize = 100,
                             System.Func<T> createFn = null,
                             System.Action<T> resetFn = null,
                             System.Action<T> onGetFn = null,
                             System.Action<T> onReleaseFn = null)
    {
        this.pool = new Stack<T>(initialSize);
        this.maxSize = maxSize;
        this.createFn = createFn ?? (() => new T());
        this.resetFn = resetFn;
        this.onGetFn = onGetFn;
        this.onReleaseFn = onReleaseFn;

        // 预创建对象
        for (int i = 0; i < initialSize; i++)
        {
            T obj = this.createFn();
            pool.Push(obj);
            Count++;
        }
    }

    /// <summary>
    /// 获取对象
    /// </summary>
    public T Get()
    {
        T obj;

        if (pool.Count > 0)
        {
            obj = pool.Pop();
        }
        else
        {
            obj = createFn();
            Count++;
        }

        ActiveCount++;
        onGetFn?.Invoke(obj);

        return obj;
    }

    /// <summary>
    /// 释放对象
    /// </summary>
    public void Release(T obj)
    {
        if (obj == null) return;

        resetFn?.Invoke(obj);
        onReleaseFn?.Invoke(obj);

        if (pool.Count < maxSize)
        {
            pool.Push(obj);
        }

        ActiveCount--;
    }

    /// <summary>
    /// 预热对象池
    /// </summary>
    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (pool.Count >= maxSize) break;

            T obj = createFn();
            pool.Push(obj);
            Count++;
        }
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void Clear()
    {
        pool.Clear();
        Count = 0;
        ActiveCount = 0;
    }

    /// <summary>
    /// 收缩对象池到指定大小
    /// </summary>
    public void Shrink(int targetSize)
    {
        while (pool.Count > targetSize)
        {
            pool.Pop();
            Count--;
        }
    }
}
