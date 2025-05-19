using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Items.Transmutable;
using UnityEngine;


/// <summary>
/// Automatically Generates Items for a Tier
/// </summary>
[CreateAssetMenu(fileName ="Tier Info",menuName="Item/TierInfo")]

public class TierItemInfoObject : ScriptableObject
{
    public TieredGameStage TieredGameStage;
    public TransmutableItemMaterial PrimaryMaterial;
    public TransmutableItemMaterial SecondaryMaterial;
    public TransmutableItemMaterial WireMaterial;
    public TransmutableItemMaterial PlasticMaterial;
}
