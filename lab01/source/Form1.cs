using System.Text;
using System.Threading;

namespace source
{
    public partial class Form1 : Form
    {
        public OllamaService service { get; private set; }
        private BaseChat _currentChat;
        private Thread _workerThread;
        private volatile bool _isGenerating;

        public Form1()
        {
            InitializeComponent();
            this.service = new OllamaService("http://localhost:11434");
            //this.service = new OllamaService("176.65.62.122:11434");
            LoadModels();
        }

        private void LoadModels()
        {
            comboBox1.Items.Clear();
            foreach (string name in CreatorFactory.available_models)
                comboBox1.Items.Add(name);

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void CreateChat()
        {
            string modelName = comboBox1.SelectedItem as string;
            if (string.IsNullOrEmpty(modelName))
            {
                MessageBox.Show("Сначала выберите модель!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BaseChatCreator creator = CreatorFactory.GetSuitableCreator(modelName);
            creator.InitOllamaService(service);

            _currentChat = creator.MakeChat(systemPrompt: "Ты - полезный ассистент для помощи пользователям. Отвечай лаконично и по делу.");
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            string userMessage = inputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userMessage)) return;
            if (_isGenerating) return;

            // Создаём чат при первом обращении
            if (_currentChat == null)
                CreateChat();
            if (_currentChat == null) return;

            // Показываем сообщение пользователя
            listBox1.Items.Add("  Вы:  " + userMessage);
            inputTextBox.Clear();

            SetUiLocked(true);

            // Плейсхолдер для ответа
            listBox1.Items.Add("  Ассистент: ");
            int responseIndex = listBox1.Items.Count - 1;

            // Захватываем ссылки для потока
            BaseChat chat = _currentChat;
            string message = userMessage;

            // Запускаем генерацию в отдельном потоке
            _isGenerating = true;
            _workerThread = new Thread(() =>
            {
                WorkerGenerate(chat, message, responseIndex);
            });
            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        // Метод выполняется в фоновом потоке.
        // Синхронно получает токены и через Invoke обновляет UI.
        private void WorkerGenerate(BaseChat chat, string userMessage, int responseIndex)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // Синхронная генерация со стримингом.
                // Коллбэк onToken вызывается на каждый токен.
                chat.GenerateWithStreaming(userMessage, delegate (string token)
                {
                    sb.Append(token);
                    string currentText = sb.ToString();

                    // Обновляем ListBox из UI-потока
                    this.Invoke(new Action(delegate
                    {
                        listBox1.Items[responseIndex] = "  Ассистент: " + currentText;
                        listBox1.TopIndex = responseIndex;
                    }));
                });

                // Генерация завершена
                this.Invoke(new Action(delegate
                {
                    listBox1.Items.Add("───────────────────────────────────────");
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(delegate
                {
                    listBox1.Items.Add("  Ошибка: " + ex.Message);
                    listBox1.Items.Add("  Убедитесь, что Ollama запущена (ollama serve)");
                }));
            }
            finally
            {
                this.Invoke(new Action(delegate
                {
                    _isGenerating = false;
                    SetUiLocked(false);
                    inputTextBox.Focus();
                }));
            }
        }

        /* ────── вспомогательные обработчики ────── */

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            if (_isGenerating) return;
            listBox1.Items.Clear();
            if (_currentChat != null)
                _currentChat.ClearContext();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isGenerating) return;
            _currentChat = null;
            listBox1.Items.Clear();
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendBtn.PerformClick();
            }
        }

        private void SetUiLocked(bool locked)
        {
            SendBtn.Enabled = !locked;
            inputTextBox.Enabled = !locked;
            comboBox1.Enabled = !locked;
            ClearBtn.Enabled = !locked;
        }
    }
}