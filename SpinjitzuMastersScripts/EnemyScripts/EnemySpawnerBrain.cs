using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


public class EnemySpawnerBrain : MonoBehaviour
{
    public NinjagoGameCon ngc;
    public EnemyBotCon[] enemyBotCons;
    public EnemyCon bossCon;

    public Transform[] enemyBotSpwnPnts, enemyNavMeshTargets;
    public Transform enemyBossTrans;

    public GameObject[] EnemyBots, enemyCanvasGos;
    public GameObject EnemyBotPrefab;

    public TMP_Text[] enemyNameText, enemyHealthBarText;
    public Slider[] enemyHealthBar;
    public bool setup;

    public void SpawnEnemybots(int bots)
    {
        EnemyBots = new GameObject[bots];
        enemyBotCons = new EnemyBotCon[bots];

        for (int i = 0; i < bots; i++)
        {
            EnemyBots[i] = Instantiate(EnemyBotPrefab, enemyBotSpwnPnts[i].position, enemyBotSpwnPnts[i].rotation);
            enemyBotCons[i] = EnemyBots[i].GetComponent<EnemyBotCon>();
            enemyBotCons[i].healthBarText = enemyHealthBarText[i];
            enemyBotCons[i].navMeshTargets = enemyNavMeshTargets;
            enemyBotCons[i].healthBar = enemyHealthBar[i];
            enemyBotCons[i].ninjaCons = ngc.ninjaCons;
            enemyBotCons[i].botInt = i;
            enemyBotCons[i].ngc = ngc;
            enemyNameText[i].text = "Ninroid";
            if (ngc.waveInt <= 1)
            {
                enemyBotCons[i].StartBot();
            }
        }

        //set gamecontrol bot fields
        ngc.EnemyBots = EnemyBots;
        ngc.enemyBotCons = enemyBotCons;
    }
    public void StartBots()
    {
        for (int i = 0; i < EnemyBots.Length; i++)
        {
            enemyBotCons[i].StartBot();
        }
    }
    public void DestroyBots()
    {
        for (int i = 0; i < EnemyBots.Length; i++)
        {
            if (EnemyBots[i] != null)
            {
                enemyCanvasGos[i].SetActive(false);
                enemyBotCons[i].DestroyCountdown();
            }
        }
    }
    public void SetUi(int ui)
    {
        if (ui == 0)
        {
            for (int x = 0; x < EnemyBots.Length; x++)
            {
                enemyNameText[x].text = "Ninroid";
                enemyHealthBarText[x].text = "180/180";
                enemyHealthBar[x].maxValue = 180;
                enemyHealthBar[x].value = 180;
                enemyCanvasGos[x].SetActive(true);
            }
        }
        else if (ui == 1)
        {
            enemyHealthBar[0].maxValue = 300;
            enemyHealthBar[0].value = 300;
            enemyNameText[0].text = "Army Scout Boss";
            enemyHealthBarText[0].text = "300/300";
            enemyCanvasGos[0].SetActive(true);
        }
    }
}
