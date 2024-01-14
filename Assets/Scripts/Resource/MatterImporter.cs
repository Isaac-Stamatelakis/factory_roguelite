using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;

public class MatterImporter
{
    public static List<Matter> jsonToMatter(string json) {
        List<Dictionary<string, object>> dictList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
        List<Matter> matters = new List<Matter>();
        foreach (Dictionary<string, object> seralizedData in dictList) {
            if (seralizedData.ContainsKey("stateType")) {

            } else {
                Matter matter = new Matter(
                    Convert.ToInt32(seralizedData["id"]),
                    null,
                    Convert.ToInt32(seralizedData["amount"])
                );
                if (seralizedData.ContainsKey("state")) {
                    switch (seralizedData["state"].ToString()) {
                        case "solid":
                            matter.state = new Solid();
                            break;
                        case "liquid":
                            matter.state = new Liquid();
                            break;
                        case "gas":
                            matter.state = new Gas();
                            break;
                        case "plasma":
                            matter.state = new Plasma();
                            break;
                    }
                } else {
                    matter.state = new Solid();
                }
                matters.Add(matter);
            }
            
        }
        return matters;
       
    }
}
