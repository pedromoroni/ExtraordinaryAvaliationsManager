namespace EamBackOffice01 {
    partial class RequestHistoryForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            requestHistoryListView = new ListView();
            refreshHistoryContextMenuStrip = new ContextMenuStrip(components);
            refreshToolStripMenuItem = new ToolStripMenuItem();
            refreshHistoryContextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // requestHistoryListView
            // 
            requestHistoryListView.Dock = DockStyle.Fill;
            requestHistoryListView.Location = new Point(0, 0);
            requestHistoryListView.Name = "requestHistoryListView";
            requestHistoryListView.Size = new Size(800, 450);
            requestHistoryListView.TabIndex = 0;
            requestHistoryListView.UseCompatibleStateImageBehavior = false;
            requestHistoryListView.View = View.Details;
            requestHistoryListView.MouseDown += requestHistoryListView_MouseDown;
            // 
            // refreshHistoryContextMenuStrip
            // 
            refreshHistoryContextMenuStrip.Items.AddRange(new ToolStripItem[] { refreshToolStripMenuItem });
            refreshHistoryContextMenuStrip.Name = "refreshHistoryContextMenuStrip";
            refreshHistoryContextMenuStrip.Size = new Size(114, 26);
            // 
            // refreshToolStripMenuItem
            // 
            refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            refreshToolStripMenuItem.Size = new Size(113, 22);
            refreshToolStripMenuItem.Text = "Refresh";
            refreshToolStripMenuItem.Click += refreshToolStripMenuItem_Click;
            // 
            // RequestHistoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(requestHistoryListView);
            Name = "RequestHistoryForm";
            Text = "RequestHistoryForm";
            Load += RequestHistoryForm_Load;
            refreshHistoryContextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ListView requestHistoryListView;
        private ContextMenuStrip refreshHistoryContextMenuStrip;
        private ToolStripMenuItem refreshToolStripMenuItem;
    }
}