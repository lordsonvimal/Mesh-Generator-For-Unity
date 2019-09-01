using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delaunay 
{
    //Alternative 1. Triangulate with some algorithm - then flip edges until we have a delaunay triangulation
    public static List<Triangle> TriangulateByFlippingEdges(List<Vector3> sites)
    {
        //Step 1. Triangulate the points with some algorithm
        //Vector3 to vertex
        List<Vertex> vertices = new List<Vertex>();

        for (int i = 0; i < sites.Count; i++)
        {
            vertices.Add(new Vertex(sites[i]));
        }

        //Triangulate the convex hull of the sites
        List<Triangle> triangles = Triangulation.IncrementalTriangulation(vertices);
//        List triangles = TriangulatePoints.TriangleSplitting(vertices);

        //Step 2. Change the structure from triangle to half-edge to make it faster to flip edges
        List<HalfEdge> halfEdges = TransformFromTriangleToHalfEdge(triangles);

        //Step 3. Flip edges until we have a delaunay triangulation
        int safety = 0;

        int flippedEdges = 0;

        while (true)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.Log("Stuck in endless loop");

                break;
            }

            bool hasFlippedEdge = false;

            //Search through all edges to see if we can flip an edge
            for (int i = 0; i < halfEdges.Count; i++)
            {
                HalfEdge thisEdge = halfEdges[i];

                //Is this edge sharing an edge, otherwise its a border, and then we cant flip the edge
                if (thisEdge.oppositeEdge == null)
                {
                    continue;
                }

                //The vertices belonging to the two triangles, c-a are the edge vertices, b belongs to this triangle
                Vertex a = thisEdge.v;
                Vertex b = thisEdge.nextEdge.v;
                Vertex c = thisEdge.prevEdge.v;
                Vertex d = thisEdge.oppositeEdge.nextEdge.v;

                Vector2 aPos = a.GetPos2D_XZ();
                Vector2 bPos = b.GetPos2D_XZ();
                Vector2 cPos = c.GetPos2D_XZ();
                Vector2 dPos = d.GetPos2D_XZ();

                //Use the circle test to test if we need to flip this edge
                if (Geometry.IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
                {
                    //Are these the two triangles that share this edge forming a convex quadrilateral?
                    //Otherwise the edge cant be flipped
                    if (Geometry.IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
                    {
                        //If the new triangle after a flip is not better, then dont flip
                        //This will also stop the algoritm from ending up in an endless loop
                        if (Geometry.IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
                        {
                            continue;
                        }

                        //Flip the edge
                        flippedEdges += 1;

                        hasFlippedEdge = true;

                        FlipEdge(thisEdge);
                    }
                }
            }

            //We have searched through all edges and havent found an edge to flip, so we have a Delaunay triangulation!
            if (!hasFlippedEdge)
            {
                //Debug.Log("Found a delaunay triangulation");

                break;
            }
        }

        //Debug.Log("Flipped edges: " + flippedEdges);

        //Dont have to convert from half edge to triangle because the algorithm will modify the objects, which belongs to the 
        //original triangles, so the triangles have the data we need

        return triangles;
    }

    //From triangle where each triangle has one vertex to half edge
    public static List<HalfEdge> TransformFromTriangleToHalfEdge(List<Triangle> triangles)
    {
        //Make sure the triangles have the same orientation
        OrientTrianglesClockwise(triangles);

        //First create a list with all possible half-edges
        List<HalfEdge> halfEdges = new List<HalfEdge>(triangles.Count * 3);

        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle t = triangles[i];

            HalfEdge he1 = new HalfEdge(t.v1);
            HalfEdge he2 = new HalfEdge(t.v2);
            HalfEdge he3 = new HalfEdge(t.v3);

            he1.nextEdge = he2;
            he2.nextEdge = he3;
            he3.nextEdge = he1;

            he1.prevEdge = he3;
            he2.prevEdge = he1;
            he3.prevEdge = he2;

            //The vertex needs to know of an edge going from it
            he1.v.halfEdge = he2;
            he2.v.halfEdge = he3;
            he3.v.halfEdge = he1;

            //The face the half-edge is connected to
            t.halfEdge = he1;

            he1.t = t;
            he2.t = t;
            he3.t = t;

            //Add the half-edges to the list
            halfEdges.Add(he1);
            halfEdges.Add(he2);
            halfEdges.Add(he3);
        }

        //Find the half-edges going in the opposite direction
        for (int i = 0; i < halfEdges.Count; i++)
        {
            HalfEdge he = halfEdges[i];

            Vertex goingToVertex = he.v;
            Vertex goingFromVertex = he.prevEdge.v;

            for (int j = 0; j < halfEdges.Count; j++)
            {
                //Dont compare with itself
                if (i == j)
                {
                    continue;
                }

                HalfEdge heOpposite = halfEdges[j];

                //Is this edge going between the vertices in the opposite direction
                if (goingFromVertex.position == heOpposite.v.position && goingToVertex.position == heOpposite.prevEdge.v.position)
                {
                    he.oppositeEdge = heOpposite;

                    break;
                }
            }
        }


        return halfEdges;
    }


    //Orient triangles so they have the correct orientation
    public static void OrientTrianglesClockwise(List<Triangle> triangles)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle tri = triangles[i];

            Vector2 v1 = new Vector2(tri.v1.position.x, tri.v1.position.z);
            Vector2 v2 = new Vector2(tri.v2.position.x, tri.v2.position.z);
            Vector2 v3 = new Vector2(tri.v3.position.x, tri.v3.position.z);

            if (!Geometry.IsTriangleOrientedClockwise(v1, v2, v3))
            {
                tri.ChangeOrientation();
            }
        }
    }
	 

    //Flip an edge
    private static void FlipEdge(HalfEdge one)
    {
        //The data we need
        //This edge's triangle
        HalfEdge two = one.nextEdge;
        HalfEdge three = one.prevEdge;
        //The opposite edge's triangle
        HalfEdge four = one.oppositeEdge;
        HalfEdge five = one.oppositeEdge.nextEdge;
        HalfEdge six = one.oppositeEdge.prevEdge;
        //The vertices
        Vertex a = one.v;
        Vertex b = one.nextEdge.v;
        Vertex c = one.prevEdge.v;
        Vertex d = one.oppositeEdge.nextEdge.v;



        //Flip

        //Change vertex
        a.halfEdge = one.nextEdge;
        c.halfEdge = one.oppositeEdge.nextEdge;

        //Change half-edge
        //Half-edge - half-edge connections
        one.nextEdge = three;
        one.prevEdge = five;

        two.nextEdge = four;
        two.prevEdge = six;

        three.nextEdge = five;
        three.prevEdge = one;

        four.nextEdge = six;
        four.prevEdge = two;

        five.nextEdge = one;
        five.prevEdge = three;

        six.nextEdge = two;
        six.prevEdge = four;

        //Half-edge - vertex connection
        one.v = b;
        two.v = b;
        three.v = c;
        four.v = d;
        five.v = d;
        six.v = a;

        //Half-edge - triangle connection
        Triangle t1 = one.t;
        Triangle t2 = four.t;

        one.t = t1;
        three.t = t1;
        five.t = t1;

        two.t = t2;
        four.t = t2;
        six.t = t2;

        //Opposite-edges are not changing!

        //Triangle connection
        t1.v1 = b;
        t1.v2 = c;
        t1.v3 = d;

        t2.v1 = b;
        t2.v2 = d;
        t2.v3 = a;

        t1.halfEdge = three;
        t2.halfEdge = four;
    }
}
