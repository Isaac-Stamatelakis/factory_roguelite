using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft;
using System.IO;

public class NewtonsoftTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //testWrite();
        //testRead();
        //testIdMapWrite();
        //testIdMapRead();
    }

    private void testWrite() {
        Human human1 = new Human {
            age = 25,
            weight = 65.5f,
            height = 175.0f,
            favouriteColor = "Blue",
            name = "Bob",
            type = "human"
        };

        Human human2 = new Human {
            age = 1002,
            weight = 65.5f,
            height = 175.0f,
            favouriteColor = "Green",
            name = "Joe",
            type = "human"
        };

        Dog doggo = new Dog {
            age = 1002,
            weight = 65.5f,
            height = 175.0f,
            favouriteBall = "Old tennis ball",
            name = "Joe",
            type = "dog"
        };
        Bird bird = new Bird {
            age = 1,
            weight = 1f,
            height = 0.2f,
            canFly = true,
            type = "bird"
        };
        AnimalWrapper animalWrapper = new AnimalWrapper();
        animalWrapper.animals = new List<Animal> {
            human1,
            human2,
            doggo,
            bird
        };

        AnimalWrapper animalWrapper1 = new AnimalWrapper();
        animalWrapper1.animals = new List<Animal> {
            human1,
            human2,
            doggo,
        };
        AnimalWrapperWrapper animalWrapperWrapper = new AnimalWrapperWrapper();
        animalWrapperWrapper.animalWrappers = new List<AnimalWrapper> {
            animalWrapper,
            animalWrapper1
        };
        string animalWrapperJson = Newtonsoft.Json.JsonConvert.SerializeObject(animalWrapper, Newtonsoft.Json.Formatting.Indented);
        string filePath = Application.dataPath + "/Resources/test.json";
        File.WriteAllText(filePath, animalWrapperJson);
        string animalWrapperWrapperJson = Newtonsoft.Json.JsonConvert.SerializeObject(animalWrapperWrapper, Newtonsoft.Json.Formatting.Indented);
        filePath = Application.dataPath + "/Resources/test1.json";
        File.WriteAllText(filePath, animalWrapperJson);
    }

    private void testRead() {
        string filePath = Application.dataPath + "/Resources/test.json";
        string animalWrapperJson = File.ReadAllText(filePath);
        List<Human> humans = new List<Human>();
        Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray) Newtonsoft.Json.Linq.JObject.Parse(animalWrapperJson)["animals"];
        foreach (Newtonsoft.Json.Linq.JObject jsonObject in array) {
            string type = (string) jsonObject["type"];
            string test = (string) jsonObject["test"];
            Debug.Log(test);
            switch (type) {
                case "human":
                    humans.Add(new Human {
                    age = (int) jsonObject["age"],
                    weight = (float) jsonObject["weight"],
                    height = (float) jsonObject["height"],
                    favouriteColor = (string) jsonObject["favouriteColor"],
                    name = (string) jsonObject["name"],
                    type = type
                    });
                    break;
            }

        }
        Debug.Log(humans.Count);

    }
    private class Animal {
        public int age;
        public float weight;
        public float height;
        public string type;
    }

    private class NamedAnimal : Animal {
        public string name;
    }
    private class Human : NamedAnimal {
        public string favouriteColor;
    }

    private class Dog : NamedAnimal {
        public string favouriteBall;
    }

    private class Bird : Animal {
        public bool canFly;
    }
    private class AnimalWrapper {
        public List<Animal> animals;
    }
    private class AnimalWrapperWrapper {
        public List<AnimalWrapper> animalWrappers;
    }
    /*
    private void testIdMapWrite() {
        TileData tileData = new TileData {
            id = 0,
            spritePath = "Sprites/pixil-frame-0|pixil-frame-0",
            name = "weirdstone1",
            hardness = 4,
            dataType = "Tile",
            tileType = "TileBlock",
            tileDecorators = new TileDecoratorDataWrapper() {
                decoratorData = new List<TileDecoratorData> {
                    new LightDecoratorData() {
                        lightLevel = 15
                    },
                    new AnimatorDecoratorData() {
                        animated = true
                    }
                }
            }
        };
        TileData tileData2 = new TileData {
            id = 1,
            spritePath = "Sprites/pixeltest|pixeltest",
            name = "weirdstone2",
            hardness = 6,
            dataType = "Tile",
            tileType = "TileBlock",
            tileDecorators = new TileDecoratorDataWrapper() {
                decoratorData = new List<TileDecoratorData> {
                    new LightDecoratorData() {
                        lightLevel = 15
                    }
                }
            }
        };
        TileData tileData3 = new TileData {
            id = 2,
            spritePath = "Sprites/pixel2|pixel2",
            name = "weirdstone2",
            hardness = 6,
            dataType = "Tile",
            tileType = "TileBlock",
            tileDecorators = new TileDecoratorDataWrapper() {
                decoratorData = new List<TileDecoratorData> {
                    new AnimatorDecoratorData() {
                        animated = true
                    }
                }
            }
        };
        IdDataWrapper idDataWrapper = new IdDataWrapper() {
            idData = new List<IdData>() {
                tileData,
                tileData2,
                tileData3
            }
        };
        string animalWrapperJson = Newtonsoft.Json.JsonConvert.SerializeObject(idDataWrapper, Newtonsoft.Json.Formatting.Indented);
        string filePath = Application.dataPath + "/Resources/Json/IdMap.json";
        File.WriteAllText(filePath, animalWrapperJson);
    }

    private void testIdMapRead() {
        IdDataMap idDataMap = IdDataMap.getInstance();
    }
    */
}
