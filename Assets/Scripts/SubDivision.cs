using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Edge
{
	public int[] vertexIdx;
	public List<int> faceIdx;
	public Vector3 newPos;

	public Edge(int vidx1, int vidx2)
	{
		if (vidx1 < vidx2)
		{
			vertexIdx = new[] {vidx1, vidx2};
		}
		else
		{
			vertexIdx = new[] {vidx2, vidx1};
		}

		faceIdx = new List<int>();
	}

	public override bool Equals(object obj)
	{
		Edge edge = obj as Edge;
		return Enumerable.SequenceEqual(vertexIdx, edge.vertexIdx);
	}
}

public class SubDivision : MonoBehaviour
{

	public int iter;

	// Use this for initialization
	void Start()
	{
		Mesh mesh = transform.GetComponent<MeshFilter>().mesh;
		for (int iter = 0; iter < this.iter; iter++)
		{
			Vector3[] vertexs = new Vector3[mesh.vertexCount];
			List<Edge> edges = new List<Edge>();
			Dictionary<String, int> edgeDict = new Dictionary<string, int>();
			HashSet<int>[] degrees = new HashSet<int>[mesh.vertexCount];
			Vector3Int[] faces = new Vector3Int[mesh.triangles.Length / 3];
			HashSet<int> edgeVertexs = new HashSet<int>();

			for (int i = 0; i < degrees.Length; i++)
			{
				degrees[i] = new HashSet<int>();
			}

			for (int i = 0; i < faces.Length; i++)
			{
				faces[i] = Vector3Int.zero;
			}

			for (int i = 0; i < mesh.triangles.Length; i += 3)
			{
				int idx1 = mesh.triangles[i];
				int idx2 = mesh.triangles[i + 1];
				int idx3 = mesh.triangles[i + 2];
				AddDegree(degrees, idx1, idx2);
				AddDegree(degrees, idx1, idx3);
				AddDegree(degrees, idx2, idx3);
				int face = i / 3;
				AddFace(faces, face, 0, AddEdge(edges, edgeDict, idx1, idx2, i));
				AddFace(faces, face, 1, AddEdge(edges, edgeDict, idx2, idx3, i));
				AddFace(faces, face, 2, AddEdge(edges, edgeDict, idx3, idx1, i));
			}

			for (int i = 0; i < edges.Count; i++)
			{
				Edge edge = edges[i];
				if (edge.faceIdx.Count == 1)
				{
					edgeVertexs.Add(edge.vertexIdx[0]);
					edgeVertexs.Add(edge.vertexIdx[1]);
					edge.newPos = (mesh.vertices[edge.vertexIdx[0]] + mesh.vertices[edge.vertexIdx[1]]) * 0.5f;
				}
				else
				{
					edge.newPos = Vector3.zero;
					edge.newPos += (mesh.vertices[edge.vertexIdx[0]] + mesh.vertices[edge.vertexIdx[1]]) * 0.375f;
					int idx1 = GetOtherIdx(mesh.triangles, edge.faceIdx[0], edge.vertexIdx[0], edge.vertexIdx[1]);
					int idx2 = GetOtherIdx(mesh.triangles, edge.faceIdx[1], edge.vertexIdx[0], edge.vertexIdx[1]);
					edge.newPos += (mesh.vertices[idx1] + mesh.vertices[idx2]) * 0.125f;
				}
			}

			for (int i = 0; i < vertexs.Length; i++)
			{
				Vector3 tmp = Vector3.zero;
				if (edgeVertexs.Contains(i))
				{
					vertexs[i] = 0.75f * mesh.vertices[i];
					int count = 0;
					foreach (int j in degrees[i])
					{
						if (edgeVertexs.Contains(j))
						{
							String key = i < j ? i + "," + j : j + "," + i;
							if (edgeDict.ContainsKey(key) && edges[edgeDict[key]].faceIdx.Count == 1)
							{
								tmp += mesh.vertices[j];
								count++;
								if (count == 2)
								{
									break;
								}
							}
						}
					}

					vertexs[i] += 0.125f * tmp;
				}
				else
				{
					int degree = degrees[i].Count;
					float beta = (0.625f - Mathf.Pow(0.375f + 0.25f * Mathf.Cos(Mathf.PI * 2.0f / degree), 2.0f)) /
					             degree;
					vertexs[i] = (1 - degree * beta) * mesh.vertices[i];
					foreach (int j in degrees[i])
					{
						tmp += mesh.vertices[j];
					}

					vertexs[i] += beta * tmp;
				}
			}

			Vector3[] newVertexs = new Vector3[vertexs.Length + edges.Count];
			for (int i = 0; i < newVertexs.Length; i++)
			{
				if (i < vertexs.Length)
				{
					newVertexs[i] = vertexs[i];
				}
				else
				{
					newVertexs[i] = edges[i - vertexs.Length].newPos;
				}
			}

			int[] newTriangles = new int[mesh.triangles.Length * 4];
			for (int i = 0; i < faces.Length; i++)
			{
				int fidx1 = faces[i][0];
				int fidx2 = faces[i][1];
				int fidx3 = faces[i][2];
				Edge edge1 = edges[fidx1];
				Edge edge2 = edges[fidx2];
				CheckOrder(edge1, mesh.triangles[i * 3]);
				CheckOrder(edge2, mesh.triangles[i * 3 + 1]);
				newTriangles[i * 12] = edge1.vertexIdx[0];
				newTriangles[i * 12 + 1] = vertexs.Length + fidx1;
				newTriangles[i * 12 + 2] = vertexs.Length + fidx3;
				newTriangles[i * 12 + 3] = vertexs.Length + fidx1;
				newTriangles[i * 12 + 4] = edge1.vertexIdx[1];
				newTriangles[i * 12 + 5] = vertexs.Length + fidx2;
				newTriangles[i * 12 + 6] = vertexs.Length + fidx3;
				newTriangles[i * 12 + 7] = vertexs.Length + fidx2;
				newTriangles[i * 12 + 8] = edge2.vertexIdx[1];
				newTriangles[i * 12 + 9] = vertexs.Length + fidx1;
				newTriangles[i * 12 + 10] = vertexs.Length + fidx2;
				newTriangles[i * 12 + 11] = vertexs.Length + fidx3;
			}

			mesh.vertices = newVertexs;
			mesh.triangles = newTriangles;
		}

		mesh.RecalculateNormals();
		transform.GetComponent<MeshFilter>().mesh = mesh;
	}

