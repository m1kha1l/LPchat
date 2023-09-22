using LPchat.Pages;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace LPchat
{
    public static class Extensions
    {
        public static void Swap<T>(this List<T> list, int i, int j)
        {
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public class MsgStorage
    {
        public struct Messages
        {
            public string msg;
            public DateTime datetime;
            public string toAddr;
            public string fromAddr;
            public Messages(string msg, DateTime datetime, string toAddr,string fromAddr)
            {
                this.msg=msg;
                this.datetime=datetime;
                this.toAddr=toAddr;
                this.fromAddr = fromAddr;
            }
            public Messages(string msg, string datetime, string toAddr, string fromAddr)
            {
                this.msg = msg;
                this.datetime = Convert.ToDateTime(datetime);
                this.toAddr = toAddr;
                this.fromAddr = fromAddr;
            }
        }
        List<Messages> messages=new List<Messages>();

        public void addMessage(string msg, DateTime datetime, string toAddr, string fromAddr)
        {
            Messages mes=new Messages(msg, datetime, toAddr,fromAddr);
            messages.Add(mes);
        }
        public void readMutualMessages(string username)
        {
            
            List<Messages> operatorMsgList = new List<Messages>();
            List<Messages> userMsgList = new List<Messages>();

            operatorMsgList = readMessagesFromDV(username, "operator");
            userMsgList = readMessagesFromDV("operator", username);
            if(operatorMsgList.Count+ userMsgList.Count==0){

            }
            int i1 = 0;
            int i2 = 0;
            
            while((i1< operatorMsgList.Count) ||(i2< userMsgList.Count))
            {              
                if((i1< operatorMsgList.Count)&&(i2< userMsgList.Count))
                {
                    DateTime d1 = operatorMsgList[i1].datetime;
                    DateTime d2 = userMsgList[i2].datetime;
                    d1 = operatorMsgList[i1].datetime;
                    d2 = userMsgList[i2].datetime;

                    if (d1 > d2)
                    {
                        messages.Add(userMsgList[i2]);
                        i2++;
                    }
                    else
                    {
                        messages.Add(operatorMsgList[i1]);
                        i1++;
                    }
                }
                else
                {
                    if (i1 + i2 == 0)
                    {

                    }
                    if (i1 < operatorMsgList.Count)
                    {
                        messages.Add(operatorMsgList[i1]);
                        i1++;
                    }
                    if (i2 < userMsgList.Count)
                    {
                        messages.Add(userMsgList[i2]);
                        i2++;
                    }
                }
            }
        }
        public List<Messages> readMessagesFromDV(string toAddr,string fromAddr)
        {
            
            List<Messages> res = new List<Messages>();
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
                    string query = "SELECT * FROM dbo.messages WHERE (ToAddr='" + toAddr + "') AND (FromAddr='" + fromAddr + "') ORDER BY Datetime ASC";
                    SqlCommand command = new SqlCommand(query, conn);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            res.Add(new Messages(reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString()));
                        }
                    }
                    conn.Close();
                }
            }
            catch
            {

            }
            return res;
        }
        public void sortMessages()
        {
            for(int i=1; i<messages.Count; i++)
            {
                if (messages[i-1].datetime> messages[i].datetime) {
                    messages.Swap(i - 1, i);
                }
            }
        }
        public void checkDouble()
        {
            for (int i = 1; i < messages.Count; i++)
            {
                if (messages[i - 1].datetime == messages[i].datetime)
                {
                    messages.RemoveAt(i);
                }
            }
        }
        public string prepareOutput()
        {

            string res = "";
            foreach(var m in messages)
            {
                if (m.toAddr == "operator")
                {
                    res +="<div class=\"message text-only\"><div class=\"response\"><p class=\"text\">"+ m.msg+"</p></div></div>";
                }
                else
                {
                    res += "<div class=\"message text-only\"><div class=\"mymessage\"><p class=\"text\">" + m.msg + "</p></div></div>";
                }
            }
            return res;
        }
    }
}
