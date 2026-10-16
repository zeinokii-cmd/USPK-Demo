# USPK-Demo — исходники для учебной демонстрации

Отдельная копия проекта для преподавателя. Здесь работают вход, четыре роли и управление пользователями от имени администратора. Раздел расписания пока показывает заглушку. Реальная база команды и production не используются.

- `USPK_Project/USPK_Project` — ASP.NET Core 10 API (C#).
- `USPK_Project_Front/USPK_Project_Front` — React, Vite, Bootstrap 5.
- `USPK_Project/compose.demo.yml` — PostgreSQL 16 в отдельном контейнере. Таблицы и четыре тестовых пользователя создаются backend при первом запуске.

## Что нужно установить на Windows

1. [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), именно **SDK**, не только Runtime. Проверьте в PowerShell: `dotnet --list-sdks` (должна быть строка `10.x`).
2. [Node.js 24 LTS](https://nodejs.org/en/download/) с npm. Проверьте: `node --version` и `npm.cmd --version`. Vite допускает также Node 22.12+, но Node 24 LTS проще выбрать при новой установке.
3. [Docker Desktop для Windows](https://docs.docker.com/desktop/setup/install/windows-install/) с Linux-контейнерами и Docker Compose v2. Запустите Docker Desktop и дождитесь его готовности. Проверьте: `docker info` и `docker compose version`. Для WSL 2 нужны поддерживаемая версия Windows, включённая аппаратная виртуализация и WSL 2; точные требования приведены в документации Docker. PostgreSQL отдельно ставить не нужно.

Visual Studio **2026 (18.0+)** поддерживает .NET 10; в Visual Studio 2022 откройте папку в VS Code и запускайте backend командами ниже. Visual Studio не требуется, если есть .NET SDK и VS Code. При первом запуске нужен интернет для NuGet, npm и загрузки образа PostgreSQL.

## Быстрый запуск (Windows 10/11)

Скачайте этот репозиторий через **Code → Download ZIP**, распакуйте в обычную папку, например `C:\USPK-Demo`. Откройте PowerShell в корне распакованной папки. Не запускайте команды из папки ZIP, пока она не распакована.

```powershell
cd 'C:\USPK-Demo'
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\start-demo.ps1
```

Скрипт проверяет нужные программы, запускает отдельную БД, устанавливает зависимости frontend и открывает backend и frontend в двух окнах PowerShell. Дождитесь сообщений о готовности в обоих окнах, затем откройте **http://127.0.0.1:5173/#login**. Оставьте оба окна открытыми. Если распакованная папка получила суффикс (например, `USPK-Demo-main`), используйте её фактическое имя в `cd`.

При первом запуске создаётся локальный `USPK_Project/.env.demo`: случайные пароль PostgreSQL и ключ подписи JWT. Файл исключён из Git. Повторный запуск сохраняет БД и эти значения.

| Роль | Логин | Пароль |
| --- | --- | --- |
| Администратор | `admin@example.test` | `USPKDemo2026Only` |
| Студент | `student@example.test` | `USPKDemo2026Only` |
| Родитель | `parent@example.test` | `USPKDemo2026Only` |
| Преподаватель | `teacher@example.test` | `USPKDemo2026Only` |

Это **публичный учебный пароль** только для вымышленных аккаунтов в локальной демонстрации. Он не должен использоваться для настоящих учётных записей.

## Ручной запуск и работа в редакторе

Если удобнее видеть каждый процесс отдельно, откройте три окна PowerShell в корне репозитория:

```powershell
# Окно 1: только локальная БД
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\USPK_Project\scripts\demo-init.ps1
```

```powershell
# Окно 2: backend
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\USPK_Project\scripts\demo-backend.ps1
```

```powershell
# Окно 3: frontend
cd .\USPK_Project_Front\USPK_Project_Front
npm.cmd ci
$env:VITE_ADMIN_ROLE = 'admin'
npm.cmd run dev -- --host 127.0.0.1
```

В Visual Studio 2026 можно открыть `USPK_Project/USPK_Project.slnx` или `.csproj` для просмотра и отладки C#. Для запуска демонстрации проще использовать скрипт backend: он подгружает локальные параметры `.env.demo`. В VS Code откройте корень репозитория и используйте встроенные терминалы с командами выше.

Проверка сборки:

```powershell
dotnet build .\USPK_Project\USPK_Project\USPK_Project.csproj
cd .\USPK_Project_Front\USPK_Project_Front
npm.cmd ci
npm.cmd run lint
npm.cmd run build
```

## Остановка и сброс

Закройте окна backend/frontend сочетанием `Ctrl+C`. Из корня репозитория остановите только этот контейнер БД:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\stop-demo.ps1
```

Чтобы удалить **только учебные данные этой копии**, выполните из `USPK_Project`:

```powershell
cd .\USPK_Project
docker compose --env-file .env.demo -f compose.demo.yml down -v
```

После сброса вновь запустите `start-demo.ps1` из корня. Если `.env.demo` удалён, а том `uspk_teacher_demo_data` остался, скрипт попросит вернуть прежний файл или явно удалить этот демонстрационный том; он не подменяет пароль существующей БД.

## Что проверено

На macOS: сборка backend, `npm ci`, lint и сборка frontend; запуск отдельного PostgreSQL через Compose; вход всех четырёх ролей, `/api/auth/me`, доступ администратора к списку пользователей, запрет доступа для остальных, ошибки анонимного входа и неверного пароля. Конфигурация использует относительные пути и адреса `127.0.0.1`; Windows PowerShell-скрипты написаны для штатного PowerShell 5.1. Полный запуск в Windows на машине преподавателя не проверялся. Windows CI проверяет сборку и синтаксис PowerShell, но не запускает Docker Desktop.
