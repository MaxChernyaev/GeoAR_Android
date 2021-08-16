using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WebSendFile : MonoBehaviour/*,IPointerDownHandler,IPointerUpHandler*/
{
    [SerializeField] private GameObject checkWifiImage;
    [SerializeField] private GameObject WarningText;
    //private bool foolCheck = false;
    private string PathToJson;

    public void buttonUpload()
    {
        if (DataHolder.TextFID != "")
        {
            //foolCheck = true;
            WarningText.GetComponent<Text>().text = "";

            SimpleFileBrowser.FileBrowser.ShowLoadDialog( ( paths ) => { PathToJson = ( paths[0] ); StartUploadJSON(); }, () => { PathToJson = ( "Canceled" ); }, SimpleFileBrowser.FileBrowser.PickMode.FilesAndFolders, false, Application.persistentDataPath, null, "Выбор JSON файла для отправки на сервер", "Выбрать");

        }
        else
        {
            //foolCheck = false;
            WarningText.GetComponent<Text>().text = "введи номер";
        }
    }

    private void StartUploadJSON()
    {
        StartCoroutine(Upload());
    }

    IEnumerator Upload()
    {
        //foolCheck = true;
        StartCoroutine(ActiveCheckWifi());
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("fid", DataHolder.TextFID));

        byte[] filebyte = Encoding.UTF8.GetBytes(File.ReadAllText(PathToJson)); // тут теперь путь до файла, полученный из файлового менеджера
        formData.Add(new MultipartFormFileSection("data", filebyte, "scene.json", "application/octet-stream"));

        UnityWebRequest www = UnityWebRequest.Post("http://192.168.2.1/upload_file", formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }

    // public void OnPointerDown(PointerEventData eventData)

    // {
    //     //Debug.Log("DOWN");
    // }

    // public void OnPointerUp(PointerEventData eventData)

    // {
    //     // //Debug.Log("UP");
    //     // if(foolCheck)
    //     // {
    //     //     StartCoroutine(ActiveCheckWifi());
    //     // }
    // }

    IEnumerator ActiveCheckWifi()
    {
        checkWifiImage.SetActive(true);
        yield return new WaitForSeconds(3f);
        checkWifiImage.SetActive(false);
    }
}


// public class WebSendFile : MonoBehaviour {
//     void Start() {
//         StartCoroutine(Upload());
//     }

//     IEnumerator Upload() {
//         WWWForm form = new WWWForm();
//         //form.AddField("myField", "myData");
//         form.AddBinaryData("myTestFile.zip",post.bytes,"myFile.zip","application/zip");

//         UnityWebRequest www = UnityWebRequest.Post("http://192.168.2.1:8080/upload_file", form);
//         yield return www.SendWebRequest();

//         if (www.result != UnityWebRequest.Result.Success) {
//             Debug.Log(www.error);
//         }
//         else {
//             Debug.Log("Form upload complete!");
//         }
//     }
// }