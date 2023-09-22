using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Server.HttpSys;
using System;
using System.Data;

using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.AspNetCore.Components.Routing;
using System.Text.Json;
//using Microsoft.Data.SqlClient;


//TODO List

//publish new version

//add database of simple phrases
//add mechanism of typing simple messages 1 in a minute

//operator page
//highlight if there is a unread message
//sort userlist by newer messages
//


// 
// push to freeasphosting.net I guess there is a free account for ASP.NET order 1

namespace LPchat.Pages
{    
    public class IndexModel : PageModel
    {
        
        public string Message { get; set; }
        private readonly ILogger<IndexModel> _logger;
        public CustomersPayload Filters=new CustomersPayload();
        
        private string username = "";
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
          
        }

        

        public void InsertMessage(string msg,string fromuser, string touser )
        {
            try
            {
                if (fromuser.Length + touser.Length <= 0) { return; } //check if there is no input usernames
                       
                //string connectionString = "Data Source=.;Initial Catalog=lpchat;Integrated Security=True;Pooling=False";
                string connectionString = "Data Source=.;Initial Catalog=lpchat; Integrated Security=True; TrustServerCertificate=True;";
                if (myDBmode.isRemote)
                {
                    connectionString = "Data Source=ms-sql-10.in-solve.ru; Initial Catalog = 1gb_lpchat; User ID =1gb_chgo1; Password=faaa3a9crty;";
                }

                // rest of the code
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    //  conn.OpenAsync();

                    conn.Open();
                    // other code goes here

                    String query;
                    SqlCommand command;
                    query = "INSERT INTO dbo.messages (Msg,Datetime,ToAddr,FromAddr) VALUES (@Msg,@Datetime, @ToAddr, @FromAddr)";

                    command = new SqlCommand(query, conn);
                    command.Parameters.AddWithValue("@Msg", msg);
                    command.Parameters.Add("@Datetime", SqlDbType.DateTime).Value = DateTime.Now;
                    command.Parameters.AddWithValue("@ToAddr", touser);
                    command.Parameters.AddWithValue("@FromAddr", fromuser);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch
            {

            }
        }
        public void OnGet()
        {
           
        }
        public void OnPostSendMsg()
        {
            //    int a = 1;
            var myMsg = Request.Form["myMsg"];
            this.Message = myMsg;

            InsertMessage(myMsg,username,"operator");          
        }

        public IActionResult OnPostGetLanding()
        {
            List<String> landings = new List<string>();

            string connectionString = "Data Source=.;Initial Catalog=lpchat; Integrated Security=True; TrustServerCertificate=True;";
            if (myDBmode.isRemote)
            {
                connectionString = "Data Source=ms-sql-10.in-solve.ru; Initial Catalog = 1gb_lpchat; User ID =1gb_chgo1; Password=faaa3a9crty;";
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                //getting size of usernamebase array
                string query = "SELECT Landing FROM dbo.LP";
                SqlCommand command = new SqlCommand(query, conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        landings.Add(reader[0].ToString());
                    }
                }
                conn.Close();
            }
            int j = 0;
            Random rnd=new Random();
            j = rnd.Next(0, landings.Count-1);
            return new JsonResult(new { landingtext = landings[j], landingid =j});
        }
        public void OnPostLPidToDB(string username,int lpid)
        {
            
            //string connectionString = "Data Source=.;Initial Catalog=lpchat;Integrated Security=True;Pooling=False";
            string connectionString = "Data Source=.;Initial Catalog=lpchat; Integrated Security=True; TrustServerCertificate=True;";
            if (myDBmode.isRemote)
            {
                connectionString = "Data Source=ms-sql-10.in-solve.ru; Initial Catalog = 1gb_lpchat; User ID =1gb_chgo1; Password=faaa3a9crty;";
            }

            // rest of the code
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                //  conn.OpenAsync();

                conn.Open();
                // other code goes here

                String query;
                SqlCommand command;
                query = "INSERT INTO dbo.LPusernames (Username,LPid) VALUES (@username,@lpid)";

                command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@lpid", lpid);               
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

            //do like this
            //https://www.aspsnippets.com/Articles/Using-the-asp-page-handler-attribute-in-ASPNet-Core-Razor-Pages.aspx
            public IActionResult OnGetExport(CustomersPayload filter)
        {
            return Page();
        }
        public class CustomersPayload
        {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
        }
        public IActionResult OnPostGetMessages(string username)
        {
            string res = "";
            JsonResult result;
            MsgStorage ms = new MsgStorage();
            ms.readMutualMessages(username);
            res=ms.prepareOutput();
            result = new JsonResult(res);
            return result;
        }
            public IActionResult OnPostGetAjax(string msg,string username)
        {
            var myMsg = msg;
            this.Message = myMsg;

            InsertMessage(myMsg, username,"operator");


            return null;
        }
        public IActionResult OnPostInsertMnlMessage(string msg, string fromusername,string tousername)
        {
            var myMsg = msg;
            this.Message = myMsg;

            InsertMessage(myMsg, fromusername, tousername);


            return null;
        }
        private List<string> getOccupiedUsernames()
        {
            List<string> usernames = new List<string>();


            string connectionString = "Data Source=.;Initial Catalog=lpchat; Integrated Security=True; TrustServerCertificate=True;";
            if (myDBmode.isRemote)
            {
                connectionString = "Data Source=ms-sql-10.in-solve.ru; Initial Catalog = 1gb_lpchat; User ID =1gb_chgo1; Password=faaa3a9crty;";
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                //getting size of usernamebase array
                string query = "SELECT FromAddr FROM dbo.messages";
                SqlCommand command = new SqlCommand(query, conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usernames.Add(reader[0].ToString());                        
                    }
                }
                conn.Close();
            }
        

            return usernames;
        }
        private string generateUsername()
        {
            string res = "";


            string user = "";
            string userbase = "";
            int i = 3;

            Random r = new Random();
            i = r.Next(1, 20);


            string connectionString = "Data Source=.;Initial Catalog=lpchat; Integrated Security=True; TrustServerCertificate=True;";
            if (myDBmode.isRemote)
            {
                connectionString = "Data Source=ms-sql-10.in-solve.ru; Initial Catalog = 1gb_lpchat; User ID =1gb_chgo1; Password=faaa3a9crty;";
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    //getting size of usernamebase array
                    SqlCommand command = new SqlCommand("SELECT TOP(1) id FROM dbo.usernamebase ORDER BY 1 DESC", conn);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int maxId = Convert.ToInt32(reader[0]);
                            i = r.Next(1, maxId);
                        }
                    }


