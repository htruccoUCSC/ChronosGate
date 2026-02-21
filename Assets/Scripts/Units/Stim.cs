using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.Collections;

public class StimUnit : BaseUnit
{
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private float m_BasicAttackBuffAmount = 10f;
    [SerializeField] private float m_BasicAttackBuffDuration = 1f;
    private float m_buffDuration = 1f;
    private BoardManager m_BoardManager;
    private Buffs buff;

    private void Awake()
    {
        m_BoardManager = FindFirstObjectByType<BoardManager>();
    }

    protected override void CastAbility()
    {
        Debug.Log("Stim uses ability");
        List<BaseUnit> adjacentTowers = GetUpDownTowers();
        if (adjacentTowers.Count == 0)        {
            return;
        }   
        //apply buff to Up and Down Adjacent towers
        foreach (BaseUnit tower in adjacentTowers)
        {
            Buff attackSpeedBuff = new Buff {
                AttackSpeedMult = 1.5f, // +50% attack speed
                duration = m_buffDuration
            };
            buff.AddTempBuff(tower,.15f,0,0,0,0,0,0,null);
            Debug.Log($"{tower.name} receives Stim's attack buff for {m_BasicAttackBuffDuration} seconds");
        }
    }

    protected override void PerformBasicAttack()
    {   
        Debug.Log("Stim performs basic attack");
        List<BaseUnit> adjacentTowers = GetRightAdjacentTowers();
        BaseUnit buffTarget = adjacentTowers[0];
        Buff attackSpeedBuff = new Buff {
            AttackSpeedMult = 1.15f, // +15% attack speed
            duration = m_buffDuration
        };
        buffTarget.AddTempBuff(attackSpeedBuff);
        Debug.Log($"{buffTarget.name} receives Stim's attack buff for {m_BasicAttackBuffDuration} seconds");
       
    }

    private List<BaseUnit> GetRightAdjacentTowers()
    {
         List<BaseUnit> result = new List<BaseUnit>();

        //get boardmanager reference if we don't have it already
        if (m_BoardManager == null)
        {
            m_BoardManager = FindFirstObjectByType<BoardManager>();
            if (m_BoardManager == null || m_BoardManager.GameTilemap == null)
            {
                return result;
            }
        }
        //get our cell position
        Vector3Int myCell = m_BoardManager.GameTilemap.WorldToCell(transform.position);
        //Right adjacent tile
        Vector2Int rightOffset = new Vector2Int(1, 0);

        //for each adjacent tile, check if there's a tower unit and if so add it to the result list
        
            int checkX = myCell.x + rightOffset.x;
            int checkY = myCell.y + rightOffset.y;

            if (checkX < 0 || checkX >= m_BoardManager.Width || checkY < 0 || checkY >= m_BoardManager.Height)
            {
                return result;
            }

            BaseUnit unit = m_BoardManager.unitGrid[checkX, checkY];
            if (unit == null || unit == this || unit.myData == null)
            {
                return result;
            }

            result.Add(unit);
            return result;
        }

        private List<BaseUnit> GetUpDownTowers()
        {
            List<BaseUnit> result = new List<BaseUnit>();

            //get boardmanager reference if we don't have it already
            if (m_BoardManager == null)
            {
                m_BoardManager = FindFirstObjectByType<BoardManager>();
                if (m_BoardManager == null || m_BoardManager.GameTilemap == null)
                {
                    return result;
                }
            }
            //get our cell position
            Vector3Int myCell = m_BoardManager.GameTilemap.WorldToCell(transform.position);
            //Up and down adjacent tiles
            Vector2Int[] offsets =
            {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };
            //for each adjacent tile, check if there's a tower unit and if so add it to the result list
            foreach (Vector2Int offset in offsets)
            {
                int checkX = myCell.x + offset.x;
                int checkY = myCell.y + offset.y;

                if (checkX < 0 || checkX >= m_BoardManager.Width || checkY < 0 || checkY >= m_BoardManager.Height)
                {
                    continue;
                }

                BaseUnit unit = m_BoardManager.unitGrid[checkX, checkY];
                if (unit == null || unit == this || unit.myData == null)
                {
                    continue;
                }

                result.Add(unit);
            }

            return result;
    }


        
    }

  

