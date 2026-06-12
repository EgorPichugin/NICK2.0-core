namespace NICK2._0_core.Infrastructure.SmartMoving.Responses;

public sealed record SmartMovingOpportunityApiResponse(
    Guid Id,
    int QuoteNumber,
    SmartMovingCustomerApiResponse Customer,
    SmartMovingBranchApiResponse Branch,
    IReadOnlyCollection<object> Contacts,
    int OpportunityType,
    int Type,
    int ServiceDate,
    int Status,
    int? LeadStatus,
    SmartMovingMoveSizeApiResponse MoveSize,
    decimal Volume,
    decimal Weight,
    int VolumeWeightCalculationMode,
    SmartMovingEstimatedTotalApiResponse EstimatedTotal,
    object? Estimator,
    SmartMovingSalesAssigneeApiResponse SalesAssignee,
    bool HasTripInfo,
    string? ReferralSource,
    Guid? AffiliateId,
    string? AffiliateName,
    bool AllowInventoryUpdates,
    string? CustomField01,
    string? CustomField02,
    string? CustomField03,
    string? CancellationReason,
    IReadOnlyCollection<SmartMovingJobApiResponse> Jobs,
    object? Payments,
    object? TripInfo,
    object? OpportunityFiles,
    object? Photos,
    object? OpportunityDocuments,
    object? Surveys,
    object? Tasks,
    SmartMovingTariffApiResponse Tariff,
    DateTimeOffset CreatedAtUtc);

public sealed record SmartMovingCustomerApiResponse(
    Guid Id,
    string Name,
    string EmailAddress,
    string PhoneNumber,
    int PhoneType);

public sealed record SmartMovingBranchApiResponse(
    string Name,
    string PhoneNumber);

public sealed record SmartMovingMoveSizeApiResponse(
    string Name,
    string Description,
    decimal Volume);

public sealed record SmartMovingEstimatedTotalApiResponse(
    decimal Subtotal,
    decimal TaxableAmount,
    decimal Tax,
    decimal FinalTotal);

public sealed record SmartMovingSalesAssigneeApiResponse(
    Guid Id,
    string Name);

public sealed record SmartMovingJobApiResponse(
    Guid Id,
    string JobNumber,
    int JobDate,
    int Type,
    bool Confirmed,
    object? JobAddresses,
    object? JobDocuments,
    object? EstimatedCharges,
    object? ActualCharges,
    SmartMovingArrivalWindowApiResponse ArrivalWindow,
    object? TotalTips,
    IReadOnlyCollection<object> CrewMembers,
    object? JobTime);

public sealed record SmartMovingArrivalWindowApiResponse(
    Guid Id,
    string Description,
    decimal StartTime,
    decimal EndTime,
    bool IsDefault);

public sealed record SmartMovingTariffApiResponse(
    Guid Id,
    string Name);
