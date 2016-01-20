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
            Byte gtServerStatus = geetest.preProcess();
            Session[GeetestLib.gtServerStatusSessionKey] = gtServerStatus;
            return geetest.ResponseStr;
        }
    }
}