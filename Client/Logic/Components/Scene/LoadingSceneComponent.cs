using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class LoadingSceneComponent : Entity
{
    public override async void OnStart()
    {
        base.OnStart();

        await UIHelper.LoadUI<UILoding>(parent: this);
    }
}
