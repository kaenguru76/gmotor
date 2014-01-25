using System;
using System.Collections.Generic;
using System.Text;

namespace gmotor
{
	public class Player
	{
		private System.Drawing.Color color;

		public System.Drawing.Color Color
		{
			get { return color; }
			set { color = value; }
		}

		public Player(System.Drawing.Color color)
		{
			this.color = color;	
		}
	}
}
