using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// データのセーブ・ロード処理
/// </summary>
public class SaveManager : MonoBehaviour
{
    [SerializeField] Text saveDirectlyText;
    //セーブデータの保存先
    string saveDatePath;
    //ロードした path を格納する変数
    [HideInInspector]
    public string loadPath;

    private void Awake()
    {
        saveDatePath = Application.persistentDataPath + "/saveDate.json";
    }

    /// <summary>
    /// セーブデータ作成処理
    /// json 形式にシリアライズして保存
    /// </summary>
    public void CreateSaveDate(string downloadFolderPath)
    {
        SaveDate saveDate = new SaveDate();
        saveDate.downloadPath = downloadFolderPath;

        var json = JsonUtility.ToJson(saveDate, false);

        Debug.Log("save");
        File.WriteAllText(saveDatePath, json);

        saveDirectlyText.text = "保存先：" + downloadFolderPath;
    }

    /// <summary>
    /// セーブデータロード処理
    /// </summary>
    public string LoadSaveDate()
    {
        //セーブデータが存在していたら
        if (File.Exists(saveDatePath))
        {
            var json = File.ReadAllText(saveDatePath);
            var obj = JsonUtility.FromJson<SaveDate>(json);
            loadPath = obj.downloadPath;
            Debug.Log("load");
            saveDirectlyText.text = "保存先：" + loadPath;
            return loadPath;
        }
        else
        {
            Debug.Log("not save data");
            loadPath = "";
            return loadPath;
        }
    }
}
