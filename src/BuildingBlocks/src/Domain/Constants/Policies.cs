namespace BuildingBlocks.Domain.Constants;

public abstract class Policies
{
    public const string CanPurge = nameof(CanPurge);

    public const string CanPrepareOffer = nameof(CanPrepareOffer);
    public const string CanReviewOffer = nameof(CanReviewOffer);
    public const string CanRejectOrApproveOffer = nameof(CanRejectOrApproveOffer);
    public const string CanCorrectOffer = nameof(CanCorrectOffer);
    public const string CanPublishOffer = nameof(CanPublishOffer);
}
