using MEBCGF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace MEBCGF
{
    public static class Startup
    {
        public static void Start()
        {
            Log.Debug($"Application Start");

            InitActions();

            InitGame();
        }

        private static void InitGame()
        {
            Game.world = new();
            Game.world.OnStart();

            _ = Game.world.AddChild<StartComponent>("Start");
        }

        private static void InitActions()
        {
            CodeLoader.UpdateAction = Update;
            CodeLoader.ApplicationQuitAction = ApplicationQuit;
            CodeLoader.ApplicationPauseAction = ApplicationPause;
            CodeLoader.ApplicationFocusAction = ApplicationFocus;
        }

        private static void ApplicationFocus(bool focus)
        {
            Log.Debug($"Application focus changed: {focus}");
        }

        private static void ApplicationPause(bool pause)
        {
            Log.Debug($"Application pause changed: {pause}");
        }

        private static void ApplicationQuit()
        {
            Log.Debug("Application is quitting.");
            Game.world.OnDestroy();
        }

        private static void Update()
        {
            Entity.FastCall(Events.Update, Time.deltaTime);
        }
    }
}