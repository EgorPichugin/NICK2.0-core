using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Infrastructure.SmartMoving.Responses;

namespace NICK2._0_core.Infrastructure.SmartMoving.Mappers;

public static class SmartMovingOpportunityMapper
{
    public static SmartMovingOpportunity ToSmartMovingOpportunity(
        this SmartMovingOpportunityApiResponse response)
    {
        return new SmartMovingOpportunity(
            response.Id,
            response.QuoteNumber,
            response.Status);
    }
}
