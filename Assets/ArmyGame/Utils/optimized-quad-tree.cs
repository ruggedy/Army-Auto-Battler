using System;
using System.Collections.Generic;
using System.Linq;

namespace SpatialDataStructures.Advanced
{
    /// <summary>
    /// An optimized region quad tree implementation with bulk loading and additional query capabilities
    /// </summary>
    public class OptimizedRegionQuadTree<T>
    {
        // Reusing the same Point class from previous examples
        private class QuadTreeNode
        {
            // The four quadrants
            public QuadTreeNode NorthWest { get; set; }
            public QuadTreeNode NorthEast { get; set; }
            public QuadTreeNode SouthWest { get; set; }
            public QuadTreeNode SouthEast { get; set; }

            // Points contained in this node (only used in leaf nodes)
            public List<Point<T>> Points { get; }

            // Bounding box for optimization
            public BoundingBox Bounds { get; set; }

            // Flag indicating if this node is subdivided
            public bool IsSubdivided => NorthWest != null;

            public QuadTreeNode(BoundingBox bounds)
            {
                Points = new List<Point<T>>();
                Bounds = bounds;
            }

            /// <summary>
            /// Subdivides this node into four child nodes
            /// </summary>
            public void Subdivide()
            {
                double midX = (Bounds.MinX + Bounds.MaxX) / 2;
                double midY = (Bounds.MinY + Bounds.MaxY) / 2;

                NorthWest = new QuadTreeNode(new BoundingBox(Bounds.MinX, midY, midX, Bounds.MaxY));
                NorthEast = new QuadTreeNode(new BoundingBox(midX, midY, Bounds.MaxX, Bounds.MaxY));
                SouthWest = new QuadTreeNode(new BoundingBox(Bounds.MinX, Bounds.MinY, midX, midY));
                SouthEast = new QuadTreeNode(new BoundingBox(midX, Bounds.MinY, Bounds.MaxX, midY));
            }
        }

        /// <summary>
        /// Represents a rectangular bounding box
        /// </summary>
        public struct BoundingBox
        {
            public double MinX { get; }
            public double MinY { get; }
            public double MaxX { get; }
            public double MaxY { get; }

            public BoundingBox(double minX, double minY, double maxX, double maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            /// <summary>
            /// Checks if this bounding box contains a point
            /// </summary>
            public bool Contains(Point<T> point)
            {
                return point.X >= MinX && point.X <= MaxX && 
                       point.Y >= MinY && point.Y <= MaxY;
            }

            /// <summary>
            /// Checks if this bounding box intersects with another bounding box
            /// </summary>
            public bool Intersects(BoundingBox other)
            {
                return !(other.MinX > MaxX || 
                         other.MaxX < MinX || 
                         other.MinY > MaxY || 
                         other.MaxY < MinY);
            }

            /// <summary>
            /// Checks if this bounding box intersects with a circle
            /// </summary>
            public bool IntersectsCircle(double centerX, double centerY, double radius)
            {
                // Find the closest point to the circle within the rectangle
                double closestX = Math.Max(MinX, Math.Min(centerX, MaxX));
                double closestY = Math.Max(MinY, Math.Min(centerY, MaxY));

                // Calculate the distance to the closest point
                double distanceX = centerX - closestX;
                double distanceY = centerY - closestY;
                double distanceSquared = distanceX * distanceX + distanceY * distanceY;

                // If the distance is less than the radius, the circle and rectangle intersect
                return distanceSquared <= radius * radius;
            }

            public override string ToString()
            {
                return $"({MinX}, {MinY}) - ({MaxX}, {MaxY})";
            }
        }

        private readonly QuadTreeNode root;
        private readonly int maxDepth;
        private readonly int maxPointsPerNode;
        private int size;

