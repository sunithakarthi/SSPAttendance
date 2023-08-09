using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Web.UI.WebControls;

namespace ACMESSPAttendance
{
    public class Utility
    {
        public static string SessionUserNameKey = "CurrentLoginUser";
        public const int SessionStringExpireSeconds = 36000; //10 hours Expired
         
        public static string LetterOrDigitString(string unsafeStr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < unsafeStr.Length; i++)
            {
                if (char.IsLetterOrDigit(unsafeStr, i))
                {
                    sb.Append(unsafeStr[i]);
                }
            }
            return sb.ToString();
        }

        //cmd = new SqlCommand(sql, conn);
        //cmd.CommandType = CommandType.Text;

        //public static List<int> GenerateRandom(int count)
        //{
        //    List<int> result = new List<int>(count);

        //    // generate count random values.
        //    HashSet<int> candidates = new HashSet<int>();
        //    for (Int32 top = Int32.MaxValue - count; top < Int32.MaxValue; top++)
        //    {
        //        // May strike a duplicate.
        //        int value = Random(top + 1);
        //        if (candidates.Add(value))
        //        {
        //            result.Add(value);
        //        }
        //        else
        //        {
        //            result.Add(top);
        //            candidates.Add(top);
        //        }
        //    }

        //    return result;
        //}


        public static string Readdatafromtextdocument()
        {
            List<string> list = new List<string>();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString);
            conn.Open();
            var textfilepath= System.Configuration.ConfigurationManager.AppSettings["LandingPageText"].ToString();
            var reader = File.ReadAllLines(textfilepath);

            list.AddRange(reader);

            List<int> possible = Enumerable.Range(1, list.Count).ToList();
            string listpagecontent = null;
            var currentdatetime = DateTime.Now; 
            string sqlselect = "select top 1 CreatedDate from RandomHomePageContent order by CreatedDate desc ";
            SqlCommand cmd = new SqlCommand(sqlselect, conn);

            var lastUpdatedDatetime =!string.IsNullOrEmpty(Convert.ToString(cmd.ExecuteScalar()))?Convert.ToDateTime( Convert.ToString(cmd.ExecuteScalar())):DateTime.Now.AddHours(-24);

           // var timedifference = currentdatetime-lastUpdatedDatetime;
            TimeSpan ts = currentdatetime - lastUpdatedDatetime;
            if (ts.TotalHours>=24)
            {

                Random rand = new Random();
                int index = rand.Next(0, possible.Count);
                listpagecontent = list[possible[index]];  


                string sql = "Insert into RandomHomePageContent(Contentvalue, CreatedDate, IsActive) " +
                     " Values(@Contentvalue, @CreatedDate, @IsActive);";
                cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@Contentvalue", System.Data.SqlDbType.Int, 15).Value = index;
                cmd.Parameters.Add("@CreatedDate", System.Data.SqlDbType.DateTime, 255).Value = DateTime.Now;
                cmd.Parameters.Add("@IsActive", System.Data.SqlDbType.Bit).Value = 1;
                cmd.ExecuteNonQuery();
            }
            else
            {
                string selectPreviousIndexValue = "select top 1 Contentvalue from RandomHomePageContent order by CreatedDate desc ";
                cmd = new SqlCommand(selectPreviousIndexValue, conn);

                var previousIndexvalue = Convert.ToInt32(cmd.ExecuteScalar());

                listpagecontent = list[previousIndexvalue];
            }
            

            //    var textcontent = list[possible[index]];
            //for (int i = 0; i < 3; i++)
            //{
            //    int index = rand.Next(0, possible.Count);
            //    var textcontent = list[possible[index]];
            //    possible.RemoveAt(index);
            //    listpagecontent.Add(textcontent);
            //}
            //do
            //{
            //    int index = rand.Next(0, possible.Count);
            //}while(index)
            

            return listpagecontent;

        }

        public const string splitline = "- ";

        public static void SetDataListText(DataList dl, string text)
        {
            text = text.TrimStart('-');
            if (text.Trim() == string.Empty)
                return;
            ArrayList alText = new ArrayList();
            int startIndex = 0;
            int index = text.IndexOf(splitline, startIndex);
            while (index >= startIndex)
            {
                alText.Add(text.Substring(startIndex, index - startIndex));
                startIndex = index + splitline.Length;
                index = text.IndexOf(splitline, startIndex);
            }
            alText.Add(text.Substring(startIndex));
            dl.DataSource = alText;
            dl.DataBind();
        }

        public static string GetFontFamilyName()
        {
            string fontfamilyname = "\"Frutiger\", Helvetica, Arial, sans-serif !important";

            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    Console.WriteLine("I'm on windows!");
                    fontfamilyname = "\"Frutiger\", Helvetica, Arial, sans-serif !important";
                    break;
                case PlatformID.Unix:
                    Console.WriteLine("I'm a linux box!");
                    fontfamilyname = "\"Frutiger\", Helvetica, Arial, sans-serif !important";
                    break;
                case PlatformID.MacOSX:
                    Console.WriteLine("I'm a mac!");
                    fontfamilyname = "Arial !important";
                    break;
                default:
                    Console.WriteLine("No Idea what I'm on!");
                    fontfamilyname = "\"Frutiger\", Helvetica, Arial, sans-serif !important";
                    break;
            }
            return fontfamilyname;
        }
    }
}