	void CheckOrder(Edge edge, int idx1)
	{
		if (edge.vertexIdx[0] != idx1)
		{
			int tmp = edge.vertexIdx[0];
			edge.vertexIdx[0] = edge.vertexIdx[1];
			edge.vertexIdx[1] = tmp;
		}
	}

	int GetOtherIdx(int[] triangles, int idx, int idx1, int idx2)
	{
		if (triangles[idx] != idx1 && triangles[idx] != idx2)
		{
			return triangles[idx];
		}
		else if (triangles[idx + 1] != idx1 && triangles[idx + 1] != idx2)
		{
			return triangles[idx + 1];
		}
		else
		{
			return triangles[idx + 2];
		}
	}

	void AddDegree(HashSet<int>[] degrees, int idx1, int idx2)
	{
		degrees[idx1].Add(idx2);
		degrees[idx2].Add(idx1);
	}

	int AddEdge(List<Edge> edges, Dictionary<String, int> edgeDict, int idx1, int idx2, int face)
	{
		Edge edge = new Edge(idx1, idx2);
		String key = edge.vertexIdx[0] + "," + edge.vertexIdx[1];
		int idx = edgeDict.ContainsKey(key) ? edgeDict[key] : -1;
		if (idx >= 0)
		{
			edges[idx].faceIdx.Add(face);
			return idx;
		}
		else
		{
			edge.faceIdx.Add(face);
			edges.Add(edge);
			edgeDict.Add(key, edges.Count - 1);
			return edges.Count - 1;
		}
	}

	void AddFace(Vector3Int[] faces, int idx1, int idx2, int edge)
	{
		faces[idx1][idx2] = edge;
	}
}