        /// <summary>
        /// Creates a new Optimized Region Quad Tree
        /// </summary>
        /// <param name="bounds">The bounding box defining the space covered by this quad tree</param>
        /// <param name="maxPointsPerNode">Maximum points per leaf node before splitting</param>
        /// <param name="maxDepth">Maximum depth to prevent excessive recursion</param>
        public OptimizedRegionQuadTree(BoundingBox bounds, int maxPointsPerNode = 4, int maxDepth = 10)
        {
            this.maxDepth = maxDepth;
            this.maxPointsPerNode = maxPointsPerNode;
            root = new QuadTreeNode(bounds);
            size = 0;
        }

        /// <summary>
        /// Gets the number of points in the quad tree
        /// </summary>
        public int Size => size;

        /// <summary>
        /// Bulk loads a collection of points into the quad tree
        /// </summary>
        public void BulkLoad(IEnumerable<Point<T>> points)
        {
            var validPoints = points.Where(p => root.Bounds.Contains(p)).ToList();

            // Simple approach: build a sorted list and recursively divide it
            var sortedByX = validPoints.OrderBy(p => p.X).ToList();
            var sortedByY = validPoints.OrderBy(p => p.Y).ToList();

            BulkLoadRecursive(root, sortedByX, sortedByY, 0);
            size += validPoints.Count;
        }

        private void BulkLoadRecursive(QuadTreeNode node, List<Point<T>> pointsSortedByX, 
                                      List<Point<T>> pointsSortedByY, int depth)
        {
            int count = pointsSortedByX.Count;

            // Base cases
            if (count == 0)
                return;

            if (count <= maxPointsPerNode || depth >= maxDepth)
            {
                node.Points.AddRange(pointsSortedByX);
                return;
            }

            // Subdivide
            node.Subdivide();

            double midX = (node.Bounds.MinX + node.Bounds.MaxX) / 2;
            double midY = (node.Bounds.MinY + node.Bounds.MaxY) / 2;

            // Partition points into quadrants
            var nwPointsX = new List<Point<T>>();
            var nePointsX = new List<Point<T>>();
            var swPointsX = new List<Point<T>>();
            var sePointsX = new List<Point<T>>();

            // Partition X-sorted points
            foreach (var point in pointsSortedByX)
            {
                bool isNorth = point.Y >= midY;
                bool isEast = point.X >= midX;

                if (isNorth && !isEast) nwPointsX.Add(point);
                else if (isNorth && isEast) nePointsX.Add(point);
                else if (!isNorth && !isEast) swPointsX.Add(point);
                else sePointsX.Add(point);
            }

            // Partition Y-sorted points
            var nwPointsY = new List<Point<T>>();
            var nePointsY = new List<Point<T>>();
            var swPointsY = new List<Point<T>>();
            var sePointsY = new List<Point<T>>();

            foreach (var point in pointsSortedByY)
            {
                bool isNorth = point.Y >= midY;
                bool isEast = point.X >= midX;

                if (isNorth && !isEast) nwPointsY.Add(point);
                else if (isNorth && isEast) nePointsY.Add(point);
                else if (!isNorth && !isEast) swPointsY.Add(point);
                else sePointsY.Add(point);
            }

            // Recursively bulk load each quadrant
            BulkLoadRecursive(node.NorthWest, nwPointsX, nwPointsY, depth + 1);
            BulkLoadRecursive(node.NorthEast, nePointsX, nePointsY, depth + 1);
            BulkLoadRecursive(node.SouthWest, swPointsX, swPointsY, depth + 1);
            BulkLoadRecursive(node.SouthEast, sePointsX, sePointsY, depth + 1);
        }

        /// <summary>
        /// Inserts a point into the quad tree
        /// </summary>
        public bool Insert(Point<T> point)
        {
            // Verify point is within bounds
            if (!root.Bounds.Contains(point))
            {
                return false;
            }

            bool inserted = InsertIntoNode(root, point, 0);
            if (inserted)
            {
                size++;
            }
            return inserted;
        }

