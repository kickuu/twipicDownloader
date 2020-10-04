using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CoreTweet;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.SceneManagement;
using TMPro;
//フォルダ選択のために使用
using System.Windows.Forms;

public class DownloadManager : MonoBehaviour
{
    [SerializeField] Text status;
    [SerializeField] GameObject downloadStartButton;
    [SerializeField] GameObject downloadStopButton;
    [SerializeField] GameObject folderSelectButton;
    [SerializeField] Transform canvas;
    [SerializeField] Text discliptionText;
    [SerializeField] TextMeshProUGUI saveDirectlyText;
    [SerializeField] GameObject notDirectlyPopup;
    //popup prefab 用の変数
    GameObject popupClone;

    long? maxID = null;
    // long? sinceID = null;
    bool downloadFlag = false;
    bool downloadStopFlag = false;
    //デバッグ用 API 使用回数
    int downloadCount = 0;
    string saveFolderPath = "";

    void Start()
    {
        DefaultMenu();
        // status.GetComponent<Text>();
        // discliptionText.GetComponent<Text>();
        // downloadStartButton.SetActive(false);
        // downloadStopButton.SetActive(false);
        // discliptionText.text = "";
    }

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
        discliptionText.text = "";
        discliptionText.text = "";
    }

    //DownloadButton クリックで呼び出し
    public void DownloadButtonClick()
    {
        StartCoroutine(StartDownload());
    }
    //DownloadStopButton クリックで呼び出し
    public void downloadStopButtonClick()
    {
        downloadStopFlag = true;
        DefaultMenu();
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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
    // ダウンロード処理
    public IEnumerator StartDownload()
    {
        downloadFlag = true;
        downloadStopFlag = false;
        downloadStartButton.SetActive(false);
        folderSelectButton.SetActive(false);
        downloadStopButton.SetActive(true);
        status.text = "ダウンロード中...";

        //ネットワーク状態を確認。つながったら次の処理へ
        //Application は windows.Forms との曖昧回避
        while (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable)
        {
            status.text = "ネットワークに接続されていません";
            yield return new WaitForSeconds(5f);
        }

        while (true)
        {
            if (MainScript.token.Favorites.List().RateLimit.Remaining > 50)
            {
                //Favolites.List のアクセス上限200回まで
                //あるツイートより古いツイートを探したいときは max_id
                foreach (var myFav in MainScript.token.Favorites.List(count => 200, include_entities => true,
                                                                      tweet_mode => TweetMode.Extended, max_id => maxID))
                {
                    // ツイートにメディアが含まれているか確認
                    if (myFav.Entities.Media != null)
                    {
                        int mediaLength = myFav.ExtendedEntities.Media.Length;

                        for (int i = 0; i <= mediaLength - 1; i++)
                        {
                            //downloadStopのフラグが立ったらコルーチン抜ける
                            if (downloadStopFlag == true)
                            {
                                yield break;
                            }

                            StringBuilder fileName = new StringBuilder();
                            string url = myFav.ExtendedEntities.Media[i].MediaUrlHttps + ":orig";

                            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                            {
                                yield return webRequest.SendWebRequest();

                                long tweetID = myFav.Id;

                                fileName.Append(saveFolderPath);
                                fileName.Append("/");
                                fileName.Append(tweetID);
                                fileName.Append("_");
                                fileName.Append(i + 1);
                                fileName.Append(".png");

                                maxID = myFav.Id;

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
                        }
                    }
                    downloadCount++;
                }
                int raterimit = MainScript.token.Favorites.List().RateLimit.Remaining;
                Debug.Log(raterimit);
                Debug.Log(downloadCount);
            }
            else
            {
                downloadFlag = false;
                StartCoroutine(RateLimitWait());
                break;
            }
        }
        //正常にDLが終了したときの処理
        if (downloadFlag == true)
        {
            downloadStartButton.SetActive(true);
            status.text = "ダウンロード完了";
        }
    }

    //API利用上限に到達したら900秒待機してシーン再読み込み
    IEnumerator RateLimitWait()
    {
        for (int i = 900; i >= 0; i--)
        {
            downloadStopButton.SetActive(false);
            // status.text = "ダウンロード停止";
            status.text = "ダウンロード回数が上限に達しました。" + i + " 秒後に制限解除";
            yield return new WaitForSeconds(1f);
        }
        DefaultMenu();
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
