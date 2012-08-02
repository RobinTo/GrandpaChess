using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNASjakk
{
    public struct move
    {
        public int id;
        public int oldX;
        public int oldY;
        public int newX;
        public int newY;
        public int shotID;
        public move(int id, int oldX, int oldY, int newX, int newY, int shotID)
        {
            this.id = id;
            this.oldX = oldX;
            this.oldY = oldY;
            this.newX = newX;
            this.newY = newY;
            this.shotID = shotID;
        }
    }
    public struct receiveMove
    {
        public int id;
        public int x;
        public int y;
        public receiveMove(int i, int t, int j)
        {
            id = i;
            x = t;
            y = j;
        }
    }

    class ChessBoard : DrawableGameComponent
    {
        string infoText = "Venstreklikk - Velg Brikke \nVenstreklikk igjen - Flytt Brikke \nHoyreklikk- Velg Annen Brikke \n \nGult Felt - Brikken valgt \nRodt felt - Brikken flyttet hit \n \nEscape for Nytt Spill";

        double connectTimer = 5;

        bool disconnected = false; // Set to true if someone disconnects to show message.

        String errorString = "Ikke tilkoblet server.";
        bool hasOpponent = false;
        NetworkManager networkManager = new NetworkManager();
        MouseState oldMouse = new MouseState();
        MouseState newMouse = new MouseState();
        KeyboardState oldKey = new KeyboardState();
        KeyboardState newKey = new KeyboardState();

        SpriteFont font;

        SpriteFont infoFont;

        Texture2D blackSquare;
        Texture2D whiteSquare;

        Texture2D pawn;
        Texture2D castle;
        Texture2D knight;
        Texture2D springer;
        Texture2D king;
        Texture2D queen;

        List<ChessPiece> pieces = new List<ChessPiece>();
        List<move> moves = new List<move>();

        int xPos, yPos;
        int selectedY = 0;
        int selectedX = 0;
        ChessPiece selectedPiece;

        bool server;
        bool move = false;

        int id = 1;

        bool connected = false;

        public ChessBoard(Game game, int x, int y)
            : base(game)
        {
            this.xPos = x;
            this.yPos = y;
        }

        public void Exit()
        {
            networkManager.CloseDown();
        }

        protected override void LoadContent()
        {
            ContentManager content = Game.Content;
            blackSquare = content.Load<Texture2D>("black");
            whiteSquare = content.Load<Texture2D>("white");

            pawn = content.Load<Texture2D>("whitePesant");
            castle = content.Load<Texture2D>("whiteTower");
            knight = content.Load<Texture2D>("whiteHorse");
            springer = content.Load<Texture2D>("whiteSpringer");
            king = content.Load<Texture2D>("whiteKing");
            queen = content.Load<Texture2D>("whiteQueen");

            font = content.Load<SpriteFont>("font");
            infoFont = content.Load<SpriteFont>("TextFont");

            try
            {
                server = false;
                String ipString = System.IO.File.ReadAllText("ipAddresse.txt");
                if (server)
                {
                    networkManager.StartServer(ipString);
                }
                else
                {
                    networkManager.StartClient(ipString);
                }
            }
            catch (Exception e)
            {
                errorString = "Feil i Innstillingene.";
            }

           

            for (int i = 0; i <= GameConstants.boardWidth; i++)
            {
                ChessPiece pawnPiece = new ChessPiece(id, pawn, i, 2, Color.White);
                pieces.Add(pawnPiece);
                id++;
            }
            for (int i = 0; i <= GameConstants.boardWidth; i++)
            {
                ChessPiece pawnPiece = new ChessPiece(id, pawn, i, 7, Color.Black);
                pieces.Add(pawnPiece);
                id++;
            }

            for (int i = 0; i <= 1; i++)
            {
                ChessPiece whiteCastles = new ChessPiece(id, castle,  i == 0 ? 1 : 8, 1, Color.White);
                pieces.Add(whiteCastles);
                id++;
            }
            for (int i = 0; i <= 1; i++)
            {
                ChessPiece blackCastles = new ChessPiece(id, castle, i == 0 ? 1 : 8, 8, Color.Black);
                pieces.Add(blackCastles);
                id++;
            }

            for (int i = 0; i <= 1; i++)
            {
                ChessPiece whiteKnight = new ChessPiece(id, knight, i == 0 ? 2 : 7, 1, Color.White);
                pieces.Add(whiteKnight);
                id++;
            }
            for (int i = 0; i <= 1; i++)
            {
                ChessPiece blackKnight = new ChessPiece(id, knight, i == 0 ? 2 : 7, 8, Color.Black);
                pieces.Add(blackKnight);
                id++;
            }

            for (int i = 0; i <= 1; i++)
            {
                ChessPiece blackSpringer = new ChessPiece(id, springer, i == 0 ? 3 : 6, 8, Color.Black);
                pieces.Add(blackSpringer);
                id++;
            }
            for (int i = 0; i <= 1; i++)
            {
                ChessPiece whiteSpringer = new ChessPiece(id, springer, i == 0 ? 3 : 6, 1, Color.White);
                pieces.Add(whiteSpringer);
                id++;
            }

            ChessPiece whiteKing = new ChessPiece(id, king, 4, 1, Color.White);
            pieces.Add(whiteKing);
            id++;
            ChessPiece whiteQueen = new ChessPiece(id, queen, 5, 1, Color.White);
            pieces.Add(whiteQueen);
            id++;
            ChessPiece blackKing = new ChessPiece(id, king, 4, 8, Color.Black);
            pieces.Add(blackKing);
            id++;
            ChessPiece blackQueen = new ChessPiece(id, queen, 5, 8, Color.Black);
            pieces.Add(blackQueen);
            id++;

            base.LoadContent();
        }

        double reconnectTimer = 0.2;

        public override void Update(GameTime gameTime)
        {
            if (!connected)
            {
                if (server)
                {
                    connected = networkManager.TryAcceptClient();
                }
                else
                {
                    if (reconnectTimer <= 0)
                    {
                        connected = networkManager.TryConnect();
                        reconnectTimer = 5.0; // Added a small timer to enable closing of client window.
                    }
                    else
                        reconnectTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
            else if (!hasOpponent)
            {
                if (networkManager.isDataAvailableClient())
                {
                    if (networkManager.ReturnByte() == 101)
                    {
                        hasOpponent = true;
                    }
                }
            }
            else if (!disconnected)
            {
                if (server && networkManager.isDataAvailableServer())
                {
                    switch (networkManager.ReturnByte())
                    {
                        case 0:
                            // It was just a ping
                            break;
                        case 98:
                            UndoMove(moves[moves.Count - 1]);
                            break;
                        case 99:
                            receiveMove r = networkManager.ReceiveMove();
                            MovePiece(r.id, r.x, r.y);
                            break;
                        case 100:
                            // errorString = "Mostander frakoblet. \nEscape for nytt spill.";
                            disconnected = true;
                            break;
                        default:

                            break;
                    }
                }
                else if (!server && networkManager.isDataAvailableClient())
                {
                    switch (networkManager.ReturnByte())
                    {
                        case 0:
                            // It was just a ping
                            break;
                        case 98:
                            UndoMove(moves[moves.Count - 1]);
                            break;
                        case 99:
                            receiveMove r = networkManager.ReceiveMove();
                            MovePiece(r.id, r.x, r.y);
                            break;
                        case 100:
                            // errorString = "Mostander frakoblet. \nEscape for nytt spill.";
                            disconnected = true;
                            break;
                        default:

                            break;
                    }
                }

                if (connectTimer > 0)
                    connectTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                else
                {
                    if (!networkManager.AreWeStillConnected())
                    {
                        disconnected = true;
                    }
                    connectTimer = 5;
                }

                newMouse = Mouse.GetState();
                newKey = Keyboard.GetState();

                if (newKey.IsKeyDown(Keys.Back) && oldKey.IsKeyUp(Keys.Back))
                {
                    if (moves.Count > 0)
                    {
                        networkManager.SendUndo();
                        UndoMove(moves[moves.Count - 1]);
                    }
                }
                else if (newKey.IsKeyDown(Keys.Left) && oldKey.IsKeyUp(Keys.Left))
                {
                    if (moves.Count > 0)
                    {
                        networkManager.SendUndo();
                        UndoMove(moves[moves.Count - 1]);
                    }
                }

                if (newMouse.RightButton == ButtonState.Pressed && oldMouse.RightButton != ButtonState.Pressed)
                {
                    selectedPiece = null;
                    move = false;
                    selectedY = 0;
                    selectedX = 0;
                }
                else if (newMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton != ButtonState.Pressed)
                {
                    int newClickX = 1 + (newMouse.X - xPos) / 80;
                    int newClickY = 1 + (newMouse.Y - yPos) / 80;

                    if (newClickX > 0 && newClickX <= 8 && newClickY > 0 && newClickY <= 8)
                    {
                        if (!move)
                        {
                            foreach (ChessPiece piece in pieces)
                            {
                                if (piece.X == newClickX && piece.Y == newClickY && piece.Visible)
                                {
                                    selectedY = newClickY;
                                    selectedX = newClickX;
                                    selectedPiece = piece;
                                    move = true;
                                }
                            }
                        }
                        else if (!(selectedPiece.X == newClickX && selectedPiece.Y == newClickY))
                        {
                            selectedY = newClickY;
                            selectedX = newClickX;

                            networkManager.SendMove(selectedPiece.Id, newClickX, newClickY);
                            MovePiece(selectedPiece.Id, newClickX, newClickY);
                            move = false;
                        }
                    }
                }

                oldMouse = newMouse;
                oldKey = newKey;
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteBatch.Begin();
            if (!connected)
            {
                spriteBatch.DrawString(font, errorString, new Vector2(50, 250), Color.Black);
            }
            else if (!hasOpponent)
            {
                spriteBatch.DrawString(font, "Venter pa motstander.", new Vector2(50, 250), Color.Black);
            }
            else
            {
                for (int i = 1; i <= GameConstants.boardHeight; i++)
                {
                    for (int y = 1; y <= GameConstants.boardWidth; y++)
                    {
                        if (i == selectedX && y == selectedY)
                        {
                            spriteBatch.Draw(whiteSquare, new Vector2(xPos + ((i - 1) * GameConstants.squareWidth), yPos + ((y - 1) * GameConstants.squareHeight)), move ? Color.Yellow : Color.Red);
                        }
                        else
                        {
                            if (i % 2 == 0)
                                spriteBatch.Draw((y % 2 == 0) ? whiteSquare : blackSquare, new Vector2(xPos + ((i - 1) * GameConstants.squareWidth), yPos + ((y - 1) * GameConstants.squareHeight)), Color.White);
                            else
                                spriteBatch.Draw((y % 2 == 0) ? blackSquare : whiteSquare, new Vector2(xPos + ((i - 1) * GameConstants.squareWidth), yPos + ((y - 1) * GameConstants.squareHeight)), Color.White);
                        }

                    }
                }

                foreach (ChessPiece piece in pieces)
                {
                    piece.Draw(gameTime, spriteBatch);
                }

                // Draw info

                spriteBatch.DrawString(infoFont, infoText, new Vector2(680, 40), Color.Black);

                // ----

                if (disconnected)
                    spriteBatch.DrawString(font, "Mistet kobling. \nEscape for nytt spill.", new Vector2(50, 250), Color.Black);
            }


            spriteBatch.End();

            base.Draw(gameTime);

        }

        public void MovePiece(int id, int x, int y)
        {
            foreach (ChessPiece piece in pieces)
            {
                if (piece.Id == id && piece.Visible)
                {
                    int shotID = 0;
                    foreach (ChessPiece dupiece in pieces)
                    {
                        if (dupiece.X == x && dupiece.Y == y)
                        {
                            dupiece.Visible = false;
                            shotID = dupiece.Id;
                        }
                    }
                    moves.Add(new move(piece.Id, piece.X, piece.Y, x, y, shotID));
                    piece.X = x;
                    piece.Y = y;
                    selectedX = x;
                    selectedY = y;
                    move = false;
                }
            }
        }

        public void UndoMove(move m)
        {
            foreach (ChessPiece piece in pieces)
            {
                if (piece.Id == m.id)
                {
                    piece.X = m.oldX;
                    piece.Y = m.oldY;

                    if (m.shotID > 0)
                    {
                        foreach (ChessPiece dupiece in pieces)
                        {
                            if (dupiece.Id == m.shotID)
                                dupiece.Visible = true;
                        }
                    }
                    selectedPiece = null;
                    selectedX = 0;
                    selectedY = 0;
                    move = false;
                }
            }
            moves.Remove(m);
        }

        public void Destroy()
        {
            networkManager.CloseDown();
        }
    }
}
