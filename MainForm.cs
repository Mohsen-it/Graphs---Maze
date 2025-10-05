using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.Json;

/// <summary>
/// نافذة منبثقة جميلة لعرض نتائج الخوارزميات والعمليات
/// </summary>
public class ResultsWindow : Form
{
    private RichTextBox resultsTextBox;
    private Panel titlePanel;
    private Label titleLabel;
    private Button btnClear, btnCopy, btnMinimize, btnClose;
    private Timer fadeTimer;
    private bool isMinimized = false;

    public ResultsWindow()
    {
        InitializeResultsWindow();
    }

    private void InitializeResultsWindow()
    {
        // إعدادات النافذة الأساسية
        this.Size = new Size(700, 500);
        this.MinimumSize = new Size(500, 300);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(45, 45, 48);
        this.ForeColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.ShowInTaskbar = false;
        this.TopMost = true;

        // إنشاء لوحة العنوان الجميلة
        CreateTitlePanel();

        // إنشاء مربع النتائج المحسن
        CreateResultsTextBox();

        // إنشاء لوحة الأزرار
        CreateButtonPanel();

        // إعدادات إضافية للنافذة
        this.Text = "نتائج العمليات - Graphs Teaching App";
        this.Icon = null; // يمكن إضافة أيقونة لاحقاً

        // إضافة تأثير التلاشي التدريجي
        fadeTimer = new Timer();
        fadeTimer.Interval = 16; // ~60 FPS
        fadeTimer.Tick += FadeTimer_Tick;

        // إضافة معالج تغيير الحجم
        this.Resize += ResultsWindow_Resize;
    }

    private void CreateTitlePanel()
    {
        titlePanel = new Panel();
        titlePanel.Size = new Size(this.Width, 50);
        titlePanel.Location = new Point(0, 0);
        titlePanel.BackColor = Color.FromArgb(33, 150, 243);

        titleLabel = new Label();
        titleLabel.Text = "📊 نتائج العمليات والخوارزميات";
        titleLabel.ForeColor = Color.White;
        titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        titleLabel.Location = new Point(15, 12);
        titleLabel.AutoSize = true;

        titlePanel.Controls.Add(titleLabel);
        this.Controls.Add(titlePanel);
    }

    private void CreateResultsTextBox()
    {
        resultsTextBox = new RichTextBox();
        resultsTextBox.Size = new Size(this.Width - 20, this.Height - 120);
        resultsTextBox.Location = new Point(10, 60);
        resultsTextBox.BackColor = Color.FromArgb(30, 30, 30);
        resultsTextBox.ForeColor = Color.FromArgb(0, 255, 0);
        resultsTextBox.Font = new Font("Consolas", 10, FontStyle.Regular);
        resultsTextBox.ReadOnly = true;
        resultsTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        resultsTextBox.BorderStyle = BorderStyle.None;

        // إضافة تأثير الحدود الداخلية
        resultsTextBox.Padding = new Padding(10);

        this.Controls.Add(resultsTextBox);
    }

    private void CreateButtonPanel()
    {
        Panel buttonPanel = new Panel();
        buttonPanel.Size = new Size(this.Width, 50);
        buttonPanel.Location = new Point(0, this.Height - 60);
        buttonPanel.BackColor = Color.FromArgb(55, 55, 58);

        // زر مسح النتائج
        btnClear = new Button();
        btnClear.Text = "🗑️ مسح";
        btnClear.Size = new Size(80, 35);
        btnClear.Location = new Point(10, 7);
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.BackColor = Color.FromArgb(244, 67, 54);
        btnClear.ForeColor = Color.White;
        btnClear.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnClear.Click += BtnClear_Click;

        // زر نسخ النتائج
        btnCopy = new Button();
        btnCopy.Text = "📋 نسخ";
        btnCopy.Size = new Size(80, 35);
        btnCopy.Location = new Point(100, 7);
        btnCopy.FlatStyle = FlatStyle.Flat;
        btnCopy.BackColor = Color.FromArgb(76, 175, 80);
        btnCopy.ForeColor = Color.White;
        btnCopy.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnCopy.Click += BtnCopy_Click;

        // زر تصغير/تكبير
        btnMinimize = new Button();
        btnMinimize.Text = "📏 تصغير";
        btnMinimize.Size = new Size(90, 35);
        btnMinimize.Location = new Point(190, 7);
        btnMinimize.FlatStyle = FlatStyle.Flat;
        btnMinimize.BackColor = Color.FromArgb(255, 193, 7);
        btnMinimize.ForeColor = Color.White;
        btnMinimize.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnMinimize.Click += BtnMinimize_Click;

        // زر إغلاق
        btnClose = new Button();
        btnClose.Text = "❌ إغلاق";
        btnClose.Size = new Size(80, 35);
        btnClose.Location = new Point(this.Width - 90, 7);
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.BackColor = Color.FromArgb(158, 158, 158);
        btnClose.ForeColor = Color.White;
        btnClose.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnClose.Click += BtnClose_Click;

        buttonPanel.Controls.AddRange(new Control[] { btnClear, btnCopy, btnMinimize, btnClose });
        this.Controls.Add(buttonPanel);
    }

