using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
public class NinjagoGameCon : MonoBehaviour
{
    private CameraController tcc;
    private EnemySpawnerBrain esb;
    private MainAudioCon mac;
    public PowerMeterCon[] powerMeters;
    public NavMeshCon[] ninjaCons;
    public EnemyBotCon[] enemyBotCons;
    public EnemyCon bossCon;
    public Animator masterWuAnim;
    public Animator[] startUI;

    public int triggerInt, waveInt;
    public bool wuSpeaksBool, attackBool, bossFightBool, endGameBool;
    public bool[] deadNinjaBool = new bool[3];

    public GameObject battleCanvas, destroyUiPanel, endGamePanel, resetGameButton;
    public GameObject[] EnemyBots, objectiveWinGos, pickUpWaveGos, startUIGos;

    public Vector3 objectiveUI;

    public TMP_Text endGameText, resetButtonText;
    public string[] endGameStrings;

    private int ranNum, destroyedScoutInt, ninjaDownInt;
    void Start()
    {
        endGameBool = true;
        tcc = FindObjectOfType<CameraController>();
        esb = FindObjectOfType<EnemySpawnerBrain>();
        mac = FindObjectOfType<MainAudioCon>();
        esb.ngc = this;

        for (int i = 0; i < powerMeters.Length; i++)
        {
            powerMeters[i].ngc = this;
            powerMeters[i].tcc = tcc;
            powerMeters[i].mac = mac;
        }
    }
    public void StartGame()
    {
        //ui touch start screen
        endGameBool = false;
        if (triggerInt == 0)
        {
            SetTriggerActions();
        }
    }
    public void StopNinjas()
    {
        for (int i = 0; i < ninjaCons.Length; i++)
        {
            ninjaCons[i].ninjaAgent.isStopped = true;
            if (i < 1)
            {
                ninjaCons[i].ninjaAnim.SetTrigger("look");
            }
            else
            {
                ninjaCons[i].ninjaAnim.SetTrigger("spiderman");
            }
        }
    }
    public void SetTriggerActions()
    {
        triggerInt += 1;
        switch (triggerInt)
        {
            case 1:
                StartCoroutine(StartGameCountdown());
                break;
            case 2:
                tcc.ClearCams(2);
                SetWuAnim(37);
                break;
            case 3:
                {
                    tcc.ClearCams(3);
                    //turning off start scene cams
                    for (int cams = 0; cams < 3; cams++)
                    {
                        tcc.vrCams[cams].gameObject.SetActive(false);
                    }

                    for (int i = 0; i < powerMeters.Length; i++)
                    {
                        powerMeters[i].chargingBool = true;
                    }

                    break;
                }
        }
    }
    public void SetWuAnim(int anim)
    {
        masterWuAnim.SetBool("Play Special", true);
        masterWuAnim.SetInteger("Special Id", anim);
    }
    public void SetNinjasTarget(bool startFight)
    {
        for (int i = 0; i < ninjaCons.Length; i++)
        {
            ninjaCons[i].SetTarget(EnemyBots[i]);
            if (startFight)
            {
                ninjaCons[i].ResetAnims();
            }
        }

        StartCoroutine(ResetNinjaFightCol());
    }
    public void Attack(int ninja)
    {
        StartCoroutine(AttackCountdown(ninja));
    }
    public void DestroyedScout()
    {
        destroyedScoutInt += 1;
        int ranNum = Random.Range(0, mac.masterWoClips.Length);
        mac.PlayWuAudio(ranNum);

        //first 2 waves scene 1
        if (destroyedScoutInt >= 3 && waveInt <= 1)
        {
            destroyedScoutInt = 0;
            WaveWin();
        }

        //gives ninjas 1 heal during battle scene 2
        if (destroyedScoutInt >= 3 && triggerInt == 3)
        {
            SetTriggerActions();
            for (int i = 0; i < ninjaCons.Length; i++)
            {
                ninjaCons[i].GainXp(20, 20);
            }
            //Debug.Log("boss battle heal");
        }     
        
        //starts boss fight
        if (destroyedScoutInt >= 6 && !bossFightBool)
        {
            bossFightBool = true;
            destroyedScoutInt = 0;
            WaveWin();
            StartNinjasBoss();
        }

        StartCoroutine(ResetNinjaFightCol());
    }
    public void NinjaDown(int player)
    {
        ninjaDownInt += 1;
        powerMeters[player].SetAttackButton(false);
        deadNinjaBool[player] = true;
        if (ninjaDownInt >= 3 && !endGameBool)
        {
            endGameBool = true;
            mac.soundFx.Stop();
            StartCoroutine(EndGameCountdown());
            return;
        }

        for (int x = 0; x < ninjaCons.Length; x++)
        {
            if (!deadNinjaBool[x])
            {
                if (ninjaDownInt == 2)
                {
                    ninjaCons[x].fightTrigger.radius = 2f;
                }
                tcc.SwitchCamTargets(player, x);
                break;
            }
        }
    }
    private void WinCombatGroup()
    {
        for (int i = 0; i < ninjaCons.Length; i++)
        {
            //resurrects any dead ninjas when game is over
            if (endGameBool)
            {
                ninjaCons[i].deathBool = false;
                ninjaCons[i].ninjaAnim.SetBool("dead", false);
            }

            //sets ninjas navMesh back towards start position
            if (!bossFightBool)
            {
                ninjaCons[i].SetTarget(ninjaCons[i].navMeshTargets[0].gameObject);
            }

            //gains xp and health
            ninjaCons[i].WonCombatGroup();
        }
    }
    public void WaveWin()
    {
        //set pick up gameObject for wave win UI
        waveInt += 1;
        pickUpWaveGos[waveInt-1].SetActive(true);
        pickUpWaveGos[waveInt-1].transform.position = ninjaCons[0].transform.position;
       
        wuSpeaksBool = true;
        mac.soundFx.Stop();

        //destroy enemy bots
        if (!endGameBool)
        {
            esb.DestroyBots();
            StartCoroutine(WaveCompleteCountdown());
        }   
    }
    public void LevelUp(int ninjaInt1)
    {
        powerMeters[ninjaInt1].LevelUp();
        // add audio
    }
    public void StartNinjasBoss()
    {
        if (bossFightBool)
        {
            //Debug.Log("Start actual Boss Fight!");
            for (int i = 0; i < ninjaCons.Length; i++)
            {
                ninjaCons[i].SetTarget(esb.enemyBossTrans.gameObject);
                ninjaCons[i].FightingNinjasBoss();
            }

            bossCon.StartActualFighting();
            esb.SetUi(1);
        }
        else
        {
            //fight boss guards wave of 6 EnemyBots
            
            tcc.ClearCams(8);
            tcc.bossBattleCam.enabled = true;
            SetNinjasTarget(true);
            wuSpeaksBool = false;
            bossCon.StartCutScene();
        }
    }  
    public void WinGame()
    {
        endGameBool = true;
        StartCoroutine(WinGameCountdown());
    }
    public void ResetGame()
    {
        //ui touch start screen
        resetGameButton.SetActive(false);
        SceneManager.LoadScene(0);
    }
    IEnumerator StartGameCountdown()
    {
        for (int x = 0; x < ninjaCons.Length; x++)
        {
            ninjaCons[x].SetStartValues();
        }

        for (int y = 0; y < startUI.Length; y++)
        {
            startUI[y].SetTrigger("start");
        }
        bossCon.StartBoss();
        tcc.ClearCams(1);
        mac.spinJitzuSound.Play();
        esb.SpawnEnemybots(3);

        yield return new WaitForSeconds(3f);
        battleCanvas.SetActive(true);

        objectiveWinGos[0].SetActive(true);
        startUIGos[0].SetActive(false);
        mac.PlayWuAudio(10);
        SetWuAnim(22);

        yield return new WaitForSeconds(2f);
        startUIGos[1].SetActive(false);

        yield return new WaitForSeconds(6f);
        SetWuAnim(22);
        esb.SetUi(1);

        yield return new WaitForSeconds(6f);
        //setting objective UI Panel and grabbing position
        StartCoroutine(FindObjectiveUi());

        //sets boss Ui for cutscene
        esb.SetUi(0);
        //Debug.Log("End StartGame Countdown");
    }
    IEnumerator FindObjectiveUi()
    {
        destroyUiPanel = GameObject.Find("Objective Win(Clone)");
        if(waveInt == 0)
        {
            objectiveUI = destroyUiPanel.transform.position;
        }
        yield return new WaitForSeconds(1f);
        //Debug.Log("Set Objective UI Position");
        destroyUiPanel.transform.position = objectiveUI;
    }
    IEnumerator AttackCountdown(int ninja)
    {
        //gives a delay for button press sound
        if (!attackBool)
        {
            attackBool = true;
            mac.PlayGameSounds(3, false);
        }

        //if ninja not dead start fighting
        if (!ninjaCons[ninja].deathBool)
        {
            ninjaCons[ninja].ninjaAgent.isStopped = false;
            ninjaCons[ninja].navin = true;
            ninjaCons[ninja].StartFighting();
        }

        yield return new WaitForSeconds(1f);
        attackBool = false;
    }
    IEnumerator WaveCompleteCountdown()
    {
        if (bossFightBool)
        {
            //Debug.Log("Waiting BossFight Cams");
            wuSpeaksBool = false;
            tcc.ClearCams(9);

            yield return new WaitForSeconds(4f);
        }

        //next 2nd wave of 3 enemyBots
        yield return new WaitForSeconds(2f);
        if (waveInt == 1)
        {
            esb.SpawnEnemybots(3);
        }
        ranNum = Random.Range(10, mac.masterWoClips.Length);
        mac.PlayWuAudio(ranNum);
        WinCombatGroup();

        yield return new WaitForSeconds(2f);
        //destroying objective panel and setting next panel active
        if (destroyUiPanel != null)
        {
            Destroy(destroyUiPanel);
            Destroy(objectiveWinGos[waveInt - 1]);
        }
        objectiveWinGos[waveInt].SetActive(true);
        if (waveInt == 1)
        {
            StartCoroutine(WuTalksCountdown());
        }
        else if (waveInt == 2)
        {
            //set boss loose and spawn 3rd wave of enemyBots          
            StartCoroutine(BossCountdown());
        }

        yield return new WaitForSeconds(3f);       
        if (waveInt <= 3)
        {
            if (waveInt == 1)
            {
                yield return new WaitForSeconds(8f);
                //Debug.Log("start fight wave  " + waveInt);
                tcc.ClearCams(3);
                //shut off wuCam

                SetNinjasTarget(true);
            }
            StartCoroutine(FindObjectiveUi());
        }
    }
    IEnumerator WuTalksCountdown()
    {
        objectiveWinGos[waveInt].SetActive(true);
        tcc.ClearCams(5);
        SetWuAnim(14);

        yield return new WaitForSeconds(3f);
        SetWuAnim(22);
        mac.PlayWuAudio(1);

        yield return new WaitForSeconds(3f);
        SetWuAnim(41);
        mac.PlayWuAudio(2);

        yield return new WaitForSeconds(9f);
        wuSpeaksBool = false;
        esb.SetUi(0);
    }
    IEnumerator BossCountdown()
    {
        //set cutscene cam
        tcc.ClearCams(4);
        tcc.cutSceneCamAnim.enabled = true;

        for (int i = 0; i < ninjaCons.Length; i++)
        {
            ninjaCons[i].ninjaAgent.isStopped = true;
            ninjaCons[i].navin = false;
        }

        //spawn level2 bots
        esb.SpawnEnemybots(6);

        //sets boss Ui for cutscene
        esb.SetUi(1);
        mac.PlayBGAudio(.7f, 1);
        bossCon.StartTaunt();

        yield return new WaitForSeconds(4f);
        //Debug.Log("Switch cams boss fight");
        tcc.ClearCams(9);

        yield return new WaitForSeconds(8f);
        tcc.ClearCams(4);

        //set ninjas navmesh enemyTarget 
        SetNinjasTarget(false);

        yield return new WaitForSeconds(6f);
        wuSpeaksBool = false;

        yield return new WaitForSeconds(8f);
        wuSpeaksBool = true;
        tcc.ClearCams(7);

        for (int i = 0; i < ninjaCons.Length; i++)
        {
            ninjaCons[i].GainXp(0, 180);
            ninjaCons[i].ninjaAgent.isStopped = true;
            ninjaCons[i].ninjaAnim.SetTrigger("spiderman");
        }

        yield return new WaitForSeconds(5f);
        //reset EnemyBot values after boss cut scene and start fight
        esb.SetUi(0);
        esb.StartBots();    
        StartNinjasBoss();

    }
    IEnumerator WinGameCountdown()
    {
        WaveWin();
        bossFightBool = false;
        mac.PlayBGAudio(.45f, 2);
        tcc.ClearCams(10);

        mac.spinJitzuSound.Play();
        WinCombatGroup();

        yield return new WaitForSeconds(4f);
        for (int i = 0; i < ninjaCons.Length; i++)
        {
            ninjaCons[i].SetTarget(ninjaCons[i].navMeshTargets[0].gameObject);
            ninjaCons[i].ResetAnims();
            ninjaCons[i].ninjaAnim.SetTrigger("airGuitar");
            ninjaCons[i].ResetAnims();
            ninjaCons[i].ninjaAnim.SetTrigger("airGuitar"); 
        }

        yield return new WaitForSeconds(5f);
        mac.PlayWinClips(6);
        WinCombatGroup();
        tcc.ClearCams(0);
        wuSpeaksBool = false;

        yield return new WaitForSeconds(4f);
        SetWuAnim(5);
        mac.PlayWinClips(5);

        yield return new WaitForSeconds(8f);
        wuSpeaksBool = true;
        mac.PlayWinClips(4);
        SetWuAnim(5);
        tcc.ClearCams(5);

        yield return new WaitForSeconds(6f);
        ranNum = Random.Range(0, mac.winClips.Length);
        mac.PlayWinClips(ranNum);
        SetWuAnim(5);
        resetGameButton.SetActive(true);
        resetButtonText.text = "ReStart";
        wuSpeaksBool = false;
        endGameText.text = endGameStrings[0];
        endGamePanel.SetActive(true);

        Debug.Log("end of code... win game");
    }
    IEnumerator EndGameCountdown()
    {
        wuSpeaksBool = true;
        bossFightBool = false;
        for (int i = 0; i < ninjaCons.Length; i++)
        {
            ninjaCons[i].SetTarget(ninjaCons[i].navMeshTargets[0].gameObject);
        }

        tcc.ClearCams(10);

        mac.PlayBGAudio(.45f, 2);
      
        yield return new WaitForSeconds(5f);
        mac.PlayWinClips(3);    
        wuSpeaksBool = false;

        yield return new WaitForSeconds(4f);
        SetWuAnim(5);
        mac.PlayWinClips(1);

        yield return new WaitForSeconds(8f);
        wuSpeaksBool = true;
        mac.PlayWinClips(4);
        SetWuAnim(5);
        tcc.ClearCams(5);

        yield return new WaitForSeconds(6f);
        mac.PlayWuAudio(0);
        SetWuAnim(5);
        wuSpeaksBool = false;
        resetGameButton.SetActive(true);
        resetButtonText.text = "Try Again";
        endGameText.text = endGameStrings[1];
        endGamePanel.SetActive(true);
        Debug.Log("end of code... lose game");
    }
    IEnumerator ResetNinjaFightCol()
    {
        for (int i = 0; i < ninjaCons.Length; i++)
        {
            ninjaCons[i].fightTrigger.enabled = false;
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < ninjaCons.Length; i++)
        {
            ninjaCons[i].fightTrigger.enabled = true;
        }
    }
}
