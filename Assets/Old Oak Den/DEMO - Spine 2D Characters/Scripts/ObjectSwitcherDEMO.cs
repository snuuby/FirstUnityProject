using System.Collections.Generic;
using UnityEngine;
using TMPro; // Pøidáme TextMeshPro namespace
using Spine;
using Spine.Unity;

namespace ApocalypseHeroesSpine2DCharactersDEMO
{
public class ObjectSwitcherDEMO : MonoBehaviour
{
    public List<GameObject> demoObjects; // seznam objektù ve scénì
    public TMP_Dropdown demoCharacterDropdown; // roletkové menu pro výbìr postavy
    public TMP_Dropdown demoAnimationDropdown; // roletkové menu pro výbìr animace
    public TMP_Dropdown demoSkinDropdown; // roletkové menu pro výbìr skinu

    private SkeletonAnimation currentSkeletonAnimation;
    private string currentAnimationName;

    void Start()
    {
        // na zaèátku nastavíme všechny objekty jako neaktivní
        foreach (GameObject obj in demoObjects)
        {
            obj.SetActive(false);
        }

        // naplníme roletkové menu pro výbìr postavy
        demoCharacterDropdown.ClearOptions();
        List<string> characterNames = new List<string>();
        foreach (GameObject obj in demoObjects)
        {
            characterNames.Add(obj.name);
        }
        demoCharacterDropdown.AddOptions(characterNames);
        demoCharacterDropdown.interactable = characterNames.Count > 1; // nastavíme roletkové menu jako interaktivní pouze pokud je více než jedna možnost výbìru

        // nastavíme akci, která se provede pøi výbìru postavy
        demoCharacterDropdown.onValueChanged.AddListener(delegate {
            SwitchCharacter(demoCharacterDropdown.value);
        });

        // nastavíme akci, která se provede pøi výbìru animace
        demoAnimationDropdown.onValueChanged.AddListener(delegate {
            PlayAnimation(demoAnimationDropdown.options[demoAnimationDropdown.value].text);
        });

        // nastavíme akci, která se provede pøi výbìru skinu
        demoSkinDropdown.onValueChanged.AddListener(delegate {
            SwitchSkin(demoSkinDropdown.options[demoSkinDropdown.value].text);
        });

        // aktivujeme první objekt
        if (demoObjects.Count > 0)
        {
            SwitchCharacter(0);
        }
    }

    void Update()
    {
        // pøepínání postav pomocí šipek nahoru a dolu
        if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                demoCharacterDropdown.value = (demoCharacterDropdown.value + 1) % demoCharacterDropdown.options.Count;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                demoCharacterDropdown.value = (demoCharacterDropdown.value - 1 + demoCharacterDropdown.options.Count) % demoCharacterDropdown.options.Count;
            }
        }

        // pøepínání animací pomocí šipek nahoru a dolu s drženým Alt
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                demoAnimationDropdown.value = (demoAnimationDropdown.value + 1) % demoAnimationDropdown.options.Count;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                demoAnimationDropdown.value = (demoAnimationDropdown.value - 1 + demoAnimationDropdown.options.Count) % demoAnimationDropdown.options.Count;
            }
        }

        // pøepínání skinù pomocí šipek nahoru a dolu s drženým Ctrl
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                demoSkinDropdown.value = (demoSkinDropdown.value + 1) % demoSkinDropdown.options.Count;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                demoSkinDropdown.value = (demoSkinDropdown.value - 1 + demoSkinDropdown.options.Count) % demoSkinDropdown.options.Count;
            }
        }
    }

    private void SwitchCharacter(int index)
    {
        // deaktivujeme aktuální objekt
        if (currentSkeletonAnimation != null)
        {
            currentSkeletonAnimation.gameObject.SetActive(false);
        }

        // aktivujeme nový objekt
        GameObject newObject = demoObjects[index];
        newObject.SetActive(true);
        currentSkeletonAnimation = newObject.GetComponent<SkeletonAnimation>();

        // naplníme roletkové menu pro výbìr animace
        demoAnimationDropdown.ClearOptions();
        List<string> animationNames = new List<string>();
        foreach (var animation in currentSkeletonAnimation.Skeleton.Data.Animations)
        {
            animationNames.Add(animation.Name);
        }
        demoAnimationDropdown.AddOptions(animationNames);
        demoAnimationDropdown.interactable = animationNames.Count > 1; // nastavíme roletkové menu jako interaktivní pouze pokud je více než jedna možnost výbìru

        // naplníme roletkové menu pro výbìr skinu
        demoSkinDropdown.ClearOptions();
        List<string> skinNames = new List<string>();
        foreach (var skin in currentSkeletonAnimation.Skeleton.Data.Skins)
        {
            skinNames.Add(skin.Name);
        }
        if (skinNames.Count == 0) // pokud neexistují žádné skiny, pøidáme možnost "default"
        {
            skinNames.Add("default");
        }
        else if (skinNames.Count > 1 && skinNames.Contains("default")) // pokud existují další skiny, odstraníme možnost "default"
        {
            skinNames.Remove("default");
        }
        demoSkinDropdown.AddOptions(skinNames);
        demoSkinDropdown.interactable = skinNames.Count > 1; // nastavíme roletkové menu jako interaktivní pouze pokud je více než jedna možnost výbìru

        // pokud je dostupná stejná animace jako u pøedchozí postavy, spustíme ji
        int animationIndex = animationNames.IndexOf(currentAnimationName);
        if (animationIndex >= 0)
        {
            demoAnimationDropdown.value = animationIndex;
            PlayAnimation(currentAnimationName);
        }
        // jinak spustíme první animaci
        else if (animationNames.Count > 0)
        {
            PlayAnimation(animationNames[0]);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (currentSkeletonAnimation != null)
        {
            currentSkeletonAnimation.AnimationState.SetAnimation(0, animationName, true);
            currentAnimationName = animationName;
        }
    }

    private void SwitchSkin(string skinName)
    {
        if (currentSkeletonAnimation != null)
        {
            currentSkeletonAnimation.initialSkinName = skinName;
            currentSkeletonAnimation.Initialize(true);

            // po zmìnì skinu znovu spustíme aktuálnì vybranou animaci
            PlayAnimation(currentAnimationName);
        }
    }
}
}