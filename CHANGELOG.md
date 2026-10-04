# Changelog

Все значимые изменения проекта.
Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/),
версии — по [Semantic Versioning](https://semver.org/lang/ru/).

## [Unreleased]
### В планах
- Диспетчер задач
- Редактор реестра
- Сканер закрепа malware
- Оффлайн-доступы

## [1.1.0] — 2026-10-05
### Добавлено
- Сплэш-скрин при запуске
- Окно «О программе» с кнопками связи
- Журнал изменений с откатом (`ChangeLog.cs`, `LogForm.cs`)
- Карантин файлов и бэкапы реестра (`BackupManager.cs`)
- Кастомный диалог подтверждения (`ConfirmDialog.cs`)
- DPI-масштабирование (100% / 125% / 150%, WinRE)
- Версия подтягивается из сборки в статус-бар

### Изменено
- Репозиторий переименован в `DarknessTool`
- Обновлён README, добавлены бейджи и таблица сравнения

## [1.0.1] — 2026-10-04
### Исправлено
- Автосборка релиза (`permissions: contents: write` в workflow)

## [1.0.0] — 2026-10-04
### Добавлено
- Первый релиз
- Главный экран с 12 плитками
- Тёмно-синий фон с треугольниками
- Автосборка через GitHub Actions

[Unreleased]: https://github.com/darkjoons3-hue/DarknessTool/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/darkjoons3-hue/DarknessTool/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/darkjoons3-hue/DarknessTool/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/darkjoons3-hue/DarknessTool/releases/tag/v1.0.0
