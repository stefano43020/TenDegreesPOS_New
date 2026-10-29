using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TenDegreesPOS_New
{
    /// <summary>
    /// A custom TabControl that removes the default white border.
    /// Uses SetWindowTheme to disable the native border, but keeps default drawing
    /// so StyleTabControl's DrawItem event still works.
    /// </summary>
    public class FlatTabControl : TabControl
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        public FlatTabControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Remove the Windows theme from this control so the native white border is not drawn
            if (this.IsHandleCreated)
            {
                SetWindowTheme(this.Handle, "", "");
            }
        }

        protected override void WndProc(ref Message m)
        {
            // WM_ERASEBKGND - suppress default background erase (prevents white flash)
            if (m.Msg == 0x0014)
            {
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Fill entire background with theme color
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }
    }
}