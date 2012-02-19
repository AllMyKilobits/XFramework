using System;
using System.Collections.Generic;

namespace XF
{
    static public class Geo
    {
        /// <summary>Finds the lowest distance of point P to a line segment defined by points A and B</summary>        
        static public float distance_from_point_to_segment(crds2 a, crds2 b, crds2 p)
        {
            float l2 = a.dist_squared(b);
            if (l2 < 0.000001f) return a.dist(p); // case : a == b
            float t = crds2.dot(p-a, b-a) / l2;
            if (t < 0f) return a.dist(p);
            if (t > 1f) return b.dist(p);
            crds2 proj =  a.lerp(b, t);
            return proj.dist(p);
        }

        /// <summary>Returns the point of projection of point P on a segment defined by A and B, expressed as a float interpolation value between A and B</returns>
        static public float projection_of_point_on_segment(crds2 a, crds2 b, crds2 p)
        {            
            return crds2.dot(p - a, b - a) / a.dist_squared(b);
        }

        /// <summary> Returns the point of intersection of two segments, where first segment is defined by A and B, and 2nd segment by C and D. </summary>        
        /// <param name="A">Point A</param><param name="B">Point B</param><param name="C">Point C</param><param name="D">Point D</param><param name="intersection_point">Intersection point, if any</param>
        /// <returns>True if there is an intersection, and if so, fills intersection_point with the exact location</returns>
        static public bool segment_segment_intersection(crds2 A, crds2 B, crds2 C, crds2 D, out crds2 intersection_point)
        {
            intersection_point = crds2.zero;

            var z = (D.y - C.y) * (B.x - A.x) - (D.x - C.x) * (B.y - A.y);
            if (z.approximately(0f)) return false;

            var U1 = ((D.x - C.x) * (A.y - C.y) - (D.y - C.y) * (A.x - C.x)) / z;
            var U2 = ((B.x - A.x) * (A.y - C.y) - (B.y - A.y) * (A.x - C.x)) / z;
            if ((U1 >= 0f && U1 <= 1f) && (U2 >= 0f && U2 <= 1f))
            {
                intersection_point = A.lerp(B, U1);
                return true;
            }
            return false;

        }

        /// <summary>same as segment-segment int. but points only define the line, don't limit it</summary>        
        static public bool line_line_intersection(crds2 A, crds2 B, crds2 C, crds2 D, out crds2 intersection_point) 
        {
            intersection_point = crds2.zero;

            var z = (D.y - C.y) * (B.x - A.x) - (D.x - C.x) * (B.y - A.y);
            if (z.approximately(0f)) return false;

            var U1 = ((D.x - C.x) * (A.y - C.y) - (D.y - C.y) * (A.x - C.x)) / z;
            var U2 = ((B.x - A.x) * (A.y - C.y) - (B.y - A.y) * (A.x - C.x)) / z;
            
            intersection_point = A.lerp(B, U1);
            return true;
            
        }

        static public crds2 random_point_in_triangle(crds2 A, crds2 B, crds2 C)
        {
            var a = Random.range(0f, 1f);
            var b = Random.range(0f, 1f);
            if (a + b > 1f) { a = 1f - a; b = 1f - b; }
            var c = 1f - a - b;
            return A * a + B * b + C * c;
        }

        //static public bool random_point_in_polygon(crds2[] poly)
        //{

        //}

        /// <summary>concept nabbed from http://www.blackpawn.com/texts/pointinpoly/default.html </summary>        
        /// <returns>true if point p is in triangle (A, B, C)</returns>
        static public bool point_in_triangle(crds2 A, crds2 B, crds2 C, crds2 p)
        {
            // Compute vectors        
            var v0 = C - A;
            var v1 = B - A;
            var v2 = p - A;
                
            // Compute dot products
            var dot00 = crds2.dot(v0, v0);
            var dot01 = crds2.dot(v0, v1);
            var dot02 = crds2.dot(v0, v2);
            var dot11 = crds2.dot(v1, v1);
            var dot12 = crds2.dot(v1, v2);

            // Compute barycentric coordinates
            var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            var v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            // Check if point is in triangle
            return (u > 0f) && (v > 0f) && (u + v < 1f);
        }

