using System;
using CoreTweet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// TwitterAPI の認証処理
/// </summary>
public class MainScript : MonoBehaviour
{
    [SerializeField] public GameObject AuthorizeButton;
    [SerializeField] public InputField pincodeInputField;
    [SerializeField] public TextMeshProUGUI discliption;

    static string CK = "veB1GzM0CgTICCDqW09N583Fp";
    static string CS = "pFLfb6lgLFQ8hHEbSPl3rB4Y48oh6QwkRvJZCA4ZyyZVq3xzmo";
    // static string AT = "3677900594-aT65SPtGuWswaTtAXJKVnKMgAgpCNp4kI6oMr2y";
    // static string AS = "XmB4i36BY6Gyj7mSXUpQWGgiNDbZbK0i0cOVaOU90CkA1";

    OAuth.OAuthSession consumerKey;
    string pincode;
    public static Tokens token { get; private set; }

    void Start()
    {
        pincodeInputField.GetComponent<InputField>();
        discliption.GetComponent<Text>();
    }

    /// <summary>
    /// TwitterAPI OAuth認証開始
    /// </summary>
    public void AuthorizeButtonClick()
    {
        consumerKey = OAuth.Authorize(CK, CS);
        string uri = consumerKey.AuthorizeUri.ToString();
        Application.OpenURL(uri);
    }

    /// <summary>
    /// ピンコード認証を実行し、Token取得
    /// </summary>
    public void GetInputPincode()
    {
        pincode = pincodeInputField.text;
        try
        {
            token = consumerKey.GetTokens(pincode);
            SceneManager.LoadScene("MenuScene");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            discliption.text = "認証に失敗しました。コードが間違っているか、期限が切れています。";
            discliption.color = Color.red;
        }
    }
}
