using SDRSharp.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SDRSharp.Tetra
{
    /// <summary>
    /// Single plugin UI that can host many internal TETRA channels (tabs).
    /// Avoids registering multiple SDR# hooks and keeps the waterfall responsive.
    /// </summary>
    public class TetraMultiPanel : UserControl
    {
        private readonly ISharpControl _control;
        private readonly TabControl _tabs;
        private readonly Button _addButton;
        private readonly Button _removeButton;

        private const string ChannelsFileName = "tetra_channels.xml";

        public TetraMultiPanel(ISharpControl control)
        {
            _control = control;

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            _addButton = new Button
            {
                Text = "+ Kanaal",
                Dock = DockStyle.Top,
                Height = 26
            };
            _addButton.Click += (s, e) => AddChannel();

            _removeButton = new Button
            {
                Text = "- Verwijder kanaal",
                Dock = DockStyle.Top,
                Height = 26
            };
            _removeButton.Click += (s, e) => RemoveCurrentChannel();

            Controls.Add(_tabs);
            Controls.Add(_removeButton);
            Controls.Add(_addButton);

            // Load channels from disk (if any) so they persist across restarts.
            var ids = LoadChannelIds();
            if (ids.Count > 0)
            {
                // Ensure new ids won't collide.
                TetraPlugin.EnsureInstanceCounterAtLeast(ids.Max());
                foreach (var id in ids)
                {
                    AddChannel(id, select: false);
                }
                if (_tabs.TabPages.Count > 0)
                {
                    _tabs.SelectedIndex = 0;
                }
            }
            else
            {
                AddChannel(); // create first channel
            }
        }

        public void SaveAll()
        {
            try
            {
                foreach (TabPage page in _tabs.TabPages)
                {
                    if (page.Controls.Count > 0 && page.Controls[0] is TetraPanel panel)
                    {
                        try { panel.SaveSettings(); } catch { }
                    }
                }
                SaveChannelIds();
            }
            catch { }
        }

        private void AddChannel()
        {
            var id = TetraPlugin.NextInstanceNumber();
            AddChannel(id, select: true);
        }

        private void AddChannel(int id, bool select)
        {
            var panel = new TetraPanel(_control, id) { Dock = DockStyle.Fill };

            var page = new TabPage($"Kanaal {id}");
            page.Controls.Add(panel);

            _tabs.TabPages.Add(page);
            if (select) _tabs.SelectedTab = page;

            SaveChannelIds();
        }

        private void RemoveCurrentChannel()
        {
            if (_tabs.TabPages.Count <= 1)
            {
                MessageBox.Show("Je moet minimaal 1 kanaal behouden.");
                return;
            }

            var page = _tabs.SelectedTab;
            if (page == null) return;

            try
            {
                if (page.Controls.Count > 0 && page.Controls[0] is TetraPanel panel)
                {
                    try { panel.SaveSettings(); } catch { }
                    try { panel.Dispose(); } catch { }
                }
            }
            catch { }

            _tabs.TabPages.Remove(page);
            SaveChannelIds();
        }

        private static string GetChannelsFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ChannelsFileName);
        }

        private List<int> LoadChannelIds()
        {
            var path = GetChannelsFilePath();
            try
            {
                if (!File.Exists(path)) return new List<int>();

                // Very small, simple "xml" file: one id per line between tags.
                // Example:
                // <channels>
                //   <id>1</id>
                //   <id>2</id>
                // </channels>
                var text = File.ReadAllText(path);
                var ids = new List<int>();
                var start = 0;
                while (true)
                {
                    var a = text.IndexOf("<id>", start, StringComparison.OrdinalIgnoreCase);
                    if (a < 0) break;
                    var b = text.IndexOf("</id>", a, StringComparison.OrdinalIgnoreCase);
                    if (b < 0) break;
                    var inner = text.Substring(a + 4, b - (a + 4)).Trim();
                    if (int.TryParse(inner, out var id) && id > 0) ids.Add(id);
                    start = b + 5;
                }
                return ids.Distinct().OrderBy(x => x).ToList();
            }
            catch
            {
                return new List<int>();
            }
        }

        private void SaveChannelIds()
        {
            var path = GetChannelsFilePath();
            try
            {
                var ids = new List<int>();
                foreach (TabPage page in _tabs.TabPages)
                {
                    // page text is "Kanaal {id}"
                    var parts = page.Text.Split(' ');
                    if (parts.Length > 1 && int.TryParse(parts[1], out var id)) ids.Add(id);
                }
                ids = ids.Distinct().OrderBy(x => x).ToList();

                var lines = new List<string>();
                lines.Add("<channels>");
                foreach (var id in ids) lines.Add($"  <id>{id}</id>");
                lines.Add("</channels>");
                File.WriteAllText(path, string.Join(Environment.NewLine, lines));
            }
            catch { }
        }
    }
}
