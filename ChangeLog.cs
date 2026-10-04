using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DarknessTool
{
    public enum ChangeType
    {
        RegistrySet,
        RegistryDelete,
        FileQuarantine,
        FileDelete,
        ServiceDisable,
        ServiceDelete,
        TaskDisable,
        TaskDelete,
        BlockRemoved,
        Other
    }

    public class ChangeEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 12);
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public ChangeType Type { get; set; }
        public string Target { get; set; } = "";
        public string Description { get; set; } = "";
        public string BackupPath { get; set; } = "";
        public string OriginalPath { get; set; } = "";
        public bool CanUndo { get; set; } = true;
        public bool Undone { get; set; } = false;

        [JsonIgnore]
        public string TypeDisplay => Type switch
        {
            ChangeType.RegistrySet      => "Реестр: изменить",
            ChangeType.RegistryDelete   => "Реестр: удалить",
            ChangeType.FileQuarantine   => "Файл → карантин",
            ChangeType.FileDelete       => "Файл: удалить",
            ChangeType.ServiceDisable   => "Служба: отключить",
            ChangeType.ServiceDelete    => "Служба: удалить",
            ChangeType.TaskDisable      => "Задача: отключить",
            ChangeType.TaskDelete       => "Задача: удалить",
            ChangeType.BlockRemoved     => "Блокировка снята",
            _                            => "Другое"
        };
    }

    public static class ChangeLog
    {
        private static readonly object _lock = new object();

        public static string BaseDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarknessTool");

        public static string FilePath => Path.Combine(BaseDir, "changelog.json");

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static List<ChangeEntry> Load()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(FilePath)) return new List<ChangeEntry>();
                    var txt = File.ReadAllText(FilePath);
                    if (string.IsNullOrWhiteSpace(txt)) return new List<ChangeEntry>();
                    return JsonSerializer.Deserialize<List<ChangeEntry>>(txt, _jsonOpts)
                           ?? new List<ChangeEntry>();
                }
                catch
                {
                    return new List<ChangeEntry>();
                }
            }
        }

        public static void Save(List<ChangeEntry> entries)
        {
            lock (_lock)
            {
                try
                {
                    Directory.CreateDirectory(BaseDir);
                    var txt = JsonSerializer.Serialize(entries, _jsonOpts);
                    File.WriteAllText(FilePath, txt);
                }
                catch { }
            }
        }

        public static void Append(ChangeEntry entry)
        {
            var list = Load();
            list.Add(entry);
            Save(list);
        }

        public static void MarkUndone(string id)
        {
            var list = Load();
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.Undone = true;
                Save(list);
            }
        }

        public static void Clear()
        {
            Save(new List<ChangeEntry>());
        }

        public static ChangeEntry? GetLastUndoable()
        {
            return Load()
                .Where(x => x.CanUndo && !x.Undone)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefault();
        }
    }
}
