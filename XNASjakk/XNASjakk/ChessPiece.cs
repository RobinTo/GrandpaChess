using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNASjakk
{
    class ChessPiece
    {
        Texture2D pieceTexture;
        Color color;
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        bool visible = true;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        int parentX = 20;
        int parentY = 20;
        int x;
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        int y;
        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public ChessPiece(int id, Texture2D image, int x, int y, Color color)
        {
            this.id = id;
            pieceTexture = image;
            this.x = x;
            this.y = y;
            this.color = color;
        }

        public bool isValidMove(int newX, int newY)
        {

            return true;
        }

        public void setPosition(int newX, int newY)
        {

        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if(visible)
                spriteBatch.Draw(pieceTexture, new Vector2(parentX + (x-1) * GameConstants.squareHeight, parentY + (y-1) * GameConstants.squareWidth), color);
        }
    }
}
