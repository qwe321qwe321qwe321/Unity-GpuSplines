using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PeDev {
    public class DrawProcedural : MonoBehaviour {
	    public int triCount = 3;
	    public Material material;

	    private void Update() {
		    Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 500);
		    Graphics.DrawProcedural(material, bounds, MeshTopology.Triangles, triCount, 1);
	    }
	    
	    private void OnRenderObject() {
		    // Debug.Log("Render");
		    // Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 500);
		    // material.SetPass(0);
		    // Graphics.DrawProceduralNow(MeshTopology.Triangles, triCount, 1);
	    }
    }
}