using System.Collections;
using UnityEngine;
using TMPro;

public class GameView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI piniataNumText;
    [SerializeField] private TextMeshProUGUI timeGapWarningText;
    
    [Header("GameObjects and UI")]
    [SerializeField] private GameObject piniataOnCooldownObj;
    
    [Header("Effects and Animations")]
    [SerializeField] private ParticleSystem hitParticleSystem;

    private ParticleSystem piniataHitParticleInstance;

    public void Init(GameManager manager)
    {
        manager.OnScoreUpdated += UpdateScore;
        manager.OnCooldownTriggered += HandleCooldownOverlay;

        // if I need to subscribe to more events, I need to do that here
        timeGapWarningText.gameObject.SetActive(false);
        piniataOnCooldownObj.SetActive(false);
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

    public void ShowTimeGapWarning(float duration)
    {
        StartCoroutine(TimeGapWarning(duration));
    }

    private System.Collections.IEnumerator TimeGapWarning(float duration)
    {
        timeGapWarningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        timeGapWarningText.gameObject.SetActive(false);
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
