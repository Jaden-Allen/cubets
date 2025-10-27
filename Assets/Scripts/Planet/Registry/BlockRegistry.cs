using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class BlockRegistry : MonoBehaviour
{
    public List<BlockType> blocks = new List<BlockType>();
    public static Dictionary<string, BlockType> typeIdToBlock = new Dictionary<string, BlockType>();
    public static Dictionary<uint, BlockType> indexToBlock = new Dictionary<uint, BlockType>();
    public static Dictionary<string, uint> typeIdToIndex = new Dictionary<string, uint>();
    
    public static List<BlockType> Blocks = new List<BlockType>();

    public void Init() {
        uint index = 0;
        foreach (var block in blocks) {
            typeIdToBlock[block.id] = block;
            indexToBlock[index] = block;
            typeIdToIndex[block.id] = index;
            Blocks.Add(block);
            block.Setup();
            index++;
        }
    }
    private void OnDestroy() {
        typeIdToBlock.Clear();
        indexToBlock.Clear();
        typeIdToIndex.Clear();
        Blocks.Clear();
    }
    private void OnValidate() {
        blocks = blocks.OrderBy(b => b.id == "air" ? 0 : 1).ThenBy(b => b.id).ToList();
    }
    [ContextMenu("Copy Registry Items")]
    public void GetRegistryList() {
        List<string> registryItems = new List<string>() {
            "public struct BlockTypeEnum {\r\n    public uint registryIndex;\r\n    public string typeId;\r\n    public BlockType type;\r\n}\n",
            "public static class BlockTypes {"
        };
        for (int i = 0; i < blocks.Count; i++) {
            var block = blocks[i];
            string itemName = Regex.Replace(block.name, @"\s+", "");
            registryItems.Add($"    public static BlockTypeEnum {itemName} => new BlockTypeEnum() {{ typeId = \"{block.id}\", registryIndex = {i}, type = BlockRegistry.indexToBlock[{i}] }};");
        }
        registryItems.Add("}");
        string joined = string.Join("\n", registryItems);

        string blockTypesCsPath = Path.Combine(Application.dataPath, "Scripts", "Planet", "Helper", "BlockTypes.cs");
        File.WriteAllText(blockTypesCsPath, joined);
        AssetDatabase.Refresh();
        Debug.Log($"Copied {blocks.Count} registry enums to the BlockTypes.cs!");
    }
}
