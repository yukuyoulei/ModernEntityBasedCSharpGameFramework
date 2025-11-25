using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEBCGF
{
    internal class StartSceneComponent : Entity
    {
        public override void OnStart()
        {
            base.OnStart();

            Log.Debug($"StartSceneComponent OnStart");

            RegisterCall(Events.LoginSuccess, LoginSuccess);
        }

        private async void LoginSuccess()
        {
            await Parent.AddChild<LoadingSceneComponent>("Prefab/Scene/LoadingScene");
            OnDispose();
        }

        public override void OnDestroy()
        {
            Log.Debug($"StartSceneComponent OnDestroy");
            base.OnDestroy();
        }
    }
}
