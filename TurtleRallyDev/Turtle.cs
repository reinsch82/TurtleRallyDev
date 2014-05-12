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
                var defectRequest = new Request("defect");
                defectRequest.Fetch = new List<string>() { "Name", "Description", "FormattedID" };
                defectRequest.Query = new Query("Iteration.StartDate", Query.Operator.LessThanOrEqualTo, DateTime.Today.ToString("yyyy-MM-dd"))
                  .And(new Query("Iteration.EndDate", Query.Operator.GreaterThanOrEqualTo, DateTime.Today.ToString("yyyy-MM-dd")))
                  .And(new Query("Release.Name", Query.Operator.Equals, "ST 15.5"));

                // Make request and process results 
                var tickets = new List<TicketItem>( );
                var queryResult = restApi.Query(defectRequest);
                if (!queryResult.Success) {
                  foreach (var er in queryResult.Errors) {
                    MessageBox.Show("Error: " + er);
                  }
                }
                foreach (var qr in queryResult.Results) {
                  tickets.Add(new TicketItem(qr["FormattedID"], qr["Name"]));
                }

                defectRequest = new Request("HierarchicalRequirement");
                defectRequest.Fetch = new List<string>() { "Name", "Description", "FormattedID" };
                defectRequest.Query = new Query("Iteration.StartDate", Query.Operator.LessThanOrEqualTo, DateTime.Today.ToString("yyyy-MM-dd"))
                  .And(new Query("Iteration.EndDate", Query.Operator.GreaterThanOrEqualTo, DateTime.Today.ToString("yyyy-MM-dd")))
                  .And(new Query("Release.Name", Query.Operator.Equals, "ST 15.5"));

              // Make request and process results 
                queryResult = restApi.Query(defectRequest);
                if (!queryResult.Success) {
                  foreach (var er in queryResult.Errors) {
                    MessageBox.Show("Error: " + er);
                  }
                }
                foreach (var qr in queryResult.Results) {
                  tickets.Add(new TicketItem(qr["FormattedID"], qr["Name"]));
                }

                revPropNames = new string[2];
                revPropValues = new string[2];
                revPropNames[0] = "bugtraq:issueIDs";
                revPropNames[1] = "myownproperty";
                revPropValues[0] = "13, 16, 17";
                revPropValues[1] = "myownvalue";

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

        public string CheckCommit( IntPtr hParentWnd, string parameters, string commonURL, string commonRoot, string[] pathList, string commitMessage )
        {
            return "the commit log message is not correct.";
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
