using UnityEngine;

class Test:MonoBehaviour
{
    [SerializeField]
    private GameObject cube;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Mesh mesh = cube.GetComponent<MeshFilter>().mesh;

            MeshLib.BoxProjection b = new MeshLib.BoxProjection();
            b.BoxUvProjection(mesh);
        }
    }
}
