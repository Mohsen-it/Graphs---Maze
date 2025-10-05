using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GraphTeachingApp
{
    /// <summary>
    /// فئة تمثل العقدة في الرسم البياني
    /// تحتوي على المعلومات الأساسية للعقدة مثل الاسم والموقع واللون
    /// </summary>
    public class Node
    {
        public string Name { get; set; }
        public Point Position { get; set; }
        public Color NodeColor { get; set; }
        public string Label { get; set; }

        /// <summary>
        /// إنشاء عقدة جديدة بالاسم والموقع
        /// </summary>
        public Node(string name, Point position)
        {
            Name = name;
            Position = position;
            NodeColor = Color.Gray; // اللون الافتراضي
            Label = name;
        }

        /// <summary>
        /// إنشاء عقدة جديدة بالاسم والموقع واللون
        /// </summary>
        public Node(string name, Point position, Color color)
        {
            Name = name;
            Position = position;
            NodeColor = color;
            Label = name;
        }
    }

    /// <summary>
    /// فئة تمثل الوصلة (الحافة) بين عقدتين في الرسم البياني
    /// تحتوي على العقدتين المتصلتين والوزن إذا كان الرسم موجهاً
    /// </summary>
    public class Edge
    {
        public Node From { get; set; }
        public Node To { get; set; }
        public int Weight { get; set; }
        public Color EdgeColor { get; set; }

        /// <summary>
        /// إنشاء وصلة جديدة بين عقدتين
        /// </summary>
        public Edge(Node from, Node to)
        {
            From = from;
            To = to;
            Weight = 1; // الوزن الافتراضي
            EdgeColor = Color.Black;
        }

        /// <summary>
        /// إنشاء وصلة جديدة بين عقدتين مع وزن محدد
        /// </summary>
        public Edge(Node from, Node to, int weight)
        {
            From = from;
            To = to;
            Weight = weight;
            EdgeColor = Color.Black;
        }

        /// <summary>
        /// إنشاء وصلة جديدة بين عقدتين مع وزن ولون محددين
        /// </summary>
        public Edge(Node from, Node to, int weight, Color color)
        {
            From = from;
            To = to;
            Weight = weight;
            EdgeColor = color;
        }
    }

    /// <summary>
    /// فئة الرسم البياني الرئيسية
    /// تحتوي على جميع العقد والوصلات وتوفر العمليات الأساسية والخوارزميات
    /// </summary>
    public class Graph
    {
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
        public Dictionary<string, List<string>> AdjacencyList { get; set; }
        public int[,] AdjacencyMatrix { get; set; }
        public bool IsDirected { get; set; }
        public bool IsWeighted { get; set; }

        /// <summary>
        /// إنشاء رسم بياني جديد فارغ
        /// </summary>
        public Graph()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
            AdjacencyList = new Dictionary<string, List<string>>();
            IsDirected = false; // افتراضياً غير موجه
            IsWeighted = false; // افتراضياً بدون أوزان
        }

        /// <summary>
        /// إضافة عقدة جديدة إلى الرسم البياني
        /// </summary>
        public void AddNode(Node node)
        {
            if (!Nodes.Any(n => n.Name == node.Name))
            {
                Nodes.Add(node);
                AdjacencyList[node.Name] = new List<string>();
                UpdateAdjacencyMatrix();
            }
        }

        /// <summary>
        /// إزالة عقدة من الرسم البياني مع جميع الوصلات المرتبطة بها
        /// </summary>
        public void RemoveNode(string nodeName)
        {
            var node = Nodes.FirstOrDefault(n => n.Name == nodeName);
            if (node != null)
            {
                Nodes.Remove(node);
                Edges.RemoveAll(e => e.From.Name == nodeName || e.To.Name == nodeName);
                AdjacencyList.Remove(nodeName);
                foreach (var list in AdjacencyList.Values)
                {
                    list.Remove(nodeName);
                }
                UpdateAdjacencyMatrix();
            }
        }

        /// <summary>
        /// إضافة وصلة جديدة بين عقدتين
        /// </summary>
        public void AddEdge(string fromNode, string toNode, int weight = 1)
        {
            var from = Nodes.FirstOrDefault(n => n.Name == fromNode);
            var to = Nodes.FirstOrDefault(n => n.Name == toNode);

            if (from != null && to != null)
            {
                var edge = new Edge(from, to, weight);
                Edges.Add(edge);

                if (!AdjacencyList.ContainsKey(fromNode))
                    AdjacencyList[fromNode] = new List<string>();
                if (!AdjacencyList[fromNode].Contains(toNode))
                    AdjacencyList[fromNode].Add(toNode);

                if (!IsDirected)
                {
                    if (!AdjacencyList.ContainsKey(toNode))
                        AdjacencyList[toNode] = new List<string>();
                    if (!AdjacencyList[toNode].Contains(fromNode))
                        AdjacencyList[toNode].Add(fromNode);
                }

                UpdateAdjacencyMatrix();
            }
        }

        /// <summary>
        /// إزالة وصلة بين عقدتين
        /// </summary>
        public void RemoveEdge(string fromNode, string toNode)
        {
            Edges.RemoveAll(e => e.From.Name == fromNode && e.To.Name == toNode);

            if (AdjacencyList.ContainsKey(fromNode))
                AdjacencyList[fromNode].Remove(toNode);

            if (!IsDirected && AdjacencyList.ContainsKey(toNode))
                AdjacencyList[toNode].Remove(fromNode);

            UpdateAdjacencyMatrix();
        }

        /// <summary>
        /// تحديث مصفوفة التجاور بناءً على العقد والوصلات الحالية
        /// </summary>
        private void UpdateAdjacencyMatrix()
        {
            if (Nodes.Count == 0)
            {
                AdjacencyMatrix = new int[0, 0];
                return;
            }

            AdjacencyMatrix = new int[Nodes.Count, Nodes.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                for (int j = 0; j < Nodes.Count; j++)
                {
                    AdjacencyMatrix[i, j] = 0;
                }
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (AdjacencyList.ContainsKey(Nodes[i].Name))
                {
                    for (int j = 0; j < Nodes.Count; j++)
                    {
                        if (AdjacencyList[Nodes[i].Name].Contains(Nodes[j].Name))
                        {
                            AdjacencyMatrix[i, j] = 1; // يمكن تعديلها لتخزين الأوزان
                        }
                    }
                }
            }
        }

        /// <summary>
        /// الحصول على قائمة بأسماء جميع العقد
        /// </summary>
        public List<string> GetNodeNames()
        {
            return Nodes.Select(n => n.Name).ToList();
        }

        /// <summary>
        /// الحصول على درجة العقدة (عدد الوصلات المرتبطة بها)
        /// </summary>
        public int GetNodeDegree(string nodeName)
        {
            if (AdjacencyList.ContainsKey(nodeName))
            {
                return AdjacencyList[nodeName].Count;
            }
            return 0;
        }

        /// <summary>
        /// الحصول على متتالية الدرجات لجميع العقد مرتبة تنازلياً
        /// </summary>
        public List<int> GetDegreeSequence()
        {
            var degrees = new List<int>();
            foreach (var node in Nodes)
            {
                degrees.Add(GetNodeDegree(node.Name));
            }
            degrees.Sort((a, b) => b.CompareTo(a)); // ترتيب تنازلي
            return degrees;
        }

        /// <summary>
        /// حساب عدد العقد والوصلات في الرسم البياني
        /// </summary>
        public (int nodeCount, int edgeCount) GetGraphInfo()
        {
            return (Nodes.Count, Edges.Count);
        }
    }
}