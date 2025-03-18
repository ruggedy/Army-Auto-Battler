using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpatialDataStructures;

class Program
{
    static void Main(string[] args)
    {
    //     // Define the boundaries of our space
    //     double minX = 0;
    //     double minY = 0;
    //     double maxX = 1000;
    //     double maxY = 1000;
    //
    //     // Example 1: Using Point Quad Tree
    //     Console.WriteLine("=== Point Quad Tree Example ===");
    //     var pointQuadTree = new PointQuadTree<string>(minX, minY, maxX, maxY);
    //     
    //     // Insert some sample data
    //     pointQuadTree.Insert(new Point<string>(100, 100, "Point 1"));
    //     pointQuadTree.Insert(new Point<string>(150, 150, "Point 2"));
    //     pointQuadTree.Insert(new Point<string>(200, 200, "Point 3"));
    //     pointQuadTree.Insert(new Point<string>(500, 500, "Point 4"));
    //     pointQuadTree.Insert(new Point<string>(800, 800, "Point 5"));
    //     
    //     // Perform a radius search
    //     Console.WriteLine("Finding points within radius 100 of (150, 150):");
    //     var nearbyPoints = pointQuadTree.FindPointsInRadius(150, 150, 100);
    //     
    //     foreach (var point in nearbyPoints)
    //     {
    //         Console.WriteLine($"Found: {point.Data} at ({point.X}, {point.Y})");
    //     }
    //     
    //     // Example 2: Using Region Quad Tree
    //     Console.WriteLine("\n=== Region Quad Tree Example ===");
    //     var regionQuadTree = new RegionQuadTree<string>(minX, minY, maxX, maxY);
    //     
    //     // Insert the same data
    //     regionQuadTree.Insert(new Point<string>(100, 100, "Point 1"));
    //     regionQuadTree.Insert(new Point<string>(150, 150, "Point 2"));
    //     regionQuadTree.Insert(new Point<string>(200, 200, "Point 3"));
    //     regionQuadTree.Insert(new Point<string>(500, 500, "Point 4"));
    //     regionQuadTree.Insert(new Point<string>(800, 800, "Point 5"));
    //     
    //     // Perform the same radius search
    //     Console.WriteLine("Finding points within radius 100 of (150, 150):");
    //     nearbyPoints = regionQuadTree.FindPointsInRadius(150, 150, 100);
    //     
    //     foreach (var point in nearbyPoints)
    //     {
    //         Console.WriteLine($"Found: {point.Data} at ({point.X}, {point.Y})");
    //     }
    //     
    //     // Performance comparison with more data points
    //     Console.WriteLine("\n=== Performance Comparison ===");
    //     ComparePerformance(1000, 10);
    // }
    //
    // static void ComparePerformance(int numPoints, int numQueries)
    // {
    //     double minX = 0;
    //     double minY = 0;
    //     double maxX = 1000;
    //     double maxY = 1000;
    //     
    //     var pointQuadTree = new PointQuadTree<int>(minX, minY, maxX, maxY);
    //     var regionQuadTree = new RegionQuadTree<int>(minX, minY, maxX, maxY);
    //     var points = new List<Point<int>>();
    //     
    //     // Generate random points
    //     Random random = new Random(42); // Use seed for reproducibility
    //     for (int i = 0; i < numPoints; i++)
    //     {
    //         double x = random.NextDouble() * maxX;
    //         double y = random.NextDouble() * maxY;
    //         points.Add(new Point<int>(x, y, i));
    //     }
    //     
    //     // Insert into both trees
    //     Stopwatch sw = new Stopwatch();
    //     
    //     // Measure Point Quad Tree insertion
    //     sw.Start();
    //     foreach (var point in points)
    //     {
    //         pointQuadTree.Insert(point);
    //     }
    //     sw.Stop();
    //     Console.WriteLine($"Point Quad Tree insertion time: {sw.ElapsedMilliseconds}ms");
    //     
    //     // Measure Region Quad Tree insertion
    //     sw.Restart();
    //     foreach (var point in points)
    //     {
    //         regionQuadTree.Insert(point);
    //     }
    //     sw.Stop();
    //     Console.WriteLine($"Region Quad Tree insertion time: {sw.ElapsedMilliseconds}ms");
    //     
    //     // Generate random query points and radii
    //     var queries = new List<Tuple<double, double, double>>();
    //     for (int i = 0; i < numQueries; i++)
    //     {
    //         double x = random.NextDouble() * maxX;
    //         double y = random.NextDouble() * maxY;
    //         double radius = random.NextDouble() * 100; // Random radius between 0 and 100
    //         queries.Add(new Tuple<double, double, double>(x, y, radius));
    //     }
    //     
    //     // Measure Point Quad Tree queries
    //     sw.Restart();
    //     int totalPointsFound = 0;
    //     foreach (var query in queries)
    //     {
    //         var results = pointQuadTree.FindPointsInRadius(query.Item1, query.Item2, query.Item3);
    //         totalPointsFound += results.Count;
    //     }
    //     sw.Stop();
    //     Console.WriteLine($"Point Quad Tree query time: {sw.ElapsedMilliseconds}ms, found {totalPointsFound} points");
    //     
    //     // Measure Region Quad Tree queries
    //     sw.Restart();
    //     totalPointsFound = 0;
    //     foreach (var query in queries)
    //     {
    //         var results = regionQuadTree.FindPointsInRadius(query.Item1, query.Item2, query.Item3);
    //         totalPointsFound += results.Count;
    //     }
    //     sw.Stop();
    //     Console.WriteLine($"Region Quad Tree query time: {sw.ElapsedMilliseconds}ms, found {totalPointsFound} points");
    }
}
