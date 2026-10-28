@AGENTS.md

# WYDAdmin — AI 助手指南

本文件为 AI 编程助手提供项目上下文。完整说明请参考 [AGENTS.md](AGENTS.md)。

## 项目概述

基于 .NET 10 的分层后端管理解决方案，使用 ASP.NET Core Web API + PostgreSQL 技术栈。

## 解决方案结构

```
WYDAdmin.slnx                解决方案文件（.slnx 格式）
├── WYD.Admin.WebAPI         ASP.NET Core Web API 入口项目（net10.0）
├── WYD.Service              服务层（net10.0）
├── WYD.ServiceCore          服务核心层（net10.0）
├── WYD.Common               通用工具层（net10.0）
├── WYD.Entity               实体层（net10.0）
├── WYD.Model                模型层（net10.0）
└── WYD.Infrastructure       基础设施层（net10.0）
```

### 项目引用关系

```
WYD.Admin.WebAPI → WYD.Service
WYD.Service → WYD.Common, WYD.Entity, WYD.Infrastructure, WYD.Model, WYD.ServiceCore
WYD.ServiceCore → WYD.Common, WYD.Entity, WYD.Infrastructure, WYD.Model
WYD.Common → WYD.Infrastructure
WYD.Infrastructure → WYD.Entity
WYD.Entity → (无项目引用)
WYD.Model → (无项目引用)
```

## 技术栈

| 领域 | 技术 |
|------|------|
| 运行时 | .NET 10 (net10.0) |
| Web 框架 | ASP.NET Core Web API |
| ORM | EF Core (Npgsql.EntityFrameworkCore.PostgreSQL) + Dapper |
| 对象映射 | Mapperly（Riok.Mapperly，编译期源生成） |
| 缓存 | StackExchange.Redis |
| 日志 | Serilog |
| 加密 | BouncyCastle.Cryptography |
| API 文档 | Scalar.AspNetCore + Microsoft.AspNetCore.OpenApi |
| DI 扫描 | Scrutor |
| 对象存储 | Minio |
| Excel | NPOI |
| 容器化 | Docker (Linux) |

## 常用命令

```bash
# 构建整个解决方案
dotnet build WYDAdmin.slnx

# 运行 Web API（开发模式）
dotnet run --project WYD.Admin.WebAPI

# 运行测试（如有）
dotnet test WYDAdmin.slnx

# Docker 构建
docker build -f WYD.Admin.WebAPI/Dockerfile -t wydadmin .
```

## 开发环境

- **HTTP**: http://localhost:5091
- **HTTPS**: https://localhost:7115
- **Docker HTTP**: 8080
- **Docker HTTPS**: 8081
- **UserSecrets ID**: `880926b9-1a5f-4ce6-8b45-acb23d0f3102`

## 项目状态

> ⚠️ 当前为脚手架初始状态，大部分项目仅包含占位文件，已实现 PhoneRebindRecord 功能作为参考实现。

### 已实现功能

- **PhoneRebindRecord（电话号码换绑记录）** — 完整 CRUD 作为参考实现
  - 实体：`WYD.Entity/Models/PhoneRebindRecord.cs`（表 `t_phone_rebind_record`）
  - DTO：`WYD.Model/PhoneRebindRecordDto.cs`（Dto、CreateDto、UpdateDto、QueryDto、PageResult）
  - 服务：`WYD.Service/PhoneRebindRecordService.cs` + `WYD.Service/IService/IPhoneRebindRecordService.cs`
  - 控制器：`WYD.Admin.WebAPI/Controllers/PhoneRebindRecordController.cs`（路由 `system/phone-rebind`）
  - 映射：`WYD.Entity/Mappings/PhoneRebindRecordMap.cs`（EF Core 实体配置）
  - 对象映射：`WYD.Model/Mappers/PhoneRebindRecordMapper.cs`

### 基础设施组件

- `WYD.Infrastructure/Model/ApiResult.cs` — 统一 API 返回格式（code/msg/data/timestamp）
- `WYD.Infrastructure/Controllers/BaseController.cs` — 控制器基类，统一 JSON 序列化
- `WYD.Infrastructure/Attributes/ActionPermissionFilter.cs` — 权限校验特性（脚手架阶段仅声明，未强制鉴权）
- `WYD.Infrastructure/Attributes/LogAttribute.cs` — 操作日志切面（记录到 Serilog）
- `WYD.Infrastructure/WebExtensions/HttpContextExtension.cs` — 获取当前登录用户
- `WYD.Infrastructure/Enums/BusinessType.cs` — 业务操作类型枚举
- `WYD.Infrastructure/Data/AdminDbContext.cs` — EF Core DbContext

### WebAPI 扩展

- `WYD.Admin.WebAPI/Extensions/DependencyInjectionExtensions.cs` — Scrutor 自动扫描 + Mapperly 映射器 + HttpContextAccessor
- `WYD.Admin.WebAPI/Extensions/DatabaseContextExtensions.cs` — DbContext 注册、初始化、预热
- `WYD.Admin.WebAPI/Extensions/SerilogExtensions.cs` — Serilog 配置（按级别分文件 + 控制台）

### 待清理 / 待实现

- `WYD.Admin.WebAPI/Controllers/WeatherForecastController.cs` — 示例控制器，待替换为实际业务控制器
- `WYD.Admin.WebAPI/WeatherForecast.cs` — 示例模型，待删除
- `WYD.Common/Class1.cs` — 占位文件，待替换为实际工具类
- `WYD.ServiceCore` — 空项目，无源文件
- 数据库使用 PostgreSQL（通过 Npgsql）

## 编码约定

- **JSON 序列化**：使用 System.Text.Json，驼峰命名（camelCase）
- **数据库列名**：snake_case（通过 EF Core 显式映射）
- **API 路由**：小写短横线命名（如 `system/phone-rebind`）
- **权限标识**：冒号分隔（如 `system:phoneRebind:list`）
- **软删除**：使用 `IsDeleted` 标记，查询时过滤 `!e.IsDeleted`
- **审计字段**：CreateBy/CreateName/CreateTime + UpdateBy/UpdateName/UpdateTime
- **DI 注册**：使用 Scrutor 自动扫描，服务实现类与接口放在 `WYD.Service` 项目
- **对象映射**：使用 Mapperly，在 `WYD.Model/Mappers/` 中以 `[Mapper]` 声明的 partial 类，编译期生成映射代码（无反射）
- **日志**：使用 Serilog，通过 `LogAttribute` 切面记录操作日志
