namespace EventSharedExpenseTracker.MvC.Common
{
    public class UiIds
    {
        public const string TripParticipants = "tripParticipants";
        public const string SearchParticipants = "searchParticipants";
        public const string EditTrip = "editTrip";
        public const string CreateTrip = "createTrip";
        public const string CreateExpense = "createExpense";
        public const string ExpenseCollection = "expCollection";

        public static string EditExpense(int id) => $"expense{id}";
    }
}
