using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoreTweet;
using UnityEngine.Networking;
using System.Text;
//フォルダ選択のために使用
using System.Windows.Forms;

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

    long? maxID = long.MaxValue;
    // long? sinceID = null;
    bool downloadFlag = false;
    bool downloadStopFlag = false;
    //デバッグ用 API 使用回数
    int downloadCount = 0;
    string saveFolderPath = "";
    //fav を格納するリスト
    List<Status> favIdList = new List<Status>();

    void Start()
    {
        DefaultMenu();
    }

    /// <summary>
    /// デフォルトメニュー画面を表示
    /// </summary>
    public void DefaultMenu()
    {
        status.text = "ボタンクリックでダウンロード開始";
        if (saveFolderPath == "")
        {
            downloadStartButton.SetActive(false);
        }
        else
        {
            downloadStartButton.SetActive(true);
        }
        downloadStopButton.SetActive(false);
        folderSelectButton.SetActive(true);
        downloadFlag = false;
    }

    /// <summary>
    /// ダウンロード終了、または失敗画面を表示する
    /// </summary>
    void StopDownload()
    {
        downloadStopButton.SetActive(false);
        downloadStartButton.SetActive(true);
        folderSelectButton.SetActive(true);
        status.text = "終了。" + downloadCount + "枚ダウンロード";
    }

    /// <summary>
    /// DdownloadButton 押されたら呼ばれる
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
    /// ダイアログボックスを操作するメソッド
    /// </summary>
    public void DialogBox()
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
                saveFolderPath = selectFolder.SelectedPath.Replace("\\", "/");
                downloadStartButton.SetActive(true);
                saveDirectlyText.text = "保存先：" + saveFolderPath;
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
            //Favolites.List のアクセス上限200回まで
            //あるツイートより古いツイートを探したいときは max_id
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
        downloadFlag = true;
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
                        //downloadStopのフラグが立ったらコルーチン抜ける
                        if (downloadStopFlag == true)
                        {
                            StopDownload();
                            yield break;
                        }

                        string url = favMediaList.ExtendedEntities.Media[j].MediaUrlHttps + ":orig";

                        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                        {
                            yield return webRequest.SendWebRequest();

                            StringBuilder fileName = new StringBuilder();
                            fileName.Append(saveFolderPath);
                            fileName.Append("/");
                            fileName.Append(favMediaList.Id);
                            fileName.Append("_");
                            fileName.Append(j + 1);
                            fileName.Append(".png");

                            maxID = favMediaList.Id;

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
                                    fileName.Clear();
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(ex.Message);
                                    status.text = "ダウンロードエラー";
                                }
                            }
                        }
                        downloadCount++;
                    }
                }
                Debug.Log(downloadCount + "枚");
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
            status.text = "ダウンロード回数が上限に達しました。" + i + " 秒で制限解除";
            yield return new WaitForSeconds(1f);
        }
        DefaultMenu();
    }
}
