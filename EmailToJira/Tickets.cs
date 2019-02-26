using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atlassian.Jira;
using System.Globalization;
using Newtonsoft.Json;
using System.IO;

namespace EmailToJira
{
    public class Tickets
    {      
        public Tickets() { }

        public String key;

        public async void MakeATicket(Jira jira, String summary, String description, String loginLabel, String login)
        {
            //log = File.AppendText("logs/log_" + logTime + ".txt");
            var s = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK"); //if not works small hh
            s = s.Substring(0, 26) + s.Substring(27, 2);
            var issue = jira.CreateIssue("PROJECT_NAME"); //Name of the Project
            issue.Type = "Incident";
            issue.Priority = "Low";
            issue.Summary = summary;
            issue.Description = description;
            issue.Components.Add("COMPONENT_NAME"); //Component name
            issue.Labels.Add(loginLabel);
            issue["Origin"] = "None";
            issue["Occurence Time"] = s;

            await issue.SaveChangesAsync();

            ToLog("Ticket has been created.");

            jira.Issues.MaxIssuesPerRequest = 1;
            var issueKey = from i in jira.Issues.Queryable
                         where i.Project == "PROJECT_NAME" && i.Status == "Open"
                         select i;
            foreach (var item in issueKey)
            {
                key = item.Key.ToString();
            }

            //log.Close();
            UpdateIssue(jira, key, login);

        }

        public async void UpdateIssue(Jira jira, String key, String login)
        {
            //log = File.AppendText("logs/log_" + logTime + ".txt");
            var issue = await jira.Issues.GetIssueAsync(key);
            await issue.AssignAsync(login);
            await issue.SaveChangesAsync();
            ToLog("Ticket has been assigned.");
            await issue.WorkflowTransitionAsync("Start Investigate");
            ToLog("Investigation starts.");

            await issue.WorkflowTransitionAsync("Resolve");
            ToLog("Issue Resolved.");

            await issue.WorkflowTransitionAsync(WorkflowActions.Close);
            ToLog("Issue Closed.");
            //log.Close();
        }

        public void IssuesList(Jira jira)
        {
            jira.Issues.MaxIssuesPerRequest = 10;
            // use LINQ syntax to retrieve issues
            var issues = from i in jira.Issues.Queryable
                         where i.Project == "PROJECT_NAME" && i.Status == "Open"
                         //orderby i.DueDate
                         select i;

            int k = 1;
            foreach (Issue item in issues)
            {
                PrintIssue(jira, item);
                k++;
            }
        }

        public void ShowIssue(Jira jira, String issueName)
        {

            var issues = from i in jira.Issues.Queryable
                     where i.Project == "PROJECT_NAME" && i.Key == issueName 
                     select i;

            foreach (var issue in issues)
            {
                PrintIssue(jira, issue);
            }
        }

        public void Assigned(Jira jira, String login)
        {
            jira.Issues.MaxIssuesPerRequest = 10;
            var issues = from i in jira.Issues.Queryable
                         where i.Project == "PROJECT_NAME" && i.Assignee.Equals(login)
                         select i;

            foreach (var issue in issues)
            {
                PrintIssue(jira, issue);
            }
        }

        public void PrintIssue(Jira jira, Issue item)
        {
            Console.WriteLine("Project: \t" + item.Project);
            Console.WriteLine("Reporter: \t" + item.Reporter);
            Console.WriteLine("Type: \t\t" + item.Type);
            Console.WriteLine("Priority: \t" + item.Priority);
            Console.WriteLine("Key: \t\t"+item.Key);
            //Console.WriteLine("Occurence time: "+item["Occurence Time"].Value);
            Console.WriteLine("Status: \t"+item.Status);
            Console.WriteLine("Components: \t" + item.Components[0]);
            Console.WriteLine("Summary: \t" + item.Summary);
            Console.WriteLine("Description:\n" + item.Description);
            Console.WriteLine("------------------------------------------------------------------------");
        }
        public static void ToLog(String text)
        {
            Console.WriteLine(DateTime.Now + "\t" + text);
            //log.WriteLine(DateTime.Now + "\t" + text);
        }
    }
}
