using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{

public static void Init()
{
    foreach (var kvp in Conditions)
    {
        var conditionId = kvp.Key;
        var condition = kvp.Value;

        condition.Id = conditionId; 
    }
}
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 8 );
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to poison");
                }
            }
        },

        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s paralyzed and can't move");
                        return false;
                    }

                    return true;
                }
            }
        }
    };
}

public enum ConditionID
{
   none, psn, par, frz, slp, 
}