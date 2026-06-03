using System;
using System.Diagnostics;
using Nodely;
using Nodely.Algorithms;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

// A quick Stopwatch harness for the headless engine hot paths (routing + path generation + serialization).
// Run: dotnet run -c Release --project bench/Nodely.Benchmarks
const int nodeCount = 2000;

var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
diagram.SetContainer(new Rectangle(0, 0, 1920, 1080));

var sw = Stopwatch.StartNew();
var nodes = new NodeModel[nodeCount];
for (var i = 0; i < nodeCount; i++)
{
    var node = new NodeModel(new Point(i % 50 * 120, i / 50 * 80)) { Size = new Size(100, 50) };
    diagram.Nodes.Add(node);
    nodes[i] = node;
}
sw.Stop();
Console.WriteLine($"Add {nodeCount} nodes:                 {sw.ElapsedMilliseconds,5} ms");

sw.Restart();
var linkCount = 0;
for (var i = 0; i < nodeCount - 1; i++)
{
    diagram.Links.Add(new LinkModel(nodes[i], nodes[i + 1]));
    linkCount++;
    if (i + 2 < nodeCount)
    {
        diagram.Links.Add(new LinkModel(nodes[i], nodes[i + 2]));
        linkCount++;
    }
}
sw.Stop();
Console.WriteLine($"Add {linkCount} links (+ first path):   {sw.ElapsedMilliseconds,5} ms");

sw.Restart();
foreach (var link in diagram.Links)
    link.Refresh(); // route + smooth-bezier path generation for every link
sw.Stop();
Console.WriteLine($"Regenerate {linkCount} link paths:     {sw.ElapsedMilliseconds,5} ms");

sw.Restart();
LayeredLayout.Arrange(diagram);
sw.Stop();
Console.WriteLine($"Layered layout ({nodeCount} nodes):       {sw.ElapsedMilliseconds,5} ms");

sw.Restart();
var json = DiagramSerializer.Serialize(diagram);
sw.Stop();
Console.WriteLine($"Serialize ({json.Length / 1024} KB):              {sw.ElapsedMilliseconds,5} ms");

sw.Restart();
DiagramSerializer.Deserialize(new NodelyDiagram(null, false), json);
sw.Stop();
Console.WriteLine($"Deserialize:                       {sw.ElapsedMilliseconds,5} ms");
