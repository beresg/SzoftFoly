namespace HFS
{
    partial class TagListItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.list = new System.Windows.Forms.ListBox();
            this.label = new System.Windows.Forms.Label();
            this.btRemove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // list
            // 
            this.list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.list.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.list.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.list.FormattingEnabled = true;
            this.list.ItemHeight = 15;
            this.list.Location = new System.Drawing.Point(38, 28);
            this.list.Name = "list";
            this.list.Size = new System.Drawing.Size(229, 122);
            this.list.TabIndex = 0;
            // 
            // label
            // 
            this.label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label.Location = new System.Drawing.Point(7, 6);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(260, 19);
            this.label.TabIndex = 1;
            this.label.Text = "Label";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btRemove
            // 
            this.btRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.btRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btRemove.Location = new System.Drawing.Point(7, 27);
            this.btRemove.Name = "btRemove";
            this.btRemove.Size = new System.Drawing.Size(26, 124);
            this.btRemove.TabIndex = 2;
            this.btRemove.Text = "Remove";
            this.btRemove.UseVisualStyleBackColor = true;
            // 
            // TagListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btRemove);
            this.Controls.Add(this.label);
            this.Controls.Add(this.list);
            this.Name = "TagListItem";
            this.Size = new System.Drawing.Size(273, 157);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox list;
        private System.Windows.Forms.Label label;
        private System.Windows.Forms.Button btRemove;
    }
}
