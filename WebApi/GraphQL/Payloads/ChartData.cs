namespace WebApi.GraphQL.Payloads;

public record ChartData(string Label, decimal Value);

public enum FrequencyType
{
    Daily,
    Monthly,
    Yearly
}