        private bool InsertIntoNode(QuadTreeNode node, Point<T> point, int depth)
        {
            // If we reached maximum depth, don't subdivide further
            if (depth >= maxDepth)
            {
                node.Points.Add(point);
                return true;
            }

            // If this node is not subdivided yet and has room, just add the point
            if (!node.IsSubdivided && node.Points.Count < maxPointsPerNode)
            {
                node.Points.Add(point);
                return true;
            }

            // If this node is not subdivided but full, subdivide it
            if (!node.IsSubdivided)
            {
                // Subdivide the node
                node.Subdivide();

                // Redistribute existing points to child nodes
                foreach (var existingPoint in node.Points)
                {
                    InsertPointIntoQuadrant(node, existingPoint);
                }

                // Clear the points from this node as they've been moved to children
                node.Points.Clear();
            }

            // Insert the new point into the appropriate quadrant
            return InsertPointIntoQuadrant(node, point, depth);
        }

        private bool InsertPointIntoQuadrant(QuadTreeNode node, Point<T> point, int depth = 0)
        {
            // Determine which quadrant the point belongs to
            double midX = (node.Bounds.MinX + node.Bounds.MaxX) / 2;
            double midY = (node.Bounds.MinY + node.Bounds.MaxY) / 2;

            bool isNorth = point.Y >= midY;
            bool isEast = point.X >= midX;

            // Insert into the appropriate quadrant
            if (isNorth && isEast)
                return InsertIntoNode(node.NorthEast, point, depth + 1);
            else if (isNorth && !isEast)
                return InsertIntoNode(node.NorthWest, point, depth + 1);
            else if (!isNorth && isEast)
                return InsertIntoNode(node.SouthEast, point, depth + 1);
            else
                return InsertIntoNode(node.SouthWest, point, depth + 1);
        }

        /// <summary>
        /// Finds all points within a specified radius of the given center point
        /// </summary>
        public List<Point<T>> FindPointsInRadius(double centerX, double centerY, double radius)
        {
            var result = new List<Point<T>>();

            // First check if the search circle intersects with the quad tree boundaries
            if (!root.Bounds.IntersectsCircle(centerX, centerY, radius))
            {
                return result; // No intersection
            }

            // Start the recursive search
            FindPointsInRadiusRecursive(root, centerX, centerY, radius, result);
            return result;
        }

        private void FindPointsInRadiusRecursive(QuadTreeNode node, double centerX, double centerY, 
                                               double radius, List<Point<T>> result)
        {
            if (node == null)
                return;

            // Check if this node's bounding box intersects with the search circle
            if (!node.Bounds.IntersectsCircle(centerX, centerY, radius))
                return;

            // If this is a leaf node with points, check each point
            if (!node.IsSubdivided)
            {
                foreach (var point in node.Points)
                {
                    double distance = point.DistanceTo(centerX, centerY);
                    if (distance <= radius)
                    {
                        result.Add(point);
                    }
                }
                return;
            }

            // Check each quadrant
            FindPointsInRadiusRecursive(node.NorthWest, centerX, centerY, radius, result);
            FindPointsInRadiusRecursive(node.NorthEast, centerX, centerY, radius, result);
            FindPointsInRadiusRecursive(node.SouthWest, centerX, centerY, radius, result);
            FindPointsInRadiusRecursive(node.SouthEast, centerX, centerY, radius, result);
        }

        /// <summary>
        /// Finds all points within a specified rectangle
        /// </summary>
        public List<Point<T>> FindPointsInRectangle(double minX, double minY, double maxX, double maxY)
        {
            var result = new List<Point<T>>();
            var searchBounds = new BoundingBox(minX, minY, maxX, maxY);

            // Check if the search rectangle intersects with the quad tree boundaries
            if (!root.Bounds.Intersects(searchBounds))
            {
                return result; // No intersection
            }

            // Start the recursive search
            FindPointsInRectangleRecursive(root, searchBounds, result);
            return result;
        }

