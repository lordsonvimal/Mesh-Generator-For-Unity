using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Triangulation 
{

    public static List<Triangle> IncrementalTriangulation(List<Vertex> points)
    {
        List<Triangle> triangles = new List<Triangle>();

        //Sort the points along x-axis
        //OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
        points = points.OrderBy(n => n.position.x).ToList();

        //The first 3 vertices are always forming a triangle
        Triangle newTriangle = new Triangle(points[0].position, points[1].position, points[2].position);

        triangles.Add(newTriangle);

        //All edges that form the triangles, so we have something to test against
        List<Edge> edges = new List<Edge>();

        edges.Add(new Edge(newTriangle.v1, newTriangle.v2));
        edges.Add(new Edge(newTriangle.v2, newTriangle.v3));
        edges.Add(new Edge(newTriangle.v3, newTriangle.v1));

        //Add the other triangles one by one
        //Starts at 3 because we have already added 0,1,2
        for (int i = 3; i < points.Count; i++)
        {
            Vector3 currentPoint = points[i].position;

            //The edges we add this loop or we will get stuck in an endless loop
            List<Edge> newEdges = new List<Edge>();

            //Is this edge visible? We only need to check if the midpoint of the edge is visible 
            for (int j = 0; j < edges.Count; j++)
            {
                Edge currentEdge = edges[j];

                Vector3 midPoint = (currentEdge.v1.position + currentEdge.v2.position) / 2f;

                Edge edgeToMidpoint = new Edge(currentPoint, midPoint);

                //Check if this line is intersecting
                bool canSeeEdge = true;

                for (int k = 0; k < edges.Count; k++)
                {
                    //Dont compare the edge with itself
                    if (k == j)
                    {
                        continue;
                    }

                    if (AreEdgesIntersecting(edgeToMidpoint, edges[k]))
                    {
                        canSeeEdge = false;

                        break;
                    }
                }

                //This is a valid triangle
                if (canSeeEdge)
                {
                    Edge edgeToPoint1 = new Edge(currentEdge.v1, new Vertex(currentPoint));
                    Edge edgeToPoint2 = new Edge(currentEdge.v2, new Vertex(currentPoint));

                    newEdges.Add(edgeToPoint1);
                    newEdges.Add(edgeToPoint2);

                    Triangle newTri = new Triangle(edgeToPoint1.v1, edgeToPoint1.v2, edgeToPoint2.v1);

                    triangles.Add(newTri);
                }
            }


            for (int j = 0; j < newEdges.Count; j++)
            {
                edges.Add(newEdges[j]);
            }
        }


        return triangles;
    }



    private static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
    {
        Vector2 l1_p1 = new Vector2(edge1.v1.position.x, edge1.v1.position.z);
        Vector2 l1_p2 = new Vector2(edge1.v2.position.x, edge1.v2.position.z);

        Vector2 l2_p1 = new Vector2(edge2.v1.position.x, edge2.v1.position.z);
        Vector2 l2_p2 = new Vector2(edge2.v2.position.x, edge2.v2.position.z);

        bool isIntersecting = Intersections.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

        return isIntersecting;
    }

    /// <summary>
    /// Triangulate the specified points.
    /// </summary>
    /// <param name="points">Points.</param>
    public static List<Triangle> Triangulate(List<Vector3> points)
    {
        if (points.Count < 3)
            return null;
        
        List<Triangle> triangles = new List<Triangle>();

        List<Vertex> vertices = new List<Vertex>();

        //Edges that cannot be repeated
        List<Edge> uniqueEdges = new List<Edge>();
        //Edges that are shared
        List<Edge> sharedEdges = new List<Edge>();

//        int number_of_triangles = points.Count - 2;

//        int shared_edge = number_of_triangles - 1;

        //Create vertices for the points
        for (int i = 0; i < points.Count; i++)
        {
            vertices.Add(new Vertex(points[i]));
        }

        //Create unique edges from the vertices
        for (int i = 0; i < points.Count-1; i++)
        {
            uniqueEdges.Add(new Edge(vertices[i],vertices[i+1]));
        }
        uniqueEdges.Add(new Edge(vertices[vertices.Count-1],vertices[0]));

        //Create a triangle and edges for the first triangle
        triangles.Add(new Triangle(vertices[0],vertices[1],vertices[2]));

        //remove unique edges as only one triangle should be using one unique edge
        uniqueEdges.RemoveAt(0);
        uniqueEdges.RemoveAt(1);

        sharedEdges.Add(new Edge(vertices[2],vertices[0]));
        sharedEdges.Add(new Edge(vertices[0],vertices[2]));


        if (points.Count == 3)
            return triangles;

        for (int i = 0; i < uniqueEdges.Count; i++)
        {
            int j = i + 1;
            if (j >= uniqueEdges.Count)
                j = 0;

            float angle = Vector3.Angle(uniqueEdges[i].v2.position - uniqueEdges[i].v1.position, uniqueEdges[j].v2.position - uniqueEdges[j].v1.position);
//            float distance = Vector3.Distance();
            Debug.Log(angle);

            for (int k = 1; k < vertices.Count; k++)
            {
                if(uniqueEdges[i].v2 != vertices[i])
                {
//                    if (uniqueEdges[i].v2)
//                    {
//                        
//                    }
                    }
            }
        }

        return triangles;
    }


    private bool AreEdgesShared(Edge e1,Edge e2)
    {
        if ((e1.v1 == e2.v2) && (e1.v2 == e2.v1))
            return true;

        return false;
    }


    private bool isSatisfied()
    {
        return true;
    }
}

