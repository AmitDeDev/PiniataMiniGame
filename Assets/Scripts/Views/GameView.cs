using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI piniataNumText;
    [SerializeField] private TextMeshProUGUI timeGapWarningText;
    [SerializeField] private GameObject piniataOnCooldownObj;
    [SerializeField] private ParticleSystem HitParticleSystem;

    private ParticleSystem piniataHitParticleInstance;

    public void UpdateScore(int _score)
    {
        scoreText.text = "Score: " + _score;
    }

    public void UpdateTimer(float _time)
    {
        timerText.text = "Time Left: " + Mathf.Ceil(_time);
    }
    public void UpdatePiniataNum(float _piniataNum)
    {
        piniataNumText.text = "Piniata: " + _piniataNum + "#";
    }
    
    public void SpawnHitParticles()
    {
        piniataHitParticleInstance =
            Instantiate(HitParticleSystem, piniataOnCooldownObj.transform.position, Quaternion.identity);
    }

    public void SetPiniataCooldown(bool _isActive, float _duration)
    {
        StartCoroutine(PiniataCooldown(_isActive, _duration));
    }
    
    private IEnumerator PiniataCooldown(bool _isActive, float _duration)
    {
        piniataOnCooldownObj.SetActive(_isActive);

        TextMeshProUGUI cooldownText = piniataOnCooldownObj.GetComponentInChildren<TextMeshProUGUI>();
        cooldownText.text = Mathf.Ceil(_duration).ToString();

        float remain = _duration;
        while (remain > 0f)
        {
            remain -= Time.deltaTime;
            cooldownText.text = Mathf.Ceil(remain).ToString();
            yield return null;
        }

        piniataOnCooldownObj.SetActive(!_isActive);
    }
    
    public void SetClickGapWarning(float _duration)
    {
        StartCoroutine(TimeGapWarning(_duration));
    }
    
    private IEnumerator TimeGapWarning(float _duration)
    {
        timeGapWarningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(_duration);

        timeGapWarningText.gameObject.SetActive(false);
    }
}
