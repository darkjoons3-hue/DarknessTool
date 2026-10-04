# Как помочь проекту

Спасибо, что заглянул! DarknessTool — открытый проект, и любой вклад ценен.

## 🐞 Нашёл баг?

1. Проверь [Issues](https://github.com/darkjoons3-hue/DarknessTool/issues) — возможно, уже сообщили.
2. Если нет — [создай новый багрепорт](https://github.com/darkjoons3-hue/DarknessTool/issues/new?template=bug_report.md).
3. Приложи:
   - Версию DarknessTool (видна в статус-баре).
   - Инфо о системе (ℹ → Скопировать инфо).
   - Скриншот или видео.
   - Шаги воспроизведения.

## 💡 Есть идея?

Открой [Issue с шаблоном «Идея»](https://github.com/darkjoons3-hue/DarknessTool/issues/new?template=feature_request.md) или напиши в [Discussions](https://github.com/darkjoons3-hue/DarknessTool/discussions).

## 💻 Хочешь написать код?

1. **Форкни** репозиторий.
2. Создай ветку: `git checkout -b feature/my-feature`.
3. Пиши код по стилю проекта (см. ниже).
4. **Проверь сборку:** `dotnet build -c Release`.
5. Запусти: `dotnet run`.
6. **Сделай коммит:** `git commit -m "Add: краткое описание"`.
7. **Запушь:** `git push origin feature/my-feature`.
8. **Открой Pull Request** в `main`.

## 📏 Стиль кода

- **C# (.NET 8)**, Windows Forms.
- Отступы: **4 пробела**.
- Имена: `PascalCase` для классов и методов, `_camelCase` для приватных полей.
- **Комментарии — по делу**, без «очевидного» (не пиши `// увеличиваем i на 1`).
- **Никаких магических чисел** — выноси в `const` или `static readonly`.
- Все цвета — через `static readonly Color`, никаких «хардкодных» в методах.
- **DPI-масштабирование обязательно** — все размеры умножать на `DeviceDpi / 96f`.
- **Без внешних библиотек** без обсуждения в Issue.
- Файлы в `namespace DarknessTool`.

## 🎨 UI-принципы

- Тёмно-синяя палитра (`#0A1428` / `#050A14`), акценты `#4A9EFF`, опасное `#D13438`.
- Скругления: плитки — 8 px, кнопки — 6 px.
- Все кастомные окна — с градиентным фоном, как в `MainForm`.
- **Опасные действия** — только через `ConfirmDialog.Ask(..., dangerous: true)`.
- **Каждое изменение системы** — записывать в `ChangeLog` и бэкапить через `BackupManager`.

## 🏗 Архитектура

- `Program.cs` — точка входа, сплэш, обработка ошибок.
- `MainForm.cs` — главное окно, плитки, кнопки в углу.
- `*Form.cs` — отдельные окна (About, Log, Confirm).
- `BackupManager.cs` — бэкап реестра и файлов, карантин, откат.
- `ChangeLog.cs` — журнал изменений.
- `TileControl.cs` — кастомный контрол плитки.

## 🧪 Тестирование

Перед PR обязательно проверь:
- Сборка проходит (`dotnet build -c Release`).
- Запуск на Windows 10 / 11 (хотя бы на одной).
- Корректный вид на 100% и 150% DPI.
- Если менял UI — скриншот «до/после».

## 📜 Лицензия

Отправляя PR, ты соглашаешься, что твой код будет распространяться под **MIT License**.

## ❓ Вопросы

Пиши в [Discussions](https://github.com/darkjoons3-hue/DarknessTool/discussions) или в Issues.

---

**Спасибо, что делаешь проект лучше! 💙**
