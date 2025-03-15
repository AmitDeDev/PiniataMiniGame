using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameController : MonoBehaviour
{
    private GameModel gameModel;
    private PiniataModel piniataModel;
    
    [SerializeField] private GameView gameView;
    [SerializeField] private Transform piniataSpawnPoint;
    [SerializeField] private GameObject piniataPrefab;
    
    // TODO: implement it via -> private PiniataController piniataController; 

    private float cooldownTimer = 3f;
    private GameObject currentPiniata;

    private void Start()
    {
        gameModel = new GameModel();
        piniataModel = new PiniataModel();
        
        gameView.UpdateScore(gameModel.Score);
        gameView.UpdateTimer(gameModel.Timer);
        
        SpawnNewPiñata();
        // TODO: implement it via -> piniataController.Initialize(this);
    }
    
    private void SpawnNewPiñata()
    {
        if (currentPiniata != null)
        {
            Destroy(currentPiniata);
        }

        if (piniataModel.CurrentPiniataNum > 1)
        {
            piniataModel.ClicksRequired++;
        }

        piniataModel.CurrentPiniataNum++;
        Debug.Log("New Piniata number: " + piniataModel.CurrentPiniataNum);
        currentPiniata = Instantiate(piniataPrefab, piniataSpawnPoint);
        currentPiniata.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PiniataClickHandler);
    }

    private void PiniataClickHandler()
    {
        Debug.Log("Piniata has been clicked!");
        HandleClick();
        gameView.SpawnHitParticles();
        
    }

    private void HandleClick()
    {
        piniataModel.ClickCount++;
        if (piniataModel.ClickCount == piniataModel.ClicksRequired)
        {
            piniataModel.IsOpened = true;
            Debug.Log("Piniata should crack now");
            Destroy(currentPiniata,1f);
            Debug.Log("Piniata number: " + piniataModel.CurrentPiniataNum + "has been destroyed");
            gameView.UpdateScore(piniataModel.CurrentPiniataNum);
            SpawnNewPiñata();
            ResetClicks();
        }
        else if (piniataModel.ClickCount > piniataModel.ClicksRequired)
        {
            Debug.Log("Piniata on cooldown for: 3 sec");
            piniataModel.IsOnCooldown = true;
            gameView.SetPiniataCooldown(true,3);
            
            ResetClicks();
        }
    }

    private void ResetClicks()
    {
        piniataModel.ClickCount = 0;
        piniataModel.IsOnCooldown = false;
    }

    private void Update()
    {
        UpdateTimer(Time.deltaTime);
    }
    
    private void UpdateTimer(float deltaTime)
    {
        if (gameModel.Timer > 0)
        {
            gameModel.Timer -= deltaTime;
            gameView.UpdateTimer(gameModel.Timer);
        }
    }
}
