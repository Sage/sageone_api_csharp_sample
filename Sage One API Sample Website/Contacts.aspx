<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Contacts.aspx.cs" Inherits="Sage_One_API_Sample_Website.Contacts" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div class="row">
        <div class="col-md-4">
            <h2>List Contacts</h2>
            <p>
               List of Sage One Contacts
            </p>

            <br />
             <asp:ListBox ID="ListBoxContacts" runat="server" Height="300px" AutoPostBack="True" Width="800px">
             </asp:ListBox>
        </div>
    </div>
    </form>
</body>
</html>
