using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GraphTeachingApp
{
    /// <summary>
    /// فئة تمثل العقدة (Node) في الرسم البياني
    /// تحتوي على جميع المعلومات الأساسية للعقدة مثل:
    /// - الاسم الفريد للعقدة (مثل A, B, C)
    /// - الموقع (الإحداثيات) في لوحة الرسم
    /// - اللون الحالي للعقدة (يستخدم في التمييز والتلوين)
    /// - التسمية المعروضة للعقدة
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
    /// فئة تمثل الوصلة (Edge/Arc) بين عقدتين في الرسم البياني
    /// تحتوي على جميع المعلومات الخاصة بالوصلة:
    /// - العقدة المصدر (From) والعقدة الهدف (To)
    /// - الوزن (Weight) للرسوم البيانية الموزونة
    /// - اللون الحالي للوصلة (يستخدم في التمييز والتلوين)
    ///
    /// مثال على الاستخدام:
    /// Edge edge = new Edge(nodeA, nodeB, 5); // وصلة من A إلى B بوزن 5
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
    /// فئة الرسم البياني (Graph) الرئيسية - قلب نظام تعليم الرسوم البيانية
    /// تحتوي على جميع البيانات والعمليات الأساسية للرسم البياني:
    ///
    /// البيانات الأساسية:
    /// - قائمة العقد (Nodes): جميع العقد الموجودة في الرسم البياني
    /// - قائمة الوصلات (Edges): جميع الروابط بين العقد
    /// - قائمة التجاور (AdjacencyList): تمثيل الرسم كقوائم مجاورة
    /// - مصفوفة التجاور (AdjacencyMatrix): تمثيل الرسم كمصفوفة ثنائية الأبعاد
    ///
    /// خصائص الرسم البياني:
    /// - IsDirected: هل الرسم موجهاً (directed) أم غير موجه (undirected)
    /// - IsWeighted: هل الرسم موزون (weighted) أم غير موزون (unweighted)
    ///
    /// العمليات المتاحة:
    /// - إضافة وحذف العقد والوصلات
    /// - حساب درجات العقد ومتتالية الدرجات
    /// - تحديث تمثيلات البيانات المختلفة
    /// - استرجاع معلومات الرسم البياني
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
        /// إضافة وصلة جديدة بين عقدتين في الرسم البياني
        ///
        /// هذه العملية الأساسية تربط عقدتين بواسطة وصلة (حافة) وتقوم بما يلي:
        /// 1. إنشاء كائن وصلة جديد يحتوي على العقدتين والوزن
        /// 2. إضافة الوصلة إلى قائمة الوصلات الرئيسية
        /// 3. تحديث قائمة التجاور للعقدة المصدر
        /// 4. إضافة الاتجاه المعاكس إذا كان الرسم غير موجه
        /// 5. تحديث مصفوفة التجاور للحفاظ على التناسق
        /// </summary>
        /// <param name="fromNode">اسم العقدة المصدر (من أين تبدأ الوصلة)</param>
        /// <param name="toNode">اسم العقدة الهدف (إلى أين تنتهي الوصلة)</param>
        /// <param name="weight">وزن الوصلة (افتراضياً 1 للرسوم غير الموزونة)</param>
        ///
        /// <example>
        /// رسم بياني غير موجه:
        /// <code>
        /// graph.AddEdge("A", "B"); // إضافة وصلة من A إلى B
        /// // في الرسوم غير الموجهة، ستضاف وصلة أخرى من B إلى A تلقائياً
        /// </code>
        ///
        /// رسم بياني موجه:
        /// <code>
        /// graph.IsDirected = true;
        /// graph.AddEdge("A", "B", 5); // إضافة وصلة موجهة من A إلى B بوزن 5
        /// </code>
        /// </example>
        public void AddEdge(string fromNode, string toNode, int weight = 1)
        {
            // البحث عن العقدتين في قائمة العقد
            var from = Nodes.FirstOrDefault(n => n.Name == fromNode);
            var to = Nodes.FirstOrDefault(n => n.Name == toNode);

            // التحقق من وجود العقدتين قبل إضافة الوصلة
            if (from != null && to != null)
            {
                // المرحلة الأولى: إنشاء كائن الوصلة الجديد
                var edge = new Edge(from, to, weight);
                Edges.Add(edge);

                // المرحلة الثانية: تحديث قائمة التجاور للعقدة المصدر
                // قائمة التجاور تخزن جيران كل عقدة
                if (!AdjacencyList.ContainsKey(fromNode))
                    AdjacencyList[fromNode] = new List<string>();

                // إضافة العقدة الهدف لجيران العقدة المصدر (إذا لم تكن موجودة)
                if (!AdjacencyList[fromNode].Contains(toNode))
                    AdjacencyList[fromNode].Add(toNode);

                // المرحلة الثالثة: التعامل مع الرسوم غير الموجهة
                // في الرسوم غير الموجهة، الوصلة تعمل في كلا الاتجاهين
                if (!IsDirected)
                {
                    // إضافة الاتجاه المعاكس للحفاظ على خاصية عدم التوجيه
                    if (!AdjacencyList.ContainsKey(toNode))
                        AdjacencyList[toNode] = new List<string>();
                    if (!AdjacencyList[toNode].Contains(fromNode))
                        AdjacencyList[toNode].Add(fromNode);
                }

                // المرحلة الرابعة: تحديث مصفوفة التجاور للحفاظ على التناسق
                UpdateAdjacencyMatrix();
            }
            else
            {
                // رسالة خطأ يمكن إضافتها لاحقاً إذا لم تكن العقد موجودة
                // throw new ArgumentException("العقدة المصدر أو الهدف غير موجودة في الرسم البياني");
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
        ///
        /// مصفوفة التجاور هي تمثيل ثنائي الأبعاد للرسم البياني حيث:
        /// - الصفوف والأعمدة تمثل العقد (بنفس الترتيب في قائمة Nodes)
        /// - القيمة 1 تعني وجود وصلة بين العقدتين
        /// - القيمة 0 تعني عدم وجود وصلة
        ///
        /// خوارزمية العمل:
        /// 1. إنشاء مصفوفة جديدة بالحجم المناسب (n×n) حيث n هو عدد العقد
        /// 2. تهيئة جميع القيم بصفر (عدم وجود وصلات افتراضياً)
        /// 3. المرور على كل عقدة وفحص جيرانها في قائمة التجاور
        /// 4. وضع القيمة 1 في المواضع التي توجد بها وصلات
        /// </summary>
        private void UpdateAdjacencyMatrix()
        {
            // المرحلة الأولى: التحقق من وجود عقد في الرسم البياني
            if (Nodes.Count == 0)
            {
                AdjacencyMatrix = new int[0, 0]; // مصفوفة فارغة إذا لم تكن هناك عقد
                return;
            }

            // المرحلة الثانية: إنشاء مصفوفة جديدة بالحجم المناسب
            // الحجم = عدد العقد × عدد العقد (مربعة دائماً)
            AdjacencyMatrix = new int[Nodes.Count, Nodes.Count];

            // المرحلة الثالثة: تهيئة المصفوفة بقيم صفر
            // هذه المرحلة ضرورية لضمان عدم وجود قيم عشوائية
            for (int i = 0; i < Nodes.Count; i++)
            {
                for (int j = 0; j < Nodes.Count; j++)
                {
                    AdjacencyMatrix[i, j] = 0; // عدم وجود وصلة افتراضياً
                }
            }

            // المرحلة الرابعة: ملء المصفوفة بناءً على قائمة التجاور
            // لكل عقدة في الرسم البياني:
            for (int i = 0; i < Nodes.Count; i++)
            {
                string currentNodeName = Nodes[i].Name;

                // التحقق من وجود قائمة التجاور لهذه العقدة
                if (AdjacencyList.ContainsKey(currentNodeName))
                {
                    // فحص كل عقدة أخرى في الرسم البياني
                    for (int j = 0; j < Nodes.Count; j++)
                    {
                        string targetNodeName = Nodes[j].Name;

                        // إذا كانت العقدة الهدف موجودة في جيران العقدة الحالية
                        if (AdjacencyList[currentNodeName].Contains(targetNodeName))
                        {
                            AdjacencyMatrix[i, j] = 1; // يوجد وصلة من i إلى j
                            // ملاحظة: يمكن تعديل هذا لتخزين الأوزان بدلاً من 1
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
        /// الحصول على متتالية الدرجات (Degree Sequence) لجميع العقد مرتبة تنازلياً
        ///
        /// متتالية الدرجات هي مفهوم أساسي في نظرية الرسوم البيانية:
        /// - تمثل قائمة بدرجات جميع العقد في الرسم البياني
        /// - الدرجة = عدد الوصلات المرتبطة بالعقدة
        /// - يتم ترتيبها تنازلياً (من الأكبر إلى الأصغر)
        ///
        /// أهمية متتالية الدرجات:
        /// - تحديد ما إذا كان الرسم منتظم (regular) أم لا
        /// - اختبار ما إذا كانت متتالية معطاة صالحة لبناء رسم بياني
        /// - استخدامها في خوارزمية Havel-Hakimi لفحص البناء
        ///
        /// مثال:
        /// رسم بياني بعقد درجاتها 3,2,2,1
        /// متتالية الدرجات: [3, 2, 2, 1]
        /// </summary>
        /// <returns>قائمة بالدرجات مرتبة تنازلياً</returns>
        public List<int> GetDegreeSequence()
        {
            var degrees = new List<int>();

            // حساب درجة كل عقدة في الرسم البياني
            foreach (var node in Nodes)
            {
                degrees.Add(GetNodeDegree(node.Name));
            }

            // ترتيب الدرجات تنازلياً (من الأكبر إلى الأصغر)
            // هذا الترتيب مهم لخوارزميات مثل Havel-Hakimi
            degrees.Sort((a, b) => b.CompareTo(a));

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