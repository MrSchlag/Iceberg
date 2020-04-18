using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class IslandGenerator
{
    public IslandGenerator()
    {
    }

    public Node2D Generate()
    {
        var node = GetIslandBase();
        return node;
    }

    private Node2D GetIslandBase()
    {
        var island = new Island();
        
        island.SetPolygon(GetIslandPolygon(10, 100));
        
        return (Node2D)island;
    }

    private Vector2[] GetIslandPolygon(int n, int size)
    {
        var rand = new Random();

        var xPool = Enumerable.Range(0, n).Select(i => (float)rand.NextDouble() * size).OrderBy(i => i).ToList();
        var yPool = Enumerable.Range(0, n).Select(i => (float)rand.NextDouble() * size).OrderBy(i => i).ToList();

        var maxX = xPool.Max();
        var minX = xPool.Min();
        var maxY = yPool.Max();
        var minY = yPool.Min();

        var xVec = new List<float>();
        var yVec = new List<float>();
        
        PoolToVec(rand, n, xPool, maxX, minX, xVec);
        PoolToVec(rand, n, yPool, maxY, minY, yVec);

        xVec = xVec.OrderBy(i => Guid.NewGuid()).ToList();
        var vec = xVec.Zip(yVec, (i, j) => new Vector2(i, j)).ToList();
        vec = vec.OrderBy(i => Mathf.Atan2(i.y, i.x)).ToList();

        var x = 0f;
        var y = 0f;
        var minPolyX = 0f;
        var minPolyY = 0f;

        var points = new List<Vector2>();
        for (int i = 0; i < n; i++)
        {
            points.Add(new Vector2(x, y));
            x += vec[i].x;
            y += vec[i].y;

            minPolyX = Mathf.Min(minPolyX, x);
            minPolyY = Mathf.Min(minPolyY, y);
        }

        var xShift = minX - minPolyX;
        var yShift = minY - minPolyY;
        
        points.ForEach(i => {
            i.x += xShift;
            i.y += yShift;
        });

        return points.ToArray();
    }

    private static void PoolToVec(Random rand, int n, List<float> pool, float max, float min, List<float> vec)
    {
        var lastTop = min;
        var lastBot = min;

        for (int i = 1; i < n - 1; i++)
        {
            var x = pool[i];

            if (rand.Next(2) == 0)
            {
                vec.Add(x - lastTop);
                lastTop = x;
            }
            else
            {
                vec.Add(lastBot - x);
                lastBot = x;
            }
        }

        vec.Add(max - lastTop);
        vec.Add(lastBot - max);
    }
}