using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class IslandPolygon
{
    public Vector2 [] Polygon {get; set;}
    public Vector2 Centroid {get; set;}
}

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

    private IslandPolygon GetIslandPolygon(int n, int size)
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
        
        points.Select(i => new Vector2(i.x + xShift, i.y + yShift)).ToList();
        
        var centroid = GetCentroid(points);
        points.ForEach(v => GD.Print(v));

        points = points.Select(i => new Vector2(i.x - centroid.x, i.y - centroid.y)).ToList();

        points.ForEach(v => GD.Print(v));
        return new IslandPolygon {Polygon = points.ToArray(), Centroid = centroid};
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

    private Vector2 GetCentroid(List<Vector2> points)
    {
        var voff = points[0];
        float twiceArea = 0;
        float x = 0;
        float y = 0;

        Vector2 p1, p2;
        float f;

        for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
        {
            p1 = points[i];
            p2 = points[j];

            f = (p1.x - voff.x) * (p2.y - voff.y) - (p2.x - voff.x) * (p1.y - voff.y);
            twiceArea += f;
            x += (p1.x + p2.x - 2 * voff.x) * f;
            y += (p1.y + p2.y - 2 * voff.y) * f;
        }

        f = twiceArea * 3;

        return new Vector2(x / f + voff.x, y / f + voff.y);
    }
}