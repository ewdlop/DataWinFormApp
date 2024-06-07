namespace DataWinFormApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2472, 1313);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);

            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem editMenu = new ToolStripMenuItem("Edit");
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("View");

            // Add items to the file menu
            fileMenu.DropDownItems.Add("New");
            fileMenu.DropDownItems.Add("Open");
            fileMenu.DropDownItems.Add("Save");

            ToolStripItem Exit = fileMenu.DropDownItems.Add("Exit");
            Exit.Click += (sender, e) => { Application.Exit(); };

            // Add the menus to the MenuStrip
            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(viewMenu);


            // Set Dock properties
            menuStrip.Dock = DockStyle.Top;

            // Add the MenuStrip and ToolStrip to the form's Controls
            this.Controls.Add(menuStrip);

            // Set form properties
            this.Text = "WinForms with Top-Level Bar";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        #endregion
    }
}
