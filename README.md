# Email To Jira - Whats that?

Email to Jira is a small program to automatize sort task in my job. All i needed was reading a specific e-mail from specific folder and creating a ticket in Jira where e-mail subject = summary and e-mail message = description of the issue. Lifecycle of ticket created by this program: <br />
Create an issue -> Start investigate -> Resolve issue -> Close issue. Immediately.

# Features
- you can set adress of your Jira in urls.config file
- show specified issue with given ID
- show 10 latest issues

# To do
Parametrization of data (to set in a file):
- Project name
- Own template for e-mail checks
