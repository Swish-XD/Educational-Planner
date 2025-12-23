using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace EducationalPlanner
{
    public partial class MainForm : Form
    {
        private List<StudyTask> tasks = new List<StudyTask>();
        private StudyTask selectedTask = null;
        private string dataFilePath = "tasks.json";
        private bool isLoading = false; // Флаг, чтобы не сохранять при загрузке

        // Элементы управления
        private DataGridView dataGridView;
        private Label lblStats;
        private TextBox txtTitle;
        private TextBox txtDescription;
        private ComboBox cmbCategory;
        private ComboBox cmbPriority;
        private DateTimePicker dtpDeadline;
        private CheckBox chkCompleted;

        public MainForm()
        {
            SetupForm();
            LoadData();
        }

        private void SetupForm()
        {
            this.Text = "Учебный Планировщик";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель управления сверху
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(240, 240, 240) };

            var btnAdd = new Button
            {
                Text = "➕ Добавить задачу",
                Location = new Point(10, 15),
                Size = new Size(150, 30),
                Font = new Font("Arial", 9)
            };
            btnAdd.Click += BtnAdd_Click;

            var btnDelete = new Button
            {
                Text = "🗑️ Удалить",
                Location = new Point(170, 15),
                Size = new Size(120, 30),
                Font = new Font("Arial", 9)
            };
            btnDelete.Click += BtnDelete_Click;

            var btnSave = new Button
            {
                Text = "💾 Сохранить всё",
                Location = new Point(300, 15),
                Size = new Size(120, 30),
                Font = new Font("Arial", 9)
            };
            btnSave.Click += BtnSave_Click;

            // Кнопка автосохранения
            var chkAutoSave = new CheckBox
            {
                Text = "Автосохранение",
                Location = new Point(430, 20),
                Size = new Size(120, 20),
                Checked = true
            };

            panelTop.Controls.AddRange(new Control[] { btnAdd, btnDelete, btnSave, chkAutoSave });

            // Разделитель экрана
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 600,
                FixedPanel = FixedPanel.Panel1,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Левая панель - список задач
            var leftPanel = new Panel { Dock = DockStyle.Fill };

            // DataGridView для задач
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            // Настраиваем колонки
            dataGridView.Columns.Add(new DataGridViewCheckBoxColumn
            {
                HeaderText = "✓",
                Name = "colCompleted",
                Width = 40
            });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Задача",
                Name = "colTitle"
            });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Категория",
                Name = "colCategory",
                Width = 100
            });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Приоритет",
                Name = "colPriority",
                Width = 80
            });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Срок",
                Name = "colDeadline",
                Width = 100
            });

            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            dataGridView.CellClick += DataGridView_CellClick;
            dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;

            leftPanel.Controls.Add(dataGridView);

            // Панель статистики
            var statsPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(230, 230, 230) };
            lblStats = new Label
            {
                Text = "Всего задач: 0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            statsPanel.Controls.Add(lblStats);
            leftPanel.Controls.Add(statsPanel);

            // Правая панель - редактирование
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 249, 250) };

            var editPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblEditTitle = new Label
            {
                Text = "Редактирование задачи:",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(0, 0),
                AutoSize = true
            };

            // Поля редактирования
            int yPos = 40;

            // Название задачи
            AddLabel(editPanel, "Название задачи:", ref yPos);
            txtTitle = new TextBox
            {
                Location = new Point(0, yPos),
                Size = new Size(300, 25),
                Font = new Font("Arial", 10)
            };
            txtTitle.TextChanged += TxtTitle_TextChanged;
            txtTitle.LostFocus += (s, e) => AutoSave();
            editPanel.Controls.Add(txtTitle);
            yPos += 30;

            // Описание
            AddLabel(editPanel, "Описание:", ref yPos);
            txtDescription = new TextBox
            {
                Location = new Point(0, yPos),
                Size = new Size(300, 80),
                Multiline = true,
                Font = new Font("Arial", 10),
                ScrollBars = ScrollBars.Vertical
            };
            txtDescription.TextChanged += TxtDescription_TextChanged;
            txtDescription.LostFocus += (s, e) => AutoSave();
            editPanel.Controls.Add(txtDescription);
            yPos += 90;

            // Категория
            AddLabel(editPanel, "Категория:", ref yPos);
            cmbCategory = new ComboBox
            {
                Location = new Point(0, yPos),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Arial", 10)
            };
            cmbCategory.Items.AddRange(new string[]
            {
                "Математика", "Физика", "Информатика",
                "Литература", "Иностранный язык",
                "История", "Биология", "Химия", "Общее"
            });
            cmbCategory.SelectedIndex = 8;
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            cmbCategory.LostFocus += (s, e) => AutoSave();
            editPanel.Controls.Add(cmbCategory);
            yPos += 40;

            // Приоритет
            AddLabel(editPanel, "Приоритет:", ref yPos);
            cmbPriority = new ComboBox
            {
                Location = new Point(0, yPos),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Arial", 10)
            };
            cmbPriority.Items.AddRange(new string[] { "Высокий", "Средний", "Низкий" });
            cmbPriority.SelectedIndex = 1;
            cmbPriority.SelectedIndexChanged += CmbPriority_SelectedIndexChanged;
            cmbPriority.LostFocus += (s, e) => AutoSave();
            editPanel.Controls.Add(cmbPriority);
            yPos += 40;

            // Срок выполнения
            AddLabel(editPanel, "Срок выполнения:", ref yPos);
            dtpDeadline = new DateTimePicker
            {
                Location = new Point(0, yPos),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10),
                Format = DateTimePickerFormat.Short
            };
            dtpDeadline.ValueChanged += DtpDeadline_ValueChanged;
            dtpDeadline.LostFocus += (s, e) => AutoSave();
            editPanel.Controls.Add(dtpDeadline);
            yPos += 40;

            // Статус выполнения
            chkCompleted = new CheckBox
            {
                Text = "Задача выполнена",
                Location = new Point(0, yPos),
                AutoSize = true,
                Font = new Font("Arial", 10)
            };
            chkCompleted.CheckedChanged += ChkCompleted_CheckedChanged;
            chkCompleted.LostFocus += (s, e) => AutoSave();
            editPanel.Controls.Add(chkCompleted);

            editPanel.Controls.Add(lblEditTitle);
            rightPanel.Controls.Add(editPanel);

            splitContainer.Panel1.Controls.Add(leftPanel);
            splitContainer.Panel2.Controls.Add(rightPanel);

            // Добавляем все на форму
            this.Controls.Add(splitContainer);
            this.Controls.Add(panelTop);
        }

        private void AddLabel(Panel panel, string text, ref int yPos)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(0, yPos),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(64, 64, 64)
            };
            panel.Controls.Add(label);
            yPos += 25;
        }

        // Обработчики событий
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var task = new StudyTask();
            tasks.Add(task);
            UpdateTasksList();
            SelectTask(task);
            SaveData(); // Сохраняем сразу после добавления
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedTask != null)
            {
                var result = MessageBox.Show($"Удалить задачу '{selectedTask.Title}'?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    tasks.Remove(selectedTask);
                    selectedTask = null;
                    ClearForm();
                    UpdateTasksList();
                    SaveData();
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

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveData();
            MessageBox.Show("Данные успешно сохранены!",
                "Сохранение",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0 && !isLoading)
            {
                var row = dataGridView.SelectedRows[0];
                if (row.Tag is StudyTask task)
                {
                    SelectTask(task);
                }
            }
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0) // Колонка "Выполнено" (чекбокс)
            {
                if (dataGridView.Rows[e.RowIndex].Tag is StudyTask task)
                {
                    task.IsCompleted = !task.IsCompleted;
                    UpdateTasksList();
                    SaveData(); // Сохраняем при изменении статуса
                }
            }
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // При двойном клике по строке активируем редактирование
            if (e.RowIndex >= 0 && e.ColumnIndex > 0)
            {
                txtTitle.Focus();
            }
        }

        private void TxtTitle_TextChanged(object sender, EventArgs e)
        {
            if (selectedTask != null && !isLoading)
            {
                selectedTask.Title = txtTitle.Text;
                UpdateTasksList(); // Обновляем отображение в списке
            }
        }

        private void TxtDescription_TextChanged(object sender, EventArgs e)
        {
            if (selectedTask != null && !isLoading)
            {
                selectedTask.Description = txtDescription.Text;
            }
        }

        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedTask != null && !isLoading && cmbCategory.SelectedItem != null)
            {
                selectedTask.Category = cmbCategory.SelectedItem.ToString();
                UpdateTasksList();
            }
        }

        private void CmbPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedTask != null && !isLoading && cmbPriority.SelectedItem != null)
            {
                selectedTask.Priority = cmbPriority.SelectedItem.ToString();
                UpdateTasksList();
            }
        }

        private void DtpDeadline_ValueChanged(object sender, EventArgs e)
        {
            if (selectedTask != null && !isLoading)
            {
                selectedTask.Deadline = dtpDeadline.Value;
                UpdateTasksList();
            }
        }

        private void ChkCompleted_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedTask != null && !isLoading)
            {
                selectedTask.IsCompleted = chkCompleted.Checked;
                UpdateTasksList();
                SaveData(); // Сохраняем при изменении статуса
            }
        }

        private void UpdateTasksList()
        {
            if (isLoading) return; // в случае если во время загрузки вызовется метод UpdateTasksList то ничего не пройзойдет

            dataGridView.Rows.Clear(); // Очищаем весь список

            // ключевое слово foreach позволяет пробежатся по каждому элементу списка отдельно
            foreach (var task in tasks)
            {
                int rowIndex = dataGridView.Rows.Add(
                    task.IsCompleted,
                    task.Title,
                    task.Category,
                    task.Priority,
                    task.Deadline.ToString("dd.MM.yyyy") // Добавляем данные в список
                );
                dataGridView.Rows[rowIndex].Tag = task;

                // Подсветка просроченных задач
                if (task.Deadline < DateTime.Today && !task.IsCompleted)
                {
                    dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightPink;
                }
                else if (task.IsCompleted)
                {
                    dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                }
            }

            UpdateStats(); // Обновляем статистику
        }

        private void UpdateStats()
        {
            int total = tasks.Count; // берем общее количество задач
            int completed = tasks.Count(t => t.IsCompleted); // получаем все выполненые задачи
            int overdue = tasks.Count(t => t.Deadline < DateTime.Today && !t.IsCompleted); // получаем все просроченные задачи

            lblStats.Text = $"Всего задач: {total} | Выполнено: {completed} | Просрочено: {overdue}"; // выводим данные на экран
        }

        private void SelectTask(StudyTask task)
        {
            isLoading = true; // Включаем флаг загрузки

            selectedTask = task;

            if (task != null)
            {
                txtTitle.Text = task.Title;
                txtDescription.Text = task.Description;

                // Устанавливаем категорию
                int categoryIndex = cmbCategory.Items.IndexOf(task.Category);
                if (categoryIndex >= 0)
                    cmbCategory.SelectedIndex = categoryIndex;
                else
                    cmbCategory.SelectedIndex = 8; // "Общее"

                // Устанавливаем приоритет
                int priorityIndex = cmbPriority.Items.IndexOf(task.Priority);
                if (priorityIndex >= 0)
                    cmbPriority.SelectedIndex = priorityIndex;
                else
                    cmbPriority.SelectedIndex = 1; // "Средний"

                dtpDeadline.Value = task.Deadline;
                chkCompleted.Checked = task.IsCompleted;

                // Выделяем строку в DataGridView
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Tag == task)
                    {
                        row.Selected = true;
                        dataGridView.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }

            isLoading = false; // Выключаем флаг загрузки
        }

        private void ClearForm()
        {
            isLoading = true;

            txtTitle.Text = "";
            txtDescription.Text = "";
            cmbCategory.SelectedIndex = 8;
            cmbPriority.SelectedIndex = 1;
            dtpDeadline.Value = DateTime.Now.AddDays(7);
            chkCompleted.Checked = false;

            isLoading = false;
        }

        private void AutoSave()
        {
            if (!isLoading && selectedTask != null)
            {
                SaveData();
            }
        }

        /// <summary>
        /// Данная функция сохраняет необходимые данные в Json файл
        /// </summary>
        private void SaveData()
        {
            try // Ключевое слово для попытки выполнить кусок кода
            {
                var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions // Функция которая создает переменную string в формате json 
                {
                    WriteIndented = true // Улучшает читаемость json документа для обычного пользователя
                });
                File.WriteAllText(dataFilePath, json); // Функция отвечающая за сохранения json файла на диск
            }
            catch (Exception ex) // Ключевое слово если "try" поймает ошибку, таким образом будет выведено диалоговое окно с кодом ошибки
            {
                // Текст ошибки, ex это переменная хранящая в себе Exception (Исключение),
                // в нем есть переменная Message хранящая в себе текст исключения, очень полезно при отладке приложений
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error); // функция для вывода диалогового окна
            }
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    var json = File.ReadAllText(dataFilePath);
                    var loadedTasks = JsonSerializer.Deserialize<List<StudyTask>>(json);

                    if (loadedTasks != null)
                    {
                        tasks = loadedTasks;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            UpdateTasksList();
        }
    }
}