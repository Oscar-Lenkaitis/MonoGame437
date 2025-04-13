
using BEPUutilities;
using Microsoft.Xna.Framework.Input;
using System;

namespace BepuDemo
{
    /// <summary>
    /// Basic camera class supporting mouse/keyboard/gamepad-based movement.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Gets or sets the position of the camera.
        /// </summary>
        public Vector3 Position { get; set; }

        float yaw;
        float pitch;
        /// <summary>
        /// Gets or sets the yaw rotation of the camera.
        /// </summary>
        public float Yaw
        {
            get
            {
                return yaw;
            }
            set
            {
                yaw = MathHelper.WrapAngle(value);
            }
        }
        /// <summary>
        /// Gets or sets the pitch rotation of the camera.
        /// </summary>
        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                pitch = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2);
            }
        }

        /// <summary>
        /// Gets or sets the speed at which the camera moves.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// sets freecam or loocked cam.
        /// </summary>
        public bool follow = false;

        /// <summary>
        /// Gets the view matrix of the camera.
        /// </summary>
        public Matrix ViewMatrix { get; set; }
        /// <summary>
        /// Gets or sets the projection matrix of the camera.
        /// </summary>
        public Matrix ProjectionMatrix { get; set; }

        /// <summary>
        /// Gets the world transformation of the camera.
        /// </summary>
        public Matrix WorldMatrix { get; set; }

        /// <summary>
        /// Gets the game owning the camera.
        /// </summary>
        public Game1 Game { get; private set; }

        /// <summary>
        /// Constructs a new camera.
        /// </summary>
        /// <param name="game">Game that this camera belongs to.</param>
        /// <param name="position">Initial position of the camera.</param>
        /// <param name="speed">Initial movement speed of the camera.</param>
        public Camera(Game1 game, Vector3 position, float speed)
        {
            Game = game;
            Position = position;
            Speed = speed;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfViewRH(MathHelper.PiOver4, 4f / 3f, .1f, 10000.0f);
            Mouse.SetPosition(200, 200);
        }

        /// <summary>
        /// Moves the camera forward using its speed.
        /// </summary>
        /// <param name="dt">Timestep duration.</param>
        public void MoveForward(float dt)
        {
            Position += WorldMatrix.Forward * (dt * Speed);
        }
        /// <summary>
        /// Moves the camera right using its speed.
        /// </summary>
        /// <param name="dt">Timestep duration.</param>
        /// 
        public void MoveRight(float dt)
        {
            Position += WorldMatrix.Right * (dt * Speed);
        }
        /// <summary>
        /// Moves the camera up using its speed.
        /// </summary>
        /// <param name="dt">Timestep duration.</param>
        /// 
        public void MoveUp(float dt)
        {
            Position += new Vector3(0, (dt * Speed), 0);
        }

        /// <summary>
        /// Updates the camera's view matrix.
        /// </summary>
        /// <param name="dt">Timestep duration.</param>
        public void Update(float dt)
        {
            //Turn based on mouse input.
            if (!follow)
            {
                Yaw += (200 - Game.MouseState.X) * dt * .12f;
                Pitch += (200 - Game.MouseState.Y) * dt * .12f;
                Mouse.SetPosition(200, 200);

                float distance = Speed * dt;

                //Scoot the camera around depending on what keys are pressed.
                if (Game.KeyboardState.IsKeyDown(Keys.E))
                    MoveForward(distance);
                if (Game.KeyboardState.IsKeyDown(Keys.D))
                    MoveForward(-distance);
                if (Game.KeyboardState.IsKeyDown(Keys.S))
                    MoveRight(-distance);
                if (Game.KeyboardState.IsKeyDown(Keys.F))
                    MoveRight(distance);
                if (Game.KeyboardState.IsKeyDown(Keys.A))
                    MoveUp(distance);
                if (Game.KeyboardState.IsKeyDown(Keys.Z))
                    MoveUp(-distance);

            }

            WorldMatrix = Matrix.CreateFromAxisAngle(Vector3.Right, Pitch) * Matrix.CreateFromAxisAngle(Vector3.Up, Yaw);


            

            WorldMatrix = WorldMatrix * Matrix.CreateTranslation(Position);
            ViewMatrix = Matrix.Invert(WorldMatrix);
        }

        public  void CreateLookAt(Camera camera, Vector3 targetPosition, Vector3 up, float dt)
        {
            //float heightAboveBall = 3f;

            Vector3 cameraForward = Vector3.Normalize(targetPosition - camera.Position);

            // Calculate the right vector (cross product of the up vector and the forward vector)
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, cameraForward));

            // Calculate the corrected up vector (cross product of forward and right vectors)
            Vector3 correctedUp = Vector3.Cross(cameraForward, right);

            camera.Yaw += (200 - Game.MouseState.X) * dt * .12f;
            camera.Pitch = (0);
            Mouse.SetPosition(200, 200);

            // Construct the 4x4 matrix
            camera.WorldMatrix =  new Matrix(
                right.X, correctedUp.X, -cameraForward.X, 0,
                right.Y, correctedUp.Y, -cameraForward.Y, 0,
                right.Z, correctedUp.Z, -cameraForward.Z, 0,
                -Vector3.Dot(right, camera.Position),
                -Vector3.Dot(correctedUp, camera.Position),
                Vector3.Dot(cameraForward, camera.Position),
                1
            );

            
        }
    }
}

