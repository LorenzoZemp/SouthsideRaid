﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruScript : BossScript
{
    bool leftBarrierActive = true;
    bool rightBarrierActive = true;
    [SerializeField] float leftBarrierHealth;
    [SerializeField] float rightBarrierHealth;
    public bool canBeHit = true;
    [SerializeField] float damageAccrued = 0.0f; // check if dealt enough damage to go into exposed state during attack
    [SerializeField] int timesAttacked = 0;
    [SerializeField] float hpLastTick;
    [SerializeField] int timesToAttack = 3;
    [SerializeField] bool erupting = false;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        leftBarrierHealth = 0.01f * maxHealth;
        rightBarrierHealth = 0.01f * maxHealth;
        hpLastTick = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!erupting)
        {
            base.Update();
        }
        CheckErupt();
    }

    protected override void Attack()
    {
        float threshold = 0.1f * maxHealth;

        // select a random lane to attack
        PlayerLaneState attackThisLane = (PlayerLaneState)Random.Range(0, 3);

        // if not already attacking
        if (timesAttacked < timesToAttack)
        {
            if (/*!(bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Left Slam") ||
                bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Middle Slam") ||
                bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Right Slam")) &&*/
                bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") ||
                bossAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                !bossAnimator.IsInTransition(0))
            {
                // play animation
                switch (attackThisLane)
                {
                    case PlayerLaneState.Left:
                        bossAnimator.Play("Left Slam");
                        break;
                    case PlayerLaneState.Middle:
                        bossAnimator.Play("Middle Slam");
                        break;
                    case PlayerLaneState.Right:
                        bossAnimator.Play("Right Slam");
                        break;

                    default: break;
                }
                //}
                timesAttacked++;
            }
        }


            // play sound

            // instantiate attack prefab

            // check dmg threshold to go to exposed/down state
            if (damageAccrued >= threshold)
            {
                stance = Stances.Down;
                damageAccrued = 0.0f;
                bossAnimator.SetInteger("State", (int)AnimStates.Opening);
                bossAnimator.Play("Exposed Idle");
                timesAttacked = 0;
                //timesToAttack = Random.Range(1, 4);
            }


            // return to idle when attack ends
            if ((bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Left Slam") ||
                    bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Middle Slam") ||
                    bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Right Slam"))
                    && bossAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !bossAnimator.IsInTransition(0)
                    && timesAttacked == timesToAttack)
            {
            //bossAnimator.SetInteger("State", (int)AnimStates.Idle);
            //stance = Stances.Idle;
            //// reset damageAccrued
            //damageAccrued = 0.0f;
            //timesAttacked = 0;
            ResetToIdle();
                return;
            }
    }
    

    public override float dealDamage(float _damage, PlayerLaneState _playerLane)
    {
        //Debug.Log("dealDamge in GRU");

        // canBeHit is set to false when erupting
        if (canBeHit)
        {
            // TODO: sum damageAccrued by correctly
            if (stance == Stances.Attack)
            {
                damageAccrued += _damage;
            }
            // if player is in blocked lane reduce damage
            if (_playerLane == PlayerLaneState.Left)
            {
                // deal reduced damage
                if (leftBarrierActive)
                {
                    leftBarrierHealth -= _damage * 0.25f;
                    if (leftBarrierHealth <= 0.0f)
                    {
                        leftBarrierActive = false;
                        bossAnimator.Play("Left Break");
                        ResetToIdle();
                    }
                    return base.dealDamage(_damage * 0.25f, _playerLane);
                }

                // deal double damage
                else
                {
                    return base.dealDamage(_damage * 2.0f, _playerLane);
                }
            }

            // if player is in blocked lane reduce damage
            else if (_playerLane == PlayerLaneState.Right)
            {
                // deal reduced damage
                if (rightBarrierActive)
                {
                    rightBarrierHealth -= _damage * 0.25f;
                    if (rightBarrierHealth <= 0.0f)
                    {
                        rightBarrierActive = false;
                        bossAnimator.Play("Right Break");
                        ResetToIdle();
                    }
                    return base.dealDamage(_damage * 0.25f, _playerLane);
                }

                // deal double damage
                else
                {
                    return base.dealDamage(_damage * 2.0f, _playerLane);
                }
            }
            else
            {
                return base.dealDamage(_damage, _playerLane);
            }
        }
        else return 0.0f;
    }

    // checks if it needs to erupt by comparing the hp last frame and this frame.
    private void CheckErupt()
    {
        if ((hpLastTick > 0.75f * maxHealth && currentHealth <= 0.75f * maxHealth)  || // when it drops below 75%
            (hpLastTick > 0.5f * maxHealth && currentHealth <= 0.5f * maxHealth)  || // when it drops below 50%
            (hpLastTick > 0.25f * maxHealth && currentHealth <= 0.25f * maxHealth)) // when it drops below 25%
        {
            Debug.Log("ERUPT BIG BOI");
            Erupt();
        }
        //else if 
        //{
        //    Debug.Log("ERUPT BIG BOI");
        //    Erupt();

        //}
        //else if (hpLastTick > 0.25f * maxHealth && currentHealth <= 0.25f * maxHealth) // when it drops below 25%
        //{
        //    Debug.Log("ERUPT BIG BOI");
        //    Erupt();

        //}

        // check if finished erupting
        if (bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Erupt")
                && bossAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            ResetToIdle();
        }

        hpLastTick = currentHealth;
    }

    private void Erupt()
    {
        //ResetToIdle();
        bossAnimator.Play("Erupt", 0);
        canBeHit = false;
        erupting = true;
    }

    private void ResetToIdle()
    {
        Debug.Log("RESET TO IDLE");
        bossAnimator.SetInteger("State", (int)AnimStates.Idle);
        erupting = false;
        stance = Stances.Idle;
        timesAttacked = 0;
        // times to attack
        //timesToAttack = Random.Range(1, 4); // 1-3 times
        damageAccrued = 0.0f;
        canBeHit = true;
    }
}
