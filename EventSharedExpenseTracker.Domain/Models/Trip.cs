using EventSharedExpenseTracker.Domain.Result;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;

public class Trip
{
    public int Id { get; set; }
    [StringLength(25)]
    public required string Name { get; set; }

    [DataType(DataType.Date)]
    public DateTime DateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime DateTo { get; set; }
    public int? CreatorId { get; set; }
    public CustomUser? Creator { get; set; }
    public string? ImagePath { get; set; }

    public ICollection<TripParticipant> Participants { get; } = [];
    public ICollection<Expense> Expenses { get; } = [];

    public DomainResult AddParticipant(int userId, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return DomainErrors.ParticipantNameRequired;

        if (Participants.Any(x => x.UserId == userId))
            return DomainErrors.ParticipantAlreadyExists;

        Participants.Add(new TripParticipant
        {
            UserId = userId,
            DisplayName = displayName
        });

        return DomainResult.Ok();
    }

    public DomainResult AddDummyParticipant(string displayName)
    {
        if(string.IsNullOrWhiteSpace(displayName))
            return DomainErrors.ParticipantNameRequired;

        var normalizedName = displayName.Trim();

        if (Participants.Any(x => x.DisplayName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
            return DomainErrors.ParticipantAlreadyExists;

        Participants.Add(new TripParticipant
        {
            DisplayName = displayName
        });

        return DomainResult.Ok();
    }

    public DomainResult RemoveParticipant(int participantId)
    {
        var participant = Participants.FirstOrDefault(x => x.Id == participantId);

        if (participant == null)
            return DomainErrors.NotFound<TripParticipant>();

        if (participant.HasPayments)
            return DomainErrors.ParticipantHasPayments;

        Participants.Remove(participant);

        return DomainResult.Ok();
    }

    public bool IsCreatedBy(int userId)
    {
        return CreatorId == userId;
    }

    public bool HasCreator => CreatorId is not null;

    public bool HasParticipant(int userId)
    {
        return Participants.Any(p => p.UserId == userId);
    }

    public DomainResult ValidateDateRange()
    {
        if (DateFrom > DateTo)
            return DomainErrors.InvalidTripDateRange;

        return DomainResult.Ok();
    }

}
