using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Этот скрипт выводит инфу об объекте над его моделью. Для этого должен быть создан 3D-текст с именем "VisualName" в префабе объекта
public class FlagVisualName : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        //transform.Find("VisualName").GetComponent<TextMesh>().text = transform.name;
        //transform.Find("VisualName").GetComponent<TextMesh>().text = transform.tag;
    }
}
