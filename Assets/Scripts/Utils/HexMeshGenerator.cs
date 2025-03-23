using UnityEngine;

public class HexagonMeshGenerator : MonoBehaviour
{
    public float outerRadius = 1f;
    public float height = 0.1f;
    
    void Start()
    {
        GenerateHexagonMesh();
    }
    
    void GenerateHexagonMesh()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        Vector3[] vertices = new Vector3[14]; // 6个外侧顶点 + 中心顶点 (顶面和底面)
        
        // 顶面中心点
        vertices[0] = new Vector3(0, height/2, 0);
        
        // 底面中心点
        vertices[7] = new Vector3(0, -height/2, 0);
        
        // 创建6个外侧顶点
        for (int i = 0; i < 6; i++)
        {
            float angle = 2f * Mathf.PI * i / 6f;
            
            // 顶面外侧顶点
            vertices[i+1] = new Vector3(
                outerRadius * Mathf.Cos(angle),
                height/2,
                outerRadius * Mathf.Sin(angle)
            );
            
            // 底面外侧顶点
            vertices[i+8] = new Vector3(
                outerRadius * Mathf.Cos(angle),
                -height/2,
                outerRadius * Mathf.Sin(angle)
            );
        }
        
        // 创建三角形索引
        int[] triangles = new int[6*6]; // 顶面6个三角形 + 侧面6个矩形(12个三角形)
        
        // 顶面三角形
        for (int i = 0; i < 6; i++)
        {
            triangles[i*3] = 0;
            triangles[i*3+1] = i+1;
            triangles[i*3+2] = i == 5 ? 1 : i+2;
        }
        
        // 设置网格数据
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}