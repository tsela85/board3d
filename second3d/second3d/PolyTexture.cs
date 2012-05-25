using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace second3d
{
    class PolyTexture
    {
        private VertexPositionNormalTexture[] vertices;
        private short[] indices;
        int vertNum;      
        private Texture2D texture;
        private Vector2 pointOnEdge;
        VertexPositionTexture[] ver;

#region properties

        public VertexPositionNormalTexture[] Vertices
        {
            get{ return vertices;}
            set {vertices = value;}
        }

        public short[] Indices
        {
            get{ return indices;}
            set {indices = value;}
        }
 
#endregion
        
        public void Initialize(Texture2D tex, int vNum, Vector3[] points,Vector2[] texCords)
        {
            pointOnEdge = Vector2.Zero;
            int iCount = (vNum - 3) * 3 + 3;
            vertNum = vNum;
            texture = tex;
            vertices = new VertexPositionNormalTexture[vNum];

            indices = new short[iCount];
            for (int i = 0; i < vNum; i++)
            {
                vertices[i].Position = points[i];
                vertices[i].TextureCoordinate = texCords[i];
                vertices[i].Normal = Vector3.Up;
            }
            //create the indices. every 2 consecutive points makes an outer edge (x,y,z) -> xy , yz (not xz)
            short j = 0;
            for (short i = 0; i < iCount; i+=3)
            {
                indices[i] = j;
                indices[i+1] = (short)(j + 1);
                indices[i + 2] = (short)((j + 2) % vNum);
                j += 2;
            }
            ver = new VertexPositionTexture[3];
        }

        public void Draw(ref GraphicsDevice device)
        {
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0,
                vertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
            
            
            device.DrawUserPrimitives(PrimitiveType.TriangleList, ver, 0, 1, VertexPositionNormalTexture.VertexDeclaration);
        }

 #region Collision
        public bool collideWithEdge(ref GraphicsDevice device,Matrix project,Matrix view, Matrix world, Vector2 point)
        {
            Vector3 one, two;
            one = device.Viewport.Project(vertices[0].Position, project, view, world);            
            for (int i = 0; i < vertNum - 1; i++)
            {
                two = device.Viewport.Project(vertices[i+1].Position, project, view, world);
                if (LineIntersectPoint(one, two, point, 5))
                {
                    one = device.Viewport.Unproject(new Vector3(pointOnEdge,0f), project, view, world);   
                    ver[0].Position = new Vector3(pointOnEdge.X - 0.5f, pointOnEdge.Y - 0.5f, 0f);
                    ver[0].TextureCoordinate = Vector2.Zero;
                    ver[1].Position = new Vector3(pointOnEdge.X, pointOnEdge.Y + 0.5f, 0f);
                    ver[1].TextureCoordinate = Vector2.Zero;
                    ver[2].Position = new Vector3(pointOnEdge.X + 0.5f, pointOnEdge.Y - 0.5f, 0f);
                    ver[2].TextureCoordinate = Vector2.Zero;
                    return true;
                }
                one = two;
            }
            return false;
        }


        private Vector2 DistanceLineSegmentToPoint(Vector2 A, Vector2 B, Vector2 p, float dist)
        {
            //get the normalized line segment vector
            Vector2 v = B - A;
            v.Normalize();

            //determine the point on the line segment nearest to the point p
            float distanceAlongLine = Vector2.Dot(p, v) - Vector2.Dot(A, v);
            Vector2 nearestPoint;
            if (distanceAlongLine < 0)
            {
                //closest point is A
                nearestPoint = A;
            }
            else if (distanceAlongLine > Vector2.Distance(A, B))
            {
                //closest point is B
                nearestPoint = B;
            }
            else
            {
                //closest point is between A and B... A + d  * ( ||B-A|| )
                nearestPoint = A + distanceAlongLine * v;
            }

            //Calculate the distance between the two points
            float actualDistance = Vector2.Distance(nearestPoint, p);
            if (actualDistance < dist)
                return nearestPoint;
            return Vector2.Zero;
        }

        private bool LineIntersectPoint(Vector3 v1, Vector3 v2, Vector2 p, float dist)
        {
            Vector2 A = new Vector2(v1.X, v1.Y);
            Vector2 B = new Vector2(v2.X, v2.Y);
            Vector2 nearestPoint = DistanceLineSegmentToPoint(A, B, p,dist);
            if (nearestPoint != Vector2.Zero)
            {
                pointOnEdge = nearestPoint;
                return true;
            }
            else
                return false;
        }
#endregion

    }
}