                    command = new SqlCommand("Select username from dbo.usernamebase where id=" + i.ToString(), conn);

                    // int result = command.ExecuteNonQuery();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userbase = reader[0].ToString();
                        }
                    }
                 
                    conn.Close();
                    List<string> occupiedUsernames = getOccupiedUsernames();
                    while (occupiedUsernames.Contains(user = userbase + r.Next(1, 1000).ToString())) ;
                    //user = user + r.Next(1, 1000).ToString();
                }
            }
            catch
            {

            }


            return user;
        }
        public IActionResult OnPostGetUsername()
        {

            string user = generateUsername();
            JsonResult result = new JsonResult(user);
           // result = this.Json(JsonConvert.SerializeObject(data), JsonRequestBehavior.AllowGet);
            return result;
           // return user;
        }
        public IActionResult OnPostGetUsername2()
        {
            JsonResult result = new JsonResult("");
            // result = this.Json(JsonConvert.SerializeObject(data), JsonRequestBehavior.AllowGet);
            return result;
        }
        public IActionResult OnPostGetWakeUpMessage()
        {
            JsonResult result = new JsonResult("");
            List<string> list = new List<string>();
            /*  list.Add("Приветик");
              list.Add("Расскажи о себе");
              list.Add("Как ты?");
              list.Add("Улыбнись)");
              list.Add("Пообщаемся?");
              list.Add("Познакомимся?");
              list.Add("Как тебя зовут?");
              list.Add("Как тебя зовут?");*/
            list.Add("Hey, how are u?");
            list.Add("What's your name?");
            list.Add("Bored? Let's chat");
            list.Add("I wanne meet with you");
            int i = 0;

            Random r = new Random();
            i = r.Next(0, list.Count-1);


            return new JsonResult(list[i]); ;
        }
    }

    static class myDBmode
    {
        public static bool isRemote=true;
    }
}
