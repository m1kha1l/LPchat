using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Identity;
using Microsoft.AspNetCore.Server.HttpSys;
using System;
using System.Data;
using static LPchat.MsgStorage;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System.Web;


namespace LPchat.Pages
{
    public class OperatorpageModel : PageModel
    {
        private readonly ILogger<OperatorpageModel> _logger;
        private List<string> processingAIresponces = new List<string>();

        int lastapiindex = 0;
        public struct chatsInfo
        {

            public string username = "";
            public bool isThereNewMsgs = false;
         
            private bool localVersion = true;
            public DateTime lastDateTime;

            
            public chatsInfo(string username, bool isThereNewMsgs, DateTime dt)
            {
                this.username = username;
                this.isThereNewMsgs = isThereNewMsgs;
                this.lastDateTime = dt;
            }
        }

        public OperatorpageModel(ILogger<OperatorpageModel> logger)
        {
            _logger = logger;

        }
        public void OnGet()
        {
        }

        List<chatsInfo> getChatInfoLoc(string usernameSelected)
        {
            Dictionary<string,chatsInfo> result = new Dictionary<string,chatsInfo>();
            List<Messages> res = new List<Messages>();

            List<chatsInfo> resultToReturn=new List<chatsInfo>();
            try
            {
                string connectionString = "Data Source=.;Initial Catalog=lpchat; Integrated Security=True; TrustServerCertificate=True;";
                if (myDBmode.isRemote)
                {
                    connectionString = "Data Source=ms-sql-10.in-solve.ru; Initial Catalog = 1gb_lpchat; User ID =1gb_chgo1; Password=faaa3a9crty;";
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    //getting size of usernamebase array
                    string query = "SELECT * FROM dbo.messages";
                    SqlCommand command = new SqlCommand(query, conn);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                         //   res.Add(new Messages(reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString()));
                            if (reader[3].ToString() == "operator")
                            {
                                //if (!result.Contains(new chatsInfo(reader[4].ToString(), true)) && !result.Contains(new chatsInfo(reader[4].ToString(), false)))
                                bool readByOperator = false;
                                if (reader[5].ToString() == "True")
                                {
                                    readByOperator = true;
                                }
                                else
                                {
                                    readByOperator = false;
                                    string lastMessageText = reader[1].ToString();
                                    if (lastMessageText == null) { lastMessageText = ""; }
                                    getOpenAIresponce(lastMessageText, reader[3].ToString(), reader[4].ToString());
                                }

                                if (!result.ContainsKey(reader[4].ToString()))
                                {                                   
                                    result.Add(reader[4].ToString(),new chatsInfo(reader[4].ToString(), !readByOperator, DateTime.Parse(reader[2].ToString())));
                                }
                                else
                                {
                                    bool isThereNewMsgs = result[reader[4].ToString()].isThereNewMsgs;
                                    if(!isThereNewMsgs) { isThereNewMsgs = !readByOperator; }
                                    result[reader[4].ToString()] = new chatsInfo(reader[4].ToString(), isThereNewMsgs, DateTime.Parse(reader[2].ToString()));
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch
            {

            }
            List<chatsInfo> listWithNewMsgs = new List<chatsInfo>();
            List<chatsInfo> listWithoutNewMsgs = new List<chatsInfo>();
            chatsInfo selectedChat;
            foreach (var element in result)
            {
                if (element.Key != usernameSelected)
                {
                    if (element.Value.isThereNewMsgs)
                    {
                        listWithNewMsgs.Add(element.Value);
                    }
                    else
                    {
                        listWithoutNewMsgs.Add(element.Value);
                    }
                }
                else
                {
                    selectedChat = element.Value;
                }
            }
            listWithNewMsgs.OrderBy(x => x.lastDateTime);
            listWithoutNewMsgs.OrderBy(x => x.lastDateTime);
            if ((usernameSelected != "")&&(usernameSelected!=null)) { resultToReturn.Add(result[usernameSelected]); }
            resultToReturn.AddRange(listWithNewMsgs);
            resultToReturn.AddRange(listWithoutNewMsgs);
            return resultToReturn;
        }
        public async Task getOpenAIresponce(string message, string fromUser, string toUser)
        {
            if (!processingAIresponces.Contains(toUser))
            {
                processingAIresponces.Add(toUser);
                await Task.Run(() =>
                {
                    MarkMessagesAsRead(fromUser);
                    string res = new openAIcommunication().communicate(message);
                    int lastapiindex1=Convert.ToInt32(res.Split("%#")[0]);
                    lastapiindex = lastapiindex1;
                    res = res.Split("%#")[1];
                    if (res != "")
                    {
                        InsertMessage(res, fromUser, toUser);
                    }
                    processingAIresponces.Remove(toUser);
                });
            }          
          //  return res;
        }
        public IActionResult OnPostGetChatGPTresponse()
        {
            string mymsg = "Hello!";
          //  string res = getOpenAIresponce(mymsg);
            JsonResult result = new JsonResult(mymsg);
            return result;
        }
        public IActionResult OnPostGetChatInfo(string selectedusername,int globallastapiindex)
        {
            lastapiindex = lastapiindex;
            List<chatsInfo> chatinfo = getChatInfoLoc(selectedusername);
            string chatinfoTable = "";

            foreach (var r in chatinfo)
            {
                if (r.username != selectedusername)
                {
                    if (r.isThereNewMsgs)
                    {
                        chatinfoTable += "<li class=\"userhasnewmsg\"><button onclick=\"Selectuser('" + r.username + "')\" style=\"width:100%; border-style:none; background-color: transparent;\"  >" + r.username + "</button></li>";
                    }
                    else
                    {
                        chatinfoTable += "<li class=\"useritem\"><button onclick=\"Selectuser('" + r.username + "')\" style=\"width:100%; border-style:none; background-color: transparent;\" >" + r.username + "</button></li>";
                    }
                }
                else
                {
                    chatinfoTable = "<li class=\"userselected\"><button onclick=\"Selectuser('" + r.username + "')\" style=\"width:100%; border-style:none; background-color: transparent;\" >" + r.username + "</button></li>" + chatinfoTable;
                }
            }

            JsonResult result = new JsonResult(lastapiindex+ "%#" + chatinfoTable);
            return result;
        }
        public IActionResult OnPostGetMessages()
        {
            string res = "";
            JsonResult result = new JsonResult(res);
            return result;
        }
        public IActionResult OnPostSwitchText(string inputText)
        {
            string res = inputText;

            res = res.Replace("class=\"response\"", "class=\"responseJunk\"");
            res = res.Replace("class=\"mymessage\"", "class=\"response\"");
            res = res.Replace("class=\"responseJunk\"", "class=\"mymessage\"");
            JsonResult result = new JsonResult(res);
            return result;
        }
        public void MarkMessagesAsRead(string username)
        {
            try
            {
                string connectionString = "Data Source=.;Initial Catalog=lpchat; Integrated Security=True; TrustServerCertificate=True;";
                if (myDBmode.isRemote)
                {
                    connectionString = "Data Source=ms-sql-10.in-solve.ru; Initial Catalog = 1gb_lpchat; User ID =1gb_chgo1; Password=faaa3a9crty;";
                }
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    //getting size of usernamebase array
                    string query = "UPDATE dbo.messages SET readByOperator=@val WHERE FromAddr=@usernm OR ToAddr=@usernm";
                    SqlCommand command = new SqlCommand(query, conn);
                    command.Parameters.AddWithValue("@val", "1");
                    command.Parameters.AddWithValue("@usernm", username);
                    command.ExecuteNonQuery();
                    /*using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            res.Add(new Messages(reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString()));
                        }
                    }*/
                    conn.Close();
                }
            }
            catch
            {

            }
        }
        public void OnPostMarkMessagesAsRead(string username)
        {
            MarkMessagesAsRead(username);
        }

        public void InsertMessage(string msg, string fromuser, string touser)
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

    }
}
