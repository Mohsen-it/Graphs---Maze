using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.Json;

namespace GraphTeachingApp
{
    /// <summary>
    /// النافذة الرئيسية لتطبيق تعليم الرسوم البيانية
    /// تحتوي على جميع عناصر الواجهة والوظائف التفاعلية
    /// </summary>
    public partial class MainForm : Form
    {
        // متغيرات عامة للنافذة
        private Graph currentGraph; // الرسم البياني الحالي
        private Timer animationTimer; // مؤقت للرسوم المتحركة
        private List<Node> highlightedNodes; // العقد المضيئة مؤقتاً
        private List<Edge> highlightedEdges; // الحواف المضيئة مؤقتاً
        private Node selectedNode; // العقدة المحددة حالياً
        private Node firstNodeForEdge; // العقدة الأولى المحددة لربط وصلة جديدة
        private bool isDrawing; // هل نحن في وضع الرسم
        private Point lastMousePosition; // آخر موقع للفأرة
        private float zoomFactor; // عامل التكبير/التصغير
        private Point zoomCenter; // مركز التكبير

        // ألوان مختلفة للحالات المختلفة
        private readonly Color NORMAL_COLOR = Color.Gray;
        private readonly Color HIGHLIGHT_COLOR = Color.Blue;
        private readonly Color VISITED_COLOR = Color.Green;
        private readonly Color COMPLEMENT_COLOR = Color.Red;
        private readonly Color BFS_COLOR = Color.Yellow;
        private readonly Color DFS_COLOR = Color.Purple;

