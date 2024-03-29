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
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        // the following constants control the speed at which the camera moves
        // how fast does the camera move up, down, left, and right?
        const float CameraRotateSpeed = .1f;
        // how fast does the camera zoom in and out?
        const float CameraZoomSpeed = .01f;
        // the camera can't be further away than this distance
        const float CameraMaxDistance = 100.0f;
        // and it can't be closer than this
        const float CameraMinDistance = 15f;

        // the following constants control how the camera's default position
        const float CameraDefaultArc = -30.0f;
        const float CameraDefaultRotation = 225;
        const float CameraDefaultDistance = 50f;


        private Matrix projection;
        public Matrix Projection
        {
            get { return projection; }
        }

        private Matrix view;
        public Matrix View
        {
            get { return view; }
        }

        private Vector3 cameraPosition = new Vector3(0, -80, 0);
        private Vector3 cameraTarget = Vector3.Zero;
        private Vector3 cameraUpVector = new Vector3(0, 0, 1);

        private Vector3 cameraReference = new Vector3(0.0f, 0.0f, -1.0f);
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;

        private float spinRate = 120.00f;

        // The next set of variables are used to control the camera used in the sample. 
        // It is an arc ball camera, so it can rotate in a sphere around the target, and
        // zoom in and out.
        float cameraArc = CameraDefaultArc;
        float cameraRotation = CameraDefaultRotation;
        float cameraDistance = CameraDefaultDistance;

        private InputHandler input;

        public Camera(Game game)
            : base(game)
        {
           // input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(ref InputHandler inp)
        {
            base.Initialize();
            input = inp;
            InitializeCamera();
        }

        private void InitializeCamera()
        {
            //Projection
            float aspectRatio = (float)Game.GraphicsDevice.Viewport.Width /
                (float)Game.GraphicsDevice.Viewport.Height;
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio,0.2f, 500.0f,out projection);
         //       1.0f, 10000.0f, out projection);

            //View
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget,
                ref cameraUpVector, out view);

          
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (input.KeyboardHandler.IsKeyDown(Keys.Left))
                cameraYaw += (spinRate * timeDelta);
            if (input.KeyboardHandler.IsKeyDown(Keys.Right))
                cameraYaw -= (spinRate * timeDelta);
            if (input.KeyboardHandler.IsKeyDown(Keys.Up))
                cameraPitch += (spinRate * timeDelta);
            if (input.KeyboardHandler.IsKeyDown(Keys.Down))
                cameraPitch -= (spinRate * timeDelta);

//#if !XBOX360
//            if ((input.MouseHandler.PrevMouseState.X > input.MouseHandler.MouseState.X) &&
//                (input.MouseHandler.IsHoldingLeftButton()))
//                cameraYaw += (spinRate * timeDelta);
//            else if ((input.MouseHandler.PrevMouseState.X < input.MouseHandler.MouseState.X) &&
//                (input.MouseHandler.IsHoldingLeftButton()))
//                cameraYaw -= (spinRate * timeDelta);
//            if ((input.MouseHandler.PrevMouseState.Y > input.MouseHandler.MouseState.Y) &&
//                (input.MouseHandler.IsHoldingLeftButton()))
//                cameraPitch += (spinRate * timeDelta);
//            else if ((input.MouseHandler.PrevMouseState.Y < input.MouseHandler.MouseState.Y) &&
//                (input.MouseHandler.IsHoldingLeftButton()))
//                cameraPitch -= (spinRate * timeDelta);
//#endif

            if (cameraYaw > 360)
                cameraYaw -= 360;
            else if (cameraYaw < 0)
                cameraYaw += 360;
            if (cameraPitch > 89)
                cameraPitch = 89;
            else if (cameraPitch < -89)
                cameraPitch = -89;

            Matrix rotationMatrix;
            Matrix rotationMatrixYaw;
            Matrix rotationMatrixPitch;

            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotationMatrixYaw);
            Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch), out rotationMatrixPitch);

            rotationMatrix = rotationMatrixYaw * rotationMatrixPitch;

            // Create a vector pointing at the direction the camera is facing
            Vector3 transformedReference;
            Vector3.Transform(ref cameraReference, ref rotationMatrix, out transformedReference);

            // Calculate position the camera is looking at
            Vector3.Add(ref cameraPosition, ref transformedReference, out cameraTarget);

            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out view);

            base.Update(gameTime);
        }

        /// <summary>
        /// Handles input for moving the camera.
        /// </summary>
        public void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // should we reset the camera?
            if (input.KeyboardHandler.IsKeyDown(Keys.R))
            {
                cameraArc = CameraDefaultArc;
                cameraDistance = CameraDefaultDistance;
                cameraRotation = CameraDefaultRotation;
            }

            // Check for input to rotate the camera up and down around the model.
            if (input.KeyboardHandler.IsKeyDown(Keys.Up) ||
                input.KeyboardHandler.IsKeyDown(Keys.W))
            {
                cameraArc += time * CameraRotateSpeed;
            }

            if (input.KeyboardHandler.IsKeyDown(Keys.Down) ||
                input.KeyboardHandler.IsKeyDown(Keys.S))
            {
                cameraArc -= time * CameraRotateSpeed;
            }

            //cameraArc += currentGamePadState.ThumbSticks.Right.Y * time *
            //    CameraRotateSpeed;

            // Limit the arc movement.
            cameraArc = MathHelper.Clamp(cameraArc, -90.0f, 90.0f);

            // Check for input to rotate the camera around the model.
            if (input.KeyboardHandler.IsKeyDown(Keys.Right) ||
                input.KeyboardHandler.IsKeyDown(Keys.D))
            {
                cameraRotation += time * CameraRotateSpeed;
            }

            if (input.KeyboardHandler.IsKeyDown(Keys.Left) ||
                input.KeyboardHandler.IsKeyDown(Keys.A))
            {
                cameraRotation -= time * CameraRotateSpeed;
            }

            //cameraRotation += currentGamePadState.ThumbSticks.Right.X * time *
            //    CameraRotateSpeed;

            // Check for input to zoom camera in and out.
            if (input.KeyboardHandler.IsKeyDown(Keys.Z))
                cameraDistance += time * CameraZoomSpeed;

            if (input.KeyboardHandler.IsKeyDown(Keys.X))
                cameraDistance -= time * CameraZoomSpeed;

            //cameraDistance += currentGamePadState.Triggers.Left * time
            //    * CameraZoomSpeed;
            //cameraDistance -= currentGamePadState.Triggers.Right * time
            //    * CameraZoomSpeed;

            // clamp the camera distance so it doesn't get too close or too far away.
            cameraDistance = MathHelper.Clamp(cameraDistance,
                CameraMinDistance, CameraMaxDistance);

            Matrix unrotatedView = Matrix.CreateLookAt(
                new Vector3(0, 0, -cameraDistance), Vector3.Zero, Vector3.Up);

            view = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          unrotatedView;
        }
    }
}