        /// <summary>returns whether a point p lies to the LEFT side of a line defined by points line_a and line_b</summary>        
        /// <returns>true if the point is "left" of the line as defined by direction from line_a to line_b</returns>
        static public bool is_left(crds2 line_a, crds2 line_b, crds2 p)
        {
            return ((line_b.x - line_a.x) * (p.y - line_a.y) - (line_b.y - line_a.y) * (p.x - line_a.x)) > 0;            
        }

        /// <summary>Returns a boolean stating whether there is ANY overlap between two triangles</summary>
        /// <param name="first_triangle">an array of exactly THREE 2d vertices defining the FIRST triangle</param>
        /// <param name="second_triangle">an array of exactly THREE 2d vertices defining the SECOND triangle</param>        
        static public bool triangle_triangle_intersection(crds2[] first_triangle, crds2[] second_triangle)
        {
            if (first_triangle.Length != 3 || second_triangle.Length != 3) throw new Exception("triangle triangle intersection error - not a triangle!");

            // if any vertex of triangle A is in triangle B, or any vertex of triangle B is in triangle A, return true
            for (int i = 0; i < 3; i++) if (point_in_triangle(first_triangle[0], first_triangle[1], first_triangle[2], second_triangle[i])) return true;
            for (int i = 0; i < 3; i++) if (point_in_triangle(second_triangle[0], second_triangle[1], second_triangle[2], first_triangle[i])) return true;                
            
            // no luck yet huh? ok, if any two segments intersect overlap

            crds2 p;

            for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
            {
                var a2 = a + 1; if (a2 == 3) a2 = 0;
                var b2 = b + 1; if (b2 == 3) b2 = 0;
                if (segment_segment_intersection(first_triangle[a], first_triangle[a2], second_triangle[b], second_triangle[b2], out p)) return true;                
            }

            // nope? Huh. Guess there is no intersection. Huh.
            return false;

        }

        public struct rect
        {
            public crds2 lo;
            public crds2 hi;
            public rect(float x0, float y0, float w, float h)
            {
                lo = new crds2(x0, y0);
                hi = new crds2(x0 + w, y0 + h);
            }

            public bool contains(crds2 point, float tolerance = 0f)
            {
                if ( point.x > lo.x - tolerance 
                  && point.x < hi.x + tolerance
                  && point.y > lo.y - tolerance
                  && point.y < hi.y + tolerance) return true;

                return false;
            }

        }


        static public bool are_points_clockwise(crds2[] polygon)
        {
            var sum = 0f;

            for (int i = 0; i < polygon.Length-1; i++)
            {
                sum += (polygon[i+1].x - polygon[i ].x) * (polygon[i + 1].y + polygon[i].y);
            }

            return sum > 0;
        }

        static public float surface_of_polygon(crds2[] polygon)
        {
            var n = polygon.Length;
            float sum = 0f;
            for (int i = 0; i < n; i++)
            {
                var j = i + 1; if (j == n) j = 0;
                sum += polygon[i].x * polygon[j].y - polygon[j].x * polygon[i].y;
            }
            return sum * 0.5f; // SUM is now the full SURFACE of the polygon
        }

        static public crds2 centroid_of_polygon(crds2[] polygon)
        {
            var surface = surface_of_polygon(polygon);
            var n = polygon.Length; crds2 coordz = crds2.zero;
            for (int i = 0; i < n; i++) { var j = i + 1; if (j == n) j = 0;
                coordz.x += (polygon[i].x + polygon[j].x) * (polygon[i].x * polygon[j].y - polygon[j].x * polygon[i].y);
                coordz.y += (polygon[i].y + polygon[j].y) * (polygon[i].x * polygon[j].y - polygon[j].x * polygon[i].y);
            }
            coordz.x /= 6 * surface;
            coordz.y /= 6 * surface;
            return coordz;
        }

        static public crds2 bezier_at(crds2 A, crds2 M, crds2 B, float t) // this also works perfectly if you change crds2 into crds3
        {
            var q = 1f - t;
            return ((A * q + M * t) * q + (M * q + B * t) * t);
        }

    }
}
