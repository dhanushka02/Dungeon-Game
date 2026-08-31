#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class DungeonHierarchyOrganizer
{
    private static readonly string[] GroupNames =
    {
        "00_SYSTEMS",
        "01_LIGHTING",
        "02_ARCHITECTURE",
        "03_FURNITURE",
        "04_PROPS",
        "05_MISC"
    };

    [MenuItem("Tools/Dungeon/Organize Current Scene Hierarchy")]
    private static void OrganizeFromMenu()
    {
        OrganizeCurrentScene();
    }

    private static void OrganizeCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No loaded scene is available to organize.");
            return;
        }

        Dictionary<string, Transform> groups = CreateGroups(scene);
        Transform props = groups["04_PROPS"];

        Dictionary<string, Transform> propGroups = new Dictionary<string, Transform>
        {
            ["Barrels_Bags_Containers"] = CreateGroup("Barrels_Bags_Containers", props),
            ["Books_Scrolls"] = CreateGroup("Books_Scrolls", props),
            ["Alchemy"] = CreateGroup("Alchemy", props),
            ["Food_Tableware"] = CreateGroup("Food_Tableware", props),
            ["Weapons_Decor"] = CreateGroup("Weapons_Decor", props),
            ["Plants_Nature"] = CreateGroup("Plants_Nature", props),
            ["Other_Props"] = CreateGroup("Other_Props", props)
        };

        int moved = 0;
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (Array.IndexOf(GroupNames, root.name) >= 0)
            {
                continue;
            }

            Transform destination = ResolveDestination(root.name, groups, propGroups);
            Undo.SetTransformParent(root.transform, destination, "Organize dungeon hierarchy");
            moved++;
        }

        SortChildren(groups["00_SYSTEMS"]);
        SortChildren(groups["01_LIGHTING"]);
        SortChildren(groups["02_ARCHITECTURE"]);
        SortChildren(groups["03_FURNITURE"]);
        SortChildren(groups["05_MISC"]);
        foreach (Transform group in propGroups.Values)
        {
            SortChildren(group);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Dungeon hierarchy organized: {moved} root objects grouped without changing world positions.");
    }

    private static Dictionary<string, Transform> CreateGroups(Scene scene)
    {
        Dictionary<string, Transform> groups = new Dictionary<string, Transform>();
        foreach (string name in GroupNames)
        {
            GameObject existing = Array.Find(scene.GetRootGameObjects(), item => item.name == name);
            groups[name] = existing != null ? existing.transform : CreateGroup(name, null);
        }

        return groups;
    }

    private static Transform CreateGroup(string name, Transform parent)
    {
        Transform existing = parent != null ? parent.Find(name) : null;
        if (existing != null)
        {
            return existing;
        }

        GameObject group = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(group, "Create hierarchy group");
        if (parent != null)
        {
            Undo.SetTransformParent(group.transform, parent, "Create hierarchy group");
        }

        return group.transform;
    }

    private static Transform ResolveDestination(
        string objectName,
        Dictionary<string, Transform> groups,
        Dictionary<string, Transform> propGroups)
    {
        string name = RemoveInstanceSuffix(objectName);

        if (Matches(name, "Main Camera", "PlayerCapsule", "PlayerFollowCamera", "Spline"))
        {
            return groups["00_SYSTEMS"];
        }

        if (StartsWithAny(name, "Directional Light", "Reflection Probe", "CandleLight", "Candle_Wall", "Candelier", "Torch_Wall"))
        {
            return groups["01_LIGHTING"];
        }

        if (StartsWithAny(name, "dungeon_room", "Dungeon", "Basement Passage", "Balcony", "FloorTIle", "Stairs_Platform", "Steps_Small", "Door_Wooden", "Fireplace"))
        {
            return groups["02_ARCHITECTURE"];
        }

        if (StartsWithAny(name, "Chair", "Table", "Shelf", "Stool", "Throne", "WeaponRack", "Carpet"))
        {
            return groups["03_FURNITURE"];
        }

        if (StartsWithAny(name, "Barrel", "Bag", "Bucket", "Chest"))
        {
            return propGroups["Barrels_Bags_Containers"];
        }

        if (StartsWithAny(name, "Book", "Scroll"))
        {
            return propGroups["Books_Scrolls"];
        }

        if (StartsWithAny(name, "Beaker", "Flask", "Potion", "CrystalBall", "Cauldron"))
        {
            return propGroups["Alchemy"];
        }

        if (StartsWithAny(name, "Plate", "Fork", "Knife", "Goblet", "Jug", "Jar", "Meat", "Carrot", "Potato"))
        {
            return propGroups["Food_Tableware"];
        }

        if (StartsWithAny(name, "Sword", "Shield", "Skull", "Amphora"))
        {
            return propGroups["Weapons_Decor"];
        }

        if (StartsWithAny(name, "Ivy", "Plant", "Tree_Branch"))
        {
            return propGroups["Plants_Nature"];
        }

        if (StartsWithAny(name, "Pot", "Jar", "Crystal", "Carpet"))
        {
            return propGroups["Other_Props"];
        }

        return groups["05_MISC"];
    }

    private static string RemoveInstanceSuffix(string value)
    {
        int suffixStart = value.LastIndexOf(" (", StringComparison.Ordinal);
        if (suffixStart < 0 || !value.EndsWith(")", StringComparison.Ordinal))
        {
            return value;
        }

        string number = value.Substring(suffixStart + 2, value.Length - suffixStart - 3);
        return int.TryParse(number, out _) ? value.Substring(0, suffixStart) : value;
    }

    private static bool Matches(string value, params string[] candidates)
    {
        return Array.Exists(candidates, candidate => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));
    }

    private static bool StartsWithAny(string value, params string[] prefixes)
    {
        return Array.Exists(prefixes, prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static void SortChildren(Transform parent)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent)
        {
            children.Add(child);
        }

        children.Sort((left, right) => EditorUtility.NaturalCompare(left.name, right.name));
        for (int index = 0; index < children.Count; index++)
        {
            children[index].SetSiblingIndex(index);
        }
    }
}
#endif
