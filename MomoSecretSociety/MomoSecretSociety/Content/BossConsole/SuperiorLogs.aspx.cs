﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace MomoSecretSociety.Content.BossConsole
{
    public partial class SuperiorLogs : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                ((Label)Master.FindControl("lastLogin")).Text = "Your last logged in was <b>"
                            + ActionLogs.getLastLoggedInOf(Context.User.Identity.Name) + "</b>";

                readLogsRespectively();
            }


            if (IsPostBack)
            {
                errormsgPasswordAuthenticate.Visible = false;
            }
        }

        protected void btnAuthenticate_Click(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                string inputUsername = Context.User.Identity.Name;
                string inputPassword = txtPasswordAuthenticate.Text;

                string dbUsername = "";
                string dbPasswordHash = "";
                string dbSalt = "";

                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["FileDatabaseConnectionString2"].ConnectionString);

                connection.Open();
                SqlCommand myCommand = new SqlCommand("SELECT HashedPassword, Salt, Role, Username FROM UserAccount WHERE Username = @AccountUsername", connection);
                myCommand.Parameters.AddWithValue("@AccountUsername", inputUsername);

                SqlDataReader myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    dbPasswordHash = (myReader["HashedPassword"].ToString());
                    dbSalt = (myReader["Salt"].ToString());
                    dbUsername = (myReader["Username"].ToString());
                }
                connection.Close();

                string passwordHash = ComputeHash(inputPassword, new SHA512CryptoServiceProvider(), Convert.FromBase64String(dbSalt));

                if (dbUsername.Equals(inputUsername.Trim()))
                {
                    if (dbPasswordHash.Equals(passwordHash))
                    {
                        Page.ClientScript.RegisterStartupScript(GetType(), "alert", "$('#myModal').modal('hide')", true);
                    }
                    else
                    {
                        Page.ClientScript.RegisterStartupScript(GetType(), "alert", "$('#myModal').modal('show')", true);
                        errormsgPasswordAuthenticate.Visible = true;
                    }

                }
            }
        }

        public static String ComputeHash(string input, HashAlgorithm algorithm, Byte[] salt)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            Byte[] saltedInput = new Byte[salt.Length + inputBytes.Length];
            salt.CopyTo(saltedInput, 0);
            inputBytes.CopyTo(saltedInput, salt.Length);

            Byte[] hashedBytes = algorithm.ComputeHash(saltedInput);

            return BitConverter.ToString(hashedBytes);
        }
        private void readLogsRespectively()
        {
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["FileDatabaseConnectionString2"].ConnectionString);

            connection.Open();
            SqlDataReader dateReader = null;
            SqlCommand dateCommand = new SqlCommand("SELECT DISTINCT(convert(date, Timestamp)) AS Date FROM Logs WHERE Username = @AccountUsername ORDER BY convert(date,Timestamp) DESC", connection);

            dateCommand.Parameters.AddWithValue("@AccountUsername", Context.User.Identity.Name);
            dateReader = dateCommand.ExecuteReader();

            while (dateReader.Read())
            {
                DateTime date = (DateTime)dateReader["Date"];
                //Response.Write("Date : " + date + "<br>");
                AddDateToPlaceholder(date);

                SqlConnection connection2 = new SqlConnection(ConfigurationManager.ConnectionStrings["FileDatabaseConnectionString2"].ConnectionString);
                connection2.Open();
                SqlDataReader logReader = null;
                SqlCommand logCommand = new SqlCommand("SELECT Action, Timestamp FROM Logs WHERE Username = @AccountUsername AND convert(date, Timestamp) = convert(date,@Date) ORDER BY Timestamp ASC", connection2);

                logCommand.Parameters.AddWithValue("@AccountUsername", Context.User.Identity.Name);
                logCommand.Parameters.AddWithValue("@Date", date);
                logReader = logCommand.ExecuteReader();

                while (logReader.Read())
                {
                    string action = logReader["Action"].ToString();
                    DateTime actionDate = (DateTime)logReader["Timestamp"];
                    //Response.Write("Date : " + actionDate + " Action : " + action + "<br>");
                    AddActionToPlaceholder(action, actionDate);
                }
            }
        }


        private void AddActionToPlaceholder(string action, DateTime actionDate)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");

            HtmlGenericControl icon = new HtmlGenericControl("i");
            string iconStyle = "fa " + GetIconStyle(action);
            icon.Attributes.Add("class", iconStyle);
            li.Controls.Add(icon);


            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "timeline-item");

            HtmlGenericControl timeholder = new HtmlGenericControl("span");
            timeholder.Attributes.Add("class", "time");

            HtmlGenericControl timeIcon = new HtmlGenericControl("i");
            timeIcon.Attributes.Add("class", "fa fa-clock-o");
            timeholder.Controls.Add(timeIcon);
            timeholder.Controls.Add(new LiteralControl(actionDate.ToString()));
            container.Controls.Add(timeholder);

            HtmlGenericControl header = new HtmlGenericControl("h3");
            header.Attributes.Add("class", "timeline-header no-border");
            header.InnerHtml = action.ToString();
            container.Controls.Add(header);


            li.Controls.Add(container);
            phTimeline.Controls.Add(li);


        }

        private void AddDateToPlaceholder(DateTime date)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "time-label");

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.Attributes.Add("class", "bg-purple");
            span.InnerHtml = date.ToLongDateString();

            li.Controls.Add(span);
            phTimeline.Controls.Add(li);
        }


        private string GetIconStyle(string actionString)
        {
            if (actionString == "Register")
            {
                return "fa-registered bg-aqua";
            }
            else if (actionString == "Login")
            {
                return "fa-sign-in bg-aqua";
            }
            else if (actionString == "Logout")
            {
                return "fa-sign-out bg-aqua";
            }
            else if (actionString == "Profile Picture was changed")
            {
                return "fa-picture-o bg-aqua";
            }
            else if (actionString == "Password was changed")
            {
                return "fa-lock bg-green";
            }
            else if (actionString == "Password was reset")
            {
                return "fa-lock bg-red";
            }
            else if (actionString == "Email was changed")
            {
                return "fa-envelope bg-aqua";
            }
            else if (actionString == "Contact No was changed")
            {
                return "fa-phone bg-aqua";
            }
            else if (actionString == "Challenge was completed")
            {
                return "fa-bullseye bg-aqua";
            }
            else if (actionString == "Write up was submitted")
            {
                return "fa-file-text bg-aqua";
            }
            else if (actionString == "Feedback was submitted")
            {
                return "fa-edit bg-aqua"; //fa-commenting
            }
            else if (actionString == "2FA was disabled")
            {
                return "fa-mobile bg-red";
            }
            else if (actionString == "2FA was enabled")
            {
                return "fa-mobile bg-green";
            }
            else if (actionString == "Account was disabled")
            {
                return "fa-user bg-red";
            }
            else if (actionString == "Account was enabled")
            {
                return "fa-user bg-green";
            }

            return "fa-user bg-aqua";

        }

    }
}