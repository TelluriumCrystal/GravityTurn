﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using KSP.IO;
using System.IO;
using ClickThroughFix;

namespace GravityTurn.Window
{

    public class PersistentWindow
    {
        [Persistent]
        public float left;
        [Persistent]
        public float top;
        [Persistent]
        public float width;
        [Persistent]
        public float height;

        public PersistentWindow(float left,float top,float width,float height)
        {
            this.left=left;
            this.top=top;
            this.width=width;
            this.height=height;
        }
        public PersistentWindow()
        {
            this.left = 0;
            this.top = 0;
            this.width = 0;
            this.height = 0;
        }
        public static implicit operator Rect(PersistentWindow rect)
        {
            return new Rect(rect.left, rect.top, rect.width, rect.height);
        }
        public static implicit operator PersistentWindow(Rect rect)
        {
            return new PersistentWindow(rect.xMin, rect.yMin, rect.width, rect.height);
        }
    }

    public class BaseWindow
    {
        int WindowID;
        protected GravityTurner turner;
        public bool WindowVisible = false; 
        public string WindowTitle = "GravityTurn";
        string filename;
        public static bool ShowGUI = true;

        [Persistent]
        public PersistentWindow windowPos = new PersistentWindow();

        // Get width of a displayed string
        public float TxtWidth(string txt)
        {
            float txtWidth;
            GUIContent content = new GUIContent(txt);
            txtWidth = GUI.skin.textField.CalcSize(content).x;

            return txtWidth;
        }

        protected void ItemLabel(string labelText)
        {
            //GUILayout.Label(labelText, GUILayout.ExpandWidth(false), GUILayout.Width(windowPos.width / 2));
            float itemLabelWidth = 150;

            // Calcul for Stock Skin
            if (HighLogic.CurrentGame.Parameters.CustomParams<GT>().useStock)
                itemLabelWidth = GravityTurner.mainWindow.mainWindowBiggerLineWidth - GravityTurner.mainWindow.inputTextField + 5;
            
            GUILayout.Label(labelText, GUILayout.ExpandWidth(false), GUILayout.Width(itemLabelWidth));
        }

        public BaseWindow(GravityTurner turner, int inWindowID)
        {
            this.turner = turner;
            turner.windowManager.Register(this);
            WindowID = inWindowID;
            filename = LaunchDB.GetBaseFilePath(turner.GetType(), string.Format("gt_window_{0}.cfg", WindowID));
            Load();
            if (windowPos.left + windowPos.width > Screen.width)
            {
                windowPos.left = Screen.width - windowPos.width;
            }
            if (windowPos.top + windowPos.height > Screen.height )
            {
                windowPos.top = Screen.height - windowPos.height;
            }
            if (windowPos.top < 0)
                windowPos.top = 0;
        }

        public void Load()
        {
            try
            {
                ConfigNode root = ConfigNode.Load(filename);
                if (root != null)
                {
                    ConfigNode.LoadObjectFromConfig(this, root);
                }
            }
            catch (Exception ex)
            {
                GravityTurner.Log("Window Load error {0}", ex.ToString());
            }
        }

        public virtual void WindowGUI(int windowID)
        {
            if (!ShowGUI)
                return;
            if (GUI.Button(new Rect(windowPos.width - 18, 2, 16, 16), "X"))
            {
                WindowVisible = false;
            }
            //GUI.DragWindow();
        }
        public void drawGUI()
        {
            if (WindowVisible && ShowGUI)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<GT>().useStock)
                {
                    GuiUtils.LoadSkin(HighLogic.CurrentGame.Parameters.CustomParams<GT>().useCompact);
                    GUI.skin = GuiUtils.skin;
                }
                else
                    GUI.skin = HighLogic.Skin;
                windowPos = ClickThruBlocker.GUILayoutWindow(WindowID, windowPos, WindowGUI, WindowTitle, GUILayout.MinWidth(275));
                //windowPos = GUILayout.Window(WindowID, windowPos, WindowGUI, WindowTitle, GUILayout.MinWidth(300));
            }
        }

        public void OnDestroy()
        {
            Save();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            ConfigNode root = ConfigNode.CreateConfigFromObject(this);
            root.Save(filename);
        }
    }
}
