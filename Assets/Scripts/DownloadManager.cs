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
    [SerializeField] public Text limitTimer;

    long? maxID = null;
    // long? sinceID = null;
    int downloadCount = 0;

    void Start()
    {
        status.GetComponent<Text>();
        limitTimer.GetComponent<Text>();
        limitTimer.text = "0";
        // RateLimitCheck();
    }

    //APIのレートリミット確認処理
    // void RateLimitCheck()
    // {
    //     if (MainScript.token.Favorites.List().RateLimit.Remaining < 1)
    //     {
    //         status.text = "TwitterAPI RateLimit! Wait 15 minute";
    //         downloadButton.SetActive(false);
    //     }

    //     if (MainScript.token.Favorites.List().RateLimit.Remaining == 200)
    //     {
    //         downloadButton.SetActive(true);
    //     }
    // }

    //DownloadButtonがクリックされたら発火
    public void DownloadButtonClick()
    {
        StartCoroutine(StartDownload());
    }

    public IEnumerator StartDownload()
    {
        downloadButton.SetActive(false);
        status.text = "Downloading picture...";

        //ネットワーク状態を確認。つながったら次の処理へ
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("ネットワークエラー");
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
                                        Debug.Log(ex.Message);
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
                Debug.Log("RateLimit !!");
                StartCoroutine(RateLimitWait());
                break;
            }
        }
        // downloadButton.SetActive(true);
        // status.text = "finish";
    }

    IEnumerator RateLimitWait()
    {
        downloadButton.SetActive(false);
        status.text = "RateLimit! Wait 15minutes";

        StartCoroutine(RateLimitTimer());
        yield return new WaitForSeconds(10f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator RateLimitTimer()
    {
        for (int i = 10; i >= 0; i--)
        {
            limitTimer.text = "制限解除まで" + i + "秒";
            yield return new WaitForSeconds(1f);
        }
    }
}
