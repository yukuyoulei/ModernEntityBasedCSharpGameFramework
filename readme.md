# Modern Entity-Based C# Game Framework

一个基于实体组件系统的现代C#游戏框架，专为Unity游戏开发设计。

## 项目概述

本框架采用实体(Entity)作为核心概念，通过组合模式实现灵活的游戏对象系统。框架将逻辑与表现分离，支持热重载和模块化开发，适用于中小型游戏项目快速开发。

## 核心特性

### 实体系统 (Entity System)
- 基于实体的架构，所有游戏对象都是[Entity](.//Codes/Base/Entity/Entity.cs#L5-L222)的子类
- 支持父子层级关系和组件组合
- 内置生命周期管理（OnStart, OnDestroy）
- 支持实体分组和类型管理

### 模块化架构
- 逻辑层与表现层分离
- 动态代码加载机制
- 支持热重载开发流程
- 统一的资源管理接口

### UI系统
- 基于Unity UI的封装
- 自动化UI组件绑定
- 内置事件系统集成
- 支持UI层级管理

## 项目结构

```
├── Client/                 # 客户端主目录
│   ├── Assets/             # Unity资源文件
│   │   ├── Editor/         # 编辑器扩展工具
│   │   ├── MonoCodes/      # Unity MonoBehaviour相关代码
│   │   └── Script/         # 入口脚本
│   ├── Logic/              # 游戏逻辑代码
│   │   ├── Components/     # 场景组件
│   │   ├── Consts/         # 常量定义
│   │   ├── Helper/         # 辅助类
│   │   ├── UI/             # UI逻辑
│   │   └── World/          # 世界管理
│   └── LogicBase/          # 客户端基础逻辑
├── Codes/                  # 核心框架代码
│   └── Base/               # 基础类库
│       └── Entity/         # 实体系统核心
└── Server/                 # 服务端代码（预留）
```

## 核心概念

### 实体 (Entity)
所有游戏对象的基础类，提供：
- 生命周期管理
- 父子层级关系
- 组件管理系统
- 分组和类型标识

### 世界 (World)
游戏世界的根实体，负责：
- 管理所有场景对象
- 协调游戏状态
- 处理全局事件

### 启动流程
1. [Main.cs](./Client/Assets/Script/Main.cs) 初始化Unity环境
2. 加载逻辑DLL ([Logic.dll](./Client/DllOutput/Logic.dll))
3. 执行[Startup.Start()](./Client/Logic/Startup.cs#L15-L22)
4. 创建[World](./Client/Logic/World/World.cs)实例
5. 加载初始场景和UI

### UI系统
- 使用[UILogin.cs](./Client/Logic/UI/UILogin.cs)作为示例UI实现
- 自动绑定UI组件
- 集成事件回调机制

## 开发指南

### 创建新实体
```csharp
public class MyEntity : Entity
{
    public override void OnStart()
    {
        base.OnStart();
        // 初始化逻辑
    }
    
    public override void OnDestroy()
    {
        // 清理逻辑
        base.OnDestroy();
    }
}
```

### 添加子实体
```csharp
var child = parent.AddChild<MyEntity>();
```

### 事件系统
```csharp
// 注册事件
RegisterCall(Events.LoginSuccess, OnLogin);

// 触发事件
FastCall(Events.LoginSuccess);
```

## 技术栈

- C# .NET
- Unity 3D
- Entity Component System (ECS) 架构思想
- TextMeshPro (富文本渲染)

## 快速开始

1. 克隆项目到本地
2. 使用Unity打开Client目录
3. 构建Logic项目生成Logic.dll
4. 运行Unity项目

## 目录说明

- **Client/Assets/MonoCodes/**: Unity引擎相关的MonoBehaviour代码
- **Client/Logic/**: 游戏逻辑实现
- **Codes/Base/Entity/**: 实体系统核心实现
- **Client/Logic/UI/**: UI系统实现

## 设计理念

本框架遵循以下设计理念：

1. **组件化开发**：通过实体和组件实现高度可复用的游戏对象系统
2. **关注点分离**：将引擎代码、框架代码和业务逻辑清晰分离
3. **易于维护**：通过约定优于配置的方式减少样板代码
4. **性能优先**：在保证灵活性的同时优化运行时性能

## 扩展性

框架设计具有良好的扩展性：
- 可轻松添加新的实体类型
- 支持自定义组件系统
- 事件系统可扩展
- UI系统支持模板化开发

## 注意事项

- 请勿直接修改自动生成的代码区域
- Logic.dll需要单独编译并放置在正确位置
- 推荐使用提供的编辑器工具辅助开发