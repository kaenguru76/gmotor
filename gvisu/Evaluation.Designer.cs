/*
 * Created by SharpDevelop.
 * User: Vančurovi
 * Date: 16.4.2014
 * Time: 22:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace gvisu
{
	partial class Evaluation
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.listViewBlack = new System.Windows.Forms.ListView();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.listViewWhite = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// listViewBlack
			// 
			this.listViewBlack.Location = new System.Drawing.Point(12, 23);
			this.listViewBlack.Name = "listViewBlack";
			this.listViewBlack.Size = new System.Drawing.Size(331, 282);
			this.listViewBlack.TabIndex = 0;
			this.listViewBlack.UseCompatibleStateImageBehavior = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(70, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(158, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Black";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(397, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(158, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "White";
			// 
			// listViewWhite
			// 
			this.listViewWhite.Location = new System.Drawing.Point(358, 23);
			this.listViewWhite.Name = "listViewWhite";
			this.listViewWhite.Size = new System.Drawing.Size(290, 282);
			this.listViewWhite.TabIndex = 3;
			this.listViewWhite.UseCompatibleStateImageBehavior = false;
			// 
			// Evaluation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(718, 448);
			this.Controls.Add(this.listViewWhite);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.listViewBlack);
			this.Name = "Evaluation";
			this.Text = "Evaluation";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.ListView listViewWhite;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView listViewBlack;
	}
}
