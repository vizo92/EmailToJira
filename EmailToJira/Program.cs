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
            /*treamWriter log;
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
            }*/

            //log.WriteLine(DateTime.Now + "\tUser "+login+" has logged in.");
            string[] settings = File.ReadAllLines(@"urls.config");
            Tickets jiraConn = new Tickets();
            Jira jira = Jira.CreateRestClient(settings[1], login, password);

            Data passedData = new Data(jira, jiraConn, login);

            Console.WriteLine("\n");
            //log.Close();
            Program.TopMenu(passedData);
            //log = File.AppendText("logs/log_" + logTime + ".txt");
            Console.WriteLine("Program stopped.");
            //log.WriteLine(DateTime.Now + "\tProgram stopped.");
            //log.Close();
            Console.ReadLine();
        }

        public static void CheckEmails(Jira jira, Tickets jiraConn, String login)
        {
            //log = File.AppendText("logs/log_" + logTime + ".txt");
            var mails = OutlookEmails.ReadMailItems(jira, jiraConn, login);
            if (mails.Count > 0)
            {
                if (mails.Count != 1)
                {
                    Console.WriteLine(DateTime.Now + "\t" + mails.Count + " new emails.");
                    //log.WriteLine(DateTime.Now + "\t" + mails.Count + " new emails.");
                }
                else
                {
                    Console.WriteLine(DateTime.Now + "\t" + mails.Count + " new email.");
                    //log.WriteLine(DateTime.Now + "\t" + mails.Count + " new email.");
                }
            }
            int j = 1;
            foreach (var mail in mails)
            {
                Console.WriteLine(DateTime.Now + "\tEmail subject: " + mail.Subject);
                //log.WriteLine(DateTime.Now + "\tEmail subject: " + mail.Subject);
                j++;
            }
            //log.Close();
            Thread.Sleep(1000);
            Console.WriteLine("Test");
            CheckEmails(jira, jiraConn, login);
        }

        public static void TopMenu(Data passedData)
        {
            Console.Clear();
            Console.WriteLine("\t\tAutomatic Jira ticket creation from UNREAD Bastion e-mails\t"+ DateTime.Now);
            Console.WriteLine("\t\t\tCreated by Sylwester Kwiatkowski, 2019\t\t\t" + passedData.Login);

            MainMenu(passedData);
        }

        public static void MainMenu(Data passedData)
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

                passedData.jiraConn.ShowIssue(passedData.jira, issueName);
                Return(passedData);
            }
            if (choose.Key == ConsoleKey.D2 || choose.Key == ConsoleKey.NumPad2)
            {
                passedData.jiraConn.IssuesList(passedData.jira);
                Return(passedData);
            }
            if (choose.Key == ConsoleKey.D3 || choose.Key == ConsoleKey.NumPad3)
            {
                passedData.jiraConn.Assigned(passedData.jira, passedData.Login);
                Return(passedData);
            }
            if (choose.Key == ConsoleKey.D4 || choose.Key == ConsoleKey.NumPad4)
            {
                Console.WriteLine("\n\n");
                //passedData.log = File.AppendText("logs/log_" + passedData.LogTime + ".txt");
                ToLog("Running script for auto issue making..");
                //passedData.log.Close();

                while (true)
                {
                    //Task.Delay(new TimeSpan(0, 0, 15)).ContinueWith(o => { passedData.CheckEmails(); });
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                    passedData.CheckEmails();
                }
                Return(passedData);
            }
        }

        public static void Return(Data passedData)
        {
            Console.WriteLine("\n\tPress any key to return....");
            Console.ReadKey();
            TopMenu(passedData);
        }

        public static void ToLog(String text)
        {
            Console.WriteLine(DateTime.Now + "\t"+text);
            //log.WriteLine(DateTime.Now + "\t" + text);
        }


        //  Not used methods yet, looking for running checking emails in the background

        private static Task<decimal> LongRunningOperation(int loop)
        {
            // Start a task and return it
            return Task.Run(() =>
            {
                decimal result = 0;

                // Loop for a defined number of iterations
                for (int i = 0; i < loop; i++)
                {
                    // Do something that takes times like a Thread.Sleep in .NET Core 2.
                    Thread.Sleep(10);
                    result += i;
                }

                return result;
            });
        }

        private static Task<decimal> LongRunningCancellableOperation(int loop, CancellationToken cancellationToken)
        {
            Task<decimal> task = null;

            // Start a task and return it
            task = Task.Run(() =>
            {
                decimal result = 0;

                // Loop for a defined number of iterations
                for (int i = 0; i < loop; i++)
                {
                    // Check if a cancellation is requested, if yes,
                    // throw a TaskCanceledException.

                    if (cancellationToken.IsCancellationRequested)
                        throw new TaskCanceledException(task);

                    // Do something that takes times like a Thread.Sleep in .NET Core 2.
                    Thread.Sleep(10);
                    result += i;
                }

                return result;
            });

            return task;
        }

        private static async Task<decimal> LongRunningOperationWithCancellationTokenAsync(int loop, CancellationToken cancellationToken)
        {
            // We create a TaskCompletionSource of decimal
            var taskCompletionSource = new TaskCompletionSource<decimal>();

            // Registering a lambda into the cancellationToken
            cancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });

            var task = LongRunningOperation(loop);

            // Wait for the first task to finish among the two
            var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);

            return await completedTask;
        }

        public static async Task CancelANonCancellableTaskAsync()
        {
            Console.WriteLine(nameof(CancelANonCancellableTaskAsync));

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                // Listening to key press to cancel
                var keyBoardTask = Task.Run(() =>
                {
                    Console.WriteLine("Press enter to cancel");
                    Console.ReadKey();

                    // Sending the cancellation message
                    cancellationTokenSource.Cancel();
                });

                try
                {
                    // Running the long running task
                    var longRunningTask = LongRunningOperationWithCancellationTokenAsync(100, cancellationTokenSource.Token);
                    var result = await longRunningTask;

                    Console.WriteLine("Result {0}", result);
                    Console.WriteLine("Press enter to continue");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was cancelled");
                }

                await keyBoardTask;
            }
        }
    }


}
