 <%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="WebApplication1.login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server" method="post" style="margin-top:100px;">
    <table>
       <tr>
          <td>用户名</td><td><input type="text"/></td>
       </tr>
       <tr>
          <td>密码</td><td><input type="password"/></td>
       </tr>
       <tr>
          <td colspan="2"><script type="text/javascript" src="http://api.geetest.com/get.php?gt=a40fd3b0d712165c5d13e6f747e948d4"></script></td>
       </tr>
       <tr>
          <td>
          <asp:Button ID="submitBtn"  runat="server" OnClick="submitBtn_Click" Text="登　录" />
          </td>
       </tr>
    </table>
    </form>
</body>
</html>
