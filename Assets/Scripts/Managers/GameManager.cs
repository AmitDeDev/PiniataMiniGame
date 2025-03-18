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
    
    private int piniatasOpenedSinceLastBomb = 0;
    private const int minPiniatasBeforeBombChance = 3;
    private const float BombGrantChance = 0.2f; 
    
    private int piñatasOpenedSinceLastCritical = 0;
    private const int MinPiniatasBeforeCriticalChance = 5;
    private const float CriticalGrantChance = 0.3f; 


    
    #region Events
    
    public event Action<int> OnScoreUpdated;
    // e.g. OnCooldownTriggered(isActive, duration) => View can show overlay
    public event Action<bool, float> OnCooldownTriggered; 
    public event Action<int> OnBombCountUpdated;
    public event Action<int> OnCriticalCountUpdated;

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
        OnBombCountUpdated?.Invoke(gameModel.BombCount);
        OnCriticalCountUpdated?.Invoke(gameModel.CriticalCount);

        SpawnNewPiniata();
    }

    private void Update()
    {
        UpdateTimer(Time.deltaTime);
    }
    
    public void OnPiniataClicked()
    {
        if (isCooldownActive) return;

        float timeSinceLastClick = Time.time - lastClickTime;
        lastClickTime = Time.time;

        if (piniataModel.ClickCount > 0 && !isPotentiallyOpened && timeSinceLastClick > maxClickInterval)
        {
            piniataModel.ClickCount = 0;
            gameView.ShowNotificationsWithTimer(2f, "Taking your time huh?");
            return;
        }

        int actualClickIncrement = 1;

        // If a critical is pending, add the extra value to this click
        if (gameModel.NextCriticalValue > 0)
        {
            actualClickIncrement += gameModel.NextCriticalValue;
            gameModel.NextCriticalValue = 0;
        }
        
        piniataModel.ClickCount += actualClickIncrement;
        gameView.SpawnHitParticles();
        
        if (isPotentiallyOpened)
        {
            StartCooldown();
            ResetClicks();
            return;
        }

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

    private void SpawnNewPiniata()
    {
        if (currentPiniata != null) 
            Destroy(currentPiniata);

        piniataModel.CurrentPiniataNum++;
        piniataModel.ClicksRequired = piniataModel.CurrentPiniataNum;
        piniataModel.ClickCount = 0;
        piniataModel.IsOnCooldown = false;
        piniataModel.IsOpened = false;
        
        gameView.UpdatePiniataNum(piniataModel.CurrentPiniataNum);
        
        currentPiniata = Instantiate(piniataPrefab, piniataSpawnPoint);
        Button piniataBtn = currentPiniata.GetComponent<Button>();
        piniataBtn.onClick.AddListener(() => OnPiniataClicked());
        piniataBtn.AddSquishEffect(0.8f, 0.1f);

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
        gameView.UpdateScore(gameModel.Score);
        
        piñatasOpenedSinceLastCritical++;
        TryGrantCritical();
        piniatasOpenedSinceLastBomb++;
        TryGrantBomb();

        Destroy(currentPiniata, 0.7f);
        SpawnNewPiniata();
        ResetClicks();
    }
    
    private void TryGrantCritical()
    {
        if (piñatasOpenedSinceLastCritical >= MinPiniatasBeforeCriticalChance)
        {
            float roll = UnityEngine.Random.value;
            if (roll <= CriticalGrantChance)
            {
                gameModel.CriticalCount++;
                OnCriticalCountUpdated?.Invoke(gameModel.CriticalCount);
                
                gameView.ShowNotificationsWithTimer(3f, "You've won A Critical Hit TNT!");
                
                piñatasOpenedSinceLastCritical = 0;
            }
        }
    }

    
    private void TryGrantBomb()
    {
        if (piniatasOpenedSinceLastBomb >= minPiniatasBeforeBombChance)
        {
            float roll = UnityEngine.Random.value; 
            if (roll <= BombGrantChance)
            {
                gameView.ShowNotificationsWithTimer(2f, "You've won A BOMB!"); 
                gameModel.BombCount++;
                OnBombCountUpdated?.Invoke(gameModel.BombCount);
                
                piniatasOpenedSinceLastBomb = 0;
            }
        }
    }
    
    public void OnBombButtonClicked()
    {
        if (gameModel.BombCount <= 0) return;
        
        gameModel.BombCount--;
        OnBombCountUpdated?.Invoke(gameModel.BombCount);
        
        if (!isCooldownActive && !piniataModel.IsOpened)
        {
            FinalizeOpen();
        }
    }
    
    public void OnCriticalButtonClicked()
    {
        if (gameModel.CriticalCount <= 0) return;
        
        gameModel.CriticalCount--;
        OnCriticalCountUpdated?.Invoke(gameModel.CriticalCount);
        
        int max = Mathf.Max(1, piniataModel.ClicksRequired - 1);
        int randomExtra = UnityEngine.Random.Range(1, max + 1);
        
        gameModel.NextCriticalValue = randomExtra;
        
        gameView.ShowNotificationsWithTimer(3f,
            $"Your next click = {randomExtra} clicks!");
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
