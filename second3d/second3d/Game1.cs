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
         Effect effect;
         Texture2D texture;
         private float angle = 0f;
         PolyTexture board;
         KeyboardState lstk;
         MouseState last_ms;

         VertexPositionTexture[] vertices;

         Matrix viewMatrix;
         Matrix projectionMatrix;
         Matrix worldMatrix;

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


             base.Initialize();
         }

         protected override void LoadContent()
         {
             spriteBatch = new SpriteBatch(GraphicsDevice);

             device = graphics.GraphicsDevice;

             effect = Content.Load<Effect>("effects");
             texture = Content.Load<Texture2D>("images");
             Vector3[] points = new Vector3[4] {
                new Vector3(-20f, 0f, 10f),
                new Vector3(20f, 0f, 10f),
                new Vector3(20f, 0f, -10f),
                new Vector3(-20f, 0f, -10f)
             };
             Vector2[] texCords = new Vector2[4] {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1)  
             };
             board.Initialize(texture, 4, points, texCords);
             SetUpCamera();

             SetUpVertices();
         }

         private void SetUpCamera()
         {
             viewMatrix = Matrix.CreateLookAt(new Vector3(0, -50, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
             projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.2f, 500.0f);
         }

         private void SetUpVertices()
         {
             vertices = new VertexPositionTexture[6];

             vertices[0].Position = new Vector3(-10f, 10f, 0f);
             vertices[0].TextureCoordinate.X = 0;
             vertices[0].TextureCoordinate.Y = 0;


             vertices[1].Position = new Vector3(10f, -10f, 0f);
             vertices[1].TextureCoordinate.X = 1;
             vertices[1].TextureCoordinate.Y = 1;

             vertices[2].Position = new Vector3(-10f, -10f, 0f);
             vertices[2].TextureCoordinate.X = 0;
             vertices[2].TextureCoordinate.Y = 1;

             vertices[3].Position = new Vector3(10f, -10f, 0f);
             vertices[3].TextureCoordinate.X = 1;
             vertices[3].TextureCoordinate.Y = 1;

             vertices[4].Position = new Vector3(-10f, 10f, 0f);
             vertices[4].TextureCoordinate.X = 0;
             vertices[4].TextureCoordinate.Y = 0;

             vertices[5].Position = new Vector3(10f, 10f, 0f);
             vertices[5].TextureCoordinate.X = 1;
             vertices[5].TextureCoordinate.Y = 0;
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
             Vector3 mouse = GetPickedPosition(new Vector2((float)ms.X, (float)ms.Y));

             if ((last_ms.LeftButton == ButtonState.Pressed) && ((ms.LeftButton == ButtonState.Released)))
             {
                 if (board.temp < 2)
                    board.collideWithEdge(mouse);
                 if (board.temp == 2)
                    board.update();
             }
             if ((last_ms.RightButton == ButtonState.Pressed) && ((ms.RightButton == ButtonState.Released)))
             {
                 if (board.temp == 3)
                     board.temp = 0;
             }
             if (lstk.IsKeyDown(Keys.Left) && (keyState.IsKeyUp(Keys.Left)))
             {
                 //angle = MathHelper.Pi;
                 //direction = 1;
                 //angle += 0.001f;
                 angle = 0.1f;
                 Matrix rotateMatrix = Matrix.CreateFromAxisAngle(vertices[1].Position - vertices[0].Position, angle);
                 vertices[2].Position = Vector3.Transform(vertices[2].Position, rotateMatrix);                 


             }
             if (lstk.IsKeyDown(Keys.Right) && (keyState.IsKeyUp(Keys.Right)))
             {
                 //angle = 0; ;
                 //direction = 1;
                 angle -= 0.001f;
             }

             // direction = ((angle < MathHelper.Pi) && (angle > 0) ? direction : (direction+1)%2);
             // angle += ((direction == 0) ?  -0.00001f : 0.00001f);             

             lstk = keyState;
             last_ms = ms;
             base.Update(gameTime);
         }

         protected override void Draw(GameTime gameTime)
         {
             device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
             RasterizerState rs = new RasterizerState();
             rs.CullMode = CullMode.None;
            // rs.FillMode = FillMode.WireFrame;
             device.RasterizerState = rs;


             worldMatrix = Matrix.Identity;
             effect.CurrentTechnique = effect.Techniques["TexturedNoShading"];
             effect.Parameters["xWorld"].SetValue(worldMatrix);
             effect.Parameters["xView"].SetValue(viewMatrix);
             effect.Parameters["xProjection"].SetValue(projectionMatrix);
             effect.Parameters["xTexture"].SetValue(texture);

             foreach (EffectPass pass in effect.CurrentTechnique.Passes)
             {
                 pass.Apply();

                 // device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices,0, 2, VertexPositionTexture.VertexDeclaration);
                 // device.DrawUserPrimitives(PrimitiveType.TriangleList, board.Vertices, 0, 1, VertexPositionTexture.VertexDeclaration);


                 board.Draw(ref device);
             }

             base.Draw(gameTime);
         }

         //----------------------------------------------------------------
         // GetPickedPosition() - gets 3D position of mouse pointer
         //                     - always on the the Y = 0 plane     
         //----------------------------------------------------------------
         public Vector3 GetPickedPosition(Vector2 mousePosition)
         {

             // create 2 positions in screenspace using the cursor position. 0 is as
             // close as possible to the camera, 10 is as far away as possible
             Vector3 nearSource = new Vector3(mousePosition, 0f);
             Vector3 farSource = new Vector3(mousePosition, 1f);

             // find the two screen space positions in world space
             Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearSource, projectionMatrix,viewMatrix,worldMatrix);

             Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farSource,
                                 projectionMatrix, viewMatrix, worldMatrix);

             // compute normalized direction vector from nearPoint to farPoint
             Vector3 direction = farPoint - nearPoint;
             direction.Normalize();

             // create a ray using nearPoint as the source
             Ray r = new Ray(nearPoint, direction);

             // calculate the ray-plane intersection point
             Vector3 n = new Vector3(0f, 1f, 0f);
             Plane p = new Plane(n, 0f);

             // calculate distance of intersection point from r.origin
             float denominator = Vector3.Dot(p.Normal, r.Direction);
             float numerator = Vector3.Dot(p.Normal, r.Position) + p.D;
             float t = -(numerator / denominator);

             // calculate the picked position on the y = 0 plane
             Vector3 pickedPosition = nearPoint + direction * t;


             return pickedPosition;
         }
     }
 }