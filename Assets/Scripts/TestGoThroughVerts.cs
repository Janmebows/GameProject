using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGoThroughVerts : MonoBehaviour {
    public Mesh mesh;
    int[] triangles;
    Vector3[] verts;
    int currentTriangleIndex;
	// Use this for initialization
	void Start () {
		if(mesh==null)
        {
            mesh = GetComponent<Mesh>();
        }
        else { return; }
        triangles = mesh.triangles;
        verts = mesh.vertices;
        Debug.Log(triangles.Length + ", " + verts.Length);
        currentTriangleIndex = 0;


    }
	
	// Update is called once per frame
	void Update () {
        if (mesh != null) { 
        if (Input.GetKey(KeyCode.UpArrow) && currentTriangleIndex<triangles.Length)
        {
            currentTriangleIndex += 1;
            Debug.Log(verts[currentTriangleIndex]);
        }
        else if(Input.GetKey(KeyCode.DownArrow) && currentTriangleIndex > 0)
        {
            currentTriangleIndex -= 1;
            Debug.Log(verts[currentTriangleIndex]);
        }
        }
	}
}
