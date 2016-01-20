<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="demo.Validate" %>

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
				<div id="captcha"></div>
        <div class="row">
          <asp:Button ID="submitBtn"  runat="server" OnClick="submitBtn_Click" Text="登　录" />
        </div>
					<script src="http://static.geetest.com/static/tools/gt.js"></script>
				<script>
				    var handler = function (captchaObj) {
				         // 将验证码加到id为captcha的元素里
				         captchaObj.appendTo("#captcha");
				     };
				    $.ajax({
				        // 获取id，challenge，success（是否启用failback）
				        url: "/getcaptcha.aspx",
				        type: "get",
				        dataType: "json", // 使用jsonp格式
				        success: function (data) {
				            // 使用initGeetest接口
				            // 参数1：配置参数，与创建Geetest实例时接受的参数一致
				            // 参数2：回调，回调的第一个参数验证码对象，之后可以使用它做appendTo之类的事件
				            initGeetest({
				                gt: data.gt,
				                challenge: data.challenge,
				                product: "embed", // 产品形式
				                offline: !data.success
				            }, handler);
				        }
				    });
				</script>
            </div>
    </form>
</body>
</html>