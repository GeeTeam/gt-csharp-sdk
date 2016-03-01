using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GeetestSDK;

namespace demo
{
    public partial class GetCaptcha : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
            Response.Write(getCaptcha());
            Response.End();
        }
        private String getCaptcha()
        {
            GeetestLib geetest = new GeetestLib(GeetestConfig.publicKey, GeetestConfig.privateKey);
            String userID = "test";
            Byte gtServerStatus = geetest.preProcess(userID);
            Session[GeetestLib.gtServerStatusSessionKey] = gtServerStatus;
            Session["userID"] = userID;
            return geetest.getResponseStr();
        }
    }
}