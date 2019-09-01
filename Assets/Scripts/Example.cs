using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MeshLib;

/// <summary>
/// Example class
/// Attach this to game object
/// Create a plane in an axis(Plane should be parallel to any of the axis)
/// Click on the plane to create points in CW order
/// </summary>
public class Example : MonoBehaviour
{
    [SerializeField]
    List<Vector3> meshPoints;
    [SerializeField]
    float thickness;

    public Meshes mesh;


    void Start()
    {
        meshPoints = new List<Vector3>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                g.transform.position = hit.point;
                g.transform.localScale = Vector3.one * 0.3f;
                meshPoints.Add(hit.point);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MeshController meshController = new MeshController();
            try
            {
                mesh = meshController.CreateSubMesh(meshPoints, thickness, "new_Mesh");

                //BoxProjection b = new BoxProjection();
                //b.BoxUvProjection(mesh.mesh, AXIS.X, 1, 45);
                //b.BoxUvProjection(mesh.mesh, AXIS.Y, 1, 45);
                //b.BoxUvProjection(mesh.mesh, AXIS.Z, 1, 45);
            }
            catch (System.Exception e)
            {
                print(e);
            }
            meshPoints.Clear();
        }
    }
}
