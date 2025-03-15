using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
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
        piniataOnCooldownObj.GameObject().SetActive(_isActive);
        TextMeshProUGUI cooldownText = piniataOnCooldownObj.GetComponentInChildren<TextMeshProUGUI>();
        cooldownText.text = Mathf.Ceil(3f).ToString();
        yield return new WaitForSeconds(_duration);
        piniataOnCooldownObj.GameObject().SetActive(!_isActive);
    }
}
