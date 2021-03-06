﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MomoSecretSociety.Content.StaffConsole
{
    public partial class ViewSelectedReport : System.Web.UI.Page
    {
        static SqlConnection viewSelectedReportsDetailsConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["FileDatabaseConnectionString2"].ConnectionString);


        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                string abc = "";

                viewSelectedReportsDetailsConnection.Open();
                SqlCommand selectedDetailsCommand = new SqlCommand("SELECT * FROM Report WHERE CaseNumber = @CaseNumber", viewSelectedReportsDetailsConnection);
                selectedDetailsCommand.Parameters.AddWithValue("@CaseNumber", Session["caseNumberOfThisSelectedReport"].ToString());


                SqlDataReader selectedReportsDetailsReader = selectedDetailsCommand.ExecuteReader();

                while (selectedReportsDetailsReader.Read())
                {
                    //let say i retrieve username
                    Label2.Text = selectedReportsDetailsReader["CaseNumber"].ToString();
                    Label6.Text = selectedReportsDetailsReader["Date"].ToString();
                    Label4.Text = selectedReportsDetailsReader["Username"].ToString();
                    Label8.Text = selectedReportsDetailsReader["Subject"].ToString();
                    Label10.Text = selectedReportsDetailsReader["Description"].ToString();
                    Label12.Text =selectedReportsDetailsReader["Remarks"].ToString();

                }
                viewSelectedReportsDetailsConnection.Close();

                Response.Write(abc);
            }
        }



    }
}