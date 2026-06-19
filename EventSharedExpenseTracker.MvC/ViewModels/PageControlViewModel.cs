namespace EventSharedExpenseTracker.MvC.ViewModels
{
    public enum ControlMode
    {
        Trips,
        Expenses,
        Participants,
        AddParticipant,
        Friends,
        AddFriend
    }

    public class PageControlsViewModel
    {
        public ControlMode Mode { get; set; }
        public string ModeLabel { get; set; } = "";

        public string SearchPlaceholder { get; set; } = "";
        public string SearchUrl { get; set; } = "";
        public string SearchTargetId { get; set; } = "";

        public string AddUrl { get; set; } = "";
        public string AddTargetId { get; set; } = "";

        public string CssThemeClass { get; set; } = "";
    }
}
