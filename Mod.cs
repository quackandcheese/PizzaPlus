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
                GameObject rawPizzaGo = rawPizza.Prefab;

                ItemGroup cookedPizza = (ItemGroup)GDOUtils.GetExistingGDO(ItemReferences.PizzaCooked);
                GameObject cookedPizzaGo = cookedPizza.Prefab;

                ItemGroup pizzaSlice = (ItemGroup)GDOUtils.GetExistingGDO(ItemReferences.PizzaSlice);
                GameObject pizzaSliceGo = pizzaSlice.Prefab;

                ItemGroup platedPizzaSlice = (ItemGroup)GDOUtils.GetExistingGDO(ItemReferences.PizzaPlated);
                GameObject platedPizzaSliceGo = platedPizzaSlice.Prefab;
                // Create new alfredo sauce for each slice

                if (args.firstBuild)
                {
                    // Raw
                    List<GameObject> alfredoSauces = new List<GameObject>();
                    foreach (ItemGroupView sliceItemGroupView in rawPizzaGo.GetComponentsInChildren<ItemGroupView>())
                    {
                        GameObject alfredoGo = GameObject.Instantiate(Bundle.LoadAsset<GameObject>("AlfredoSauce")).AssignMaterialsByNames();
                        alfredoGo.transform.SetParent(sliceItemGroupView.transform.Find("Slice"), false);
                        alfredoSauces.Add(alfredoGo);
                    }

                    // Add roux + alfredo sauce game object to component groups, replicating tomato sauce
                    rawPizzaGo.GetComponent<ItemGroupView>().ComponentGroups.Add(new()
                    {
                        DrawAll = true,
                        Objects = alfredoSauces,
                        Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion),
                    });


                    // Cooked
                    List<GameObject> cookedAlfredoSauces = new List<GameObject>();
                    foreach (ItemGroupView sliceItemGroupView in cookedPizzaGo.GetComponentsInChildren<ItemGroupView>())
                    {
                        GameObject alfredoGo = GameObject.Instantiate(Bundle.LoadAsset<GameObject>("AlfredoSauce")).AssignMaterialsByNames();
                        alfredoGo.transform.SetParent(sliceItemGroupView.transform.Find("Slice"), false);
                        cookedAlfredoSauces.Add(alfredoGo);
                    }

                    // Add roux + alfredo sauce game object to component groups, replicating tomato sauce
                    cookedPizzaGo.GetComponent<ItemGroupView>().ComponentGroups.Add(new()
                    {
                        DrawAll = true,
                        Objects = cookedAlfredoSauces,
                        Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion),
                    });
                    

                    GameObject alfredoGoSlice = GameObject.Instantiate(Bundle.LoadAsset<GameObject>("AlfredoSauce")).AssignMaterialsByNames();
                    alfredoGoSlice.transform.SetParent(pizzaSliceGo.transform.Find("Slice"), false);
                    pizzaSliceGo.GetComponent<ItemGroupView>().ComponentGroups.Add(new()
                    {
                        DrawAll = true,
                        GameObject = alfredoGoSlice,
                        Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion),
                    });
                    pizzaSliceGo.GetComponent<ItemGroupView>().ComponentGroups.Add(new()
                    {
                        DrawAll = true,
                        GameObject = pizzaSliceGo.transform.Find("Slice/Sauce").gameObject,
                        Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce),
                    });


                    GameObject alfredoGoPlatedSlice = GameObject.Instantiate(Bundle.LoadAsset<GameObject>("AlfredoSauce")).AssignMaterialsByNames();
                    alfredoGoPlatedSlice.transform.SetParent(platedPizzaSliceGo.transform.Find("Pizza Slice/Slice"), false);
                    platedPizzaSliceGo.GetComponent<ItemGroupView>().ComponentGroups.Add(new()
                    {
                        DrawAll = true,
                        GameObject = alfredoGoPlatedSlice,
                        Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion),
                    });
                    platedPizzaSliceGo.GetComponent<ItemGroupView>().ComponentGroups.Add(new()
                    {
                        DrawAll = true,
                        GameObject = platedPizzaSliceGo.transform.Find("Pizza Slice/Slice/Sauce").gameObject,
                        Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce),
                    });

                    // Add labels
                    AddColorblindLabel(rawPizzaGo);
                    AddColorblindLabel(cookedPizzaGo);
                    //AddColorblindLabel(pizzaSliceGo);
                    Item steak = (Item)GDOUtils.GetExistingGDO(ItemReferences.SteakMedium);
                    if (steak != null)
                    {
                        GameObject ColorBlind = GameObject.Instantiate(steak.Prefab.transform.Find("Colour Blind").gameObject);
                        ColorBlind.name = "Colour Blind";
                        ColorBlind.transform.SetParent(pizzaSliceGo.transform);
                        ColorBlind.transform.Find("Title").GetComponent<TMP_Text>().text = "Alf";
                    }
                    AddColorblindLabel(platedPizzaSliceGo);
                }


                // Remove tomato sauce from ItemSet with crust and sauce to make new ItemSet with tomato sauce OR roux

                ItemSet rawPizzaItemSet = rawPizza.DerivedSets[1];
                Item tomatoSauce = (Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce);
                if (rawPizzaItemSet.Items.Contains(tomatoSauce))
                {
                    rawPizzaItemSet.Items.Remove(tomatoSauce);
                    rawPizzaItemSet.Max = 1;
                    rawPizzaItemSet.Min = 1;
                    rawPizza.DerivedSets[1] = rawPizzaItemSet;
                }
                
                rawPizza.DerivedSets.Add(new()
                {
                    Max = 1,
                    Min = 1,
                    RequiresUnlock = false,
                    IsMandatory = true,
                    Items = new List<Item>()
                    {
                        (Item)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion),
                        (Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce)
                    }
                });


                ItemSet platedPizzaItemSet = platedPizzaSlice.DerivedSets[0];
                if (platedPizzaItemSet.Items.Contains(tomatoSauce))
                {
                    platedPizzaItemSet.Items.Remove(tomatoSauce);
                    platedPizzaItemSet.Max = 1;
                    platedPizzaItemSet.Min = 1;
                    platedPizzaSlice.DerivedSets[0] = platedPizzaItemSet;
                }

                platedPizzaSlice.DerivedSets.Add(new()
                {
                    Max = 1,
                    Min = 1,
                    RequiresUnlock = false,
                    IsMandatory = true,
                    Items = new List<Item>()
                    {
                        (Item)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion),
                        (Item)GDOUtils.GetExistingGDO(ItemReferences.TomatoSauce)
                    }
                });


                /* ReflectionUtils.GetField<List<ItemGroupView.ColourBlindLabel>>("ComponentLabels").SetValue(rawPizzaGo, new List<ItemGroupView.ColourBlindLabel>()
                 {
                     new()
                     {
                         Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion),
                         Text = "Alf"
                     }
                 });*/
            };
        }

        public void AddColorblindLabel(GameObject go)
        {
            ItemGroupView itemGroupView = go.GetComponent<ItemGroupView>();
            FieldInfo componentLabelsField = ReflectionUtils.GetField<ItemGroupView>("ComponentLabels");
            List<ItemGroupView.ColourBlindLabel> colorblindLabels = componentLabelsField.GetValue(itemGroupView) as List<ItemGroupView.ColourBlindLabel>;

            colorblindLabels.Add(new()
            {
                Item = (Item)GDOUtils.GetExistingGDO(ItemReferences.RouxPortion),
                Text = "Alf"
            }
            );

            componentLabelsField.SetValue(itemGroupView, colorblindLabels);
        }
    }
}