        /// <summary>
        /// إنشاء النافذة الرئيسية وتهيئة جميع العناصر
        /// </summary>
        public MainForm()
        {
            try
            {
                // تحديد حجم النافذة كما هو مطلوب
                this.Size = new Size(1200, 800);
                this.Text = "تعليم الرسوم البيانية - Graphs Teaching App";
                this.StartPosition = FormStartPosition.CenterScreen;

                // إنشاء العناصر المرئية أولاً
                InitializeCustomComponents();

                // تهيئة المتغيرات الأساسية بعد إنشاء العناصر
                currentGraph = new Graph();
                highlightedNodes = new List<Node>();
                highlightedEdges = new List<Edge>();
                animationTimer = new Timer();
                animationTimer.Interval = 1000;
                animationTimer.Tick += AnimationTimer_Tick;

                zoomFactor = 1.0f;
                isDrawing = false;
                firstNodeForEdge = null; // تهيئة العقدة الأولى لربط الوصلات

                // رسالة ترحيب في مربع النتائج
                AppendToResults("مرحباً بك في تطبيق تعليم الرسوم البيانية!");
                AppendToResults("يمكنك البدء بإنشاء رسم بياني جديد أو تحميل ملف موجود.");

                AppendToResults($"تم تهيئة النافذة بنجاح. عدد العناصر: {this.Controls.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء النافذة: {ex.Message}\n\nتفاصيل الخطأ: {ex.StackTrace}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// تهيئة العناصر المخصصة للواجهة
        /// </summary>
        private void InitializeCustomComponents()
        {
            // إنشاء لوحة الرسم الرئيسية
            drawingPanel = new Panel();
            drawingPanel.Size = new Size(800, 600);
            drawingPanel.Location = new Point(20, 60);
            drawingPanel.BackColor = Color.White;
            drawingPanel.BorderStyle = BorderStyle.FixedSingle;

            // إضافة أحداث الماوس للوحة الرسم
            drawingPanel.MouseDown += DrawingPanel_MouseDown;
            drawingPanel.MouseMove += DrawingPanel_MouseMove;
            drawingPanel.MouseUp += DrawingPanel_MouseUp;
            drawingPanel.MouseWheel += DrawingPanel_MouseWheel;
            drawingPanel.Paint += DrawingPanel_Paint;

            // إضافة اللوحة إلى النافذة
            this.Controls.Add(drawingPanel);

            // إنشاء شريط الأدوات العلوي
            CreateToolbar();

            // إنشاء مربع النتائج السفلي
            CreateResultsTextBox();

            // إنشاء مربعات الإدخال للعقد والمسارات
            CreateInputControls();
        }

        /// <summary>
        /// إنشاء شريط الأدوات مع جميع الأزرار
        /// </summary>
        private void CreateToolbar()
        {
            try
            {
                // إنشاء لوحة شريط الأدوات
                toolbarPanel = new Panel();
                toolbarPanel.Size = new Size(1150, 80);
                toolbarPanel.Location = new Point(10, 10);
                toolbarPanel.BackColor = Color.LightGray;

                // إنشاء الأزرار في صف واحد مع عرض مناسب
                int buttonWidth = 105;
                int buttonHeight = 30;
                int spacing = 5;
                int currentX = 10;
                int currentY = 5;

                // قائمة بجميع الأزرار مع معالجاتها
                var buttons = new (string text, EventHandler handler)[]
                {
                    ("تحميل ملف", BtnLoad_Click),
                    ("حفظ ملف", BtnSave_Click),
                    ("رسم البيان", BtnDraw_Click),
                    ("رسم المتمم", BtnComplement_Click),
                    ("الكود الثنائي", BtnBinary_Click),
                    ("إيجاد مسارات", BtnFindPaths_Click),
                    ("نوع البيان", BtnCheckType_Click),
                    ("متتالية الدرجات", BtnDegreeSequence_Click),
                    ("الجزئيات", BtnSubgraphs_Click),
                    ("تنفيذ DFS", BtnDFS_Click),
                    ("تنفيذ BFS", BtnBFS_Click),
                    ("جميع المسارات", BtnAllPaths_Click)
                };

                // إنشاء جميع الأزرار
                foreach (var (text, handler) in buttons)
                {
                    Button btn = new Button();
                    btn.Text = text;
                    btn.Size = new Size(buttonWidth, buttonHeight);
                    btn.Location = new Point(currentX, currentY);
                    btn.BackColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;

                    if (handler != null)
                        btn.Click += handler;

                    toolbarPanel.Controls.Add(btn);
                    currentX += buttonWidth + spacing;
                }

                // إضافة شريط الأدوات إلى النافذة
                this.Controls.Add(toolbarPanel);

                AppendToResults($"تم إنشاء شريط الأدوات مع {buttons.Length} زر");
            }
            catch (Exception ex)
            {
                AppendToResults($"خطأ في إنشاء شريط الأدوات: {ex.Message}");
            }
        }

        /// <summary>
        /// إنشاء زر جديد بالمواصفات المحددة
        /// </summary>
        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new Point(x, y);
            button.Size = new Size(width, height);
            button.BackColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            return button;
        }

        /// <summary>
        /// إنشاء مربع النتائج السفلي
        /// </summary>
        private void CreateResultsTextBox()
        {
            resultsTextBox = new RichTextBox();
            resultsTextBox.Size = new Size(1150, 100);
            resultsTextBox.Location = new Point(10, 680);
            resultsTextBox.BackColor = Color.Black;
            resultsTextBox.ForeColor = Color.Green;
            resultsTextBox.Font = new Font("Consolas", 10);
            resultsTextBox.ReadOnly = true;
            resultsTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;

            this.Controls.Add(resultsTextBox);
        }

        /// <summary>
        /// إنشاء مربعات الإدخال للعقد والمسارات
        /// </summary>
        private void CreateInputControls()
        {
            // تسمية العقدة المصدر
            Label lblSourceNode = new Label();
            lblSourceNode.Text = "العقدة المصدر:";
            lblSourceNode.Location = new Point(850, 60);
            lblSourceNode.Size = new Size(100, 20);
            this.Controls.Add(lblSourceNode);

            // قائمة العقد المصدر
            sourceNodeComboBox = new ComboBox();
            sourceNodeComboBox.Location = new Point(850, 85);
            sourceNodeComboBox.Size = new Size(100, 25);
            sourceNodeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(sourceNodeComboBox);

            // تسمية العقدة الهدف
            Label lblTargetNode = new Label();
            lblTargetNode.Text = "العقدة الهدف:";
            lblTargetNode.Location = new Point(970, 60);
            lblTargetNode.Size = new Size(100, 20);
            this.Controls.Add(lblTargetNode);

            // قائمة العقد الهدف
            targetNodeComboBox = new ComboBox();
            targetNodeComboBox.Location = new Point(970, 85);
            targetNodeComboBox.Size = new Size(100, 25);
            targetNodeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(targetNodeComboBox);

            // تسمية طول المسار
            Label lblPathLength = new Label();
            lblPathLength.Text = "طول المسار:";
            lblPathLength.Location = new Point(850, 120);
            lblPathLength.Size = new Size(100, 20);
            this.Controls.Add(lblPathLength);

            // مربع إدخال طول المسار
            pathLengthTextBox = new TextBox();
            pathLengthTextBox.Location = new Point(850, 145);
            pathLengthTextBox.Size = new Size(100, 25);
            pathLengthTextBox.Text = "3";
            this.Controls.Add(pathLengthTextBox);

            // تسمية اسم العقدة الجديدة
            Label lblNewNode = new Label();
            lblNewNode.Text = "اسم العقدة:";
            lblNewNode.Location = new Point(970, 120);
            lblNewNode.Size = new Size(100, 20);
            this.Controls.Add(lblNewNode);

            // مربع إدخال اسم العقدة الجديدة
            newNodeTextBox = new TextBox();
            newNodeTextBox.Location = new Point(970, 145);
            newNodeTextBox.Size = new Size(100, 25);
            newNodeTextBox.Text = "A";
            this.Controls.Add(newNodeTextBox);
        }

        /// <summary>
        /// إضافة نص إلى مربع النتائج مع الطابع الزمني
        /// </summary>
        private void AppendToResults(string message)
        {
            try
            {
                // التحقق من وجود مربع النتائج وتهيئته
                if (resultsTextBox == null)
                {
                    return; // خروج مبكر إذا لم يكن مربع النتائج جاهزاً بعد
                }

                if (resultsTextBox.InvokeRequired)
                {
                    resultsTextBox.Invoke(new Action<string>(AppendToResults), message);
                    return;
                }

                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                resultsTextBox.AppendText($"[{timestamp}] {message}\n");
                resultsTextBox.SelectionStart = resultsTextBox.Text.Length;
                resultsTextBox.ScrollToCaret();
            }
            catch (Exception ex)
            {
                // في حالة حدوث خطأ، نحاول طباعته في Console للتشخيص
                Console.WriteLine($"خطأ في AppendToResults: {ex.Message}");
            }
        }

        /// <summary>
        /// تحديث قوائم العقد في مربعات الاختيار
        /// </summary>
        private void UpdateNodeComboBoxes()
        {
            var nodeNames = currentGraph.GetNodeNames();

            sourceNodeComboBox.Items.Clear();
            targetNodeComboBox.Items.Clear();

            foreach (var nodeName in nodeNames)
            {
                sourceNodeComboBox.Items.Add(nodeName);
                targetNodeComboBox.Items.Add(nodeName);
            }

            if (nodeNames.Count > 0)
            {
                sourceNodeComboBox.SelectedIndex = 0;
                if (nodeNames.Count > 1)
                    targetNodeComboBox.SelectedIndex = 1;
                else
                    targetNodeComboBox.SelectedIndex = 0;
            }
        }

        // متغيرات العناصر المرئية (سيتم تعريفها في الطريقة الجزئية)
        private Panel drawingPanel;
        private Panel toolbarPanel;
        private RichTextBox resultsTextBox;
        private ComboBox sourceNodeComboBox;
        private ComboBox targetNodeComboBox;
        private TextBox pathLengthTextBox;
        private TextBox newNodeTextBox;

        #region رسم البيان والتفاعل مع الماوس

        /// <summary>
        /// رسم الرسم البياني على لوحة الرسم
        /// </summary>
        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // رسم جميع الوصلات أولاً
            foreach (var edge in currentGraph.Edges)
            {
                DrawEdge(g, edge);
            }

            // رسم جميع العقد
            foreach (var node in currentGraph.Nodes)
            {
                DrawNode(g, node);
            }

            // رسم العقد المضيئة مؤقتاً
            foreach (var node in highlightedNodes)
            {
                DrawNode(g, node, true);
            }

            // رسم الوصلات المضيئة مؤقتاً
            foreach (var edge in highlightedEdges)
            {
                DrawEdge(g, edge, true);
            }
        }

        /// <summary>
        /// رسم عقدة واحدة
        /// </summary>
        private void DrawNode(Graphics g, Node node, bool highlight = false)
        {
            // تطبيق عامل التكبير
            Point scaledPosition = ScalePoint(node.Position);

            int nodeRadius = (int)(25 * zoomFactor);
            Brush brush = new SolidBrush(highlight ? HIGHLIGHT_COLOR : node.Position == lastMousePosition ? HIGHLIGHT_COLOR : node.NodeColor);
            Pen pen = new Pen(Color.Black, 2);

            // رسم الدائرة الخارجية
            g.DrawEllipse(pen, scaledPosition.X - nodeRadius, scaledPosition.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);

            // رسم الدائرة الداخلية ملونة
            g.FillEllipse(brush, scaledPosition.X - nodeRadius + 2, scaledPosition.Y - nodeRadius + 2, nodeRadius * 2 - 4, nodeRadius * 2 - 4);

            // رسم اسم العقدة في المنتصف
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            Font font = new Font("Arial", (int)(10 * zoomFactor), FontStyle.Bold);
            g.DrawString(node.Label, font, Brushes.White,
                new RectangleF(scaledPosition.X - nodeRadius, scaledPosition.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2), format);

            pen.Dispose();
            brush.Dispose();
            font.Dispose();
        }

        /// <summary>
        /// رسم وصلة واحدة بين عقدتين
        /// </summary>
        private void DrawEdge(Graphics g, Edge edge, bool highlight = false)
        {
            // تطبيق عامل التكبير على مواقع العقد
            Point fromScaled = ScalePoint(edge.From.Position);
            Point toScaled = ScalePoint(edge.To.Position);

            Pen pen = new Pen(highlight ? HIGHLIGHT_COLOR : edge.EdgeColor, highlight ? 3 : 2);

            // رسم خط مستقيم بين العقدتين
            g.DrawLine(pen, fromScaled, toScaled);

            // رسم وزن الوصلة في المنتصف إذا كانت موجهة
            if (currentGraph.IsWeighted)
            {
                Point midPoint = new Point((fromScaled.X + toScaled.X) / 2, (fromScaled.Y + toScaled.Y) / 2);
                Font font = new Font("Arial", (int)(8 * zoomFactor));
                g.DrawString(edge.Weight.ToString(), font, Brushes.Red, midPoint);
                font.Dispose();
            }

            pen.Dispose();
        }

        /// <summary>
        /// تطبيق عامل التكبير على نقطة محددة
        /// </summary>
        private Point ScalePoint(Point point)
        {
            Point center = zoomCenter;
            int scaledX = center.X + (int)((point.X - center.X) * zoomFactor);
            int scaledY = center.Y + (int)((point.Y - center.Y) * zoomFactor);
            return new Point(scaledX, scaledY);
        }

        /// <summary>
        /// الحصول على العقدة في الموقع المحدد
        /// </summary>
        private Node GetNodeAtPosition(Point position)
        {
            foreach (var node in currentGraph.Nodes)
            {
                Point scaledPosition = ScalePoint(node.Position);
                int nodeRadius = (int)(25 * zoomFactor);
                int distance = (int)Math.Sqrt(Math.Pow(position.X - scaledPosition.X, 2) + Math.Pow(position.Y - scaledPosition.Y, 2));

                if (distance <= nodeRadius)
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// توليد مواقع عشوائية للعقد في شكل دائري
        /// </summary>
        private void ArrangeNodesInCircle()
        {
            if (currentGraph.Nodes.Count == 0) return;

            int centerX = drawingPanel.Width / 2;
            int centerY = drawingPanel.Height / 2;
            int radius = Math.Min(centerX, centerY) - 50;

            double angleStep = 2 * Math.PI / currentGraph.Nodes.Count;

            for (int i = 0; i < currentGraph.Nodes.Count; i++)
            {
                double angle = i * angleStep;
                int x = centerX + (int)(radius * Math.Cos(angle));
                int y = centerY + (int)(radius * Math.Sin(angle));

                currentGraph.Nodes[i].Position = new Point(x, y);
            }

            drawingPanel.Invalidate();
        }

        #endregion

        #region أحداث الماوس للتفاعل

        /// <summary>
        /// حدث ضغط زر الماوس على لوحة الرسم
        /// </summary>
        private void DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            Node clickedNode = GetNodeAtPosition(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                if (clickedNode != null)
                {
                    // إذا لم تكن هناك عقدة أولى محددة للربط
                    if (firstNodeForEdge == null)
                    {
                        firstNodeForEdge = clickedNode;
                        highlightedNodes.Add(clickedNode);
                        AppendToResults($"تم تحديد العقدة الأولى للربط: {clickedNode.Name}");
                        AppendToResults("انقر على عقدة ثانية لإنشاء وصلة، أو انقر مرة أخرى على نفس العقدة لإلغاء التحديد");
                    }
                    else
                    {
                        // إذا كانت هناك عقدة أولى محددة، أنشئ وصلة مع العقدة الثانية
                        if (clickedNode != firstNodeForEdge)
                        {
                            CreateEdgeBetweenNodes(firstNodeForEdge, clickedNode);
                        }

                        // إزالة التحديد من العقدة الأولى
                        highlightedNodes.Remove(firstNodeForEdge);
                        firstNodeForEdge = null;
                    }

                    drawingPanel.Invalidate();
                }
                else
                {
                    // إزالة تحديد العقدة الأولى إذا نقر المستخدم في مكان فارغ
                    if (firstNodeForEdge != null)
                    {
                        highlightedNodes.Remove(firstNodeForEdge);
                        firstNodeForEdge = null;
                        drawingPanel.Invalidate();
                        AppendToResults("تم إلغاء تحديد العقدة للربط");
                    }

                    // إضافة عقدة جديدة عند النقر المزدوج (Ctrl + Click)
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        AddNodeAtPosition(e.Location);
                    }
                }
            }
            else if (e.Button == MouseButtons.Right && clickedNode != null)
            {
                // حذف العقدة بالنقر الأيمن
                RemoveNode(clickedNode.Name);
            }
        }

        /// <summary>
        /// حدث حركة الماوس على لوحة الرسم
        /// </summary>
        private void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && selectedNode != null)
            {
                // سحب العقدة لتغيير موقعها
                int deltaX = e.X - lastMousePosition.X;
                int deltaY = e.Y - lastMousePosition.Y;

                selectedNode.Position = new Point(
                    selectedNode.Position.X + (int)(deltaX / zoomFactor),
                    selectedNode.Position.Y + (int)(deltaY / zoomFactor)
                );

                lastMousePosition = e.Location;
                drawingPanel.Invalidate();
            }
        }

