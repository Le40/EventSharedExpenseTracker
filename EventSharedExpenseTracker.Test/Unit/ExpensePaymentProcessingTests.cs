using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.PaymentProcessing;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Tests.Unit
{
    public class ExpensePaymentProcessingTests
    {
        [Fact]
        public void ProcessForSaving_WhenPaidEqualsOwed_ReturnsSuccess()
        {
            ICollection<PaymentInput> drafts =
            [
                new() { ParticipantId = 1, Amount = 20, IsOwed = false },
                new() { ParticipantId = 1, Amount = 10, IsOwed = true, IsEquallyShared = true },
                new() { ParticipantId = 2, Amount = 10, IsOwed = true, IsEquallyShared = true },
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ProcessForSaving_WhenPaidDoesNotEqualOwed_ReturnsFailure()
        {
            ICollection<PaymentInput> drafts =
            [
                new() { ParticipantId = 1, Amount = 30, IsOwed = false },
                new() { ParticipantId = 1, Amount = 10, IsOwed = true, IsEquallyShared = true },
                new() { ParticipantId = 2, Amount = 10, IsOwed = true, IsEquallyShared = true },
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void ProcessForSaving_WhenEquallySharedOwedExists_SplitsRemainingAmount()
        {
            ICollection<PaymentInput> drafts =
            [
                new() { ParticipantId = 1, Amount = 30, IsOwed = false },
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
            ICollection<PaymentInput> drafts =
            [
                new() { ParticipantId = 1, Amount = 30, IsOwed = false },
                new() { ParticipantId = 1, Amount = 5, IsOwed = true, IsEquallyShared = true },
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
            ICollection<PaymentInput> drafts =
            [
                new() { ParticipantId = 1, Amount = 20, IsOwed = false },
                new() { ParticipantId = 1, Amount = 30, IsOwed = true }
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void ProcessForSaving_WhenPaidMoreThanSpentAndNoSharedOwed_ReturnsFailure()
        {
            ICollection<PaymentInput> drafts =
            [
                new() { ParticipantId = 1, Amount = 30, IsOwed = false },
                new() { ParticipantId = 1, Amount = 10, IsOwed = true }
            ];

            var result = ExpenseProcessor.ProcessForSaving(drafts,1m);

            result.IsSuccess.Should().BeFalse();
        }

    }
}
