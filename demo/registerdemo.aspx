<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="registerdemo.aspx.cs" Inherits="demo.registerdemo" %>

<!DOCTYPE html>

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
          <td colspan="2"><script type="text/javascript" src="<%=getGTFront()%>">"></script></td>
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