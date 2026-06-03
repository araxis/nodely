using System;
using System.Collections.Generic;
using System.Linq;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Routers;

/// <summary>
/// Routes links along right angles, avoiding the source/target node bounds. Builds a sparse grid from the
/// node edges and runs A* (penalizing direction changes), falling back to another router when it can't
/// (e.g. unattached links or non-port endpoints). Requires cardinal (Top/Right/Bottom/Left) port alignments.
/// </summary>
public class OrthogonalRouter : Router
{
    private readonly Router _fallbackRouter;
    private readonly double _shapeMargin;
    private readonly double _globalMargin;

    /// <summary>Creates the router with the given margins and fallback.</summary>
    public OrthogonalRouter(double shapeMargin = 10d, double globalMargin = 50d, Router? fallbackRouter = null)
    {
        _shapeMargin = shapeMargin;
        _globalMargin = globalMargin;
        _fallbackRouter = fallbackRouter ?? new NormalRouter();
    }

    /// <inheritdoc />
    public override Point[] GetRoute(Diagram diagram, BaseLinkModel link)
    {
        if (!link.IsAttached)
            return _fallbackRouter.GetRoute(diagram, link);

        if (link.Source is not SinglePortAnchor spa1)
            return _fallbackRouter.GetRoute(diagram, link);

        if (link.Target is not SinglePortAnchor targetAnchor)
            return _fallbackRouter.GetRoute(diagram, link);

        var sourcePort = spa1.Port;
        if (sourcePort.Parent.Size == null || targetAnchor.Port.Parent.Size == null)
            return _fallbackRouter.GetRoute(diagram, link);

        var targetPort = targetAnchor.Port;

        var shapeMargin = _shapeMargin;
        var globalBoundsMargin = _globalMargin;
        var spots = new HashSet<Point>();
        var verticals = new List<double>();
        var horizontals = new List<double>();
        var sideA = sourcePort.Alignment;
        var sideAVertical = IsVerticalSide(sideA);
        var sideB = targetPort.Alignment;
        var sideBVertical = IsVerticalSide(sideB);
        var originA = GetPortPositionBasedOnAlignment(sourcePort);
        var originB = GetPortPositionBasedOnAlignment(targetPort);
        var shapeA = sourcePort.Parent.GetBounds(includePorts: true)!;
        var shapeB = targetPort.Parent.GetBounds(includePorts: true)!;
        var inflatedA = shapeA.Inflate(shapeMargin, shapeMargin);
        var inflatedB = shapeB.Inflate(shapeMargin, shapeMargin);

        if (inflatedA.Intersects(inflatedB))
        {
            shapeMargin = 0;
            inflatedA = shapeA;
            inflatedB = shapeB;
        }

        var bounds = inflatedA.Union(inflatedB).Inflate(globalBoundsMargin, globalBoundsMargin);

        verticals.Add(inflatedA.Left);
        verticals.Add(inflatedA.Right);
        horizontals.Add(inflatedA.Top);
        horizontals.Add(inflatedA.Bottom);
        verticals.Add(inflatedB.Left);
        verticals.Add(inflatedB.Right);
        horizontals.Add(inflatedB.Top);
        horizontals.Add(inflatedB.Bottom);

        (sideAVertical ? verticals : horizontals).Add(sideAVertical ? originA.X : originA.Y);
        (sideBVertical ? verticals : horizontals).Add(sideBVertical ? originB.X : originB.Y);

        spots.Add(GetOriginSpot(originA, sideA, shapeMargin));
        spots.Add(GetOriginSpot(originB, sideB, shapeMargin));

        verticals.Sort();
        horizontals.Sort();

        var grid = RulersToGrid(verticals, horizontals, bounds);
        var gridPoints = GridToSpots(grid, new[] { inflatedA, inflatedB });
        spots.UnionWith(gridPoints);

        var ys = spots.Select(p => p.Y).Distinct().ToList();
        var xs = spots.Select(p => p.X).Distinct().ToList();
        ys.Sort();
        xs.Sort();

        var nodes = spots.ToDictionary(p => p, p => new RouteNode(p));

        for (var i = 0; i < ys.Count; i++)
        {
            for (var j = 0; j < xs.Count; j++)
            {
                var b = new Point(xs[j], ys[i]);
                if (!nodes.ContainsKey(b))
                    continue;

                if (j > 0)
                {
                    var a = new Point(xs[j - 1], ys[i]);
                    if (nodes.ContainsKey(a))
                    {
                        nodes[a].ConnectedTo.Add(nodes[b]);
                        nodes[b].ConnectedTo.Add(nodes[a]);
                    }
                }

                if (i > 0)
                {
                    var a = new Point(xs[j], ys[i - 1]);
                    if (nodes.ContainsKey(a))
                    {
                        nodes[a].ConnectedTo.Add(nodes[b]);
                        nodes[b].ConnectedTo.Add(nodes[a]);
                    }
                }
            }
        }

        var nodeA = nodes[GetOriginSpot(originA, sideA, shapeMargin)];
        var nodeB = nodes[GetOriginSpot(originB, sideB, shapeMargin)];
        var path = AStarPathfinder.GetPath(nodeA, nodeB);

        return path.Count > 0 ? path.ToArray() : _fallbackRouter.GetRoute(diagram, link);
    }

