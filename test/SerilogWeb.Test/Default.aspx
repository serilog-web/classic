<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SerilogWeb.Test.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox runat="server" Text="My Content"></asp:TextBox>
        <button runat="server" OnServerClick="Fail">Click me</button>
    
    </div>
    </form>
</body>
</html>
