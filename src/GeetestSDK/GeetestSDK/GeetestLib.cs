using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace GeetestSDK
{
    /// <summary>
    /// GeetestLib 极验验证C# SDK基本库
    /// </summary>
    public class GeetestLib
    {
        /// <summary>
        /// SDK版本号
        /// </summary>
        public const String version = "3.2.0";
        /// <summary>
        /// SDK开发语言
        /// </summary>
        public const String sdkLang = "csharp";
        /// <summary>
        /// 极验验证API URL
        /// </summary>
        protected const String apiUrl = "http://api.geetest.com";
        /// <summary>
        /// register url
        /// </summary>
        protected const String registerUrl = "/register.php";
        /// <summary>
        /// validate url
        /// </summary>
        protected const String validateUrl = "/validate.php";
        /// <summary>
        /// 极验验证API服务状态Session Key
        /// </summary>
        public const String gtServerStatusSessionKey = "gt_server_status";
        /// <summary>
        /// 极验验证二次验证表单数据 Chllenge
        /// </summary>
        public const String fnGeetestChallenge = "geetest_challenge";
        /// <summary>
        /// 极验验证二次验证表单数据 Validate
        /// </summary>
        public const String fnGeetestValidate = "geetest_validate";
        /// <summary>
        /// 极验验证二次验证表单数据 Seccode
        /// </summary>
        public const String fnGeetestSeccode = "geetest_seccode";
        private String userID = "";
        private String responseStr = "";
        private String captchaID = "";
        private String privateKey = "";

        /// <summary>
        /// 验证成功结果字符串
        /// </summary>
        public const int successResult = 1;
        /// <summary>
        /// 证结失败验果字符串
        /// </summary>
        public const int failResult = 0;
        /// <summary>
        /// 判定为机器人结果字符串
        /// </summary>
        public const String forbiddenResult = "forbidden";

        /// <summary>
        /// GeetestLib构造函数
        /// </summary>
        /// <param name="publicKey">极验验证公钥</param>
        /// <param name="privateKey">极验验证私钥</param>
        public GeetestLib(String publicKey, String privateKey)
        {
            this.privateKey = privateKey;
            this.captchaID = publicKey;
        }
        private int getRandomNum()
        {
            Random rand =new Random();
            int randRes = rand.Next(100);
            return randRes;
        }

        /// <summary>
        /// 验证初始化预处理
        /// </summary>
        /// <returns>初始化结果</returns>
        public Byte preProcess()
        {
            if (this.captchaID == null)
            {
                Console.WriteLine("publicKey is null!");
            }
            else
            {
                String challenge = this.registerChallenge();
                if (challenge.Length == 32)
                {
                    this.getSuccessPreProcessRes(challenge);
                    return 1;
                }
                else
                {
                    this.getFailPreProcessRes();
                    Console.WriteLine("Server regist challenge failed!");
                }
            }

            return 0;

        }
        public Byte preProcess(String userID)
        {
            if (this.captchaID == null)
            {
                Console.WriteLine("publicKey is null!");
            }
            else
            {
                this.userID = userID;
                String challenge = this.registerChallenge();
                if (challenge.Length == 32)
                {
                    this.getSuccessPreProcessRes(challenge);
                    return 1;
                }
                else
                {
                    this.getFailPreProcessRes();
                    Console.WriteLine("Server regist challenge failed!");
                }
            }

            return 0;

        }
        public String getResponseStr()
        {
            return this.responseStr;
        }
        /// <summary>
        /// 预处理失败后的返回格式串
        /// </summary>
        private void getFailPreProcessRes()
        {
            int rand1 = this.getRandomNum();
            int rand2 = this.getRandomNum();
            String md5Str1 = this.md5Encode(rand1 + "");
            String md5Str2 = this.md5Encode(rand2 + "");
            String challenge = md5Str1 + md5Str2.Substring(0, 2);
            this.responseStr = "{" + string.Format(
                 "\"success\":{0},\"gt\":\"{1}\",\"challenge\":\"{2}\"", 0,
                this.captchaID, challenge) + "}";
        }
        /// <summary>
        /// 预处理成功后的标准串
        /// </summary>
        private void getSuccessPreProcessRes(String challenge)
        {
            challenge = this.md5Encode(challenge + this.privateKey);
            this.responseStr ="{" + string.Format(
                "\"success\":{0},\"gt\":\"{1}\",\"challenge\":\"{2}\"", 1, 
                this.captchaID, challenge) + "}";
        }
        /// <summary>
        /// failback模式的验证方式
        /// </summary>
        /// <param name="challenge">failback模式下用于与validate一起解码答案， 判断验证是否正确</param>
        /// <param name="validate">failback模式下用于与challenge一起解码答案， 判断验证是否正确</param>
        /// <param name="seccode">failback模式下，其实是个没用的参数</param>
        /// <returns>验证结果</returns>
        public int failbackValidateRequest(String challenge, String validate, String seccode)
        {
            if (!this.requestIsLegal(challenge, validate, seccode)) return GeetestLib.failResult;
            String[] validateStr = validate.Split('_');
            String encodeAns = validateStr[0];
            String encodeFullBgImgIndex = validateStr[1];
            String encodeImgGrpIndex = validateStr[2];
            int decodeAns = this.decodeResponse(challenge, encodeAns);
            int decodeFullBgImgIndex = this.decodeResponse(challenge, encodeFullBgImgIndex);
            int decodeImgGrpIndex = this.decodeResponse(challenge, encodeImgGrpIndex);
            int validateResult = this.validateFailImage(decodeAns, decodeFullBgImgIndex, decodeImgGrpIndex);
            return validateResult;
        }
        private int validateFailImage(int ans, int full_bg_index, int img_grp_index)
        {
            const int thread = 3;
            String full_bg_name = this.md5Encode(full_bg_index + "").Substring(0, 10);
            String bg_name = md5Encode(img_grp_index + "").Substring(10, 10);
            String answer_decode = "";
            for (int i = 0;i < 9; i++)
            {
                if (i % 2 == 0) answer_decode += full_bg_name.ElementAt(i);
                else if (i % 2 == 1) answer_decode += bg_name.ElementAt(i);
            }
            String x_decode = answer_decode.Substring(4);
            int x_int = Convert.ToInt32(x_decode, 16);
            int result = x_int % 200;
            if (result < 40) result = 40;
            if (Math.Abs(ans - result) < thread) return GeetestLib.successResult;
            else return GeetestLib.failResult;
        }
        private Boolean requestIsLegal(String challenge, String validate, String seccode)
        {
            if (challenge.Equals(string.Empty) || validate.Equals(string.Empty) || seccode.Equals(string.Empty)) return false;
            return true;
        }

        /// <summary>
        /// 向gt-server进行二次验证
        /// </summary>
        /// <param name="challenge">本次验证会话的唯一标识</param>
        /// <param name="validate">拖动完成后server端返回的验证结果标识字符串</param>
        /// <param name="seccode">验证结果的校验码，如果gt-server返回的不与这个值相等则表明验证失败</param>
        /// <returns>二次验证结果</returns>
        public int enhencedValidateRequest(String challenge, String validate, String seccode)
        {
            if (!this.requestIsLegal(challenge, validate, seccode)) return GeetestLib.failResult;
            if (validate.Length > 0 && checkResultByPrivate(challenge, validate))
            {
                String query = "seccode=" + seccode + "&sdk=csharp_" + GeetestLib.version;
                String response = "";
                try
                {
                    response = postValidate(query);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                if (response.Equals(md5Encode(seccode)))
                {
                    return GeetestLib.successResult;
                }
            }
            return GeetestLib.failResult;
        }
        public int enhencedValidateRequest(String challenge, String validate, String seccode, String userID)
        {
            if (!this.requestIsLegal(challenge, validate, seccode)) return GeetestLib.failResult;
            if (validate.Length > 0 && checkResultByPrivate(challenge, validate))
            {
                String query = "seccode=" + seccode + "&user_id=" + userID + "&sdk=csharp_" + GeetestLib.version;
                String response = "";
                try
                {
                    response = postValidate(query);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                if (response.Equals(md5Encode(seccode)))
                {
                    return GeetestLib.successResult;
                }
            }
            return GeetestLib.failResult;
        }
        private String readContentFromGet(String url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 20000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                String retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }
           catch
           {
               return "";     
           }

        }
        private String registerChallenge()
        {
            String url = "";
            if (string.Empty.Equals(this.userID))
            {
                url = string.Format("{0}{1}?gt={2}", GeetestLib.apiUrl, GeetestLib.registerUrl, this.captchaID);
            }
            else
            {
                url = string.Format("{0}{1}?gt={2}&user_id={3}", GeetestLib.apiUrl, GeetestLib.registerUrl, this.captchaID, this.userID);
            }
            string retString = this.readContentFromGet(url);
            return retString;
        }
        private Boolean checkResultByPrivate(String origin, String validate)
        {
            String encodeStr = md5Encode(privateKey + "geetest" + origin);
            return validate.Equals(encodeStr);
        }
        private String postValidate(String data)
        {
            String url = string.Format("{0}{1}", GeetestLib.apiUrl, GeetestLib.validateUrl);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(data);
            // 发送数据
            Stream myRequestStream = request.GetRequestStream();
            byte[] requestBytes = System.Text.Encoding.ASCII.GetBytes(data);
            myRequestStream.Write(requestBytes, 0, requestBytes.Length);
            myRequestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // 读取返回信息
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;

        }
        private int decodeRandBase(String challenge)
        {
            String baseStr = challenge.Substring(32, 2);
            List<int> tempList = new List<int>();
            for(int i = 0; i < baseStr.Length; i++)
            {
                int tempAscii = (int)baseStr[i];
                tempList.Add((tempAscii > 57) ? (tempAscii - 87)
                    : (tempAscii - 48));
            }
            int result = tempList.ElementAt(0) * 36 + tempList.ElementAt(1);
            return result;
        }
        private int decodeResponse(String challenge, String str)
        {
            if (str.Length>100) return 0;
            int[] shuzi = new int[] { 1, 2, 5, 10, 50};
            String chongfu = "";
            Hashtable key = new Hashtable();
            int count = 0;
            for (int i=0;i<challenge.Length;i++)
            {
                String item = challenge.ElementAt(i) + "";
                if (chongfu.Contains(item)) continue;
                else
                {
                    int value = shuzi[count % 5];
                    chongfu += item;
                    count++;
                    key.Add(item, value);
                }
            }
            int res = 0;
            for (int i = 0; i < str.Length; i++) res += (int)key[str[i]+""];
            res = res - this.decodeRandBase(challenge);
            return res;
        }
        private String md5Encode(String plainText)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(plainText)));
            t2 = t2.Replace("-", "");
            t2 = t2.ToLower();
            return t2;
        }

    }
}
