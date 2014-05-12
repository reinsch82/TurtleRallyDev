﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TurtleRallyDev
{
    public partial class CommitFinishedForm : Form
    {
        public CommitFinishedForm( List<TicketItem> selectedTickets )
        {
            InitializeComponent( );
            string selectedIssuesString = "Selected Issues :";
            foreach (TicketItem ticket in selectedTickets)
            {
                selectedIssuesString += ticket.TicketId.ToString( ) + " ";
            }
            label1.Text = selectedIssuesString;
        }
    }
}
