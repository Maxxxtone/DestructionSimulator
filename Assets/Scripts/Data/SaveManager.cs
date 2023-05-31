using InstantGamesBridge;
using InstantGamesBridge.Modules.Storage;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string OpenedState = "Open", ChosenState = "Chosen", LevelKey = "LastLevel";
    public Action DataLoaded;
    public Action<int> LevelLoaded;
    //public Action<CharacterSkin> SelectedSkinLoaded;    
    [SerializeField] private ItemsDatabase _database;
    private void Start()
    {
        //ClearData();
    }
    public void ClearData()
    {
        var keys = new List<string>();
        foreach (var item in _database.WeaponsItems)
        {
            keys.Add(item.Key + OpenedState);
            keys.Add(item.Key + ChosenState);
        }
        foreach (var item in _database.LevelsItems)
        {
            keys.Add(item.Key + OpenedState);
        }
        keys.Add("Money");
        Bridge.storage.Delete(keys, null, StorageType.LocalStorage);
    }
    public void LoadData()
    {
        _database.SetWeaponsKeys();
        _database.SetLevelsKeys();
        int openedSkins = 0;
        var openWeaponsKeys = new List<string>();
        var chosenWeaponsKeys = new List<string>();
        var openLevelsKeys = new List<string>();
        foreach (var item in _database.WeaponsItems)
        {
            openWeaponsKeys.Add(item.Key + OpenedState);
            chosenWeaponsKeys.Add(item.Key + ChosenState);
        }
        foreach (var item in _database.LevelsItems)
        {
            openLevelsKeys.Add(item.Key + OpenedState);
        }
        #region LoadWeapons
        Bridge.storage.Get(openWeaponsKeys, (success, data) =>
        {
            if (success)
            {
                if (data != null)
                {
                    for (int i = 0; i < _database.WeaponsItems.Length; i++)
                    {
                        var opened = Convert.ToBoolean(data[i]);
                        _database.WeaponsItems[i].Opened = opened;
                        if (opened)
                            openedSkins++;
                    }
                    if (openedSkins == 0)
                    {
                        _database.WeaponsItems[0].Opened = true;
                        print("0 OPENED SKINS FOUND");
                    }
                }
                else
                {
                    print("OPEN DATA FUCK");
                }
            }
        }, StorageType.LocalStorage);

        Bridge.storage.Get(chosenWeaponsKeys, (success, data) =>
        {
            if (success)
            {
                if (data != null)
                {
                    for (int i = 0; i < _database.WeaponsItems.Length; i++)
                    {
                        _database.WeaponsItems[i].Selected = Convert.ToBoolean(data[i]);
                    }
                    if(openedSkins == 0)
                        _database.WeaponsItems[0].Selected = true;
                }
                else
                {
                    print("CHOSE DATA FUCK");
                }
            }
        }, StorageType.LocalStorage);
        #endregion
        #region LoadLevels
        int levelsOpened = 0;
        Bridge.storage.Get(openLevelsKeys, (success, data) =>
        {
            if (success)
            {
                if (data != null)
                {
                    for (int i = 0; i < _database.LevelsItems.Length; i++)
                    {
                        var opened = Convert.ToBoolean(data[i]);
                        _database.LevelsItems[i].Opened = opened;
                        if (opened)
                            levelsOpened++;
                    }
                    if(levelsOpened == 0)
                        _database.LevelsItems[0].Opened = true;
                }
                else
                {
                    print("LEVELS OPEN DATA FUCK");
                }
            }
        }, StorageType.LocalStorage);
        #endregion
        Bridge.storage.Get("Money", (success, data) =>
        {
            if (data != null)
            {
                print($"MONEY DATA {data}");
                _database.Money = Convert.ToInt32(data);
                //database.ShowMoney.invoke
                DataLoaded?.Invoke();
            }
            else
            {
                print("MONEY DATA NOT SET");
                SaveMoney(0);
                DataLoaded?.Invoke();
            }
        }, StorageType.LocalStorage);
        Bridge.storage.Get("Sound", (success, data) =>
        {
            if (data != null)
            {
                print($"SOUND DATA {data}");
                _database.SoundOn = Convert.ToBoolean(data);
                if (!_database.SoundOn)
                    AudioListener.volume = 0;
            }
            else
            {
                print("SOUND DATA NOT SET");
                _database.SoundOn = true;
            }
        }, StorageType.LocalStorage);
        Bridge.storage.Get("Sensetivity", (success, data) =>
        {
            if (data != null)
            {
                print($"SENSETIVITY DATA {data}");
                _database.Sensetivity = float.Parse(data);
            }
            else
            {
                print("SENSETIVITY DATA NOT SET");
                _database.Sensetivity = 2f;
            }
        }, StorageType.LocalStorage);
    }
    public void SaveMoney()
    {
        Bridge.storage.Set("Money", _database.Money, null, StorageType.LocalStorage);
    }
    public void SaveMoney(int money)
    {
        Bridge.storage.Set("Money", money, null, StorageType.LocalStorage);
    }
    public void SaveSettings(float sensetivity, bool soundState)
    {
        Bridge.storage.Set("Sensetivity", sensetivity.ToString(), null, StorageType.LocalStorage);
        Bridge.storage.Set("Sound", soundState, null, StorageType.LocalStorage);
    }
    public void SaveLevel(int level)
    {
        Bridge.storage.Set(LevelKey, level, null, StorageType.LocalStorage);
    }
    public void SaveLevelsData()
    {
        var openLevelsKeys = new List<string>();
        var openLevelsState = new List<object>();
        foreach (var item in _database.LevelsItems)
        {
            openLevelsKeys.Add(item.Key + OpenedState);
            openLevelsState.Add(item.Opened);
        }
        Bridge.storage.Set(openLevelsKeys, openLevelsState,null, StorageType.LocalStorage);
    }
    public void SaveWeaponsData()
    {
        var openWeaponsKeys = new List<string>();
        var openWeaponsState = new List<object>();
        var selectWeaponsKeys = new List<string>();
        var selectWeaponsState = new List<object>();
        foreach (var item in _database.WeaponsItems)
        {
            openWeaponsKeys.Add(item.Key + OpenedState);
            openWeaponsState.Add(item.Opened);
            selectWeaponsKeys.Add(item.Key + ChosenState);
            selectWeaponsState.Add(item.Selected);
        }
        Bridge.storage.Set(openWeaponsKeys, openWeaponsState, null, StorageType.LocalStorage);
        Bridge.storage.Set(selectWeaponsKeys, selectWeaponsState, null, StorageType.LocalStorage);
    }
}
