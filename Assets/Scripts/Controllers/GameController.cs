using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameView gameView;
    [SerializeField] private Transform piniataSpawnPoint;
    [SerializeField] private GameObject piniataPrefab;

    private GameModel gameModel;
    private PiniataModel piniataModel;
    
    private const float maxClickInterval = 2f;
    private float lastClickTime;
    private bool isCooldownActive = false;
    private bool isPotentiallyOpened = false;
    private Coroutine openDelayCoroutine;

    private GameObject currentPiniata;

    private void Start()
    {
        gameModel = new GameModel();
        piniataModel = new PiniataModel();
        
        // Initialize UI
        gameView.UpdateScore(gameModel.Score);
        gameView.UpdateTimer(gameModel.Timer);
        gameView.UpdatePiniataNum(piniataModel.CurrentPiniataNum);

        SpawnNewPiñata();
    }

    private void Update()
    {
        UpdateTimer(Time.deltaTime);
    }
    
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
        currentPiniata.GetComponent<Button>().onClick.AddListener(PiniataClickHandler);

        lastClickTime = Time.time;
        
        Debug.Log("Spawned Piñata #" + piniataModel.CurrentPiniataNum
                  + " requiring " + piniataModel.ClicksRequired + " click(s).");
    }
    
    private void PiniataClickHandler()
    {
        // Ignore clicks during cooldown
        if (isCooldownActive) return;

        // Check how long it has been since the last click
        float timeSinceLastClick = Time.time - lastClickTime;
        lastClickTime = Time.time;

        // If gap is too big (and we're not in the final open grace period), reset clicks
        if (piniataModel.ClickCount > 0 && !isPotentiallyOpened && timeSinceLastClick > maxClickInterval)
        {
            piniataModel.ClickCount = 0;
            gameView.SetClickGapWarning(2f);
            Debug.Log("Click gap too long -> Reset click count.");
            return;
        }
        
        // Increment click count
        piniataModel.ClickCount++;
        Debug.Log("Piñata clicked! Click count = " + piniataModel.ClickCount);

        // Show hit VFX
        gameView.SpawnHitParticles();

        // If we already reached required clicks and are in grace period, overshoot => cooldown
        if (isPotentiallyOpened)
        {
            Debug.Log("Overshoot during finalization -> Begin cooldown.");
            if (openDelayCoroutine != null) StopCoroutine(openDelayCoroutine);
            StartCoroutine(CoHandleCooldown(3f));
            ResetClicks();
            return;
        }

        // Exactly required clicks => begin grace period
        if (piniataModel.ClickCount == piniataModel.ClicksRequired)
        {
            isPotentiallyOpened = true;
            openDelayCoroutine = StartCoroutine(CoFinalizeOpenAfterDelay(1f));
        }
        // Too many clicks => immediate cooldown
        else if (piniataModel.ClickCount > piniataModel.ClicksRequired)
        {
            Debug.Log("Immediate overshoot -> Begin cooldown for 3 seconds.");
            StartCoroutine(CoHandleCooldown(3f));
            ResetClicks();
        }
    }

    // Waits briefly to detect overshoot; if none, finalize opening
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
        Debug.Log("Piñata " + piniataModel.CurrentPiniataNum + " opens!");

        // Increase the score by Piñata number
        gameModel.Score += piniataModel.CurrentPiniataNum;
        gameView.UpdateScore(gameModel.Score);

        // Remove current Piñata after a short delay
        Destroy(currentPiniata, 0.7f);

        // Spawn the next Piñata and reset
        SpawnNewPiñata();
        ResetClicks();
    }
    
    private IEnumerator CoHandleCooldown(float cooldownDuration)
    {
        isCooldownActive = true;
        piniataModel.IsOnCooldown = true;

        gameView.SetPiniataCooldown(true, cooldownDuration);
        yield return new WaitForSeconds(cooldownDuration);

        isCooldownActive = false;
        piniataModel.IsOnCooldown = false;
        Debug.Log("Cooldown ended; Piñata is active again.");
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
            // Timer ended
            gameModel.Timer = 0;
            gameView.UpdateTimer(gameModel.Timer);
            Debug.Log("Time is up! Final score = " + gameModel.Score);
        }
    }
}
