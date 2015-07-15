using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GeetestSDK;

namespace demo
{
    public partial class failback_login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        protected void submitBtn_Click(object sender, EventArgs e)
        {
            GeetestLib geetest = GeetestLib.getGtSession(Session);
            int gt_server_status_code = GeetestLib.getGtServerStatusSession(Session);
            String result = "";
            if (gt_server_status_code == 1) result = geetest.enhencedValidateRequest(Request);
            else result = geetest.failbackValidateRequest(Request);
            Response.Write(result);
        }
    }
}