        /// <summary>
        /// حدث رفع زر الماوس من لوحة الرسم
        /// </summary>
        private void DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            selectedNode = null;
        }

        /// <summary>
        /// إعادة تعيين حالة ربط العقد
        /// </summary>
        private void ResetEdgeCreation()
        {
            if (firstNodeForEdge != null)
            {
                highlightedNodes.Remove(firstNodeForEdge);
                firstNodeForEdge = null;
                drawingPanel.Invalidate();
            }
        }

        /// <summary>
        /// حدث تدوير عجلة الماوس للتكبير والتصغير
        /// </summary>
        private void DrawingPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoomFactor;

            if (e.Delta > 0)
                zoomFactor = Math.Min(zoomFactor * 1.2f, 5.0f); // تكبير
            else
                zoomFactor = Math.Max(zoomFactor / 1.2f, 0.1f); // تصغير

            if (oldZoom != zoomFactor)
            {
                zoomCenter = e.Location;
                drawingPanel.Invalidate();
                AppendToResults($"عامل التكبير: {zoomFactor:F2}");
            }
        }

        #endregion

        #region وظائف إضافة وحذف العقد والوصلات

        /// <summary>
        /// إضافة عقدة جديدة في الموقع المحدد
        /// </summary>
        private void AddNodeAtPosition(Point position)
        {
            string nodeName = newNodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(nodeName))
            {
                AppendToResults("خطأ: يرجى إدخال اسم العقدة في مربع النص");
                return;
            }

            // التحقق من عدم وجود عقدة بنفس الاسم
            if (currentGraph.Nodes.Any(n => n.Name == nodeName))
            {
                AppendToResults($"خطأ: العقدة '{nodeName}' موجودة مسبقاً");
                return;
            }

            // تحويل الموقع بناءً على عامل التكبير
            Point actualPosition = new Point(
                zoomCenter.X + (int)((position.X - zoomCenter.X) / zoomFactor),
                zoomCenter.Y + (int)((position.Y - zoomCenter.Y) / zoomFactor)
            );

            Node newNode = new Node(nodeName, actualPosition);
            currentGraph.AddNode(newNode);

            UpdateNodeComboBoxes();
            drawingPanel.Invalidate();

            AppendToResults($"تم إضافة العقدة: {nodeName}");
        }

        /// <summary>
        /// حذف العقدة المحددة
        /// </summary>
        private void RemoveNode(string nodeName)
        {
            if (currentGraph.Nodes.Any(n => n.Name == nodeName))
            {
                currentGraph.RemoveNode(nodeName);
                UpdateNodeComboBoxes();
                drawingPanel.Invalidate();

                AppendToResults($"تم حذف العقدة: {nodeName}");
            }
        }

        /// <summary>
        /// إنشاء وصلة بين عقدتين
        /// </summary>
        private void CreateEdgeBetweenNodes(Node node1, Node node2)
        {
            // التحقق من عدم وجود وصلة مسبقة بين نفس العقدتين
            bool edgeExists = currentGraph.Edges.Any(e =>
                (e.From.Name == node1.Name && e.To.Name == node2.Name) ||
                (!currentGraph.IsDirected && e.From.Name == node2.Name && e.To.Name == node1.Name));

            if (edgeExists)
            {
                AppendToResults($"الوصلة موجودة مسبقاً بين {node1.Name} و {node2.Name}");
                return;
            }

            // إضافة الوصلة للرسم البياني
            currentGraph.AddEdge(node1.Name, node2.Name);

            drawingPanel.Invalidate();
            AppendToResults($"تم إنشاء وصلة بين {node1.Name} و {node2.Name}");
        }

        #endregion

        #region معالجات أحداث الأزرار

        /// <summary>
        /// معالج حدث زر تحميل الملف
        /// </summary>
        private void BtnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "ملفات النص|*.txt|ملفات JSON|*.json|جميع الملفات|*.*";
                openFileDialog.Title = "اختر ملف الرسم البياني";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);

                    if (openFileDialog.FileName.EndsWith(".json"))
                        LoadFromJson(fileContent);
                    else
                        LoadFromText(fileContent);

                    UpdateNodeComboBoxes();
                    ArrangeNodesInCircle();
                    AppendToResults($"تم تحميل الملف: {openFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                AppendToResults($"خطأ في تحميل الملف: {ex.Message}");
            }
        }

        /// <summary>
        /// معالج حدث زر حفظ الملف
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "ملفات النص|*.txt|ملفات JSON|*.json";
                saveFileDialog.Title = "حفظ ملف الرسم البياني";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string content = "";
                    if (saveFileDialog.FileName.EndsWith(".json"))
                        content = SaveToJson();
                    else
                        content = SaveToText();

                    File.WriteAllText(saveFileDialog.FileName, content);
                    AppendToResults($"تم حفظ الملف: {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                AppendToResults($"خطأ في حفظ الملف: {ex.Message}");
            }
        }

        /// <summary>
        /// معالج حدث زر رسم البيان
        /// </summary>
        private void BtnDraw_Click(object sender, EventArgs e)
        {
            if (currentGraph.Nodes.Count == 0)
            {
                AppendToResults("لا يوجد عقد في الرسم البياني لرسمها");
                return;
            }

            ArrangeNodesInCircle();
            drawingPanel.Invalidate();
            AppendToResults($"تم رسم الرسم البياني مع {currentGraph.Nodes.Count} عقدة و {currentGraph.Edges.Count} وصلة");
        }

        /// <summary>
        /// معالج حدث زر رسم المتمم
        /// </summary>
        private void BtnComplement_Click(object sender, EventArgs e)
        {
            GenerateComplementGraph();
            AppendToResults("تم رسم المتمم للرسم البياني");
        }

        /// <summary>
        /// معالج حدث زر توليد الكود الثنائي
        /// </summary>
        private void BtnBinary_Click(object sender, EventArgs e)
        {
            string binaryCode = GenerateBinaryCode();
            AppendToResults($"الكود الثنائي: {binaryCode}");
        }

        /// <summary>
        /// معالج حدث زر إيجاد المسارات
        /// </summary>
        private void BtnFindPaths_Click(object sender, EventArgs e)
        {
            string sourceNode = sourceNodeComboBox.SelectedItem?.ToString();
            string targetNode = targetNodeComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(sourceNode) || string.IsNullOrEmpty(targetNode))
            {
                AppendToResults("يرجى اختيار العقدتين المصدر والهدف");
                return;
            }

            if (!int.TryParse(pathLengthTextBox.Text, out int pathLength) || pathLength <= 0)
            {
                AppendToResults("يرجى إدخال طول المسار صحيح");
                return;
            }

            var paths = FindPaths(sourceNode, targetNode, pathLength);
            AppendToResults($"تم العثور على {paths.Count} مسار بطول {pathLength} من {sourceNode} إلى {targetNode}");
        }

        /// <summary>
        /// معالج حدث زر التحقق من نوع البيان
        /// </summary>
        private void BtnCheckType_Click(object sender, EventArgs e)
        {
            CheckGraphType();
        }

        /// <summary>
        /// معالج حدث زر تحقق متتالية الدرجات
        /// </summary>
        private void BtnDegreeSequence_Click(object sender, EventArgs e)
        {
            CheckDegreeSequence();
        }

        /// <summary>
        /// معالج حدث زر رسم جميع الجزئيات
        /// </summary>
        private void BtnSubgraphs_Click(object sender, EventArgs e)
        {
            GenerateSubgraphs();
        }

        /// <summary>
        /// معالج حدث زر تنفيذ DFS
        /// </summary>
        private void BtnDFS_Click(object sender, EventArgs e)
        {
            string startNode = sourceNodeComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(startNode))
            {
                AppendToResults("يرجى اختيار العقدة المصدر لـ DFS");
                return;
            }

            ExecuteDFS(startNode);
        }

        /// <summary>
        /// معالج حدث زر تنفيذ BFS
        /// </summary>
        private void BtnBFS_Click(object sender, EventArgs e)
        {
            string startNode = sourceNodeComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(startNode))
            {
                AppendToResults("يرجى اختيار العقدة المصدر لـ BFS");
                return;
            }

            ExecuteBFS(startNode);
        }

        /// <summary>
        /// معالج حدث زر توليد جميع المسارات
        /// </summary>
        private void BtnAllPaths_Click(object sender, EventArgs e)
        {
            string startNode = sourceNodeComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(startNode))
            {
                AppendToResults("يرجى اختيار العقدة المصدر");
                return;
            }

            GenerateAllPaths(startNode);
        }

        #endregion

        #region وظائف الخوارزميات والعمليات الأساسية

        /// <summary>
        /// تحميل البيانات من ملف نصي
        /// تنسيق الملف: كل سطر يمثل عقدة مع جيرانها
        /// مثال: A: B C D
        /// </summary>
        private void LoadFromText(string content)
        {
            currentGraph = new Graph();
            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#")) continue;

                string[] parts = trimmedLine.Split(':');
                if (parts.Length >= 2)
                {
                    string nodeName = parts[0].Trim();
                    string[] neighbors = parts[1].Trim().Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                    // إضافة العقدة إذا لم تكن موجودة
                    if (!currentGraph.Nodes.Any(n => n.Name == nodeName))
                    {
                        currentGraph.AddNode(new Node(nodeName, new Point(0, 0)));
                    }

                    // إضافة الجيران
                    foreach (string neighbor in neighbors)
                    {
                        string neighborName = neighbor.Trim();
                        if (!currentGraph.Nodes.Any(n => n.Name == neighborName))
                        {
                            currentGraph.AddNode(new Node(neighborName, new Point(0, 0)));
                        }
                        currentGraph.AddEdge(nodeName, neighborName);
                    }
                }
            }
        }

        /// <summary>
        /// تحميل البيانات من ملف JSON
        /// تنسيق الملف: {"nodes": ["A", "B", "C"], "edges": [["A", "B"], ["B", "C"]]}
        /// </summary>
        private void LoadFromJson(string content)
        {
            try
            {
                // هنا سنستخدم تحليل JSON بسيط بدلاً من مكتبة خارجية
                currentGraph = new Graph();

                // تحليل مبسط للـ JSON (يمكن تحسينه لاحقاً)
                if (content.Contains("\"nodes\""))
                {
                    // استخراج أسماء العقد
                    int nodesStart = content.IndexOf("\"nodes\"") + 8;
                    int nodesEnd = content.IndexOf("]", nodesStart) + 1;
                    string nodesPart = content.Substring(nodesStart, nodesEnd - nodesStart);

                    // تحليل أسماء العقد البسيط
                    string[] nodeNames = nodesPart.Replace("[", "").Replace("]", "").Replace("\"", "").Split(',');
                    foreach (string nodeName in nodeNames)
                    {
                        string trimmed = nodeName.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            currentGraph.AddNode(new Node(trimmed, new Point(0, 0)));
                        }
                    }
                }

                // استخراج الوصلات
                if (content.Contains("\"edges\""))
                {
                    int edgesStart = content.IndexOf("\"edges\"") + 8;
                    int edgesEnd = content.IndexOf("]", edgesStart) + 1;
                    string edgesPart = content.Substring(edgesStart, edgesEnd - edgesStart);

                    // تحليل الوصلات البسيط
                    // هذا تحليل مبسط - يمكن تحسينه لاحقاً
                }
            }
            catch (Exception ex)
            {
                AppendToResults($"خطأ في تحليل JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// حفظ البيانات إلى ملف نصي
        /// </summary>
        private string SaveToText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# ملف الرسم البياني - تم إنشاؤه بواسطة تطبيق تعليم الرسوم البيانية");
            sb.AppendLine($"# عدد العقد: {currentGraph.Nodes.Count}");
            sb.AppendLine($"# عدد الوصلات: {currentGraph.Edges.Count}");
            sb.AppendLine();

            foreach (var node in currentGraph.Nodes)
            {
                sb.Append(node.Name + ": ");
                var neighbors = currentGraph.AdjacencyList[node.Name];
                if (neighbors != null && neighbors.Count > 0)
                {
                    sb.AppendLine(string.Join(" ", neighbors));
                }
                else
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// حفظ البيانات إلى ملف JSON
        /// </summary>
        private string SaveToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");

            // حفظ العقد
            sb.AppendLine("  \"nodes\": [");
            for (int i = 0; i < currentGraph.Nodes.Count; i++)
            {
                sb.Append($"    \"{currentGraph.Nodes[i].Name}\"");
                if (i < currentGraph.Nodes.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("  ],");

            // حفظ الوصلات
            sb.AppendLine("  \"edges\": [");
            var uniqueEdges = new HashSet<string>();
            foreach (var edge in currentGraph.Edges)
            {
                string edgeKey = $"{edge.From.Name},{edge.To.Name}";
                if (!uniqueEdges.Contains(edgeKey))
                {
                    uniqueEdges.Add(edgeKey);
                    sb.AppendLine($"    [\"{edge.From.Name}\", \"{edge.To.Name}\"]");
                }
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// توليد الرسم البياني المتمم
        /// </summary>
        private void GenerateComplementGraph()
        {
            if (currentGraph.Nodes.Count < 2)
            {
                AppendToResults("يجب وجود عقدتين على الأقل لإنشاء المتمم");
                return;
            }

            // إنشاء رسم بياني جديد للمتمم
            Graph complementGraph = new Graph();

            // نسخ جميع العقد
            foreach (var node in currentGraph.Nodes)
            {
                complementGraph.AddNode(new Node(node.Name, node.Position));
            }

            // إضافة الوصلات المفقودة (المتمم)
            for (int i = 0; i < currentGraph.Nodes.Count; i++)
            {
                for (int j = 0; j < currentGraph.Nodes.Count; j++)
                {
                    if (i != j)
                    {
                        string node1 = currentGraph.Nodes[i].Name;
                        string node2 = currentGraph.Nodes[j].Name;

                        // التحقق من عدم وجود وصلة في الرسم الأصلي
                        bool edgeExists = currentGraph.Edges.Any(e =>
                            (e.From.Name == node1 && e.To.Name == node2) ||
                            (!currentGraph.IsDirected && e.From.Name == node2 && e.To.Name == node1));

                        if (!edgeExists)
                        {
                            complementGraph.AddEdge(node1, node2);
                        }
                    }
                }
            }

            // رسم المتمم باللون الأحمر
            currentGraph = complementGraph;
            foreach (var node in currentGraph.Nodes)
            {
                node.NodeColor = COMPLEMENT_COLOR;
            }

            UpdateNodeComboBoxes();
            drawingPanel.Invalidate();
        }

        /// <summary>
        /// توليد الكود الثنائي للرسم البياني
        /// </summary>
        private string GenerateBinaryCode()
        {
            if (currentGraph.Nodes.Count == 0)
                return "لا يوجد عقد في الرسم البياني";

            StringBuilder binary = new StringBuilder();

            // إنشاء مصفوفة التجاور الثنائية
            for (int i = 0; i < currentGraph.Nodes.Count; i++)
            {
                for (int j = 0; j < currentGraph.Nodes.Count; j++)
                {
                    if (i != j)
                    {
                        bool hasEdge = currentGraph.Edges.Any(e =>
                            (e.From.Name == currentGraph.Nodes[i].Name && e.To.Name == currentGraph.Nodes[j].Name) ||
                            (!currentGraph.IsDirected && e.From.Name == currentGraph.Nodes[j].Name && e.To.Name == currentGraph.Nodes[i].Name));

                        binary.Append(hasEdge ? "1" : "0");
                    }
                    else
                    {
                        binary.Append("0"); // لا توجد وصلات ذاتية
                    }
                }
                binary.Append(" ");
            }

            return binary.ToString().Trim();
        }

        /// <summary>
        /// إيجاد جميع المسارات بين عقدتين بطول محدد
        /// </summary>
        private List<List<string>> FindPaths(string start, string end, int maxLength)
        {
            var paths = new List<List<string>>();
            var currentPath = new List<string>();
            var visited = new HashSet<string>();

            FindPathsRecursive(start, end, maxLength, currentPath, visited, paths);

            return paths;
        }

        /// <summary>
        /// الدالة التكرارية لإيجاد المسارات
        /// </summary>
        private void FindPathsRecursive(string current, string end, int remainingLength,
            List<string> currentPath, HashSet<string> visited, List<List<string>> allPaths)
        {
            currentPath.Add(current);
            visited.Add(current);

            if (current == end && currentPath.Count - 1 <= remainingLength)
            {
                allPaths.Add(new List<string>(currentPath));
            }
            else if (currentPath.Count - 1 < remainingLength)
            {
                if (currentGraph.AdjacencyList.ContainsKey(current))
                {
                    foreach (string neighbor in currentGraph.AdjacencyList[current])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            FindPathsRecursive(neighbor, end, remainingLength, currentPath, visited, allPaths);
                        }
                    }
                }
            }

            currentPath.RemoveAt(currentPath.Count - 1);
            visited.Remove(current);
        }

        /// <summary>
        /// التحقق من نوع الرسم البياني (تام، ثنائي التجزئة، منتظم)
        /// </summary>
        private void CheckGraphType()
        {
            if (currentGraph.Nodes.Count == 0)
            {
                AppendToResults("لا يوجد رسم بياني لفحصه");
                return;
            }

            StringBuilder result = new StringBuilder();
            result.AppendLine("نتائج فحص نوع الرسم البياني:");

            // فحص الرسم التام
            bool isComplete = true;
            for (int i = 0; i < currentGraph.Nodes.Count && isComplete; i++)
            {
                for (int j = i + 1; j < currentGraph.Nodes.Count && isComplete; j++)
                {
                    string node1 = currentGraph.Nodes[i].Name;
                    string node2 = currentGraph.Nodes[j].Name;

                    bool hasEdge = currentGraph.Edges.Any(e =>
                        (e.From.Name == node1 && e.To.Name == node2) ||
                        (e.From.Name == node2 && e.To.Name == node1));

                    if (!hasEdge)
                    {
                        isComplete = false;
                        break;
                    }
                }
            }

            result.AppendLine($"الرسم التام: {(isComplete ? "نعم" : "لا")}");

            // فحص الرسم المنتظم
            int firstDegree = currentGraph.GetNodeDegree(currentGraph.Nodes[0].Name);
            bool isRegular = true;

            foreach (var node in currentGraph.Nodes)
            {
                if (currentGraph.GetNodeDegree(node.Name) != firstDegree)
                {
                    isRegular = false;
                    break;
                }
            }

            result.AppendLine($"الرسم المنتظم: {(isRegular ? "نعم" : "لا")}");
            if (isRegular) result.AppendLine($"درجة الانتظام: {firstDegree}");

            // فحص الرسم ثنائي التجزئة (مبسط)
            bool isBipartite = IsBipartite();
            result.AppendLine($"الرسم ثنائي التجزئة: {(isBipartite ? "نعم" : "لا")}");

            AppendToResults(result.ToString());
        }

        /// <summary>
        /// فحص ما إذا كان الرسم ثنائي التجزئة باستخدام BFS
        /// </summary>
        private bool IsBipartite()
        {
            if (currentGraph.Nodes.Count == 0) return true;

            var colors = new Dictionary<string, int>();
            var queue = new Queue<string>();

            // تلوين العقدة الأولى باللون 0
            string startNode = currentGraph.Nodes[0].Name;
            colors[startNode] = 0;
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                if (currentGraph.AdjacencyList.ContainsKey(current))
                {
                    foreach (string neighbor in currentGraph.AdjacencyList[current])
                    {
                        if (!colors.ContainsKey(neighbor))
                        {
                            // تلوين الجار باللون المعاكس
                            colors[neighbor] = 1 - colors[current];
                            queue.Enqueue(neighbor);
                        }
                        else if (colors[neighbor] == colors[current])
                        {
                            // نفس اللون - ليس ثنائي التجزئة
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// التحقق من متتالية الدرجات باستخدام مبرهنة Havel-Hakimi
        /// </summary>
        private void CheckDegreeSequence()
        {
            var degreeSequence = currentGraph.GetDegreeSequence();

            if (degreeSequence.Count == 0)
            {
                AppendToResults("لا يوجد رسم بياني لفحص متتالية درجاته");
                return;
            }

            AppendToResults("فحص متتالية الدرجات:");
            AppendToResults($"المتتالية الحالية: {string.Join(", ", degreeSequence)}");

            var sequenceCopy = new List<int>(degreeSequence);
            bool isGraphical = HavelHakimiAlgorithm(sequenceCopy);

            AppendToResults($"هل المتتالية بيانية؟ {(isGraphical ? "نعم" : "لا")}");
        }

        /// <summary>
        /// تطبيق خوارزمية Havel-Hakimi خطوة بخطوة
        /// </summary>
        private bool HavelHakimiAlgorithm(List<int> sequence)
        {
            AppendToResults("تطبيق خوارزمية Havel-Hakimi:");

            while (sequence.Count > 0)
            {
                // ترتيب المتتالية تنازلياً
                sequence.Sort((a, b) => b.CompareTo(a));

                AppendToResults($"المتتالية الحالية: {string.Join(", ", sequence)}");

                // إذا كانت جميع القيم صفر أو سالبة
                if (sequence.All(d => d <= 0))
                {
                    AppendToResults("جميع القيم صفر أو أقل - انتهت الخوارزمية بنجاح");
                    return true;
                }

                // أخذ أكبر قيمة
                int first = sequence[0];
                if (first >= sequence.Count)
                {
                    AppendToResults($"القيمة {first} أكبر من أو تساوي طول المتتالية - فشلت الخوارزمية");
                    return false;
                }

                // طرح 1 من أول first قيم
                sequence.RemoveAt(0);
                for (int i = 0; i < first && i < sequence.Count; i++)
                {
                    sequence[i]--;
                    if (sequence[i] < 0)
                    {
                        AppendToResults($"قيمة سالبة في المتتالية - فشلت الخوارزمية");
                        return false;
                    }
                }
            }

            AppendToResults("انتهت الخوارزمية بنجاح - المتتالية بيانية");
            return true;
        }

        /// <summary>
        /// توليد جميع الجزئيات الممكنة
        /// </summary>
        private void GenerateSubgraphs()
        {
            if (currentGraph.Nodes.Count > 7)
            {
                AppendToResults("عدد العقد كبير جداً لتوليد جميع الجزئيات. الحد الأقصى 7 عقد.");
                return;
            }

            AppendToResults($"توليد جميع الجزئيات لـ {currentGraph.Nodes.Count} عقدة:");

            // توليد جميع المجموعات الفرعية للعقد
            int totalSubgraphs = (int)Math.Pow(2, currentGraph.Nodes.Count);

            for (int subset = 1; subset < totalSubgraphs; subset++)
            {
                var subgraphNodes = new List<Node>();
                var subgraphEdges = new List<Edge>();

                // تحديد العقد الموجودة في هذه المجموعة الفرعية
                for (int i = 0; i < currentGraph.Nodes.Count; i++)
                {
                    if ((subset & (1 << i)) != 0)
                    {
                        subgraphNodes.Add(currentGraph.Nodes[i]);
                    }
                }

                // إضافة الوصلات الموجودة بين عقد المجموعة الفرعية
                foreach (var edge in currentGraph.Edges)
                {
                    if (subgraphNodes.Contains(edge.From) && subgraphNodes.Contains(edge.To))
                    {
                        subgraphEdges.Add(edge);
                    }
                }

                AppendToResults($"المجموعة الفرعية {subset}: عقد = {subgraphNodes.Count}, وصلات = {subgraphEdges.Count}");
            }
        }

        /// <summary>
        /// تنفيذ خوارزمية DFS مع الرسم التدريجي
        /// </summary>
        private void ExecuteDFS(string startNode)
        {
            var visited = new HashSet<string>();
            var dfsOrder = new List<string>();

            AppendToResults($"بدء DFS من العقدة: {startNode}");

            DFSRecursive(startNode, visited, dfsOrder);

            AppendToResults($"ترتيب الزيارة في DFS: {string.Join(" -> ", dfsOrder)}");
        }

        /// <summary>
        /// الدالة التكرارية لـ DFS
        /// </summary>
        private void DFSRecursive(string current, HashSet<string> visited, List<string> order)
        {
            visited.Add(current);
            order.Add(current);

            // تلوين العقدة باللون الأزرق أثناء الزيارة
            var node = currentGraph.Nodes.FirstOrDefault(n => n.Name == current);
            if (node != null)
            {
                node.NodeColor = DFS_COLOR;
                drawingPanel.Invalidate();
                System.Threading.Thread.Sleep(1000); // تأخير للتوضيح
            }

            if (currentGraph.AdjacencyList.ContainsKey(current))
            {
                foreach (string neighbor in currentGraph.AdjacencyList[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        DFSRecursive(neighbor, visited, order);
                    }
                }
            }

            // تلوين العقدة باللون الأخضر بعد الانتهاء
            if (node != null)
            {
                node.NodeColor = VISITED_COLOR;
                drawingPanel.Invalidate();
                System.Threading.Thread.Sleep(500);
            }
        }

        /// <summary>
        /// تنفيذ خوارزمية BFS مع الرسم التدريجي
        /// </summary>
        private void ExecuteBFS(string startNode)
        {
            var visited = new HashSet<string>();
            var bfsOrder = new List<string>();
            var queue = new Queue<string>();

            AppendToResults($"بدء BFS من العقدة: {startNode}");

            visited.Add(startNode);
            queue.Enqueue(startNode);
            bfsOrder.Add(startNode);

            // تلوين العقدة الأولى
            var startNodeObj = currentGraph.Nodes.FirstOrDefault(n => n.Name == startNode);
            if (startNodeObj != null)
            {
                startNodeObj.NodeColor = BFS_COLOR;
                drawingPanel.Invalidate();
                System.Threading.Thread.Sleep(1000);
            }

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                if (currentGraph.AdjacencyList.ContainsKey(current))
                {
                    foreach (string neighbor in currentGraph.AdjacencyList[current])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                            bfsOrder.Add(neighbor);

                            // تلوين العقدة الجديدة
                            var neighborNode = currentGraph.Nodes.FirstOrDefault(n => n.Name == neighbor);
                            if (neighborNode != null)
                            {
                                neighborNode.NodeColor = BFS_COLOR;
                                drawingPanel.Invalidate();
                                System.Threading.Thread.Sleep(1000);
                            }
                        }
                    }
                }

                // تلوين العقدة باللون الأخضر بعد الانتهاء من معالجتها
                var currentNode = currentGraph.Nodes.FirstOrDefault(n => n.Name == current);
                if (currentNode != null)
                {
                    currentNode.NodeColor = VISITED_COLOR;
                    drawingPanel.Invalidate();
                    System.Threading.Thread.Sleep(500);
                }
            }

            AppendToResults($"ترتيب الزيارة في BFS: {string.Join(" -> ", bfsOrder)}");
        }

        /// <summary>
        /// توليد جميع المسارات من عقدة محددة
        /// </summary>
        private void GenerateAllPaths(string startNode)
        {
            var allPaths = new List<List<string>>();
            var currentPath = new List<string>();
            var visited = new HashSet<string>();

            AppendToResults($"توليد جميع المسارات من العقدة: {startNode}");

            GenerateAllPathsRecursive(startNode, currentPath, visited, allPaths);

            AppendToResults($"تم العثور على {allPaths.Count} مسار:");
            for (int i = 0; i < allPaths.Count; i++)
            {
                AppendToResults($"المسار {i + 1}: {string.Join(" -> ", allPaths[i])}");
            }
        }

        /// <summary>
        /// الدالة التكرارية لتوليد جميع المسارات
        /// </summary>
        private void GenerateAllPathsRecursive(string current, List<string> currentPath,
            HashSet<string> visited, List<List<string>> allPaths)
        {
            currentPath.Add(current);
            visited.Add(current);

            // إضافة المسار الحالي إذا كان يحتوي على أكثر من عقدة واحدة
            if (currentPath.Count > 1)
            {
                allPaths.Add(new List<string>(currentPath));
            }

            if (currentGraph.AdjacencyList.ContainsKey(current))
            {
                foreach (string neighbor in currentGraph.AdjacencyList[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        GenerateAllPathsRecursive(neighbor, currentPath, visited, allPaths);
                    }
                }
            }

            currentPath.RemoveAt(currentPath.Count - 1);
            visited.Remove(current);
        }

        #endregion

        #region معالج المؤقت للرسوم المتحركة

        /// <summary>
        /// معالج حدث المؤقت للرسوم المتحركة
        /// </summary>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // إعادة تعيين الألوان إلى الحالة الطبيعية
            foreach (var node in currentGraph.Nodes)
            {
                node.NodeColor = NORMAL_COLOR;
            }

            highlightedNodes.Clear();
            highlightedEdges.Clear();

            drawingPanel.Invalidate();
            animationTimer.Stop();
        }

        #endregion

        /// <summary>
        /// طريقة جزئية مطلوبة لتصميم النموذج
        /// </summary>
        private void InitializeComponent()
        {
            // هذه الطريقة مطلوبة لتصميم Windows Forms
            // سنقوم بتعريف جميع العناصر يدوياً في InitializeCustomComponents
            SuspendLayout();
            ResumeLayout(false);
        }
    }
}