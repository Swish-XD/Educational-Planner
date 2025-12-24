using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EducationalPlanner.ViewModels.Logic
{
    using SaveManager;

    public class MainLogic
    {
        private MainForm Form;
        private SaveManager SaveManager;

        public MainLogic(MainForm form)
        {
            Form = form;
            SaveManager = new SaveManager(Form.dataFilePath);
        }

        public void Save()
        {
            SaveManager.SaveData(Form.tasks);
        }

        public void Load()
        {
            Form.isLoading = true;
            var loaded = SaveManager.LoadData();
            Form.tasks = loaded;
            UpdateTasksList();
            Form.isLoading = false;
        }

        public void BtnDelete_Click(object sender, EventArgs e)
        {
            if (Form.selectedTask != null)
            {
                var result = MessageBox.Show($"Удалить задачу '{Form.selectedTask.Title}'?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Form.tasks.Remove(Form.selectedTask);
                    Form.selectedTask = null;
                    ClearForm();
                    UpdateTasksList();
                    Save();
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для удаления",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        public void BtnSave_Click(object sender, EventArgs e)
        {
            Save();
            MessageBox.Show("Данные успешно сохранены!",
                "Сохранение",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (Form.dataGridView.SelectedRows.Count > 0 && !Form.isLoading)
            {
                var row = Form.dataGridView.SelectedRows[0];
                if (row.Tag is StudyTask task)
                {
                    SelectTask(task);
                }
            }
        }

        public void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0) // Колонка "Выполнено" (чекбокс)
            {
                if (Form.dataGridView.Rows[e.RowIndex].Tag is StudyTask task)
                {
                    task.IsCompleted = !task.IsCompleted;
                    UpdateTasksList();
                    Save(); // Сохраняем при изменении статуса
                }
            }
        }

        public void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // При двойном клике по строке активируем редактирование
            if (e.RowIndex >= 0 && e.ColumnIndex > 0)
            {
                Form.txtTitle.Focus();
            }
        }

        public void TxtTitle_TextChanged(object sender, EventArgs e)
        {
            if (Form.selectedTask != null)
            {
                Form.selectedTask.Title = Form.txtTitle.Text;
                UpdateTasksList(); // Обновляем отображение в списке
            }
        }

        public void TxtDescription_TextChanged(object sender, EventArgs e)
        {
            if (Form.selectedTask != null)
            {
                Form.selectedTask.Description = Form.txtDescription.Text;
            }
        }

        public void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Form.selectedTask != null && Form.cmbCategory.SelectedItem != null)
            {
                Form.selectedTask.Category = Form.cmbCategory.SelectedItem.ToString();
                UpdateTasksList();
            }
        }

        public void CmbPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Form.selectedTask != null && Form.cmbPriority.SelectedItem != null)
            {
                Form.selectedTask.Priority = Form.cmbPriority.SelectedItem.ToString();
                UpdateTasksList();
            }
        }

        public void DtpDeadline_ValueChanged(object sender, EventArgs e)
        {
            if (Form.selectedTask != null)
            {
                Form.selectedTask.Deadline = Form.dtpDeadline.Value;
                UpdateTasksList();
            }
        }

        public void ChkCompleted_CheckedChanged(object sender, EventArgs e)
        {
            if (Form.selectedTask != null)
            {
                Form.selectedTask.IsCompleted = Form.chkCompleted.Checked;
                UpdateTasksList();
                Save(); // Сохраняем при изменении статуса
            }
        }

        public void UpdateTasksList()
        {
            Form.dataGridView.Rows.Clear(); // Очищаем весь список

            // ключевое слово foreach позволяет пробежатся по каждому элементу списка отдельно
            foreach (var task in Form.tasks)
            {
                int rowIndex = Form.dataGridView.Rows.Add(
                    task.IsCompleted,
                    task.Title,
                    task.Category,
                    task.Priority,
                    task.Deadline.ToString("dd.MM.yyyy") // Добавляем данные в список
                );
                Form.dataGridView.Rows[rowIndex].Tag = task;

                // Подсветка просроченных задач
                if (task.Deadline < DateTime.Today && !task.IsCompleted)
                {
                    Form.dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightPink;
                }
                else if (task.IsCompleted)
                {
                    Form.dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                }
            }

            UpdateStats(); // Обновляем статистику
        }

        public void UpdateStats()
        {
            int total = Form.tasks.Count; // берем общее количество задач
            int completed = Form.tasks.Count(t => t.IsCompleted); // получаем все выполненые задачи
            int overdue = Form.tasks.Count(t => t.Deadline < DateTime.Today && !t.IsCompleted); // получаем все просроченные задачи

            Form.lblStats.Text = $"Всего задач: {total} | Выполнено: {completed} | Просрочено: {overdue}"; // выводим данные на экран
        }

        public void SelectTask(StudyTask task)
        {
            Form.isLoading = true; // Включаем флаг загрузки

            Form.selectedTask = task;

            if (task != null)
            {
                Form.txtTitle.Text = task.Title;
                Form.txtDescription.Text = task.Description;

                // Устанавливаем категорию
                int categoryIndex = Form.cmbCategory.Items.IndexOf(task.Category);
                if (categoryIndex >= 0)
                    Form.cmbCategory.SelectedIndex = categoryIndex;
                else
                    Form.cmbCategory.SelectedIndex = 8; // "Общее"

                // Устанавливаем приоритет
                int priorityIndex = Form.cmbPriority.Items.IndexOf(task.Priority);
                if (priorityIndex >= 0)
                    Form.cmbPriority.SelectedIndex = priorityIndex;
                else
                    Form.cmbPriority.SelectedIndex = 1; // "Средний"

                Form.dtpDeadline.Value = task.Deadline;
                Form.chkCompleted.Checked = task.IsCompleted;

                // Выделяем строку в DataGridView
                foreach (DataGridViewRow row in Form.dataGridView.Rows)
                {
                    if (row.Tag == task)
                    {
                        row.Selected = true;
                        Form.dataGridView.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }

            Form.isLoading = false; // Выключаем флаг загрузки
        }

        public void ClearForm()
        {
            Form.isLoading = true;

            Form.txtTitle.Text = "";
            Form.txtDescription.Text = "";
            Form.cmbCategory.SelectedIndex = 8;
            Form.cmbPriority.SelectedIndex = 1;
            Form.dtpDeadline.Value = DateTime.Now.AddDays(7);
            Form.chkCompleted.Checked = false;

            Form.isLoading = false;
        }
    }
}
