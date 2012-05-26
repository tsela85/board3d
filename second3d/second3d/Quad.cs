using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Quad
{
    public struct Quad
{
    public Vector3 Origin;
    public Vector3 UpperLeft;
    public Vector3 LowerLeft;
    public Vector3 UpperRight;
    public Vector3 LowerRight;
    public Vector3 Normal;
    public Vector3 Up;
    public Vector3 Left;

    public VertexPositionNormalTexture[] Vertices;
    public int[] Indices;

    public Quad( Vector3 origin, Vector3 normal, Vector3 up, float width, float height )
    {
        Vertices = new VertexPositionNormalTexture[4];
        Indices = new int[6];
        Origin = origin;
        Normal = normal;
        Up = up;

        // Calculate the quad corners
        Left = Vector3.Cross( normal, Up );
        Vector3 uppercenter = (Up * height / 2) + origin;
        UpperLeft = uppercenter + (Left * width / 2);
        UpperRight = uppercenter - (Left * width / 2);
        LowerLeft = UpperLeft - (Up * height);
        LowerRight = UpperRight - (Up * height);

        FillVertices();
    }
    
    private void FillVertices()
    {
        // Fill in texture coordinates to display full texture
        // on quad
        Vector2 textureUpperLeft = new Vector2( 0.0f, 0.0f );
        Vector2 textureUpperRight = new Vector2( 1.0f, 0.0f );
        Vector2 textureLowerLeft = new Vector2( 0.0f, 1.0f );
        Vector2 textureLowerRight = new Vector2( 1.0f, 1.0f );

        // Provide a normal for each vertex
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].Normal = Normal;
        }

        // Set the position and texture coordinate for each
        // vertex
        Vertices[0].Position = LowerLeft;
        Vertices[0].TextureCoordinate = textureLowerLeft;
        Vertices[1].Position = UpperLeft;
        Vertices[1].TextureCoordinate = textureUpperLeft;
        Vertices[2].Position = LowerRight;
        Vertices[2].TextureCoordinate = textureLowerRight;
        Vertices[3].Position = UpperRight;
        Vertices[3].TextureCoordinate = textureUpperRight;

        // Set the index buffer for each vertex, using
        // clockwise winding
        Indices[0] = 0;
        Indices[1] = 1;
        Indices[2] = 2;
        Indices[3] = 2;
        Indices[4] = 1;
        Indices[5] = 3;
    }
}
 
    Quad quad;
    VertexDeclaration quadVertexDecl;
    Matrix View, Projection;        
   public void Initialize()
    {
        // TODO: Add your initialization logic here
        quad = new Quad( Vector3.Zero, Vector3.Backward, Vector3.Up, 1, 1 );
        View = Matrix.CreateLookAt( new Vector3( 0, 0, 2 ), Vector3.Zero, Vector3.Up );
        Projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.PiOver4, 4.0f / 3.0f, 1, 500 );
    }

    Texture2D texture;
    BasicEffect quadEffect;
    public LoadGraphicsContent( bool loadAllContent )
    {
        if (loadAllContent)
        {
            // TODO: Load any ResourceManagementMode.Automatic content
            texture = content.Load<Texture2D>( "Glass" );
            quadEffect = new BasicEffect( graphics.GraphicsDevice, null );
            quadEffect.EnableDefaultLighting();

            quadEffect.World = Matrix.Identity;
            quadEffect.View = View;
            quadEffect.Projection = Projection;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = texture;
        }
        // TODO: Load any ResourceManagementMode.Manual content
        quadVertexDecl = new VertexDeclaration(graphics.GraphicsDevice, 
            VertexPositionNormalTexture.VertexElements );
    }

    protected override void Draw( GameTime gameTime )
    {
        graphics.GraphicsDevice.Clear( Color.CornflowerBlue );


        // TODO: Add your drawing code here
        DrawQuad();

        base.Draw( gameTime );
    }
    private void DrawQuad()
    {
        graphics.GraphicsDevice.VertexDeclaration = quadVertexDecl;
        quadEffect.Begin();
        foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
        {
            pass.Begin();

            graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                PrimitiveType.TriangleList, quad.Vertices, 0, 4, quad.Indices, 0, 2 );

            pass.End();
        }
        quadEffect.End();
    }
