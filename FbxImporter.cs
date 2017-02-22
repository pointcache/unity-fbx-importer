namespace FBXTools {

    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    [Serializable]
    public class FBXFile {
        public string Name;
        public List<FBXNode> nodes = new List<FBXNode>();



        /// <summary>
        /// Search for a single node 
        /// path = "SomeRootNode/SubNode1/Subnode2..."
        /// Will return first found even if multiple exists, use with caution
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FBXNode FindNode(string path) {
            FBXNode result = null;
            try {
                string[] split = path.Split('/');
                FBXNode root = nodes.FirstOrDefault(x => x.Name == split[0]);
                if (split.Length > 1)
                    result = findRecursive(root, split, 1);
                else
                    result = root;
            }
            catch (Exception e) {
                Debug.LogError("Node could not be found");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
            return result;
        }

        private FBXNode findRecursive(FBXNode node, string[] path, int index) {
            FBXNode result = node.nodes.FirstOrDefault(x => x.Name == path[index]);
            if (path.Length < index+1) {
                findRecursive(result, path, index++);
                return result;
            }else
                return result;
        }
    }

    [Serializable]
    public class FBXNode {
        public string Name;
        public List<string> properties;
        public List<FBXNode> nodes = new List<FBXNode>();

        /// <summary>
        /// Search for multiple child nodes with the same name
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public FBXNode[] FindNodes(string name) {
            try {
                return nodes.Where(x => x.Name == name).ToArray();
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
            return null;
        }
    }

    public class FbxImporter : MonoBehaviour {

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
}