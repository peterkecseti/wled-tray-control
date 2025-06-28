using System;
using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;
using Application = System.Windows.Application;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace WLEDProfiles
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Forms.NotifyIcon _notifyIcon;
        private List<Preset> _presets = new List<Preset>();

        public IReadOnlyList<Preset> Presets => _presets.AsReadOnly();

        public App()
        {
            _notifyIcon = new Forms.NotifyIcon();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await LoadPresetsAsync();
            InitializeNotifyIcon();
            AddContextItems();

            
        }

        private void InitializeNotifyIcon()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "WLEDProfiles.assets.icon.ico"; // Adjust this to your actual namespace + path

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    _notifyIcon.Icon = new Icon(stream);
                }
                else
                {
                    // Handle the case where resource is not found (optional)
                    throw new FileNotFoundException($"Resource '{resourceName}' not found.");
                }
            }

            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Set WLED Profiles";
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
        }
        private void AddContextItems()
        {

            _notifyIcon.ContextMenuStrip.Items.Add("Toggle", null, (s, ev) => {SendJsonRequest(new { on = "t" }, s, ev); });
            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _notifyIcon.ContextMenuStrip.Items.Add("High", null, (s, ev) => { SendJsonRequest(new { bri = 255 }, s, ev); });
            _notifyIcon.ContextMenuStrip.Items.Add("Low", null, (s, ev) => { SendJsonRequest(new { bri = 25 }, s, ev); });
            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

            for (int i = 1; i < _presets.Count; i++)
            {
                var item = new ToolStripMenuItem(_presets[i].Name);
                var body = new { ps = i };
                item.Click += (s, ev) => { SendJsonRequest(body, s, ev); };

                _notifyIcon.ContextMenuStrip.Items.Add(item);
            }

            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, ev) => { Shutdown(); });
        }

        private async void SendJsonRequest(object json, object sender, EventArgs e)
        {
            using(HttpClient client = new HttpClient())
            {
                string jsonString = JsonConvert.SerializeObject(json);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client.PostAsync("http://wled.local/json/state", content);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error sending request: {ex.Message}");
                }
            }
        }

        private async Task LoadPresetsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("http://wled.local/presets.json");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var presetsDict = JsonConvert.DeserializeObject<Dictionary<string, Preset>>(json);
                        _presets.Clear();
                        _presets.AddRange(presetsDict.Values);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"HTTP error fetching presets: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException hre)
                {
                    System.Windows.MessageBox.Show($"HTTP request error: {hre.Message}");
                }
                catch (JsonException je)
                {
                    System.Windows.MessageBox.Show($"JSON parsing error: {je.Message}");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Unexpected error: {ex.Message}");
                }
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();

            base.OnExit(e);
        }
    }
}
