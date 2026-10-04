# 🛡️ DarknessTool 1.1

**Portable Windows recovery tool for cleaning infected systems, removing 
restrictions, and safe rollback. Works both in live Windows and WinRE 
(Windows Recovery Environment).**

[![Release](https://img.shields.io/github/v/release/darkjoons3-hue/DarknessTool?label=release)](https://github.com/darkjoons3-hue/DarknessTool/releases/latest)
[![Build](https://github.com/darkjoons3-hue/DarknessTool/actions/workflows/build.yml/badge.svg)](https://github.com/darkjoons3-hue/DarknessTool/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-blue)]()
[![.NET](https://img.shields.io/badge/.NET-self--contained-purple)]()

🇷🇺 [Русская версия](README.md) · 🇬🇧 **English version**

---

## 📥 Download

**[⬇ DarknessTool.exe (latest release)](https://github.com/darkjoons3-hue/DarknessTool/releases/latest/download/DarknessTool.exe)**

- Windows 10 / 11 (x64)
- Administrator rights
- **No .NET required** — self-contained build
- No installation

---

## ✨ Features

| Module | Description |
|---|---|
| 🧩 **Task Manager** | Processes (CPU, RAM, Disk, Network), signature status, VirusTotal verdict, process tree, terminate, freeze, properties, modules, threads, handles |
| 🗝 **Registry Editor** | Full tree view, edit all value types, search, key backup, undo, offline hive support |
| 🗂 **Offline Access** | Services, drivers, scheduled tasks, user profiles — in a non-booted Windows |
| 🔍 **Persistence Scanner** | IFEO, Winlogon, AppInit_DLLs, Run/RunOnce, Startup, tasks, services, drivers, WMI subscriptions, policies, BHO |
| 🔓 **Restrictions Remover** | Unblock Task Manager, Registry Editor, cmd, USB, Autorun, .exe/.reg associations, hosts, etc. |
| 📊 **Registry Diff** | Snapshot "before/after", diff, selective rollback |
| 🔤 **System Fonts** | Reset fonts and associations to defaults |
| 🔑 **Utilman Replacement** | Access locked-out system (with backup) |
| 🗄 **Quarantine** | Every deletion goes to quarantine with restore option |
| 🧪 **PowerShell Scripts** | Custom rules with logging and undo |
| 🔗 **VirusTotal** | Hash check for autoruns and processes (optional) |
| ✍️ **Signature Verification** | Sign status for every file, filter "unsigned only" |
| 📖 **Built-in Help** | Offline reference guide, works in WinRE |

---

## 🚀 Why DarknessTool

- ✅ **Works in WinRE** — clean up a dead system from a USB stick
- ✅ **Offline mode** — services, drivers, tasks, user profiles
- ✅ **Full rollback** — quarantine + change log
- ✅ **Single .exe** — no install, no dependencies
- ✅ **Portable** — leaves nothing behind on the infected machine
- ✅ **Open Source** — MIT

### Comparison with alternatives

| | Process Hacker | Autoruns | Simple Unlocker | AnVir | **DarknessTool** |
|---|:---:|:---:|:---:|:---:|:---:|
| Process monitoring | ✅ | — | — | ✅ | ✅ |
| Autoruns scanning | ✅ | ✅ | — | ✅ | ✅ |
| Malware cleanup | — | — | — | — | ✅ |
| Offline (WinRE) mode | — | — | — | — | ✅ |
| Change rollback | — | — | — | — | ✅ |
| VirusTotal integration | ✅ | — | — | ✅ | ✅ |
| Restrictions removal | — | — | ✅ | — | ✅ |
| Portability | ✅ | ✅ | ✅ | — | ✅ |

---

## 🎯 Use cases

**Cleaning a live system** → Task Manager → Autoruns → Persistence Scanner → Quarantine

**Recovery from USB** → WinRE → Offline Access → Services/Drivers → Reboot

**Regaining access** → WinRE → Restrictions Remover / Utilman Replacement

**Change analysis** → Registry snapshot → Diff → Rollback

---

## 🔐 Security

- Everything runs locally, no telemetry
- Every change is backed up first
- Full rollback via quarantine and change log
- Open source

⚠️ Requests administrator rights. May trigger antivirus false positives — 
download only from the [official releases page](https://github.com/darkjoons3-hue/DarknessTool/releases).

---

## 🗺️ Roadmap

- [x] v1.0 — skeleton, CI/CD, first release
- [x] v1.1 — splash, change log, quarantine, backups, DPI scaling
- [ ] v1.2 — Task Manager
- [ ] v1.3 — Registry Editor
- [ ] v1.4 — Persistence Scanner
- [ ] v1.5 — Restrictions Remover
- [ ] v2.0 — VirusTotal, Help system, localization

---

## 🛠 Build from source

```bash
git clone https://github.com/darkjoons3-hue/DarknessTool.git
cd DarknessTool
dotnet build -c Release
