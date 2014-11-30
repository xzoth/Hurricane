﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hurricane.MagicArrow.DockManager
{
    public class DockManager : IDisposable
    {
        protected double maxwidth;
        protected Window basewindow;

        public DockManager(Window window)
        {
            maxwidth = Utilities.WpfScreen.AllScreensWidth;
            basewindow = window;
        }

        public void DragStart()
        {
            if (!dragstopped) return;
            dragstopped = false;
            enabled = true;
            Utilities.HookManager.MouseHook.HookManager.MouseMove += HookManager_MouseMove;
        }

        private bool isatborder = false;
        private DockRangeWindow window;

        protected double height;
        protected double left;
        protected DockingSide side = DockingSide.None; //new side
        protected DockingSide currentside; //the applied side
        protected bool enabled;
        void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!enabled) return;
            if (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Released) { DragStop(); return; } //If the user doubleclicks the window, relocates the window and releases the mouse, it doesn't get stopped
            if (e.X < 5 || e.X >= maxwidth - 5)
            {
                if (!isatborder)
                {
                    isatborder = true;
                    side = e.X <= 5 ? DockingSide.Left : DockingSide.Right;
                    height = Utilities.WpfScreen.GetScreenFrom(new System.Windows.Point(e.X, e.Y)).WorkingArea.Height;
                    left = side == DockingSide.Left ? 0 : maxwidth - 300;
                    window = new DockRangeWindow(left, height);
                    window.Show();
                }
            }
            else if (isatborder) { isatborder = false; CloseWindowIfExists(); side = DockingSide.None; }
        }

        protected void CloseWindowIfExists()
        {
            if (window != null) { window.Close(); window = null; }
        }

        protected bool dragstopped = true;
        public void DragStop()
        {
            if (dragstopped) return;
            dragstopped = true;
            enabled = false;
            Utilities.HookManager.MouseHook.HookManager.MouseMove -= HookManager_MouseMove;

            if (side != DockingSide.None)
            {
                basewindow.Height = height;
                basewindow.Left = left;
                basewindow.Top = 0;
            }
            currentside = side;
            side = DockingSide.None;
            CloseWindowIfExists();
        }

        public void InitializeWindow()
        {
            var appstate = Settings.HurricaneSettings.Instance.Config.ApplicationState;
            if (appstate == null)
            {
                basewindow.Left = 0;
                basewindow.Top = 0;
                basewindow.Height = System.Windows.SystemParameters.WorkArea.Height;
                return;

            }
            currentside = appstate.CurrentSide;
            if (appstate.CurrentSide == DockingSide.None)
            {
                basewindow.Top = appstate.Top;
                if (appstate.Left <= maxwidth) //When the user disconnects the monitor, the application would be out of range
                    basewindow.Left = appstate.Left;
                basewindow.Height = appstate.Height;
            }
            else
            {
                basewindow.Top = 0;
                basewindow.Left = appstate.CurrentSide == DockingSide.Left ? 0 : maxwidth - 300;
                basewindow.Height = Utilities.WpfScreen.GetScreenFrom(new System.Windows.Point(basewindow.Left, 0)).WorkingArea.Height;
            }
        }

        public void Save()
        {
            if (Settings.HurricaneSettings.Instance.Config.ApplicationState == null) Settings.HurricaneSettings.Instance.Config.ApplicationState = new DockingApplicationState();
            var appstate = Settings.HurricaneSettings.Instance.Config.ApplicationState;

            appstate.CurrentSide = currentside;
            if (currentside == DockingSide.None)
            {
                appstate.Left = basewindow.Left;
                appstate.Top = basewindow.Top;
                appstate.Height = basewindow.Height;
            }
            else
            {
                appstate.Height = -1;
                appstate.Left = -1;
                appstate.Top = -1;
            }
        }

        public void Dispose()
        {
            Utilities.HookManager.MouseHook.HookManager.MouseMove -= HookManager_MouseMove;
        }
    }
}
