using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace StS2Aris.StS2ArisCode.Utils;

public static class CardRewardCompat
{
    private static readonly ConstructorInfo? RewardConstructor = typeof(CardReward)
        .GetConstructors()
        .OrderBy(constructor => constructor.GetParameters().Length)
        .FirstOrDefault(constructor =>
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            return parameters.Length >= 3
                   && parameters[0].ParameterType == typeof(CardCreationOptions)
                   && parameters[1].ParameterType == typeof(int)
                   && parameters[2].ParameterType == typeof(Player)
                   && parameters.Skip(3).All(parameter => parameter.IsOptional);
        });

    public static CardReward Create(CardCreationOptions options, int cardCount, Player player)
    {
        if (RewardConstructor == null)
        {
            throw new MissingMethodException(typeof(CardReward).FullName, ".ctor");
        }

        object?[] arguments = RewardConstructor.GetParameters()
            .Select(parameter => parameter.DefaultValue).ToArray();
        arguments[0] = options;
        arguments[1] = cardCount;
        arguments[2] = player;

        try
        {
            return (CardReward)RewardConstructor.Invoke(arguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }
}