    private void BtnClear_Click(object sender, EventArgs e)
    {
        resultsTextBox.Clear();
        AddTimestampMessage("تم مسح جميع النتائج");
    }

    private void BtnCopy_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(resultsTextBox.Text))
        {
            Clipboard.SetText(resultsTextBox.Text);
            ShowToastMessage("تم نسخ النتائج إلى الحافظة");
        }
    }

    private void BtnMinimize_Click(object sender, EventArgs e)
    {
        if (!isMinimized)
        {
            this.Size = new Size(500, 200);
            btnMinimize.Text = "📏 تكبير";
            isMinimized = true;
        }
        else
        {
            this.Size = new Size(700, 500);
            btnMinimize.Text = "📏 تصغير";
            isMinimized = false;
        }
    }

    private void BtnClose_Click(object sender, EventArgs e)
    {
        this.Hide();
    }

    private void ResultsWindow_Resize(object sender, EventArgs e)
    {
        if (titlePanel != null) titlePanel.Width = this.Width;
        if (resultsTextBox != null)
        {
            resultsTextBox.Width = this.Width - 20;
            resultsTextBox.Height = this.Height - 120;
        }
        if (btnClose != null) btnClose.Location = new Point(this.Width - 90, 7);
    }

    private void FadeTimer_Tick(object sender, EventArgs e)
    {
        // تأثير التلاشي التدريجي عند الفتح (يمكن تطويره لاحقاً)
    }

    private void ShowToastMessage(string message)
    {
        // رسالة منبثقة مؤقتة جميلة
        Label toast = new Label();
        toast.Text = message;
        toast.BackColor = Color.FromArgb(76, 175, 80);
        toast.ForeColor = Color.White;
        toast.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        toast.Size = new Size(200, 40);
        toast.Location = new Point((this.Width - 200) / 2, this.Height - 100);
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.BorderStyle = BorderStyle.FixedSingle;

        this.Controls.Add(toast);
        toast.BringToFront();

        Timer toastTimer = new Timer();
        toastTimer.Interval = 2000;
        toastTimer.Tick += (s, args) =>
        {
            this.Controls.Remove(toast);
            toastTimer.Stop();
            toastTimer.Dispose();
        };
        toastTimer.Start();
    }

    public void AddMessage(string message, bool isWelcomeMessage = false)
    {
        try
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, bool>(AddMessage), message, isWelcomeMessage);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            resultsTextBox.AppendText($"[{timestamp}] {message}\n");

            // التمرير التلقائي للأسفل
            resultsTextBox.SelectionStart = resultsTextBox.Text.Length;
            resultsTextBox.ScrollToCaret();

            // إظهار النافذة إذا كانت مخفية وليست رسالة ترحيبية
            if (!this.Visible && !isWelcomeMessage)
            {
                this.Show();
                this.BringToFront();

                // إضافة رسالة ترحيبية أولى للنافذة المنبثقة
                if (resultsTextBox.Text.Length < 100) // إذا كانت هذه أول رسالة في النافذة
                {
                    resultsTextBox.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] 📊 مرحباً بك في نافذة النتائج!\n");
                    resultsTextBox.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] يمكنك نسخ النتائج أو مسحها أو تصغير النافذة\n");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"خطأ في AddMessage: {ex.Message}");
        }
    }

    private void AddTimestampMessage(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        resultsTextBox.AppendText($"[{timestamp}] {message}\n");
        resultsTextBox.SelectionStart = resultsTextBox.Text.Length;
        resultsTextBox.ScrollToCaret();
    }

    public void ClearResults()
    {
        resultsTextBox.Clear();
    }

    public string GetResults()
    {
        return resultsTextBox.Text;
    }
}

