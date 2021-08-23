using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System;
using System.Net;


/// <summary>
/// Этот скрипт создаёт плоскость, если найден якорь (базовая точка), затем
/// размешает маркер на плоскости и позволяет создавать объекты в нужных местах с линиями между ними
/// т.е. реализует процесс создания AR сцены
/// </summary>

public class ARSceneMakingManager : MonoBehaviour
{
    public GameObject parentPlane; // плоскость-родитель для всех остальных объектов
    private GameObject PlaneMarkerPrefab; // маркер (картинка круга)
    [SerializeField] private GameObject MarkerCircle;
    [SerializeField] private GameObject MarkerCircle_White;
    [SerializeField] private GameObject MarkerPoint;
    private bool SwapColorMarker = false;
    private GameObject ObjectToSpawn; // объект, который будем ставить на сцену
    [SerializeField] private GameObject PillarPrefab;
    [SerializeField] private GameObject RedFlagPrefab;
    [SerializeField] private GameObject WhiteFlagPrefab;
    [SerializeField] private GameObject TreePrefab;
    [SerializeField] private GameObject HatchwayPrefab;
    [SerializeField] private GameObject DeleteObjPrefab;
    private Vector3 point; // точка перечения луча и плоскости
    public Text TextLog;  // лог на canvas
    private ARTrackedImageManager myARTrackedImageManager; // чтобы выключать/выключать компонент ARTrackedImageManager
    private GameObject FindObject; // найденный объект (для переименования)
    private GameObject FindPlane; // найденная плоскость
    private GameObject FindPlaneTEST; // найденная плоскость
    private int Flag_white_num  = 0; // порядковый номер созданного белого флага
    private int numObject = 0;
    private float numObjectBoundaryRadarogramFlag = 0;
    private float AddParallelNum = 0;
    private bool PermissionIncrement = true;
    LineRenderer lineRenderer; // для отрисовки линий между объектами (треки)
    private bool SplitCameraON; // true - выбран VR режим, false - выбран обычный режим
    private int RayParameter; // число (2/4) означающее часть экрана откуда начинается луч
    private int NewI = 0;
    private ARPlaneManager planeManager;
    [SerializeField] private GameObject inscriptionTable; // надпись во всех экран в начале
    [SerializeField] private GameObject radarogram; // радарограмма
    [SerializeField] private GameObject whiteLine; // белая линия между белыми флагами
    [SerializeField] private GameObject redLine; // красная линия между красными флагами
    [SerializeField] private GameObject greenLine; // зелёная линия между красными флагами
    private bool BoundaryRadarogramFlag = false;
    private bool BoundaryRedFlag = false;
    private float numObjectBoundaryRedFlag = 0;
    private Vector3 euler;
    private bool PermissionInst = false; // Разрешение на установку объекта
    [SerializeField] private GameObject BasePointGizmo;
    //int intcheck = 0;
    private GameObject FindButton = null;
    private GameObject LastFindButton;
    // private int SelectObj; // Какой из объектов для установки на сцену выбран (1-столбик, 2-красный флаг, 3-радарограмма, 4-дерево)
    // private int SelectTypeObj; // Какой тип установки объектов выбран (1-одиночные(отдельные)объекты, 2-комплекс объектов)
    [SerializeField] private GameObject ARPlanePrefab;
    // private int timeTracking = 0;
    [SerializeField] private GameObject ARCamera;
    [SerializeField] private ARMenu ARMenuScript;
    private bool checkDeleteObj = false;
    private RaycastHit hit;
    private int LineColor; // цвет линии между красными флагами: 1-красная, 2-зеленая
    private GameObject StripedLine;
    private GameObject InstObj; // последний установленный объект
    private GameObject newWLine; // последняя созданная белая линия
    private GameObject newRLine; // последняя созданная красная линия
    [SerializeField] private Text distance_to_marker;
    [SerializeField] private GameObject SmartPhone;

