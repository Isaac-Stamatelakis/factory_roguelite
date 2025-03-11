using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Layer;

public class DevMode : MonoBehaviour
{
    private static DevMode instance;
    public static DevMode Instance => instance;
    public void Awake()
    {
        instance = this;
    }

    [SerializeField] public bool noClip;
    [SerializeField] public bool flight;
    [SerializeField] public bool instantBreak;
    [SerializeField] public bool noBreakCooldown;
    [SerializeField] public bool noHit;
    [SerializeField] public bool noPlaceCost;
    [SerializeField] public bool noPlaceLimit;
    public bool NoEnergyCost;
    public bool EnableGameStages = true;
    public float FlightSpeed = 5f;
    public bool NoReachLimit;
    public bool LightOn = true;


}
