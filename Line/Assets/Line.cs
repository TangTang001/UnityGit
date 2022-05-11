using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Line : MonoBehaviour
{
    [Header("对象名称")]
    public string objectName;
    [Header("线路提示内容")]
    public string tipContent;
    [Header("提示框大小")]
    public float tipSize = 1;
    [Header("连线的每点世界坐标")]
    public Vector3[] points;
    [Header("线宽")]
    public float width = 1;
    [Header("线颜色")]
    public string lineColor;
    [Header("线选中颜色")]
    public string selectedColor;
    private Color color;
    private GameObject lineTip;
    // Start is called before the first frame update
    void Start()
    {

        InitLine(); 
    }

    // Update is called once per frame

    private void InitLine()
    {
        this.name = objectName;
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
        lineRenderer.widthMultiplier = width;
        ColorUtility.TryParseHtmlString(lineColor, out color);
        lineRenderer.material.color = color;
        //lineRenderer.Simplify(4);   //简化线

        //碰撞器添加网格
        GetColliderMesh(points);
        //MeshCollider激活态必须手动改变下才能触发，暂时不知道为啥
        this.GetComponent<MeshCollider>().enabled = false;
        this.GetComponent<MeshCollider>().enabled = true;


        //添加鼠标事件
        //InfoItem添加trigger
        this.gameObject.AddComponent<EventTrigger>();
        EventTrigger trigger = this.GetComponent<EventTrigger>();
        //添加鼠标按下事件
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(OnPointerDown);
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
        //添加鼠标抬起事件
        EventTrigger.Entry entry1 = new EventTrigger.Entry();
        entry1.eventID = EventTriggerType.PointerUp;
        UnityAction<BaseEventData> callback1 = new UnityAction<BaseEventData>(OnPointerUp);
        entry1.callback.AddListener(callback1);
        trigger.triggers.Add(entry1);
    }

    public void OnPointerDown(BaseEventData arg0)
    {       
        Debug.Log("mouse down");
        Color selected;
        ColorUtility.TryParseHtmlString(selectedColor, out selected);
        gameObject.GetComponent<LineRenderer>().material.color = selected;
        string text = tipContent;
        if (lineTip == null)
        {
            GameObject lineTipPrefab = Resources.Load<GameObject>("ObjectTip");
            lineTip = Instantiate(lineTipPrefab);
        }
        lineTip.transform.position = points[points.Length / 2];
        lineTip.transform.localScale = new Vector3(tipSize, tipSize, tipSize);
        GameObject textObject = lineTip.transform.Find("TipCanvas/Image/Text").gameObject;
        GameObject imgObject = lineTip.transform.Find("TipCanvas/Image").gameObject;
        textObject.GetComponent<Text>().text = text;
    }
    public void OnPointerUp(BaseEventData arg0)
    {
        Debug.Log("mouse up");
        if (lineTip != null)
        {
            Destroy(lineTip);
        }
        gameObject.GetComponent<LineRenderer>().material.color = color;
    }

    private void GetColliderMesh(Vector3[] pointList)  //根据描线的点和和线宽构造线的网格
    {
        List<Vector3> meshVertices = new List<Vector3>();
        for (int i = 1; i < pointList.Length; i++)
        {
            Vector3 startPoint = pointList[i - 1];
            Vector3 endPoint = pointList[i];
            Vector3 dir = endPoint - startPoint;

            Vector3 otherDir = Vector3.up;
            Vector3 planeNormal = Vector3.Cross(otherDir, dir);

            Vector3 vertical = Vector3.Cross(dir, planeNormal).normalized;

            Vector3 up = startPoint + vertical * width * 0.5f ;
            Vector3 down = startPoint - vertical * width * 0.5f;
            meshVertices.Add(down);
            meshVertices.Add(up);
            if (i == pointList.Length - 1)
            {
                up = endPoint + vertical * width * 0.5f;
                down = endPoint - vertical * width * 0.5f;
                meshVertices.Add(down);
                meshVertices.Add(up);
            }
        }

        Mesh lineMesh = new Mesh();
        this.GetComponent<MeshFilter>().mesh = lineMesh;
        this.GetComponent<MeshCollider>().sharedMesh = lineMesh;
        lineMesh.name = objectName+"LineMesh";
        lineMesh.vertices = meshVertices.ToArray();
        int trisCount = (meshVertices.Count / 2 - 1) * 2 * 3;
        int[] triangleIndex = new int[trisCount];
        for (int m = 0, n = 0; m < trisCount; m += 6, n += 2)
        {
            triangleIndex[m] = n;
            triangleIndex[m + 1] = n + 1;
            triangleIndex[m + 2] = n + 2;
            triangleIndex[m + 3] = n + 2;
            triangleIndex[m + 4] = n + 1;
            triangleIndex[m + 5] = n + 3;
        }
        lineMesh.triangles = triangleIndex;        
    }
}
