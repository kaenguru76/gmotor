using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace gvisu
{
	public class BoardVisu
	{
		private Image image;
		private gmotor.Game game;

		public BoardVisu(gmotor.Game game)
		{
			this.game = game;
			
			//g = new Graphics();
		}

		public Image Image
		{
			get {
				Graphics g = Graphics.FromImage(image);
				g.Clear(Color.Black);
				return image; 
			}
			set { }
		}
	}
}
