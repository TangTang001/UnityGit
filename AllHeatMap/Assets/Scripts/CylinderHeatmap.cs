using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CylinderHeatmap : MonoBehaviour
{
    public float PreHeight = 10.0f;
    public float Size = 1.0f;
    public float Radius = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        Dictionary<Vector3, float> heatDic = new Dictionary<Vector3, float>();
        heatDic.Add(new Vector3(-20, 0, -10), 0.9f);
        heatDic.Add(new Vector3(20, 0, 15), 0.3f);
        heatDic.Add(new Vector3(0, 0, 0), 0.9f);
        heatDic.Add(new Vector3(5, 0, 2), 0.1f);
        heatDic.Add(new Vector3(-10, 0, 18), 0.6f);
        heatDic.Add(new Vector3(5, 0, -3), 0.05f);
        SetCylinderHeatmap(heatDic);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetCylinderHeatmap(Dictionary<Vector3, float> heatDic)
    {
        Vector3 startPos = this.GetComponent<MeshRenderer>().bounds.min;   //地图包围框的世界坐标
        Vector3 endPos = this.GetComponent<MeshRenderer>().bounds.max;
        Debug.Log(startPos + ";" + endPos);

        Vector3 meshMin = new Vector3(Mathf.Ceil(startPos.x), Mathf.Ceil(startPos.y), Mathf.Ceil(startPos.z));
        Vector3 meshMax = new Vector3(Mathf.Floor(endPos.x), Mathf.Floor(endPos.y), Mathf.Floor(endPos.z));

        GameObject mapMesh = CreateMesh(meshMin, meshMax);
        Vector3[] modifiedVertices = SetHeatMesh(mapMesh, heatDic);
        DrawCylinderHeatmap(mapMesh, modifiedVertices, PreHeight);

    }

    public GameObject CreateMesh(Vector3 vertMin, Vector3 vertMax)
    {
        GameObject mapMesh = new GameObject();
        mapMesh.name = "mapHeatmap";
        mapMesh.transform.position = vertMin ;
        MeshFilter meshFilter = mapMesh.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        /*MeshRenderer meshRenderer = mapMesh.AddComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.green;
        meshRenderer.material = mat;*/

        int height = (int)(vertMax.z - vertMin.z);
        int width = (int)(vertMax.x - vertMin.x);
        float threeCoord = mapMesh.transform.position.y;
        Vector3[] vertices = new Vector3[height * width];   //顶点坐标组
        Vector2[] uv = new Vector2[height * width];

        //把UV缩放到0-1
        Vector2 uvScale = new Vector2(1.0f / (width - 1), 1.0f / (height - 1));
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //生成顶点
                vertices[y * width + x] = new Vector3(x, threeCoord, y);
                //生成uv
                uv[y * width + x] = Vector2.Scale(new Vector2(x, y), uvScale);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        //三角形index
        int[] triangles = new int[(height - 1) * (width - 1) * 6];
        int index = 0;
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                //每个格子2个三角形,总共6个index
                triangles[index++] = (y * width) + x;
                triangles[index++] = ((y + 1) * width) + x;
                triangles[index++] = (y * width) + x + 1;

                triangles[index++] = ((y + 1) * width) + x;
                triangles[index++] = ((y + 1) * width) + x + 1;
                triangles[index++] = (y * width) + x + 1;
            }
        }
        mesh.triangles = triangles; //三角面
        mesh.RecalculateNormals();  //计算法线
        return mapMesh;
    }

    public Vector3[] SetHeatMesh(GameObject mapMesh, Dictionary<Vector3, float> heatPoints)
    {
        //heatPoint中value范围0-1
        Vector3[] meshVertices = mapMesh.GetComponent<MeshFilter>().mesh.vertices;
        int verticeLength = meshVertices.Length;
        Vector3[] modifiedVertices = new Vector3[verticeLength];       

        for (int i = 0; i < verticeLength; i++)
        {
            float alldist = 0;
            foreach(var heat in heatPoints)
            {
                Vector3 heatPos = heat.Key;
                float dist = Mathf.Sqrt(Mathf.Pow(Vector3.Distance(mapMesh.transform.TransformPoint(meshVertices[i]), heatPos), 2));
                alldist += Mathf.Pow(1 / dist, 2);
            }


            float value = 0;
            /*foreach (var heat in heatPoints)
            {
                Vector3 heatPos = heat.Key;
                float heatValue = heat.Value;
                float distance = Mathf.Sqrt(Mathf.Pow(Vector3.Distance(mapMesh.transform.TransformPoint(meshVertices[i]), heatPos), 2)); //计算世界坐标系下网格每个顶点到热力点的距离
                float ratio = 0;
                ratio = Mathf.Pow(1 / distance, 2) / alldist;
                ratio = Mathf.Clamp01(ratio);
                value += heatValue * ratio;                
            }*/
            foreach(var heat in heatPoints)
            {
                Vector3 heatPos = heat.Key;
                float heatValue = heat.Value;
                float distance = Mathf.Sqrt(Mathf.Pow(Vector3.Distance(mapMesh.transform.TransformPoint(meshVertices[i]), heatPos), 2)); //计算世界坐标系下网格每个顶点到热力点的距离
                if(distance < Radius)
                {
                    float ratio = 1 - (Mathf.Cos(180*(Radius-distance)/Radius*Mathf.Deg2Rad)+1)/2;
                    //float ratio = 1 - (distance / Radius);
                    float nowvalue = heatValue * ratio;
                    if(nowvalue != 0)
                    {
                        value += nowvalue / (nowvalue + value) * nowvalue;
                    }
                }

            }


            value = Mathf.Clamp01(value);
            modifiedVertices[i] = meshVertices[i];
            modifiedVertices[i].y = value;
        }
        return modifiedVertices;
    }

    public void DrawCylinderHeatmap(GameObject mapMesh, Vector3[] modifiedVertices, float preHeight)
    {
        GameObject heatCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        heatCylinder.GetComponent<MeshRenderer>().material = Resources.Load<Material>("HeatCylinder");
        Mesh objectMesh = heatCylinder.GetComponent<MeshFilter>().mesh;
        Material _mat = heatCylinder.GetComponent<MeshRenderer>().material;
        _mat.enableInstancing = true;
        _mat.SetFloat("_PreHeight", preHeight);
        Vector3[] vertices = modifiedVertices;
        List<Matrix4x4> matrix4x4List = new List<Matrix4x4>();
        int m = 0;
        for(int i=0; i<vertices.Length; i++)
        {
            
            float height = preHeight * vertices[i].y;
            Vector3 fwd = -Vector3.up;
            Vector3 pos = mapMesh.transform.TransformPoint(new Vector3(vertices[i].x, height, vertices[i].z));    //获取网格顶点的世界坐标
            bool grounded = Physics.Raycast(pos, fwd, preHeight * 100);
            if (grounded && vertices[i].y > 0.01f) // && (pos.x+pos.z) % UnityEngine.Random.Range(1, 9) ==0
            {
                Vector3 scale = new Vector3(Size, height, Size);
                Matrix4x4 item = Matrix4x4.TRS(pos, heatCylinder.transform.rotation, scale);
                matrix4x4List.Add(item);
                m++;
            }
                       
        }
        Matrix4x4[] matrix4X4s = matrix4x4List.ToArray();
        int rendercount = matrix4X4s.Length / 1023 + (matrix4X4s.Length % 1023 == 0 ? 0 : 1);
        CommandBuffer[] m_buff = new CommandBuffer[rendercount];
        for (int i = 0; i < m_buff.Length; i++)
        {
            m_buff[i] = new CommandBuffer();
            m_buff[i].name = "tempratureObject" + i;
        }

        for (int i = 0; i < rendercount; i++)
        {
            int count = i < rendercount - 1 ? 1023 : (matrix4X4s.Length - i * 1023);
            Matrix4x4[] senddata = new Matrix4x4[count];
            Array.Copy(matrix4X4s, i * 1023, senddata, 0, count);
            m_buff[i].DrawMeshInstanced(objectMesh, 0, _mat, 0, senddata, senddata.Length);
            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_buff[i]);
        }
        Destroy(heatCylinder);

    }


    /*float[] GaussTable_1D(int size, float sigma)
    {
        float[] result = new float[size];
        float u = (size - 1) / 2f;
        float sum = 0;

        for (int x = 0; x < size; x++)
        {
            result[x] = Mathf.Exp(-(u - x) * (u - x)) / (2 * (sigma * sigma) / (Mathf.Sqrt(2 * Mathf.PI * sigma)));
            sum += result[x];
        }

        for(int x = 0; x< size; x++)
        {
            result[x] /= sum;
        }
        return result;
    }*/
}
