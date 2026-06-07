using EventSharedExpenseTracker.Domain.PaymentProcessing;
using FluentAssertions;

namespace EventSharedExpenseTracker.Tests.Unit
{
    public class ExpensePaymentProcessingTests
    {
        [Fact]
        public void ProcessForSaving_WhenPaidEqualsOwed_ReturnsSuccess()
        {
            ICollection<PaymentDraft> drafts =
            [
                new() { ParticipantId = 1, UserEnteredAmount = 20, IsOwed = false },
                new() { ParticipantId = 1, UserEnteredAmount = 10, IsOwed = true, IsEquallyShared = true },
                new() { ParticipantId = 2, UserEnteredAmount = 10, IsOwed = true, IsEquallyShared = true },
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ProcessForSaving_WhenPaidDoesNotEqualOwed_ReturnsFailure()
        {
            ICollection<PaymentDraft> drafts =
            [
                new() { ParticipantId = 1, UserEnteredAmount = 30, IsOwed = false },
                new() { ParticipantId = 1, UserEnteredAmount = 10, IsOwed = true, IsEquallyShared = true },
                new() { ParticipantId = 2, UserEnteredAmount = 10, IsOwed = true, IsEquallyShared = true },
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void ProcessForSaving_WhenEquallySharedOwedExists_SplitsRemainingAmount()
        {
            ICollection<PaymentDraft> drafts =
            [
                new() { ParticipantId = 1, UserEnteredAmount = 30, IsOwed = false },
                new() { ParticipantId = 1, IsOwed = true, IsEquallyShared = true },
                new() { ParticipantId = 2, IsOwed = true, IsEquallyShared = true },
                new() { ParticipantId = 3, IsOwed = true, IsEquallyShared = true }
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeTrue();

            result.Value.Should().Contain(p => p.ParticipantId == 1 && p.AmountBase == -10);
            result.Value.Should().Contain(p => p.ParticipantId == 2 && p.AmountBase == -10);
            result.Value.Should().Contain(p => p.ParticipantId == 3 && p.AmountBase == -10);
        }

        [Fact]
        public void ProcessForSaving_WhenManualOwedAmountHasEqualShareFlag_ManualAmountWins()
        {
            ICollection<PaymentDraft> drafts =
            [
                new() { ParticipantId = 1, UserEnteredAmount = 30, IsOwed = false },
                new() { ParticipantId = 1, UserEnteredAmount = 5, IsOwed = true, IsEquallyShared = true },
                new() { ParticipantId = 2, IsOwed = true, IsEquallyShared = true }
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeTrue();

            result.Value.Should().Contain(p => p.ParticipantId == 1 && p.AmountBase == -5 && p.IsEquallyShared == false);
            result.Value.Should().Contain(p => p.ParticipantId == 2 && p.AmountBase == -25);
        }

        [Fact]
        public void ProcessForSaving_WhenSpentIsMoreThanPaid_ReturnsFailure()
        {
            ICollection<PaymentDraft> drafts =
            [
                new() { ParticipantId = 1, UserEnteredAmount = 20, IsOwed = false },
                new() { ParticipantId = 1, UserEnteredAmount = 30, IsOwed = true }
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void ProcessForSaving_WhenPaidMoreThanSpentAndNoSharedOwed_ReturnsFailure()
        {
            ICollection<PaymentDraft> drafts =
            [
                new() { ParticipantId = 1, UserEnteredAmount = 30, IsOwed = false },
                new() { ParticipantId = 1, UserEnteredAmount = 10, IsOwed = true }
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeFalse();
        }

    }
}
