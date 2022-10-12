using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;

/// <summary>
/// Этот скрипт реализует возможность установки PNG изображений радарограмм из файла.
/// (Большая часть кода портирована из проекта под OculusQuest2)
/// </summary>
public class InstallRadarogramsOnTheARScene : MonoBehaviour
{
    GameObject FirstObg_1, FirstObg_2, FirstObg_3, FirstObg_4, FirstObg_5, FirstObg_6, FirstObg_7, FirstObg_8, FirstObg_9, FirstObg_10;
    Vector3 FirstPosition_1, FirstPosition_2, FirstPosition_3, FirstPosition_4, FirstPosition_5, FirstPosition_6, FirstPosition_7, FirstPosition_8, FirstPosition_9, FirstPosition_10;
    GameObject SecondObg_1, SecondObg_2, SecondObg_3, SecondObg_4, SecondObg_5, SecondObg_6, SecondObg_7, SecondObg_8, SecondObg_9, SecondObg_10;
    Vector3 SecondPosition_1, SecondPosition_2, SecondPosition_3, SecondPosition_4, SecondPosition_5, SecondPosition_6, SecondPosition_7, SecondPosition_8, SecondPosition_9, SecondPosition_10;
    GameObject RG_1, RG_2, RG_3, RG_4, RG_5, RG_6, RG_7, RG_8, RG_9, RG_10;
    Vector3 normalizedDirection_1, normalizedDirection_2, normalizedDirection_3, normalizedDirection_4, normalizedDirection_5, normalizedDirection_6, normalizedDirection_7, normalizedDirection_8, normalizedDirection_9, normalizedDirection_10;
    Vector3 MyCenter_1, MyCenter_2, MyCenter_3, MyCenter_4, MyCenter_5, MyCenter_6, MyCenter_7, MyCenter_8, MyCenter_9, MyCenter_10;
    private int RadarogramCounter = 0;
    private GameObject FindPlane; // найденная плоскость
    [SerializeField] private GameObject radarogramPrefab; // заготовка под радарограмму
    private bool BlanksSet = false; // чтобы при выборе другого архива с радарограммами, менялись картинки на старых заготовках, а не распознавались новые
    private JsonRadarogramReader.CommonData myJsonRadarogramData;
    string zipPath; // сюда нужно передать путь до архива из файлового менеджера
    string extractPath; // куда будем распаковывать архивы

    private void Awake()
    {
        extractPath = Application.persistentDataPath + "/extractZIP"; // куда будем распаковывать архивы
    }

