using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI piniataNumText;
    [SerializeField] private TextMeshProUGUI bombCountText;
    [SerializeField] private TextMeshProUGUI criticalCountText;
    
    [Header("GameObjects and UI")]
    [SerializeField] private GameObject piniataOnCooldownObj;
    [SerializeField] private Button bombButton;
    [FormerlySerializedAs("criticalHitButton")] [SerializeField] private Button criticalButton;
    [SerializeField] private GameObject Notifications;
    
    [Header("Effects and Animations")]
    [SerializeField] private ParticleSystem hitParticleSystem;

    private ParticleSystem piniataHitParticleInstance;

    public void Init(GameManager manager)
    {
        manager.OnScoreUpdated += UpdateScore;
        manager.OnCooldownTriggered += HandleCooldownOverlay;
        manager.OnBombCountUpdated += UpdateBombUI; 
        manager.OnCriticalCountUpdated += UpdateCriticalUI;
        
        bombButton.AddSquishEffect(0.8f,0.1f);
        bombButton.onClick.AddListener(() => manager.OnBombButtonClicked());
        
        criticalButton.AddSquishEffect(0.8f, 0.1f);
        criticalButton.onClick.AddListener(() => manager.OnCriticalButtonClicked());
    
        Notifications.gameObject.SetActive(false);
        piniataOnCooldownObj.SetActive(false);
        bombButton.gameObject.SetActive(true);
    }


    public void UpdateScore(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
    }

    public void UpdateTimer(float timeLeft)
    {
        timerText.text = "Time Left: " + Mathf.Ceil(timeLeft);
    }

    public void UpdatePiniataNum(float piñataNum)
    {
        piniataNumText.text = "Piniata: " + piñataNum + "#";
    }

    public void ShowNotificationsWithTimer(float duration, string text)
    {
        StartCoroutine(Notification(duration,text));
    }
    
    private void UpdateBombUI(int newBombCount)
    {
        bombCountText.text = $"x{newBombCount}";
        bombButton.interactable = (newBombCount > 0);
    }
    
    private void UpdateCriticalUI(int newCriticalCount)
    {
        criticalCountText.text = $"x{newCriticalCount}";
        criticalButton.interactable = (newCriticalCount > 0);
    }



    private IEnumerator Notification(float duration, string text)
    {
        Notifications.gameObject.SetActive(true);
        Notifications.GetComponentInChildren<TextMeshProUGUI>().text = text;
        yield return new WaitForSeconds(duration);
        Notifications.gameObject.SetActive(false);
    }

    public void SpawnHitParticles()
    {
        piniataHitParticleInstance = Instantiate(
            hitParticleSystem,
            piniataOnCooldownObj.transform.position,
            Quaternion.identity
        );
    }
    
    private void HandleCooldownOverlay(bool isActive, float cdDuration)
    {
        if (isActive)
        {
            piniataOnCooldownObj.SetActive(true);
            StartCoroutine(ShowCooldownCountdown(cdDuration));
        }
        else
        {
            piniataOnCooldownObj.SetActive(false);
        }
    }

    private IEnumerator ShowCooldownCountdown(float cdDuration)
    {
        TextMeshProUGUI cooldownText = piniataOnCooldownObj.GetComponentInChildren<TextMeshProUGUI>();
        float remain = cdDuration;
        while (remain > 0f)
        {
            remain -= Time.deltaTime;
            cooldownText.text = Mathf.Ceil(remain).ToString();
            yield return null;
        }
        piniataOnCooldownObj.SetActive(false);
    }
}
