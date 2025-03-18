using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonSquishExtensions
{
    public static void AddSquishEffect(this Button button, float scaleFactor, float duration)
    {
        Vector3 originalScale = button.transform.localScale;
        
        button.onClick.AddListener(() =>
        {
            button.transform.DOScale(originalScale * scaleFactor, duration).OnComplete(() =>
            {
                button.transform.DOScale(originalScale, duration)
                    .SetEase(Ease.OutBack); 
            });
        });
    }
}