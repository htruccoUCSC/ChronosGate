using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;

public class SquireUnit : BaseUnit
{
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private float m_BasicAttackBuffAmount = 10f;
    [SerializeField] private float m_BasicAttackBuffDuration = 3f;
    private BoardManager m_BoardManager;

    private void Awake()
    {
        m_BoardManager = FindFirstObjectByType<BoardManager>();
    }

    protected override void CastAbility()
    {
        Debug.Log("Squire uses ability");
        List<BaseUnit> adjacentTowers = GetAdjacentTowers();
        if (adjacentTowers.Count == 0)
        {
            return;
        }
        //apply buff to all adjacent towers
        foreach (BaseUnit tower in adjacentTowers)
        {
            StartCoroutine(ApplyTemporaryAttackBuff(tower, m_BasicAttackBuffAmount, m_BasicAttackBuffDuration));
        }
    }

    protected override void PerformBasicAttack()
    {
        Debug.Log("Squire performs basic attack");
        List<BaseUnit> adjacentTowers = GetAdjacentTowers();
        if (adjacentTowers.Count == 0)
        {
            return;
        }
        //apply buff to one random adjacent tower
        BaseUnit buffTarget = adjacentTowers[UnityEngine.Random.Range(0, adjacentTowers.Count)];
        StartCoroutine(ApplyTemporaryAttackBuff(buffTarget, m_BasicAttackBuffAmount, m_BasicAttackBuffDuration));
    }

    private List<BaseUnit> GetAdjacentTowers()
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

        if (m_BoardManager.unitGrid == null)
            return result;

        int gridW = m_BoardManager.unitGrid.GetLength(0);
        int gridH = m_BoardManager.unitGrid.GetLength(1);

        //get our cell position
        Vector3Int myCell = m_BoardManager.GameTilemap.WorldToCell(transform.position);

        //4 adjacent tiles (up, down, left, right)
        Vector2Int[] offsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (Vector2Int offset in offsets)
        {
            int checkX = myCell.x + offset.x;
            int checkY = myCell.y + offset.y;

            // use the ACTUAL array size (prevents IndexOutOfRange)
            if (checkX < 0 || checkX >= gridW || checkY < 0 || checkY >= gridH)
                continue;

            BaseUnit unit = m_BoardManager.unitGrid[checkX, checkY];
            if (unit == null || unit == this || unit.myData == null)
                continue;

            result.Add(unit);
        }

        return result;
    }

    private IEnumerator ApplyTemporaryAttackBuff(BaseUnit targetUnit, float attackAmount, float durationSeconds)
    {
        if (targetUnit == null || targetUnit.myData == null)
        {
            yield break;
        }

        targetUnit.myData.DamageFlatMod += attackAmount;

        yield return new WaitForSeconds(durationSeconds);

        if (targetUnit != null && targetUnit.myData != null)
        {
            targetUnit.myData.DamageFlatMod -= attackAmount;
        }
    }
}