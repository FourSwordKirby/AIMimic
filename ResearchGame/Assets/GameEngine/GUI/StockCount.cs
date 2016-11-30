using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class StockCount : MonoBehaviour {
    public List<Image> stockImages;
    public Image multipleStockImage;

    public Sprite stockSprite;
    public Sprite multipleStockSprite;

    private int stockCount;

    void Start()
    {
        foreach (Image positionImage in stockImages)
        {
            positionImage.enabled = false;
            positionImage.sprite = stockSprite;
        }

        multipleStockImage.enabled = false;
        multipleStockImage.sprite = multipleStockSprite;
    }

    public void SetStockCount(int stockCount)
    {
        this.stockCount = stockCount;

        if (this.stockCount > 5)
        {
            foreach (Image positionImage in stockImages)
            {
                positionImage.enabled = false;
            }
            multipleStockImage.enabled = true;
        }
        else
        {
            for (int i = 0; i < stockImages.Count; i++)
            {
                if(i < this.stockCount)
                    stockImages[i].enabled = true;
                else
                    stockImages[i].enabled = false;
            }

            multipleStockImage.enabled = false;
        }
    }
}
