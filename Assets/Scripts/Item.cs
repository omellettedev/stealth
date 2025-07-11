using UnityEngine;

public abstract class ItemBase
{
    public abstract ItemData Data { get; }
}

public abstract class Item<T> : ItemBase where T : ItemData
{
    private T _data;
    public override ItemData Data => _data;

    public Item(T data)
    {
        _data = data;
    }
}