        private void FindPointsInRectangleRecursive(QuadTreeNode node, BoundingBox searchBounds, List<Point<T>> result)
        {
            if (node == null)
                return;

            // Check if this node's bounding box intersects with the search rectangle
            if (!node.Bounds.Intersects(searchBounds))
                return;

            // If this is a leaf node with points, check each point
            if (!node.IsSubdivided)
            {
                foreach (var point in node.Points)
                {
                    if (point.X >= searchBounds.MinX && point.X <= searchBounds.MaxX &&
                        point.Y >= searchBounds.MinY && point.Y <= searchBounds.MaxY)
                    {
                        result.Add(point);
                    }
                }
                return;
            }

            // Check each quadrant
            FindPointsInRectangleRecursive(node.NorthWest, searchBounds, result);
            FindPointsInRectangleRecursive(node.NorthEast, searchBounds, result);
            FindPointsInRectangleRecursive(node.SouthWest, searchBounds, result);
            FindPointsInRectangleRecursive(node.SouthEast, searchBounds, result);
        }

        /// <summary>
        /// Finds the nearest point to the specified coordinates
        /// </summary>
        public Point<T> FindNearestPoint(double x, double y, double maxDistance = double.MaxValue)
        {
            var searchPoint = new Point<double>(x, y);
            var bestDistance = maxDistance;
            Point<T> bestPoint = null;

            FindNearestPointRecursive(root, searchPoint, ref bestPoint, ref bestDistance);
            return bestPoint;
        }

        private void FindNearestPointRecursive(QuadTreeNode node, Point<double> searchPoint, 
                                             ref Point<T> bestPoint, ref double bestDistance)
        {
            if (node == null)
                return;

            // Calculate minimum possible distance to this node's bounding box
            double minPossibleDistance = CalculateMinimumDistance(searchPoint, node.Bounds);

            // If this node's bounding box is farther than the best distance we've found, skip it
            if (minPossibleDistance > bestDistance)
                return;

            // If this is a leaf node with points, check each point
            if (!node.IsSubdivided)
            {
                foreach (var point in node.Points)
                {
                    double distance = Math.Sqrt(
                        (point.X - searchPoint.X) * (point.X - searchPoint.X) + 
                        (point.Y - searchPoint.Y) * (point.Y - searchPoint.Y)
                    );

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPoint = point;
                    }
                }
                return;
            }

            // Determine which quadrant would contain the search point
            double midX = (node.Bounds.MinX + node.Bounds.MaxX) / 2;
            double midY = (node.Bounds.MinY + node.Bounds.MaxY) / 2;

            bool isNorth = searchPoint.Y >= midY;
            bool isEast = searchPoint.X >= midX;

            // First search the quadrant that would contain the search point
            if (isNorth && isEast)
            {
                FindNearestPointRecursive(node.NorthEast, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.NorthWest, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.SouthEast, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.SouthWest, searchPoint, ref bestPoint, ref bestDistance);
            }
            else if (isNorth && !isEast)
            {
                FindNearestPointRecursive(node.NorthWest, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.NorthEast, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.SouthWest, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.SouthEast, searchPoint, ref bestPoint, ref bestDistance);
            }
            else if (!isNorth && isEast)
            {
                FindNearestPointRecursive(node.SouthEast, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.NorthEast, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.SouthWest, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.NorthWest, searchPoint, ref bestPoint, ref bestDistance);
            }
            else
            {
                FindNearestPointRecursive(node.SouthWest, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.NorthWest, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.SouthEast, searchPoint, ref bestPoint, ref bestDistance);
                FindNearestPointRecursive(node.NorthEast, searchPoint, ref bestPoint, ref bestDistance);
            }
        }

        private double CalculateMinimumDistance(Point<double> point, BoundingBox bounds)
        {
            double dx = 0;
            double dy = 0;

            // Calculate horizontal distance
            if (point.X < bounds.MinX)
                dx = bounds.MinX - point.X;
            else if (point.X > bounds.MaxX)
                dx = point.X - bounds.MaxX;

            // Calculate vertical distance
            if (point.Y < bounds.MinY)
                dy = bounds.MinY - point.Y;
            else if (point.Y > bounds.MaxY)
                dy = point.Y - bounds.MaxY;

            // If the point is inside the bounding box, the distance is 0
            if (dx == 0 && dy == 0)
                return 0;

            // Calculate Euclidean distance
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