    private static Grid RulersToGrid(List<double> verticals, List<double> horizontals, Rectangle bounds)
    {
        var result = new Grid();
        verticals.Sort();
        horizontals.Sort();

        var lastX = bounds.Left;
        var lastY = bounds.Top;
        var column = 0;
        var row = 0;

        foreach (var y in horizontals)
        {
            foreach (var x in verticals)
            {
                result.Set(row, column++, new Rectangle(lastX, lastY, x, y));
                lastX = x;
            }

            result.Set(row, column, new Rectangle(lastX, lastY, bounds.Right, y));
            lastX = bounds.Left;
            lastY = y;
            column = 0;
            row++;
        }

        lastX = bounds.Left;

        foreach (var x in verticals)
        {
            result.Set(row, column++, new Rectangle(lastX, lastY, x, bounds.Bottom));
            lastX = x;
        }

        result.Set(row, column, new Rectangle(lastX, lastY, bounds.Right, bounds.Bottom));
        return result;
    }

    private static HashSet<Point> GridToSpots(Grid grid, Rectangle[] obstacles)
    {
        bool IsInsideObstacles(Point p)
        {
            foreach (var obstacle in obstacles)
                if (obstacle.ContainsPoint(p))
                    return true;
            return false;
        }

        void AddIfFree(HashSet<Point> list, Point p)
        {
            if (!IsInsideObstacles(p))
                list.Add(p);
        }

        var gridPoints = new HashSet<Point>();
        foreach (var rowKvp in grid.Data)
        {
            var row = rowKvp.Key;
            var firstRow = row == 0;
            var lastRow = row == grid.Rows - 1;

            foreach (var colKvp in rowKvp.Value)
            {
                var col = colKvp.Key;
                var r = colKvp.Value;
                var firstCol = col == 0;
                var lastCol = col == grid.Columns - 1;
                var nw = firstCol && firstRow;
                var ne = firstRow && lastCol;
                var se = lastRow && lastCol;
                var sw = lastRow && firstCol;

                if (nw || ne || se || sw)
                {
                    AddIfFree(gridPoints, r.NorthWest);
                    AddIfFree(gridPoints, r.NorthEast);
                    AddIfFree(gridPoints, r.SouthWest);
                    AddIfFree(gridPoints, r.SouthEast);
                }
                else if (firstRow)
                {
                    AddIfFree(gridPoints, r.NorthWest);
                    AddIfFree(gridPoints, r.North);
                    AddIfFree(gridPoints, r.NorthEast);
                }
                else if (lastRow)
                {
                    AddIfFree(gridPoints, r.SouthEast);
                    AddIfFree(gridPoints, r.South);
                    AddIfFree(gridPoints, r.SouthWest);
                }
                else if (firstCol)
                {
                    AddIfFree(gridPoints, r.NorthWest);
                    AddIfFree(gridPoints, r.West);
                    AddIfFree(gridPoints, r.SouthWest);
                }
                else if (lastCol)
                {
                    AddIfFree(gridPoints, r.NorthEast);
                    AddIfFree(gridPoints, r.East);
                    AddIfFree(gridPoints, r.SouthEast);
                }
                else
                {
                    AddIfFree(gridPoints, r.NorthWest);
                    AddIfFree(gridPoints, r.North);
                    AddIfFree(gridPoints, r.NorthEast);
                    AddIfFree(gridPoints, r.East);
                    AddIfFree(gridPoints, r.SouthEast);
                    AddIfFree(gridPoints, r.South);
                    AddIfFree(gridPoints, r.SouthWest);
                    AddIfFree(gridPoints, r.West);
                    AddIfFree(gridPoints, r.Center);
                }
            }
        }

        return gridPoints;
    }

