using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Atlassian.Jira;
using System.Threading;

namespace EmailToJira
{
    public class Data
    {
        private String login;
        public StreamWriter log;
        public Jira jira;
        public Tickets jiraConn;
        private String logTime;

        public String Login { get { return login; } set { login = value; } }
        public String LogTime { get { return logTime; } set { logTime = value; } }

        public Data() { }

        public Data(StreamWriter log, Jira jira, Tickets jiraConn, String Login, String LogTime)
        {
            this.log = log;
            this.jira = jira;
            this.jiraConn = jiraConn;
            login = Login;
            logTime = LogTime;
        }


        public void CheckEmails()
        {
            log = File.AppendText("logs/log_" + LogTime + ".txt");
            var mails = OutlookEmails.ReadMailItems(log, jira, jiraConn, login, logTime);
            if (mails.Count > 0)
            {
                if (mails.Count != 1)
                {
                    Console.WriteLine(DateTime.Now + "\t" + mails.Count + " new emails.");
                    log.WriteLine(DateTime.Now + "\t" + mails.Count + " new emails.");
                }
                else
                {
                    Console.WriteLine(DateTime.Now + "\t" + mails.Count + " new email.");
                    log.WriteLine(DateTime.Now + "\t" + mails.Count + " new email.");
                }
            }
            int j = 1;
            foreach (var mail in mails)
            {
                Console.WriteLine(DateTime.Now + "\tEmail subject: " + mail.Subject);
                log.WriteLine(DateTime.Now + "\tEmail subject: " + mail.Subject);
                j++;
            }
            log.Close();
            Thread.Sleep(1000*30);
        }
    }
}
