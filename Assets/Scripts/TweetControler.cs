using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoreTweet;

/// <summary>
/// テストコード （現在未使用）
/// </summary>
public class TweetControler : MonoBehaviour
{
    // MainScript mainScript;
    [SerializeField] public InputField tweetTextInputField;

    void Start()
    {
    }

    public void Tweet()
    {
        // InputField tweetText = tweetTextInputField.GetComponent<InputField>();
        // MainScript mainScript = GameObject.Find("AppManager").GetComponent<MainScript>();
        // MainScript mainScript = GetComponent<MainScript>();
        string tweetText = tweetTextInputField.text;
        if (tweetText != "")
        {
            MainScript.token.Statuses.Update(status => tweetText);
            Debug.Log("Successful Tweet!");
        }
        else
        {
            Debug.Log("Error! Non text!!");
        }
    }
}
