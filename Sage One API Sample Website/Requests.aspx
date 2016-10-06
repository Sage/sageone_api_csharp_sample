<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Requests.aspx.cs" Inherits="Sage_One_API_Sample_Website.Requests" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h2>Endpoint</h2>
        <div class="col-md-4">
            <asp:Label ID="Label1" runat="server" Text="Endpoint"></asp:Label>
            <br />
            <asp:TextBox ID="txtEndpoint" runat="server"></asp:TextBox>
            <br />
        </div>
        <div>
            <asp:DropDownList ID="methodDropDownList" runat="server">
                <asp:ListItem>GET</asp:ListItem>
                <asp:ListItem>POST</asp:ListItem>
                <asp:ListItem>PUT</asp:ListItem>
                <asp:ListItem>DELETE</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div>
            <asp:Label ID="Label2" runat="server" Text="Request Body"></asp:Label>            
            <br />
            <asp:TextBox id="txtAreaRequestBody" TextMode="multiline" Columns="70" Rows="30" runat="server" />
            <br />
        </div>
        <div>
            <asp:Button ID="btnMakeRequest" runat="server" OnClick="btnMakeRequest_Click" Text="Make Request" />
            <br />
        </div>

         <div>
            <asp:Label ID="Label3" runat="server" Text="Request Response"></asp:Label>
            <br />
            <asp:TextBox id="txtAreaResult" TextMode="multiline" Columns="70" Rows="30" runat="server" />
            <br />
        </div>
        
    </div>
    </form>
</body>
</html>
