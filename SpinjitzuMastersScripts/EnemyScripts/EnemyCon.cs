using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class EnemyCon : MonoBehaviour
{    
    private NinjagoGameCon ngc;
    private CameraController tcc;
    private MainAudioCon mac;
    private NavMeshAgent enemyAgent;
    private Animator enemyAnim;
    private NavMeshCon ninjaCon;
    public NavMeshCon[] ninjaCons;
    
    public GameObject enemyCanvas, damageTextGo, deathEffect, healingEffect;
    public GameObject[] bossTextBoxes;
    public Transform[] navMeshTargets, ninjasTrans;
    public Transform target, bossTextBoxTrans;
    public Vector3 bossTextBoxPos;
    public bool navin, startFightBool, fightingBool, handCombatBool, weaponHitBool,
         deathBool, healBool;

    public Slider healthBar;
    public TMP_Text healthBarText, damageText;
    private int hitInt, healthInt, damageInt, healInt, ninjaInt, bossTalkInt;

    private void Start()
    {
        ngc = FindObjectOfType<NinjagoGameCon>();
        tcc = FindObjectOfType<CameraController>();
        mac = FindObjectOfType<MainAudioCon>();
        enemyAgent = GetComponent<NavMeshAgent>();
        enemyAnim = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        if (navin && target != null)
        {
            enemyAgent.SetDestination(target.position);
        }
        if (!ngc.endGameBool && fightingBool)
        {
            CheckTarget();
        }      
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!deathBool && !ngc.endGameBool)
        {
            if (!ngc.bossFightBool)
            {
                //start and cut scenes
                if (other.gameObject == navMeshTargets[0].gameObject && hitInt == 0)
                {
                    hitInt = 1;
                    StartCoroutine(WaitingCountdown());
                }
                if (other.gameObject == navMeshTargets[1].gameObject && hitInt == 1)
                {
                    hitInt = 2;
                    target = navMeshTargets[2];
                }
                if (other.gameObject == navMeshTargets[2].gameObject && hitInt == 2)
                {
                    hitInt = 3;
                    SetNavMesh(true);
                }
            }
            else
            {
                //restarts fighting animations when switch target
                if (other.gameObject == target.gameObject && !startFightBool)
                {
                    startFightBool = true;
                    StartCoroutine(FightingCountdown());
                }

                //combat collisions
                if (other.gameObject.CompareTag("Projectile") && !weaponHitBool)
                {
                    weaponHitBool = true;
                    ninjaInt = other.gameObject.GetComponent<WeaponCon>().ninjaInt;
                    if (healthInt >= 0)
                    {
                        //simple random damage, could set damage from weapon easily
                        damageInt = Random.Range(12, 20);
                        healthInt -= damageInt;
                        CheckHealth();                                                                             
                    }

                }
                if (other.gameObject.CompareTag("handCombat") && !handCombatBool)
                {
                    handCombatBool = true;
                    ninjaCon = other.gameObject.GetComponentInParent<NavMeshCon>();
                    
                    if (healthInt >= 0 )
                    {
                        //simple random damage, set damage from spinjitzu or handCombat
                        if (ninjaCon.spinjitzuBool)
                        {
                            damageInt = Random.Range(12, 20);
                        }
                        else
                        {
                            damageInt = Random.Range(2, 11);
                        }
                        healthInt -= damageInt;
                        CheckHealth();                   
                    }
                }
            }
        }
    }

    public void StartBoss()
    {
        bossTextBoxPos = bossTextBoxTrans.position;
        ninjaCons = ngc.ninjaCons;
        enemyAnim.SetFloat("Speed", 2);
        target = navMeshTargets[0];
        navin = true;
        healthInt = 300;
    }
    public void SetBossUi()
    {
        //esb.enemyNameText[0].text = "Army Scout Boss";
        healthBarText.text = healthInt + "/300";
        healthBar.value = healthInt; ;
        healthBar.maxValue = 300;
        healthBar.value = 300;
        enemyCanvas.SetActive(true);
    }

    private void SetNavMesh(bool stopped)
    {
        if (stopped)
        {
            navin = false;
            enemyAgent.isStopped = true;
            enemyAnim.SetFloat("Speed", 0);
        }
        else
        {
            navin = true;
            enemyAgent.isStopped = false;
            enemyAnim.SetFloat("Speed", 2);
        }
    }
    public void StartTaunt()
    {
        StartCoroutine(StartingTaunt());
    }
    public void StartCutScene()
    {
        StartCoroutine(StartingCutscene());
    }
    public void StartActualFighting()
    {
        StartCoroutine(ActualFightingCountdown());
    }
    public void CheckTarget()
    {
        if (target.gameObject != null && !healBool)
        {
            if (!startFightBool)
            {
                if (!target.GetComponent<NavMeshCon>().deathBool)
                {
                    startFightBool = true;
                    StartCoroutine(FightingCountdown());
                }
                else
                {
                    for (int i = 0; i < ninjaCons.Length; i++)
                    {
                        if (!ninjaCons[i].deathBool)
                        {
                            target = ninjasTrans[i];
                        }
                    }
                    mac.BossSoundPlay(0, true);
                    startFightBool = false;
                }

            }

        }
        else
        {
            fightingBool = false;
        }
    }
    public void HealingBoss(int amount)
    {
        healthInt += amount;
        if (healthInt >= 300)
        {
            healthInt = 300;
        }
        healthBarText.text = healthInt + "/300";
        healthBar.value = healthInt;
    }
    public void CheckHealth()
    {
        if (healthInt <= 0)
        {
            healthInt = 0;
            deathBool = true;
            if (handCombatBool)
            {
                ninjaCon.WonHandCombat();
            }
            else
            {
                ninjaCons[ninjaInt].WonCombatWeapon();
            }

            DeathCountdown();
            return;
        }
        if (healthInt <= 150 && !healBool && healInt <= 1)
        {
            healBool = true;
            StartCoroutine(HealingCountdown());
        }
        if (!healBool)
        {
            PlayAnimation(37);
        }

      
        StartCoroutine(HitCountdown());
    }
    public void HealBots()
    {
        for (int i = 0; i < 6; i++)
        {
            if (!ngc.enemyBotCons[i].deathBool)
            {
                ngc.enemyBotCons[i].HealBot();
            }
        }
    }
    public void PlayAnimation(int id)
    {
        enemyAnim.SetBool("Play Special", true);
        enemyAnim.SetInteger("Special Id", id);
    }
    public void DeathCountdown()
    {
        fightingBool = false;
        startFightBool = false;
        healthBarText.text = healthInt + "/300";
        healthBar.value = healthInt;

        target = navMeshTargets[2];

        deathEffect.SetActive(true);
        PlayAnimation(2);
        // win game
        ngc.WinGame();
    }
    public void DestroyCountdown()
    {
        Destroy(gameObject, 30f);
    }
    IEnumerator WaitingCountdown()
    {
        SetNavMesh(true);
        enemyCanvas.SetActive(true);
        PlayAnimation(6);
        ngc.SetTriggerActions();

        yield return new WaitForSeconds(4f);
        SetNavMesh(false);
       
        enemyCanvas.SetActive(false);
        target = navMeshTargets[1];

    }
    IEnumerator StartingTaunt()
    {
        PlayAnimation(13);

        yield return new WaitForSeconds(4f);
        PlayAnimation(9);
        mac.BossSoundPlay(2,false);
        bossTextBoxes[0].transform.position = bossTextBoxPos;

        yield return new WaitForSeconds(4f);
        PlayAnimation(29);
        mac.BossSoundPlay(0, false);

        yield return new WaitForSeconds(5f);
        PlayAnimation(39);

        yield return new WaitForSeconds(5f);
        PlayAnimation(6);
        mac.BossSoundPlay(3, false);
        bossTextBoxes[2].transform.position = bossTextBoxPos;
    }
    IEnumerator StartingCutscene()
    {
        SetNavMesh(false);
        target = navMeshTargets[1];
        
        yield return new WaitForSeconds(2f);
        PlayAnimation(13);
        mac.BossSoundPlay(0, false);
        yield return new WaitForSeconds(4f);
        //set new text
        bossTextBoxes[1].transform.position = bossTextBoxPos;
        mac.BossSoundPlay(1, false);

        // turn boss towards ninjas
        SetNavMesh(true);
        target = ninjasTrans[0];
        Vector3 dir2 = ninjasTrans[0].position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir2), 1f);
       
        yield return new WaitForSeconds(10f);
        healingEffect.SetActive(true);
        mac.PlayGameSounds(5,true);
        mac.BossSoundPlay(0, false);
        tcc.ClearCams(9);

        //Debug.Log("boss heal guards");
        HealBots();  
        PlayAnimation(9);
        target = navMeshTargets[0];
        SetNavMesh(false);

        yield return new WaitForSeconds(6f);

        tcc.ClearCams(8);
        tcc.vrCams[9].gameObject.SetActive(false);

        healingEffect.SetActive(false);
        mac.PlayGameSounds(5, false);

        yield return new WaitForSeconds(6f);
        SetNavMesh(true);
        Vector3 dir3 = ninjasTrans[0].position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir3), 1f);
        target = ninjasTrans[0];
        PlayAnimation(29);

        yield return new WaitForSeconds(5f);
        healingEffect.SetActive(true);
        mac.PlayGameSounds(5, true);
        mac.BossSoundPlay(0, false);

        //Debug.Log("boss fighting heal guards");
        HealBots();
        PlayAnimation(39);

        yield return new WaitForSeconds(6f);
        PlayAnimation(6);

        yield return new WaitForSeconds(6f);
        healingEffect.SetActive(false);
        mac.PlayGameSounds(5, false);
        mac.BossSoundPlay(2, false);
    }
    IEnumerator ActualFightingCountdown()
    {
        PlayAnimation(39);
        mac.BossSoundPlay(3, false);

        yield return new WaitForSeconds(4f);
        int ranNum = Random.Range(0, ninjasTrans.Length);

        for (int i = 0; i < ninjasTrans.Length; i++)
        {
            if(i == ranNum)
            {
                SetNavMesh(false);
                target = ninjasTrans[i];
            }   
        }
        SetBossUi();
        ngc.wuSpeaksBool = false;
    }
    IEnumerator HitCountdown()
    {        
        //boss talking
        if (healthInt <= 250 && bossTalkInt == 0)
        {
            bossTalkInt += 1;
            mac.BossSoundPlay(0, true);
            StartCoroutine(MoveCountdown());
        }
        else if (healthInt <= 200 && bossTalkInt == 1)
        {
            bossTalkInt += 1;
            mac.BossSoundPlay(0, true);
        }
        else if (healthInt <= 175 && bossTalkInt == 2)
        {
            bossTalkInt += 1;
            mac.BossSoundPlay(0, true);
        }
        else if (healthInt <= 175 && bossTalkInt == 3)
        {
            bossTalkInt += 1;
            mac.BossSoundPlay(0, true);
        }
        else if (healthInt <= 100 && bossTalkInt == 4)
        {
            bossTalkInt += 1;
            mac.BossSoundPlay(0, true);
        }
        else if (healthInt <= 50 && bossTalkInt == 5)
        {
            bossTalkInt += 1;
            mac.BossSoundPlay(0, true);
            StartCoroutine(MoveCountdown());
        }

        healthBarText.text = healthInt + "/300";
        healthBar.value = healthInt;
        damageText.text = "-" + damageInt;
        damageTextGo.SetActive(true);

        yield return new WaitForSeconds(1f);
        damageTextGo.SetActive(false); 
        handCombatBool = false;
        weaponHitBool = false;
    }
    IEnumerator HealingCountdown()
    {
        healInt += 1;
        target = navMeshTargets[1];
        tcc.ClearCams(9);

        yield return new WaitForSeconds(4f);
        PlayAnimation(13);
        healingEffect.SetActive(true);
        mac.PlayGameSounds(5, true);
        mac.BossSoundPlay(0, false);
        HealingBoss(25);
       
        yield return new WaitForSeconds(2f);
        HealingBoss(25);
        target = navMeshTargets[0];
       
        yield return new WaitForSeconds(2f);
        HealingBoss(25);
        PlayAnimation(9);

        yield return new WaitForSeconds(2f);
        HealingBoss(25);

        // turn boss towards ninjas
        for (int i = 0; i < ninjaCons.Length; i++)
        {
            if (!ninjaCons[i].deathBool)
            {
                target = ninjasTrans[i];
                break;
            }
        }
        Vector3 dir2 = ninjasTrans[0].position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir2), 1f);
        PlayAnimation(29);
        mac.BossSoundPlay(2, false);
        healingEffect.SetActive(false);
        mac.PlayGameSounds(5, false);

        yield return new WaitForSeconds(2f);
        healBool = false;
    }
    IEnumerator MoveCountdown()
    {
        healBool = true;
        target = navMeshTargets[1];

        PlayAnimation(29);
        mac.BossSoundPlay(0, false);

        yield return new WaitForSeconds(4f);
        target = navMeshTargets[0];

        yield return new WaitForSeconds(4f);
        if (!deathBool)
        {
            for (int i = 0; i < ninjaCons.Length; i++)
            {
                if (!ninjaCons[i].deathBool)
                {
                    target = ninjasTrans[i];
                }
            }
            Vector3 dir2 = ninjasTrans[0].position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir2), 1f);

            PlayAnimation(6);
            mac.BossSoundPlay(2, false);
        }

        healBool = false;
    }
    IEnumerator FightingCountdown()
    {
        int num = Random.Range(0, 2);
        fightingBool = true;
        if (num == 0)
        {
            enemyAnim.SetTrigger("kickLeft");
        }
        else
        {
            enemyAnim.SetTrigger("kickRight");
        }

        yield return new WaitForSeconds(2f);
        startFightBool = false;

    }
}
