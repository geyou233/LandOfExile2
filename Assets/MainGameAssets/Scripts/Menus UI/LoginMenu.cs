
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DefaultNamespace;
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

    [Serializable]
    private class ErrorResponse
    {
        public string message;
        public int errcode;
    }
    
    /// <summary>注册登录请求</summary>
    [Serializable]
    private class RegisterOrLoginRequest
    {
        public string phone;
        public string smscode;
    }
    
    [Serializable]
    private class MsgCodeRequest
    {
        public string phone;
    }

    
    /// <summary>token登录请求</summary>
    [Serializable]
    private class PhoneTokenRequest
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
        base.ShowMenu();
        privacyTipsRoot.SetActive(PlayerPrefs.GetInt("AgreePrivacyTips", 0) == 0);
        loginLayerRoot.SetActive(PlayerPrefs.GetInt("AgreePrivacyTips", 0) == 1);
        verifyIDLayerRoot.SetActive(false);
        waitSendTime.text = "获取验证码";
        waitTime = 0;
        canSendVerifyCode = true;
        popupTips.SetActive(false);
        
        if (MenuControl.Instance.battleMusicController != null || MenuControl.Instance.adventureMusicController == null)
            MenuControl.Instance.PlayAdventureMusic();

        var saveTokenTime = PlayerPrefs.GetString("saveTokenTime");
        if (!string.IsNullOrEmpty(saveTokenTime))
        {
            DateTime oldTime = DateTime.Parse(saveTokenTime);
            
            DateTime now = DateTime.Now;

            var offsetTime = now - oldTime;
            if (offsetTime.TotalSeconds >= 1 * 3600)
                return;
            
            var phone = PlayerPrefs.GetString("phone");
            var token = PlayerPrefs.GetString("token");
            if (string.IsNullOrEmpty(token)) return;
            
            LoginWithToken("login_with_token", phone, token);
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
        string phone = account.text;
        if (string.IsNullOrEmpty(phone))
        {
            ShowPopMessage("请输入手机号码");
            return;
        }

        if (!CheckPhoneIsValid(phone))
        {
            ShowPopMessage("手机号码格式不正确，请重新输入");
            return;
        }

        if (!agreePrivacy.isOn)
        {
            ShowPopMessage("请先阅读并勾选同意");
            return;
        }
        
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

        if (PlayerPrefs.GetString("phone") == account.text)
        {
            Login("login", account.text, password.text);
        }
        else
        {
            Register("register", account.text, password.text);
        }
    }
    
    public void OnIDCardVerifyBtnClick()
    {
        if (string.IsNullOrEmpty(realName.text))
        {
            nameErrorTips.SetActive(true);
            return;
        }
        nameErrorTips.SetActive(false);
        
        if (string.IsNullOrEmpty(realNo.text))
        {
            idCardErrorTips.SetActive(true);
            return;
        }
        idCardErrorTips.SetActive(false);

        var result = IDCardValidator.ValidateRealName(realName.text);
        if (!result.IsValid)
        {
            ShowPopMessage(result.Message);
            return;
        }

        if (!IDCardValidator.IsValidIDCard(realNo.text))
        {
            ShowPopMessage("身份证号码错误，请重新输入");
            return;
        }

        if (!IDCardValidator.IsAdult(realNo.text))
        {
            ShowPopMessage("为进一步落实未成年人保护，未满18周岁的用户不允许注册游戏账号(10010)");
            return;
        }
        string phone = PlayerPrefs.GetString("phone");
        string token = PlayerPrefs.GetString("token");
        IDCardVerify("realname_and_id_varify", phone, realNo.text, realName.text, token);
    }

    public void OnIDCardCancelBtnClick()
    {
        verifyIDLayerRoot.SetActive(false);
    }

    public void OnChangeAccountBtnClick()
    {
        account.text = "";
        password.text = "";
        verifyIDLayerRoot.SetActive(false);
        loginLayerRoot.SetActive(true); 
    }

    public void OnAgeButtonClick()
    {
        ageTipsMenu.ShowMenu();
    }

    #endregion

    
    private IEnumerator SendPostRequest(string url, string jsonData, Action<string> OnSucess, Action<int> onFail = null)
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
                
                var errorMesg = JsonUtility.FromJson<ErrorResponse>(result);
                if (errorMesg != null && errorMesg.errcode != 0 && errorMesg.errcode != 10019)
                {
                    onFail?.Invoke(errorMesg.errcode);
                }
                else
                {
                    OnSucess?.Invoke(result);
                }
            }
            else
            {
                Debug.LogError("SendRequest Error: " + request.error);
            }
        }
    }

    
    //检测手机号码是否合法
    private bool CheckPhoneIsValid(string input)
    {
        if (input.Length < 11)
        {
            return false;
        }

        //电信手机号码正则
        string dianxin = @"^1[3578][01379]\d{8}$";
        Regex regexDX = new Regex(dianxin);
        
        //联通手机号码正则
        string liantong = @"^1[34578][01256]\d{8}";
        Regex regexLT = new Regex(liantong);
        
        //移动手机号码正则
        string yidong = @"^(1[012345678]\d{8}|1[345678][012356789]\d{8})$";
        Regex regexYD = new Regex(yidong);

        if (regexDX.IsMatch(input) || regexLT.IsMatch(input) || regexYD.IsMatch(input))
        {
            return true;
        }

        return false;
    }


    

    #region 发送验证码
    //发送验证码信息
    private void SendMessageVerifyCode(string request, string phone)
    {
        var user = new MsgCodeRequest { phone = phone };
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
            ShowPopMessage("请先阅读并勾选同意");
            return;
        }
        var user = new RegisterOrLoginRequest { phone = phone, smscode = pwd };
        string jsonData = JsonUtility.ToJson(user);

        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnRegisterSuccess, OnRegisterFail));
    }
    
    private void OnRegisterSuccess(string jsonData)
    {
        RegisterOrLoginResponseData data = JsonUtility.FromJson<RegisterOrLoginResponseData>(jsonData);
        if (data == null) return;
        
        PlayerPrefs.SetString("phone", account.text.Trim());
        PlayerPrefs.SetString("token", data.access_token);
        PlayerPrefs.Save();
        //表示已经实名认证过
        if (data.realname_varify_finish == 1)
        {
            //自动登录
            LoginWithToken("login_with_token", account.text, data.access_token);
        }
        else
        {
            //todo
            loginLayerRoot.SetActive(false);
            verifyIDLayerRoot.SetActive(true);
        }
    }

    private void OnRegisterFail(int code)
    {
        if (code == 10001 || code == 10003)
        {
            ShowPopMessage($"验证码错误({code})");
        }
        if (code == 10002)
        {
            ShowPopMessage($"验证码错误或已过期({code})");
        }
        else if (code == 10004)
        {
            //已经注册过直接登录
            Login("login", account.text, password.text);
        }
    }
    
    #endregion
    
    #region 登录
    /// <summary>登录</summary>
    private void Login(string request, string phone, string pwd)
    {
        if (!agreePrivacy.isOn)
        {
            ShowPopMessage("请先阅读并勾选同意");
            return;
        }
        var user = new RegisterOrLoginRequest { phone = phone, smscode = pwd };
        string jsonData = JsonUtility.ToJson(user);
        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnLoginSuccess, OnLoginFail));
    }
    
    /// <summary>使用token登录</summary>
    private void LoginWithToken(string request, string phone, string token)
    {
        var user = new PhoneTokenRequest {phone = phone, tmp_token = token};
        string jsonData = JsonUtility.ToJson(user);
        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnLoginWithTokenSuccess, OnLoginWithTokenFail));
    }

    private void OnLoginWithTokenSuccess(string jsonData)
    {
        ShowMainMenu();
    }

    private void OnLoginWithTokenFail(int code)
    {
        if (code == 10020)
        {
            loginLayerRoot.SetActive(false);
            verifyIDLayerRoot.SetActive(true);
        }
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

    private void OnLoginFail(int code)
    {
        if (code == 10015)
        {
            ShowPopMessage($"手机号错误({code})");
        }
        if (code == 10016)
        {
            ShowPopMessage($"验证码错误或已过期({code})");
        }
        if (code == 10017)
        {
            ShowPopMessage($"验证码错误({code})");
        }
        if (code == 10018)
        {
            Register("register", account.text, password.text);
        }
    }
    #endregion

    #region 实名认证
    ///实名认证
    private void IDCardVerify(string request, string phone, string idNo, string userName, string token)
    {
        var data = new RealNameData() { phone = phone, realname = userName, idstr = idNo, tmp_token = token };
        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(SendPostRequest(webUrl + request, jsonData, OnIDCardVerifySuccess, OnIDCardVerifyFail));
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


    private void OnIDCardVerifyFail(int code)
    {
        if (code == 10009)
        {
            ShowPopMessage($"身份证号码错误({code})");  
        }
        if (code == 10010)
        {
            ShowPopMessage("用户身份证未满18周岁");
        }
        if (code == 10011 || code == 10012 || code == 10013 || code == 10014)
        {
            ShowPopMessage($"姓名或身份证错误，请重新输入({code})");
        }
    }

    public void OnNameEditEnd(string strName)
    {
        nameErrorTips.SetActive(string.IsNullOrWhiteSpace(strName));
    }
    
    public void OnIDCardEditEnd(string idCardNo)
    {
        idCardErrorTips.SetActive(string.IsNullOrWhiteSpace(idCardNo));
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
