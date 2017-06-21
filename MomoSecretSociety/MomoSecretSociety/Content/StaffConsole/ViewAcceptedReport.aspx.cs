﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;
using Spire.Pdf.Security;

namespace MomoSecretSociety.Content.StaffConsole
{
    public partial class TestDisplay : System.Web.UI.Page
    {
        static SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["FileDatabaseConnectionString2"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                ((Label)Master.FindControl("lastLoginStaff")).Text = "Your last logged in was <b>"
                            + ActionLogs.getLastLoggedInOf(Context.User.Identity.Name) + "</b>";
            }

            if (IsPostBack)
            {
                errormsgPasswordAuthenticate.Visible = false;
            }

            //This should be on click of the particular report then will appear
            string dbCaseNumber = "";
            string dbUsername = "";
            string dbDate = "";
            string dbSubject = "";
            string dbDescription = "";
            string dbRemarks = "";
            string dbReportStatus = "";

            connection.Open();
            SqlCommand myCommand = new SqlCommand("SELECT CaseNumber, Username, Date, Subject, Description, Remarks, ReportStatus FROM Report WHERE CaseNumber = @caseNo", connection);
            myCommand.Parameters.AddWithValue("@caseNo", 201700001); //Hardcoded the case number - next time change to auto input when onclick of the particular report
            SqlDataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                dbCaseNumber = (myReader["CaseNumber"].ToString());
                dbUsername = (myReader["Username"].ToString());
                dbDate = (myReader["Date"].ToString());
                dbSubject = (myReader["Subject"].ToString());
                dbDescription = (myReader["Description"].ToString());
                dbRemarks = (myReader["Remarks"].ToString());
                dbReportStatus = (myReader["ReportStatus"].ToString());

            }

            connection.Close();

            Label2.Text = dbCaseNumber + " -";
            Label4.Text = dbDate;
            Label6.Text = dbUsername;
            Label8.Text = dbSubject;
            Label10.Text = dbDescription;
            Label12.Text = dbRemarks;
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


        public static string dbCaseNumber = "";
        public static string dbUsername = "";
        public static string dbDate = "";
        public static string dbSubject = "";
        public static string dbDescription = "";
        public static string dbRemarks = "";
        public static string dbCreatedDateTime = "";

        protected void btnSaveAsPDF_Click(object sender, EventArgs e)
        {
            string inputUsername = Session["AccountUsername"].ToString();
            string rStatus = "accepted";

            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["FileDatabaseConnectionString2"].ConnectionString);
            connection.Open();
            SqlCommand myCommand = new SqlCommand("SELECT CaseNumber, Username, Date, Subject, Description, Remarks, CreatedDateTime FROM Report WHERE Username = @AccountUsername AND ReportStatus = @reportStatus" , connection);
            myCommand.Parameters.AddWithValue("@AccountUsername", inputUsername); //Taking the latest report of that user only. //Should be click on a particular report number - thats the report that we should take
            myCommand.Parameters.AddWithValue("@reportStatus", rStatus);

            SqlDataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                dbCaseNumber = (myReader["CaseNumber"].ToString());
                dbUsername = (myReader["Username"].ToString());
                dbDate = (myReader["Date"].ToString());
                dbSubject = (myReader["Subject"].ToString());
                dbDescription = (myReader["Description"].ToString());
                dbRemarks = (myReader["Remarks"].ToString());
                dbCreatedDateTime = (myReader["CreatedDateTime"].ToString());

            }
            connection.Close();

            //Creating a pdf document
            PdfDocument doc = new PdfDocument();

            //Create a page
            PdfPageBase page = doc.Pages.Add();

            
            //Draw the contents of page
            AlignText(page);

            //DigitalSignature (KaiTat)

            string wmText = "Report #" + dbCaseNumber + " by " + dbUsername;

            // + Watermark (text) -> DrawString(string s, PdfFontBase font, PdfBrush brush, float x, float y, PdfStringFormat format)
            PdfTilingBrush brush = new PdfTilingBrush(new SizeF(page.Canvas.ClientSize.Width / 2, page.Canvas.ClientSize.Height / 3));
            brush.Graphics.SetTransparency(0.3f);
            brush.Graphics.Save();
            brush.Graphics.TranslateTransform(brush.Size.Width / 2, brush.Size.Height / 2);
            brush.Graphics.RotateTransform(-45);
            brush.Graphics.DrawString(wmText, new PdfFont(PdfFontFamily.Helvetica, 20), PdfBrushes.Black, 0, 0, new PdfStringFormat(PdfTextAlignment.Center));
            brush.Graphics.Restore();
            brush.Graphics.SetTransparency(1);
            page.Canvas.DrawRectangle(brush, new RectangleF(new PointF(1, 1), page.Canvas.ClientSize));

            //Save pdf to a location
            doc.SaveToFile("C:\\Users\\User\\Desktop\\CreatePDFTest" + dbCaseNumber + ".pdf");


        }

        private static void AlignText(PdfPageBase page)
        {
            float x1 = 20;
            float y1 = 50;
            float x2 = 90;
            string text = "";
            float pageWidth = page.Canvas.ClientSize.Width;

            //Title
            PdfBrush brush1 = new PdfSolidBrush(Color.Black);
            PdfTrueTypeFont font1 = new PdfTrueTypeFont(new Font("Arial", 16f, FontStyle.Bold));
            PdfStringFormat format1 = new PdfStringFormat(PdfTextAlignment.Center);
            //format2.CharacterSpacing = 1f;
            text = "Report Case #" + dbCaseNumber;
            page.Canvas.DrawString(text, font1, brush1, pageWidth / 2, 10, format1);
            
            //Draw the text - alignment
            PdfFont font2 = new PdfFont(PdfFontFamily.Helvetica, 10f);
            PdfTrueTypeFont font3 = new PdfTrueTypeFont(new Font("Helvetica", 10f, FontStyle.Bold));
            PdfSolidBrush brush = new PdfSolidBrush(Color.Black);
            PdfStringFormat leftAlignment = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);

            //DATE
            page.Canvas.DrawString("Date: ", font2, brush, x1, y1, leftAlignment);
            page.Canvas.DrawString(dbDate, font3, brush, x2, y1, leftAlignment);
            y1 = y1 + 30;

            //FROM
            page.Canvas.DrawString("From: ", font2, brush, x1, y1, leftAlignment);
            page.Canvas.DrawString(dbUsername, font3, brush, x2, y1, leftAlignment);
            y1 = y1 + 30;

            //SUBJECT
            page.Canvas.DrawString("Subject: ", font2, brush, x1, y1, leftAlignment);
            page.Canvas.DrawString(dbSubject, font3, brush, x2, y1, leftAlignment);
            y1 = y1 + 30;

            //PdfWordWrapType wordWrap;
            //BreakLine(dbDescription, 90, 515);

            //CASE DESCRIPTION
            page.Canvas.DrawString("Case Description: ", font2, brush, x1, y1, leftAlignment);
            y1 = y1 + 30;
            page.Canvas.DrawString(dbDescription, font3, brush, x2, y1, leftAlignment);
            y1 = y1 + 30;

            //if (dbDescription.Length.Equals(515))
            //{
            //    string[] words = dbDescription.Split(' ');
            //    foreach (string word in words)
            //    {
            //    }
            //}

            //REMARKS
            page.Canvas.DrawString("Remarks: ", font2, brush, x1, y1, leftAlignment);
            y1 = y1 + 30;
            page.Canvas.DrawString(dbRemarks, font3, brush, x2, y1, leftAlignment);

            //WIDTH 515 HEIGHT 762
            //To print out the size of the whole page
            /*
            y1 = y1 + 30;
            SizeF size = page.Canvas.ClientSize;
            string sizeText = size.ToString();
            page.Canvas.DrawString(sizeText, font3, brush, x2, y1, leftAlignment);
            */

        }

        //I need to add breakline 
        private static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max;
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;

            // If no whitespace found, break at maximum length
            if (i < 0)
                return max;

            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;

            // Return length of text before whitespace
            return i + 1;
        }
    }
}