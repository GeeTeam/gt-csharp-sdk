using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Text;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace GeetestSDK
{
    /// <summary>
    /// GeetestLib
    /// </summary>
    public class GeetestLib
    {
        //SDK版本以及名称
        public const String version = "2.0.0";
        public const String sdkLang = "csharp";
       
        protected const String baseUrl = "api.geetest.com";
        protected const String apiUrl = "http://" + baseUrl;
        protected const String httpsApiUrl = "https://" + baseUrl;
        protected const int hostPort = 80;

        protected const String gtSessionKey = "geetest";
        protected const String gtServerStatusSessionKey = "gt_server_status";

        public const String fnGeetestChallenge = "geetest_challenge";
        public const String fnGeetestValidate = "geetest_validate";
        public const String fnGeetestSeccode = "geetest_seccode";

        private String captchaID = "";
        private String privateKey = "";
        private String challenge = "";
        private String host = apiUrl;
        private String productType = "embed";
        private String popupBtnID = "";
        
        private Boolean https = false;
        public const String successResult = "success";
        public const String failResult = "fail";
        public const String forbiddenResult = "forbidden";
        public Boolean Https
        {
            set 
            { 
                this.https = value;
                if (this.https) this.host = GeetestLib.httpsApiUrl;
                else this.host = GeetestLib.apiUrl;
            }
            get { return this.Https; }
        }
        public String ProductType 
        {
            set { this.productType = value; }
            get { return this.productType; }
        }
        public String PopupBtnID
        {
            set { this.popupBtnID = value; }
            get { return this.popupBtnID; }
        }
        public String Challenge
        {
            //set { this.challenge = value; }
            get { return this.challenge; }
        }
        public String CaptchaID
        {
            set { this.captchaID = value; }
            get { return this.captchaID; }
        }
        public String PrivateKey
        {
            set { this.privateKey = value; }
            get { return this.privateKey; }
        }
        public GeetestLib(){ }

        public GeetestLib(String privateKey, String publicKey)
        {
            this.privateKey = privateKey;
            this.captchaID = publicKey;
        }

        public GeetestLib(String privateKey) 
        {
            this.privateKey = privateKey;
        }

        /// <summary>
        /// 设置极验服务器的gt-server状态值
        /// </summary>
        /// <param name="session"></param>
        /// <param name="statusCode">gt-server状态值,0表示不正常，1表示正常</param>
        public void setGtServerStatusSession(HttpSessionState session, int statusCode)
        {
            session.Add(GeetestLib.gtServerStatusSessionKey, statusCode);
        }
        /// <summary>
        /// 获取gt-server状态值,0表示不正常，1表示正常
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <returns>int，0表示不正常，1表示正常</returns>
        public static int getGtServerStatusSession(HttpSessionState session)
        {
            return (int) session.Contents[GeetestLib.gtServerStatusSessionKey];
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <returns>int</returns>
        public int getRandomNum()
        {
            Random rand =new Random();
            int randRes = rand.Next(100);
            return randRes;
        }

        /// <summary>
        /// 验证初始化预处理
        /// </summary>
        /// <returns>Boolean</returns>
        public Boolean preProcess()
        {
            if (this.register()) return true;
            return false;
        }
        /// <summary>
        /// 预处理失败后的返回格式串
        /// </summary>
        /// <returns>String Json字符串</returns>
        public String getFailPreProcessRes()
        {
            int rand1 = this.getRandomNum();
            int rand2 = this.getRandomNum();
            String md5Str1 = this.md5Encode(rand1 + "");
            String md5Str2 = this.md5Encode(rand2 + "");
            String challenge = md5Str1 + md5Str2.Substring(0, 2);
            this.challenge = challenge;
            return "{" + string.Format(
                 "\"success\":{0},\"gt\":\"{1}\",\"challenge\":\"{2}\"", 0,
                this.captchaID, this.challenge)+"}";
        }
        /// <summary>
        /// 预处理成功后的标准串
        /// </summary>
        /// <returns>Json字符串</returns>
        public String getSuccessPreProcessRes()
        {
            return "{" + string.Format(
                "\"success\":{0},\"gt\":\"{1}\",\"challenge\":\"{2}\"", 1, 
                this.captchaID, this.challenge) + "}";
        }
        /// <summary>
        /// failback模式的验证方式
        /// </summary>
        /// <param name="request"></param>
        /// <returns>验证结果</returns>
        public String failbackValidateRequest(HttpRequest request)
        {
            if (!this.requestIsLegal(request)) return GeetestLib.failResult;
            String challenge = request.Params[GeetestLib.fnGeetestChallenge];
            String validate = request.Params[GeetestLib.fnGeetestValidate];
            String seccode = request.Params[GeetestLib.fnGeetestSeccode];
            if (!challenge.Equals(this.challenge)) return GeetestLib.failResult;
            String[] validateStr = validate.Split('_');
            String encodeAns = validateStr[0];
            String encodeFullBgImgIndex = validateStr[1];
            String encodeImgGrpIndex = validateStr[2];

            int decodeAns = this.decodeResponse(this.challenge, encodeAns);
            int decodeFullBgImgIndex = this.decodeResponse(this.challenge, encodeFullBgImgIndex);
            int decodeImgGrpIndex = this.decodeResponse(this.challenge, encodeImgGrpIndex);

            String validateResult = this.validateFailImage(decodeAns, decodeFullBgImgIndex, decodeImgGrpIndex);
            if (!validateResult.Equals(GeetestLib.failResult))
            {
                int rand1 = this.getRandomNum();
                String md5Str = this.md5Encode(rand1 + "");
                this.challenge = md5Str;
            }

            return validateResult;
        }
        /// <summary>
        /// failback模式下，简单判断轨迹是否通过
        /// </summary>
        /// <param name="ans">答案位置</param>
        /// <param name="full_bg_index"></param>
        /// <param name="img_grp_index"></param>
        /// <returns>轨迹验证结果</returns>
        private String validateFailImage(int ans, int full_bg_index, int img_grp_index)
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
        /// <summary>
        /// 判断request参数是否合法
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Boolean</returns>
        private Boolean requestIsLegal(HttpRequest request)
        {
            String challenge = request.Params[GeetestLib.fnGeetestChallenge];
            String validate = request.Params[GeetestLib.fnGeetestValidate];
            String seccode = request.Params[GeetestLib.fnGeetestSeccode];
            if (challenge.Equals(string.Empty) || validate.Equals(string.Empty) || seccode.Equals(string.Empty)) return false;
            return true;
        }

        /// <summary>
        /// 向gt-server进行二次验证
        /// </summary>
        /// <param name="request"></param>
        /// <returns>String 二次验证结果</returns>
        public String enhencedValidateRequest(HttpRequest request)
        {
            if (!this.requestIsLegal(request)) return GeetestLib.failResult;
            String path = "/validate.php";
            String challenge = request.Params[GeetestLib.fnGeetestChallenge];
            String validate = request.Params[GeetestLib.fnGeetestValidate];
            String seccode = request.Params[GeetestLib.fnGeetestSeccode];
            if (validate.Length > 0 && checkResultByPrivate(challenge, validate))
            {
                String query = "seccode=" + seccode + "&sdk=csharp_" + GeetestLib.version;
                String response = "";
                try
                {
                    response = postValidate(this.host, path, query);
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
        /// <summary>
        /// 进行GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns>String HTTP请求的结果</returns>
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
        //向gt-server请求一个challenge
        /// <summary>
        /// 向gt-server请求一个challenge，并以此判断服务器状态
        /// </summary>
        /// <returns>Boolean 请求是否成功</returns>
        public Boolean register()
        {
            String path = "/register.php";
            if (this.captchaID == null)
            {
                Console.WriteLine("publicKey is null!");
            } 
            else
            {
                String challenge = this.registerChallenge(this.host, path, this.captchaID);
                if (challenge.Length == 32)
                {
                    this.challenge = challenge;
                    return true;
                }
                else
                {
                    Console.WriteLine("Server regist challenge failed!");
                }
            }

            return false;
            
        }
        /// <summary>
        /// 向gt-server请求一个challenge
        /// </summary>
        /// <param name="host"></param>
        /// <param name="path"></param>
        /// <param name="gt">captcha_id</param>
        /// <returns>String challenge</returns>
        private String registerChallenge(String host, String path, String gt)
        {
            String url = host + path + "?gt=" + gt;
            string retString = this.readContentFromGet(url);
            return retString;
        }
        /// <summary>
        /// 二次验证先验判断，判断validate是否与privatekey challenge 吻合
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="validate"></param>
        /// <returns>Boolean 判断结果</returns>
        private Boolean checkResultByPrivate(String origin, String validate)
        {
            String encodeStr = md5Encode(privateKey + "geetest" + origin);
            return validate.Equals(encodeStr);
        }
        /// <summary>
        /// 向gt-server发起二次验证请求
        /// </summary>
        /// <param name="host"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private String postValidate(String host, String path, String data)
        {
            String url = host + path;
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
        /// <summary>
        /// 解码偏移量
        /// </summary>
        /// <param name="challenge"></param>
        /// <returns>int 偏移量</returns>
        public int decodeRandBase(String challenge)
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
        /// <summary>
        /// 解码随机参数
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public int decodeResponse(String challenge, String str)
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

        /// <summary>
        /// md5加密
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns>String 加密结果</returns>
        public String md5Encode(String plainText)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(plainText)));
            t2 = t2.Replace("-", "");
            t2 = t2.ToLower();
            return t2;
        }

    }
}
