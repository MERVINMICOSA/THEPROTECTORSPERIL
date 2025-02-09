using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }
public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    int selectedItem = 0;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;
    [SerializeField] PartyScreen partyScreen;
    List<ItemSlotUI> slotUIList;

    InventoryUIState state;

    Inventory inventory;

    Action onItemUsed;
    private void Awake()
    {
        inventory = Inventory.GetInventory();
    }

    private void Start()
    {
        UpdateItemList();
        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        // clear all items
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.Slots)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }
        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            if (Input.GetKeyDown(KeyCode.DownArrow))
                ++selectedItem;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                --selectedItem;

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

            if (prevSelection != selectedItem)
                UpdateItemSelection();
            if (Input.GetKeyDown(KeyCode.Z))
                OpenPartyScreen();
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
    }


    IEnumerator UseItem()
    {

        state = InventoryUIState.Busy;
       var usedItem =  inventory.UseItem(selectedItem, partyScreen.SelectedMember);

        if (usedItem != null)
        {
          yield return  DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");
            onItemUsed?.Invoke();
        }
        else
        {
           yield return DialogManager.Instance.ShowDialogText($"The {usedItem.Name} won't have any effect!");
        }

        ClosePartyScreen();
    }
    void UpdateItemSelection()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

        var item = inventory.Slots[selectedItem].Item;
        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
    }
}
