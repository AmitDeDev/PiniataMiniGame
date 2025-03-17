using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameView gameView;
    [SerializeField] private Transform piniataSpawnPoint;
    [SerializeField] private GameObject piniataPrefab;

    [Header("Settings")]
    [SerializeField] private float maxClickInterval = 2f;
    [SerializeField] private float cooldownDuration = 3f;
    [SerializeField] private float overshootGracePeriod = 1f;

    #region Models
    
    private GameModel gameModel;
    private PiniataModel piniataModel;
    
    #endregion

    private float lastClickTime;
    private bool isCooldownActive;
    private bool isPotentiallyOpened;
    private Coroutine openDelayCoroutine;
    private GameObject currentPiniata;

    #region Events
    public event Action<int> OnScoreUpdated;
    public event Action<bool, float> OnCooldownTriggered; 
    // e.g. OnCooldownTriggered(isActive, duration) => View can show overlay
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        gameModel = new GameModel();
        piniataModel = new PiniataModel();
        
        gameView.Init(this); 
        gameView.UpdateScore(gameModel.Score);
        gameView.UpdateTimer(gameModel.Timer);
        gameView.UpdatePiniataNum(piniataModel.CurrentPiniataNum);

        SpawnNewPiñata();
    }

    private void Update()
    {
        UpdateTimer(Time.deltaTime);
    }
    
    public void OnPiñataClicked()
    {
        if (isCooldownActive) return;

        float timeSinceLastClick = Time.time - lastClickTime;
        lastClickTime = Time.time;

        // Gap check
        if (piniataModel.ClickCount > 0 && !isPotentiallyOpened && timeSinceLastClick > maxClickInterval)
        {
            piniataModel.ClickCount = 0;
            gameView.ShowTimeGapWarning(2f); 
            return;
        }

        piniataModel.ClickCount++;
        gameView.SpawnHitParticles();

        // If already reached required clicks and are in time period => overshoot
        if (isPotentiallyOpened)
        {
            StartCooldown();
            ResetClicks();
            return;
        }

        // Check exact vs overshoot
        if (piniataModel.ClickCount == piniataModel.ClicksRequired)
        {
            isPotentiallyOpened = true;
            openDelayCoroutine = StartCoroutine(CoFinalizeOpenAfterDelay(overshootGracePeriod));
        }
        else if (piniataModel.ClickCount > piniataModel.ClicksRequired)
        {
            StartCooldown();
            ResetClicks();
        }
    }

    #region Core Logic

    private void SpawnNewPiñata()
    {
        if (currentPiniata != null) Destroy(currentPiniata);

        piniataModel.CurrentPiniataNum++;
        piniataModel.ClicksRequired = piniataModel.CurrentPiniataNum;
        piniataModel.ClickCount = 0;
        piniataModel.IsOnCooldown = false;
        piniataModel.IsOpened = false;
        
        gameView.UpdatePiniataNum(piniataModel.CurrentPiniataNum);
        
        currentPiniata = Instantiate(piniataPrefab, piniataSpawnPoint);
        currentPiniata.GetComponent<Button>().onClick.AddListener(() => OnPiñataClicked());

        lastClickTime = Time.time;
    }

    private IEnumerator CoFinalizeOpenAfterDelay(float delay)
    {
        float timer = delay;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        
        if (!isCooldownActive)
        {
            FinalizeOpen();
        }
    }

    private void FinalizeOpen()
    {
        piniataModel.IsOpened = true;

        gameModel.Score += piniataModel.CurrentPiniataNum;
        OnScoreUpdated?.Invoke(gameModel.Score);

        Destroy(currentPiniata, 0.7f);
        SpawnNewPiñata();
        ResetClicks();
    }

    private void StartCooldown()
    {
        if (!isCooldownActive) StartCoroutine(CoHandleCooldown(cooldownDuration));
    }

    private IEnumerator CoHandleCooldown(float duration)
    {
        isCooldownActive = true;
        piniataModel.IsOnCooldown = true;
        
        OnCooldownTriggered?.Invoke(true, duration);

        yield return new WaitForSeconds(duration);

        isCooldownActive = false;
        piniataModel.IsOnCooldown = false;
        
        OnCooldownTriggered?.Invoke(false, 0f);
    }

    private void ResetClicks()
    {
        piniataModel.ClickCount = 0;
        isPotentiallyOpened = false;
        if (openDelayCoroutine != null)
        {
            StopCoroutine(openDelayCoroutine);
            openDelayCoroutine = null;
        }
    }

    private void UpdateTimer(float deltaTime)
    {
        if (gameModel.Timer > 0)
        {
            gameModel.Timer -= deltaTime;
            gameView.UpdateTimer(gameModel.Timer);
        }
        if (gameModel.Timer <= 0)
        {
            gameModel.Timer = 0;
            gameView.UpdateTimer(gameModel.Timer);
        }
    }

    #endregion
}