    private static bool IsVerticalSide(PortAlignment alignment)
        => alignment == PortAlignment.Top || alignment == PortAlignment.Bottom;

    private static Point GetOriginSpot(Point p, PortAlignment alignment, double shapeMargin)
        => alignment switch
        {
            PortAlignment.Top => p.Add(0, -shapeMargin),
            PortAlignment.Right => p.Add(shapeMargin, 0),
            PortAlignment.Bottom => p.Add(0, shapeMargin),
            PortAlignment.Left => p.Add(-shapeMargin, 0),
            _ => throw new NodelyException(
                $"OrthogonalRouter requires cardinal port alignments; got '{alignment}'."),
        };
}

internal sealed class Grid
{
    public Dictionary<double, Dictionary<double, Rectangle>> Data { get; } = new();
    public double Rows { get; private set; }
    public double Columns { get; private set; }

    public void Set(double row, double column, Rectangle rectangle)
    {
        Rows = Math.Max(Rows, row + 1);
        Columns = Math.Max(Columns, column + 1);

        if (!Data.ContainsKey(row))
            Data.Add(row, new Dictionary<double, Rectangle>());

        Data[row].Add(column, rectangle);
    }
}

internal sealed class RouteNode
{
    public RouteNode(Point position)
    {
        Position = position;
        ConnectedTo = new List<RouteNode>();
    }

    public Point Position { get; }
    public List<RouteNode> ConnectedTo { get; }
    public double Cost { get; internal set; }
    public RouteNode? Parent { get; internal set; }

    public override bool Equals(object? obj) => obj is RouteNode other && Position.Equals(other.Position);

    public override int GetHashCode() => Position.GetHashCode();
}

internal static class AStarPathfinder
{
    public static IReadOnlyList<Point> GetPath(RouteNode start, RouteNode goal)
    {
        var frontier = new PriorityQueue<RouteNode, double>();
        frontier.Enqueue(start, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(goal))
                break;

            foreach (var next in current.ConnectedTo)
            {
                var newCost = current.Cost + 1.0;
                if (current.Parent != null && IsChangeOfDirection(current.Parent.Position, current.Position, next.Position))
                {
                    newCost *= newCost;
                    newCost *= newCost;
                }

                if (next.Cost == 0 || newCost < next.Cost)
                {
                    next.Cost = newCost;
                    var priority = newCost + Heuristic(next.Position, goal.Position);
                    frontier.Enqueue(next, priority);
                    next.Parent = current;
                }
            }
        }

        var result = new List<Point>();
        var c = goal.Parent;

        while (c != null && c != start)
        {
            result.Insert(0, c.Position);
            c = c.Parent;
        }

        if (c != start)
            return Array.Empty<Point>();

        result = SimplifyPath(result);

        if (result.Count > 2)
        {
            if (AreOnSameLine(result[result.Count - 2], result[result.Count - 1], goal.Position))
                result.RemoveAt(result.Count - 1);

            if (AreOnSameLine(start.Position, result[0], result[1]))
                result.RemoveAt(0);
        }

        return result;
    }

    private static bool AreOnSameLine(Point prev, Point curr, Point next)
        => (prev.X == curr.X && curr.X == next.X) || (prev.Y == curr.Y && curr.Y == next.Y);

    private static List<Point> SimplifyPath(List<Point> path)
    {
        for (var i = path.Count - 2; i > 0; i--)
        {
            if (AreOnSameLine(path[i + 1], path[i], path[i - 1]))
                path.RemoveAt(i);
        }

        return path;
    }

    private static bool IsChangeOfDirection(Point a, Point b, Point c)
    {
        if (a.X == b.X && b.X != c.X)
            return true;
        if (a.Y == b.Y && b.Y != c.Y)
            return true;
        return false;
    }

    private static double Heuristic(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
}
