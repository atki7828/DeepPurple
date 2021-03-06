﻿/*
*  Inventory.cs
*  Programmer: Gabriel Hasenoehrl
*  Description: This is the main script that manages the inventory.
*  Uses the singleton patter as there should never be more than one
*  inventory.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour 
{

	//Singleton Pattern
	//Will be able to access this with Inventory.instance
	public static Inventory instance;
	void Awake()
	{
		if(instance != null)
		{
			Debug.LogWarning("More than 1 instance of Inventory");
			return;
		}
		instance = this;
	}

	//delegate is similar to a signal, as when 
	//triggered methods listening for this delegate will be called
	public delegate void OnItemChanged();
	public OnItemChanged OnItemChangedCallBack;

	//Inventory variables
	int space = 35;
	public int money = 0;

	//Data structure to hold the items
	public List<Item> items = new List<Item>();

	//Adds item to inventory
	public bool Add(Item item)
	{
		if(!item.isCurrency)
		{
			if(items.Count >= space)
			{
				Debug.Log("Not enough room");
				return false;
			}
			items.Add(item);
			if (OnItemChangedCallBack != null)
			{
				//triggering delegate
				OnItemChangedCallBack.Invoke();
			}
		}
		else
		{
			money++;
			if (OnItemChangedCallBack != null)
			{
				//triggering delegate
				OnItemChangedCallBack.Invoke();
			}
		}
		return true;
	}

	//Removes item from the inventory
	public void Remove (Item item)
	{
		items.Remove(item);
		if (OnItemChangedCallBack != null)
		{
			OnItemChangedCallBack.Invoke();
		}
	}

	public int getScore()
	{
		return money;
	}

	public void incScore(int value)
	{
		money += value;
		OnItemChangedCallBack.Invoke();
	}


}
