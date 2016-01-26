Gt C# SDK
=========

极验验证　C#　SDK,支持.Net Framework3.5及以上版本．本项目提供的Demo的前端实现方法均是面向PC端的。 本项目是面向服务器端的，具体使用可以参考我们的 `文档 <http://www.geetest.com/install/sections/idx-server-sdk.html>`_ ,客户端相关开发请参考我们的 `前端文档 <http://www.geetest.com/install/>`_.

开发环境
________

    - Visual Studio（推荐VS2012以上版本）
    - .NET Framework 4.5

快速开始
________

1. 从 `Github <https://github.com/GeeTeam/gt-csharp-sdk/>`_ 上Clone代码:

.. code-block:: bash

    $ git clone https://github.com/GeeTeam/gt-csharp-sdk.git

2. 根据你的.Net Framework版本编译代码(或者用VS打开项目直接运行DEMO)．
#. 将编译完成的DLL引入你的项目.
#. 编写你的代码，代码示例:

.. code-block:: csharp

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
	            return geetest.getResponseStr();
	        }
	    }
	}


发布日志
-----------------
+ 3.1.1

 - 统一接口

+ 3.1.0

 - 添加challenge加密特性，使验证更安全， 老版本更新请先联系管理员

+ 2.0.2
    - 修复Failback Bug

+ 2.0.1 
    - 完善注释
    - 添加API文档
    - 修改Demo
+ 2.0.0
    - 去除旧的接口
    - 添加注释
