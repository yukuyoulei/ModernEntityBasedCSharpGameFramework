using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEBCGF
{
    internal class StartComponent : Entity
    {
        public override void OnStart()
        {
            base.OnStart();

            Log.Debug($"StartComponent OnStart");
        }
        public override void OnDestroy()
        {
            Log.Debug($"StartComponent OnDestroy");
            base.OnDestroy();
        }
    }
}
