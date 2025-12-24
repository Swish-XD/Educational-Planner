using EducationalPlanner.ViewModels.Logic;
using EducationalPlanner.ViewModels.SaveManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EducationalPlanner
{
    public partial class MainForm : Form
    {
        // Элементы управления
        public List<StudyTask> tasks = new List<StudyTask>();
        public StudyTask selectedTask = null;
        public string dataFilePath = "tasks.json";
        public bool isLoading = false; // Флаг, чтобы не сохранять при загрузке

        public DataGridView dataGridView;
        public Label lblStats;
        public TextBox txtTitle;
        public TextBox txtDescription;
        public ComboBox cmbCategory;
        public ComboBox cmbPriority;
        public DateTimePicker dtpDeadline;
        public CheckBox chkCompleted;

        private MainLogic Logic;

        public MainForm()
        {
            Logic = new MainLogic(this);
            SetupForm();
            Logic.Load();
        }

        private void SetupForm()
        {
            this.Text = "Учебный Планировщик";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Верхняя панель управления
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(240, 240, 240) };

            var btnAdd = new Button { Text = "➕ Добавить задачу", Location = new Point(10, 15), Size = new Size(150, 30), Font = new Font("Arial", 9) };
            btnAdd.Click += BtnAdd_Click;

            var btnDelete = new Button { Text = "🗑️ Удалить", Location = new Point(170, 15), Size = new Size(120, 30), Font = new Font("Arial", 9) };
            btnDelete.Click += Logic.BtnDelete_Click;

            var btnSave = new Button { Text = "💾 Сохранить всё", Location = new Point(300, 15), Size = new Size(120, 30), Font = new Font("Arial", 9) };
            btnSave.Click += Logic.BtnSave_Click;

            var chkAutoSave = new CheckBox { Text = "Автосохранение", Location = new Point(430, 20), Size = new Size(120, 20), Checked = true };

            panelTop.Controls.AddRange(new Control[] { btnAdd, btnDelete, btnSave, chkAutoSave });

            // SplitContainer
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 400, // ширина левой панели
                FixedPanel = FixedPanel.Panel1,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Левая панель - список задач + статистика
            var leftPanel = new Panel { Dock = DockStyle.Fill };

            // DataGridView
            dataGridView = new DataGridView
            {
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Dock = DockStyle.Fill
            };

            // Настраиваем колонки
            dataGridView.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "✓", Name = "colCompleted", Width = 40 });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Задача", Name = "colTitle" });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Категория", Name = "colCategory", Width = 100 });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Приоритет", Name = "colPriority", Width = 80 });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Срок", Name = "colDeadline", Width = 100 });

            dataGridView.Columns[0].Width = 40; // чекбокс
            dataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            for (int i = 1; i < dataGridView.Columns.Count; i++)
            {
                dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            dataGridView.SelectionChanged += Logic.DataGridView_SelectionChanged;
            dataGridView.CellClick += Logic.DataGridView_CellClick;
            dataGridView.CellDoubleClick += Logic.DataGridView_CellDoubleClick;

            // Статистика
            var statsPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(230, 230, 230) };
            lblStats = new Label { Text = "Всего задач: 0", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 10, FontStyle.Bold) };
            statsPanel.Controls.Add(lblStats);

            leftPanel.Controls.Add(dataGridView);
            leftPanel.Controls.Add(statsPanel);

            // Правая панель - редактирование
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding(20) };
            var yPos = 0;

            var lblEditTitle = new Label { Text = "Редактирование задачи:", Font = new Font("Arial", 14, FontStyle.Bold), AutoSize = true, Location = new Point(0, yPos) };
            rightPanel.Controls.Add(lblEditTitle);
            yPos += 40;

            // Поля редактирования
            AddLabel(rightPanel, "Название задачи:", ref yPos);
            txtTitle = new TextBox { Location = new Point(0, yPos), Size = new Size(300, 25), Font = new Font("Arial", 10) };
            txtTitle.TextChanged += Logic.TxtTitle_TextChanged;
            txtTitle.LostFocus += (s, e) => Logic.Save();
            rightPanel.Controls.Add(txtTitle);
            yPos += 30;

            AddLabel(rightPanel, "Описание:", ref yPos);
            txtDescription = new TextBox { Location = new Point(0, yPos), Size = new Size(300, 80), Multiline = true, Font = new Font("Arial", 10), ScrollBars = ScrollBars.Vertical };
            txtDescription.TextChanged += Logic.TxtDescription_TextChanged;
            txtDescription.LostFocus += (s, e) => Logic.Save();
            rightPanel.Controls.Add(txtDescription);
            yPos += 90;

            AddLabel(rightPanel, "Категория:", ref yPos);
            cmbCategory = new ComboBox { Location = new Point(0, yPos), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Arial", 10) };
            cmbCategory.Items.AddRange(new string[] { "Математика", "Физика", "Информатика", "Литература", "Иностранный язык", "История", "Биология", "Химия", "Общее" });
            cmbCategory.SelectedIndex = 8;
            cmbCategory.SelectedIndexChanged += Logic.CmbCategory_SelectedIndexChanged;
            cmbCategory.LostFocus += (s, e) => Logic.Save();
            rightPanel.Controls.Add(cmbCategory);
            yPos += 40;

            AddLabel(rightPanel, "Приоритет:", ref yPos);
            cmbPriority = new ComboBox { Location = new Point(0, yPos), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Arial", 10) };
            cmbPriority.Items.AddRange(new string[] { "Высокий", "Средний", "Низкий" });
            cmbPriority.SelectedIndex = 1;
            cmbPriority.SelectedIndexChanged += Logic.CmbPriority_SelectedIndexChanged;
            cmbPriority.LostFocus += (s, e) => Logic.Save();
            rightPanel.Controls.Add(cmbPriority);
            yPos += 40;

            AddLabel(rightPanel, "Срок выполнения:", ref yPos);
            dtpDeadline = new DateTimePicker { Location = new Point(0, yPos), Size = new Size(200, 25), Font = new Font("Arial", 10), Format = DateTimePickerFormat.Short };
            dtpDeadline.ValueChanged += Logic.DtpDeadline_ValueChanged;
            dtpDeadline.LostFocus += (s, e) => Logic.Save();
            rightPanel.Controls.Add(dtpDeadline);
            yPos += 40;

            chkCompleted = new CheckBox { Text = "Задача выполнена", Location = new Point(0, yPos), AutoSize = true, Font = new Font("Arial", 10) };
            chkCompleted.CheckedChanged += Logic.ChkCompleted_CheckedChanged;
            chkCompleted.LostFocus += (s, e) => Logic.Save();
            rightPanel.Controls.Add(chkCompleted);

            splitContainer.Panel1.Controls.Add(leftPanel);
            splitContainer.Panel2.Controls.Add(rightPanel);

            statsPanel.Dock = DockStyle.Bottom;
            dataGridView.Dock = DockStyle.Fill;

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
            Logic.UpdateTasksList();
            Logic.SelectTask(task);
            Logic.Save(); // Сохраняем сразу после добавления
        }
    }
}