using System;
using System.Runtime.InteropServices;
// using System.Diagnostics;
using CoreTweet;
using System.Net;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainScript : MonoBehaviour
{
    [SerializeField] public GameObject AuthorizeButton;
    [SerializeField] public InputField pincodeInputField;

    static string CK = "veB1GzM0CgTICCDqW09N583Fp";
    static string CS = "pFLfb6lgLFQ8hHEbSPl3rB4Y48oh6QwkRvJZCA4ZyyZVq3xzmo";
    // static string AT = "3677900594-aT65SPtGuWswaTtAXJKVnKMgAgpCNp4kI6oMr2y";
    // static string AS = "XmB4i36BY6Gyj7mSXUpQWGgiNDbZbK0i0cOVaOU90CkA1";

    OAuth.OAuthSession consumerKey;
    string pincode;
    // public static Tokens token;
    public static Tokens token {get; private set;}

    // static long sinceID = 0;
    // static long? maxID = null;
    // static int downloadCount = 0;
    // static int methodCount;
    // static string nl = Environment.NewLine;

    void Start()
    {
        pincodeInputField.GetComponent<InputField>();
        consumerKey = OAuth.Authorize(CK, CS);
    }

    public void AuthorizeButtonClick()
    {
        string uri = consumerKey.AuthorizeUri.ToString();
        Application.OpenURL(uri);
    }

    public void GetInputPincode()
    {
        pincode = pincodeInputField.text;
        token = consumerKey.GetTokens(pincode);
        if (token != null)
        {
            SceneManager.LoadScene("MenuScene");
        }
    }

    // static void Main(string[] args)
    // {
    // osを判定して規定のブラウザで開く
    // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    // {
    //     uri = uri.Replace("&", "^&");
    //     Process.Start(new ProcessStartInfo("cmd", $"/c start{uri}") { CreateNoWindow = true });
    // }
    // else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    // {
    //     Process.Start("xdg-open", uri);
    // }
    // else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    // {
    //     Process.Start("open", uri);
    // }

    //         Console.WriteLine("Successfully authenticated!" + " What do you do?" + nl);

    //         PleaseUserAction();

    //         void PleaseUserAction()
    //         {
    //             Console.WriteLine("tw: tweet" + nl + "tl: timeline" + nl + "f: favolist" + nl + "e: exit" + nl + "c: createFile" + nl);
    //             Console.Write("-> ");
    //             string userAction = Console.ReadLine();
    //             if (userAction == "tw")
    //             {
    //                 Tweet();
    //             }
    //             else if (userAction == "tl")
    //             {
    //                 TimeLine();
    //             }
    //             else if (userAction == "f")
    //             {
    //                 FavoList();
    //             }
    //             else if (userAction == "e")
    //             {
    //                 return;
    //             }
    //             else if (userAction == "c")
    //             {
    //                 var rateStatus = token.Application.RateLimitStatus();
    //                 Console.WriteLine(rateStatus.Json);
    //                 // Console.WriteLine(token.Favorites.List().RateLimit.Remaining);
    //                 // string tmpPath = "/mnt/c/Users/prprp/Desktop/twitter/tmp.txt";
    //                 // FileStream fs = File.Create(tmpPath);
    //                 // using (StreamWriter fs = new StreamWriter(tmpPath, true))
    //                 // {
    //                 // string text = Console.ReadLine();
    //                 // fs.Write(text);
    //                 // }
    //                 ReturnAction();
    //             }
    //             else
    //             {
    //                 Console.WriteLine("Error! Prease Action select!" + nl);
    //                 ReturnAction();
    //             }
    //         }

    //         void ReturnAction()
    //         {
    //             PleaseUserAction();
    //         }

    //         void Tweet()
    //         {
    //             Console.WriteLine("What do you tweet about?");
    //             Console.Write("-> ");
    //             string tweet = Console.ReadLine();
    //             token.Statuses.Update(status => tweet);
    //             Console.WriteLine("Successful tweet!");
    //             PleaseUserAction();
    //         }

    //         void TimeLine()
    //         {
    //             foreach (var status in token.Statuses.HomeTimeline(count => 10, tweet_mode => TweetMode.Extended))
    //             {
    //                 Console.WriteLine("---------------------------------------------------------------");
    //                 Console.WriteLine("@{0}: {1}", status.User.ScreenName, status.FullText);
    //             }
    //             PleaseUserAction();
    //         }

    //         void FavoList()
    //         {
    //             // WebClient client = new WebClient();
    //             // Console.WriteLine("Downloading Images..." + nl);

    //             if (token.Favorites.List().RateLimit.Remaining < 5)
    //             {
    //                 Console.WriteLine("API Ratelimit!!");
    //                 return;
    //             }

    //             foreach (var myFav in token.Favorites.List(count => 200, include_entities => true,
    //                                                        tweet_mode => TweetMode.Extended, max_id => maxID))
    //             {
    //                 if (myFav.Entities.Media != null)
    //                 {
    //                     // if (myFav.Id < sinceID)
    //                     // {
    //                     //     sinceID = myFav.Id;
    //                     // }
    //                     var mediaLength = myFav.ExtendedEntities.Media.Length;

    //                     // for (int i = 0; i <= mediaLength - 1; i++)
    //                     // {
    //                     //     var url = myFav.ExtendedEntities.Media[i].MediaUrlHttps + ":orig";
    //                     //     var tweetID = myFav.Id;
    //                     //     string path = "/mnt/c/Users/prprp/Desktop/twitter/";
    //                     //     string fileName = path + tweetID + "_" + (i + 1) + ".jpg";
    //                     //     client.DownloadFile(url, fileName);
    //                     // }
    //                     maxID = myFav.Id;
    //                     downloadCount++;
    //                 }
    //             }

    //             if (downloadCount == 0)
    //             {
    //                 Console.WriteLine("Not found new tweets! Please next action." + nl);
    //             }
    //             else
    //             {
    //                 Console.WriteLine(downloadCount + " tweets downloaded!" + " Please next action." + nl);
    //                 Console.WriteLine(token.Favorites.List().RateLimit.Remaining);
    //             }

    //             // PleaseUserAction();
    //             FavoList();
    //         }

    //         // try
    //         // {

    //         // }
    //         // catch (TwitterException e)
    //         // {
    //         //     Console.WriteLine(e.Message);
    //         //     Console.ReadKey();
    //         // }
    //         // catch (System.Net.WebException e)
    //         // {
    //         //     Console.WriteLine(e.Message);
    //         //     Console.ReadKey();
    //         // }
    //     }
}