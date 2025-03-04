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
        public Model CubeModel;
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
            Camera = new Camera(this, new Vector3(0, 3, 10), 5);
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //This 1x1x1 cube model will represent the box entities in the space.
            CubeModel = Content.Load<Model>("ball");


            PlaygroundModel = Content.Load<Model>("basicHole2");

            //Construct a new space for the physics simulation to occur within.
            space = new Space();

            //Set the gravity of the simulation by accessing the simulation settings of the space.
            //It defaults to (0,0,0); this changes it to an 'earth like' gravity.
            //Try looking around in the space's simulationSettings to familiarize yourself with the various options.
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            //Make a box representing the ground and add it to the space.
            //The Box is an "Entity," the main simulation object type of BEPUphysics.
            //Examples of other entities include cones, spheres, cylinders, and a bunch more (a full listing is in the BEPUphysics.Entities namespace).

            //Every entity has a set of constructors.  Some half a parameter for mass, others don't.
            //Constructors that allow the user to specify a mass create 'dynamic' entiites which fall, bounce around, and generally work like expected.
            //Constructors that have no mass parameter create a create 'kinematic' entities.  These can be thought of as having infinite mass.
            //This box being added is representing the ground, so the width and length are extended and it is kinematic.
            //Box ground = new Box(Vector3.Zero, 30, 1, 30);
            //space.Add(ground);


            //Now that we have something to fall on, make a few more boxes.
            //These need to be dynamic, so give them a mass- in this case, 1 will be fine.
            //space.Add(new Box(new Vector3(0, 4, 0), 1, 1, 1, 1));
            //space.Add(new Box(new Vector3(0, 8, 0), 1, 1, 1, 1));
            //space.Add(new Box(new Vector3(0, 12, 0), 1, 1, 1, 1));

            //Create a physical environment from a triangle mesh.
            //First, collect the the mesh data from the model using a helper function.
            //This special kind of vertex inherits from the TriangleMeshVertex and optionally includes
            //friction/bounciness data.
            //The StaticTriangleGroup requires that this special vertex type is used in lieu of a normal TriangleMeshVertex array.
            Vector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(PlaygroundModel, out vertices, out indices);
            //Give the mesh information to a new StaticMesh.  
            //Give it a transformation which scoots it down below the kinematic box entity we created earlier.
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -10, 0)));

            //Add it to the space!
            space.Add(mesh);

            //Make it visible too.
            Components.Add(new StaticModel(PlaygroundModel, mesh.WorldTransform.Matrix, this));

            //Hook an event handler to an entity to handle some game logic.
            //Refer to the Entity Events documentation for more information.
            //Box deleterBox = new Box(new Vector3(5, 2, 0), 3, 3, 3);
            //space.Add(deleterBox);
            //deleterBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;


            //Go through the list of entities in the space and create a graphical representation for them.
            foreach (Entity e in space.Entities)
            {
                Box box = e as Box;
                if (box != null) //This won't create any graphics for an entity that isn't a box since the model being used is a box.
                {

                    Matrix scaling = Matrix.CreateScale(box.Width, box.Height, box.Length); //Since the cube model is 1x1x1, it needs to be scaled to match the size of each individual box.
                    EntityModel model = new EntityModel(e, CubeModel, scaling, this);
                    //Add the drawable game component for this entity to the game.
                    Components.Add(model);
                    e.Tag = model; //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
                }
            }

        }

        /// <summary>
        /// Used to handle a collision event triggered by an entity specified above.
        /// </summary>
        /// <param name="sender">Entity that had an event hooked.</param>
        /// <param name="other">Entity causing the event to be triggered.</param>
        /// <param name="pair">Collision pair between the two objects in the event.</param>
        void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            //This type of event can occur when an entity hits any other object which can be collided with.
            //They aren't always entities; for example, hitting a StaticMesh would trigger this.
            //Entities use EntityCollidables as collision proxies; see if the thing we hit is one.
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                //We hit an entity! remove it.
                space.Remove(otherEntityInformation.Entity);
                //Remove the graphics too.
                Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
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
            Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (MouseState.LeftButton == ButtonState.Pressed)
            {
                //If the user is clicking, start firing some boxes.
                //First, create a new dynamic box at the camera's location.
                
                //Set the velocity of the new box to fly in the direction the camera is pointing.
                //Entities have a whole bunch of properties that can be read from and written to.
                //Try looking around in the entity's available properties to get an idea of what is available.
                

                var (compoundBody, center) = CreateBEPUEntity(CubeModel);
                float size = 1f;
                CompoundBody toAdd = compoundBody;
                toAdd.LinearVelocity = Camera.WorldMatrix.Forward * 10;
                //Add the new box to the simulation.
                space.Add(toAdd);


                //Add a graphical representation of the box to the drawable game components.
                EntityModel model = new EntityModel(toAdd, CubeModel, Matrix.CreateTranslation(center), this);
                Components.Add(model);
                toAdd.Tag = model;  //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
            }

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

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private (CompoundBody, BEPUutilities.Vector3) CreateBEPUEntity(Model model)
        {
            List<CompoundShapeEntry> shapes = new List<CompoundShapeEntry>();

            foreach (ModelMesh mesh in model.Meshes)
            {
                List<BEPUutilities.Vector3> bepuPoints = new List<BEPUutilities.Vector3>();

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // Extract vertex data
                    VertexBuffer vertexBuffer = part.VertexBuffer;
                    int vertexStride = vertexBuffer.VertexDeclaration.VertexStride;
                    int vertexCount = vertexBuffer.VertexCount;
                    float[] vertexData = new float[vertexCount * vertexStride / sizeof(float)];
                    vertexBuffer.GetData(vertexData);

                    // Iterate through vertices and convert them
                    for (int i = 0; i < vertexData.Length; i += vertexStride / sizeof(float))
                    {
                        // Extract the position from vertex data
                        Microsoft.Xna.Framework.Vector3 vertexPosition = new Microsoft.Xna.Framework.Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);

                        // Transform the vertex position by the model's parent bone transformation
                        vertexPosition = Microsoft.Xna.Framework.Vector3.Transform(vertexPosition, mesh.ParentBone.ModelTransform);

                        // Convert to BEPUutilities.Vector3 and add it to the list
                        bepuPoints.Add(new BEPUutilities.Vector3(vertexPosition.X, vertexPosition.Y, vertexPosition.Z));
                    }
                }

                // Create a ConvexHullShape from the transformed vertices
                ConvexHullShape convexHullShape = new ConvexHullShape(bepuPoints);

                // Create a CompoundShapeEntry, considering the model's parent bone translation
                CompoundShapeEntry cshape = new CompoundShapeEntry(
                    convexHullShape,
                    new BEPUutilities.Vector3(mesh.ParentBone.ModelTransform.Translation.X, mesh.ParentBone.ModelTransform.Translation.Y, mesh.ParentBone.ModelTransform.Translation.Z)
                );

                shapes.Add(cshape);
            }

            // Create the CompoundShape and extract the center
            BEPUutilities.Vector3 center;
            CompoundShape shape = new CompoundShape(shapes, out center);

            // Create the CompoundBody using the generated shapes
            CompoundBody body = new CompoundBody(shapes);

            // Return the body and the center (converted to BEPUutilities.Vector3)
            return (body, center);
        }



    }
}
