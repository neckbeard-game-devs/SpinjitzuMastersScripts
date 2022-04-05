using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class NavMeshCon : MonoBehaviour
{
    public NinjagoGameCon ngc;
    public MainAudioCon mac;
    public WeaponCon weapon;
    public NavMeshAgent ninjaAgent;
    public Animator ninjaAnim;
    public AudioSource jumpSound, hitSound;
    public AudioClip[] hitSoundClips;
    public Transform[] navMeshTargets;
    public Transform target, weaponSpwnPnt;
    public GameObject enemyTarget, weaponPrefab, weaponGo, damageTextGo, spinJitzuGo, xpGo;
    public GameObject[] activeGos;
    public Image[] activeImages;
    public NavMeshCon[] otherNinjas;
    public bool[] ninjas;
    public bool navin, hitBool, fightingBool, spinjitzuBool, handCombatBool, bossCombatBool, deadBool, deathBool, healingBool;
    public int ninjaInt, wonCombatInt, expInt, levelInt, hitInt, healthInt, damageInt;
    public int[] lvlUpInts;
    public string expBarString, healthBarString;
    public Slider healthBar, expBar;
    public TMP_Text healthBarText, expText, damageText;
    public SphereCollider fightTrigger;

    private void Update()
    {
        if (!deathBool)
        {
            if (navin && target != null)
            {
                ninjaAgent.SetDestination(target.position);
            }
            if(enemyTarget != null && fightingBool)
            {
                if (ngc.bossFightBool)
                {
                    if (!hitBool && !enemyTarget.GetComponent<EnemyCon>().deathBool)
                    {
                        hitBool = true;
                        StartCoroutine(FightingCountdown());
                    }
                }
                else if (!ngc.endGameBool && !ngc.wuSpeaksBool)
                {
                    if (enemyTarget.GetComponent<EnemyBotCon>().deathBool)
                    {
                        hitBool = false;
                        fightingBool = false;
                        for (int i = 0; i < ngc.EnemyBots.Length; i++)
                        {
                            if (ngc.EnemyBots[i] != null)
                            {
                                if (!ngc.EnemyBots[i].GetComponent<EnemyBotCon>().deathBool)
                                {
                                    SetTarget(ngc.EnemyBots[i]);
                                }
                            }
                        }
                    }
                    else if (!hitBool)
                    {
                        hitBool = true;
                        StartCoroutine(FightingCountdown());
                    }
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (deathBool)
        {
            if (other.gameObject == navMeshTargets[1].gameObject)
            {
                target = navMeshTargets[2];
                ninjaAgent.SetDestination(target.position);
            }
            if (other.gameObject == navMeshTargets[2].gameObject)
            {
                target = navMeshTargets[3];
                ninjaAgent.SetDestination(target.position);
            }
            if (other.gameObject == navMeshTargets[3].gameObject)
            {
                target = navMeshTargets[2];
                ninjaAgent.SetDestination(target.position);
            }
        }
        else if (!ngc.wuSpeaksBool)
        {
            //leader 
            if (ninjas[0])
            {
                if (ngc.triggerInt == 1 && other.gameObject == navMeshTargets[0].gameObject)
                {
                    ngc.SetTriggerActions();
                    StartCoroutine(WaitingCountdown());
                }
            }
                       
            if (!hitBool && other.gameObject == enemyTarget)
            {
                hitBool = true;
                mac.soundFx.Play();
                StartCoroutine(FightingCountdown());
            }

            if (!handCombatBool && other.gameObject.CompareTag("enemyCombat"))
            {
                handCombatBool = true;
                if (healthInt >= 0 && !deadBool)
                {
                    damageInt = Random.Range(3, 11);
                    healthInt -= damageInt;
                    if (healthInt <= 0)
                    {
                        healthInt = 0;
                        deadBool = true;
                        if (!deathBool)
                        {
                            deathBool = true;
                            StartCoroutine(NinjaDownCountdown());
                            return;
                        }
                    }
                    StartCoroutine(HitCountdown());
                }


            }
            if (!bossCombatBool && other.gameObject.CompareTag("bossCombat"))
            {
                bossCombatBool = true;
                if (healthInt >= 0 && !deadBool)
                {
                    damageInt = Random.Range(6, 18);
                    healthInt -= damageInt;
                    if (healthInt <= 0)
                    {
                        healthInt = 0;
                        deadBool = true;
                        if (!deathBool)
                        {
                            deathBool = true;
                            StartCoroutine(NinjaDownCountdown());
                            return;
                        }
                    }
                    StartCoroutine(HitCountdown());
                }
            }
        }
    }
    public void SetStartValues()
    {
        healthInt = 180;
        expBarString = "/60 Lvl1";
        healthBarString = "/180";
        expBar.maxValue = lvlUpInts[0];
        SetTarget(navMeshTargets[0].gameObject);
    }
    public void SetTarget(GameObject targetGo)
    {
        //Debug.Log("setting target NavMeshCon " + targetGo.name );
        fightingBool = false;
        hitBool = false;
        if (targetGo != navMeshTargets[0].gameObject)
        {
            enemyTarget = targetGo;
        } 
        target = targetGo.transform;
        navin = true;
        ResetAnims();
    } 
    public void StartFighting()
    {
        weaponGo = Instantiate(weaponPrefab, weaponSpwnPnt.position, weaponSpwnPnt.rotation);
        weapon = weaponGo.GetComponent<WeaponCon>();
        weapon.ninjaInt = ninjaInt;
        weapon.FireWeapon();
    }
    public void StartSpinjitzu()
    {
        StartCoroutine(SpinJitzuCountdown());
    }
    public void FightingNinjasBoss()
    {
        StartCoroutine(BossFightingCountdown());
    }   
    public void WonHandCombat()
    {
        GainXp(25, 15);
        wonCombatInt += 1;     
    }
    public void WonCombatWeapon()
    {
        GainXp(25, 15);
        wonCombatInt += 1;
    }
    public void WonCombatGroup()
    {
        GainXp(20, 20);
        ninjaAnim.SetTrigger("flipAttack");
        ResetAnims();
    }
    public void GainXp(int xp, int health)
    {
        expInt += xp;
        healthInt += health;
        StartCoroutine(XpCountdown());
    }
    public void ResetAnims()
    {
        ninjaAnim.ResetTrigger("flipAttack");
        ninjaAnim.ResetTrigger("spinKick");
        ninjaAnim.ResetTrigger("punchLeft");
        ninjaAnim.ResetTrigger("kickLeft");
        ninjaAnim.ResetTrigger("kickRight");
        ninjaAnim.ResetTrigger("spinjitsu");
        ninjaAnim.ResetTrigger("look");
        ninjaAnim.SetTrigger("walk");
        ninjaAgent.isStopped = false;
    }
    IEnumerator WaitingCountdown()
    {
        ngc.StopNinjas();
        yield return new WaitForSeconds(6f);
        ngc.SetNinjasTarget(true);
        ninjaAnim.SetTrigger("spiderman");

        yield return new WaitForSeconds(2f);
        ResetAnims();
    }
    IEnumerator HitCountdown()
    {
        hitSound.clip = hitSoundClips[0];
        hitSound.Play();
        if (levelInt == 2)
        {        
            healthBarText.text = healthInt + "/300";
            expBarString = "/180 Lvl3";
            expText.text = expInt + expBarString;
        }
        else if (levelInt == 1)
        {
            healthBarText.text = healthInt + "/260";
            expBarString = "/100 Lvl2";
            expText.text = expInt + expBarString;
        }
        else if (levelInt == 0)
        {
            healthBarText.text = healthInt + "/180";
            expBarString = "/60 Lvl1";
            expText.text = expInt + expBarString;
        }
     
        healthBar.value = healthInt;
        damageText.text = "-" + damageInt;
        damageTextGo.SetActive(true);

        yield return new WaitForSeconds(1f);
        damageTextGo.SetActive(false);

        if (bossCombatBool)
        {
            bossCombatBool = false;
        }
        if (handCombatBool)
        {
            handCombatBool = false;
        }
    }
    IEnumerator FightingCountdown()
    {
        fightingBool = true;
        int num = Random.Range(0, 5);
        
        if(num == 0)
        {
            ninjaAnim.SetTrigger("kickLeft");
        }
        else if (num == 1)
        {
            ninjaAnim.SetTrigger("punchLeft");
        }
        else if (num == 2)
        {
            ninjaAnim.SetTrigger("kickRight");
        }
        else if (num == 3)
        {
            ninjaAnim.SetTrigger("spinKick");
            jumpSound.Play();
        }
        else if (num == 4)
        {
            ninjaAnim.SetTrigger("flipAttack");
            jumpSound.Play();
        }

        
        yield return new WaitForSeconds(1.5f);
        hitBool = false;
        ResetAnims();
    }
    IEnumerator NinjaDownCountdown()
    {
        ninjaAgent.isStopped = true;
        if (bossCombatBool)
        {
            target = navMeshTargets[2];           
        }
        else
        {
            target = navMeshTargets[2];
        }
        if (levelInt == 2)
        {
            healthBarText.text = healthInt + "/300";
            expBarString = "/180 Lvl3";
            expText.text = expInt + expBarString;
        }
        else if (levelInt == 1)
        {
            healthBarText.text = healthInt + "/260";
            expBarString = "/100 Lvl2";
            expText.text = expInt + expBarString;
        }
        else if (levelInt == 0)
        {
            healthBarText.text = healthInt + "/180";
            expBarString = "/60 Lvl1";
            expText.text = expInt + expBarString;
        }
        for (int i = 0; i < activeGos.Length; i++)
        {
            activeGos[i].SetActive(false);
            activeImages[i].fillAmount = 1;            
        }

        healthBar.value = healthInt;
        ninjaAgent.SetDestination(target.position);
        hitSound.clip = hitSoundClips[2];
        hitSound.Play();
        ResetAnims();
        ninjaAnim.SetBool("dead", true);
        yield return new WaitForSeconds(1f);
      
        ngc.NinjaDown(ninjaInt);

    }
    IEnumerator BossFightingCountdown()
    {
        ninjaAgent.isStopped = true;
        ninjaAnim.SetTrigger("spiderman");
        yield return new WaitForSeconds(4f);
        ResetAnims();
    }
    IEnumerator XpCountdown()
    {
        xpGo.SetActive(true);
        hitSound.clip = hitSoundClips[1];
        hitSound.Play();

        /// check for level UP
        if (!deathBool)
        {
            if (expInt >= lvlUpInts[0] && levelInt == 0)
            {
                levelInt = 1;
                expBar.maxValue = lvlUpInts[1];
                healthInt = 260;
                healthBar.maxValue = 260;
                healthBarString = "/260";
                expInt = 0;
                expBarString = "/100 Lvl2";
                ngc.LevelUp(ninjaInt);
            }

            if (expInt >= lvlUpInts[1] && levelInt == 1)
            {
                levelInt = 2;
                expBar.maxValue = lvlUpInts[2];
                healthInt = 300;
                healthBar.maxValue = 300;
                healthBarString = "/300";
                expInt = 0;
                expBarString = "/600 Lvl3";
            }

            switch (levelInt)
            {
                case 0:
                    if (healthInt >= 180)
                    {
                        healthInt = 180;
                    }
                    break;
                case 1:
                    if (healthInt >= 260)
                    {
                        healthInt = 260;
                    }

                    break;
                case 2:
                    if (healthInt >= 300)
                    {
                        healthInt = 300;
                    }
                    break;
            }

            healthBar.value = healthInt;
            healthBarText.text = healthInt + healthBarString;
            expText.text = expInt + expBarString;
            expBar.value = expInt;
        }


        yield return new WaitForSeconds(6f);
        xpGo.SetActive(false);
    }
    IEnumerator SpinJitzuCountdown()
    {
        spinjitzuBool = true;

        if(levelInt == 1)
        {
            GainXp(0, 15);
        }
        else
        {
            GainXp(0, 20);
        }
        

        for (int i = 0; i < otherNinjas.Length; i++)
        {
            if (levelInt == 1)
            {
                otherNinjas[i].GainXp(0, 10);
            }
            else
            {
                otherNinjas[i].GainXp(0, 15);
            }
        }

        healthBar.value = healthInt;
        healthBarText.text = healthInt + healthBarString;
        spinJitzuGo.SetActive(true);
        ninjaAnim.SetTrigger("spinjitsu");
        //mac.PlayGameSounds(4, false);
        yield return new WaitForSeconds(4f);
        spinJitzuGo.SetActive(false);
        ResetAnims();
        spinjitzuBool = false;
    }
}
//if (!deathBool)
//{
//    if (navin && target != null)
//    {
//        ninjaAgent.SetDestination(target.position);
//    }
//    if (enemyTarget != null && !ngc.bossFightBool && !ngc.wuSpeaksBool)
//    {
//        if (enemyTarget.GetComponent<EnemyBotCon>().deathBool)
//        {
//            for (int i = 0; i < ngc.EnemyBots.Length; i++)
//            {
//                if (ngc.EnemyBots[i] != null)
//                {
//                    if (!ngc.EnemyBots[i].GetComponent<EnemyBotCon>().deathBool)
//                    {
//                        enemyTarget = ngc.EnemyBots[i];
//                        target = ngc.EnemyBots[i].transform;
//                    }
//                    else
//                    {
//                        hitBool = false;
//                        fightingBool = false;
//                    }
//                }
//            }
//        }

//    }
//    if (fightingBool)
//    {
//        if (enemyTarget != null && !ngc.bossFightBool && !ngc.endGameBool)
//        {
//            if (!hitBool)
//            {
//                if (!enemyTarget.GetComponent<EnemyBotCon>().deathBool)
//                {
//                    hitBool = true;
//                    StartCoroutine(FightingCountdown());
//                }
//            }
//        }
//        if (ngc.bossFightBool)
//        {
//            if (!hitBool)
//            {
//                if (enemyTarget != null && !enemyTarget.GetComponent<EnemyCon>().deathBool)
//                {
//                    hitBool = true;
//                    StartCoroutine(FightingCountdown());
//                }
//            }
//        }
//    }
//}
