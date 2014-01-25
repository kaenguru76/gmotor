using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gvisu
{
	public partial class NewGame : Form
	{
		private int m_BoardSize;

		public NewGame()
		{
			InitializeComponent();
			m_BoardSize = 20;
		}

		private void NewGame_Load(object sender, EventArgs e)
		{
			nudBoardSize.Value = m_BoardSize;
			this.AcceptButton = btnOK;
		}

		public int BoardSize
		{
			get { return m_BoardSize; }
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
		}

		private void nudBoardSize_ValueChanged(object sender, EventArgs e)
		{
			m_BoardSize = Convert.ToInt32(nudBoardSize.Value);
		}

	}
}