    void Start()
    {   
        SplitCameraON = DataHolder.SplitCameraON;
        if (SplitCameraON){
            RayParameter = 4;
        }
        else{
            RayParameter = 2;
        }
        PlaneMarkerPrefab = MarkerCircle;
        PlaneMarkerPrefab.SetActive(false); // убираем маркер до нахождения плоскости
        myARTrackedImageManager = GetComponent<ARTrackedImageManager>(); // компонент распознавания изображений
        planeManager = GetComponent<ARPlaneManager>(); // компонент отслеживания плоскостей
        planeManager.enabled = false;
        //Instantiate(parentPlane); // ДЛЯ ТЕСТОВ В РЕДАКТОРЕ
        // Отключение визуализации распознанных плоскостей
        ARPlanePrefab.GetComponent<ARPlaneMeshVisualizer>().enabled = false;
        ARPlanePrefab.GetComponent<MeshRenderer>().enabled = false;
        DeleteObjPrefab.SetActive(false);
        distance_to_marker.enabled = false; // отключаю отображение расстояния до маркера, пока не будет создана плоскость
    } 

    void Update()
    {
        FindGizmo(); // при нахождении базовой точки создать или позиционировать плоскость относительно неё
        PlanesManager(); // выделение нужных плоскостей и дополнительное позиционирование базовой плоскости относительно них
        ShowMarker(); // отображение маркера на плоскости
        InstantiateMyObject(); // установка объекта по нажатию
        //MyVRControllerTEST();
        //OnGUI();
        SpawnRadarogram();
        DeleteObject();
        DistanceToMarker();
    }

    public void CatDownload()
    {
        WebClient webClient = new WebClient();
        //webClient.DownloadFile("https://medialeaks.ru/wp-content/uploads/2017/10/catbread-03-600x400.jpg", "Assets/cat.jpg");
        webClient.DownloadFile("https://medialeaks.ru/wp-content/uploads/2017/10/catbread-04-600x400.jpg", "Assets/cat.jpg");
    }

