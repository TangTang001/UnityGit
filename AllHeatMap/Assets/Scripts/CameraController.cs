using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
//using SceneManager;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("旋转速度")]
    public uint rotateSpeed = 10;
    [Header("缩放速度")]
    public uint zoomSpeed = 10;
    [Header("移动速度")]
    public uint moveSpeed = 1000;
    [Header("最大俯视角度")]
    public int yMinLimit = 10;
    [Header("最大仰视角度")]
    public uint yMaxLimit = 90;
    [Header("最小缩放范围值")]
    public uint zoomMin = 40;
    [Header("最大缩放范围值")]
    public uint zoomMax = 80;
    //[Header("用于设置zoomMin和zoomMax的参考值")]
    //public float distance; 
    [Header("是否可以移动")]
    public bool canMove = true;
    [Header("是否可以旋转")]
    public bool canRotation = true;
    [Header("是否可以缩放")]
    public bool canZoom = true;

    private Vector3 originpos;   //相机初始位置   
    private Quaternion originrot;  //相机初始角度
    private float originFiled;
    private float distance;
    private float currentZoomValue;//当前视野范围
    private float currentAngleForX;//当前X轴旋转的角度
    [HideInInspector]
    public Vector3 rotaAxis;//旋转的轴心
    private float time1;//用于记录点击时间，实现双击效果 
    private float time2;//用于记录点击时间，实现双击效果 
    private Vector3 moveTargetPoint;//双击时摄像头移动的目标坐标
    //private float cameraCruiseByTargetAngle = 0;//定点环绕360度巡航，镜头当前巡航角度
    private UnityEvent cameraCruiseByTargetCallBackListener; //定点环绕360度巡航结束时回调
    private UnityEvent cameraCruiseByPosListCallBackListener; //定线巡航结束时回调
    //private int cameraCruisePosListCount = 0;//定线巡航定时任务当前调用次数
    //private List<Vector3> cameraCruisePosList;//定线巡航路线集合


    void Start()
    {
        //Camera c = this.GetComponent<Camera>();
        //c.DOFieldOfView(60, 2.0f);
        InitCamera();
        currentZoomValue = this.GetComponent<Camera>().fieldOfView;

    }
        
    private void LateUpdate()
    {
        Zoom();
        Rotate();
        Move();
        DoCameraReset();
        //Focus();
    }

    private void InitCamera()
    {
        originpos = transform.position;
        originrot = transform.rotation;
        originFiled = Camera.main.fieldOfView;
        Debug.Log(originFiled);

        currentAngleForX = transform.eulerAngles.x;  //获取相机绕X轴旋转的角度
        Ray ray = new Ray(transform.position, transform.forward ); //从相机位置向Z轴正方向发射一条射线
        RaycastHit hit;
        bool isHit = Physics.Raycast(ray, out hit);
        if (isHit)
        {
            //Debug.Log("main camera hitted scene");
            rotaAxis = new Vector3(hit.point.x, 0, hit.point.z);
            distance = Vector3.Distance(transform.position, rotaAxis);  //默认视图中相机到物体距离            
        }
    }

    private void Zoom()
    {
       /* float mouseScollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScollWheel == 0 || canZoom == false)
            return;
        float zoom = mouseScollWheel * zoomSpeed * Time.deltaTime * 100;//滚轮向前为正，向后为负
        
        distance = Vector3.Distance(transform.position, rotaAxis); //当前相机和物体的距离
        if(distance - zoom < zoomMin)
        {
            float offsetValue = distance - zoomMin;
            zoom = zoom < 0 ? -1 * offsetValue : offsetValue;
        }
        if(distance - zoom > zoomMax)
        {
            float offsetValue = zoomMax - distance;
            zoom = zoom < 0 ? -1 * offsetValue : offsetValue;
        }
        Debug.Log("Zoom:" + zoom);
        transform.Translate(new Vector3(0, 0, zoom), Space.Self);  //相机位置向前或后移动zoom距离*/

        float mouseScollWheel = Input.GetAxis("Mouse ScrollWheel");
        currentZoomValue += mouseScollWheel * zoomSpeed ;
        //Debug.Log("Zoom:" + currentZoomValue + "view:" + Camera.main.fieldOfView);
        currentZoomValue = Mathf.Clamp(currentZoomValue, zoomMin, zoomMax);
        Camera.main.fieldOfView = currentZoomValue;

    }

    private void Rotate()
    {
        if(Input.GetMouseButton(0) && canRotation)
        {
            float yMove = Input.GetAxis("Mouse Y") * 10 * rotateSpeed * Time.deltaTime; //鼠标沿着屏幕Y轴移动时触发
            //yMove引起欧拉角X轴的变化，故angle.x -= yMove
            float rotatedAngle = currentAngleForX + yMove * (-1);
            //限制沿X轴上下旋转的范围
            if(rotatedAngle <= yMinLimit)
            {
                yMove = currentAngleForX - yMinLimit;
            }
            else if(rotatedAngle >= yMaxLimit)
            {
                yMove = -1 * (yMaxLimit - currentAngleForX);
            }

            //transform.RotateAround(rotaAxis, -Vector3.forward, yMove);
            transform.RotateAround(rotaAxis, -transform.right, yMove);
            currentAngleForX = ClampAngle(rotatedAngle, yMinLimit, yMaxLimit);

            //让Y轴左右旋转
            float xMove = Input.GetAxis("Mouse X") * 10 * rotateSpeed * Time.deltaTime;
            transform.RotateAround(rotaAxis, Vector3.up, xMove);

        }
    }

    private void Move()
    {
        if(Input.GetKey(KeyCode.W) && canMove)
        {
            float move =  moveSpeed * Time.deltaTime ;
            transform.Translate(new Vector3(0, -move, 0), Space.Self);
        }
        else if(Input.GetKey(KeyCode.S) && canMove)
        {
            float move = moveSpeed * Time.deltaTime ;
            transform.Translate(new Vector3(0, move, 0), Space.Self);
        }
        else if (Input.GetKey(KeyCode.A) && canMove)
        {
            float move = moveSpeed * Time.deltaTime ;
            transform.Translate(new Vector3(move, 0, 0), Space.Self);
        }
        else if (Input.GetKey(KeyCode.D) && canMove)
        {
            float move = moveSpeed * Time.deltaTime ;
            transform.Translate(new Vector3(-move, 0, 0), Space.Self);
        }
    }

    private void Focus()
    {
        if (Input.GetMouseButtonUp(0) && canMove)
        {
            
            Physics.queriesHitBackfaces = true;
            time2 = Time.realtimeSinceStartup;
            //双击时执行
            if (time2 - time1 < 0.3)
            {
                 //从当前摄像头到鼠标的位置创建射线
                 Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                 RaycastHit hit;
                 bool isHit = Physics.Raycast(ray, out hit);//射线检测碰撞体
                 if (isHit)
                 {
                     Debug.Log("camera focus");
                     canZoom = false;
                     canRotation = false;
                    //获取镜头聚焦时需要移动的目标坐标
                    Debug.Log("zoomMin:" + zoomMin + hit.point);
                     var dist = Vector3.Distance(transform.position, hit.point)/4;
                     moveTargetPoint = GetBetweenPointByDist(transform.position, hit.point, dist);
                     transform.position = moveTargetPoint;
                     //把摄像朝向目标点
                     transform.DOLookAt(hit.point, 1f).OnComplete(() =>
                     {
                         currentAngleForX = transform.eulerAngles.x;
                         canZoom = canRotation = true;
                     });

                     //重设旋转轴心点
                    // rotaAxis = new Vector3(hit.point.x, 0, hit.point.z);

                    Physics.queriesHitBackfaces = false;

                }
                 else
                 {
                     moveTargetPoint = Vector3.zero;
                 }
                
            }
            time1 = time2;
        }
    }

    private void DoCameraReset()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            CameraReset();
        }
    }

    public void CameraReset()
    {
        transform.position = originpos;
        transform.rotation = originrot;
        
        //Debug.Log("reset" + originFiled);
        //Camera.main.fieldOfView = originFiled;
        //transform.GetComponent<Camera>().DOFieldOfView(originFiled, 1);
        /*if (Input.GetKeyDown(KeyCode.H))
        {
            transform.position = originpos;
            transform.rotation = originrot;
        }*/
    }


    /// <summary>
    ///  角度限制在 min与max之间,且>=-360和<=360
    /// </summary>
    /// <param name="angle">当前角度</param>
    /// <param name="min">最小角度</param>
    /// <param name="max">最大角度</param>
    /// <returns>限制在 min与max之间,且>=-360和<=360的角度</returns>
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
    /// <summary>
    ///  获取两点之间一定距离的点
    /// </summary>
    /// <param name="start"> 开始坐标点 </param>
    /// <param name="end"> 终点坐标点</param>
    /// <param name="distance"> 要截取的长度</param>
    /// <returns> 两点之间一定距离的点 </returns>
    private Vector3 GetBetweenPointByDist(Vector3 start, Vector3 end, float distance)
    {
        Vector3 normal = (end - start).normalized;
        return normal * distance + start;
    }


    /*public void CameraCruiseByTarget(Vector3 target, UnityAction callback //定点环绕360度巡航
    {
        //获取镜头聚焦时需要移动的目标坐标
        var dist = Vector3.Distance(transform.position, target) - zoomMin * 10;
        moveTargetPoint = GetBetweenPointByDist(transform.position, target, dist);
        transform.position = moveTargetPoint;

        //禁止镜头动作
        canMove = false;
        canRotation = false;
        canZoom = false;

        //绑定回调事件
        cameraCruiseByTargetCallBackListener = new UnityEvent();
        cameraCruiseByTargetCallBackListener.AddListener(callback);

        //定时器调用
        InvokeRepeating("CameraCruiseByTargetTimer", 0, 0.001F);
    }
    //定时环绕360度巡航任务
    private void CameraCruiseByTargetTimer()
    {
        var angle = 10.0F * Time.deltaTime;
        transform.RotateAround(rotaAxis, Vector3.up, angle);
        cameraCruiseByTargetAngle += angle;

        if(cameraCruiseByTargetAngle >= 360)
        {
            //停止调用
            CancelInvoke("CameraCruiseByTargetTimer");

            //恢复镜头动作
            cameraCruiseByTargetAngle = 0;
            canMove = true;
            canRotation = true;
            canZoom = true;

            //回调
            cameraCruiseByTargetCallBackListener.Invoke();
        }
    }

    //定线巡航
    public void CameraCruiseByPosList(List<Vector3> posList, UnityAction callback)
    {
        canMove = false;
        canRotation = false;
        canZoom = false;

        Camera c = transform.GetComponent<Camera>();
        c.DONearClipPlane(10, 1.0f);
        c.DOFarClipPlane(3000, 1.0f);
        cameraCruiseByPosListCallBackListener = new UnityEvent();
        cameraCruiseByPosListCallBackListener.AddListener(callback);


        cameraCruisePosListCount = 0;
        cameraCruisePosList = posList;

        transform.DOPath(poses, 100f).OnPlay(() =>{
            }
        });
        for(int i= 0; i<cameraCruisePosList.Count; i++)
        {
            float time = 5.1f * i;      //时间间隔设置 5.1 = CameraCruiseByPosListTimer里lookAt时间+move时间
            Invoke("CameraCruiseByPosListTimer", time);
        }
    }

    private void CameraCruiseByPosListTimer()  //定线巡航任务
{

        Vector3 pos = cameraCruisePosList[cameraCruisePosListCount];
        cameraCruisePosListCount++;

        //把摄像朝向目标点
        transform.DOLookAt(new Vector3(pos.x, 0, pos.z), 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            //镜头移动至目标点
            transform.DOMove(pos, 5F).SetEase(Ease.Linear).OnComplete(() =>
            {
                //最后一次
                if (pos == cameraCruisePosList[cameraCruisePosList.Count - 1]
                )
                {
                    CancelInvoke("CameraCruiseByPosListTimer");

                    cameraCruisePosListCount = 0;
                    canMove = true;
                    canRotation = true;
                    canZoom = true;

                    Camera c = transform.GetComponent<Camera>();
                    c.DONearClipPlane(100, 1.0f);
                    c.DOFarClipPlane(8000, 1.0f);

                    cameraCruiseByPosListCallBackListener.Invoke();
                }
            });
        });
    }*/



}