namespace GraphTeachingApp
{
    /// <summary>
    /// النافذة الرئيسية لتطبيق تعليم الرسوم البيانية - مركز التحكم في التطبيق
    ///
    /// هذه الفئة هي قلب التطبيق وتتولى جميع المهام التالية:
    ///
    /// إدارة الواجهة:
    /// - إنشاء وترتيب جميع عناصر الواجهة (أزرار، مربعات نص، لوحة رسم)
    /// - التعامل مع تغيير أحجام النافذة والتكيف التلقائي
    /// - إدارة النافذة المنبثقة لعرض النتائج
    ///
    /// إدارة الرسم البياني:
    /// - حفظ الحالة الحالية للرسم البياني وقائمة العقد والوصلات
    /// - إدارة الرسوم المتحركة والتلوين التفاعلي للخوارزميات
    /// - حفظ حالة التحديد والتفاعل مع المستخدم
    ///
    /// الخوارزميات والعمليات:
    /// - تنفيذ خوارزميات البحث (DFS, BFS)
    /// - إيجاد المسارات وتحليل الرسم البياني
    /// - إنشاء الرسوم المتممة والجزئيات
    /// - فحص أنواع الرسوم البيانية المختلفة
    ///
    /// التفاعل مع المستخدم:
    /// - معالجة أحداث الماوس (إضافة عقد، رسم وصلات، سحب العقد)
    /// - معالجة أحداث الأزرار والمدخلات
    /// - عرض النتائج والرسائل التوضيحية
    ///
    /// مثال على دورة العمل النموذجية:
    /// 1. إنشاء رسم بياني جديد أو تحميل ملف موجود
    /// 2. إضافة عقد بالنقر المزدوج في لوحة الرسم
    /// 3. ربط العقد ببعضها بالنقر على عقدتين متتاليتين
    /// 4. تنفيذ خوارزمية أو عملية من الأزرار المتاحة
    /// 5. مراقبة النتائج في النافذة المنبثقة وعلى لوحة الرسم
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
        private ResultsWindow resultsWindow; // النافذة المنبثقة للنتائج

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
                // تحديد حجم النافذة المحسن مع بدء التشغيل بكامل الشاشة
                this.Size = new Size(1200, 800);
                this.MinimumSize = new Size(900, 600); // حد أدنى لمنع تصغير زائد
                this.Text = "تعليم الرسوم البيانية - Graphs Teaching App";
                this.StartPosition = FormStartPosition.CenterScreen;
                this.WindowState = FormWindowState.Maximized; // بدء التشغيل بكامل الشاشة
                this.Resize += MainForm_Resize; // إضافة معالج تغيير الحجم

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

                // رسالة ترحيب بسيطة في النافذة الرئيسية فقط
                AppendToResults("مرحباً بك في تطبيق تعليم الرسوم البيانية!", true);
                AppendToResults("يمكنك البدء بإنشاء رسم بياني جديد أو تحميل ملف موجود.", true);
                AppendToResults("نصيحة: انقر نقرة مزدوجة في أي مكان لإضافة عقدة جديدة", true);

                AppendToResults($"تم تهيئة النافذة بنجاح. عدد العناصر: {this.Controls.Count}", true);

