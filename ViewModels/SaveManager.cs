using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Windows.Forms;
using EducationalPlanner.ViewModels.Logic;

namespace EducationalPlanner.ViewModels.SaveManager
{
    public class SaveManager
    {
        private MainForm Form;
        private MainLogic Logic;

        public string dataFilePath;

        public SaveManager(string path)
        {
            dataFilePath = path;
        }

        public void SaveData(List<StudyTask> tasks)
        {
            var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(dataFilePath, json);
        }

        public List<StudyTask> LoadData()
        {
            if (!File.Exists(dataFilePath))
                return new List<StudyTask>();

            var json = File.ReadAllText(dataFilePath);
            return JsonSerializer.Deserialize<List<StudyTask>>(json) ?? new List<StudyTask>();
        }
    }
}
