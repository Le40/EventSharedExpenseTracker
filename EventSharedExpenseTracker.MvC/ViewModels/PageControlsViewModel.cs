namespace EventSharedExpenseTracker.MvC.ViewModels
{
    public class PageControlsViewModel
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";

        public string SearchPlaceholder { get; set; } = "";
        public string SearchUrl { get; set; } = "";
        public string SearchTargetId { get; set; } = "";

        public string AddUrl { get; set; } = "";
        public string AddTargetId { get; set; } = "";

        public string CssThemeClass { get; set; } = "";
        public string PanelType { get; set; } = "";
        public string ParseUrl { get; set; } = "";
    }
}