                // رسالة توضيحية حول النافذة المنبثقة
                AppendToResults("ملاحظة: ستظهر نافذة النتائج المنبثقة عند تنفيذ أي خوارزمية أو عملية", true);
                AppendToResults("يمكنك إغلاقها وستعود للظهور عند الحاجة", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء النافذة: {ex.Message}\n\nتفاصيل الخطأ: {ex.StackTrace}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// تهيئة العناصر المخصصة للواجهة مع تخطيط محسن ومتجاوب
        /// </summary>
        private void InitializeCustomComponents()
        {
            // تعيين خصائص النافذة للتكيف التلقائي
            this.AutoScroll = false;

            // إنشاء لوحة الرسم الرئيسية مع خصائص متجاوبة
            drawingPanel = new Panel();
            drawingPanel.Size = new Size(800, 600);
            drawingPanel.Location = new Point(20, 60);
            drawingPanel.BackColor = Color.White;
            drawingPanel.BorderStyle = BorderStyle.FixedSingle;
            drawingPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // إضافة أحداث الماوس للوحة الرسم
            drawingPanel.MouseDown += DrawingPanel_MouseDown;
            drawingPanel.MouseMove += DrawingPanel_MouseMove;
            drawingPanel.MouseUp += DrawingPanel_MouseUp;
            drawingPanel.MouseWheel += DrawingPanel_MouseWheel;
            drawingPanel.Paint += DrawingPanel_Paint;

            // إضافة اللوحة إلى النافذة
            this.Controls.Add(drawingPanel);

            // إنشاء شريط الأدوات العلوي مع خصائص متجاوبة
            CreateResponsiveToolbar();

            // إنشاء مربعات الإدخال المتجاوبة للعقد والمسارات مع خصائص متجاوبة
            CreateResponsiveInputControls();
        }

        /// <summary>
        /// إنشاء شريط الأدوات المتجاوب مع جميع الأزرار
        /// </summary>
        private void CreateResponsiveToolbar()
        {
            try
            {
                // إنشاء لوحة شريط الأدوات مع خصائص متجاوبة
                toolbarPanel = new Panel();
                toolbarPanel.Size = new Size(1150, 80);
                toolbarPanel.Location = new Point(10, 10);
                toolbarPanel.BackColor = Color.LightGray;
                toolbarPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                toolbarPanel.AutoScroll = false;

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
                     ("جميع المسارات", BtnAllPaths_Click),
                     ("عرض النتائج", BtnShowResults_Click)
                 };

                // إنشاء جميع الأزرار
                foreach (var (text, handler) in buttons)
                {
                    Button btn = new Button();
                    btn.Text = text;
                    btn.Size = new Size(105, 30);
                    btn.BackColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                    if (handler != null)
                        btn.Click += handler;

                    toolbarPanel.Controls.Add(btn);
                }

                // إضافة شريط الأدوات إلى النافذة
                this.Controls.Add(toolbarPanel);

                AppendToResults($"تم إنشاء شريط الأدوات المتجاوب مع {buttons.Length} زر", true);
            }
            catch (Exception ex)
            {
                AppendToResults($"خطأ في إنشاء شريط الأدوات: {ex.Message}", true);
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
        /// إنشاء مربعات الإدخال المتجاوبة للعقد والمسارات
        /// </summary>
        private void CreateResponsiveInputControls()
        {
            // لوحة جانبية لمربعات الإدخال
            Panel inputPanel = new Panel();
            inputPanel.Size = new Size(220, 400);
            inputPanel.Location = new Point(10, 10); // سيتم تعديل الموقع ديناميكياً
            inputPanel.BackColor = Color.FromArgb(245, 245, 245);
            inputPanel.BorderStyle = BorderStyle.FixedSingle;
            inputPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            inputPanel.Name = "inputPanel";
            this.Controls.Add(inputPanel);

            // تسمية العقدة المصدر
            Label lblSourceNode = new Label();
            lblSourceNode.Text = "العقدة المصدر:";
            lblSourceNode.Location = new Point(10, 15);
            lblSourceNode.Size = new Size(90, 20);
            lblSourceNode.Name = "lblSourceNode";
            inputPanel.Controls.Add(lblSourceNode);

            // قائمة العقد المصدر
            sourceNodeComboBox = new ComboBox();
            sourceNodeComboBox.Location = new Point(10, 40);
            sourceNodeComboBox.Size = new Size(90, 25);
            sourceNodeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sourceNodeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputPanel.Controls.Add(sourceNodeComboBox);

            // تسمية العقدة الهدف
            Label lblTargetNode = new Label();
            lblTargetNode.Text = "العقدة الهدف:";
            lblTargetNode.Location = new Point(115, 15);
            lblTargetNode.Size = new Size(90, 20);
            lblTargetNode.Name = "lblTargetNode";
            inputPanel.Controls.Add(lblTargetNode);

            // قائمة العقد الهدف
            targetNodeComboBox = new ComboBox();
            targetNodeComboBox.Location = new Point(115, 40);
            targetNodeComboBox.Size = new Size(90, 25);
            targetNodeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            targetNodeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputPanel.Controls.Add(targetNodeComboBox);

            // تسمية طول المسار
            Label lblPathLength = new Label();
            lblPathLength.Text = "طول المسار:";
            lblPathLength.Location = new Point(10, 80);
            lblPathLength.Size = new Size(90, 20);
            lblPathLength.Name = "lblPathLength";
            inputPanel.Controls.Add(lblPathLength);

            // مربع إدخال طول المسار
            pathLengthTextBox = new TextBox();
            pathLengthTextBox.Location = new Point(10, 105);
            pathLengthTextBox.Size = new Size(90, 25);
            pathLengthTextBox.Text = "3";
            pathLengthTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputPanel.Controls.Add(pathLengthTextBox);

            // تسمية اسم العقدة الجديدة
            Label lblNewNode = new Label();
            lblNewNode.Text = "اسم العقدة:";
            lblNewNode.Location = new Point(115, 80);
            lblNewNode.Size = new Size(90, 20);
            lblNewNode.Name = "lblNewNode";
            inputPanel.Controls.Add(lblNewNode);

            // مربع إدخال اسم العقدة الجديدة
            newNodeTextBox = new TextBox();
            newNodeTextBox.Location = new Point(115, 105);
            newNodeTextBox.Size = new Size(90, 25);
            newNodeTextBox.Text = "A";
            newNodeTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputPanel.Controls.Add(newNodeTextBox);
        }

        /// <summary>
        /// إضافة نص إلى النافذة المنبثقة للنتائج مع الطابع الزمني
        /// </summary>
        private void AppendToResults(string message, bool isWelcomeMessage = false)
        {
            try
            {
                // إضافة الرسالة للنافذة المنبثقة (إلا إذا كانت رسالة ترحيب)
                if (!isWelcomeMessage)
                {
                    ShowResultsWindow(message);
                }
            }
            catch (Exception ex)
            {
                // في حالة حدوث خطأ، نحاول طباعته في Console للتشخيص
                Console.WriteLine($"خطأ في AppendToResults: {ex.Message}");
            }
        }

        /// <summary>
        /// إنشاء وعرض النافذة المنبثقة للنتائج بأمان
        /// </summary>
        private void ShowResultsWindow(string message)
        {
            try
            {
                // إنشاء النافذة إذا لم تكن موجودة أو تم إغلاقها
                if (resultsWindow == null || resultsWindow.IsDisposed)
                {
                    resultsWindow = new ResultsWindow();
                    resultsWindow.Show();
                }
                else if (!resultsWindow.Visible)
                {
                    resultsWindow.Show();
                }

                // إضافة الرسالة للنافذة المنبثقة
                resultsWindow.AddMessage(message, false);

                // جعل النافذة في المقدمة
                resultsWindow.BringToFront();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في ShowResultsWindow: {ex.Message}");
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
                        AppendToResults("نصيحة: انقر نقرة مزدوجة في مكان فارغ لإضافة عقدة جديدة");
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

                    // إضافة عقدة جديدة عند النقر المزدوج
                    if (e.Clicks >= 2)
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
        /// معالج حدث تغيير حجم النافذة للتكيف التلقائي
        /// </summary>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            try
            {
                // إعادة ترتيب العناصر عند تغيير حجم النافذة
                ArrangeControlsForCurrentSize();
            }
            catch (Exception ex)
            {
                AppendToResults($"تحذير: خطأ في تعديل الحجم: {ex.Message}");
            }
        }

        /// <summary>
        /// ترتيب العناصر حسب الحجم الحالي للنافذة
        /// </summary>
        private void ArrangeControlsForCurrentSize()
        {
            if (this.WindowState == FormWindowState.Minimized) return;

            int windowWidth = this.ClientSize.Width;
            int windowHeight = this.ClientSize.Height;

            // تحديث حجم وموقع لوحة الرسم لتكون متجاوبة
            if (drawingPanel != null)
            {
                // احسب المساحة المتاحة للوحة الرسم (بدون مربع النتائج السفلي)
                int toolbarHeight = toolbarPanel?.Height ?? 80;
                int inputControlsWidth = 240; // عرض لوحة الإدخال الجانبية

                int drawingWidth = Math.Max(400, windowWidth - inputControlsWidth - 60);
                int drawingHeight = Math.Max(300, windowHeight - toolbarHeight - 40); // مساحة أكبر بدون مربع النتائج

                drawingPanel.Size = new Size(drawingWidth, drawingHeight);
                drawingPanel.Location = new Point(20, toolbarHeight + 20);

                // تحديث حجم الخط بناءً على حجم لوحة الرسم
                UpdateFontSizes();
            }

            // تحديث حجم شريط الأدوات
            if (toolbarPanel != null)
            {
                toolbarPanel.Width = Math.Max(600, windowWidth - 40);
                // إعادة ترتيب الأزرار في شريط الأدوات
                ArrangeToolbarButtons();
            }

            // تحديث موقع لوحة الإدخال الجانبية
            if (this.Controls.ContainsKey("inputPanel") && this.Controls["inputPanel"] is Panel inputPanel)
            {
                int inputControlsWidth = 240; // عرض لوحة الإدخال الجانبية
                inputPanel.Location = new Point(windowWidth - inputControlsWidth - 20, toolbarPanel?.Height ?? 80);
                inputPanel.Size = new Size(inputControlsWidth, windowHeight - (toolbarPanel?.Height ?? 80) - 40); // ارتفاع أكبر بدون مربع النتائج
            }

            // تحديث موقع وأبعاد مربعات الإدخال
            ArrangeInputControls();

            // إعادة رسم لوحة الرسم
            if (drawingPanel != null)
            {
                drawingPanel.Invalidate();
            }
        }

        /// <summary>
        /// تحديث أحجام الخطوط حسب حجم النافذة
        /// </summary>
        private void UpdateFontSizes()
        {
            if (drawingPanel == null) return;

            float scaleFactor = Math.Min(drawingPanel.Width / 800f, drawingPanel.Height / 600f);
            scaleFactor = Math.Max(0.5f, Math.Min(2.0f, scaleFactor)); // تحديد بين 0.5 و 2.0

            // تحديث حجم خط النافذة المنبثقة للنتائج
            if (resultsWindow != null)
            {
                // سيتم تحديث حجم خط النافذة المنبثقة داخل النافذة نفسها
            }

            // تحديث حجم خط الأزرار
            if (toolbarPanel != null)
            {
                foreach (Control ctrl in toolbarPanel.Controls)
                {
                    if (ctrl is Button btn)
                    {
                        btn.Font = new Font(btn.Font.FontFamily, (int)(9 * scaleFactor));
                    }
                }
            }

            AppendToResults($"تم تحديث أحجام العناصر للحجم الجديد (معامل القياس: {scaleFactor:F2})", true);
        }

        /// <summary>
        /// ترتيب أزرار شريط الأدوات بشكل متجاوب
        /// </summary>
        private void ArrangeToolbarButtons()
        {
            if (toolbarPanel == null) return;

            int buttonWidth = 105;
            int buttonHeight = 30;
            int spacing = 5;
            int currentX = 10;
            int currentY = 5;

            // تعديل عرض الأزرار حسب حجم شريط الأدوات
            int availableWidth = toolbarPanel.Width - 20;
            int buttonsPerRow = Math.Max(1, availableWidth / (buttonWidth + spacing));

            if (buttonsPerRow < 6) // إذا كان العرض ضيق، اجعل الأزرار أصغر
            {
                buttonWidth = (availableWidth - (buttonsPerRow + 1) * spacing) / buttonsPerRow;
            }

            int buttonsInCurrentRow = 0;

            foreach (Control ctrl in toolbarPanel.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.Size = new Size(buttonWidth, buttonHeight);
                    btn.Location = new Point(currentX, currentY);

                    currentX += buttonWidth + spacing;
                    buttonsInCurrentRow++;

                    if (buttonsInCurrentRow >= buttonsPerRow && ctrl != toolbarPanel.Controls[toolbarPanel.Controls.Count - 1])
                    {
                        currentX = 10;
                        currentY += buttonHeight + spacing;
                        buttonsInCurrentRow = 0;
                    }
                }
            }
        }

        /// <summary>
        /// ترتيب مربعات الإدخال بشكل متجاوب داخل اللوحة الجانبية
        /// </summary>
        private void ArrangeInputControls()
        {
            if (sourceNodeComboBox == null) return;

            // جميع العناصر الآن داخل اللوحة الجانبية، لذا لا نحتاج لتعديل مواقعها
            // فقط نتأكد من أنها مرئية ومحدثة

            // تحديث أسماء العقد في القوائم إذا لزم الأمر
            UpdateNodeComboBoxes();

            // تحديث حجم الخط للعناصر داخل اللوحة الجانبية
            if (this.Controls.ContainsKey("inputPanel") && this.Controls["inputPanel"] is Panel inputPanel)
            {
                float scaleFactor = Math.Min(inputPanel.Width / 220f, inputPanel.Height / 400f);
                scaleFactor = Math.Max(0.8f, Math.Min(1.5f, scaleFactor));

                foreach (Control ctrl in inputPanel.Controls)
                {
                    if (ctrl is Label lbl)
                    {
                        lbl.Font = new Font(lbl.Font.FontFamily, (int)(9 * scaleFactor));
                    }
                    else if (ctrl is ComboBox cb)
                    {
                        cb.Font = new Font(cb.Font.FontFamily, (int)(9 * scaleFactor));
                    }
                    else if (ctrl is TextBox tb)
                    {
                        tb.Font = new Font(tb.Font.FontFamily, (int)(9 * scaleFactor));
                    }
                }
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
                AppendToResults("خطأ: يرجى إدخال اسم العقدة في مربع النص", true);
                return;
            }

            // التحقق من عدم وجود عقدة بنفس الاسم
            if (currentGraph.Nodes.Any(n => n.Name == nodeName))
            {
                AppendToResults($"خطأ: العقدة '{nodeName}' موجودة مسبقاً", true);
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

        /// <summary>
        /// معالج حدث زر عرض النتائج في النافذة المنبثقة
        /// </summary>
        private void BtnShowResults_Click(object sender, EventArgs e)
        {
            ShowResultsWindow("تم فتح نافذة النتائج يدوياً");
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
        /// فحص ما إذا كان الرسم البياني ثنائي التجزئة (Bipartite) باستخدام خوارزمية BFS
        ///
        /// الرسم البياني ثنائي التجزئة هو رسم يمكن تقسيمه إلى مجموعتين منفصلتين حيث:
        /// - لا توجد وصلات داخل نفس المجموعة
        /// - جميع الوصلات تكون بين المجموعتين المختلفتين
        ///
        /// خوارزمية التلوين (Coloring Algorithm):
        /// - نستخدم نظام تلوين بسيط: اللون 0 واللون 1
        /// - نبدأ بتلوين عقدة باللون 0
        /// - كل جار يجب أن يكون بلون مختلف عن العقدة الحالية
        /// - إذا وجدنا جار بنفس اللون، فالرسم ليس ثنائي التجزئة
        ///
        /// خطوات الخوارزمية:
        /// 1. إنشاء خريطة للألوان لكل عقدة
        /// 2. إنشاء طابور لبدء BFS من العقدة الأولى
        /// 3. تلوين العقدة الأولى باللون 0 وإضافتها للطابور
        /// 4. لكل عقدة في الطابور:
        ///    - فحص جميع جيرانها
        ///    - تلوين الجيران غير الملونين باللون المعاكس
        ///    - إذا وُجد جار بلون نفس العقدة الحالية → الرسم ليس ثنائي
        ///
        /// أهمية الرسوم ثنائية التجزئة:
        /// - تستخدم في نمذجة المشاكل ذات التقسيم الثنائي
        /// - مفيدة في حل مشاكل التخصيص والجدولة
        /// - أساس خوارزميات المطابقة (Matching Algorithms)
        ///
        /// أمثلة:
        /// - الرسم الذي يمثل مجموعتين من الأشخاص والوظائف (علاقات التوظيف)
        /// - رسم الصداقة بين مجموعتين مختلفتين بدون صداقات داخلية
        /// </summary>
        /// <returns>true إذا كان الرسم ثنائي التجزئة، false إذا لم يكن كذلك</returns>
        private bool IsBipartite()
        {
            // حالة خاصة: الرسم الفارغ ثنائي التجزئة افتراضياً
            if (currentGraph.Nodes.Count == 0) return true;

            var colors = new Dictionary<string, int>();  // خريطة الألوان للعقد
            var queue = new Queue<string>();            // طابور لخوارزمية BFS

            // المرحلة الأولى: بدء التلوين من العقدة الأولى
            string startNode = currentGraph.Nodes[0].Name;
            colors[startNode] = 0;  // تلوين باللون 0 (المجموعة الأولى)
            queue.Enqueue(startNode);

            // المرحلة الثانية: معالجة جميع العقد بالترتيب
            while (queue.Count > 0)
            {
                string current = queue.Dequeue();  // اخراج العقدة الحالية

                // فحص جميع جيران العقدة الحالية
                if (currentGraph.AdjacencyList.ContainsKey(current))
                {
                    foreach (string neighbor in currentGraph.AdjacencyList[current])
                    {
                        // إذا لم تكن العقدة ملونة بعد
                        if (!colors.ContainsKey(neighbor))
                        {
                            // تلوين الجار باللون المعاكس للعقدة الحالية
                            // إذا كانت العقدة الحالية بلون 0، فالجار بلون 1 والعكس
                            colors[neighbor] = 1 - colors[current];
                            queue.Enqueue(neighbor);  // إضافة الجار للطابور لمعالجته لاحقاً
                        }
                        // إذا كانت العقدة ملونة وكان لونها نفس لون العقدة الحالية
                        else if (colors[neighbor] == colors[current])
                        {
                            // خطأ: نفس اللون في نفس المجموعة - ليس ثنائي التجزئة
                            // هذا يعني وجود وصلة داخل نفس المجموعة
                            AppendToResults($"تعارض في التلوين بين {current} و {neighbor}");
                            return false;
                        }
                        // إذا كان اللون مختلف، فهذا متوقع وليس هناك مشكلة
                    }
                }
            }

            // إذا انتهت المعالجة بدون تعارض، فالرسم ثنائي التجزئة
            AppendToResults($"تم تلوين الرسم بنجاح: مجموعة 0 ({colors.Count(kvp => kvp.Value == 0)} عقد)، مجموعة 1 ({colors.Count(kvp => kvp.Value == 1)} عقد)");
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
        /// تنفيذ خوارزمية البحث في العمق (Depth-First Search - DFS) مع الرسم التدريجي
        ///
        /// خوارزمية DFS هي إحدى أهم خوارزميات التنقل في الرسوم البيانية:
        ///
        /// مبدأ عمل DFS:
        /// - نبدأ من عقدة البداية ونكتشف أكبر عمق ممكن في اتجاه واحد
        /// - نستكشف الفرع كاملاً قبل العودة لاستكشاف فروع أخرى
        /// - نستخدم مبدأ الـ Stack (Last In, First Out) في التنفيذ
        ///
        /// خطوات الخوارزمية:
        /// 1. ابدأ من العقدة المحددة وعلّمها كمُزارة
        /// 2. أضف العقدة إلى ترتيب الزيارة
        /// 3. لكل جار غير مُزار من العقدة الحالية:
        ///    - اذهب إليه وكرر العملية نفسها
        /// 4. عند الانتهاء من فرع، ارجع للفرع السابق
        ///
        /// الخصائص المهمة:
        /// - تستخدم في اكتشاف العقد واستكشاف الفرع بالكامل
        /// - مفيدة في حل المشاكل التي تتطلب استكشاف أعماق البيانات
        /// - أداؤها O(V + E) حيث V عدد العقد و E عدد الوصلات
        ///
        /// مثال عملي:
        /// للرسم: A-B, A-C, B-D, C-E
        /// ترتيب DFS من A: A -> B -> D -> C -> E
        /// </summary>
        /// <param name="startNode">العقدة التي نبدأ منها البحث</param>
        private void ExecuteDFS(string startNode)
        {
            var visited = new HashSet<string>(); // تتبع العقد المُزارة لتجنب التكرار
            var dfsOrder = new List<string>();  // حفظ ترتيب زيارة العقد

            AppendToResults($"بدء خوارزمية البحث في العمق (DFS) من العقدة: {startNode}");

            // بدء الخوارزمية التكرارية
            DFSRecursive(startNode, visited, dfsOrder);

            // عرض النتيجة النهائية للمستخدم
            AppendToResults($"ترتيب زيارة العقد في DFS: {string.Join(" -> ", dfsOrder)}");
            AppendToResults($"إجمالي العقد المُزارة: {dfsOrder.Count}");
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
        /// تنفيذ خوارزمية البحث في العرض أولاً (Breadth-First Search - BFS) مع الرسم التدريجي
        ///
        /// خوارزمية BFS هي خوارزمية بحث منهجية تستكشف الرسم البياني طبقة بطبقة:
        ///
        /// مبدأ عمل BFS:
        /// - نبدأ من عقدة البداية ونزور جميع جيرانها أولاً
        /// - ثم نزور جيران الجيران، وهكذا في طبقات متتالية
        /// - نستخدم مبدأ الـ Queue (First In, First Out) في التنفيذ
        ///
        /// خطوات الخوارزمية:
        /// 1. أنشئ طابور فارغ ولديك مجموعة للعقد المُزارة
        /// 2. أضف العقدة البداية للطابور وعلّمها كمُزارة
        /// 3. بينما الطابور غير فارغ:
        ///    - اخرج العقدة الأولى من الطابور
        ///    - لكل جار غير مُزار من هذه العقدة:
        ///      - أضفه للطابور وعلّمه كمُزار
        ///      - سجل ترتيب الزيارة
        ///
        /// الخصائص المهمة:
        /// - تزور العقد في طبقات حسب بعدها عن نقطة البداية
        /// - مفيدة في إيجاد أقصر مسار في رسوم غير موزونة
        /// - مضمونة للعثور على أقصر مسار في رسوم بدون أوزان
        /// - أداؤها O(V + E) حيث V عدد العقد و E عدد الوصلات
        ///
        /// مثال عملي:
        /// للرسم: A-B, A-C, B-D, C-E
        /// ترتيب BFS من A: A -> B -> C -> D -> E
        /// (الطبقة الأولى: B,C ثم الطبقة الثانية: D,E)
        /// </summary>
        /// <param name="startNode">العقدة التي نبدأ منها البحث</param>
        private void ExecuteBFS(string startNode)
        {
            var visited = new HashSet<string>();     // تتبع العقد المُزارة
            var bfsOrder = new List<string>();      // ترتيب زيارة العقد
            var queue = new Queue<string>();        // الطابور لتنفيذ BFS

            AppendToResults($"بدء خوارزمية البحث في العرض (BFS) من العقدة: {startNode}");

            // المرحلة الأولى: تهيئة البداية
            visited.Add(startNode);                 // علّم العقدة البداية كمُزارة
            queue.Enqueue(startNode);              // أضفها للطابور
            bfsOrder.Add(startNode);               // سجلها في ترتيب الزيارة

            // تلوين العقدة الأولى باللون المميز للبداية
            var startNodeObj = currentGraph.Nodes.FirstOrDefault(n => n.Name == startNode);
            if (startNodeObj != null)
            {
                startNodeObj.NodeColor = BFS_COLOR;  // لون أزرق للعقدة الحالية
                drawingPanel.Invalidate();          // إعادة رسم لوحة الرسم
                System.Threading.Thread.Sleep(1000); // انتظار لتوضيح العملية
            }

            // المرحلة الثانية: المعالجة الرئيسية للخوارزمية
            while (queue.Count > 0)
            {
                string current = queue.Dequeue();   // اخراج العقدة الأولى من الطابور

                // فحص جميع جيران العقدة الحالية
                if (currentGraph.AdjacencyList.ContainsKey(current))
                {
                    foreach (string neighbor in currentGraph.AdjacencyList[current])
                    {
                        // إذا لم تكن العقدة مُزارة بعد
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);         // علّمها كمُزارة
                            queue.Enqueue(neighbor);       // أضفها للطابور للمعالجة لاحقاً
                            bfsOrder.Add(neighbor);        // سجلها في ترتيب الزيارة

                            // تلوين العقدة الجديدة باللون المميز
                            var neighborNode = currentGraph.Nodes.FirstOrDefault(n => n.Name == neighbor);
                            if (neighborNode != null)
                            {
                                neighborNode.NodeColor = BFS_COLOR;  // لون أزرق للعقدة الجديدة
                                drawingPanel.Invalidate();           // إعادة رسم لوحة الرسم
                                System.Threading.Thread.Sleep(1000); // انتظار لتوضيح العملية
                            }
                        }
                    }
                }

                // تلوين العقدة الحالية باللون الأخضر بعد الانتهاء من معالجة جميع جيرانها
                var currentNode = currentGraph.Nodes.FirstOrDefault(n => n.Name == current);
                if (currentNode != null)
                {
                    currentNode.NodeColor = VISITED_COLOR;  // لون أخضر للعقدة المكتملة
                    drawingPanel.Invalidate();              // إعادة رسم لوحة الرسم
                    System.Threading.Thread.Sleep(500);     // انتظار أقصر للانتقال السلس
                }
            }

            // عرض النتائج النهائية للمستخدم
            AppendToResults($"ترتيب زيارة العقد في BFS: {string.Join(" -> ", bfsOrder)}");
            AppendToResults($"إجمالي العقد المُزارة: {bfsOrder.Count}");
            AppendToResults($"عدد المستويات (الطبقات): ~{Math.Ceiling(Math.Log(bfsOrder.Count, 2))}");
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