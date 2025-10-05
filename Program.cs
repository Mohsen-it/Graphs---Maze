using System;
using System.Windows.Forms;

namespace GraphTeachingApp
{
    static class Program
    {
        /// <summary>
        /// النقطة الرئيسية لبدء تشغيل التطبيق
        /// </summary>
        [STAThread]
        static void Main()
        {
            // تمكين التصميم الحديث للنوافذ
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // إنشاء وتشغيل النافذة الرئيسية
            Application.Run(new MainForm());
        }
    }
}