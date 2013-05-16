using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HFS
{
    public partial class TagListItem : UserControl
    {
        public TagListItem()
        {
            InitializeComponent();
        }

        public String TagText { get { return label.Text; } set { label.Text= value;} }

        public event EventHandler RemoveClick
        {
            add
            {
                btRemove.Click += value;
            }
            remove
            {
                btRemove.Click -= value;
            }
        }
    }
}
