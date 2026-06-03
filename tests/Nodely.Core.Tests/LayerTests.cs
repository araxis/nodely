using System;
using System.Collections.Generic;
using Nodely.Models.Base;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

file sealed class Item : Model { }

file sealed class CountingBatcher : IModelBatcher
{
    public int Calls { get; private set; }

    public void Batch(Action action)
    {
        Calls++;
        action();
    }
}

public class LayerTests
{
    [Fact]
    public void Add_stores_items_and_raises_added()
    {
        var layer = new Layer<Item>();
        var added = new List<Item>();
        layer.Added += added.Add;

        var a = layer.Add(new Item());
        var b = layer.Add(new Item());

        layer.Count.ShouldBe(2);
        layer[0].ShouldBeSameAs(a);
        layer.Contains(b).ShouldBeTrue();
        added.Count.ShouldBe(2);
    }

    [Fact]
    public void Remove_raises_removed()
    {
        var layer = new Layer<Item>();
        var item = layer.Add(new Item());
        Item? removed = null;
        layer.Removed += i => removed = i;

        layer.Remove(item);

        layer.Count.ShouldBe(0);
        removed.ShouldBeSameAs(item);
    }

    [Fact]
    public void Remove_unknown_item_does_not_raise()
    {
        var layer = new Layer<Item>();
        var raised = false;
        layer.Removed += _ => raised = true;

        layer.Remove(new Item());

        raised.ShouldBeFalse();
    }

    [Fact]
    public void Clear_removes_all_and_raises_each()
    {
        var layer = new Layer<Item>();
        layer.Add(new[] { new Item(), new Item(), new Item() });
        var removed = 0;
        layer.Removed += _ => removed++;

        layer.Clear();

        layer.Count.ShouldBe(0);
        removed.ShouldBe(3);
    }

    [Fact]
    public void Batcher_wraps_each_mutation()
    {
        var batcher = new CountingBatcher();
        var layer = new Layer<Item>(batcher);

        layer.Add(new Item());
        layer.Add(new Item());

        batcher.Calls.ShouldBe(2);
    }

    [Fact]
    public void Add_null_throws()
    {
        var layer = new Layer<Item>();
        Should.Throw<ArgumentNullException>(() => layer.Add((Item)null!));
    }

    [Fact]
    public void Enumeration_yields_items_in_insertion_order()
    {
        var layer = new Layer<Item>();
        var a = layer.Add(new Item());
        var b = layer.Add(new Item());

        layer.ShouldBe(new[] { a, b });
    }
}
