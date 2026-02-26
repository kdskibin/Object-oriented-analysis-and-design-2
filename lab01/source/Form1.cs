using System.Text;
using System.Threading;

namespace source
{
    public partial class MainForm : Form
    {
        public OllamaService service { get; private set; }
        private BaseChat currentChat;
        private Thread workerThread;
        private volatile bool _isGenerating;

        public MainForm()
        {
            InitializeComponent();
            service = new OllamaService();
            LoadModels();
        }

        private void LoadModels()
        {
            ModelSelector_cb.Items.Clear();
            foreach (string name in CreatorFactory.available_models)
                ModelSelector_cb.Items.Add(name);

            if (ModelSelector_cb.Items.Count > 0)
                ModelSelector_cb.SelectedIndex = 0;
        }

        private void CreateChat()
        {
            string modelName = ModelSelector_cb.SelectedItem as string;
            if (string.IsNullOrEmpty(modelName))
            {
                MessageBox.Show("Сначала выберите модель!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BaseChatCreator creator = CreatorFactory.GetSuitableCreator(modelName);
            creator.InitOllamaService(service);

            currentChat = creator.MakeChat(systemPrompt: "Ты - полезный ассистент для помощи пользователям. Отвечай лаконично и по делу.");
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            string userMessage = InputMessage_tb.Text.Trim();
            if (string.IsNullOrEmpty(userMessage)) return;
            if (_isGenerating) return;

            // Создаём чат при первом обращении
            if (currentChat == null)
                CreateChat();
            if (currentChat == null) return;

            // Показываем сообщение пользователя
            ConversationBox.Font = new Font("Arial", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ConversationBox.Items.Add("  Вы:  " + userMessage);
            InputMessage_tb.Clear();

            SetUiLocked(true);

            // Плейсхолдер для ответа
            ConversationBox.Items.Add("  Ассистент: ");
            ConversationBox.Font = new Font("Cascadia Mono", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            int responseIndex = ConversationBox.Items.Count - 1;

            // Захватываем ссылки для потока
            BaseChat chat = currentChat;
            string message = userMessage;

            // Запускаем генерацию в отдельном потоке
            _isGenerating = true;
            workerThread = new Thread(() =>
            {
                WorkerGenerate(chat, message, responseIndex);
            });
            workerThread.IsBackground = true;
            workerThread.Start();
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
                        ConversationBox.Items[responseIndex] = "  Ассистент: " + currentText;
                        ConversationBox.TopIndex = responseIndex;
                    }));
                });

                // Генерация завершена
                this.Invoke(new Action(delegate
                {
                    ConversationBox.Items.Add("───────────────────────────────────────");
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(delegate
                {
                    ConversationBox.Items.Add("  Ошибка: " + ex.Message);
                    ConversationBox.Items.Add("  Убедитесь, что Ollama запущена (ollama serve)");
                }));
            }
            finally
            {
                this.Invoke(new Action(delegate
                {
                    _isGenerating = false;
                    SetUiLocked(false);
                    InputMessage_tb.Focus();
                }));
            }
        }

        // Вспомогательные обработчики
        private void SetUiLocked(bool locked)
        {
            Send_Btn.Enabled = !locked;
            InputMessage_tb.Enabled = !locked;
            ModelSelector_cb.Enabled = !locked;
            Clear_Btn.Enabled = !locked;
        }

        private void Clear_Btn_Click(object sender, EventArgs e)
        {
            if (_isGenerating) return;
            ConversationBox.Items.Clear();
            if (currentChat != null)
                currentChat.ClearContext();
        }
    }
}