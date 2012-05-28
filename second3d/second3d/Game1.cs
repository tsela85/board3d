 using System;
 using System.Collections.Generic;
 using System.Linq;
 using Microsoft.Xna.Framework;
 using Microsoft.Xna.Framework.Audio;
 using Microsoft.Xna.Framework.Content;
 using Microsoft.Xna.Framework.GamerServices;
 using Microsoft.Xna.Framework.Graphics;
 using Microsoft.Xna.Framework.Input;
 using Microsoft.Xna.Framework.Media;
 
 namespace second3d
 {
     public class Game1 : Microsoft.Xna.Framework.Game
     {
         GraphicsDeviceManager graphics;
         SpriteBatch spriteBatch;
         GraphicsDevice device;
        
         Texture2D texture;
      
         PolyTexture board;
         Effect effect;

         private InputHandler input;
         private Camera camera;


         public Game1()
         {
             graphics = new GraphicsDeviceManager(this);
             Content.RootDirectory = "Content";
         }

         protected override void Initialize()
         {
             graphics.PreferredBackBufferWidth = 800;
             graphics.PreferredBackBufferHeight = 600;
             graphics.IsFullScreen = false;
             graphics.ApplyChanges();
             board = new PolyTexture();
             this.IsMouseVisible = true;
             Window.Title = "Lets Fold It!!";

             input = new InputHandler(this);
        //     Components.Add(input);

             camera = new Camera(this);
          //   Components.Add(camera);


             base.Initialize();
         }

         protected override void LoadContent()
         {
             spriteBatch = new SpriteBatch(GraphicsDevice);

             device = graphics.GraphicsDevice;


             texture = Content.Load<Texture2D>("images");
             effect = Content.Load<Effect>("effects");
             Vector3[] points = new Vector3[4] {
                new Vector3(-25f, 0f, 20f),
                new Vector3(25f, 0f, 20f),
                new Vector3(25f, 0f, -20f),
                new Vector3(-25f, 0f, -20f)
             };
             Vector2[] texCords = new Vector2[4] {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1)  
             };
             camera.Initialize();
             board.Initialize(texture, 4, points, texCords, effect,ref camera,ref device,ref input);
            
         }


         protected override void UnloadContent()
         {
         }

         protected override void Update(GameTime gameTime)
         {
             if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                 this.Exit();
             KeyboardState keyState = Keyboard.GetState();
             MouseState ms = Mouse.GetState();
           
             input.Update(gameTime);      

             board.update();

          //   camera.Update(gameTime);
             base.Update(gameTime);
         }

         protected override void Draw(GameTime gameTime)
         {
             device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
             RasterizerState rs = new RasterizerState();
             rs.CullMode = CullMode.None;
           //  rs.FillMode = FillMode.WireFrame;
             device.RasterizerState = rs;

             board.Draw();  
             
             base.Draw(gameTime);
         }

     }
 }