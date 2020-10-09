using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
//フォルダ選択のために使用
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
//twitterAPIライブラリ
using CoreTweet;

/// <summary>
/// ダウンロード処理全般のクラス
/// </summary>
public class DownloadManager : MonoBehaviour
{
    [SerializeField] Text status;
    [SerializeField] GameObject downloadStartButton;
    [SerializeField] GameObject downloadStopButton;
    [SerializeField] GameObject folderSelectButton;
    [SerializeField] Transform canvas;
    [SerializeField] Text saveDirectlyText;
    [SerializeField] GameObject notDirectlyPopup;
    //popup prefab 用の変数
    GameObject popupClone;

    long maxID = long.MaxValue;
    bool downloadingFlag = false;
    bool downloadStopFlag = false;
    int downloadCount = 0;
    public string downloadFolderPath;
    List<Status> favIdList = new List<Status>();
    StringBuilder fileName = new StringBuilder();

    void Start()
    {
        //セーブデータのロード
        SaveManager saveManager = GetComponent<SaveManager>();
        saveManager.LoadSaveDate();
        downloadFolderPath = saveManager.LoadSaveDate();
        DefaultMenu();
    }

    /// <summary>
    /// デフォルトメニュー画面を表示
    /// </summary>
    public void DefaultMenu()
    {

        status.text = "ボタンクリックでダウンロード開始";

        if (downloadFolderPath == "")
        {
            downloadStartButton.SetActive(false);
        }
        else
        {
            downloadStartButton.SetActive(true);
        }
        Debug.Log("パスは" + downloadFolderPath + "です");
        downloadStopButton.SetActive(false);
        folderSelectButton.SetActive(true);
        downloadingFlag = false;
    }

    /// <summary>
    /// ダウンロード終了、または失敗画面を表示する
    /// </summary>
    void StopDownload()
    {
        downloadStopButton.SetActive(false);
        downloadStartButton.SetActive(true);
        folderSelectButton.SetActive(true);
        status.text = "終了 " + downloadCount + "枚ダウンロード";
    }

    /// <summary>
    /// DownloadButton 押されたら呼ばれる
    /// </summary>
    public void DownloadButtonClick()
    {
        StartCoroutine(StartDownload());
    }

    /// <summary>
    /// DownloadStopButton が押されたら呼ばれる
    /// </summary>
    public void downloadStopButtonClick()
    {
        downloadStopFlag = true;
    }

    /// <summary>
    /// ダウンロードするフォルダを選ぶメソッド
    /// </summary>
    public void SerectDownloadFolder()
    {
        FolderBrowserDialog selectFolder = new System.Windows.Forms.FolderBrowserDialog();
        selectFolder.Description = "select save directly";
        if (selectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            if (selectFolder.SelectedPath == "")
            {
                popupClone = Instantiate(notDirectlyPopup) as GameObject;
                popupClone.transform.SetParent(canvas.transform, false);
            }
            else
            {
                //path の区切り文字を Unity 用に変換
                downloadFolderPath = selectFolder.SelectedPath.Replace("\\", "/");

                //downloadFolderPath のセーブ処理
                SaveManager saveManager = GetComponent<SaveManager>();
                saveManager.CreateSaveDate(downloadFolderPath);

                downloadStartButton.SetActive(true);
                if (popupClone)
                {
                    Destroy(popupClone);
                }
            }
        }
    }
    /// <summary>
    /// いいねしたツイートの中から画像つきのツイートを取得するメソッド
    /// </summary>
    void GetFavList()
    {
        favIdList.Clear();
        try
        {
            //Favolites.List のアクセス上限200回
            //max_id を指定すると max_id よりも古いツイートのみを取得する
            foreach (var favList in MainScript.token.Favorites.List(count => 200, include_entities => true,
                                                                    tweet_mode => TweetMode.Extended, max_id => maxID - 1))
            {
                if (favList.Entities.Media != null)
                {
                    favIdList.Add(favList);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            StartCoroutine(RateLimitWait());
        }
    }

    /// <summary>
    /// メインのダウンロード処理
    /// </summary>
    IEnumerator StartDownload()
    {
        downloadCount = 0;
        downloadingFlag = true;
        downloadStopFlag = false;
        downloadStartButton.SetActive(false);
        folderSelectButton.SetActive(false);
        downloadStopButton.SetActive(true);
        status.text = "ダウンロード中...";

        //Application は windows.Forms との曖昧回避
        while (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable)
        {
            status.text = "ネットワークに接続されていません";
            yield return new WaitForSeconds(5f);
        }

        while (true)
        {
            GetFavList();
            if (favIdList.Count != 0)
            {
                foreach (var favMediaList in favIdList)
                {
                    int mediaLength = favMediaList.ExtendedEntities.Media.Length;

                    for (int j = 0; j <= mediaLength - 1; j++)
                    {
                        if (downloadStopFlag == true)
                        {
                            StopDownload();
                            yield break;
                        }

                        string url = favMediaList.ExtendedEntities.Media[j].MediaUrlHttps + ":orig";

                        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                        {
                            yield return webRequest.SendWebRequest();

                            fileName.Append(downloadFolderPath);
                            fileName.Append("/");
                            fileName.Append(favMediaList.User.ScreenName);
                            fileName.Append("-");
                            fileName.Append(favMediaList.Id);
                            fileName.Append("_");
                            fileName.Append(j + 1);
                            fileName.Append(".png");

                            if (webRequest.isNetworkError || webRequest.isHttpError)
                            {
                                Debug.Log(webRequest.error);
                            }
                            else
                            {
                                try
                                {
                                    string fileNameString = fileName.ToString();
                                    File.WriteAllBytes(@fileNameString, webRequest.downloadHandler.data);
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(ex.Message);
                                    status.text = "ダウンロードエラー";
                                }
                            }
                            fileName.Clear();
                            maxID = favMediaList.Id;
                        }
                        downloadCount++;
                    }
                }
            }
            else if (favIdList.Count == 0)
            {
                break;
            }
        }
        StopDownload();
    }

    /// <summary>
    /// API利用回数が上限に達したときの処理
    /// </summary>
    IEnumerator RateLimitWait()
    {
        for (int i = 900; i >= 0; i--)
        {
            downloadStartButton.SetActive(false);
            downloadStopButton.SetActive(false);
            folderSelectButton.SetActive(false);
            status.text = "ダウンロード回数が上限に達しました。\n" + i + " 秒で制限解除";
            yield return new WaitForSeconds(1f);
        }
        DefaultMenu();
    }
}
