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

        public static string BaseDir => PathHelper.BaseDir;

        public static string FilePath => PathHelper.ChangeLogFile;

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static List<ChangeEntry> Load()
        {
            lock (_lock)
            {
                var candidates = new[]
                {
                    FilePath,
                    PathHelper.BackupChangeLogFile,
                    PathHelper.PortableChangeLogFile
                };

                foreach (var path in candidates)
                {
                    try
                    {
                        if (!File.Exists(path)) continue;
                        var txt = File.ReadAllText(path);
                        if (string.IsNullOrWhiteSpace(txt)) continue;
                        var result = JsonSerializer.Deserialize<List<ChangeEntry>>(txt, _jsonOpts);
                        if (result != null) return result;
                    }
                    catch { }
                }

                return new List<ChangeEntry>();
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

                    TryCopy(FilePath, PathHelper.BackupChangeLogFile);

                    if (PathHelper.UsePortableCopy)
                        TryCopy(FilePath, PathHelper.PortableChangeLogFile);
                }
                catch { }
            }
        }

        private static void TryCopy(string source, string dest)
        {
            try
            {
                var fullDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(fullDir))
                    Directory.CreateDirectory(fullDir);
                File.Copy(source, dest, true);
            }
            catch { }
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
