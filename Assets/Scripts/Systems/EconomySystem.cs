using System;

public class EconomySystem
{
    private readonly MenuSystem menuSystem;
    private readonly UpgradeSystem upgradeSystem;
    private readonly StoreTierSystem storeTierSystem;
    private readonly CustomerSystem customerSystem;
    private readonly PrestigeSystem prestigeSystem;
    private readonly BoostState boostState = new BoostState();

    public double Currency { get; private set; }
    public double TotalEarned { get; private set; }

    public event Action<double> OnIncomeGained;

    public EconomySystem(MenuSystem menuSystem, UpgradeSystem upgradeSystem, StoreTierSystem storeTierSystem, CustomerSystem customerSystem, PrestigeSystem prestigeSystem, double startingCurrency, double totalEarned)
    {
        this.menuSystem = menuSystem;
        this.upgradeSystem = upgradeSystem;
        this.storeTierSystem = storeTierSystem;
        this.customerSystem = customerSystem;
        this.prestigeSystem = prestigeSystem;
        Currency = Math.Max(0, startingCurrency);
        TotalEarned = Math.Max(0, totalEarned);
    }

    public double IncomePerSec => CalculateIncomePerSec();

    public void Tick(float dt)
    {
        boostState.Tick(dt);
        var income = IncomePerSec * dt;
        if (income > 0)
        {
            Currency += income;
            TotalEarned += income;
            OnIncomeGained?.Invoke(income);
        }
    }

    public void AddCurrency(double amount)
    {
        if (amount <= 0)
        {
            return;
        }
        Currency += amount;
        TotalEarned += amount;
        OnIncomeGained?.Invoke(amount);
    }

    public bool Spend(double amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Currency < amount)
        {
            return false;
        }

        Currency -= amount;
        return true;
    }

    public void ApplyBoost(float multiplier, float duration)
    {
        boostState.Start(multiplier, duration);
    }

    private double CalculateIncomePerSec()
    {
        var baseIncome = menuSystem.CalculateMenuIncome();
        var upgradeMultiplier = upgradeSystem.GetGlobalMultiplier();
        var staffMultiplier = upgradeSystem.GetCategoryMultiplier("staff");
        var serviceMultiplier = upgradeSystem.GetCategoryMultiplier("service");
        var storeMultiplier = storeTierSystem.CurrentTierMultiplier;
        var boostMultiplier = boostState.Multiplier;
        var tipMultiplier = customerSystem != null ? customerSystem.GetTipMultiplier() : 1f;
        var comboMultiplier = customerSystem != null ? customerSystem.GetComboMultiplier() : 1f;
        var prestigeMultiplier = prestigeSystem != null ? prestigeSystem.PrestigeMultiplier : 1.0;
        return baseIncome * upgradeMultiplier * staffMultiplier * serviceMultiplier * storeMultiplier * boostMultiplier * tipMultiplier * comboMultiplier * prestigeMultiplier;
    }

    private class BoostState
    {
        private float timer;
        private float multiplier = 1f;

        public float Multiplier => multiplier;

        public void Start(float newMultiplier, float duration)
        {
            multiplier = Math.Max(1f, newMultiplier);
            timer = Math.Max(0f, duration);
        }

        public void Tick(float dt)
        {
            if (timer <= 0f)
            {
                multiplier = 1f;
                return;
            }

            timer -= dt;
            if (timer <= 0f)
            {
                multiplier = 1f;
            }
        }
    }
}
