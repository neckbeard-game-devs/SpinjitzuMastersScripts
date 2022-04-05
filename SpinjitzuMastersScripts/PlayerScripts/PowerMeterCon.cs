using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PowerMeterCon : MonoBehaviour
{
    public NinjagoGameCon ngc;
    public CameraController tcc;
    public MainAudioCon mac;
    public Image powerMeterImage, spinpowerMeterImage;
    public GameObject spinjitsuImage, attackButton, nameText;
    public bool pmChargedBool, spnChargedBool, spinjitzuImageBools, attackBool, chargingBool;
    [SerializeField]
    private float chargeAmount;
    private int levelInt;
    void Update()
    {
        if (chargingBool && powerMeterImage.fillAmount < 1)
        {
            powerMeterImage.fillAmount += Time.deltaTime * chargeAmount;
        }
        if (powerMeterImage.fillAmount == 1)
        {
            if (!pmChargedBool && !ngc.wuSpeaksBool)
            {
                pmChargedBool = true;
                SetAttackButton(true);
                mac.PlayGameSounds(0,false);
                tcc.ClearCams(6);
            }
            if(levelInt >= 1)
            {
                spinpowerMeterImage.fillAmount += Time.deltaTime * chargeAmount;
                if (spinpowerMeterImage.fillAmount == 1 && !spnChargedBool)
                {
                    spnChargedBool = true;
                    spinjitsuImage.SetActive(true);

                    if (!spinjitzuImageBools)
                    {
                        //masterwu says "boom"
                        spinjitzuImageBools = true;
                        mac.PlayGameSounds(1, false);
                    }
                    //Debug.Log("Spinjitzu Charged");
                }
            }
        }
    }
    public void LevelUp()
    {
        levelInt += 1;
        spinpowerMeterImage.gameObject.SetActive(true);
    }
    public void Attack(int ninja)
    {
        //main Attack Ui Button
        if (!attackBool)
        {
            attackBool = true;
            powerMeterImage.fillAmount = 0;
            StartCoroutine(AttackCountdown(ninja));
        }
    }
    public void SetAttackButton(bool active)
    {
        if (active)
        {
            attackButton.SetActive(true);
            nameText.SetActive(false);
        }
        else
        {
            attackButton.SetActive(false);
            nameText.SetActive(true);
        }
    }
    IEnumerator AttackCountdown(int ninja)
    {
        pmChargedBool = false;
        SetAttackButton(false);

        if (ngc.waveInt <=1)
        {
            tcc.ClearCams(3);
        }
        else if (ngc.waveInt == 2)
        {
            tcc.ClearCams(8);
        }
        else
        {
            tcc.ClearCams(9);
        }

        if (spnChargedBool == true)
        {
            spnChargedBool = false;
            spinpowerMeterImage.fillAmount = 0;
            spinjitsuImage.SetActive(false);
            mac.spinJitzuSound.Play();
            ngc.ninjaCons[ninja].StartSpinjitzu();
            //Debug.Log("Power Down Spinjitzu");
        }
        ngc.Attack(ninja);

        yield return new WaitForSeconds(1f);
        attackBool = false;
    }
}
