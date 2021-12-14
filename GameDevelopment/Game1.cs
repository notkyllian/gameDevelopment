﻿using GameDevelopment.GameObject;
using GameDevelopment.Input;
using GameDevelopment.Map;
using GameDevelopment.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace GameDevelopment
{



    public abstract class GameState
    {
        protected abstract void Initialize();
        protected abstract void Update(GameTime gameTime);
        protected abstract void Draw(GameTime gameTime);

    }

    public class MenuState : GameState
    {
        protected override void Draw(GameTime gameTime)
        {
            throw new System.NotImplementedException();
        }

        protected override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        protected override void Update(GameTime gameTime)
        {
            throw new System.NotImplementedException();
        }
    }



    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private RenderTarget2D _scene;
        private SpriteBatch _spriteBatch;
        private MapManager _mapManager;
        private Texture2D _heroTexture;
        private Texture2D _mapTexture;

        private Hero hero;

        private int[,] gameboard;

        private List<Block> blocks = new List<Block>();
        public Game1()
        {

            //_scene = new RenderTarget2D(_graphics.GraphicsDevice, 1366, 768, false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.DiscardContents);
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

           

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic 

            base.Initialize();

            hero = new Hero(_heroTexture, new KeyboardReader());

            _mapManager = MapManager.getInstance();

            _mapManager.addMap(new int[,]
            {
                { 0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0 },
                { 1,1,1,1,1,1,1,1 }
            }, _mapTexture);


            _mapManager.selectMap(0);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here

            _heroTexture = Content.Load<Texture2D>("doctor");
            _mapTexture = Content.Load<Texture2D>("map");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            hero.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);


            hero.Draw(_spriteBatch);
            _mapManager.currentMap.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

       
    }
}
