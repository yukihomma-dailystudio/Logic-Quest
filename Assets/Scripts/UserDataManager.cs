using UnityEngine;

public static class UserDataManager
{
    private const string LevelKey = "ThinQuest.User.Level";
    private const string TotalExpKey = "ThinQuest.User.TotalExp";
    private const string BattlesCompletedKey = "ThinQuest.User.BattlesCompleted";

    private static readonly AbilityProgress[] DefaultAbilities =
    {
        new AbilityProgress("要約力", "ThinQuest.Ability.SummaryExp"),
        new AbilityProgress("論理力", "ThinQuest.Ability.LogicExp"),
        new AbilityProgress("具体化力", "ThinQuest.Ability.ConcreteExp"),
        new AbilityProgress("反論耐性", "ThinQuest.Ability.CounterExp"),
        new AbilityProgress("説明力", "ThinQuest.Ability.ExplainExp"),
        new AbilityProgress("視点切替力", "ThinQuest.Ability.PerspectiveExp")
    };

    public static UserProfile LoadProfile()
    {
        var totalExp = PlayerPrefs.GetInt(TotalExpKey, 0);
        var level = Mathf.Max(1, PlayerPrefs.GetInt(LevelKey, CalculateLevel(totalExp)));
        var battlesCompleted = PlayerPrefs.GetInt(BattlesCompletedKey, 0);
        var abilities = new AbilityProgress[DefaultAbilities.Length];

        for (var i = 0; i < DefaultAbilities.Length; i++)
        {
            var template = DefaultAbilities[i];
            var exp = PlayerPrefs.GetInt(template.ExpKey, 0);
            abilities[i] = new AbilityProgress(template.Name, template.ExpKey, exp, CalculateLevel(exp));
        }

        return new UserProfile(level, totalExp, battlesCompleted, abilities);
    }

    public static void AddExp(int amount, string abilityName)
    {
        if (amount <= 0)
        {
            return;
        }

        var totalExp = PlayerPrefs.GetInt(TotalExpKey, 0) + amount;
        PlayerPrefs.SetInt(TotalExpKey, totalExp);
        PlayerPrefs.SetInt(LevelKey, CalculateLevel(totalExp));
        PlayerPrefs.SetInt(BattlesCompletedKey, PlayerPrefs.GetInt(BattlesCompletedKey, 0) + 1);

        for (var i = 0; i < DefaultAbilities.Length; i++)
        {
            var ability = DefaultAbilities[i];
            if (ability.Name != abilityName)
            {
                continue;
            }

            PlayerPrefs.SetInt(ability.ExpKey, PlayerPrefs.GetInt(ability.ExpKey, 0) + amount);
            break;
        }

        PlayerPrefs.Save();
    }

    private static int CalculateLevel(int exp)
    {
        return Mathf.Max(1, exp / 100 + 1);
    }
}

public readonly struct UserProfile
{
    public UserProfile(int level, int totalExp, int battlesCompleted, AbilityProgress[] abilities)
    {
        Level = level;
        TotalExp = totalExp;
        BattlesCompleted = battlesCompleted;
        Abilities = abilities;
    }

    public int Level { get; }
    public int TotalExp { get; }
    public int BattlesCompleted { get; }
    public AbilityProgress[] Abilities { get; }
}

public readonly struct AbilityProgress
{
    public AbilityProgress(string name, string expKey, int exp = 0, int level = 1)
    {
        Name = name;
        ExpKey = expKey;
        Exp = exp;
        Level = level;
    }

    public string Name { get; }
    public string ExpKey { get; }
    public int Exp { get; }
    public int Level { get; }
}
