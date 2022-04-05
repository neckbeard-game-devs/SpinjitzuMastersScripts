using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class EnemyBotCon : MonoBehaviour
{
    public NinjagoGameCon ngc;
    public NavMeshAgent enemyAgent;
    public Animator enemyAnim;

    private NavMeshCon ninjaCon;
    public NavMeshCon[] ninjaCons;
    public Transform[] navMeshTargets;
    public Transform target;
    public GameObject targetGo, damageTextGo, deathEffect, healingEffect;

    public bool navin, startFightBool, fightingBool, handCombatBool, weaponHitBool, deathBool;
    public int botInt, healthInt, damageInt, ninjaInt;
    public Slider healthBar;
    public TMP_Text healthBarText, damageText;
    public BoxCollider[] enemyCombatCol;

    private void Update()
    {
        if (navin && target != null)
        {
            enemyAgent.SetDestination(target.position);
        }
        if (fightingBool)
        {
            CheckTarget();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!deathBool)
        {
            if (other.gameObject == targetGo && !startFightBool)
            {
                startFightBool = true;
                StartCoroutine(FightingCountdown());
            }

            if (other.gameObject.CompareTag("Projectile") && !weaponHitBool)
            {
                weaponHitBool = true;
                ninjaInt = other.gameObject.GetComponent<WeaponCon>().ninjaInt;

                if (healthInt >= 0)
                {
                    damageInt = Random.Range(20, 30);
                    healthInt -= damageInt;
                    CheckHealth();
                }
            }
            if (other.gameObject.CompareTag("handCombat") && !handCombatBool)
            {
                handCombatBool = true;
                ninjaCon = other.gameObject.GetComponentInParent<NavMeshCon>();
                if (healthInt >= 0)
                {
                    if (ninjaCon.spinjitzuBool)
                    {
                        damageInt = Random.Range(15, 25);
                    }
                    else
                    {
                        damageInt = Random.Range(5, 13);
                    }

                    healthInt -= damageInt;
                    CheckHealth();
                }
            }
        }
    }
    public void StartBot()
    {
        enemyCombatCol = GetComponentsInChildren<BoxCollider>();
        enemyAnim.SetFloat("Speed", 2);

        if (botInt <= 2)
        {
            targetGo = navMeshTargets[botInt].gameObject;
            target = navMeshTargets[botInt];
        }
        else
        {
            targetGo = navMeshTargets[botInt - 3].gameObject;
            target = navMeshTargets[botInt - 3];
        }

        navin = true;
        healthInt = 180;
        healthBarText.text = healthInt + "/180";
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
                PlayAnimation(37);
            }
            else
            {
                ninjaCons[ninjaInt].WonCombatWeapon();
                PlayAnimation(35);
            }

            DeathCountdown();
            return;
        }

        StartCoroutine(HitCountdown());
    }
    public void CheckTarget()
    {
        if (targetGo != null)
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
                            target = ninjaCons[i].gameObject.transform;
                        }
                    }
                    startFightBool = false;
                }
            }
        }
        else
        {
            fightingBool = false;
        }
    }
    public void HealBot()
    {
        StartCoroutine(HealingCountdown());
    }
    public void HealingBot(int amount)
    {
        healthInt += amount;
        if (healthInt >= 180)
        {
            healthInt = 180;
        }
        healthBarText.text = healthInt + "/180";
        healthBar.value = healthInt;
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
        healthBarText.text = healthInt + "/180";
        healthBar.value = healthInt;

        for (int i = 0; i < enemyCombatCol.Length; i++)
        {
            enemyCombatCol[i].enabled = false;
        }

        if (ngc.waveInt <= 1)
        {
            target = navMeshTargets[3];
        }
        else
        {
            target = navMeshTargets[4];
        }

        deathEffect.SetActive(true);
        PlayAnimation(2);
        ngc.DestroyedScout();
    }
    public void DestroyCountdown()
    {
        Destroy(gameObject, 30f);
    }
    IEnumerator FightingCountdown()
    {
        fightingBool = true;
        int num = Random.Range(0, 2);
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
    IEnumerator HitCountdown()
    {
        healthBarText.text = healthInt + "/180";
        healthBar.value = healthInt;
        damageText.text = "-" + damageInt;
        damageTextGo.SetActive(true);

        yield return new WaitForSeconds(1f);
        damageTextGo.SetActive(false);

        if (weaponHitBool)
        {
            weaponHitBool = false;
        }
        if (handCombatBool)
        {
            handCombatBool = false;
        }
    }
    IEnumerator HealingCountdown()
    {
        healingEffect.SetActive(true);

        yield return new WaitForSeconds(1f);
        HealingBot(20);

        yield return new WaitForSeconds(2f);
        HealingBot(20);

        yield return new WaitForSeconds(2f);
        HealingBot(20);

        yield return new WaitForSeconds(2f);
        HealingBot(20);
        healingEffect.SetActive(false);
    }
   
}

  
