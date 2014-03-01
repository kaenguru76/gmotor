namespace gvisu
{
    partial class PossibleMoves
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PossibleMoves));
        	this.listView1 = new System.Windows.Forms.ListView();
        	this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
        	this.time = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
        	this.buttonPlayMove = new System.Windows.Forms.Button();
        	this.checkBoxThreat = new System.Windows.Forms.CheckBox();
        	this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
        	this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
        	this.SuspendLayout();
        	// 
        	// listView1
        	// 
        	this.listView1.BackColor = System.Drawing.SystemColors.Control;
        	this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        	        	        	this.columnHeader1,
        	        	        	this.columnHeader2,
        	        	        	this.time,
        	        	        	this.columnHeader3,
        	        	        	this.columnHeader4,
        	        	        	this.columnHeader5,
        	        	        	this.columnHeader6,
        	        	        	this.columnHeader7,
        	        	        	this.columnHeader8,
        	        	        	this.columnHeader9,
        	        	        	this.columnHeader10,
        	        	        	this.columnHeader11});
        	this.listView1.FullRowSelect = true;
        	this.listView1.Location = new System.Drawing.Point(0, 0);
        	this.listView1.MultiSelect = false;
        	this.listView1.Name = "listView1";
        	this.listView1.Size = new System.Drawing.Size(528, 208);
        	this.listView1.TabIndex = 42;
        	this.listView1.UseCompatibleStateImageBehavior = false;
        	this.listView1.View = System.Windows.Forms.View.Details;
        	this.listView1.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView1_ItemSelectionChanged);
        	// 
        	// columnHeader1
        	// 
        	this.columnHeader1.Text = "#";
        	this.columnHeader1.Width = 26;
        	// 
        	// columnHeader2
        	// 
        	this.columnHeader2.Text = "move";
        	// 
        	// time
        	// 
        	this.time.Text = "time";
        	this.time.Width = 50;
        	// 
        	// columnHeader3
        	// 
        	this.columnHeader3.Text = "type";
        	this.columnHeader3.Width = 106;
        	// 
        	// columnHeader4
        	// 
        	this.columnHeader4.Text = "value";
        	this.columnHeader4.Width = 85;
        	// 
        	// columnHeader5
        	// 
        	this.columnHeader5.Text = "vct";
        	this.columnHeader5.Width = 80;
        	// 
        	// columnHeader6
        	// 
        	this.columnHeader6.Text = "moves";
        	// 
        	// columnHeader7
        	// 
        	this.columnHeader7.Text = "depth";
        	// 
        	// buttonPlayMove
        	// 
        	this.buttonPlayMove.Location = new System.Drawing.Point(212, 214);
        	this.buttonPlayMove.Name = "buttonPlayMove";
        	this.buttonPlayMove.Size = new System.Drawing.Size(127, 36);
        	this.buttonPlayMove.TabIndex = 43;
        	this.buttonPlayMove.Text = "Play move";
        	this.buttonPlayMove.UseVisualStyleBackColor = true;
        	this.buttonPlayMove.Click += new System.EventHandler(this.buttonPlayMove_Click);
        	// 
        	// checkBoxThreat
        	// 
        	this.checkBoxThreat.AutoSize = true;
        	this.checkBoxThreat.Location = new System.Drawing.Point(12, 214);
        	this.checkBoxThreat.Name = "checkBoxThreat";
        	this.checkBoxThreat.Size = new System.Drawing.Size(53, 17);
        	this.checkBoxThreat.TabIndex = 44;
        	this.checkBoxThreat.Text = "threat";
        	this.checkBoxThreat.UseVisualStyleBackColor = true;
        	this.checkBoxThreat.CheckedChanged += new System.EventHandler(this.checkBoxThreat_CheckedChanged);
        	this.checkBoxThreat.Click += new System.EventHandler(this.checkBoxThreat_Click);
        	// 
        	// columnHeader8
        	// 
        	this.columnHeader8.Text = "vctBlack";
        	// 
        	// columnHeader9
        	// 
        	this.columnHeader9.Text = "vctWhite";
        	// 
        	// columnHeader10
        	// 
        	this.columnHeader10.Text = "vctBlackDepth";
        	// 
        	// columnHeader11
        	// 
        	this.columnHeader11.Text = "vctWhiteDepth";
        	// 
        	// PossibleMoves
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(528, 255);
        	this.Controls.Add(this.checkBoxThreat);
        	this.Controls.Add(this.buttonPlayMove);
        	this.Controls.Add(this.listView1);
        	this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        	this.Name = "PossibleMoves";
        	this.Text = "Possible Moves";
        	this.Load += new System.EventHandler(this.PossibleMoves_Load);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader8;

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button buttonPlayMove;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.CheckBox checkBoxThreat;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader time;
    }
}