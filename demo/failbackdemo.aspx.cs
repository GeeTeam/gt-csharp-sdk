using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GeetestSDK;

namespace demo
{
    public partial class failbackdemo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
            Response.Write(getCaptcha());
            Response.End();
        }
        private String getCaptcha()
        {
            GeetestLib geetest = new GeetestLib();
            geetest.CaptchaID = GeetestConfig.publicKey;
            geetest.PrivateKey = GeetestConfig.privateKey;
            String resStr = "";
            if (geetest.preProcess())
            {
                resStr = geetest.getSuccessPreProcessRes();
                geetest.setGtServerStatusSession(Session, 1);
            }
            else
            {
                resStr = geetest.getFailPreProcessRes();
                geetest.setGtServerStatusSession(Session, 0);
            }
            return resStr;
        }
    }
}