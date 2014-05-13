using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Rally.RestApi;

namespace TurtleRallyDev
{
    [ComVisible(true),
        Guid("FBD16270-FFE7-47F8-9667-2ED4162CFD44"),
        ClassInterface(ClassInterfaceType.None)]
    public class Turtle : Interop.BugTraqProvider.IBugTraqProvider2, Interop.BugTraqProvider.IBugTraqProvider
    {
        private List<TicketItem> selectedTickets = new List<TicketItem>();
        string issueIds = "";

        public bool ValidateParameters(IntPtr hParentWnd, string parameters)
        {
            return true;
        }

        public string GetLinkText(IntPtr hParentWnd, string parameters)
        {
            return "Choose Rally Issue";
        }

        public string GetCommitMessage(IntPtr hParentWnd, string parameters, string commonRoot, string[] pathList,
                                       string originalMessage)
        {
            string[] revPropNames = new string[0];
            string[] revPropValues = new string[0];
            string dummystring = "";
            return GetCommitMessage2( hParentWnd, parameters, "", commonRoot, pathList, originalMessage, "", out dummystring, out revPropNames, out revPropValues );
        }

        public string GetCommitMessage2( IntPtr hParentWnd, string parameters, string commonURL, string commonRoot, string[] pathList,
                               string originalMessage, string bugID, out string bugIDOut, out string[] revPropNames, out string[] revPropValues )
        {
            try
            {
                var restApi = new RallyRestApi("reinhold.degenfellner@microfocus.com", "Reinsch1987", "https://rally1.rallydev.com", "v2.0");

                // Build request
                var request = BuildRequest("defect");

                // Make request and process results 
                var queryResult = restApi.Query(request);
                if (!queryResult.Success) {
                  foreach (var er in queryResult.Errors) {
                    MessageBox.Show("Error: " + er);
                  }
                }
                
                var tickets = createTicketsAndIssueIds(queryResult);

                request = BuildRequest("HierarchicalRequirement");  
                // Make request and process results 
                queryResult = restApi.Query(request);
                if (!queryResult.Success) {
                  foreach (var er in queryResult.Errors) {
                    MessageBox.Show("Error: " + er);
                  }
                }

                tickets.AddRange(createTicketsAndIssueIds(queryResult));

                revPropNames = new string[1];
                revPropValues = new string[1];
                revPropNames[0] = "bugtraq:issueIDs";
                revPropValues[0] = issueIds;

                bugIDOut = bugID + "added";

                MyIssuesForm form = new MyIssuesForm( tickets );
                if ( form.ShowDialog( ) != DialogResult.OK )
                    return originalMessage;

                StringBuilder result = new StringBuilder( originalMessage );
                if ( originalMessage.Length != 0 && !originalMessage.EndsWith( "\n" ) )
                    result.AppendLine( );

                foreach ( TicketItem ticket in form.TicketsFixed )
                {
                    result.AppendFormat( "Fixed {0}: {1}", ticket.TicketId, ticket.Summary );
                    result.AppendLine( );
                    selectedTickets.Add( ticket );
                }


                return result.ToString( );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.ToString( ) );
                throw;
            }
        }

        private List<TicketItem> createTicketsAndIssueIds(Rally.RestApi.Response.QueryResult queryResult)
        {
            var tickets = new List<TicketItem>();
            foreach (var qr in queryResult.Results) {
                tickets.Add(new TicketItem(qr["FormattedID"], qr["Name"]));
                issueIds = issueIds + qr["FormattedID"] + ";";
            }
            return tickets;
        }

        private static Request BuildRequest(string type) {
          var todayDate = DateTime.Today.ToString("yyyy-MM-dd");
          var q = new Query("Iteration.StartDate", Query.Operator.LessThanOrEqualTo, todayDate)
            .And(new Query("Iteration.EndDate", Query.Operator.GreaterThanOrEqualTo, todayDate)
            .And(new Query("Release.Name", Query.Operator.Equals, "ST 15.5")));

          var fetchValues = new List<string>() { "Name", "Description", "FormattedID" };
          var defectRequest = new Request(type);
          defectRequest.Fetch = fetchValues;
          defectRequest.Query = q;
          return defectRequest;
        }

        public string CheckCommit( IntPtr hParentWnd, string parameters, string commonURL, string commonRoot, string[] pathList, string commitMessage )
        {
            return "";
        }

        public string OnCommitFinished( IntPtr hParentWnd, string commonRoot, string[] pathList, string logMessage, int revision )
        {
            // we now could use the selectedTickets member to find out which tickets
            // were assigned to this commit.
            CommitFinishedForm form = new CommitFinishedForm( selectedTickets );
            if ( form.ShowDialog( ) != DialogResult.OK )
                return "";
            // just for testing, we return an error string
            return "an error happened while closing the issue";
        }

        public bool HasOptions()
        {
            return true;
        }

        public string ShowOptionsDialog( IntPtr hParentWnd, string parameters )
        {
            OptionsForm form = new OptionsForm( );
            if ( form.ShowDialog( ) != DialogResult.OK )
                return "";

            string options = form.checkBox1.Checked ? "option1" : "";
            options += form.checkBox2.Checked ? "option2" : "";
            return options;
        }

    }
}
