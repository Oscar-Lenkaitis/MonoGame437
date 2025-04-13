using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Entities;
using BEPUphysics;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Vector3 = BEPUutilities.Vector3;
using Matrix = BEPUutilities.Matrix;
using System.Collections.Generic;
using System;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Materials;
using MathHelper = BEPUutilities.MathHelper;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Xna.Framework.Audio;
using BEPUphysics.Constraints.TwoEntity.Joints;
using System.Diagnostics;


namespace BepuDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        /// <summary>
        /// World in which the simulation runs.
        /// </summary>
        Space space;
        /// <summary>
        /// Controls the viewpoint and how the user can see the world.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Graphical model to use for the boxes in the scene.
        /// </summary>
        public Model BallModel;
        /// <summary>
        /// Graphical model to use for the environment.
        /// </summary>
        public Model PlaygroundModel;

        /// <summary>
        /// Contains the latest snapshot of the keyboard's input state.
        /// </summary>
        public KeyboardState KeyboardState;
        /// <summary>
        /// Contains the latest snapshot of the mouse's input state.
        /// </summary>
        public MouseState MouseState;

        private Sphere GolfBall;

        public Score Score;

        //track if space was pressed so i can't hold it down
        private bool spacePressedLastFrame = false;
        private bool GPressedLastFrame = false;
        private bool RPressedLastFrame = false;
        private bool startInLookat = true;

        //power for hitting the ball
        public float Power = 0f;
        private float MaxPower = 55f;
        bool charging = false;
        public float chargeRate = 20;

        //blank Texture for GUI
        public Texture2D blankTexture;
        public Vector3 cameraForward;

        public SoundEffect bounce;
        public SoundEffect hit;
        public SoundEffect inTheHole;

        public Box golfhole1;
        public Vector3 golfhole1Pos = new Vector3(-9.9f, -1.7f, 64f);
        public Box golfhole2;
        public Vector3 golfhole2Pos = new Vector3(-75.5f, -1.7f, 172f);

        public int hole = 1;

        public Vector3 tee1 = new Vector3(-10, 3, -1);
        public Vector3 tee2 = new Vector3(-309, 43, 171.3f);

        public ScoreManager scoreManager;
        public List<ScoreEntry> topScores = new List<ScoreEntry>();




        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Setup the camera.
            Camera = new Camera(this, new Vector3(-5, 3, -1), 5);
            Score = new Score();

            graphics.ApplyChanges();
            cameraForward = new Vector3();
            scoreManager = new ScoreManager();
            scoreManager.LoadScores();


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {   //texture for GUI stuff
            blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            blankTexture.SetData(new[] { Color.White });

            //sound effects
            hit = Content.Load<SoundEffect>("golfhit");
            inTheHole = Content.Load<SoundEffect>("inthehole");
            bounce = Content.Load<SoundEffect>("ballbounce");

            //This 1x1x1 cube model will represent the box entities in the space.
            BallModel = Content.Load<Model>("ball2");


            PlaygroundModel = Content.Load<Model>("Course1");

            //Construct a new space for the physics simulation to occur within.
            space = new Space();

            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            Vector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(PlaygroundModel, out vertices, out indices);
            //Give the mesh information to a new StaticMesh.  
            //Give it a transformation which scoots it down below the kinematic box entity we created earlier.
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -10, 0)));
            var material = new Material(kineticFriction: 2f, staticFriction: 2f, bounciness: 0f);
            mesh.Material = material;

            //Add it to the space!
            space.Add(mesh);

            //Make it visible too.
            Components.Add(new StaticModel(PlaygroundModel, mesh.WorldTransform.Matrix, this));


            GolfBall = new Sphere(tee1, .5f, 1);
            space.Add(GolfBall);

            EntityModel GolfBallModel = new EntityModel(GolfBall, BallModel, Matrix.Identity, this);
            Components.Add(GolfBallModel);
            GolfBall.Tag = GolfBallModel;
            GolfBall.CollisionInformation.Events.InitialCollisionDetected += HandleGolfBallCollision;
            GolfBall.Material = material;
            GolfBall.LinearDamping = .2f;

            Score.spriteBatch = new SpriteBatch(GraphicsDevice);
            Score.font = Content.Load<SpriteFont>("ScoreFont");
            Viewport viewport = GraphicsDevice.Viewport;
            Score.fontPos = new Vector2(viewport.Width / 2, viewport.Height / 6);
            Score.powerPos = new Vector2(viewport.Width / 10, viewport.Height * .9f);

            golfhole1 = new Box(golfhole1Pos,2,1,2,0);
            CollisionRules.AddRule(golfhole1, GolfBall, CollisionRule.NoSolver);

            golfhole2 = new Box(golfhole2Pos, 3, 1, 3, 0);
            CollisionRules.AddRule(golfhole2, GolfBall, CollisionRule.NoSolver);

            space.Add(golfhole1);
            space.Add(golfhole2);
            //EntityModel hitbox = new EntityModel(golfhole1, BallModel, Matrix.Identity, this);
            //Components.Add(hitbox);



        }

        void HandleGolfBallCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if (other is StaticMesh)
            {
                System.Diagnostics.Debug.WriteLine("GolfBall hit the mesh!");
                bounce.Play();
            }
            else if (other is not StaticMesh && hole == 1)
            {
                inTheHole.Play();
                System.Diagnostics.Debug.WriteLine("scored!");
                Camera.Position = new Vector3(-328, 50, 171.3f);
                cameraForward = new Vector3(-1, 0, 0);
                GolfBall.Position = tee2;
                GolfBall.LinearVelocity = Vector3.Zero;
                Camera.follow = true;
                Score.currentShots = 0;
                hole = 2;

            }
            else if (other is not StaticMesh && hole == 2)
            {
                inTheHole.Play();
                var score = new ScoreEntry { Initials = "ME", Score = Score.totalShots };
                scoreManager.AddScore(score);
                topScores = scoreManager.GetTopScores(3);
                Score.reset();
                Camera.Position = new Vector3(-5, 3, -1);
                cameraForward = new Vector3(1, 0, 0);
                GolfBall.Position = tee1;
                GolfBall.LinearVelocity = Vector3.Zero;
                Camera.follow = true;
                hole = 1;
                
            }
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            // Allows the game to exit
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }
            //Update the camera.
            if (startInLookat)
            {
                Camera.Yaw = MathHelper.PiOver2;
                startInLookat = false;

            }
            Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            cameraForward = Camera.WorldMatrix.Forward;

            // Ignore the Z component to restrict movement to the X and Y plane
            cameraForward.Y = 0;

            // Normalize the direction vector to ensure consistent force magnitude
            cameraForward = Vector3.Normalize(cameraForward);
           
            if (KeyboardState.IsKeyDown(Keys.Space) )
            {
                charging = true;
                Power += chargeRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Power = MathHelper.Clamp(Power, 0, MaxPower);
                Score.powerPercent = (int)(MathHelper.Clamp(Power / MaxPower, 0, 1) * 100);
                Score.powerUpdate();

                //GolfBall.LinearVelocity = Camera.WorldMatrix.Forward * 50;
                //Score.hit();
            }
            else if(spacePressedLastFrame && charging)
            {
                GolfBall.LinearVelocity = Camera.WorldMatrix.Forward * Power;
                Score.hit();
                hit.Play();

                Power = 0f;
                charging = false;
            }
            if (Camera.follow)
            {
                Camera.Position = GolfBall.Position - cameraForward * 5 + new Vector3(0,1f,0);
                Camera.CreateLookAt(Camera, GolfBall.Position, Vector3.Up, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
           


            spacePressedLastFrame = KeyboardState.IsKeyDown(Keys.Space);

            if (KeyboardState.IsKeyDown(Keys.R) && !RPressedLastFrame && hole == 1)
            {
                GolfBall.Position = tee1;
                GolfBall.LinearMomentum = Vector3.Zero;
                GolfBall.LinearVelocity = Vector3.Zero;
                Camera.Position = new Vector3(-5, 3, -1);
                cameraForward = new Vector3(1, 0, 0);
                Camera.follow = true;
            }
            else if (KeyboardState.IsKeyDown(Keys.R) && !RPressedLastFrame && hole == 2)
            {
                GolfBall.Position = tee2;
                GolfBall.LinearMomentum = Vector3.Zero;
                GolfBall.LinearVelocity = Vector3.Zero;
                Camera.Position = new Vector3(-328, 50, 171.3f);
                cameraForward = new Vector3(-1, 0, 0);
                Camera.follow = true;
            }

            RPressedLastFrame = KeyboardState.IsKeyDown(Keys.R);


            if (KeyboardState.IsKeyDown(Keys.G) && !GPressedLastFrame)
            {
                Camera.follow = !Camera.follow;
            }

            GPressedLastFrame = KeyboardState.IsKeyDown(Keys.G);


            //Steps the simulation forward one time step.
            space.Update();

            base.Update(gameTime);
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue); 

            base.Draw(gameTime);

            Score.spriteBatch.Begin(SpriteSortMode.Deferred,
               BlendState.AlphaBlend,
               SamplerState.PointClamp,
               DepthStencilState.Default,
               RasterizerState.CullNone);
            Vector2 FontOrigin = Score.font.MeasureString(Score.output) / 2;
            Score.spriteBatch.DrawString(Score.font, Score.output, Score.fontPos, Color.Black, 0, FontOrigin, 1.0f, SpriteEffects.None, 0f);
            Score.spriteBatch.DrawString(Score.font, Score.powerOutput, Score.powerPos, Color.Black);

            topScores = scoreManager.GetTopScores(3);
            
            for (int i = 0; i < topScores.Count; i++)
            {
                var score = topScores[i];
                Score.spriteBatch.DrawString(Score.font, $"{i + 1}. {score.Initials}: {score.Score}", new Vector2(10, 10 + i * 20), Color.Black);
            }


            Score.spriteBatch.End();
        }

    }
}
