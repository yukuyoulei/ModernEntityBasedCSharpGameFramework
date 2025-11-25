// 文件中部分内容为自动生成，请勿修改标记内的代码防止被再次生成时覆盖
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UILoding : Entity
{
    //[AUTO_GENERATED_START]
    // 自动生成的成员变量部分，不要手动修改
    //[MEMBER_VARIABLES_START]
    public Button btnLogin;
    public Slider Slider;
    //[MEMBER_VARIABLES_END]

    public override void OnStart()
    {
        base.OnStart();
        // 初始化代码部分，不要手动修改
        //[INITIALIZATION_START]
        btnLogin = this.GetMonoComponent<Button>("btnLogin");
        Slider = this.GetMonoComponent<Slider>("Slider");
        //[INITIALIZATION_END]
    }
}