using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace SDRSharp.Tetra
{
    /// <summary>
    /// Simple editor for per-mast frequency list (stored in tetraSettings.xml).
    /// This is a safe "fallback" step: it does NOT change decoding; it only manages presets.
    /// </summary>
    public sealed class MastManagerForm : Form
    {
        private readonly BindingList<MastConfig> _items;
        private readonly DataGridView _grid = new DataGridView();
        private readonly Button _add = new Button();
        private readonly Button _remove = new Button();
        private readonly Button _ok = new Button();
        private readonly Button _cancel = new Button();

        public MastManagerForm(BindingList<MastConfig> items)
        {
            _items = items;

            Text = "Masts";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(640, 320);

            _grid.Parent = this;
            _grid.Location = new Point(10, 10);
            _grid.Size = new Size(620, 250);
            _grid.AutoGenerateColumns = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;

            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(MastConfig.Enabled),
                HeaderText = "On",
                Width = 40
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(MastConfig.Name),
                HeaderText = "Name",
                Width = 220
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Frequency (MHz)",
                Width = 160,
                Name = "FrequencyMHz"
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(MastConfig.AfcDisabled),
                HeaderText = "AFC off",
                Width = 70
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(MastConfig.MmOnlyMode),
                HeaderText = "MM only",
                Width = 70
            });

            _grid.DataSource = _items;

            _grid.CellFormatting += (s, e) =>
            {
                if (_grid.Columns[e.ColumnIndex].Name == "FrequencyMHz")
                {
                    var item = _items[e.RowIndex];
                    e.Value = (item.FrequencyHz / 1e6).ToString("0.000000", CultureInfo.InvariantCulture);
                    e.FormattingApplied = true;
                }
            };

            _grid.CellParsing += (s, e) =>
            {
                if (_grid.Columns[e.ColumnIndex].Name == "FrequencyMHz")
                {
                    if (e.Value == null)
                    {
                        e.ParsingApplied = true;
                        return;
                    }

                    var raw = e.Value.ToString()?.Trim() ?? "";
                    raw = raw.Replace(',', '.'); // allow comma decimal
                    if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var mhz))
                    {
                        _items[e.RowIndex].FrequencyHz = (long)Math.Round(mhz * 1_000_000.0);
                        e.ParsingApplied = true;
                    }
                }
            };

            _add.Parent = this;
            _add.Text = "Add";
            _add.Location = new Point(10, 270);
            _add.Size = new Size(90, 32);
            _add.Click += (s, e) =>
            {
                var next = new MastConfig
                {
                    Name = "Mast " + (_items.Count + 1),
                    FrequencyHz = 0,
                    Enabled = true
                };
                _items.Add(next);
                _grid.ClearSelection();
                _grid.Rows[_items.Count - 1].Selected = true;
            };

            _remove.Parent = this;
            _remove.Text = "Remove";
            _remove.Location = new Point(110, 270);
            _remove.Size = new Size(90, 32);
            _remove.Click += (s, e) =>
            {
                if (_grid.CurrentRow == null) return;
                var idx = _grid.CurrentRow.Index;
                if (idx >= 0 && idx < _items.Count)
                    _items.RemoveAt(idx);
            };

            _ok.Parent = this;
            _ok.Text = "OK";
            _ok.Location = new Point(450, 270);
            _ok.Size = new Size(85, 32);
            _ok.DialogResult = DialogResult.OK;

            _cancel.Parent = this;
            _cancel.Text = "Cancel";
            _cancel.Location = new Point(545, 270);
            _cancel.Size = new Size(85, 32);
            _cancel.DialogResult = DialogResult.Cancel;

            AcceptButton = _ok;
            CancelButton = _cancel;
        }
    }
}
