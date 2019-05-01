using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

namespace ConvexHull
{
    /// <summary>
    /// Program for finding Convex Hull using quickhull algorithm
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("specify file path:");
            string filePath = Console.ReadLine();
            List<Point> points = ReadPointsFromFile(filePath);
            Console.WriteLine("All Points:");
            foreach (Point point in points)
            {
                Console.WriteLine(point);
            }
            Console.WriteLine();

            HashSet<Point> convexHull = FindConvexHull(points);
            Console.WriteLine("Found points:");
            foreach (Point point in convexHull)
            {
                Console.WriteLine(point);
            }
        }
        /// <summary>
        /// Reads point list from text file, where each line contains two numbers being coordinates 
        /// separated by space and assigns letter or number-letter to it
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>List of points</returns>
        private static List<Point> ReadPointsFromFile(string filePath)
        {
            List<Point> points = new List<Point>();
            using (StreamReader fs = new StreamReader(filePath))
            {
                string line = fs.ReadLine();
                string[] splitLine;
                int letter = 0;
                string name = "";
                double[] coordinates = new double[2];
                while (line != null)
                {
                    name = "";
                    splitLine = line.Split(' ');
                    coordinates[0] = double.Parse(splitLine[0]);
                    coordinates[1] = double.Parse(splitLine[1]);
                    if(letter > 25)
                    {
                        name = (letter / 25).ToString();
                    }
                    name += (char)(letter % 26 + 65);

                    points.Add(new Point(coordinates[0], coordinates[1], name));
                    letter++;
                    line = fs.ReadLine();
                }
                return points;
            }
        }
        /// <summary>
        /// Starter method for finding convex hull 
        /// </summary>
        /// <param name="list">List of all points</param>
        /// <returns>Set of convex hull points</returns>
        private static HashSet<Point> FindConvexHull(List<Point> list)
        {
            Point[] borderPoints = FindMinAndMax(list);
            Line border = new Line(borderPoints[0], borderPoints[1]);
            HashSet<Point> above = SelectAboveOrBelow(list.ToHashSet(), border, '+');
            HashSet<Point> below = SelectAboveOrBelow(list.ToHashSet(), border, '-');

            above = FindConvexPoints(above, border, '+');
            below = FindConvexPoints(below, border, '-');

            HashSet<Point> result = new HashSet<Point>();
            result.Add(borderPoints[0]);
            result.Add(borderPoints[1]);
            result.UnionWith(above);
            result.UnionWith(below);
            return result;
        }
        /// <summary>
        /// Finds recursively edge points by finding furthest one from line and checking points lying above/bellow that line recursively
        /// </summary>
        /// <param name="points">List of points o be checked</param>
        /// <param name="border">line to be checked</param>
        /// <param name="v"></param>
        /// <returns>Convex Hull Points</returns>
        private static HashSet<Point> FindConvexPoints(HashSet<Point> points, Line border, char v)
        {
            HashSet<Point> result = new HashSet<Point>();
            Point furthest = border.FindFurthest(points);
            Line firstLine = new Line(border.pointA, furthest);
            Line secondLine = new Line(border.pointB, furthest);
            HashSet<Point> firstSet = SelectAboveOrBelow(points, firstLine, v);
            HashSet<Point> secondSet = SelectAboveOrBelow(points, secondLine, v);
            HashSet<Point> tmp = new HashSet<Point>();
            if (firstSet.Count == 0)
            {
                result.Add(furthest);
            }
            else
            {
                result.UnionWith(FindConvexPoints(firstSet, firstLine, v));
            }
            if (secondSet.Count == 0)
            {
                result.Add(furthest);
            }
            else
            {
                result.UnionWith(FindConvexPoints(secondSet, secondLine, v));
            }
            return result;
        }
        /// <summary>
        /// Selects points that are above or bellow line depending on input
        /// </summary>
        /// <param name="list">List of points to be checked</param>
        /// <param name="border">line to be checked</param>
        /// <param name="v">+ for points above, - for points below line</param>
        /// <returns>List of points above/below line</returns>
        private static HashSet<Point> SelectAboveOrBelow(HashSet<Point> list, Line border, char v)
        {
            HashSet<Point> result = new HashSet<Point>();
            int plus = v == '+' ? 1 : -1;
            int check;
            foreach (Point point in list)
            {
                check = border.CheckIfAbove(point);
                if (check == plus)
                {
                    result.Add(point);
                }
            }
            return result;
        }
        /// <summary>
        /// Finds first pair to divide for upper and lower cases
        /// </summary>
        /// <param name="points">List of all points</param>
        /// <returns>First and last extreme points</returns>
        private static Point[] FindMinAndMax(List<Point> points)
        {
            var result = points.OrderBy(a => a.X).ThenBy(a => a.Y);
            return new Point[] { result.First(), result.Last() };
        }
    }
    /// <summary>
    /// Geometric point
    /// </summary>
    internal class Point
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Point(double x, double y, string name)
        {
            X = x;
            Y = y;
            Name = name;
        }
        public override string ToString()
        {
            return $"{Name}: {X}, {Y}";
        }
    }
    /// <summary>
    /// Geometric line definied 
    /// </summary>
    internal class Line
    {
        public string Name { get; }
        //y = a * x + b
        public double A { get; set; }
        public double B { get; set; }
        public Point pointA { get; set; } = null;
        public Point pointB { get; set; } = null;
        public Line(double a, double b, string name)
        {
            A = a;
            B = b;
            Name = name;
        }
        public Line(Point A, Point B)
        {
            pointA = A;
            pointB = B;
            this.A = (B.Y - A.Y) / (B.X - A.X);
            this.B = (B.X * A.Y - A.X * B.Y) / (B.X - A.X);
            Name = A.Name + B.Name;
        }
        /// <summary>
        /// ToString() override for Line printing
        /// </summary>
        /// <returns>String containing line info</returns>
        public override string ToString()
        {
            return $"{Name}: {A} x + {B}";
        }
        /// <summary>
        /// Finds which point is furthest from line
        /// </summary>
        /// <param name="points">Set of points to be checked</param>
        /// <returns>Point that is furthest from line</returns>
        public Point FindFurthest(HashSet<Point> points)
        {
            Point furthest = null;
            double maxDistance = 0, distance;
            foreach (Point point in points)
            {
                distance = CalculateDistance(point);
                if (distance > maxDistance)
                {
                    furthest = point;
                    maxDistance = distance;
                }
            }
            return furthest;
        }
        /// <summary>
        /// calculates distance of point from line
        /// </summary>
        /// <param name="point">Point to be calculated</param>
        /// <returns>Distance of point from line</returns>
        private double CalculateDistance(Point point)
        {
            Point A = pointA;
            Point B = pointB;
            double distance =
                (Math.Abs((B.Y - A.Y) * point.X - (B.X - A.X) * point.Y + B.X * A.Y - B.Y * A.X) /
                Math.Sqrt(Math.Pow((B.Y - A.Y), 2) + Math.Pow((B.X - A.X), 2)));
            return distance;
        }
        /// <summary>
        /// checking if point is above or below line
        /// </summary>
        /// <param name="point">Point to be checked</param>
        /// <returns>1 for point above, -1 for below, 0 for point on line</returns>
        public int CheckIfAbove(Point point)
        {
            double lineY = A * point.X + B;
            if (lineY == point.Y)
            {
                return 0;
            }
            else if(lineY < point.Y)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
