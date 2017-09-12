using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class StockCount : MonoBehaviour {
    public List<Image> roundImages;

    public int roundLimit;
    private int stockCount;

    private float fillTime = 1.0f;
    private float timer = 0;

    void Start()
    {
        foreach (Image positionImage in roundImages)
        {
            positionImage.fillAmount = 0;
        }
    }

    void Update()
    {
        for (int i = 0; i < roundImages.Count; i++)
        {
            if(i < roundLimit)
                roundImages[i].gameObject.SetActive(true);
            else
                roundImages[i].gameObject.SetActive(false);
        }

        if (timer < fillTime)
            timer += Time.deltaTime;

        for (int i = 0; i < roundImages.Count; i++)
        {
            if (i < this.stockCount)
                roundImages[i].fillAmount = Mathf.Max(roundImages[i].fillAmount, timer / fillTime);
            else
                roundImages[i].fillAmount = 0;
        }
    }

    public void SetStockCount(int stockCount)
    {
        timer = 0;
        this.stockCount = stockCount;
    }
}
