﻿using Microsoft.AspNet.Identity;
using MomoSecretSociety.Content;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MomoSecretSociety
{
    public partial class SiteMaster : MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

        SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["FileDatabaseConnectionString2"].ConnectionString);

        protected void Page_Init(object sender, EventArgs e)
        {
            //// The code below helps to protect against XSRF attacks
            //var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            //Guid requestCookieGuidValue;
            //if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            //{
            //    // Use the Anti-XSRF token from the cookie
            //    _antiXsrfTokenValue = requestCookie.Value;
            //    Page.ViewStateUserKey = _antiXsrfTokenValue;
            //}
            //else
            //{
            //    // Generate a new Anti-XSRF token and save to the cookie
            //    _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
            //    Page.ViewStateUserKey = _antiXsrfTokenValue;

            //    var responseCookie = new HttpCookie(AntiXsrfTokenKey)
            //    {
            //        HttpOnly = true,
            //        Value = _antiXsrfTokenValue
            //    };
            //    if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
            //    {
            //        responseCookie.Secure = true;
            //    }
            //    Response.Cookies.Set(responseCookie);
            //}

            //Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            //if (!IsPostBack)
            //{
            //    // Set Anti-XSRF token
            //    ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
            //    ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            //}
            //else
            //{
            //    // Validate the Anti-XSRF token
            //    if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
            //        || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
            //    {
            //        throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
            //    }

            //}
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Request.IsAuthenticated)
            //{
            //    ((Label)FindControl("lastLogin")).Text = "Your last logged in was <b>"
            //                + ActionLogs.getLastLoggedInOf(Context.User.Identity.Name) + "</b>";
            //}

            

        }

        protected void Unnamed_LoggingOut(object sender, LoginCancelEventArgs e)
        {
         
            Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            Session["AccountUsername"] = Context.User.Identity.Name;
            //Add to logs
            ActionLogs.Action action = ActionLogs.Action.Logout;
            ActionLogs.Log(Session["AccountUsername"].ToString(), action);

            connection.Open();
            SqlCommand updateFirstLoginAccess = new SqlCommand("UPDATE UserAccount SET isFirstTimeAccessed = @isFirstTimeAccessed WHERE Username = @AccountUsername", connection);
            updateFirstLoginAccess.Parameters.AddWithValue("@isFirstTimeAccessed", "0");
            updateFirstLoginAccess.Parameters.AddWithValue("@AccountUsername", Session["AccountUsername"].ToString());
            updateFirstLoginAccess.ExecuteNonQuery();
            connection.Close();


        }





        //protected void logNavBar_Click(object sender, EventArgs e)
        //{
        //    if (Request.IsAuthenticated)
        //    {
        //        if (Context.User.Identity.Name != "boss")
        //        {
        //            Page.ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('You are not authorized to do so.');", true);
        //        }
        //    }
        //}


    }
}