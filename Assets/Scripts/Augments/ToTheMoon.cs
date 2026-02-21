using UnityEngine;

public class ToTheMoon : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;
public CurrencyManager currency;
public Buffs buffs;
public int tracker;
public void ToTheMoonCall()
{
    if (tracker <3){
    currency.AddCurrency(8);

       currency.maxInterest = 100;
    tracker++;
    }
}
}