using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atlassian.Jira;
using System.IO;
using System.Threading;

namespace EmailToJira
{
    public class Program
    {
        static void Main(string[] args)
        {
            String login;
            String password = "";


            Console.Write("Login: ");
            login = Console.ReadLine();
            Console.Write("Password: ");
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);


            // Create a writer and open the file:
            StreamWriter log;
            String logTime = DateTime.Now.ToString();
            logTime = logTime.Replace(" ", "_");
            logTime = logTime.Replace(":", "_");
            logTime = logTime.Replace(".", "_");
            if (!File.Exists("logs/log_"+logTime+".txt"))
            {
                log = new StreamWriter("logs/log_" + logTime + ".txt");
            }
            else
            {
                log = File.AppendText("logs/log_" + logTime + ".txt");
            }

            log.WriteLine(DateTime.Now + "\tUser "+login+" has logged in.");
            string[] settings = File.ReadAllLines(@"urls.config");
            Tickets jiraConn = new Tickets();
            Jira jira = Jira.CreateRestClient(settings[1], login, password);

            Console.WriteLine("\n");
            log.Close();
            Program.TopMenu(log, jiraConn, jira, login, logTime);

            Console.WriteLine("Program stopped.");
            log.WriteLine(DateTime.Now + "\tProgram stopped.");
            log.Close();
            Console.ReadLine();
        }

        public static void CheckEmails(StreamWriter log, Jira jira, Tickets jiraConn, String login, String logTime)
        {
            log = File.AppendText("logs/log_" + logTime + ".txt");
            var mails = OutlookEmails.ReadMailItems(log, jira, jiraConn, login, logTime);
            ToLog(log, "Checking for new e-mails...");
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
            else
            {
                ToLog(log, "No new emails.");
            }
            int j = 1;
            foreach (var mail in mails)
            {
                Console.WriteLine(DateTime.Now + "\tEmail subject: " + mail.Subject);
                log.WriteLine(DateTime.Now + "\tEmail subject: " + mail.Subject);
                j++;
            }
            ToLog(log, "Next check in 5 minutes.");
            log.Close();
            Thread.Sleep(300000);
            CheckEmails(log, jira, jiraConn, login, logTime);
        }

        public static void TopMenu(StreamWriter log, Tickets jiraConn, Jira jira, String login, String logTime)
        {
            Console.Clear();
            Console.WriteLine("\t\tAutomatic Jira ticket creation from UNREAD Bastion e-mails\t"+ DateTime.Now);
            Console.WriteLine("\t\t\tCreated by Sylwester Kwiatkowski, 2019\t\t\t" + login);

            MainMenu(log, jiraConn, jira, login, logTime);
        }

        public static void MainMenu(StreamWriter log, Tickets jiraConn, Jira jira, String login, String logTime)
        {
            Console.WriteLine("\n\n\n\t1. Show issue");
            Console.WriteLine("\t2. View 10 latest Jira tickets");
            Console.WriteLine("\t3. Tickets assigned to me");
            Console.WriteLine("\t4. Run job to create tickets");
            Console.WriteLine("\n\n\tE. Exit");
            Console.Write("\n\tMy choose: ");
            ConsoleKeyInfo choose = Console.ReadKey();

            if(choose.Key == ConsoleKey.E)
            {
                Console.WriteLine("\n\n\tPress any key to exit....");
                Console.ReadKey();
                Environment.Exit(0);
            }
            if (choose.Key == ConsoleKey.D1 || choose.Key == ConsoleKey.NumPad1)
            {
                Console.WriteLine("");
                Console.Write("Issue name(SWLORO-X): ");
                string issueName = "SWLORO-";
                issueName = issueName + Console.ReadLine();

                jiraConn.ShowIssue(jira, issueName);
                Return(log, jiraConn, jira, login, logTime);
            }
            if (choose.Key == ConsoleKey.D2 || choose.Key == ConsoleKey.NumPad2)
            {
                jiraConn.IssuesList(jira);
                Return(log, jiraConn, jira, login, logTime);
            }
            if (choose.Key == ConsoleKey.D3 || choose.Key == ConsoleKey.NumPad3)
            {
                jiraConn.Assigned(jira, login);
                Return(log, jiraConn, jira, login, logTime);
            }
            if (choose.Key == ConsoleKey.D4 || choose.Key == ConsoleKey.NumPad4)
            {
                Console.WriteLine("\n\n");
                log = File.AppendText("logs/log_" + logTime + ".txt");
                ToLog(log, "Running script for auto issue making..");
                log.Close();
                CheckEmails(log, jira, jiraConn, login, logTime);
                Return(log, jiraConn, jira, login, logTime);
            }
        }

        public static void Return(StreamWriter log, Tickets jiraConn, Jira jira, String login, String logTime)
        {
            Console.WriteLine("\n\tPress any key to return....");
            Console.ReadKey();
            TopMenu(log, jiraConn, jira, login, logTime);
        }

        public static void ToLog(StreamWriter log, String text)
        {
            Console.WriteLine(DateTime.Now + "\t"+text);
            log.WriteLine(DateTime.Now + "\t" + text);
        }
    }
}