    // Сбор основных объектов сцены
    private void FindWhiteFlag()
    {
        GameObject[] allGo = FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGo)
        {
            if (go.name == "Flag_white(Clone)")
            {
                if (go.transform.Find("Number").GetComponent<TextMesh>().text == "1")
                {
                    //SecondObgList.Insert(0, go);
                    RadarogramCounter++;
                    FirstObg_1 = go;
                    FirstPosition_1 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "1,1")
                {
                    SecondObg_1 = go;
                    SecondPosition_1 = go.transform.position;
                }

                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "2")
                {
                    RadarogramCounter++;
                    FirstObg_2 = go;
                    FirstPosition_2 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "2,1")
                {
                    SecondObg_2 = go;
                    SecondPosition_2 = go.transform.position;
                }

                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "3")
                {
                    RadarogramCounter++;
                    FirstObg_3 = go;
                    FirstPosition_3 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "3,1")
                {
                    SecondObg_3 = go;
                    SecondPosition_3 = go.transform.position;
                }

                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "4")
                {
                    RadarogramCounter++;
                    FirstObg_4 = go;
                    FirstPosition_4 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "4,1")
                {
                    SecondObg_4 = go;
                    SecondPosition_4 = go.transform.position;
                }

                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "5")
                {
                    RadarogramCounter++;
                    FirstObg_5 = go;
                    FirstPosition_5 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "5,1")
                {
                    SecondObg_5 = go;
                    SecondPosition_5 = go.transform.position;
                }

                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "6")
                {
                    RadarogramCounter++;
                    FirstObg_6 = go;
                    FirstPosition_6 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "6,1")
                {
                    SecondObg_6 = go;
                    SecondPosition_6 = go.transform.position;
                }

                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "7")
                {
                    RadarogramCounter++;
                    FirstObg_7 = go;
                    FirstPosition_7 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "7,1")
                {
                    SecondObg_7 = go;
                    SecondPosition_7 = go.transform.position;
                }

                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "8")
                {
                    RadarogramCounter++;
                    FirstObg_8 = go;
                    FirstPosition_8 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "8,1")
                {
                    SecondObg_8 = go;
                    SecondPosition_8 = go.transform.position;
                }

                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "9")
                {
                    RadarogramCounter++;
                    FirstObg_9 = go;
                    FirstPosition_9 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "9,1")
                {
                    SecondObg_9 = go;
                    SecondPosition_9 = go.transform.position;
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "10")
                {
                    RadarogramCounter++;
                    FirstObg_10 = go;
                    FirstPosition_10 = go.transform.position;
                    //go.transform.Find("Canvas").gameObject.SetActive(true);
                }
                else if(go.transform.Find("Number").GetComponent<TextMesh>().text == "10,1")
                {
                    SecondObg_10 = go;
                    SecondPosition_10 = go.transform.position;
                }

            }
            else if (go.name == "BasePlane(Clone)")
            {
                FindPlane = go;
            }
        }
    }

    // установка заготовок под радарограммы, которые потом можно текстурировать
    private void InstallBlank()
    {
        if(FirstObg_1 != null)
        {
            BlanksSet = true;
            Vector3 DirectionVector_1 = SecondPosition_1 - FirstPosition_1;
            normalizedDirection_1 = DirectionVector_1.normalized;
            SecondPosition_1 = FirstPosition_1 + (normalizedDirection_1 * 5/* * myJsonRadarogramData.images.jpg0[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_1.transform.position = SecondPosition_1; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_1 = Vector3.Lerp(FirstPosition_1, SecondPosition_1, 0.5f);
            RG_1 = Instantiate(radarogramPrefab, MyCenter_1, Quaternion.identity, FindPlane.transform);
            RG_1.transform.LookAt(SecondPosition_1);
            RG_1.transform.Rotate(0,-90,0);
            RG_1.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_2 != null)
        {
            Vector3 DirectionVector_2 = SecondPosition_2 - FirstPosition_2;
            normalizedDirection_2 = DirectionVector_2.normalized;
            SecondPosition_2 = FirstPosition_2 + (normalizedDirection_2 * 5/* * myJsonRadarogramData.images.jpg1[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_2.transform.position = SecondPosition_2; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_2 = Vector3.Lerp(FirstPosition_2, SecondPosition_2, 0.5f);
            RG_2 = Instantiate(radarogramPrefab, MyCenter_2, Quaternion.identity, FindPlane.transform);
            RG_2.transform.LookAt(SecondPosition_2);
            RG_2.transform.Rotate(0,-90,0);
            RG_2.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_3 != null)
        {
            Vector3 DirectionVector_3 = SecondPosition_3 - FirstPosition_3;
            normalizedDirection_3 = DirectionVector_3.normalized;
            SecondPosition_3 = FirstPosition_3 + (normalizedDirection_3 * 5/* * myJsonRadarogramData.images.jpg2[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_3.transform.position = SecondPosition_3; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_3 = Vector3.Lerp(FirstPosition_3, SecondPosition_3, 0.5f);
            RG_3 = Instantiate(radarogramPrefab, MyCenter_3, Quaternion.identity, FindPlane.transform);
            RG_3.transform.LookAt(SecondPosition_3);
            RG_3.transform.Rotate(0,-90,0);
            RG_3.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_4 != null)
        {
            Vector3 DirectionVector_4 = SecondPosition_4 - FirstPosition_4;
            normalizedDirection_4 = DirectionVector_4.normalized;
            SecondPosition_4 = FirstPosition_4 + (normalizedDirection_4 * 5/* * myJsonRadarogramData.images.jpg3[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_4.transform.position = SecondPosition_4; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_4 = Vector3.Lerp(FirstPosition_4, SecondPosition_4, 0.5f);
            RG_4 = Instantiate(radarogramPrefab, MyCenter_4, Quaternion.identity, FindPlane.transform);
            RG_4.transform.LookAt(SecondPosition_4);
            RG_4.transform.Rotate(0,-90,0);
            RG_4.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_5 != null)
        {
            Vector3 DirectionVector_5 = SecondPosition_5 - FirstPosition_5;
            normalizedDirection_5 = DirectionVector_5.normalized;
            SecondPosition_5 = FirstPosition_5 + (normalizedDirection_5 * 5/* * myJsonRadarogramData.images.jpg0[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_5.transform.position = SecondPosition_5; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_5 = Vector3.Lerp(FirstPosition_5, SecondPosition_5, 0.5f);
            RG_5 = Instantiate(radarogramPrefab, MyCenter_5, Quaternion.identity, FindPlane.transform);
            RG_5.transform.LookAt(SecondPosition_5);
            RG_5.transform.Rotate(0,-90,0);
            RG_5.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_6 != null)
        {
            Vector3 DirectionVector_6 = SecondPosition_6 - FirstPosition_6;
            normalizedDirection_6 = DirectionVector_6.normalized;
            SecondPosition_6 = FirstPosition_6 + (normalizedDirection_6 * 5/* * myJsonRadarogramData.images.jpg0[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_6.transform.position = SecondPosition_6; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_6 = Vector3.Lerp(FirstPosition_6, SecondPosition_6, 0.5f);
            RG_6 = Instantiate(radarogramPrefab, MyCenter_6, Quaternion.identity, FindPlane.transform);
            RG_6.transform.LookAt(SecondPosition_6);
            RG_6.transform.Rotate(0,-90,0);
            RG_6.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_7 != null)
        {
            Vector3 DirectionVector_7 = SecondPosition_7 - FirstPosition_7;
            normalizedDirection_7 = DirectionVector_7.normalized;
            SecondPosition_7 = FirstPosition_7 + (normalizedDirection_7 * 5/* * myJsonRadarogramData.images.jpg0[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_7.transform.position = SecondPosition_7; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_7 = Vector3.Lerp(FirstPosition_7, SecondPosition_7, 0.5f);
            RG_7 = Instantiate(radarogramPrefab, MyCenter_7, Quaternion.identity, FindPlane.transform);
            RG_7.transform.LookAt(SecondPosition_7);
            RG_7.transform.Rotate(0,-90,0);
            RG_7.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_8 != null)
        {
            Vector3 DirectionVector_8 = SecondPosition_8 - FirstPosition_8;
            normalizedDirection_8 = DirectionVector_8.normalized;
            SecondPosition_8 = FirstPosition_8 + (normalizedDirection_8 * 5/* * myJsonRadarogramData.images.jpg0[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_8.transform.position = SecondPosition_8; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_8 = Vector3.Lerp(FirstPosition_8, SecondPosition_8, 0.5f);
            RG_8 = Instantiate(radarogramPrefab, MyCenter_8, Quaternion.identity, FindPlane.transform);
            RG_8.transform.LookAt(SecondPosition_8);
            RG_8.transform.Rotate(0,-90,0);
            RG_8.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_9 != null)
        {
            Vector3 DirectionVector_9 = SecondPosition_9 - FirstPosition_9;
            normalizedDirection_9 = DirectionVector_9.normalized;
            SecondPosition_9 = FirstPosition_9 + (normalizedDirection_9 * 5/* * myJsonRadarogramData.images.jpg0[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_9.transform.position = SecondPosition_9; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_9 = Vector3.Lerp(FirstPosition_9, SecondPosition_9, 0.5f);
            RG_9 = Instantiate(radarogramPrefab, MyCenter_9, Quaternion.identity, FindPlane.transform);
            RG_9.transform.LookAt(SecondPosition_9);
            RG_9.transform.Rotate(0,-90,0);
            RG_9.transform.position += new Vector3(0, 1.5f, 0);
        }

        if(FirstObg_10 != null)
        {
            Vector3 DirectionVector_10 = SecondPosition_10 - FirstPosition_10;
            normalizedDirection_10 = DirectionVector_10.normalized;
            SecondPosition_10 = FirstPosition_10 + (normalizedDirection_10 * 5/* * myJsonRadarogramData.images.jpg0[0]/100*/); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
            SecondObg_10.transform.position = SecondPosition_10; // отодвигаем второй белый флаг, чтобы влезла радарограмма
            MyCenter_10 = Vector3.Lerp(FirstPosition_10, SecondPosition_10, 0.5f);
            RG_10 = Instantiate(radarogramPrefab, MyCenter_10, Quaternion.identity, FindPlane.transform);
            RG_10.transform.LookAt(SecondPosition_10);
            RG_10.transform.Rotate(0,-90,0);
            RG_10.transform.position += new Vector3(0, 1.5f, 0);
        }
    }

    private void InstallRadarogram()
    {
        if(File.Exists(Application.persistentDataPath + "/extractZIP/data.json"))
        {   
            myJsonRadarogramData = LoadJsonRadarogram(Application.persistentDataPath + "/extractZIP/data.json"); // Читаю data.json

            if(RG_1 != null)
            {
                SecondPosition_1 = FirstPosition_1 + (normalizedDirection_1 * myJsonRadarogramData.images.jpg0[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                // Debug.Log("ПЕРЕДВИНУЛ на SecondPosition_1 :" + SecondPosition_1.ToString());
                SecondObg_1.transform.position = SecondPosition_1; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_1 = Vector3.Lerp(FirstPosition_1, SecondPosition_1, 0.5f);
                RG_1.transform.position = MyCenter_1;
                RG_1.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/0.png");
                RG_1.transform.position += new Vector3(0, 1.5f, 0);
                RG_1.name = "radarogramPrefab_1";
                RG_1.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg0[0]/100, (float)myJsonRadarogramData.images.jpg0[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_2 != null)
            {
                SecondPosition_2 = FirstPosition_2 + (normalizedDirection_2 * myJsonRadarogramData.images.jpg1[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_2.transform.position = SecondPosition_2; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_2 = Vector3.Lerp(FirstPosition_2, SecondPosition_2, 0.5f);
                RG_2.transform.position = MyCenter_2;
                RG_2.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/1.png");
                RG_2.transform.position += new Vector3(0, 1.5f, 0);
                RG_2.name = "radarogramPrefab_2";
                RG_2.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg1[0]/100, (float)myJsonRadarogramData.images.jpg1[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_3 != null)
            {
                SecondPosition_3 = FirstPosition_3 + (normalizedDirection_3 * myJsonRadarogramData.images.jpg2[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_3.transform.position = SecondPosition_3; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_3 = Vector3.Lerp(FirstPosition_3, SecondPosition_3, 0.5f);
                RG_3.transform.position = MyCenter_3;
                RG_3.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/2.png");
                RG_3.transform.position += new Vector3(0, 1.5f, 0);
                RG_3.name = "radarogramPrefab_3";
                RG_3.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg2[0]/100, (float)myJsonRadarogramData.images.jpg2[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_4 != null)
            {
                SecondPosition_4 = FirstPosition_4 + (normalizedDirection_4 * myJsonRadarogramData.images.jpg3[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_4.transform.position = SecondPosition_4; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_4 = Vector3.Lerp(FirstPosition_4, SecondPosition_4, 0.5f);
                RG_4.transform.position = MyCenter_4;
                RG_4.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/3.png");
                RG_4.transform.position += new Vector3(0, 1.5f, 0);
                RG_4.name = "radarogramPrefab_4";
                RG_4.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg3[0]/100, (float)myJsonRadarogramData.images.jpg3[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_5 != null)
            {
                SecondPosition_5 = FirstPosition_5 + (normalizedDirection_5 * myJsonRadarogramData.images.jpg4[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_5.transform.position = SecondPosition_5; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_5 = Vector3.Lerp(FirstPosition_5, SecondPosition_5, 0.5f);
                RG_5.transform.position = MyCenter_5;
                RG_5.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/4.png");
                RG_5.transform.position += new Vector3(0, 1.5f, 0);
                RG_5.name = "radarogramPrefab_5";
                RG_5.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg4[0]/100, (float)myJsonRadarogramData.images.jpg4[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_6 != null)
            {
                SecondPosition_6 = FirstPosition_6 + (normalizedDirection_6 * myJsonRadarogramData.images.jpg5[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_6.transform.position = SecondPosition_6; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_6 = Vector3.Lerp(FirstPosition_6, SecondPosition_6, 0.5f);
                RG_6.transform.position = MyCenter_6;
                RG_6.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/5.png");
                RG_6.transform.position += new Vector3(0, 1.5f, 0);
                RG_6.name = "radarogramPrefab_6";
                RG_6.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg5[0]/100, (float)myJsonRadarogramData.images.jpg5[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_7 != null)
            {
                SecondPosition_7 = FirstPosition_7 + (normalizedDirection_7 * myJsonRadarogramData.images.jpg6[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_7.transform.position = SecondPosition_7; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_7 = Vector3.Lerp(FirstPosition_7, SecondPosition_7, 0.5f);
                RG_7.transform.position = MyCenter_7;
                RG_7.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/6.png");
                RG_7.transform.position += new Vector3(0, 1.5f, 0);
                RG_7.name = "radarogramPrefab_7";
                RG_7.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg6[0]/100, (float)myJsonRadarogramData.images.jpg6[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_8 != null)
            {
                SecondPosition_8 = FirstPosition_8 + (normalizedDirection_8 * myJsonRadarogramData.images.jpg7[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_8.transform.position = SecondPosition_8; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_8 = Vector3.Lerp(FirstPosition_8, SecondPosition_8, 0.5f);
                RG_8.transform.position = MyCenter_8;
                RG_8.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/7.png");
                RG_8.transform.position += new Vector3(0, 1.5f, 0);
                RG_8.name = "radarogramPrefab_8";
                RG_8.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg7[0]/100, (float)myJsonRadarogramData.images.jpg7[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_9 != null)
            {
                SecondPosition_9 = FirstPosition_9 + (normalizedDirection_9 * myJsonRadarogramData.images.jpg8[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_9.transform.position = SecondPosition_9; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_9 = Vector3.Lerp(FirstPosition_9, SecondPosition_9, 0.5f);
                RG_9.transform.position = MyCenter_9;
                RG_9.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/8.png");
                RG_9.transform.position += new Vector3(0, 1.5f, 0);
                RG_9.name = "radarogramPrefab_9";
                RG_9.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg8[0]/100, (float)myJsonRadarogramData.images.jpg8[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
            if(RG_10 != null)
            {
                SecondPosition_10 = FirstPosition_10 + (normalizedDirection_10 * myJsonRadarogramData.images.jpg9[0]/100); // умножается на количество метров (длина радарограммы). Делю на 100, потому что сейчас 100 пикселей на метр
                SecondObg_10.transform.position = SecondPosition_10; // отодвигаем второй белый флаг, чтобы влезла радарограмма
                MyCenter_10 = Vector3.Lerp(FirstPosition_10, SecondPosition_10, 0.5f);
                RG_10.transform.position = MyCenter_10;
                RG_10.GetComponent<SpriteRenderer>().sprite = LoadSprite(Application.persistentDataPath + "/extractZIP/RadarogramTexture/9.png");
                RG_10.transform.position += new Vector3(0, 1.5f, 0);
                RG_10.name = "radarogramPrefab_10";
                RG_10.GetComponent<BoxCollider>().size = new Vector3((float)myJsonRadarogramData.images.jpg9[0]/100, (float)myJsonRadarogramData.images.jpg9[1]/100, 0.01f); // растягиваем коллайдер по размеру изображения
            }
        }
    }

    private Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (System.IO.File.Exists(path))
        {
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        return null;
    }

    private JsonRadarogramReader.CommonData LoadJsonRadarogram(string path)
    {
        if(File.Exists(path))
        {
            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                JsonRadarogramReader.CommonData loadRadData = JsonUtility.FromJson<JsonRadarogramReader.CommonData>(reader.ReadToEnd());

                return loadRadData;
            }
        }
        return null;
    }

    public void InstallRadarogramButton()
    {
        // string startPath = @".\start";
        // string startPath = Application.persistentDataPath + "/start";
        // string zipPath = @".\result.zip";
        //string zipPath = Application.persistentDataPath + "/result.zip"; // сюда нужно передать путь до архива из файлового менеджера
        // string extractPath = @".\extract";
        //string extractPath = Application.persistentDataPath + "/extractZIP";

        // ZipFile.CreateFromDirectory(startPath, zipPath);

        //ZipFile.ExtractToDirectory(zipPath, extractPath);

        if (Directory.Exists(extractPath)){ // удаляю прошлую папку с распакованным архивом, потому что новый архив не распакуется и выдаст ошибку если файлы уже есть
            Directory.Delete(extractPath, true);
        }

        SimpleFileBrowser.FileBrowser.ShowLoadDialog( ( paths ) => { zipPath = ( paths[0] ); InstallRad(zipPath); }, () => { zipPath = ( "Canceled" ); }, SimpleFileBrowser.FileBrowser.PickMode.FilesAndFolders, false, "/storage/emulated/0/Download", null, "Выбор ZIP архива с радарограммами", "Выбрать");
    }
    private void InstallRad(string zipPath)
    {
        StartCoroutine("InstallRadarogramButton_coroutine");
    }
    IEnumerator InstallRadarogramButton_coroutine()
    {
        ZipFile.ExtractToDirectory(zipPath, extractPath);
        yield return new WaitForSeconds(0.2f);
        // последовательность установки радарограммы из файла - ищем флаги начала/конца, устанавливаем заготовки, затем текстурируем радарограммами из файла
        if (BlanksSet == false){ // чтобы при выборе другого архива с радарограммами, менялись картинки на старых заготовках, а не распознавались новые
            FindWhiteFlag();
            yield return new WaitForSeconds(0.1f);
            InstallBlank();
            yield return new WaitForSeconds(0.1f);
        }
        InstallRadarogram();
    }
}
