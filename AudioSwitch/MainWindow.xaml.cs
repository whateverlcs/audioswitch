using AudioSwitch.Controllers;
using AudioSwitch.Models;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioSwitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Variável para armazenar a hotkey do teclado no atalho 1.
        /// </summary>
        private Key? Hotkey1Key = null;

        /// <summary>
        /// Variável para armazenar a hotkey do teclado no atalho 1.
        /// </summary>
        private Key? Hotkey2Key = null;

        /// <summary>
        /// Variável para armazenar a hotkey do teclado no atalho 1.
        /// </summary>
        private Key? Hotkey3Key = null;

        /// <summary>
        /// Variável para armazenar a hotkey do mouse no atalho 1.
        /// </summary>
        private MouseButton? Hotkey1MouseButton = null;

        /// <summary>
        /// Variável para armazenar a hotkey do mouse no atalho 2.
        /// </summary>
        private MouseButton? Hotkey2MouseButton = null;

        /// <summary>
        /// Variável para armazenar a hotkey do mouse no atalho 3.
        /// </summary>
        private MouseButton? Hotkey3MouseButton = null;

        /// <summary>
        /// Controla se os atalhos foram salvos
        /// </summary>
        private bool areHotkeysSaved = false;

        /// <summary>
        /// Variável para controlar qual Atalho está sendo configurado, ex: 1: Atalho 1, 2: Atalho 2, 3: Atalho 3
        /// </summary>
        private int hotkeyBeingConfigured = 0;

        /// <summary>
        /// Caminho do arquivo para armazenar as configurações
        /// </summary>
        private string settingsFilePath = "hotkeys.json";

        /// <summary>
        /// Injeção do Controller ControlLogs.
        /// </summary>
        private ControlLogs clog = new ControlLogs();

        public MainWindow()
        {
            InitializeComponent();
            TaskbarIcon.Visibility = Visibility.Visible;

            LoadHotkeys();

            AddMouseEventHandlers();
        }

        /// <summary>
        /// Adiciona os handles do mouse.
        /// </summary>
        private void AddMouseEventHandlers()
        {
            EventManager.RegisterClassHandler(typeof(Window), Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnGlobalMouseButtonPressed));
            EventManager.RegisterClassHandler(typeof(Window), Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnGlobalMouseEvent));
        }

        /// <summary>
        /// Verifica se o botão pressionado do mouse faz parte das hotkeys para alterar o dispositivo de audio.
        /// </summary>
        private void OnGlobalMouseEvent(object sender, MouseButtonEventArgs e)
        {
            CheckHotkeyCombination();
        }

        /// <summary>
        /// Captura o evento de mouse global, por conta de nativamente no WPF ele não identificar botões do mouse, como mouse 3, mouse 4, etc.
        /// </summary>
        private void OnGlobalMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            var textbox = e.Source as TextBox;

            if (textbox == null) { return; }

            // Apenas registra o botão do mouse durante a configuração
            if (hotkeyBeingConfigured != 0)
            {
                string mouseButton = e.ChangedButton.ToString(); // Identifica qual botão do mouse foi pressionado

                if (mouseButton == "Left" || mouseButton == "Right" || mouseButton == "Middle") { return; }

                if (hotkeyBeingConfigured == 1)
                {
                    areHotkeysSaved = Hotkey1MouseButton == e.ChangedButton;
                    Hotkey1MouseButton = e.ChangedButton;
                    Hotkey1TextBox.Text = mouseButton.ToString();
                }
                else if (hotkeyBeingConfigured == 2)
                {
                    areHotkeysSaved = Hotkey2MouseButton == e.ChangedButton;
                    Hotkey2MouseButton = e.ChangedButton;
                    Hotkey2TextBox.Text = mouseButton.ToString();
                }
                else if (hotkeyBeingConfigured == 3)
                {
                    areHotkeysSaved = Hotkey3MouseButton == e.ChangedButton;
                    Hotkey3MouseButton = e.ChangedButton;
                    Hotkey3TextBox.Text = mouseButton.ToString();
                }

                this.Focus();

                CheckHotkeyCombination();

                hotkeyBeingConfigured = 0;
            }
        }

        /// <summary>
        /// Evento para minimizar a janela para a bandeja
        /// </summary>
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        /// <summary>
        /// Evento para restaurar a janela
        /// </summary>
        private void OnShowWindowClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        /// <summary>
        /// Evento de sair do programa
        /// </summary>
        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Método disparado para a atribuição do atalho 1
        /// </summary>
        private void Hotkey1Click(object sender, MouseButtonEventArgs e)
        {
            hotkeyBeingConfigured = 1;
            WaitForKeyPress();
        }

        /// <summary>
        /// Método disparado para a atribuição do atalho 2
        /// </summary>
        private void Hotkey2Click(object sender, MouseButtonEventArgs e)
        {
            hotkeyBeingConfigured = 2;
            WaitForKeyPress();
        }

        /// <summary>
        /// Método disparado para a atribuição do atalho 3
        /// </summary>
        private void Hotkey3Click(object sender, MouseButtonEventArgs e)
        {
            hotkeyBeingConfigured = 3;
            WaitForKeyPress();
        }

        /// <summary>
        /// Método para esperar o usuário pressionar uma tecla e atribuir ao Atalho
        /// </summary>
        private void WaitForKeyPress()
        {
            KeyDown += OnKeyPressed;
        }

        /// <summary>
        /// Trata a detecção de teclas do teclado, atribuindo a tecla ao Atalho correspondente com base na variável spaceBeingConfigured.
        /// </summary>
        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (hotkeyBeingConfigured == 1)
            {
                Hotkey1Key = e.Key;
                Hotkey1TextBox.Text = Hotkey1Key.ToString();
            }
            else if (hotkeyBeingConfigured == 2)
            {
                Hotkey2Key = e.Key;
                Hotkey2TextBox.Text = Hotkey2Key.ToString();
            }
            else if (hotkeyBeingConfigured == 3)
            {
                Hotkey3Key = e.Key;
                Hotkey3TextBox.Text = Hotkey3Key.ToString();
            }

            ResetConfiguration();
        }

        /// <summary>
        /// Reseta a configuração atual, removendo os eventos associados.
        /// </summary>
        private void ResetConfiguration()
        {
            KeyDown -= OnKeyPressed;
            hotkeyBeingConfigured = 0;
        }

        /// <summary>
        /// Método para capturar teclas do teclado
        /// </summary>
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (hotkeyBeingConfigured == 1)
            {
                Hotkey1Key = e.Key;
                Hotkey1MouseButton = null;
            }
            else if (hotkeyBeingConfigured == 2)
            {
                Hotkey2Key = e.Key;
                Hotkey2MouseButton = null;
            }
            else if (hotkeyBeingConfigured == 3)
            {
                Hotkey3Key = e.Key;
                Hotkey3MouseButton = null;
            }

            hotkeyBeingConfigured = 0;
            UnsubscribeKeyAndMouseHandlers();
        }

        /// <summary>
        /// Método para cancelar a inscrição dos eventos de teclado e mouse após o atalho ser configurado
        /// </summary>
        private void UnsubscribeKeyAndMouseHandlers()
        {
            KeyDown -= OnKeyDownHandler;
        }

        /// <summary>
        /// Verifica a combinação dos atalhos salvos para alterar o dispositivo de áudio
        /// </summary>
        private void CheckHotkeyCombination()
        {
            if (!areHotkeysSaved) return;

            bool isHotkey1Pressed = (Hotkey1Key.HasValue && Keyboard.IsKeyDown(Hotkey1Key.Value)) ||
                                    (Hotkey1MouseButton.HasValue && MouseButtonIsPressed(Hotkey1MouseButton.Value));
            bool isHotkey2Pressed = (Hotkey2Key.HasValue && Keyboard.IsKeyDown(Hotkey2Key.Value)) ||
                                    (Hotkey2MouseButton.HasValue && MouseButtonIsPressed(Hotkey2MouseButton.Value));
            bool isHotkey3Pressed = (Hotkey3Key.HasValue && Keyboard.IsKeyDown(Hotkey3Key.Value)) ||
                                    (Hotkey3MouseButton.HasValue && MouseButtonIsPressed(Hotkey3MouseButton.Value));

            // Contar quantos atalhos foram definidos
            int definedHotkeysCount = 0;
            if (Hotkey1Key.HasValue || Hotkey1MouseButton.HasValue) definedHotkeysCount++;
            if (Hotkey2Key.HasValue || Hotkey2MouseButton.HasValue) definedHotkeysCount++;
            if (Hotkey3Key.HasValue || Hotkey3MouseButton.HasValue) definedHotkeysCount++;

            // Condições para permitir a troca do dispositivo de áudio, dependendo do número de atalhos definidos
            if (definedHotkeysCount == 1 && (isHotkey1Pressed || isHotkey2Pressed || isHotkey3Pressed))
            {
                SwitchAudioDevice();
            }
            else if (definedHotkeysCount == 2 && ((isHotkey1Pressed && isHotkey2Pressed) || (isHotkey1Pressed && isHotkey3Pressed) || (isHotkey2Pressed && isHotkey3Pressed)))
            {
                SwitchAudioDevice();
            }
            else if (definedHotkeysCount == 3 && (isHotkey1Pressed && isHotkey2Pressed && isHotkey3Pressed))
            {
                SwitchAudioDevice();
            }
        }

        /// <summary>
        /// Método para verificar qual botão do mouse foi pressionado e se ele foi pressionado ou não
        /// </summary>
        private bool MouseButtonIsPressed(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return Mouse.LeftButton == MouseButtonState.Pressed;

                case MouseButton.Right:
                    return Mouse.RightButton == MouseButtonState.Pressed;

                case MouseButton.Middle:
                    return Mouse.MiddleButton == MouseButtonState.Pressed;

                case MouseButton.XButton1:
                    return Mouse.XButton1 == MouseButtonState.Pressed;

                case MouseButton.XButton2:
                    return Mouse.XButton2 == MouseButtonState.Pressed;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Método que realiza a alteração do dispositivo de áudio atual
        /// </summary>
        private void SwitchAudioDevice()
        {
            try
            {
                var audioController = new AudioSwitcher.AudioApi.CoreAudio.CoreAudioController();
                var playbackDevices = audioController.GetPlaybackDevices(AudioSwitcher.AudioApi.DeviceState.Active).ToList();
                var defaultDevice = audioController.DefaultPlaybackDevice;

                if (playbackDevices.Count == 0)
                {
                    MessageBox.Show("Nenhum dispositivo de reprodução habilitado foi encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var currentIndex = playbackDevices.FindIndex(d => d.Id == defaultDevice.Id);
                var nextDevice = playbackDevices[(currentIndex + 1) % playbackDevices.Count];
                nextDevice.SetAsDefault();

                ShowNotification($"Dispositivo alterado para: {nextDevice.FullName}", "Alteração de Dispositivo de Áudio");
            }
            catch (Exception e)
            {
                clog.LogException(e.ToString(), "SwitchAudioDevice()");
                MessageBox.Show("Ocorreu um erro ao alterar o dispositivo atual de audio, favor tentar novamente em instantes.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Método que realiza o carregamento dos atalhos salvos pelo usuário
        /// </summary>
        private void LoadHotkeys()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    var json = File.ReadAllText(settingsFilePath);
                    var settings = JsonConvert.DeserializeObject<HotkeySettings>(json);

                    Hotkey1Key = !string.IsNullOrEmpty(settings.Hotkey1Key) ? (Key)Enum.Parse(typeof(Key), settings.Hotkey1Key) : (Key?)null;
                    Hotkey1MouseButton = !string.IsNullOrEmpty(settings.Hotkey1MouseButton) ? (MouseButton)Enum.Parse(typeof(MouseButton), settings.Hotkey1MouseButton) : (MouseButton?)null;

                    Hotkey2Key = !string.IsNullOrEmpty(settings.Hotkey2Key) ? (Key)Enum.Parse(typeof(Key), settings.Hotkey2Key) : (Key?)null;
                    Hotkey2MouseButton = !string.IsNullOrEmpty(settings.Hotkey2MouseButton) ? (MouseButton)Enum.Parse(typeof(MouseButton), settings.Hotkey2MouseButton) : (MouseButton?)null;

                    Hotkey3Key = !string.IsNullOrEmpty(settings.Hotkey3Key) ? (Key)Enum.Parse(typeof(Key), settings.Hotkey3Key) : (Key?)null;
                    Hotkey3MouseButton = !string.IsNullOrEmpty(settings.Hotkey3MouseButton) ? (MouseButton)Enum.Parse(typeof(MouseButton), settings.Hotkey3MouseButton) : (MouseButton?)null;

                    Hotkey1TextBox.Text = Hotkey1Key?.ToString() ?? Hotkey1MouseButton?.ToString() ?? "Clique para configurar Atalho 1";
                    Hotkey2TextBox.Text = Hotkey2Key?.ToString() ?? Hotkey2MouseButton?.ToString() ?? "Clique para configurar Atalho 2";
                    Hotkey3TextBox.Text = Hotkey3Key?.ToString() ?? Hotkey3MouseButton?.ToString() ?? "Clique para configurar Atalho 3";

                    areHotkeysSaved = true;
                }
                else
                {
                    Hotkey1Key = null;
                    Hotkey1MouseButton = null;
                    Hotkey2Key = null;
                    Hotkey2MouseButton = null;
                    Hotkey3Key = null;
                    Hotkey3MouseButton = null;

                    Hotkey1TextBox.Text = "Clique para configurar Atalho 1";
                    Hotkey2TextBox.Text = "Clique para configurar Atalho 2";
                    Hotkey3TextBox.Text = "Clique para configurar Atalho 3";
                }
            }
            catch (Exception e)
            {
                clog.LogException(e.ToString(), "LoadHotkeys()");
                MessageBox.Show("Ocorreu um erro ao carregar os atalhos, favor tentar novamente em instantes.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Método que realiza o salvamento dos atalhos
        /// </summary>
        private void SaveHotkeys()
        {
            try
            {
                var settings = new HotkeySettings
                {
                    Hotkey1Key = Hotkey1Key?.ToString(),
                    Hotkey1MouseButton = Hotkey1MouseButton?.ToString(),
                    Hotkey2Key = Hotkey2Key?.ToString(),
                    Hotkey2MouseButton = Hotkey2MouseButton?.ToString(),
                    Hotkey3Key = Hotkey3Key?.ToString(),
                    Hotkey3MouseButton = Hotkey3MouseButton?.ToString()
                };

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, json);

                areHotkeysSaved = true;
                ShowNotification("Atalhos salvos com sucesso", "Salvamento dos atalhos");
            }
            catch (Exception e)
            {
                clog.LogException(e.ToString(), "SaveHotkeys()");
                MessageBox.Show("Ocorreu um erro ao salvar os atalhos, favor tentar novamente em instantes.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Evento KeyDown para detectar a combinação de teclas
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            CheckHotkeyCombination();
        }

        /// <summary>
        /// Evento de fechamento da janela
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                e.Cancel = true;
                Hide();
            }

            if (!areHotkeysSaved)
            {
                var result = MessageBox.Show("Você não salvou os atalhos. Deseja sair sem salvar?", "Confirmar saída", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Evento para salvar os atalhos
        /// </summary>
        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            SaveHotkeys();
        }

        /// <summary>
        /// Método responsável por limpar os atalhos salvos pelo usuário.
        /// </summary>
        private void OnClearHotkeysClick(object sender, RoutedEventArgs e)
        {
            Hotkey1Key = null;
            Hotkey2Key = null;
            Hotkey3Key = null;
            Hotkey1MouseButton = null;
            Hotkey2MouseButton = null;
            Hotkey3MouseButton = null;

            areHotkeysSaved = false;

            if (File.Exists(settingsFilePath))
            {
                File.Delete(settingsFilePath);
            }

            Hotkey1TextBox.Text = "Clique para configurar Atalho 1";
            Hotkey2TextBox.Text = "Clique para configurar Atalho 2";
            Hotkey3TextBox.Text = "Clique para configurar Atalho 3";

            ShowNotification("Atalhos limpos com sucesso!", "Limpeza de Atalhos");
        }

        /// <summary>
        /// Método responsável por exibir uma notificação no windows para o usuário.
        /// </summary>
        /// <param name="message">A mensagem a ser exibida</param>
        /// <param name="title">O titulo a ser exibido.</param>
        private void ShowNotification(string message, string title)
        {
            TaskbarIcon.ShowBalloonTip(title, message, BalloonIcon.Info);
        }
    }
}