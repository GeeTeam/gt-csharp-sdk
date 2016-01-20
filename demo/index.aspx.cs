using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GeetestSDK;

namespace demo
{
    public partial class Validate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        protected void submitBtn_Click(object sender, EventArgs e)
        {
            GeetestLib geetest = new GeetestLib(GeetestConfig.publicKey, GeetestConfig.privateKey);
            Byte gt_server_status_code = (Byte) Session[GeetestLib.gtServerStatusSessionKey];
            String result = "";
            String challenge = Request.Form.Get(GeetestLib.fnGeetestChallenge);
            String validate = Request.Form.Get(GeetestLib.fnGeetestValidate);
            String seccode = Request.Form.Get(GeetestLib.fnGeetestSeccode);
            if (gt_server_status_code == 1) result = geetest.enhencedValidateRequest(challenge, validate, seccode);
            else result = geetest.failbackValidateRequest(challenge, validate, seccode);
            Response.Write(result);
        }
    }
}