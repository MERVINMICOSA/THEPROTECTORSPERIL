using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
        StatusChanges = new Queue<string>();
        Init(); // Initialize stats and moves
    }

    public Pokemon(PokemonSaveData saveData) // This constructor now correctly resides within the Pokemon class.
    {
        _base = PokemonDB.GetPokemonByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(s => new Move(s)).ToList();
        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
    }

    [SerializeField] private PokemonBase _base;
    [SerializeField] private int level;

    public PokemonBase Base => _base;
    public int Level => level;
    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
   
    public event System.Action OnHPChanged;
    public int MaxHp { get; private set; }

    public void Init()
    {
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }
        Exp = Base.GetExpForLevel(Level);
        CalculateStats();
        HP = MaxHp;
        ResetStatBoost();
    }

   

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>
        {
            { Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5 },
            { Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5 },
            { Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5 },
            { Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5 },
            { Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5 }
        };
        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 }
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }
        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel() =>
        Base.LearnableMoves.FirstOrDefault(x => x.Level == level);

    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count >= PokemonBase.MaxNumOfMoves)
            return;
        Moves.Add(new Move(moveToLearn.Base));
    }

    public int Attack => GetStat(Stat.Attack);
    public int Defense => GetStat(Stat.Defense);
    public int SpAttack => GetStat(Stat.SpAttack);
    public int SpDefense => GetStat(Stat.SpDefense);
    public int Speed => GetStat(Stat.Speed);

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = Random.value * 100f <= 6.25f ? 2f : 1f;
        float type = TypeChart.GetEffectiveness(move.Base.Type, Base.Type1) *
                     TypeChart.GetEffectiveness(move.Base.Type, Base.Type2);

        var damageDetails = new DamageDetails
        {
            TypeEffectivenes = type,
            Critical = critical,
            Fainted = false
        };

        float attack = move.Base.Category == MoveCategory.Special ? attacker.SpAttack : attacker.Attack;
        float defense = move.Base.Category == MoveCategory.Special ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        return damageDetails;
    }

    public void Heal()
    {
        HP = MaxHp;
        OnHPChanged?.Invoke();
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
       
    }

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
        
    }

    public void SetStatus(ConditionID conditionId)
    {
        Status = ConditionsDB.Conditions[conditionId];
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();
        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove() =>
        Status?.OnBeforeMove?.Invoke(this) ?? true;

    public void OnAfterTurn() =>
        Status?.OnAfterTurn?.Invoke(this);

    public void OnBattleOver() => ResetStatBoost();

    public PokemonSaveData GetSaveData()
    {
        return new PokemonSaveData
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m=> m.GetSaveData()).ToList()
        };
    }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectivenes { get; set; }
}
