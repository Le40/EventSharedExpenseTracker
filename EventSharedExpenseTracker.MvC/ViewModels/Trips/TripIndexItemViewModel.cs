namespace EventSharedExpenseTracker.MvC.ViewModels.Trips
{
    public class TripIndexItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string? ImagePath { get; set; }
        public List<string> ParticipantNames { get; set; } = [];
    }
}
