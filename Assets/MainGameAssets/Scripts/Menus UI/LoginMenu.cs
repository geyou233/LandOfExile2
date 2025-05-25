
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Doozy.Engine.UI;
using MainGameAssets.Scripts.Menus_UI;
using MetarCommonSupport;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginMenu : BasicMenu
{
    [SerializeField] private string webUrl = "http://account.thearky.cn:8090/";

    [SerializeField] private GameObject privacyTipsRoot;

    [SerializeField] private GameObject loginLayerRoot;
    
    [SerializeField] private GameObject verifyIDLayerRoot;

    [SerializeField] private InputField account;

    [SerializeField] private InputField password;
    
    [SerializeField] private MainMenu mainMenu;

    [SerializeField] private AgeTipsMenu ageTipsMenu;

    [SerializeField] private InputField realName;

    [SerializeField] private InputField realNo;

    /// <summary>等待下一次发送时间</summary>
    [SerializeField] private Text waitSendTime;

    [SerializeField] private GameObject popupTips;

    [SerializeField] private Toggle agreePrivacy;

    [SerializeField] private Toggle autoLogin;

    [SerializeField] private GameObject nameErrorTips;
    
    [SerializeField] private GameObject idCardErrorTips;
    
    /// <summary>等待下次发送短信的时间</summary>
    private float waitTime = 60;

    /// <summary>是否可以发送短信验证码</summary>
    private bool canSendVerifyCode = true;
    
    /// <summary>注册登录请求</summary>
    [Serializable]
    private class RegisterOrLoginRequest
    {
        public string phone;
        public string smscode;
    }
    
    [Serializable]
    private class PhoneVerification
    {
        public string phone;
    }

    [Serializable]
    private class PhoneToken
    {
        public string phone;
        public string tmp_token;
    }

    [Serializable]
    public class RegisterOrLoginResponseData
    {
        public string access_token;
        public string token_type;
        public int realname_varify_finish;
    }

    
    /// <summary>实名认证数据</summary>
    [Serializable]
    public class RealNameData
    {
        public string phone;
        public string realname;
        public string idstr;
        public string tmp_token;
    }

    [Serializable]
    public class RealNameResponse
    {
        public string access_token;
        public string token_type;
        public string realname_varify_finish;
    }

    public override void ShowMenu()
    {
        // PlayerPrefs.SetInt("AgreePrivacyTips", 0);
        base.ShowMenu();
        privacyTipsRoot.SetActive(PlayerPrefs.GetInt("AgreePrivacyTips", 0) == 0);
        loginLayerRoot.SetActive(PlayerPrefs.GetInt("AgreePrivacyTips", 0) == 1);
        verifyIDLayerRoot.SetActive(false);
        waitSendTime.text = "获取验证码";
        waitTime = 0;
        canSendVerifyCode = true;
        popupTips.SetActive(false);

        var saveTokenTime = PlayerPrefs.GetString("saveTokenTime");
        if (!string.IsNullOrEmpty(saveTokenTime))
        {
            DateTime oldTime = DateTime.Parse(saveTokenTime);
            
            DateTime now = DateTime.Now;

            var offsetTime = oldTime - now;
            if (offsetTime.TotalSeconds >= 24 * 3600)
                return;
            
            var token = PlayerPrefs.GetString("token");
            if (string.IsNullOrEmpty(token)) return;
            
            // LoginWithToken();
            
        }
    }


    public override void HideMenu(bool instantly = false)
    {
        base.HideMenu(instantly);
    }


    public void OnAgreePrivacyTipsBtnClick()
    {
        PlayerPrefs.SetInt("AgreePrivacyTips", 1);
        privacyTipsRoot.SetActive(false);
        loginLayerRoot.SetActive(true);
    }

    public void OnDisAgreePrivacyTipsBtnClick()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (canSendVerifyCode)
        {
            return;
        }
        
        if (waitTime > 0)
        {
            canSendVerifyCode = false;
            waitTime -= Time.deltaTime;

            waitSendTime.text = $"{Mathf.FloorToInt(waitTime):D2}秒";
        }
        else
        {
            canSendVerifyCode = true;
            waitTime = 0;
            waitSendTime.text = "获取验证码";
        }
    }
    
    
    #region Button Events
    
    /// <summary>发送短信验证码</summary>
    public void OnSendVerifyCodeClick()
    {
        if (waitTime > 0)
        {
            return;
        }
        SendMessageVerifyCode("get_sms_code", account.text);
        waitTime = 60;
        canSendVerifyCode = false;
    }
    
    public void OnRegisterBtnClick()
    {
        Register("register", account.text, password.text);
    }

    public void OnLoginBtnClick()
    {
        if (string.IsNullOrEmpty(account.text))
        {
            ShowPopMessage("请输入手机号码");
            return;
        }

        if (string.IsNullOrEmpty(password.text))
        {
            ShowPopMessage("请输入验证码");
            return;
        }
        
        if(!MetarnetRegex.IsMobilePhone(account.text))
        {
            ShowPopMessage("电话号码错误");
            return;
        }
        
        if(!MetarnetRegex.IsNumber(password.text))
        {
            ShowPopMessage("验证码不合法");
            return;
        }
        Login("login", account.text, password.text);
    }
    
    public void OnIDCardVerifyBtnClick()
    {
        string phone = PlayerPrefs.GetString("phone");
        string token = PlayerPrefs.GetString("token");
        IDCardVerify("realname_and_id_varify", phone, realNo.text, realName.text, token);
    }

    public void OnIDCardCancelBtnClick()
    {
        verifyIDLayerRoot.SetActive(false);
    }

    public void OnAgeButtonClick()
    {
        ageTipsMenu.ShowMenu();
    }

    #endregion

    
    private IEnumerator SendPostRequest(string url, string jsonData, Action<string> callback)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var result = request.downloadHandler.text;
                Debug.Log("Response: " + result);
                callback?.Invoke(result);
            }
            else
            {
                Debug.LogError("SendRequest Error: " + request.error);
            }
        }
    }


    #region 发送验证码
    //发送验证码信息
    private void SendMessageVerifyCode(string request, string phone)
    {
        var user = new PhoneVerification { phone = phone };
        string jsonData = JsonUtility.ToJson(user);
        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnSendVerifyCodeSuccess));
    }

    private void OnSendVerifyCodeSuccess(string result)
    {
        
    }
    
    #endregion
    
    #region 注册
    /// <summary>注册</summary>
    private void Register(string request, string phone, string pwd)
    {
        if (!agreePrivacy.isOn)
        {
            ShowPopMessage("请先同意隐私协议");
            return;
        }
        var user = new RegisterOrLoginRequest { phone = phone, smscode = pwd };
        string jsonData = JsonUtility.ToJson(user);

        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnRegisterSuccess));
    }
    
    private void OnRegisterSuccess(string jsonData)
    {
        var data = JsonUtility.FromJson<RegisterOrLoginResponseData>(jsonData);

        //表示已经实名认证过
        if (data.realname_varify_finish == 1)
        {
            //自动登录
            var request = "login_with_token";
            LoginWithToken(webUrl + request, account.text, data.access_token);
        }
        else
        {
            //todo
            loginLayerRoot.SetActive(false);
            verifyIDLayerRoot.SetActive(true);
        }
    }
    
    #endregion
    
    #region 登录
    /// <summary>登录</summary>
    private void Login(string request, string phone, string pwd)
    {
        if (!agreePrivacy.isOn)
        {
            ShowPopMessage("请先同意隐私协议");
            return;
        }
        var user = new RegisterOrLoginRequest { phone = phone, smscode = pwd };
        string jsonData = JsonUtility.ToJson(user);
        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnLoginSuccess));
    }
    
    /// <summary>使用token登录</summary>
    private void LoginWithToken(string request, string phone, string token)
    {
        var user = new PhoneToken {phone = phone, tmp_token = token};
        string jsonData = JsonUtility.ToJson(user);
        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnLoginSuccess));
    }
    
    private void OnLoginSuccess(string jsonData)
    {
        var data = JsonUtility.FromJson<RegisterOrLoginResponseData>(jsonData);
        PlayerPrefs.SetString("phone", account.text.Trim());
        PlayerPrefs.SetString("token", data.access_token);
        PlayerPrefs.SetString("saveTokenTime", DateTime.Now.ToString());
        PlayerPrefs.Save();

        if (data.realname_varify_finish == 1)   //已经实名认证过
        {
            ShowMainMenu();
        }
        else
        {
            loginLayerRoot.SetActive(false);
            verifyIDLayerRoot.SetActive(true);
        }
    }
    #endregion

    #region 实名认证
    ///实名认证
    private void IDCardVerify(string request, string phone, string idNo, string userName, string token)
    {
        var data = new RealNameData() { phone = phone, realname = userName, idstr = idNo, tmp_token = token };
        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnIDCardVerifySuccess));
    }

    /// <summary>实名认证结果</summary>
    /// <param name="jsonData"></param>
    private void OnIDCardVerifySuccess(string jsonData)
    {
        var data = JsonUtility.FromJson<RealNameResponse>(jsonData);
        PlayerPrefs.SetString("token", data.access_token);
        PlayerPrefs.SetString("saveTokenTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
        ShowMainMenu();
    }


    public void OnNameEditEnd(string name)
    {
        
    }

    public void OnIDCardEditEnd(string idCardNo)
    {
        idCardErrorTips.SetActive(!CheckIDCard18(idCardNo));
    }
    
    /// <summary>检查身份证号码</summary>
    private static bool CheckIDCard18(string str)
    {
        if (str.Length < 18) return false;

        string number17 = str.Substring(0, 17);
        string pattern = @"^\d*$";
        if (!Regex.IsMatch(number17, pattern)) return false;

        string number18 = str.Substring(17);
        string check = "10X98765432";
        int[] num = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
        int sum = 0;
        for (int i = 0; i < number17.Length; i++)
        {
            sum += Convert.ToInt32(number17[i].ToString()) * num[i];
        }
        sum %= 11;
        if (number18.Equals(check[sum].ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }
    #endregion
    
    public void ShowMainMenu()
    {
        HideMenu();
        mainMenu.ShowMenu();
    }
    
    private void ShowPopMessage(string message)
    {
        if (LeanTween.isTweening(popupTips))
        {
            return;
        }
        var tipsMessage = popupTips.GetComponentInChildren<Text>();
        tipsMessage.text = message;
        popupTips.SetActive(true);
        popupTips.transform.localPosition = new Vector3(0, 100, 0);
        var tween = LeanTween.moveLocalY(popupTips, 300, 0.75f);
        LeanTween.alpha(popupTips, 0.5f, 0.75f);
        tween.setOnComplete(() =>
        {
            popupTips.SetActive(false);
        });
    }
    
}
