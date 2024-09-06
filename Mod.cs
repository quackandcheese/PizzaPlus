using Kitchen;
using KitchenData;
using KitchenLib;
using KitchenLib.Colorblind;
using KitchenLib.Event;
using KitchenLib.Interfaces;
using KitchenLib.Logging;
using KitchenLib.Logging.Exceptions;
using KitchenLib.References;
using KitchenLib.Utils;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using static KitchenData.ItemGroup;
using KitchenLogger = KitchenLib.Logging.KitchenLogger;

namespace KitchenPizzaPlus
{
    public class Mod : BaseMod, IModSystem, IAutoRegisterAll
    {
        public const string MOD_GUID = "com.quackandcheese.pizzaplus";
        public const string MOD_NAME = "PizzaPlus";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "QuackAndCheese";
        public const string MOD_GAMEVERSION = ">=1.1.9";

        internal static AssetBundle Bundle;
        internal static KitchenLogger Logger;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            Logger.LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).FirstOrDefault() ?? throw new MissingAssetBundleException(MOD_GUID);
            Logger = InitLogger();

            Events.BuildGameDataEvent += delegate (object s, BuildGameDataEventArgs args)
            {
                ItemGroup rawPizza = (ItemGroup)GDOUtils.GetExistingGDO(ItemReferences.PizzaRaw);

                ItemGroup cookedPizza = (ItemGroup)GDOUtils.GetExistingGDO(ItemReferences.PizzaCooked);

                ItemGroup pizzaSlice = (ItemGroup)GDOUtils.GetExistingGDO(ItemReferences.PizzaSlice);

                ItemGroup platedPizzaSlice = (ItemGroup)GDOUtils.GetExistingGDO(ItemReferences.PizzaPlated);
                // Create new alfredo sauce for each slice

                if (args.firstBuild)
                {
                    SetupPizzaPrefab(rawPizza, "AlfredoSauce", ItemReferences.RouxPortion, "Alf");
                    SetupPizzaPrefab(cookedPizza, "AlfredoSauce", ItemReferences.RouxPortion, "Alf");
                    SetupPizzaPrefab(pizzaSlice, "AlfredoSauce", ItemReferences.RouxPortion, "Alf", isSlice: true);
                    SetupPizzaPrefab(platedPizzaSlice, "AlfredoSauce", ItemReferences.RouxPortion, "Alf", "Pizza Slice/Slice", isSlice: true);
                }

                SetupPizzaItemSets(rawPizza, ItemReferences.RouxPortion);
                SetupPizzaItemSets(platedPizzaSlice, ItemReferences.RouxPortion);
            };
        }

        public void AddColorblindLabel(Item pizzaItem, int itemReference, String label)
        {
            GameObject go = pizzaItem.Prefab;

            if (pizzaItem is ItemGroup)
            {
                ItemGroupView itemGroupView = go.GetComponent<ItemGroupView>();
                FieldInfo componentLabelsField = ReflectionUtils.GetField<ItemGroupView>("ComponentLabels");
                List<ItemGroupView.ColourBlindLabel> colorblindLabels = componentLabelsField.GetValue(itemGroupView) as List<ItemGroupView.ColourBlindLabel>;

                colorblindLabels.Add(new()
                {
                    Item = (Item)GDOUtils.GetExistingGDO(itemReference),
                    Text = label
                }
                );

                componentLabelsField.SetValue(itemGroupView, colorblindLabels);
            }
            else
            {
                /*Item steak = (Item)GDOUtils.GetExistingGDO(ItemReferences.SteakMedium);
                if (steak != null)
                {
                    GameObject ColorBlind = GameObject.Instantiate(steak.Prefab.transform.Find("Colour Blind").gameObject);
                    ColorBlind.name = "Colour Blind";
                    ColorBlind.transform.SetParent(go.transform);
                    ColorBlind.transform.Find("Title").GetComponent<TMP_Text>().text = label;
                }*/
            }
        }

        public void SetupPizzaPrefab(Item pizzaItemGroup, String saucePrefabName, int itemReference, String label, String slicePath = "Slice", bool isSlice = false)
        {
            GameObject pizzaGo = pizzaItemGroup.Prefab;
            ItemGroupView itemGroupView = pizzaGo.GetComponent<ItemGroupView>();
            List<GameObject> alfredoSauces = new List<GameObject>();

            if (isSlice)
            {
                GameObject sauceGo = GameObject.Instantiate(Bundle.LoadAsset<GameObject>(saucePrefabName)).AssignMaterialsByNames();
                sauceGo.transform.SetParent(pizzaGo.transform.Find(slicePath), false);
                itemGroupView.ComponentGroups.Add(new()
                {
                    DrawAll = true,
                    GameObject = sauceGo,
                    Item = (Item)GDOUtils.GetExistingGDO(itemReference),
                });
                itemGroupView.ComponentGroups.Add(new()
                {
                    DrawAll = true,
                    GameObject = pizzaGo.transform.Find(slicePath + "/Sauce").gameObject,
                    Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce),
                });
            }
            else
            {
                foreach (ItemGroupView sliceItemGroupView in pizzaGo.GetComponentsInChildren<ItemGroupView>())
                {
                    GameObject sauceGo = GameObject.Instantiate(Bundle.LoadAsset<GameObject>(saucePrefabName)).AssignMaterialsByNames();
                    sauceGo.transform.SetParent(sliceItemGroupView.transform.Find(slicePath), false);
                    alfredoSauces.Add(sauceGo);
                }

                // Add roux + sauce game object to component groups, replicating tomato sauce
                itemGroupView.ComponentGroups.Add(new()
                {
                    DrawAll = true,
                    Objects = alfredoSauces,
                    Item = (Item)GDOUtils.GetExistingGDO(itemReference),
                });
            }

            AddColorblindLabel(pizzaItemGroup, itemReference, label);
        }
        
        public void SetupPizzaItemSets(ItemGroup pizzaItemGroup, int itemReference)
        {
            ItemSet saucesItemSet = default(ItemSet);
            int sauceItemSetIndex = -1;

            ItemSet pizzaItemSet = default(ItemSet);
            int index = -1;
            for (int i = 0; i < pizzaItemGroup.DerivedSets.Count; i++)
            {
                ItemSet itemSet = pizzaItemGroup.DerivedSets[i];
                if (itemSet.Items.Contains((Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce)) && itemSet.Items.Contains((Item)GDOUtils.GetExistingGDO(ItemReferences.PizzaCrust)))
                {
                    pizzaItemSet = itemSet;
                    index = i;
                }
                else if (itemSet.Items.Contains((Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce)))
                {
                    saucesItemSet = itemSet;
                    sauceItemSetIndex = i;
                }
            }
            if (!pizzaItemSet.Equals(default(ItemSet)) && index != -1)
            {
                Item tomatoSauce = (Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce);
                if (pizzaItemSet.Items.Contains(tomatoSauce))
                {
                    pizzaItemSet.Items.Remove(tomatoSauce);
                    pizzaItemSet.Max = 1;
                    pizzaItemSet.Min = 1;
                    pizzaItemGroup.DerivedSets[index] = pizzaItemSet;
                }
                pizzaItemGroup.DerivedSets.Add(new()
                {
                    Max = 1,
                    Min = 1,
                    RequiresUnlock = false,
                    IsMandatory = true,
                    Items = new List<Item>()
                    {
                        (Item)GDOUtils.GetExistingGDO(itemReference),
                        (Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce)
                    }
                });
            }

            if (!saucesItemSet.Equals(default(ItemSet)) && sauceItemSetIndex != -1)
            {
                saucesItemSet.Items.Add((Item)GDOUtils.GetExistingGDO(itemReference));
                pizzaItemGroup.DerivedSets[sauceItemSetIndex] = saucesItemSet;
            }
        }
    }
}