using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KitchenPizzaPlus.Customs.Dishes
{
    public class AlfredoPizza : CustomDish
    {
        public override string UniqueNameID => "AlfredoPizzaDish";
        public override DishType Type => DishType.Extra;
        public override int Difficulty => 2;
        public override GameObject IconPrefab => (GDOUtils.GetExistingGDO(DishReferences.PizzaBase) as Dish).IconPrefab;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;
        public override CardType CardType => CardType.Default;
        public override Unlock.RewardLevel ExpReward => Unlock.RewardLevel.Medium;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override bool DestroyAfterModUninstall => false;
        public override bool IsUnlockable => true;

        public override List<Unlock> HardcodedRequirements => new()
        {
            (Dish)GDOUtils.GetExistingGDO(DishReferences.PizzaBase)
        };

        public override HashSet<Dish.IngredientUnlock> IngredientsUnlocks => new HashSet<Dish.IngredientUnlock>
        {
            new Dish.IngredientUnlock
            {
                Ingredient = (Item)GDOUtils.GetExistingGDO(ItemReferences.PizzaPlated),
                MenuItem = (ItemGroup)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion)
            }
        };

        public override HashSet<Item> MinimumIngredients => new HashSet<Item>
        {
            (Item)GDOUtils.GetExistingGDO(ItemReferences.Flour),
            (Item)GDOUtils.GetExistingGDO(ItemReferences.Oil),
            (Item)GDOUtils.GetExistingGDO(ItemReferences.Cheese),
            (Item)GDOUtils.GetExistingGDO(ItemReferences.Plate),
            (Item)GDOUtils.GetExistingGDO(ItemReferences.Pot),
            (Item)GDOUtils.GetExistingGDO(ItemReferences.Milk),
            (Item)GDOUtils.GetExistingGDO(ItemReferences.Butter),
        };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.Knead),
        };

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Add butter and flour to pot and cook. Add milk and mix. Add more milk and mix again. Add sauce to raw pizza crust before adding cheese." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new()
        {
            (Locale.English, new UnlockInfo
            {
                Name = "Alfredo Pizza",
                Description = "Adds alfredo as a pizza sauce",
                FlavourText = ""
            })
        };
    }
}
