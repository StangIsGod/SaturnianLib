using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;

public class LibVersionChecker : MonoBehaviour
{
    string Ver = "1.0a";
    void Start()
    {
        StartCoroutine(GetText());
    }
    IEnumerator GetText()
    {
        UnityWebRequest www = new UnityWebRequest("http://www.my-server.com");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // 結果をテキストで表示
            //Debug.Log(www.downloadHandler.text);
            if (www.downloadHandler.text != Ver)
            {
                bool a = EditorUtility.DisplayDialog("Saturnian Library", "Saturnian Libraryの更新が来ています！\n配布ページを開きますか？", "Yes", "No");
                if (a)
                {
                    Application.OpenURL("");
                }
            }

            // または、結果をバイナリデータで取得
            byte[] results = www.downloadHandler.data;
        }
    }
    void Update()
    {
        
    }
}
