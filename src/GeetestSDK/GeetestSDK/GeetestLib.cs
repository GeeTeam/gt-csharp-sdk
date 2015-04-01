using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace GeetestSDK
{
    public class GeetestLib
    {
        private String privateKey;
        private String captchaID;
        private String challenge;
        private String host = "http://api.geetest.com";
        private String productType;
        private String popupBtnID;
        private String version = "2.15.4.1.1";
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
        public GeetestLib(String privateKey, String publicKey)
        {
            this.privateKey = privateKey;
            this.captchaID = publicKey;
        }

        public GeetestLib(String privateKey) 
        {
            this.privateKey = privateKey;
        }

        public String getGTApiUrl()
        {
            String frontSource = string.Format("{0}/get.php?gt={1}&challenge={2}", this.host, this.captchaID, this.challenge);
            if (this.productType !=null)
            {
                if (this.productType.Equals("popup"))
                {
                    frontSource += string.Format("&product={0}&popupbtnid={1}", this.productType, this.popupBtnID);
                }
                else
                {
                    frontSource += "&product=" + this.productType;
                }
            }
            return frontSource;
        }

        public Boolean validate(String challenge, String validate, String seccode)
        {
            String path = "/validate.php";
            int port = 80;
            if (validate.Length > 0 && checkResultByPrivate(challenge, validate))
            {
                String query = "seccode=" + seccode + "&sdk=csharp_" + this.version;
                String response = "";
                try
                {
                    response = postValidate(this.host, path, query, port);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                if (response.Equals(md5Encode(seccode)))
                {
                    return true;
                }
            }
            return false;

        }

        public Boolean getGtServerStatus()
        {
            String path = "/check_status.php";
            String url = this.host + path;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                if (retString == "ok") 
                {
                    return true;
                }
            }
            catch
            {
                
            }
            return false;
        }

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

        private String registerChallenge(String host, String path, String gt)
        {
            String url = this.host + path + "?gt=" + gt;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        private Boolean checkResultByPrivate(String origin, String validate)
        {
            String encodeStr = md5Encode(privateKey + "geetest" + origin);
            return validate.Equals(encodeStr);
        }

        private String postValidate(String host, String path, String data, int port)
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

        //转为UTF8编码
        private String fixEncoding(String str)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(str);
            String decodedString = utf8.GetString(encodedBytes);
            return decodedString;
        }

        //md5 加密
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
