using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using LitJson;
public class HeatMap : MonoBehaviour
{
    [Tooltip("heatmap1 or heatmap2")]
    public string heatmapStyle;
    [Tooltip("Peak or Hill")]
    public string shaderStyle;
    public bool is3DHeatmap;
    public float maxHeight = 10;
    public Dictionary<Vector3, float> heatmapDic;
    private Material mtl;

    void Start()
    {
        heatmapDic = new Dictionary<Vector3, float>();
        heatmapDic.Add(new Vector3(-20, 0, -10), 0.6f);
        heatmapDic.Add(new Vector3(2, 0, 5), 0.1f);
        heatmapDic.Add(new Vector3(0.0f, 0, 0), 0.9f);
        heatmapDic.Add(new Vector3(15, 0, 10), 0.8f);
        heatmapDic.Add(new Vector3(5, 0, -3), 0.9f);
        heatmapDic.Add(new Vector3(10, 0, 18), 0.1f);

        setHeatMap();
    }

    public void setHeatMap()
    {
        mtl = this.GetComponent<MeshRenderer>().material;
        if(heatmapDic.Count == 0)
        {
            mtl.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            Color trans;
            ColorUtility.TryParseHtmlString("#FFFFFF00", out trans);
            mtl.color = trans;
            
            return;
        }


        mtl.shader = Shader.Find("HeatMap/"+shaderStyle);
        if (!is3DHeatmap)
        {
            maxHeight = 0;
        }
        Texture tex = Resources.Load<Texture>(heatmapStyle);
        mtl.SetTexture("_HeatMapTex", tex);

        mtl.SetInt("_FactorCount", heatmapDic.Count);
        mtl.SetFloat("_MaxHeight", maxHeight);
        if(shaderStyle == "Hill")
        {
            mtl.SetFloat("_Radius", 10);
        }
        
        var ifPosition = new Vector4[heatmapDic.Count];
        int i = 0;
        foreach(var factor in heatmapDic)
        {
             ifPosition[i] = new Vector4(factor.Key.x, factor.Key.y, factor.Key.z, factor.Value);
             i++;
        }
        mtl.SetVectorArray("_Factors", ifPosition);   
    }
}
