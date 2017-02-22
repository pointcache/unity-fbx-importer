using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;

public class FbxImporter : MonoBehaviour {

    [Serializable]
    public class FBXFile {
        public string Name;
        public List<FBXNode> nodes = new List<FBXNode>();
    }

    [Serializable]
    public class FBXNode {
        public string Name;
        public List<string> properties;
        public List<FBXNode> nodes = new List<FBXNode>();
    }

    public string path;
    public FBXFile fbx;

    void OnEnable() {

        path = Application.dataPath + "/" + path + ".fbx";
        fbx = ParseFile(path);
    }

    public static FBXFile ParseFile(string path) {
        FBXFile file = new FBXFile();

        try {
            using (StreamReader sr = new StreamReader(path)) {
                while (!sr.EndOfStream) {
                    string line = sr.ReadLine();
                    //Skip comments and empty lines
                    if (line.Length == 0 || isComment(line))
                        continue;
                    //detect node start
                    if (line.Contains(":")) {
                        file.nodes.Add(new_node(sr, line));
                    }
                }
            }
        }
        catch (Exception e) {
            Debug.LogError("The file could not be read:");
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
        }

        file.Name = Path.GetFileNameWithoutExtension(path);
        return file;
    }

    static FBXNode new_node(StreamReader sr, string line) {
        if (!line.Contains(":") || isComment(line))
            return null;

        FBXNode node = new FBXNode();

        if (line.Contains("{")) {
            string next = "";
            while (!next.Contains("}")) {
                next = sr.ReadLine();
                if (!next.Contains("}"))
                    addNode(node, new_node(sr, next));
            }
        }

        string[] split1 = line.Split(':');
        node.Name = split1[0].Replace("\t", "");

        string props = line.Remove(0, split1[0].Length + 1);
        props = props.Remove(props.Length - 1, 1);
        string[] propsSplit = props.Split(',');
        if (propsSplit.Length > 0) {

            for (int i = 0; i < propsSplit.Length; i++) {
                if (IsNullOrWhiteSpace(propsSplit[i]))
                    continue;

                if (node.properties == null)
                    node.properties = new List<string>();

                if (propsSplit[i][0] == ' ')
                    node.properties.Add(propsSplit[i].Remove(0, 1));
                else
                    node.properties.Add(propsSplit[i]);
            }
        }

        return node;
    }

    static void addNode(FBXNode parent, FBXNode child) {
        if (child == null)
            return;
        parent.nodes.Add(child);
    }

    static bool isComment(string line) {
        bool result = false;
        for (int i = 0; i < line.Length; i++) {
            if (line[i] == ' ' || line[i] == '\t')
                continue;
            if (line[i] == ';') {
                result = true;
                break;
            } else
                break;
        }
        return result;
    }

    public static bool IsNullOrWhiteSpace(String value) {
        if (value == null) return true;

        for (int i = 0; i < value.Length; i++) {
            if (!Char.IsWhiteSpace(value[i])) return false;
        }

        return true;
    }
}
