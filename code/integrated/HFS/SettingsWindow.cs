using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HFS
{
    public partial class SettingsWindow : Form
    {
        public BindingList<Config> configs = ConfigAdapter.load();

        private BindingSource bs = new BindingSource();

        public SettingsWindow()
        {
            Init();
        }

        private void Init()
        {
            InitializeComponent();

            setState(false);

            bs.DataSource = configs;

            cboxSetting.DataSource = bs.DataSource;

            cboxSetting.DisplayMember = "Name";
            cboxSetting.ValueMember = "Name";
        }
        
        private void setState(bool state)
        {
            tboxName.Enabled = state;
            tboxPort.Enabled = state;
            cbUpload.Enabled = state;
            numUsers.Enabled = state;

            btnDel.Enabled = state;
            btnSave.Enabled = state;
        }

        private void clearValues()
        {
            tboxName.Text = "";
            tboxPort.Text = "";
            cbUpload.Checked = false;
            numUsers.Value = 0;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            int i;
            Config configItem = getConfigItem(cboxSetting.Text, out i);

            if (configItem == null)
                return;

            int port;
            Int32.TryParse(tboxPort.Text, out port);

            configItem.Name = tboxName.Text;
            configItem.Port = port;
            configItem.MaxUsers = (int)numUsers.Value;
            configItem.AllowUpload = cbUpload.Checked;

            configs[i] = configItem;

            clearValues();
            setState(false);
        }

        private void cboxSetting_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(cboxSetting.Text))
                btnCreate.Enabled = false;
            else
                btnCreate.Enabled = true;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            erProv.Clear();

            if (configs.Where(x => x.Name.Equals(cboxSetting.Text)).ToList().Count > 0)
            {
                erProv.SetError(cboxSetting, "A hozzáadni kívánt elem már létezik!");
                return;
            }

            Config configItem = new Config() { Port = 8888, Name = cboxSetting.Text, AllowUpload = true, MaxUsers = 0 };

            configs.Insert(0, configItem);

            cboxSetting.SelectedIndex = 0;

            tboxName.Text = configItem.Name;
            tboxPort.Text = configItem.Port.ToString();
            numUsers.Value = configItem.MaxUsers;
            cbUpload.Checked = configItem.AllowUpload;

            setState(true);
        }

        private void cboxSetting_SelectedValueChanged(object sender, EventArgs e)
        {
            Config configItem = getConfigItem(cboxSetting.Text);

            if (configItem == null)
                return;

            tboxName.Text = configItem.Name;
            tboxPort.Text = configItem.Port.ToString();
            numUsers.Value = configItem.MaxUsers;
            cbUpload.Checked = configItem.AllowUpload;

            setState(true);
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            Config configItem = getConfigItem(cboxSetting.Text);

            if (configItem == null)
                return;

            configs.Remove(configItem);

            clearValues();
            setState(false);
        }

        private Config getConfigItem(String id)
        {
            List<Config> items = configs.Where(x => x.Name.Equals(cboxSetting.Text)).ToList();

            if (items.Count > 0)
                return items[0];

            return null;
        }

        private Config getConfigItem(string p, out int i)
        {
            i = 0;
            foreach (Config item in configs)
            {
                if (item.Name.Equals(p))
                    return item;

                i++;
            }

            return null;
        }

        private void tboxPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                int num;

                Int32.TryParse(tboxPort.Text + e.KeyChar, out num);

                if (num > 65535)
                    e.Handled = true;
            }
            else if (e.KeyChar != '\b')
                e.Handled = true;
        }

        private void SettingsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            ConfigAdapter.save(configs);
        }
    }
}
