namespace BuildingBlocks.Domain.Constants;

public abstract class Policies
{
    public const string CanPurge = nameof(CanPurge);

    public const string CanPrepareOffer = nameof(CanPrepareOffer);
    public const string CanReviewOffer = nameof(CanReviewOffer);
    public const string CanRejectOffer = nameof(CanRejectOffer);
    public const string CanCorrectOffer = nameof(CanCorrectOffer);
    public const string CanApproveOffer = nameof(CanApproveOffer);
    public const string CanPublishOffer = nameof(CanPublishOffer);
}
