
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace SDRSharp.Tetra
{
    public sealed class MastManagerForm : Form
    {
        private readonly BindingList<MastConfig> _items;
        private readonly DataGridView _grid = new DataGridView();
        private readonly Button _add = new Button();
        private readonly Button _remove = new Button();
        private readonly Button _ok = new Button();
        private readonly Button _cancel = new Button();

        public BindingList<MastConfig> Items => _items;

        public MastManagerForm(BindingList<MastConfig> items)
        {
            _items = items;

            Text = "Manage masts";
            Width = 520;
            Height = 360;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            StartPosition = FormStartPosition.CenterParent;

            _grid.Dock = DockStyle.Top;
            _grid.Height = 260;
            _grid.AutoGenerateColumns = false;
            _grid.AllowUserToAddRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;

            _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "On", DataPropertyName = "Enabled", Width = 40 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = "Name", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Frequency (MHz)", Width = 120, Name = "FreqMHz" });

            _grid.DataSource = _items;
            _grid.CellFormatting += (s, e) =>
            {
                if (_grid.Columns[e.ColumnIndex].Name == "FreqMHz")
                {
                    var cfg = _grid.Rows[e.RowIndex].DataBoundItem as MastConfig;
                    e.Value = (cfg.FrequencyHz / 1_000_000.0).ToString("0.000000", CultureInfo.InvariantCulture);
                    e.FormattingApplied = true;
                }
            };
            _grid.CellParsing += (s, e) =>
            {
                if (_grid.Columns[e.ColumnIndex].Name == "FreqMHz")
                {
                    var cfg = _grid.Rows[e.RowIndex].DataBoundItem as MastConfig;
                    if (double.TryParse(Convert.ToString(e.Value), NumberStyles.Float, CultureInfo.InvariantCulture, out var mhz))
                    {
                        cfg.FrequencyHz = (long)Math.Round(mhz * 1_000_000.0);
                        e.ParsingApplied = true;
                    }
                }
            };

            var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };

            _ok.Text = "OK";
            _ok.Width = 90;
            _ok.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            _cancel.Text = "Cancel";
            _cancel.Width = 90;
            _cancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            _add.Text = "Add";
            _add.Width = 90;
            _add.Click += (s, e) =>
            {
                _items.Add(new MastConfig { Name = "Mast", FrequencyHz = 0, Enabled = true });
                _grid.Refresh();
            };

            _remove.Text = "Remove";
            _remove.Width = 90;
            _remove.Click += (s, e) =>
            {
                if (_grid.SelectedRows.Count == 0) return;
                var cfg = _grid.SelectedRows[0].DataBoundItem as MastConfig;
                if (cfg != null) _items.Remove(cfg);
            };

            panel.Controls.Add(_ok);
            panel.Controls.Add(_cancel);
            panel.Controls.Add(_remove);
            panel.Controls.Add(_add);

            Controls.Add(panel);
            Controls.Add(_grid);
        }
    }
}
