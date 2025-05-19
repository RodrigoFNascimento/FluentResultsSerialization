using FluentResultsSerialization.Strategies;
using Microsoft.AspNetCore.Http;

namespace FluentResultsSerialization.Serializer;
internal sealed class ResultSerializer : IResultSerializer
{
    private readonly IEnumerable<IResultSerializationStrategy> _strategies;
    private readonly string _noStrategyMessage = LocalizationHelper.GetMessage("NoStrategy");

    public ResultSerializer(IEnumerable<IResultSerializationStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IResult Serialize(FluentResults.Result result)
    {
        var correctStrategy = _strategies
            .FirstOrDefault(e => e.ShouldSerialize(result))
            ?? throw new InvalidOperationException(
                $"{_noStrategyMessage} {result.GetType().FullName}");

        return correctStrategy.Serialize(result);
    }

    public IResult Serialize<TValue>(FluentResults.Result<TValue> result)
    {
        var correctStrategy = _strategies
            .FirstOrDefault(e => e.ShouldSerialize(result))
            ?? throw new InvalidOperationException(
                $"{_noStrategyMessage} {result.GetType().FullName}");

        return correctStrategy.Serialize(result);
    }
}