    void FindGizmo()
    {
        GameObject[] allGo = FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGo)
        {
            if (go.name == "Number")
            {
                go.transform.LookAt(ARCamera.transform);
                // //go.transform.rotation = Quaternion.identity;
                // float rotateX = go.transform.rotation.eulerAngles.x;
                float rotateY = go.transform.rotation.eulerAngles.y;
                // float rotateZ = go.transform.rotation.eulerAngles.z;
                go.transform.rotation = Quaternion.Euler(0, rotateY+180, 0);
            }
            if(go.CompareTag("BaseGizmo"))
            {   
                if (go.GetComponent<ARAnchor>() == null)
                {
                    go.AddComponent<ARAnchor>();
                }
                // if (intcheck > 4 && intcheck < 7)
                // {
                //     Instantiate(BasePointGizmo, go.transform.position, go.transform.rotation);
                //     // secondGizmo.name = "NewBasePoint";
                // }
                // if (intcheck < 8)
                // {
                //     intcheck++;
                // }
                inscriptionTable.SetActive(false);
                distance_to_marker.enabled = true;
                planeManager.enabled = true;
                // if(timeTracking < 300)
                // {
                //     TextLog.text = timeTracking.ToString();
                //     timeTracking++;
                // }
                // if(timeTracking == 300)
                // {
                //     TextLog.text = "timeTracking == 300";
                //     timeTracking++;
                //     // GameObject.Find("ARPlane").GetComponent<ARPlaneMeshVisualizer>().enabled = false;
                //     // GameObject.Find("ARPlane").GetComponent<MeshRenderer>().enabled = false;
                //     // GameObject.Find("ARPlane(Clone)").GetComponent<ARPlaneMeshVisualizer>().enabled = false;
                //     // GameObject.Find("ARPlane(Clone)").GetComponent<MeshRenderer>().enabled = false;
                //     // ARPlanePrefab.GetComponent<ARPlaneMeshVisualizer>().enabled = false;
                //     // ARPlanePrefab.GetComponent<MeshRenderer>().enabled = false;
                // }
                if ((FindPlane = GameObject.Find("BasePlane(Clone)")) == true) // если плоскость уже создана позиционируем её относительно Gizmo
                {   
                    if ((DataHolder.CheckBox != true) && (NewI > 10)) // пользователь решил не перераспознавать QR-код
                    {
                        // включение/выключение компонента ARTrackedImageManager, т.е. отслеживания QR-кода
                        // myARTrackedImageManager.enabled = !myARTrackedImageManager.enabled;
                        myARTrackedImageManager.enabled = false;
                    }
                    else
                    {
                        FindPlane.transform.position = go.transform.position;
                        FindPlane.transform.rotation = go.transform.rotation;
                        if (NewI <= 10)
                        {
                            NewI++;
                        }
                    }
                }
                else
                {
                    FindPlaneTEST = Instantiate(parentPlane, go.transform.position, go.transform.rotation, go.transform); // в другом случае, создаём её
                    //FindPlaneTEST.AddComponent<ARAnchor>();
                    //FindPlaneTEST.GetComponent<ARAnchorManager>().AddAnchor();
                }
            }
        }
    }

    // отображение маркера (картинка круга)
    void ShowMarker()
    {
        //RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / RayParameter, Screen.height / 2, 0)); // отправляем луч из центра экрана
        
        if(Physics.Raycast(ray, out hit) == true) // ели пересечение было записываем в hit
        {
            point = hit.point; // точка пересечения луча с объектом
            //TextLog.text = hit.collider.gameObject.name; // выводим имя объекта на который смотрим
            if (hit.collider.gameObject.name == "BasePlanePrefab")
            {
                PermissionInst = true;
                PlaneMarkerPrefab.SetActive(false);
                if (SwapColorMarker)
                {
                    PlaneMarkerPrefab = MarkerCircle;
                }
                else
                {
                    PlaneMarkerPrefab = MarkerCircle_White;
                }
                PlaneMarkerPrefab.SetActive(true);
            }
            else
            {
                PermissionInst = false;
                PlaneMarkerPrefab.SetActive(false);
                PlaneMarkerPrefab = MarkerPoint;
                PlaneMarkerPrefab.SetActive(true);

                string ObjName = hit.collider.gameObject.name;
                switch (ObjName)
                {
                    case "MenuPlane":
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        break;
                    case "SaveButtonCube":
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            GameObject.Find("scripts").GetComponent<ObjectManager>().SaveButton();
                            // FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "LoadButtonCube":
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            GameObject.Find("scripts").GetComponent<ObjectManager>().LoadButton();
                            // FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "SendButtonCube":
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            GameObject.Find("scripts").GetComponent<Saver>().SendSaveJSON();
                            // FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "DeleteButtonCube":
                        CleaningNameObject();
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            //Destroy(InstObj);
                            checkDeleteObj = true;
                            ARMenuScript.ARMenuPlanePrefab.gameObject.SetActive(false);
                            ARMenuScript.checkMenuActive = false;
                        }
                        break;
                    case "PillarButtonCube":
                        CleaningNameObject();
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            ObjectToSpawn = PillarPrefab;
                            BoundaryRedFlag = false;
                            BoundaryRadarogramFlag = false;
                            GameObject.Find("RadarogramButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("TreeButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("HatchwayButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube_GreenLine").GetComponent<Renderer>().material.color = Color.white;
                            FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "RedFlagButtonCube":
                        CleaningNameObject();
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            LineColor = 1;
                            ObjectToSpawn = RedFlagPrefab;
                            BoundaryRedFlag = true;
                            BoundaryRadarogramFlag = false;
                            GameObject.Find("PillarButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RadarogramButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("TreeButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("HatchwayButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube_GreenLine").GetComponent<Renderer>().material.color = Color.white;
                            FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "RedFlagButtonCube_GreenLine":
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            LineColor = 2;
                            ObjectToSpawn = RedFlagPrefab;
                            BoundaryRedFlag = false;
                            BoundaryRadarogramFlag = false;
                            GameObject.Find("PillarButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RadarogramButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("TreeButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("HatchwayButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "RadarogramButtonCube":
                        CleaningNameObject();
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            ObjectToSpawn = WhiteFlagPrefab;
                            BoundaryRedFlag = false;
                            BoundaryRadarogramFlag = true;
                            GameObject.Find("PillarButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("TreeButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("HatchwayButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube_GreenLine").GetComponent<Renderer>().material.color = Color.white;
                            FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "TreeButtonCube":
                        CleaningNameObject();
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            ObjectToSpawn = TreePrefab;
                            BoundaryRedFlag = false;
                            BoundaryRadarogramFlag = false;
                            GameObject.Find("PillarButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RadarogramButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("HatchwayButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube_GreenLine").GetComponent<Renderer>().material.color = Color.white;
                            FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "HatchwayButtonCube":
                        CleaningNameObject();
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            ObjectToSpawn = HatchwayPrefab;
                            BoundaryRedFlag = false;
                            BoundaryRadarogramFlag = false;
                            GameObject.Find("PillarButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RadarogramButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("TreeButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            GameObject.Find("RedFlagButtonCube_GreenLine").GetComponent<Renderer>().material.color = Color.white;
                            FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "OneObjButtonCube":
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            PermissionIncrement = true;
                            GameObject.Find("SomeObjButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "SomeObjButtonCube":
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            PermissionIncrement = false;
                            numObject++;
                            GameObject.Find("OneObjButtonCube").GetComponent<Renderer>().material.color = Color.white;
                            FindButton.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        break;
                    case "SwapColorMarkerButtonCube":
                        LastFindButton = FindButton;
                        FindButton = GameObject.Find(ObjName);
                        checkDeleteObj = false;
                        if ((LastFindButton != FindButton) && (LastFindButton.GetComponent<Renderer>().material.color == Color.grey))
                        {
                            LastFindButton.GetComponent<Renderer>().material.color = Color.white;
                        }
                        if (FindButton.GetComponent<Renderer>().material.color != Color.blue)
                        {
                            FindButton.GetComponent<Renderer>().material.color = Color.grey;
                        }
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            SwapColorMarker = !SwapColorMarker;
                            if (FindButton.GetComponent<Renderer>().material.color == Color.blue)
                            {
                                FindButton.GetComponent<Renderer>().material.color = Color.white;
                            }
                            else
                            {
                                FindButton.GetComponent<Renderer>().material.color = Color.blue;
                            }
                        }
                        break;
                    // default:
                    //     что-то сделать
                    //     break;
                }
            }
            PlaneMarkerPrefab.transform.position = point; // ставим в это место маркер (картинка круга)
            PlaneMarkerPrefab.SetActive(true); // показываем маркер
        }
    }

    // установка новой копии объекта на сцену
    void InstantiateMyObject()
    {
        // Нажали на нижнюю кнопку на VR контроллере под указательным пальцем
        if(/*(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)||*/ (Input.GetKeyDown(KeyCode.LeftShift)) && (PermissionInst == true))
        {
            // point = new Vector3(1, 1, 1);
            InstObj = Instantiate(ObjectToSpawn, point, ObjectToSpawn.transform.rotation, FindPlane.transform); // ставим в определенное заранее место наш объект + делаем его потомком плоскости
            if ((PermissionIncrement == true) && (BoundaryRadarogramFlag == false) && (BoundaryRedFlag == false))
            {
                numObject++;
                InstObj.transform.Find("Number").GetComponent<TextMesh>().text = numObject.ToString();
            }
            else if(PermissionIncrement == false && (BoundaryRadarogramFlag == false) && (BoundaryRedFlag == false))
            {
                InstObj.transform.Find("Number").GetComponent<TextMesh>().text = numObject.ToString();
            }
            else if(BoundaryRadarogramFlag == true) // Устанавливаемый объект - граница профиля
            {   
                numObjectBoundaryRadarogramFlag++;
                AddParallelNum++;
                if(AddParallelNum % 2 == 0)
                {
                    InstObj.transform.Find("Number").GetComponent<TextMesh>().text = (numObjectBoundaryRadarogramFlag - 0.9f).ToString();
                    InstObj.transform.Find("Number").GetComponent<MeshRenderer>().enabled = false; // Выключаю отображение вторых номерков у профилей!
                    numObjectBoundaryRadarogramFlag--;
                }
                else
                {
                    InstObj.transform.Find("Number").GetComponent<TextMesh>().text = numObjectBoundaryRadarogramFlag.ToString();
                }
            }
            else if(BoundaryRedFlag == true) // Устанавливаемый объект - граница красной линии
            {
                if(numObjectBoundaryRedFlag < 10) // ИСПРАВИТЬ! пока только до 10ти можно ставить красную линию с значением 0.1 - 0.9
                {
                    numObjectBoundaryRedFlag++;
                    // float f = numObjectBoundaryRedFlag/10;
                    InstObj.transform.Find("Number").GetComponent<TextMesh>().text = (numObjectBoundaryRedFlag/10).ToString();
                    InstObj.transform.Find("Number").GetComponent<MeshRenderer>().enabled = false;
                    InstObj.transform.Find("Number_fix").GetComponent<TextMesh>().text = numObjectBoundaryRedFlag.ToString();
                }
                else
                {
                    numObjectBoundaryRedFlag++;
                    InstObj.transform.Find("Number").GetComponent<TextMesh>().text = (numObjectBoundaryRedFlag).ToString();
                }
            }

            if (InstObj.name == "Flag_white(Clone)") // если свежесозданный объект - граница профиля
            {
                Flag_white_num++;
                InstObj.tag = "Important_flag"; // Чтобы этот объект нельзя было удалить рандомно через кнопку удалить
                InstObj.name = ("BoundaryRadarogram" + Flag_white_num); // переименовываем его с добавлением порядкового номера
                if ((Flag_white_num % 2) == 0) // сбрасываем счетчик, если втрой флаг был установлен
                {
                    Flag_white_num = 0;
                }
            }
            if (InstObj.name == "Flag_red(Clone)") // если свежесозданный объект - граница искомого объекта (красная или зелёная линия)
            {
                Flag_white_num++;
                if (LineColor == 1) // красная линия
                {
                    InstObj.tag = "Important_flag"; // Чтобы этот объект нельзя было удалить рандомно через кнопку удалить, а можно было только через нижнюю кнопку на пульте
                }
                InstObj.name = ("BoundaryFindObj" + Flag_white_num); // переименовываем его с добавлением порядкового номера
                if ((Flag_white_num % 2) == 0) // сбрасываем счетчик, если втрой флаг был установлен
                {
                    Flag_white_num = 0;
                }
            }
            //FindObject.AddComponent<ARAnchor>(); // вешаю на него компонент якоря в пространстве
        }
    }

    void PlanesManager()
    {
        foreach (var plane in planeManager.trackables)
        {
            //TextLog.text = Mathf.Abs(plane.transform.position.y - FindPlaneTEST.transform.position.y).ToString();
            if (Mathf.Abs(plane.transform.position.y - FindPlaneTEST.transform.position.y) > 0.2)
            {
                plane.gameObject.SetActive(false);
            }
            else
            {
                // Появилась проблема с тем, что теперь сцена не поворачивается вместе с маркером
                euler = plane.transform.rotation.eulerAngles;
                euler.y = FindPlaneTEST.transform.rotation.eulerAngles.y;
                FindPlane.transform.rotation = Quaternion.Euler(euler);
            }
        }
    }

    void SpawnRadarogram()
    {   
        // если найдены 2 границы из белых флагов
        if (GameObject.Find("BoundaryRadarogram1"))
        {
            // PlaneMarkerPrefab.SetActive(false);
            // PlaneMarkerPrefab = MarkerArrow;
            // PlaneMarkerPrefab.SetActive(true);
            if (GameObject.Find("BoundaryRadarogram2"))
            {
                // PlaneMarkerPrefab.SetActive(false);
                // PlaneMarkerPrefab = MarkerCircle;
                // PlaneMarkerPrefab.SetActive(true);
                Vector3 FirstPosition = GameObject.Find("BoundaryRadarogram1").transform.position;
                Vector3 SecondPosition = GameObject.Find("BoundaryRadarogram2").transform.position;
                Vector3 DirectionVector = SecondPosition - FirstPosition;
                float MyMagnitude = DirectionVector.magnitude;
                //Vector3 normalizedVector = DirectionVector / MyMagnitude;
                //float MyDistance = Vector3.Distance(FirstPosition,SecondPosition);
                //Vector3 normalizedDirection = DirectionVector.normalized;
                //SecondPosition = FirstPosition + (normalizedDirection * 6);
                //GameObject.Find("Boundary2").transform.position = SecondPosition;
                Vector3 MyCenter = Vector3.Lerp(FirstPosition,SecondPosition,0.5f);

                //GameObject RG = Instantiate(radarogram, MyCenter, Quaternion.identity, FindPlane.transform);
                newWLine = Instantiate(whiteLine, MyCenter, Quaternion.identity, FindPlane.transform);
                newWLine.tag = "Important_flag"; // Чтобы этот объект нельзя было удалить рандомно через кнопку удалить
                newWLine.transform.LookAt(SecondPosition);
                newWLine.transform.localScale = new Vector3(0.01f, 1f, 0.1f * MyMagnitude);
                
                //RG.transform.LookAt(SecondPosition);
                // Vector3 currentPos = RG.transform.position;
                //Vector3 testAddPos = new Vector3(0, 0.745f, 0);
                //RG.transform.position += testAddPos;
                //RG.transform.Rotate(0,-90,0);
                //RG.SetActive(false);
                // LineDrawingButton("Boundary1","Boundary2");
                GameObject.Find("BoundaryRadarogram1").name = "BoundaryRadarogram_1_" + numObjectBoundaryRadarogramFlag.ToString(); // таким образом, после полного создания профиля, флаги будут хранится как
                GameObject.Find("BoundaryRadarogram2").name = "BoundaryRadarogram_2_" + numObjectBoundaryRadarogramFlag.ToString(); // BoundaryRadarogram_1/2(начало/конец)_номерпрофиля
            }
        }
        // если найдены 2 границы из красных флагов
        if (GameObject.Find("BoundaryFindObj1") && GameObject.Find("BoundaryFindObj2"))
        {
            Vector3 FirstPosition = GameObject.Find("BoundaryFindObj1").transform.position;
            Vector3 SecondPosition = GameObject.Find("BoundaryFindObj2").transform.position;
            Vector3 DirectionVector = SecondPosition - FirstPosition;
            float MyMagnitude = DirectionVector.magnitude;
            Vector3 MyCenter = Vector3.Lerp(FirstPosition,SecondPosition,0.5f);

            if (LineColor == 1)
            {
                StripedLine = redLine;
            }
            else if (LineColor == 2)
            {
                StripedLine = greenLine;
            }
            newRLine = Instantiate(StripedLine, MyCenter, Quaternion.identity, FindPlane.transform);
            newRLine.transform.LookAt(SecondPosition);
            newRLine.GetComponent<SpriteRenderer>().size = new Vector2(MyMagnitude ,0.31f);
            newRLine.transform.Rotate(90,newRLine.transform.rotation.y + 90,0);
            //newWLine.transform.localScale = new Vector3(0.01f, 1f, 0.1f * MyMagnitude);

            if (LineColor == 1) // красная линия
            {
                newRLine.tag = "Important_flag"; // Чтобы этот объект нельзя было удалить рандомно через кнопку удалить;
                GameObject.Find("BoundaryFindObj1").name = "BoundaryFindObj_1_" + numObjectBoundaryRedFlag.ToString();
                GameObject.Find("BoundaryFindObj2").name = "BoundaryFindObj_2_" + numObjectBoundaryRedFlag.ToString();
            }
            else if (LineColor == 2) // зеленая линия
            {
                GameObject.Find("BoundaryFindObj1").name = "BoundaryFindObj";
                GameObject.Find("BoundaryFindObj2").name = "BoundaryFindObj1"; // 2 переименовываем в 1 чтобы зеленая линия соединяла все флаги в группе
                Flag_white_num++; // повысил счетчик, чтобы показать что первый флаг уже создан и новый флаг был сразу вторым
            }
        }

        // Измерение расстояния между двумя объектами
        // if (GameObject.Find("point11") && GameObject.Find("point22"))
        // {
        //     Vector3 FirstPosition = GameObject.Find("point11").transform.position;
        //     Vector3 SecondPosition = GameObject.Find("point22").transform.position;
        //     Vector3 DirectionVector = SecondPosition - FirstPosition;
        //     float MyMagnitude = DirectionVector.magnitude;
        // //     Debug.Log("DirectionVector: " + DirectionVector.ToString());
        //     Debug.Log("MyMagnitude: " + MyMagnitude.ToString());
        // }

    }

    // эта функция подчищает названия объектов, обнуляет счетчики и тд, при завершении установки групп объектов
    void CleaningNameObject()
    {
        // для зелёной линии
        Flag_white_num = 0;
        if (GameObject.Find("BoundaryFindObj1"))
        {
            GameObject.Find("BoundaryFindObj1").name = "BoundaryFindObj";
        }
        if (GameObject.Find("BoundaryFindObj2"))
        {
            GameObject.Find("BoundaryFindObj2").name = "BoundaryFindObj";
        }
    }
    void DeleteObject()
    {
        String substring = "";
        // Если нажали кнопку "удалить последний созданный объект"
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            try
            {
                // пробую выделить из названия объекта "BoundaryRadarogram_N"
                if (InstObj.name.Substring(0,12) == "BoundaryFind")
                    substring = InstObj.name.Substring(0,17); // для "BoundaryFindObj_2"
                if (InstObj.name.Substring(0,13) == "BoundaryRadar")
                    substring = InstObj.name.Substring(0,20); // для "BoundaryRadarogram_2"

                // это работает так:
                /*
                    String value = "BoundaryRadarogram_1_13";
                    int startIndex = 0;
                    int length = 20;
                    String substring = value.Substring(startIndex, length);
                    Console.WriteLine(substring); // BoundaryRadarogram_1
                */

            }
            catch
            {
                // ошибка из-за того что имя короче чем "BoundaryRadarogram"
            }

            // ниже два if для белого флага
            if (InstObj.name == "BoundaryRadarogram1") // если объект, который пытаются удалить - первая граница профиля - значит нужно декрементировать номер профиля и счетчик Flag_white_num
            {
                Destroy(InstObj); // удаляем флаг
                numObjectBoundaryRadarogramFlag--; // декрементируем, чтобы при установке следующего, номер не перепрыгнул
                Flag_white_num--; // чтобы вновь созданный флаг опять был "BoundaryRadarogram1"
                AddParallelNum--; // сквозная нумерация для определения четности/нечетности
            }
            else if(substring == "BoundaryRadarogram_2") // если объект, который пытаются удалить - вторая граница профиля - нужно удалить объект и последнюю созданную линию,
            {                                            // затем переименовать флаг налача профиля и откатить нумерацию, чтобы можно было поставить второй флаг и линия заново создалась
                Destroy(InstObj); // удаляем флаг
                Destroy(newWLine); // удаляем последнюю созданную линию

                //TextLog.text = "ищу " + "BoundaryRadarogram_1" + InstObj.name.Substring(20);
                GameObject StartProfile = GameObject.Find("BoundaryRadarogram_1" + InstObj.name.Substring(20)); // ищем флаг - начало данного профиля
                StartProfile.name = "BoundaryRadarogram1";
                Flag_white_num++; // повысил счетчик, чтобы показать что первый флаг уже создан и новый флаг был сразу вторым
                //numObjectBoundaryRadarogramFlag++;
                AddParallelNum--; // сквозная нумерация для определения четности/нечетности
            }

            // ниже два if для красного флага
            else if((InstObj.name == "BoundaryFindObj1") && (InstObj.tag == "Important_flag")) // доп условие, чтобы не удалялись флаги на зелёных линиях
            {
                Destroy(InstObj); // удаляем флаг
                numObjectBoundaryRedFlag--; // декрементируем, чтобы при установке следующего, номер не перепрыгнул
                Flag_white_num--; // чтобы вновь созданный флаг опять был "BoundaryFindObj1"
            }
            else if(substring == "BoundaryFindObj_2")
            {
                Destroy(InstObj); // удаляем флаг
                Destroy(newRLine); // удаляем последнюю созданную красную линию
                numObjectBoundaryRedFlag--; // декрементируем, чтобы при установке следующего, номер не перепрыгнул

                //TextLog.text = "ищу " + "BoundaryFindObj_1" + InstObj.name.Substring(17);
                GameObject StartProfile = GameObject.Find("BoundaryFindObj_1" + InstObj.name.Substring(17)); // ищем флаг - начало данного профиля
                StartProfile.name = "BoundaryFindObj1";
                Flag_white_num++; // повысил счетчик, чтобы показать что первый флаг уже создан и новый флаг был сразу вторым
            }

        }
        // Если была нажата кнопка Delete в AR-меню
        if (checkDeleteObj)
        {
            if ((hit.collider.gameObject.name != "BasePlanePrefab") && (hit.collider.gameObject.name != "DeleteButtonCube") && (hit.collider.gameObject.tag != "Important_flag"))
            {
                //TextLog.text = hit.collider.gameObject.name;
                DeleteObjPrefab.transform.position = hit.collider.gameObject.transform.position;
                DeleteObjPrefab.transform.LookAt(ARCamera.transform);
                DeleteObjPrefab.SetActive(true);
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    Destroy(hit.collider.gameObject);
                }
            }
        }
        else
        {
            DeleteObjPrefab.SetActive(false);
        }
    }

    // создаю линию между двумя флагами
    public void LineDrawingButton(string obj1, string obj2)
    { 
        lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
        //lineRenderer.startColor = Color.white;
        lineRenderer.material.SetColor("color",Color.white);
        //lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;    

        FindObject = GameObject.Find(obj1);
        lineRenderer.SetPosition(0, FindObject.transform.position);
        FindObject = GameObject.Find(obj2);
        lineRenderer.SetPosition(1, FindObject.transform.position);
    }

    private void DistanceToMarker()
    {
        // Измерение расстояния между двумя объектами
        Vector3 FirstPosition = SmartPhone.transform.position; // позиция телефона
        FirstPosition.y = 0; // обнуляю координату по высоте, чтобы не учитывать рост человека
        Vector3 SecondPosition = PlaneMarkerPrefab.transform.position; // позиция маркера
        SecondPosition.y = 0; // обнуляю по высоте координату маркета, потому что она почему-то не 0, а -0.9, хотя видно что маркер лежит в плоскости пола
        // TextLog.text = "|";
        // TextLog.text += FirstPosition.ToString();
        // TextLog.text += "|";
        // TextLog.text += SecondPosition.ToString();
        Vector3 DirectionVector = SecondPosition - FirstPosition;
        float MyMagnitude = DirectionVector.magnitude;
        distance_to_marker.text = MyMagnitude.ToString("0.00") + " м";
    }

    // выводит какие команды посылает VR контроллер
    // void OnGUI()
    // {
    //     if (Event.current.isKey && Event.current.type == EventType.KeyDown)
    //     {
    //         Debug.Log(Event.current.keyCode);
    //         TextLog.text = Event.current.keyCode.ToString();
    //     }
    // }
    // для тестирования какие команды посылает VR контроллер
    void MyVRControllerTEST()
    {
        // фазы нажатия ЛКМ
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("МЫШКА DOWN");
            TextLog.text = "МЫШКА DOWN";
        }
        if (Input.GetMouseButton(0))
        {
            Debug.Log("МЫШКА HOLD");
            TextLog.text = "МЫШКА HOLD";
        }
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("МЫШКА UP");
            TextLog.text = "МЫШКА UP";
        }

        // Следующие команды работают только когда пульт в режиме VR - нажать (@ + C)
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("НИЖНЯЯ кнопка на VR контроллере под указательным пальцем");
        }
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            Debug.Log("ВЕРХНЯЯ кнопка на VR контроллере под указательным пальцем");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            Debug.Log("Верхняя (C) кнопка пульт");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            Debug.Log("Правая (A) кнопка пульт");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            Debug.Log("Левая (B) кнопка пульт");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            Debug.Log("Нижняя (D) кнопка пульт");
        }
    }
}
