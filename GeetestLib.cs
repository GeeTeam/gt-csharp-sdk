using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace WebApplication1
{
    public class GeetestLib
    {
        private String privateKey;
        public GeetestLib(String privateKey) {
		    this.privateKey=privateKey;
	    }

        public Boolean validate(String challenge, String validate, String seccode){
    	String host="http://api.geetest.com";
    	String path="/validate.php";
    	int port=80;
    	if(validate.Length>0 && checkResultByPrivate(challenge,validate)){
    		String query="seccode="+seccode;
    		String response = "";
			try {
				response = postValidate(host, path, query, port);
			} catch (Exception e) {
                Console.WriteLine(e);
			}
    		if(response.Equals(md5Encode(seccode))){
    			return true;
    		}
    	}
    	return false;
    	
    }
    private Boolean checkResultByPrivate(String origin, String validate){
    	String encodeStr=md5Encode(privateKey+"geetest"+origin);
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
    private String fixEncoding(String str){
        UTF8Encoding utf8 = new UTF8Encoding();
        Byte[] encodedBytes = utf8.GetBytes(str);
        String decodedString = utf8.GetString(encodedBytes);
        return decodedString;
    }
    
    //md5 加密
    public String md5Encode(String plainText) {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        string t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(plainText)));
        t2 = t2.Replace("-", "");
        t2 = t2.ToLower();
        return t2;
    }

    }
}

