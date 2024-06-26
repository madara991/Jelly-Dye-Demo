using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScreenSpaceMeshGenerator : MonoBehaviour
{
	public float offsetDepthVertexis = 1;
	public GameObject targetObject;

	[HideInInspector]
	public Vector3[] newVertices;
	[HideInInspector]
	public List<int> newTriangles;

	public Vector2 uvScale;



	private void OnDrawGizmosSelected()
	{

		Mesh mesh = targetObject.GetComponent<MeshFilter>().sharedMesh;
		if (mesh == null)
			return;
		
		Vector3[] vertices = mesh.vertices;
		newVertices = new Vector3[vertices.Length]; // Create a new array for modified vertices
		newTriangles = new List<int>();

		Vector3 cameraPosition = Camera.main.transform.position;
		Vector3 targetObjectCenter = targetObject.transform.position;

		// Transform vertices from local to world space
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = targetObject.transform.TransformPoint(vertices[i]);


			float vertexDistance = Vector3.Distance(vertices[i], cameraPosition);
			float targetDistance = Vector3.Distance(targetObjectCenter, cameraPosition);

		
			
			if (vertexDistance + offsetDepthVertexis < targetDistance)
			{
				newVertices[i] = vertices[i];
				Gizmos.color = UnityEngine.Color.blue;
				Gizmos.DrawSphere(newVertices[i], 0.025f);
			}
		}

		int[] triangles = mesh.triangles;
		for (int i = 0; i < mesh.triangles.Length; i += 3)
		{

			//if (newVertices[triangles[i]]    != Vector3.zero &&
			//   newVertices[triangles[i + 1]] != Vector3.zero &&
			//   newVertices[triangles[i + 2]] != Vector3.zero)
			//{
			//}

			Vector3 p1 = newVertices[mesh.triangles[i + 0]];
			Vector3 p2 = newVertices[mesh.triangles[i + 1]];
			Vector3 p3 = newVertices[mesh.triangles[i + 2]];


			Gizmos.color = UnityEngine.Color.red;// NOTE : WHEN MAKE REALY (MESH) YOU NEED 
												 // ADD STATEMENT FOR GET >>TRINAGLES<< THAT NON ZERO 
												 // OR GET TRIANGLES THAT WE CALCULATED AS SPECIFIC VERTECES
			if (p1 != Vector3.zero && p2 != Vector3.zero && p3 != Vector3.zero)
			{
				newTriangles.Add(triangles[i]);
				newTriangles.Add(triangles[i + 1]);
				newTriangles.Add(triangles[i + 2]);

				Gizmos.color = UnityEngine.Color.red;
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p3);
				Gizmos.DrawLine(p3, p1);
			}
			
		}

	}

	public void CreateNewMesh(Vector3[] vertices, int[] triangles) 
	{
		if (gameObject.transform.childCount > 0)
			DestroyImmediate(transform.GetChild(0).gameObject);
			
		Mesh newMesh = new Mesh();
		List<Vector3> filteredVertices = new List<Vector3>();
		List<int> filteredTriangles = new List<int>();
		List<Vector2> filteredUv= new List<Vector2>();


		Dictionary<int, int> vertexMap = new Dictionary<int, int>();

		
		for (int i = 0; i < vertices.Length; i++)
		{
			if (vertices[i] != Vector3.zero)
			{
				vertexMap[i] = filteredVertices.Count;
				filteredVertices.Add(vertices[i]);
				filteredUv.Add(new Vector2(vertices[i].x * uvScale.x, vertices[i].z * uvScale.y));

			}
		}

		for (int i = 0; i < triangles.Length; i += 3)
		{
			if (vertexMap.ContainsKey(triangles[i]) &&
				vertexMap.ContainsKey(triangles[i + 1]) &&
				vertexMap.ContainsKey(triangles[i + 2]))
			{
				filteredTriangles.Add(vertexMap[triangles[i]]);
				filteredTriangles.Add(vertexMap[triangles[i + 1]]);
				filteredTriangles.Add(vertexMap[triangles[i + 2]]);
			}
		}

		newMesh.vertices = filteredVertices.ToArray();
		newMesh.triangles = filteredTriangles.ToArray();
		newMesh.uv = filteredUv.ToArray();

		// Optionally, recalculate normals, bounds, etc.
		newMesh.RecalculateNormals();
		newMesh.RecalculateBounds();
		
		
		// Assign the new mesh to a new GameObject or a MeshFilter
		GameObject newObject = new GameObject("ModifiedMesh");
		newObject.transform.parent = transform;
		MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();
		meshFilter.mesh = newMesh;
		meshRenderer.material = targetObject.GetComponent<MeshRenderer>().sharedMaterial;
	}

	// 1- NEED UV FOR WORK RENDRER   if not   continue with normal renderer
	// 2- mechanism Needle
	// 2- edit full scene project (check) and trteb
	
}


[CustomEditor(typeof(ScreenSpaceMeshGenerator))]
class ScreenSpaceMeshGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		
		var myScript = target as ScreenSpaceMeshGenerator;


		if (GUILayout.Button("Generate Mesh"))
		{
			myScript.CreateNewMesh(myScript.newVertices, myScript.newTriangles.ToArray());
		}
		
		base.OnInspectorGUI();
		
	}

}




