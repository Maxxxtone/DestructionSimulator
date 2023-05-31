using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelDestruction
{
    public class Demo_SimpleInventory : MonoBehaviour
    {
        public Item[] items;

        private int currentItem;
        
        private void Start()
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i].overlay.gameObject.SetActive(false);
            }

            currentItem = 0;
            items[currentItem].overlay.gameObject.SetActive(true);
            
            for (int j = 0; j < items[currentItem].activateObj.Length; j++)
            {
                items[currentItem].activateObj[j].SetActive(true);
            }
            for (int j = 0; j < items[currentItem].deactivateObj.Length; j++)
            {
                items[currentItem].deactivateObj[j].SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                int next = currentItem + Convert.ToInt32(-Input.mouseScrollDelta.y);
                if (next >= items.Length)
                    next = 0;
                if (next < 0)
                    next = items.Length - 1;
                
                SelectItem(next);
            }
        }

        private void SelectItem(int i)
        {
            for (int j = 0; j < items[currentItem].activateObj.Length; j++)
            {
                items[currentItem].activateObj[j].SetActive(false);
            }
            for (int j = 0; j < items[currentItem].deactivateObj.Length; j++)
            {
                items[currentItem].deactivateObj[j].SetActive(true);
            }
            
            items[currentItem].overlay.gameObject.SetActive(false);
            currentItem = i;
            items[currentItem].overlay.gameObject.SetActive(true);
            
            for (int j = 0; j < items[currentItem].activateObj.Length; j++)
            {
                items[currentItem].activateObj[j].SetActive(true);
            }
            for (int j = 0; j < items[currentItem].deactivateObj.Length; j++)
            {
                items[currentItem].deactivateObj[j].SetActive(false);
            }
        }
    }

    [System.Serializable]
    public class Item
    {
        public Image overlay;

        public GameObject[] activateObj;
        public GameObject[] deactivateObj;
    }
}