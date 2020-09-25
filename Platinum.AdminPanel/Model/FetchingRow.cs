namespace Platinum.AdminPanel.Model
{
    public class FetchingRow
    {
        public int RowId { get; set; }
        public string DisplayName
        {
            get
            {
                return "(#" + UserId + ") " + Login;
            }
        }

        public string Login { get; set; }
        public int UserId { get; set; }
        public int InProcessTasks { get; set; }
        public int WaitingTasks { get; set; }
    }
}