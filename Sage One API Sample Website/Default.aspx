<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Sage_One_API_Sample_Website._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Getting Started with the Sage One API </h1>
        <p class="lead">Details on the OAuth process and available resources can be found in the documentation area together with examples of how to compose requests and what responses look like</p>
         <address>
            <strong>Developer:</strong>   <a href="https://developers.sageone.com/">Developer Support Site</a><br />
         </address>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Stage 1 - Authorisation</h2>
            <p>
                Use OAuth 2.0 to allow Sage One users to authorize third party applications to access their data without sharing their actual login details.
            </p>

            <asp:Button OnClick="button_authorise_onclick" Text="Authorise" runat="server" />
        </div>
    </div>

</asp:Content>
