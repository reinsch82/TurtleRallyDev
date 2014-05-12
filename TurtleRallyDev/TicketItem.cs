namespace TurtleRallyDev
{
    public class TicketItem
    {
        private readonly string _ticketId;
        private readonly string _ticketSummary;

        public TicketItem(string ticketNumber, string ticketSummary)
        {
            _ticketId = ticketNumber;
            _ticketSummary = ticketSummary;
        }

        public string TicketId
        {
            get { return _ticketId; }
        }

        public string Summary
        {
            get { return _ticketSummary; }
        }
    }
}