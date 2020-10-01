using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CoreTweet;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.SceneManagement;

public class DownloadManager : MonoBehaviour
{
    [SerializeField] public Text status;
    [SerializeField] public GameObject downloadButton;
    [SerializeField] public GameObject downloadStopButton;
    [SerializeField] public Text limitText;

    long? maxID = null;
    // long? sinceID = null;
    bool downloadFlag = false;
    bool downloadStopFlag = false;
    int downloadCount = 0;

    void Start()
    {
        status.GetComponent<Text>();
        limitText.GetComponent<Text>();
        downloadStopButton.SetActive(false);
        limitText.text = "";
    }

    //DownloadButtonがクリックされたら発火
    public void DownloadButtonClick()
    {
        StartCoroutine(StartDownload());
    }
    //DownloadStopButtonが押されたら中止フラグを立ててシーン再読み込み
    public void downloadStopButtonClick()
    {
        downloadStopFlag = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //ダウンロード処理
    public IEnumerator StartDownload()
    {
        downloadFlag = true;
        downloadButton.SetActive(false);
        downloadStopButton.SetActive(true);
        status.text = "ダウンロード中...";

        //ネットワーク状態を確認。つながったら次の処理へ
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            limitText.text = "ネットワークに接続されていません";
            yield return new WaitForSeconds(5f);
        }

        while (true)
        {
            if (MainScript.token.Favorites.List().RateLimit.Remaining > 73)
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

                                string path = "D:/twipicImage/";
                                fileName.Append(path);
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
                                        limitText.text = "ダウンロードエラー";
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
        if (downloadFlag == true)
        {
            downloadButton.SetActive(true);
            status.text = "ダウンロード完了";
        }
    }

    //API利用上限に到達したら900秒待機してシーン再読み込み
    IEnumerator RateLimitWait()
    {
        for (int i = 10; i >= 0; i--)
        {
            downloadStopButton.SetActive(false);
            status.text = "ダウンロード停止";
            limitText.text = "ダウンロード回数が上限に達しました。 " + i + "秒後に制限解除";
            yield return new WaitForSeconds(1f);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
