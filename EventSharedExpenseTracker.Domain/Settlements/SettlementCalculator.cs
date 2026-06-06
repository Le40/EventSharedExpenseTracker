
using EventSharedExpenseTracker.Domain.Settlements;

namespace EventSharedExpenseTracker.Domain.SettlementProcessing
{
    public class SettlementCalculator
    {
        public static List<Settlement> CalculateSettlements(
            Dictionary<int, decimal> paidByParticipant,
            Dictionary<int, decimal> owedByParticipant)
        {
            var creditors = new Queue<(int ParticipantId, decimal Amount)>();
            var debtors = new Queue<(int ParticipantId, decimal Amount)>();

            var participantIds = paidByParticipant.Keys
                .Union(owedByParticipant.Keys)
                .ToList();

            foreach (var id in participantIds)
            {
                var paid = paidByParticipant.GetValueOrDefault(id);
                var owed = owedByParticipant.GetValueOrDefault(id);
                var net = paid - owed;

                if (net > 0)
                    creditors.Enqueue((id, net));
                else if (net < 0)
                    debtors.Enqueue((id, -net));
            }

            var settlements = new List<Settlement>();

            while (creditors.Count > 0 && debtors.Count > 0)
            {
                var debtor = debtors.Dequeue();
                var creditor = creditors.Dequeue();

                var amount = Math.Min(debtor.Amount, creditor.Amount);

                settlements.Add(new Settlement
                {
                    FromParticipantId = debtor.ParticipantId,
                    ToParticipantId = creditor.ParticipantId,
                    Amount = amount
                });

                var debtorRemaining = debtor.Amount - amount;
                var creditorRemaining = creditor.Amount - amount;

                if (debtorRemaining > 0)
                    debtors.Enqueue((debtor.ParticipantId, debtorRemaining));

                if (creditorRemaining > 0)
                    creditors.Enqueue((creditor.ParticipantId, creditorRemaining));
            }

            return settlements;
        }
    }
}
