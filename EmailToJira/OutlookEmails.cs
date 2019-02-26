using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Atlassian.Jira;
using System.IO;

namespace EmailToJira
{
    public class OutlookEmails : Program
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public static List<OutlookEmails> ReadMailItems(Jira jira, Tickets jiraConn, String login)
        {
            Application outlookApplication = null;
            NameSpace outlookNamespace = null;
            MAPIFolder inboxFolder = null;
            String loginLabel;

            Items mailItems = null;
            List<OutlookEmails> listEmailDetails = new List<OutlookEmails>();
            OutlookEmails emailDetails;

            //Regular expression for login label
            string re1 = ".*?"; // Non-greedy match on filler
            string re2 = "\\s+";    // Uninteresting: ws
            string re3 = ".*?"; // Non-greedy match on filler
            string re4 = "\\s+";    // Uninteresting: ws
            string re5 = ".*?"; // Non-greedy match on filler
            string re6 = "\\s+";    // Uninteresting: ws
            string re7 = ".*?"; // Non-greedy match on filler
            string re8 = "\\s+";    // Uninteresting: ws
            string re9 = ".*?"; // Non-greedy match on filler
            string re10 = "(\\s+)"; // White Space 1
            string re11 = "((?:[a-z][a-z0-9_]*))";  // Variable Name 1
            string re12 = "(.)";    // Any Single Character 1

            Regex r = new Regex(re1 + re2 + re3 + re4 + re5 + re6 + re7 + re8 + re9 + re10 + re11 + re12, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            try
            {
                outlookApplication = new Application();
                outlookNamespace = outlookApplication.GetNamespace("MAPI");
                inboxFolder = outlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                mailItems = inboxFolder.Items;

                foreach (MAPIFolder subFolder in inboxFolder.Folders)
                {
                    if (subFolder.Name == "Bastion")
                    {
                        foreach (MailItem item in subFolder.Items)
                        {
                            if (item.UnRead && item.Subject.Contains("New approval request"))
                            {
                                emailDetails = new OutlookEmails();
                                emailDetails.From = item.SenderEmailAddress;
                                emailDetails.Subject = item.Subject;
                                emailDetails.Subject = emailDetails.Subject.Replace("FW: ", "");
                                emailDetails.Body = item.Body;
                                if (emailDetails.Body.Contains("[THIS MESSAGE ORIGINATED FROM A NON-IGT EMAIL ADDRESS]"))
                                {
                                    emailDetails.Body = RemoveFirstLines(emailDetails.Body, 10);
                                }
                                if (emailDetails.Body.Contains("Note Importante"))
                                {
                                    emailDetails.Body = emailDetails.Body.Replace("________________________________", "");
                                    emailDetails.Body = emailDetails.Body.Replace("Note Importante: Le contenu de ce courriel est uniquement réservé à la personne ou l'organisme à qui il est destiné. Si vous n'êtes pas le destinataire prévu, veuillez nous en informer au plus vite et détruire le présent courriel. Dans ce cas, il ne vous est pas permis de copier ce courriel, de le distribuer ou de l'utiliser de quelque manière que ce soit.", "");
                                    emailDetails.Body = emailDetails.Body.Replace("________________________________", "");
                                    emailDetails.Body = emailDetails.Body.Replace("Important Notice: The content of this e - mail is intended only and solely for the use of the named recipient or organization.If you are not the named recipient, please inform us immediately and delete the present e - mail.In this case, you are not allowed to copy, distribute or use this e - mail in any way.", "");
                                }

                                //Regex regex = new Regex(@".+\@");
                                MatchCollection labelOf = r.Matches(emailDetails.Subject);
                                loginLabel = labelOf[0].Value;
                                loginLabel = loginLabel.Replace("[Bastion] New approval request for ", "");
                                loginLabel = loginLabel.Replace("@", "");
                                //log.Close();
                                jiraConn.MakeATicket(jira, emailDetails.Subject, emailDetails.Body, loginLabel, login);
                                listEmailDetails.Add(emailDetails);
                                
                                item.UnRead = false;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                ReleaseComObject(mailItems);
                ReleaseComObject(inboxFolder);
                ReleaseComObject(outlookNamespace);
                ReleaseComObject(outlookApplication);
            }
            return listEmailDetails;
        }

        private static void ReleaseComObject(object obj)
        {
            if (obj != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
        }

        public static string RemoveFirstLines(string text, int linesCount)
        {
            var lines = Regex.Split(text, "\r\n|\r|\n").Skip(linesCount);
            lines = lines.Take(lines.Count() - 9);
            return string.Join(Environment.NewLine, lines.ToArray());
        }
    }
}
