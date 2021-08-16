using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebSendJSON : MonoBehaviour
{
    public void WebUploadJSON()
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("fid = 666"));
        formData.Add(new MultipartFormFileSection("max file data", Application.persistentDataPath + "maxfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post("http://192.168.2.1/upload_file", formData);
        //yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Ошибка");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }
}
