using System;
using System.Collections.Generic;

namespace SpatialDataStructures
{
    /// <summary>
    /// Represents a 2D point with optional attached data
    /// </summary>
    /// <typeparam name="T">Type of data associated with this point</typeparam>
    public class Point<T>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public T Data { get; set; }

        public Point(double x, double y, T data = default)
        {
            X = x;
            Y = y;
            Data = data;
        }

        public double DistanceTo(Point<T> other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public double DistanceTo(double x, double y)
        {
            double dx = X - x;
            double dy = Y - y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// Represents a point quad tree for efficient spatial queries
    /// </summary>
    /// <typeparam name="T">Type of data associated with points</typeparam>
    public class PointQuadTree<T>
    {
        private class Node
        {
            public Point<T> Point { get; set; }
            public Node NorthWest { get; set; }
            public Node NorthEast { get; set; }
            public Node SouthWest { get; set; }
            public Node SouthEast { get; set; }

            public Node(Point<T> point)
            {
                Point = point;
            }

            public bool IsLeaf()
            {
                return NorthWest == null && NorthEast == null && 
                       SouthWest == null && SouthEast == null;
            }
        }

        private Node root;
        private readonly double minX;
        private readonly double minY;
        private readonly double maxX;
        private readonly double maxY;
        private readonly int maxDepth;

        /// <summary>
        /// Creates a new Point Quad Tree
        /// </summary>
        /// <param name="minX">Minimum X coordinate of the quad tree boundary</param>
        /// <param name="minY">Minimum Y coordinate of the quad tree boundary</param>
        /// <param name="maxX">Maximum X coordinate of the quad tree boundary</param>
        /// <param name="maxY">Maximum Y coordinate of the quad tree boundary</param>
        /// <param name="maxDepth">Maximum depth to prevent excessive recursion</param>
        public PointQuadTree(double minX, double minY, double maxX, double maxY, int maxDepth = 10)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
            this.maxDepth = maxDepth;
        }

        /// <summary>
        /// Inserts a point into the quad tree
        /// </summary>
        public void Insert(Point<T> point)
        {
            // Verify point is within bounds
            if (point.X < minX || point.X > maxX || point.Y < minY || point.Y > maxY)
            {
                throw new ArgumentOutOfRangeException(nameof(point), "Point is outside the boundaries of this quad tree");
            }

            if (root == null)
            {
                root = new Node(point);
                return;
            }

            InsertIntoNode(root, point, minX, minY, maxX, maxY, 0);
        }

        private void InsertIntoNode(Node node, Point<T> point, double nodeMinX, double nodeMinY, 
                                   double nodeMaxX, double nodeMaxY, int depth)
        {
            // If we reached maximum depth, we can't subdivide further
            if (depth >= maxDepth)
            {
                // In a real implementation, we might want to store multiple points at a leaf
                Console.WriteLine($"Warning: Maximum depth reached at ({nodeMinX}, {nodeMinY}, {nodeMaxX}, {nodeMaxY})");
                return;
            }

            // Calculate the center of this node's area
            double midX = (nodeMinX + nodeMaxX) / 2;
            double midY = (nodeMinY + nodeMaxY) / 2;

            // Determine which quadrant the point belongs to
            bool isNorth = point.Y >= midY;
            bool isEast = point.X >= midX;

            Node child = null;
            double childMinX = 0, childMinY = 0, childMaxX = 0, childMaxY = 0;

            if (isNorth && isEast) // Northeast
            {
                childMinX = midX;
                childMinY = midY;
                childMaxX = nodeMaxX;
                childMaxY = nodeMaxY;
                child = node.NorthEast;
                if (child == null)
                {
                    node.NorthEast = new Node(point);
                    return;
                }
            }
            else if (isNorth && !isEast) // Northwest
            {
                childMinX = nodeMinX;
                childMinY = midY;
                childMaxX = midX;
                childMaxY = nodeMaxY;
                child = node.NorthWest;
                if (child == null)
                {
                    node.NorthWest = new Node(point);
                    return;
                }
            }
            else if (!isNorth && isEast) // Southeast
            {
                childMinX = midX;
                childMinY = nodeMinY;
                childMaxX = nodeMaxX;
                childMaxY = midY;
                child = node.SouthEast;
                if (child == null)
                {
                    node.SouthEast = new Node(point);
                    return;
                }
            }
            else // Southwest
            {
                childMinX = nodeMinX;
                childMinY = nodeMinY;
                childMaxX = midX;
                childMaxY = midY;
                child = node.SouthWest;
                if (child == null)
                {
                    node.SouthWest = new Node(point);
                    return;
                }
            }

            // Continue inserting into the child node
            InsertIntoNode(child, point, childMinX, childMinY, childMaxX, childMaxY, depth + 1);
        }

        /// <summary>
        /// Finds all points within a specified radius of the given center point
        /// </summary>
        /// <param name="centerX">X coordinate of the search center</param>
        /// <param name="centerY">Y coordinate of the search center</param>
        /// <param name="radius">Search radius</param>
        /// <returns>A list of points within the specified radius</returns>
        public List<Point<T>> FindPointsInRadius(double centerX, double centerY, double radius)
        {
            var result = new List<Point<T>>();

            if (root == null)
                return result;

            // First check if the search circle intersects with the quad tree boundaries
            double closestX = Math.Max(minX, Math.Min(centerX, maxX));
            double closestY = Math.Max(minY, Math.Min(centerY, maxY));
            double distanceSquared = (closestX - centerX) * (closestX - centerX) + 
                                      (closestY - centerY) * (closestY - centerY);

            if (distanceSquared > radius * radius)
                return result; // No intersection

            // Start the recursive search
            FindPointsInRadiusRecursive(root, centerX, centerY, radius, minX, minY, maxX, maxY, result);
            return result;
        }

        private void FindPointsInRadiusRecursive(Node node, double centerX, double centerY, double radius,
                                                double nodeMinX, double nodeMinY, double nodeMaxX, double nodeMaxY,
                                                List<Point<T>> result)
        {
            if (node == null)
                return;

            // Check if the current node's point is within radius
            double distance = node.Point.DistanceTo(centerX, centerY);
            if (distance <= radius)
            {
                result.Add(node.Point);
            }

            // Calculate midpoints of this node
            double midX = (nodeMinX + nodeMaxX) / 2;
            double midY = (nodeMinY + nodeMaxY) / 2;

            // Check if the search circle overlaps each quadrant and recursively search if needed
            // Northwest quadrant
            if (DoesCircleIntersectRectangle(centerX, centerY, radius, nodeMinX, midY, midX, nodeMaxY))
            {
                FindPointsInRadiusRecursive(node.NorthWest, centerX, centerY, radius,
                                        nodeMinX, midY, midX, nodeMaxY, result);
            }

            // Northeast quadrant
            if (DoesCircleIntersectRectangle(centerX, centerY, radius, midX, midY, nodeMaxX, nodeMaxY))
            {
                FindPointsInRadiusRecursive(node.NorthEast, centerX, centerY, radius,
                                        midX, midY, nodeMaxX, nodeMaxY, result);
            }

            // Southwest quadrant
            if (DoesCircleIntersectRectangle(centerX, centerY, radius, nodeMinX, nodeMinY, midX, midY))
            {
                FindPointsInRadiusRecursive(node.SouthWest, centerX, centerY, radius,
                                        nodeMinX, nodeMinY, midX, midY, result);
            }

            // Southeast quadrant
            if (DoesCircleIntersectRectangle(centerX, centerY, radius, midX, nodeMinY, nodeMaxX, midY))
            {
                FindPointsInRadiusRecursive(node.SouthEast, centerX, centerY, radius,
                                        midX, nodeMinY, nodeMaxX, midY, result);
            }
        }

        /// <summary>
        /// Determines if a circle intersects with a rectangle
        /// </summary>
        private bool DoesCircleIntersectRectangle(double centerX, double centerY, double radius,
                                                double rectMinX, double rectMinY, double rectMaxX, double rectMaxY)
        {
            // Find the closest point to the circle within the rectangle
            double closestX = Math.Max(rectMinX, Math.Min(centerX, rectMaxX));
            double closestY = Math.Max(rectMinY, Math.Min(centerY, rectMaxY));

            // Calculate the distance to the closest point
            double distanceX = centerX - closestX;
            double distanceY = centerY - closestY;
            double distanceSquared = distanceX * distanceX + distanceY * distanceY;

            // If the distance is less than the radius, the circle and rectangle intersect
            return distanceSquared <= radius * radius;
        }
    }
}
