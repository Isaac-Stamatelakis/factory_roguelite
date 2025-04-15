using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevTools.ItemImageGenerator
{
    public class ItemImageGeneratorUI : MonoBehaviour
    {
        [SerializeField] private Button mGenerateButton;
        [SerializeField] private ItemImageGenerator mItemImageGeneratorPrefab;
        
        
        public void Initialize()
        {
            mGenerateButton.onClick.AddListener(OnGenerateButtonClicked);
        }

        public void OnGenerateButtonClicked()
        {

            mGenerateButton.interactable = false;
            ItemImageGenerator itemImageGenerator = Instantiate(mItemImageGeneratorPrefab);
            StartCoroutine(itemImageGenerator.CaptureCoroutine(ItemRegistry.GetInstance().GetAllItems(), () =>
            {
                mGenerateButton.interactable = true;
            }));


        }
    }
}
