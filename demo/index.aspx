<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="demo.failback_login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="http://libs.baidu.com/jquery/1.9.0/jquery.js"></script>

</head>
<body>
    <form id="form1" runat="server" method="post" style="margin-top:100px;">
      <div class="row">
				<label for="name">邮箱</label> <input type="text" id="email"
					name="email" value="geetest@126.com" />
			</div>
			<div class="row">
				<label for="passwd">密码</label> <input type="password" id="passwd"
					name="passwd" value="gggggggg" />
			</div>

			<%--Start  Code--%>
			<div class="row">
				<div id="div_geetest_lib"></div>
				<div id="div_id_embed"></div>
        <div class="row">
          <asp:Button ID="submitBtn"  runat="server" OnClick="submitBtn_Click" Text="登　录" />
        </div>
				<script type="text/javascript">


				    function geetest_ajax_results() {
				        $.ajax({
				            url: "/todo/VerifyLoginAction",
				            type: "post",
				            data: gt_captcha_obj.getValidate(),
				            success: function (sdk_result) {
				                console.log(sdk_result)
				            }
				        });
				    }


				    var gtFailbackFrontInitial = function (result) {
				        var s = document.createElement('script');
				        s.id = 'gt_lib';
				        s.src = 'http://static.geetest.com/static/js/geetest.0.0.0.js';
				        s.charset = 'UTF-8';
				        s.type = 'text/javascript';
				        document.getElementsByTagName('head')[0].appendChild(s);
				        var
					loaded = false;
				        s.onload = s.onreadystatechange = function () {
				            if (!loaded
									&& (!this.readyState
											|| this.readyState === 'loaded' || this.readyState === 'complete')) {
				                loadGeetest(result);
				                loaded = true;
				            }
				        };
				    }
				    //get  geetest server status, use the failback solution


				    var loadGeetest = function (config) {

				        //1. use geetest capthca
				        window.gt_captcha_obj = new window.Geetest({
				            gt: config.gt,
				            challenge: config.challenge,
				            product: 'embed',
				            offline: !config.success
				        });

				        gt_captcha_obj.appendTo("#div_id_embed");

				        //Ajax request demo,if you use submit form ,then ignore it 
				        gt_captcha_obj.onSuccess(function () {
				            geetest_ajax_results()
				        });
				    }

				    s = document.createElement('script');
				    s.src = 'http://api.geetest.com/get.php?callback=gtcallback';
				    $("#div_geetest_lib").append(s);

				    var gtcallback = (function () {
				        var status = 0, result, apiFail;
				        return function (r) {
				            status += 1;
				            if (r) {
				                result = r;
				                setTimeout(function () {
				                    if (!window.Geetest) {
				                        apiFail = true;
				                        gtFailbackFrontInitial(result)
				                    }
				                }, 1000)
				            }
				            else if (apiFail) {
				                return
				            }
				            if (status == 2) {
				                loadGeetest(result);
				            }
				        }
				    })()

				    $.ajax({
				        url: "failbackdemo.aspx",
				        type: "get",
				        dataType: 'JSON',
				        success: function (result) {
				            gtcallback(result)
				        }
				    })
				</script>
            </div>
    </form>
</body>
</html>