using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Domain.Settlements;

public static class SettlementCalculator
{
    public static List<Settlement> Calculate(Trip trip)
    {
        var debtors = trip.Participants
            .Select(p => new
            {
                Participant = p,
                Balance = p.Payments.Sum(x => x.AmountBase)
            })
            .Where(x => x.Balance < 0)
            .OrderBy(x => x.Balance) // most negative first
            .ToList();

        var creditors = trip.Participants
            .Select(p => new
            {
                Participant = p,
                Balance = p.Payments.Sum(x => x.AmountBase)
            })
            .Where(x => x.Balance > 0)
            .OrderByDescending(x => x.Balance)
            .ToList();

        var settlements = new List<Settlement>();

        var debtorIndex = 0;
        var creditorIndex = 0;

        while (debtorIndex < debtors.Count && creditorIndex < creditors.Count)
        {
            var debtor = debtors[debtorIndex];
            var creditor = creditors[creditorIndex];

            var amount = Math.Min(
                Math.Abs(debtor.Balance),
                creditor.Balance);

            settlements.Add(new Settlement
            {
                FromParticipantId = debtor.Participant.Id,
                FromParticipantName = debtor.Participant.DisplayName,
                ToParticipantId = creditor.Participant.Id,
                ToParticipantName = creditor.Participant.DisplayName,
                Amount = amount
            });

            debtors[debtorIndex] = debtor with
            {
                Balance = debtor.Balance + amount
            };

            creditors[creditorIndex] = creditor with
            {
                Balance = creditor.Balance - amount
            };

            if (debtors[debtorIndex].Balance == 0)
                debtorIndex++;

            if (creditors[creditorIndex].Balance == 0)
                creditorIndex++;
        }

        return settlements;
    }
}